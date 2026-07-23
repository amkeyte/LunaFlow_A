#!/usr/bin/env python3
"""
.trx Triage Engine for LunaFlow test suite.

Parses a .trx file, classifies failures by type (timeout, exception, race, etc.),
enriches with tier/namespace metadata, detects cascading failures, and generates
a ranked triage report.

Caches triage results by .trx file hash. If the same .trx is triaged twice, the
second run short-circuits and shows the cached result.

Usage:
    python triage.py <path_to_latest.trx> [--output json|markdown] [--force]

Options:
    --output json|markdown    Output format (default: markdown)
    --force                   Re-triage even if already cached
"""

import sys
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from dataclasses import dataclass, asdict
from typing import List, Dict, Optional, Tuple
from datetime import datetime
import re
import hashlib


@dataclass
class TriageFailure:
    """A single test failure with diagnostic classification."""
    test_name: str
    test_class: str
    outcome: str
    message: str
    stack_trace: str
    duration_seconds: float
    failure_type: str  # timeout, exception, assertion, race, etc.
    probable_cause: str
    next_step: str
    tier: Optional[int]
    namespace_tags: List[str]
    phase_tags: List[str]
    is_cascade: bool = False
    cascade_reason: str = ""
    priority: int = 999  # lower = higher priority


class TriageEngine:
    """Main triage orchestrator."""

    def __init__(self, config_dir: Path):
        self.config_dir = config_dir
        self.tiers = self._load_json("tiers.json")["tiers"]
        self.diagnostics = self._load_json("diagnostics.json")["failure_patterns"]
        self.cascade_rules = self._load_json("diagnostics.json")["cascade_rules"]
        self.namespaces = self._load_json("namespaces.json")["namespaces"]
        self.phases = self._load_json("namespaces.json")["phases"]
        self.state_file = config_dir.parent / ".triaged-state.json"
        self._build_tier_map()

    def _load_json(self, filename: str) -> dict:
        path = self.config_dir / filename
        with open(path) as f:
            return json.load(f)

    def _build_tier_map(self):
        """Build quick-lookup map of test class -> tier."""
        self.class_to_tier = {}
        for tier_info in self.tiers:
            for test_class in tier_info["test_classes"]:
                self.class_to_tier[test_class] = tier_info["tier"]

    def _compute_file_hash(self, trx_path: Path) -> str:
        """Compute SHA256 hash of the .trx file."""
        sha256_hash = hashlib.sha256()
        with open(trx_path, "rb") as f:
            for byte_block in iter(lambda: f.read(4096), b""):
                sha256_hash.update(byte_block)
        return sha256_hash.hexdigest()

    def _load_state(self) -> Dict:
        """Load triaged state (which .trx files have been processed)."""
        if self.state_file.exists():
            with open(self.state_file) as f:
                return json.load(f)
        return {}

    def _save_state(self, state: Dict):
        """Save triaged state."""
        with open(self.state_file, "w") as f:
            json.dump(state, f, indent=2)

    def check_if_already_triaged(self, trx_path: Path) -> Optional[Dict]:
        """
        Check if this .trx file has already been triaged.
        Returns cached result if yes, None if not.
        """
        file_hash = self._compute_file_hash(trx_path)
        state = self._load_state()

        if trx_path.name in state:
            cached = state[trx_path.name]
            if cached.get("hash") == file_hash:
                return cached
        return None

    def mark_as_triaged(self, trx_path: Path, report: str, report_format: str):
        """Mark a .trx file as triaged and cache the report."""
        file_hash = self._compute_file_hash(trx_path)
        state = self._load_state()

        state[trx_path.name] = {
            "hash": file_hash,
            "triaged_at": datetime.now().isoformat(),
            "format": report_format,
            "report_lines": report.split("\n")[:10],  # Cache first 10 lines as preview
        }
        self._save_state(state)

    def parse_trx(self, trx_path: Path) -> List[TriageFailure]:
        """Parse .trx XML and extract failures."""
        tree = ET.parse(trx_path)
        root = tree.getroot()

        # MSTest .trx namespace
        ns = {"": "http://microsoft.com/schemas/VisualStudio/TestResults/2010"}

        failures = []
        for result in root.findall(".//TestResult", ns):
            outcome = result.get("outcome", "Unknown")
            if outcome != "Failed":
                continue

            test_name = result.get("testName", "Unknown")
            test_class = self._extract_class_name(test_name)
            duration = float(result.get("duration", "0").replace("PT", "").replace("S", "") or 0)

            message = ""
            stack_trace = ""
            for output in result.findall("./Output", ns):
                msg_elem = output.find("Message", ns)
                if msg_elem is not None:
                    message = msg_elem.text or ""
                trace_elem = output.find("StackTrace", ns)
                if trace_elem is not None:
                    stack_trace = trace_elem.text or ""

            failure = TriageFailure(
                test_name=test_name,
                test_class=test_class,
                outcome=outcome,
                message=message,
                stack_trace=stack_trace,
                duration_seconds=self._parse_duration(duration),
                failure_type="unknown",
                probable_cause="",
                next_step="",
                tier=self.class_to_tier.get(test_class),
                namespace_tags=[],
                phase_tags=[],
            )
            failures.append(failure)

        return failures

    def _extract_class_name(self, fully_qualified_name: str) -> str:
        """Extract the test class name from a fully qualified test name."""
        # Format: namespace.ClassName.MethodName
        parts = fully_qualified_name.split(".")
        if len(parts) >= 2:
            return parts[-2]
        return fully_qualified_name

    def _parse_duration(self, duration_str) -> float:
        """Parse ISO 8601 duration or float seconds."""
        try:
            return float(duration_str)
        except:
            # ISO 8601 format: PT1.234S
            match = re.search(r"PT(\d+(?:\.\d+)?)S", str(duration_str))
            if match:
                return float(match.group(1))
            return 0.0

    def classify_failures(self, failures: List[TriageFailure]) -> List[TriageFailure]:
        """Classify each failure by type using diagnostic patterns."""
        full_text_lower = lambda f: (f.message + " " + f.stack_trace).lower()

        for failure in failures:
            text_lower = full_text_lower(failure)

            # Try each diagnostic pattern in order
            best_match = None
            for diag in self.diagnostics:
                keywords = diag.get("detection", {}).get("keywords", [])
                duration_threshold = diag.get("detection", {}).get("duration_threshold_seconds")

                # Check keyword match
                keyword_match = any(kw.lower() in text_lower for kw in keywords)

                # Check duration threshold
                duration_match = (
                    duration_threshold is None
                    or failure.duration_seconds >= duration_threshold
                )

                if keyword_match or duration_match:
                    best_match = diag
                    break

            if best_match:
                failure.failure_type = best_match["id"]
                failure.probable_cause = best_match["probable_cause"]
                failure.next_step = best_match["next_step"]
            else:
                failure.failure_type = "unknown"
                failure.probable_cause = "Unable to classify"
                failure.next_step = "Review message and stack trace"

        return failures

    def detect_cascades(self, failures: List[TriageFailure]) -> List[TriageFailure]:
        """Mark failures that are cascades from higher-tier breaks."""
        broken_tiers = set(f.tier for f in failures if f.tier is not None)

        for failure in failures:
            if failure.tier is None:
                continue

            # Check if a lower tier is broken
            for rule in self.cascade_rules:
                if rule["broken_tier"] in broken_tiers and failure.tier in rule["cascade_to"]:
                    failure.is_cascade = True
                    failure.cascade_reason = rule["reason"]
                    break

        return failures

    def rank_failures(self, failures: List[TriageFailure]) -> List[TriageFailure]:
        """Rank failures by priority (lower number = higher priority)."""
        def priority_score(f: TriageFailure) -> int:
            score = 0

            # Tier is primary: Tier 1 first
            if f.tier is not None:
                score += f.tier * 100  # Tier 1: +100, Tier 2: +200, etc.

            # Cascades are lowest priority
            if f.is_cascade:
                score += 1000

            # Severity of failure type
            severity_map = {
                "timeout": 10,
                "flaky_race": 20,
                "state_sync": 15,
                "queue_corruption": 15,
                "uncaught_exception": 25,
                "assertion": 40,
                "unknown": 50,
            }
            score += severity_map.get(f.failure_type, 50)

            return score

        failures.sort(key=priority_score)
        for i, f in enumerate(failures):
            f.priority = i + 1

        return failures

    def generate_report(self, failures: List[TriageFailure], format: str = "markdown") -> str:
        """Generate a triage report in the specified format."""
        if format == "json":
            return self._report_json(failures)
        else:
            return self._report_markdown(failures)

    def _report_json(self, failures: List[TriageFailure]) -> str:
        """JSON report format."""
        by_tier = self._group_by_tier(failures)
        by_tier_serializable = {
            tier: [asdict(f) for f in tier_failures]
            for tier, tier_failures in by_tier.items()
        }
        report = {
            "generated_at": datetime.now().isoformat(),
            "total_failures": len(failures),
            "by_tier": by_tier_serializable,
            "failures": [asdict(f) for f in failures],
        }
        return json.dumps(report, indent=2)

    def _report_markdown(self, failures: List[TriageFailure]) -> str:
        """Markdown report format for human consumption."""
        lines = []
        lines.append(f"# Test Triage Report")
        lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        lines.append("")

        # Summary by tier
        by_tier = self._group_by_tier(failures)
        lines.append("## Summary by Tier")
        for tier in sorted(by_tier.keys()):
            count = len(by_tier[tier])
            tier_name = next(
                (t["name"] for t in self.tiers if t["tier"] == tier), f"Tier {tier}"
            )
            cascades = sum(1 for f in by_tier[tier] if f.is_cascade)
            cascade_str = f" ({cascades} cascades)" if cascades > 0 else ""
            lines.append(f"- **Tier {tier} ({tier_name}):** {count} failures{cascade_str}")
        lines.append("")

        # Cascade warning
        cascades = [f for f in failures if f.is_cascade]
        if cascades:
            lines.append("## ⚠️  Cascade Warning")
            lines.append(
                f"**{len(cascades)} failures appear to be cascades from higher-tier breaks.**"
            )
            lines.append("Fix the highest-tier failures first; cascades will likely resolve.")
            lines.append("")

        # Ranked failures
        lines.append("## Failures (ranked by priority)")
        lines.append("")
        lines.append(
            "| Priority | Test | Type | Tier | Probable Cause | Next Step | Cascade? |"
        )
        lines.append("|----------|------|------|------|----------------|-----------|----------|")

        for f in failures:
            cascade_marker = "⚠️ CASCADE" if f.is_cascade else ""
            tier_str = f"Tier {f.tier}" if f.tier else "?"
            lines.append(
                f"| {f.priority} | `{f.test_class}.{self._extract_method(f.test_name)}` | "
                f"{f.failure_type} | {tier_str} | {f.probable_cause[:40]} | {f.next_step[:30]} | {cascade_marker} |"
            )

        lines.append("")
        lines.append("## Diagnostic Details")
        lines.append("")
        for f in failures[:5]:  # Show first 5 in detail
            lines.append(f"### {f.priority}. {f.test_class}.{self._extract_method(f.test_name)}")
            lines.append(f"**Type:** {f.failure_type} | **Tier:** {f.tier or '?'}")
            lines.append(f"**Duration:** {f.duration_seconds:.2f}s")
            lines.append(f"**Probable Cause:** {f.probable_cause}")
            lines.append(f"**Next Step:** {f.next_step}")
            if f.message:
                lines.append(f"**Message:** `{f.message[:100]}`")
            if f.is_cascade:
                lines.append(f"⚠️ **This is a cascade:** {f.cascade_reason}")
            lines.append("")

        return "\n".join(lines)

    def _extract_method(self, fully_qualified_name: str) -> str:
        """Extract method name from fully qualified test name."""
        parts = fully_qualified_name.split(".")
        if parts:
            return parts[-1]
        return fully_qualified_name

    def _group_by_tier(self, failures: List[TriageFailure]) -> Dict[int, List[TriageFailure]]:
        """Group failures by tier."""
        by_tier = {}
        for f in failures:
            tier = f.tier or 0
            if tier not in by_tier:
                by_tier[tier] = []
            by_tier[tier].append(f)
        return by_tier


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    trx_path = Path(sys.argv[1])
    if not trx_path.exists():
        print(f"Error: {trx_path} not found")
        sys.exit(1)

    output_format = "markdown"
    if "--output" in sys.argv:
        idx = sys.argv.index("--output")
        if idx + 1 < len(sys.argv):
            output_format = sys.argv[idx + 1]

    force_retriage = "--force" in sys.argv

    config_dir = Path(__file__).parent / "config"
    engine = TriageEngine(config_dir)

    # Check if this .trx has already been triaged (skip if --force)
    cached = engine.check_if_already_triaged(trx_path) if not force_retriage else None
    if cached:
        print(f"ℹ️  Already triaged: {trx_path.name}")
        print(f"   Triaged at: {cached['triaged_at']}")
        print(f"   (Same results as last run; use --force to re-triage)")
        print("")
        print("   Preview:")
        for line in cached["report_lines"]:
            if line:
                print(f"   {line}")
        sys.exit(0)

    failures = engine.parse_trx(trx_path)
    failures = engine.classify_failures(failures)
    failures = engine.detect_cascades(failures)
    failures = engine.rank_failures(failures)

    report = engine.generate_report(failures, format=output_format)
    print(report)

    # Mark as triaged
    engine.mark_as_triaged(trx_path, report, output_format)


if __name__ == "__main__":
    main()

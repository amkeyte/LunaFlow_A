#!/usr/bin/env python3
"""
doc-lint — audit the LunaFlow Documents/ tree for drift against wiki-style.md's conventions.

See doc-lint-SKILL.md in this same folder for full usage, checks, and examples.
Standard library only — no external dependencies.
"""

import argparse
import json
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path

# Directories under Documents/ that are excluded from all scans entirely.
EXCLUDED_DIRS = {"ApiReference"}

# Filenames that count as entry points and are exempt from the "must be linked" orphan check.
ENTRY_POINT_NAMES_PREFIXES = ("index.md", "README")

# Directories exempt from the orphan check entirely: CodePlans are Baraka's working notes for a
# specific build ticket (the ticket links to the CodePlan, not the other way around), not
# wiki-indexed reference docs. Being unlinked is normal for them, not drift.
ORPHAN_EXEMPT_DIRS = {"Roadmap/CodePlans"}

STATUS_LINE_RE = re.compile(r"^\*\*Status:\*\*\s+\S|^Status:\s+\S", re.MULTILINE)
STATUS_DRIFT_SCAN_LINES = 15  # only the header block counts; template/example text deeper in a file doesn't
LINK_RE = re.compile(r"\[([^\]]*)\]\(([^)]+)\)")
DECISION_HEADING_RE = re.compile(r"^##\s+Decision\s*$", re.MULTILINE)
TICKET_STATUS_RE = re.compile(r"^\*\*Status:\*\*\s*(.+)$", re.MULTILINE)


@dataclass
class Finding:
    check: str
    path: str
    line: int | None
    message: str


@dataclass
class Report:
    findings: list[Finding] = field(default_factory=list)

    def add(self, check: str, path: Path, message: str, line: int | None = None) -> None:
        self.findings.append(Finding(check=check, path=str(path), line=line, message=message))

    def for_check(self, check: str) -> list[Finding]:
        return [f for f in self.findings if f.check == check]


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def documents_dir() -> Path:
    return repo_root() / "Documents"


def all_docs(root_docs: Path) -> list[Path]:
    out = []
    for p in root_docs.rglob("*.md"):
        rel_parts = p.relative_to(root_docs).parts
        if rel_parts and rel_parts[0] in EXCLUDED_DIRS:
            continue
        if "Archive" in rel_parts:
            continue
        out.append(p)
    return out


def check_status_drift(docs: list[Path], root_docs: Path, report: Report) -> None:
    exempt_top = {"Roadmap", "Handoffs", "Specs"}
    for p in docs:
        rel_parts = p.relative_to(root_docs).parts
        # Handoffs lives under ClaudeWiki/Processes/Handoffs — check anywhere in the path.
        if any(part in exempt_top for part in rel_parts):
            continue
        text = p.read_text(encoding="utf-8", errors="replace")
        for i, line in enumerate(text.splitlines(), start=1):
            if i > STATUS_DRIFT_SCAN_LINES:
                break  # a real status field lives in the header block, not buried in template/example text
            if STATUS_LINE_RE.match(line):
                report.add("status-drift", p.relative_to(repo_root()),
                           f'Status field found outside Roadmap/Handoffs/Specs: "{line.strip()}"',
                           line=i)


def normalize_link_target(from_file: Path, target: str) -> Path | None:
    if target.startswith(("http://", "https://", "mailto:")):
        return None
    target = target.split("#", 1)[0]
    if not target:
        return None
    if target.startswith("/"):
        return None  # absolute paths not used in this repo's docs; skip
    resolved = (from_file.parent / target).resolve()
    return resolved


def check_orphaned(docs: list[Path], root_docs: Path, report: Report) -> None:
    doc_set = {p.resolve() for p in docs}
    referenced: set[Path] = set()

    for p in docs:
        text = p.read_text(encoding="utf-8", errors="replace")
        for _, target in LINK_RE.findall(text):
            resolved = normalize_link_target(p, target)
            if resolved and resolved in doc_set:
                referenced.add(resolved)

    for p in docs:
        rp = p.resolve()
        if rp in referenced:
            continue
        if p.name == "index.md" or p.name.startswith("README"):
            continue
        rel_posix = p.relative_to(repo_root()).as_posix()
        if any(rel_posix.startswith(f"Documents/{d}/") for d in ORPHAN_EXEMPT_DIRS):
            continue
        report.add("orphaned", p.relative_to(repo_root()), "Not linked from any other doc in Documents/")


def check_decision_section(root_docs: Path, report: Report) -> None:
    handoffs_dir = root_docs / "ClaudeWiki" / "Processes" / "Handoffs"
    if not handoffs_dir.is_dir():
        return
    for p in sorted(handoffs_dir.glob("*.md")):
        if p.name.startswith("index"):  # index.md, index-archive.md, any future index-*.md split
            continue
        text = p.read_text(encoding="utf-8", errors="replace")
        m = TICKET_STATUS_RE.search(text)
        status = m.group(1).strip().lower() if m else ""
        if status.startswith("open"):  # covers "open" and "open (parked — ...)" variants
            continue
        if not DECISION_HEADING_RE.search(text):
            report.add("decision", p.relative_to(repo_root()),
                       f'No "## Decision" heading found (status: "{status or "unknown"}")')


DEPRECATED_MARKER_RE = re.compile(r"\bDEPRECATED\b")  # ALL-CAPS only — the actual banner convention,
# not a lowercase/mixed-case prose mention of the concept (e.g. discussing a deprecated code method)


def check_deprecation(docs: list[Path], report: Report) -> None:
    for p in docs:
        text = p.read_text(encoding="utf-8", errors="replace")
        lines = text.splitlines()
        for i, line in enumerate(lines):
            if DEPRECATED_MARKER_RE.search(line):
                window = "\n".join(lines[i:i + 11])
                if not LINK_RE.search(window):
                    report.add("deprecation", p.relative_to(repo_root()),
                               '"DEPRECATED" marker found but no link to current pattern in next 10 lines',
                               line=i + 1)
                break  # one flag per file is enough; don't re-flag every mention


def check_links(docs: list[Path], root_docs: Path, report: Report) -> None:
    for p in docs:
        text = p.read_text(encoding="utf-8", errors="replace")
        for i, line in enumerate(text.splitlines(), start=1):
            for _, target in LINK_RE.findall(line):
                resolved = normalize_link_target(p, target)
                if resolved is None:
                    continue
                if not resolved.exists():
                    report.add("links", p.relative_to(repo_root()),
                               f'Link target not found: "{target}"', line=i)


CHECKS = {
    "status-drift": check_status_drift,
    "orphaned": check_orphaned,
    "decision": None,  # handled separately, no docs/root_docs signature match
    "deprecation": check_deprecation,
    "links": check_links,
}


def run_checks(which: list[str]) -> Report:
    report = Report()
    root_docs = documents_dir()
    docs = all_docs(root_docs)

    if "status-drift" in which:
        check_status_drift(docs, root_docs, report)
    if "orphaned" in which:
        check_orphaned(docs, root_docs, report)
    if "decision" in which:
        check_decision_section(root_docs, report)
    if "deprecation" in which:
        check_deprecation(docs, report)
    if "links" in which:
        check_links(docs, root_docs, report)

    return report


def print_text_report(report: Report, which: list[str]) -> None:
    labels = {
        "status-drift": "Status drift",
        "orphaned": "Orphaned docs",
        "decision": "Missing Decision section",
        "deprecation": "Incomplete deprecation",
        "links": "Broken links",
    }
    total = 0
    for check in which:
        findings = report.for_check(check)
        total += len(findings)
        if not findings:
            print(f"✓ {labels[check]}: OK (0 issues)")
            continue
        print(f"✗ {labels[check]}: {len(findings)} issue(s) found")
        for f in findings:
            loc = f"{f.path}:{f.line}" if f.line else f.path
            print(f"  {loc}  →  {f.message}")
    print()
    if total == 0:
        print("Summary: All checks passed. Docs are clean.")
    else:
        print(f"Summary: {total} issue(s).")


def main() -> int:
    parser = argparse.ArgumentParser(
        prog="doc-lint.py",
        description="Audit LunaFlow's Documents/ tree for drift against wiki-style.md conventions.",
    )
    parser.add_argument(
        "--check", choices=list(CHECKS.keys()), action="append", dest="checks",
        help="Run only this check (repeatable). Default: run all checks.",
    )
    parser.add_argument(
        "--format", choices=["text", "json"], default="text",
        help="Output format (default: text).",
    )
    args = parser.parse_args()

    which = args.checks or list(CHECKS.keys())
    report = run_checks(which)

    if args.format == "json":
        print(json.dumps([f.__dict__ for f in report.findings], indent=2))
    else:
        print_text_report(report, which)

    return 1 if report.findings else 0


if __name__ == "__main__":
    sys.exit(main())

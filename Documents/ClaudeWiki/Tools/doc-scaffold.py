#!/usr/bin/env python3
"""
doc-scaffold — generate correctly-structured handoff tickets and specs for the LunaFlow wiki.

See doc-scaffold-SKILL.md in this same folder for full usage, parameters, and examples.
Standard library only — no external dependencies.
"""

import argparse
import datetime
import re
import sys
from pathlib import Path


def repo_root() -> Path:
    """This script lives at Documents/ClaudeWiki/Tools/doc-scaffold.py — walk up to repo root."""
    return Path(__file__).resolve().parent.parent.parent.parent


def slugify(title: str) -> str:
    slug = title.strip().lower()
    slug = re.sub(r"[^a-z0-9]+", "-", slug)
    slug = re.sub(r"-+", "-", slug).strip("-")
    return slug


def existing_ticket_ids(handoffs_dir: Path) -> set[str]:
    ids = set()
    for f in handoffs_dir.glob("*.md"):
        if f.name == "index.md":
            continue
        m = re.match(r"^(\d{4})-", f.name)
        if m:
            ids.add(m.group(1))
    return ids


TICKET_TEMPLATE = """# {id} — {title}

**Status:** open
**Opened by:** {opened_by}
**Opened:** {date}
**For:** {for_}

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

<What prompted this — link to the doc/code/conversation it came from.>

### Ask

<The specific thing the assignee needs to do or answer.>

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
"""

SPEC_TEMPLATE = """# {name_title} — Implementation Spec

**Status:** draft
**Companion design doc:** [`../design/{design_doc}`](../design/{design_doc}) — read for rationale.

## Purpose

<What this spec turns into buildable form. One paragraph; link to the design doc for "why," don't
restate it. See `Specs/README.md` for the full preamble template and discipline.>

## Scope

<In / Out, or brief bullets of what's covered.>

## Related pages

- [`../design/{design_doc}`](../design/{design_doc})
- [Specs README](README.md)
"""


def cmd_ticket(args: argparse.Namespace) -> int:
    root = repo_root()
    handoffs_dir = root / "Documents" / "ClaudeWiki" / "Processes" / "Handoffs"
    if not handoffs_dir.is_dir():
        print(f"ERROR: Handoffs directory not found at {handoffs_dir}", file=sys.stderr)
        return 2

    ticket_id = args.id.zfill(4)
    existing = existing_ticket_ids(handoffs_dir)
    if ticket_id in existing:
        print(f"ERROR: Ticket ID {ticket_id} already in use in {handoffs_dir}.", file=sys.stderr)
        print("Pick the next free number (check Handoffs/index.md too — the folder and the index can drift).", file=sys.stderr)
        return 1

    slug = slugify(args.title)
    filename = f"{ticket_id}-{slug}.md"
    out_path = handoffs_dir / filename

    content = TICKET_TEMPLATE.format(
        id=ticket_id,
        title=args.title,
        opened_by=args.opened_by or "TBD",
        date=datetime.date.today().isoformat(),
        for_=args.for_ or "TBD",
    )
    out_path.write_text(content, encoding="utf-8")

    print(f"Created: {out_path.relative_to(root)}")
    print("Next steps:")
    print("  1. Fill in ## Details (Context, Ask).")
    print("  2. Add a row to Handoffs/index.md once there's real content (not auto-generated).")
    print("  3. Leave ## Decision as-is until there's a ruling to summarize.")
    return 0


def cmd_spec(args: argparse.Namespace) -> int:
    root = repo_root()
    design_dir = root / "Documents" / "design"
    specs_dir = root / "Documents" / "Specs"

    design_doc_name = args.design_doc
    if not design_doc_name.endswith(".md"):
        design_doc_name += ".md"
    design_doc_path = design_dir / design_doc_name
    if not design_doc_path.is_file():
        print(f"ERROR: Design doc not found: {design_doc_path}", file=sys.stderr)
        print("A spec needs a real companion design doc — that's the point of this check.", file=sys.stderr)
        return 1

    name_slug = slugify(args.name)
    filename = f"{name_slug}-spec.md"
    out_path = specs_dir / filename
    if out_path.exists():
        print(f"ERROR: Spec already exists: {out_path}", file=sys.stderr)
        return 1

    name_title = args.name.replace("-", " ").replace("_", " ").title()

    content = SPEC_TEMPLATE.format(name_title=name_title, design_doc=design_doc_name)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(content, encoding="utf-8")

    print(f"Created: {out_path.relative_to(root)}")
    print("Next steps:")
    print("  1. Expand ## Purpose and ## Scope.")
    print("  2. Register the spec wherever the companion design doc's 'Related' section lists specs, if applicable.")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(
        prog="doc-scaffold.py",
        description="Generate correctly-structured handoff tickets and specs for the LunaFlow wiki.",
    )
    sub = parser.add_subparsers(dest="command", required=True)

    p_ticket = sub.add_parser("ticket", help="Generate a new handoff ticket")
    p_ticket.add_argument("--id", required=True, help="Ticket ID, e.g. 0033")
    p_ticket.add_argument("--title", required=True, help="Human-readable ticket title")
    p_ticket.add_argument("--opened-by", dest="opened_by", default=None)
    p_ticket.add_argument("--for", dest="for_", default=None)
    p_ticket.set_defaults(func=cmd_ticket)

    p_spec = sub.add_parser("spec", help="Generate a new buildable spec")
    p_spec.add_argument("--name", required=True, help="Spec name, e.g. engine-host")
    p_spec.add_argument("--design-doc", dest="design_doc", required=True,
                         help="Companion design doc filename in Documents/design/, e.g. engine-core.md")
    p_spec.set_defaults(func=cmd_spec)

    args = parser.parse_args()
    return args.func(args)


if __name__ == "__main__":
    sys.exit(main())

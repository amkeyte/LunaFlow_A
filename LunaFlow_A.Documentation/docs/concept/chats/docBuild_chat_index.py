# docBuild_chat_index.py
#
# Scans for .txt files in this directory and outputs index.md with links.

from pathlib import Path

chat_dir = Path(__file__).parent
output_file = chat_dir / "index.md"

lines = [
    "# Chat Transcripts Index",
    "",
    "Available chat notes:",
    ""
]

for txt_file in sorted(chat_dir.glob("*.txt")):
    rel_path = txt_file.name  # Local to this directory
    lines.append(f"- [{rel_path}]({rel_path})")

output_file.write_text("\n".join(lines), encoding="utf-8")

# docBuild_chat_index.py
# Builds chatList.md with URL-encoded links for .txt files

print("Building chatList.md")

from pathlib import Path
from urllib.parse import quote

chat_dir = Path(__file__).parent
output_file = chat_dir / "chatList.md"

lines = [
    "# Chat Transcripts Index",
    "",
    "Available chat notes:",
    ""
]

for txt_file in sorted(chat_dir.glob("*.txt")):
    filename = txt_file.name
    display_name = filename
    encoded_name = quote(filename, safe='')  # This forces encoding spaces and all special chars
    lines.append(f"- [{display_name}]({encoded_name})")

output_file.write_text("\n".join(lines), encoding="utf-8")

print(f"{output_file.name} written with {len(lines) - 4} entries.")

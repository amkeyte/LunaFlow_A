import os
import yaml

GENERIC_INDEX_TEMPLATE = """---
title: {title}
---

# {title}

Welcome to the **{title}** section.
"""

def generate_index_md(folder_path, title):
    index_path = os.path.join(folder_path, "index.md")
    with open(index_path, "w", encoding="utf-8") as f:
        f.write(GENERIC_INDEX_TEMPLATE.format(title=title))
    print(f"Regenerated index.md in {folder_path}")


def generate_toc_yml(folder_path, subfolders, md_files):
    toc = []

    # Add top-level entry
    toc.append({
        "name": os.path.basename(folder_path).title(),
        "href": "index.md",
        "homepage": "index.md"
    })

    # Add .md files
    for md in md_files:
        title = os.path.splitext(md)[0].replace('_', ' ').replace('-', ' ').title()
        toc.append({
            "name": title,
            "href": md
        })

    # Add subfolders with TOCs
    for sub in subfolders:
        sub_index = os.path.join(sub, "index.md")
        if os.path.exists(sub_index):
            href = os.path.relpath(sub_index, folder_path).replace("\\", "/")
            toc.append({
                "name": os.path.basename(sub).title(),
                "href": href,
                "homepage": href
            })

    # Write toc.yml
    toc_path = os.path.join(folder_path, "toc.yml")
    with open(toc_path, "w", encoding="utf-8") as f:
        yaml.dump(toc, f, sort_keys=False)
    print(f"Generated toc.yml in {folder_path}")

def process_folder(folder_path):
    items = os.listdir(folder_path)
    subfolders = []
    md_files = []

    for item in items:
        full_path = os.path.join(folder_path, item)
        if os.path.isdir(full_path):
            subfolders.append(full_path)
        elif item.endswith(".md") and item.lower() != "index.md":
            md_files.append(item)

    # Create index.md if missing
    generate_index_md(folder_path, os.path.basename(folder_path).title())

    # Generate toc.yml
    generate_toc_yml(folder_path, subfolders, md_files)

    # Recurse into subfolders
    for subfolder in subfolders:
        process_folder(subfolder)

if __name__ == "__main__":
    root = os.path.dirname(os.path.abspath(__file__))
    print(f"Starting TOC/index generation in: {root}")
    process_folder(root)
    print("Done.")

import os
import yaml

GENERIC_INDEX_TEMPLATE = """---
title: {title}
---

# {title}

Welcome to the **{title}** section.
"""

import subprocess

def generate_index_md(folder_path, title):
    template_path = os.path.join(folder_path, "index_template.md")
    index_path = os.path.join(folder_path, "index.md")

    # Step 1: Read the template (custom or generic)
    if os.path.exists(template_path):
        with open(template_path, "r", encoding="utf-8") as f:
            result = f.read()
        print(f"Using custom index_template.md in {folder_path}")
    else:
        result = GENERIC_INDEX_TEMPLATE.format(title=title)
        print(f"Using generic template for {folder_path}")

    result = result.replace("{{title}}", title)

    # Step 2: Handle {{embed}} replacements like {{chatList}}
    import re
    embed_tags = re.findall(r"{{\s*([\w\-]+)\s*}}", result)
    for tag in embed_tags:
        script_path = os.path.join(folder_path, f"{tag}.py")
        md_path = os.path.join(folder_path, f"{tag}.md")

        # Run the script if it exists
        if os.path.exists(script_path):
            try:
                print(f"Running script: {script_path}")
                subprocess.run(["python", script_path], check=True)
            except subprocess.CalledProcessError as e:
                print(f"Error running {script_path}: {e}")

        # Replace tag with Markdown content
        if os.path.exists(md_path):
            with open(md_path, "r", encoding="utf-8") as f:
                embedded = f.read()
            result = result.replace(f"{{{{{tag}}}}}", embedded)
            print(f"Embedded content from {tag}.md")
        else:
            print(f"No markdown file found for tag: {tag}")
            result = result.replace(f"{{{{{tag}}}}}", f"*Missing content: {tag}.md*")

    # Step 3: Write index.md
    with open(index_path, "w", encoding="utf-8") as f:
        f.write(result)

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

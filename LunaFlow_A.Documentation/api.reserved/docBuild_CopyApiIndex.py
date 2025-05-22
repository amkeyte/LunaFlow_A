import os
import shutil

# Define paths
script_dir = os.path.dirname(os.path.abspath(__file__))
source_dir = script_dir
target_dir = os.path.abspath(os.path.join(script_dir, '../api'))

print(f"Copying files from {source_dir} to {target_dir}:")

# Ensure the target directory exists
os.makedirs(target_dir, exist_ok=True)

# Files to copy
files_to_copy = ['index.md', 'toc.yml']

for filename in files_to_copy:
    src_file = os.path.join(source_dir, filename)
    dst_file = os.path.join(target_dir, filename)

    if os.path.exists(src_file):
        shutil.copy2(src_file, dst_file)
        print(f"Copied {filename} to /api")
    else:
        print(f"Warning: {filename} not found in /api.reserved")

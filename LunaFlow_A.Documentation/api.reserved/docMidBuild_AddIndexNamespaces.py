import os
import yaml
from collections import defaultdict

API_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "../api"))

def collect_namespaces():
    namespaces = defaultdict(list)
    for filename in os.listdir(API_DIR):
        if filename.endswith(".yml") and not filename.startswith("toc"):
            path = os.path.join(API_DIR, filename)
            with open(path, "r", encoding="utf-8") as f:
                data = yaml.safe_load(f)
            if "items" in data:
                for item in data["items"]:
                    ns = item.get("namespace")
                    if ns:
                        parts = ns.split(".")
                        for i in range(1, len(parts)):
                            parent = ".".join(parts[:i])
                            child = ".".join(parts[:i+1])
                            namespaces[parent].append(child)
    return {k: sorted(set(v)) for k, v in namespaces.items()}

def patch_toc_for_namespace(ns, subnames):
    # hack
    if ns == "LunaFlow":
       return

    ns_file = os.path.join(API_DIR, f"{ns}.yml")
    if not os.path.exists(ns_file):
        print(f"Missing file for {ns}")
        return

    with open(ns_file, "r", encoding="utf-8") as f:
        lines = f.read().splitlines()

    if lines and lines[0].strip() == "### YamlMime:ManagedReference":
        yaml_body = "\n".join(lines[1:])
    else:
        yaml_body = "\n".join(lines)

    toc = yaml.safe_load(yaml_body)

    if not toc.get("items"):
        print(f"Skipping invalid {ns_file}: no items")
        return

    main = toc["items"][0]

    existing_children = set(main.get("children", []))
    references = toc.setdefault("references", [])

    for sub in subnames:
        if sub not in existing_children:
            print(f"Adding subnamespace {sub} to {ns}.yml")
            main.setdefault("children", []).append(sub)

            references.append({
                "uid": sub,
                "commentId": f"N:{sub}",
                "href": f"{sub}.html",
                "name": sub.split(".")[-1],
                "nameWithType": sub.split(".")[-1],
                "fullName": sub
            })

    # Re-emit with correct header
    with open(ns_file, "w", encoding="utf-8") as f:
        f.write("### YamlMime:ManagedReference\n")
        yaml.dump(toc, f, sort_keys=False)

if __name__ == "__main__":
    print("Scanning DocFX API output for subnamespaces...")
    ns_map = collect_namespaces()
    for ns, subs in ns_map.items():
        patch_toc_for_namespace(ns, subs)
    print("Namespace TOCs updated.")

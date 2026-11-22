#!/usr/bin/env python3
"""
Exportador de Fragmentos In-World do Project Daedalus para o Godot 4.
Lê todas as notas em jogos/parallax/fragmentos/{goliath,phantom,cipher}/
e gera um JSON estruturado pronto para ser consumido em res://data/fragments.json.
"""

import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FRAGMENTS_DIR = os.path.join(ROOT, "jogos", "parallax", "fragmentos")


def parse_note(fpath):
    with open(fpath, "r", encoding="utf-8") as f:
        text = f.read()

    meta = {}
    body = text
    if text.startswith("---\n"):
        end_idx = text.find("\n---", 4)
        if end_idx != -1:
            yaml_block = text[4:end_idx]
            body = text[end_idx + 4 :].strip()
            for line in yaml_block.splitlines():
                if ":" in line:
                    k, v = line.split(":", 1)
                    meta[k.strip()] = v.strip()

    # Segurança: nunca exportar notas restritas
    if meta.get("camada") == "restrito":
        return None

    title_match = re.search(r"^#\s+(.+)$", body, flags=re.MULTILINE)
    title = title_match.group(1).strip() if title_match else os.path.basename(fpath)[:-3]

    quotes = re.findall(r"^>\s?(.*)$", body, flags=re.MULTILINE)
    in_world_text = "\n".join(q for q in quotes if q).strip()

    links = re.findall(r"\[\[([^\]|#]+)\]\]", meta.get("liga", ""))

    return {
        "id": os.path.basename(fpath)[:-3],
        "title": title,
        "olhar": meta.get("olhar", "comum"),
        "status": meta.get("status", "cânone"),
        "text": in_world_text,
        "links": links,
    }


def main():
    out_path = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "studio", "scripts", "fragments_export.json")
    fragments = []

    for olhar in ("goliath", "phantom", "cipher"):
        sub = os.path.join(FRAGMENTS_DIR, olhar)
        if not os.path.isdir(sub):
            continue
        for fname in sorted(os.listdir(sub)):
            if fname.endswith(".md"):
                item = parse_note(os.path.join(sub, fname))
                if item:
                    fragments.append(item)

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump({"game": "parallax", "count": len(fragments), "fragments": fragments}, f, ensure_ascii=False, indent=2)

    print(f"Exportados {len(fragments)} fragmentos in-world para {out_path}")


if __name__ == "__main__":
    main()

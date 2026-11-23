#!/usr/bin/env python3
"""
Exportador de Fragmentos In-World do Project Daedalus para a Unity (PARALLAX).

Lê jogos/parallax/fragmentos/{goliath,phantom,cipher}/ e gera, dentro do projeto Unity:
  - Assets/_Project/Data/Lore/Generated/fragments.json (importado como LoreFragment pelo menu
    Parallax/Daedalus/Import Generated JSON);
  - Assets/_Project/Data/Localization/Source/daedalus_strings.csv (Key,pt-BR,en,zh-Hans,es) para o
    pipeline de tradução contextual. Traduções existentes são preservadas enquanto o texto PT-BR não mudar.

Segurança: notas com camada restrito, arquivos dentro de restrito/ e fragmentos que apontam para
verdades (verdade-*) nunca são exportados.
"""

import argparse
import csv
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FRAGMENTS_DIR = os.path.join(ROOT, "jogos", "parallax", "fragmentos")
UNITY_PROJECT = os.path.join(ROOT, "client", "parallax-unity")
VIEWERS = ("goliath", "phantom", "cipher")
LOCALES = ("pt-BR", "en", "zh-Hans", "es")


def parse_frontmatter(text):
    text = text.replace("\r\n", "\n")
    if not text.startswith("---\n"):
        return {}, text
    end = text.find("\n---", 4)
    if end == -1:
        return {}, text
    meta = {}
    for line in text[4:end].splitlines():
        if ":" in line:
            key, value = line.split(":", 1)
            meta[key.strip()] = value.strip()
    return meta, text[end + 4:].strip()


def parse_note(path):
    with open(path, "r", encoding="utf-8") as handle:
        meta, body = parse_frontmatter(handle.read())

    normalized = path.replace(os.sep, "/")
    if meta.get("camada") == "restrito" or "/restrito/" in normalized:
        return None

    links = re.findall(r"\[\[([^\]|#]+)", meta.get("liga", "") + "\n" + body)
    if any(link.strip().startswith("verdade-") for link in links):
        print(f"[BLOQUEADO] {normalized}: fragmento aponta para verdade restrita.", file=sys.stderr)
        return None

    title_match = re.search(r"^#\s+(.+)$", body, flags=re.MULTILINE)
    title = title_match.group(1).strip() if title_match else os.path.basename(path)[:-3]
    quotes = re.findall(r"^>\s?(.*)$", body, flags=re.MULTILINE)
    in_world_text = "\n".join(q for q in quotes if q).strip()

    return {
        "id": os.path.basename(path)[:-3],
        "title": title,
        "olhar": meta.get("olhar", "comum"),
        "status": meta.get("status", "cânone"),
        "camada": meta.get("camada", "base"),
        "text": in_world_text,
        "links": sorted({link.strip() for link in re.findall(r"\[\[([^\]|#]+)", meta.get("liga", ""))}),
    }


def collect_fragments():
    fragments = []
    for viewer in VIEWERS:
        folder = os.path.join(FRAGMENTS_DIR, viewer)
        if not os.path.isdir(folder):
            continue
        for name in sorted(os.listdir(folder)):
            if name.endswith(".md"):
                item = parse_note(os.path.join(folder, name))
                if item:
                    fragments.append(item)
    return fragments


def read_existing_translations(csv_path):
    existing = {}
    if not os.path.isfile(csv_path):
        return existing
    with open(csv_path, "r", encoding="utf-8", newline="") as handle:
        for row in csv.DictReader(handle):
            existing[row.get("Key", "")] = row
    return existing


def write_localization_csv(csv_path, fragments):
    existing = read_existing_translations(csv_path)
    rows = []
    for fragment in fragments:
        for suffix, source in (("title", fragment["title"]), ("text", fragment["text"])):
            key = f"fragment.{fragment['id']}.{suffix}"
            row = {"Key": key, "pt-BR": source}
            previous = existing.get(key)
            keep = previous is not None and previous.get("pt-BR") == source
            for locale in LOCALES[1:]:
                row[locale] = previous.get(locale, "") if keep else ""
            rows.append(row)

    os.makedirs(os.path.dirname(csv_path), exist_ok=True)
    with open(csv_path, "w", encoding="utf-8", newline="\n") as handle:
        writer = csv.DictWriter(handle, fieldnames=("Key",) + LOCALES, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)
    return len(rows)


def main():
    parser = argparse.ArgumentParser(description="Exporta fragmentos do Daedalus para a Unity.")
    parser.add_argument("--unity-project", default=UNITY_PROJECT, help="Raiz do projeto Unity.")
    args = parser.parse_args()

    data_root = os.path.join(args.unity_project, "Assets", "_Project", "Data")
    json_path = os.path.join(data_root, "Lore", "Generated", "fragments.json")
    csv_path = os.path.join(data_root, "Localization", "Source", "daedalus_strings.csv")

    fragments = collect_fragments()
    os.makedirs(os.path.dirname(json_path), exist_ok=True)
    with open(json_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump({"game": "parallax", "count": len(fragments), "fragments": fragments}, handle, ensure_ascii=False, indent=2)
        handle.write("\n")

    keys = write_localization_csv(csv_path, fragments)
    print(f"Exportados {len(fragments)} fragmentos in-world ({keys} chaves de localização) para {os.path.relpath(data_root, ROOT)}")


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""
Validador oficial do Project Daedalus (HTL Games).
Verifica:
1. Frontmatter YAML obrigatório nas notas de jogos/parallax/ (exceto blueprint-de-investimento.md, que é documento público de TTS).
2. Existência de todos os alvos de Wikilinks [[...]].
3. Ausência de termos e gerúndios lusitanos (PT-PT).
4. Vazamento de links de 'restrito/' em 'fragmentos/'.
"""

import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PARALLAX_DIR = os.path.join(ROOT, "jogos", "parallax")

PT_PT_PATTERNS = [
    r"\b(estou|está|estamos|estão|estava|estavam|andar|andam|continuar|continuam|ficar|ficam)\s+a\s+(fazer|dizer|estar|ter|ser|criar|usar|rodar|contar|funcionar)\b",
    r"\bfacto\b",
    r"\bequipa\b",
    r"\becrã\b",
    r"\butilizador\b",
    r"\bcontacto\b",
]

REQUIRED_YAML_KEYS = {"jogo", "tipo", "status", "camada", "liga"}
EXEMPT_FRONTMATTER = {"blueprint-de-investimento.md"}


def as_posix(path):
    return path.replace(os.sep, "/")


SKIPPED_DIRS = ("/.git", "/.agents", "/.cursor", "/studio/ide-", "/client/parallax-unity/Library", "/client/parallax-unity/Temp")


def collect_markdown_files():
    md_files = {}
    for dirpath, _, filenames in os.walk(ROOT):
        if any(skipped in as_posix(dirpath) for skipped in SKIPPED_DIRS):
            continue
        for fname in filenames:
            if fname.endswith(".md"):
                stem = fname[:-3]
                md_files[stem] = os.path.join(dirpath, fname)
    return md_files


def validate():
    md_files = collect_markdown_files()
    errors = []
    notes_count = 0

    for dirpath, _, filenames in os.walk(PARALLAX_DIR):
        for fname in filenames:
            if not fname.endswith(".md"):
                continue
            notes_count += 1
            fpath = os.path.join(dirpath, fname)
            rel_path = os.path.relpath(fpath, ROOT)
            with open(fpath, "r", encoding="utf-8") as f:
                content = f.read().replace("\r\n", "\n")

            # 1. Check YAML frontmatter
            if fname not in EXEMPT_FRONTMATTER:
                if not content.startswith("---\n"):
                    errors.append(f"[FRONTMATTER AUSENTE] {rel_path}")
                else:
                    end_idx = content.find("\n---", 4)
                    if end_idx == -1:
                        errors.append(f"[FRONTMATTER INCOMPLETO] {rel_path}")
                    else:
                        yaml_block = content[4:end_idx]
                        present_keys = {
                            line.split(":", 1)[0].strip()
                            for line in yaml_block.splitlines()
                            if ":" in line
                        }
                        missing = REQUIRED_YAML_KEYS - present_keys
                        if missing:
                            errors.append(f"[CHAVES YAML FALTANDO: {missing}] {rel_path}")

            # 2. Check Wikilinks
            for match in re.findall(r"\[\[([^\]|#]+)", content):
                target = match.strip()
                if target not in md_files:
                    errors.append(f"[WIKILINK QUEBRADO] {rel_path} -> [[{target}]]")
                elif "/fragmentos/" in as_posix(fpath) and target.startswith("verdade-"):
                    errors.append(f"[VAZAMENTO RESTRITO EM FRAGMENTO] {rel_path} referencia [[{target}]]")

            # 3. Check PT-PT terms
            for pat in PT_PT_PATTERNS:
                for m in re.finditer(pat, content, flags=re.IGNORECASE):
                    errors.append(f"[TERMO PT-PT DETECTADO: '{m.group(0)}'] {rel_path}")

    if errors:
        print(f"FALHA NA VALIDAÇÃO DO DAEDALUS ({len(errors)} problemas encontrados):")
        for err in errors:
            print("  -", err)
        sys.exit(1)
    else:
        print(f"DAEDALUS OK: {notes_count} notas validadas em jogos/parallax/ (0 links quebrados, 0 termos PT-PT).")
        sys.exit(0)


if __name__ == "__main__":
    validate()

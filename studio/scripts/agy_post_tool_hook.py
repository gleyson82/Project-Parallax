#!/usr/bin/env python3
"""
Hook PostToolUse para o Antigravity (.agents/hooks.json).
Quando um arquivo Markdown dentro de jogos/parallax/ é criado ou editado,
executa studio/scripts/validate_daedalus.py silenciosamente (erros vão para stderr)
e sempre retorna {} em stdout conforme o contrato de PostToolUse do Antigravity.
"""

import json
import os
import subprocess
import sys


def main() -> None:
    try:
        raw = sys.stdin.read().strip()
        payload = json.loads(raw) if raw else {}
    except Exception:
        payload = {}

    try:
        args = payload.get("toolCall", {}).get("args", {})
        target_path = str(args.get("Path", "")).replace("\\", "/").lower()
        if "/jogos/parallax/" in target_path and target_path.endswith(".md"):
            validator = os.path.join(
                os.path.dirname(os.path.abspath(__file__)),
                "validate_daedalus.py",
            )
            res = subprocess.run(
                [sys.executable, validator],
                capture_output=True,
                text=True,
                timeout=10,
            )
            if res.returncode != 0 and res.stdout:
                sys.stderr.write(res.stdout)
    except Exception:
        pass

    print(json.dumps({}))


if __name__ == "__main__":
    main()

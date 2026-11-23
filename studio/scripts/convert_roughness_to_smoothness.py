#!/usr/bin/env python3
"""
Converte mapas *_roughness.png (padrão do Material Maker) em *_metallicSmoothness.png do URP Lit:
canal R = metálico, canais G/B = 0, alfa = 1 - roughness.

Uso (sem instalar nada no Python global):
  uv run --with pillow studio/scripts/convert_roughness_to_smoothness.py [pasta] [--metallic 0.0]
"""

import argparse
import os
import sys

try:
    from PIL import Image, ImageOps
except ImportError:
    sys.exit("Pillow ausente. Rode com: uv run --with pillow studio/scripts/convert_roughness_to_smoothness.py")

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DEFAULT_ART = os.path.join(ROOT, "client", "parallax-unity", "Assets", "_Project", "Art")


def convert(path, metallic):
    out_path = path[: -len("_roughness.png")] + "_metallicSmoothness.png"
    roughness = Image.open(path).convert("L")
    smoothness = ImageOps.invert(roughness)
    metal = Image.new("L", roughness.size, int(round(max(0.0, min(1.0, metallic)) * 255)))
    zero = Image.new("L", roughness.size, 0)
    Image.merge("RGBA", (metal, zero, zero, smoothness)).save(out_path, optimize=True)
    return out_path


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", nargs="?", default=DEFAULT_ART)
    parser.add_argument("--metallic", type=float, default=0.0)
    args = parser.parse_args()

    count = 0
    for dirpath, _, filenames in os.walk(args.folder):
        for name in filenames:
            if name.endswith("_roughness.png"):
                out = convert(os.path.join(dirpath, name), args.metallic)
                print("OK", os.path.relpath(out, ROOT))
                count += 1
    print(f"{count} mapas convertidos.")


if __name__ == "__main__":
    main()

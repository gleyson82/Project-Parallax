---
name: parallax-asset-pipeline
description: >-
  Orquestra a criação e importação de cenários brutalistas (ProBuilder + kits do Blender), modelos 3D
  low-poly (Blender 5.3 + Blender MCP, Blockbench, Tripo/Hunyuan3D), texturas PBR (Material Maker +
  ComfyUI local + Krita), áudio industrial e a exportação do Project Daedalus para a Unity 6 LTS, dentro
  do orçamento de US$ 2.000.
---

# Skill: PARALLAX Asset Pipeline & Automation (Unity 6 LTS)

Use esta skill quando o pedido envolver gerar, converter, automatizar ou importar modelos 3D, níveis brutalistas, texturas PBR, retratos e HUD 2D, vozes de rádio, efeitos sonoros ou dados de lore para `client/parallax-unity/`.

## Fluxos por tipo de ativo

1. **Níveis brutalistas (ProBuilder + kits modulares):**
   - Blockout direto na Unity com **ProBuilder** (paredes monolíticas, vigas, dutos, abismos verticais), em escala métrica real: Goliath 2,6 m, Phantom e Cipher 1,8 m.
   - Detalhe com kits do Blender (tubulações, flanges, passarelas).
   - Entidades de puzzle são prefabs com componentes (`HeavyDoor`, `LiveLine`, `GrapplePoint`, `OverrideTerminal`, `LockActuator`), e não entidades FGD.
2. **Automação 3D via Blender MCP (Blender 5.3 + addon `blender_mcp`):**
   - Confirme a conexão antes de rodar `bpy`.
   - Exporte **FBX** (personagens com rig) ou **GLB** (props) direto para `Assets/_Project/Art/<Characters|Props|Environment|Weapons|Enemies>/`. Os `.blend` ficam em `art-pipeline/source/blender/`.
   - Malhas com sufixo `_col` viram `MeshCollider` automaticamente (`ParallaxAssetPostprocessor`).
3. **Economia de rigs (2 esqueletos, Humanoid na Unity):**
   - `Rig_Bipede_Padrao` (1,8 m): Phantom, Cipher, Esquadras de Contenção.
   - `Rig_Bipede_Pesado` (2,6 m): Goliath, Mestre de Fornalha, Regulador de Brandt, Gestor-9.
   - Modelos em `Art/Characters/` importam como Humanoid para reaproveitar animações do Mixamo.
4. **Texturas PBR:**
   - Material Maker/ComfyUI geram `_albedo`, `_normal`, `_roughness`. Converta roughness para o URP Lit com
     `uv run --no-project --with pillow studio/scripts/convert_roughness_to_smoothness.py` (gera `_metallicSmoothness`).
   - **Regra de copyright da HTL Games:** nenhum raster gerado por IA entra cru no lançamento; toda imagem passa por paintover humano no Krita ou Aseprite.
5. **Áudio e rádio militar:**
   - Vozes (Candeia, Brandt): corte abaixo de 300 Hz e acima de 3.400 Hz, leve saturação e ruído de interferência magnética; exporte `.ogg` para `Assets/_Project/Audio/Radio/`.
   - A Batida usa amostras curtas em `Audio/Beat/` disparadas por `BeatSequencer` (`dspTime`). A Nota do Piso é sintetizada (`FloorNoteSynth`), sem arquivo.
6. **Daedalus → Unity:**
   - `python studio/scripts/sync_daedalus.py` gera `Data/Lore/Generated/fragments.json` e `Data/Localization/Source/daedalus_strings.csv`.
   - Na Unity: menu `Parallax/Daedalus/Import Generated JSON` (cria `LoreFragment` e preenche a tabela `Lore` em PT-BR, EN, ZH-CN, ES).

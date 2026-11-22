---
name: parallax-asset-pipeline
description: >-
  Orquestra a criação e importação de cenários brutalistas (TrenchBroom + FuncGodot),
  modelos 3D low-poly (Blender 5.3 + mcp-for-blender v2, Blockbench, Tripo/Hunyuan3D),
  arte 2D/texturas PBR (ComfyUI MCP local + Material Maker + Krita) e áudio industrial
  para o Godot 4 dentro do orçamento de US$ 2.000.
---

# Skill: PARALLAX Asset Pipeline & Automation

Use esta skill quando o usuário pedir para gerar, converter, automatizar ou importar modelos 3D, mapas brutalistas, texturas PBR, retratos/HUD 2D, vozes de rádio ou efeitos sonoros para o projeto.

## Fluxos de Automação por Tipo de Ativo

1. **Cenários Brutalistas (TrenchBroom -> FuncGodot):**
   - Gere ou configure definições `.fgd` (Forge Game Data) para entidades de puzzle de PARALLAX (`func_valve_thermatek`, `func_live_line`, `func_triple_lock`, `func_listening_ring`).
   - Mapas `.map` salvos em `res://maps/` são construídos automaticamente em malhas 3D e colisores Jolt pelo **FuncGodot**.
2. **Automação 3D via Blender MCP (`mcp-for-blender` v2 / Blender 5.3):**
   - Sempre valide a conexão com `get_addon_status` antes de rodar scripts `bpy`.
   - Use `execute_blender_code` para gerar os 3 Auxiliares (`Macaco`, `Pipa`, `Cartucho de Override`), tubulações modulares, flanges, válvulas, passarelas e aplicar o modificador `Decimate` em malhas geradas por IA (como as carcaças fundidas do chefe **Gestor-9**).
   - Exporte sempre em formato **`.glb` (glTF 2.0 binário)** diretamente para as pastas de módulo em `client/parallax/`.
3. **Economia de Rigs (Apenas 2 Esqueletos):**
   - Vincule personagens humanos e trajes leves ao `Rig_Bipede_Padrao` (`1,8 m` — Phantom, Cipher, Esquadras de Contenção).
   - Vincule exoesqueletos industriais ao `Rig_Bipede_Pesado` (`2,6 m` — Goliath, Mestre de Fornalha, Regulador de Brandt, Gestor-9).
4. **Geração 2D e Texturas PBR Locais via ComfyUI MCP (`comfyui`):**
   - Servidor local em `http://127.0.0.1:8188` com saída em `C:\ComfyUI\output_ide`.
   - Use para conceitos visuais, bases de retratos de rádio, panoramas (Face Voltada com Netuno a 48° vs. Face Cega sem Netuno) e bases de texturas tileable para os 5 biomas (`art-pipeline/05_biomas_e_transportes/`).
   - **Regra de Copyright da HTL Games:** nenhum ativo raster gerado por IA entra cru no lançamento comercial da Steam; toda imagem gerada serve como base para acabamento/paintover humano no **Krita** ou **Aseprite**, com mapas PBR complementados no **Material Maker**.
5. **Áudio e Filtro de Rádio Militar:**
   - Para vozes geradas no sprint (Candeia, Brandt), aplique processamento de rádio industrial (corte abaixo de 300 Hz e acima de 3.400 Hz, leve saturação e ruído de interferência magnética de Netuno) e exporte em `.ogg`.

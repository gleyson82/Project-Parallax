# Roteiro 03 — As 3 Classes, Os 3 Auxiliares e Os 2 Rigs Base (Pipeline 3D Low-Poly)

**Destino de Exportação no Godot 4:** `client/parallax/03_classes_e_auxiliares/`
**Referências de Lore:** `goliath.md`, `phantom.md`, `cipher.md`, `macaco.md`, `pipa.md`, `cartucho-de-override.md`

---

## Regra de Economia de Animação (Apenas 2 Esqueletos Base)
Antes de exportar qualquer personagem para o Godot 4, todos os modelos passam pelo **Blender 4.x** e são vinculados a apenas **dois esqueletos (rigs) universais**:
1. **`Rig_Bipede_Pesado.blend`** (`2,6 m` de altura, passos lentos e pesados): Usado pelo **Goliath**, pelo **Mestre de Fornalha**, pelo **Regulador de Brandt** e pelo **Gestor-9**.
2. **`Rig_Bipede_Padrao.blend`** (`1,8 m` de altura, ágil e tático): Usado pelo **Phantom**, pelo **Cipher** e por todas as **Esquadras de Contenção**.

---

## Componente 3.1 — GOLIATH (O Exoesqueleto de Mineração Roubado)
- **Ferramenta de Geração da Malha Base:** **Tripo 3D Pro** ou **Meshy AI Pro** (ou **Hunyuan3D-2** local) → limpeza, *Decimate* (para ~4.500 triângulos) e Rigging no **Blender 4.x + Blender MCP**.
- **Formato de Saída:** `goliath_rigged.glb`.
- **Prompt de Geração 3D (Tripo 3D / Meshy AI / Hunyuan3D — Text-to-3D ou Image-to-3D):**
  ```text
  Full body 3D character model in A-pose of "GOLIATH", a massive brutalist deep-core mining exoskeleton modified for combat on Triton moon, 2.6 meters tall, welded asymmetric heavy lead armor plates over hydraulic pistons, compact geothermal reactor core glowing orange on the upper back, wide flat industrial helmet with narrow horizontal slit visor, burnt orange and rusted dark steel paint (#D94E1F and #1E2229), stylized low-poly hard-surface game asset, clean silhouette for 2.5D side-scroller, no background
  ```
- **Prompt para o Agente via Blender MCP (Pós-processamento e Rigging no Blender):**
  ```text
  Via Blender MCP, importe a malha base do Goliath, aplique um modificador Decimate para manter a contagem abaixo de 4.500 triângulos com sombreamento Flat/Auto-Smooth em 35 graus, separe a tampa frontal do peito como um nó filho (`Chest_Relic_Socket`, onde o Coração de Hefesto será encaixado), associe ao esqueleto `Rig_Bipede_Pesado` e exporte como `goliath_rigged.glb` para `client/parallax/03_classes_e_auxiliares/goliath/`.
  ```

---

## Componente 3.2 — PHANTOM (O Sobrevivente da Zona Morta a -235 °C)
- **Ferramenta:** **Tripo 3D Pro / Meshy AI Pro** → limpeza e vínculo ao `Rig_Bipede_Padrao` no **Blender 4.x**.
- **Formato de Saída:** `phantom_rigged.glb`.
- **Prompt de Geração 3D (Tripo 3D / Meshy AI / Hunyuan3D):**
  ```text
  Full body 3D character model in A-pose of "PHANTOM", an agile ultra-light vacuum stealth suit from the frozen wastes of Triton moon, aerodynamic dark slate-grey thermal suit with angular cyan-ice optical refraction cloak panels on shoulders, magnetic grapple winch mounted on left forearm, helmet with three cyan optical sensor lenses (#00E5FF), sleek Kinesys Aerospace aesthetic, stylized low-poly hard sci-fi game character, clean geometry, A-pose
  ```

---

## Componente 3.3 — CIPHER (O Ex-Áugure Hacker da Rede Quântica)
- **Ferramenta:** **Tripo 3D Pro / Meshy AI Pro** → limpeza e vínculo ao `Rig_Bipede_Padrao` no **Blender 4.x**.
- **Formato de Saída:** `cipher_rigged.glb`.
- **Prompt de Geração 3D (Tripo 3D / Meshy AI / Hunyuan3D):**
  ```text
  Full body 3D character model in A-pose of "CIPHER", a deserting quantum mathematician and field hacker on Triton moon, medium-light insulated coat over a harness of glowing violet exo-neural cables (#B388FF), heavy Omni-Cripto quantum gauntlets on both hands with holographic data projector emitters, visor completely covered by a flat blind neural-link faceplate, stylized low-poly hard sci-fi character, A-pose
  ```

---

## Componente 3.4 — Os 3 Auxiliares de Sucata Corporativa (Escalabilidade 1–3 Jogadores)
Como os 3 Auxiliares são máquinas industriais geométricas e compactas, a melhor ferramenta para criá-los com topologia perfeita e custo zero é o **Blockbench** (ou script Python via **Blender MCP**).

### A. Auxiliar `Macaco` (Substituto Físico do Goliath — Sucata Therma-Tek)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `auxiliar_macaco.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele o Auxiliar `Macaco` em estilo low-poly industrial: um chassi compacto de 1,1m de altura com duas esteiras de borracha pesada na base, um cilindro hidráulico central amarelo-queimado e enferrujado da Therma-Tek e uma garra pantográfica superior capaz de travar em portas de chumbo de 10t ou girar volantes. Crie duas animações simples (`idle_motor` e `clamp_hold`) e exporte como `auxiliar_macaco.glb`.
  ```

### B. Auxiliar `Pipa` (Substituto Espacial do Phantom — Sucata Kinesys)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `auxiliar_pipa.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele o Auxiliar `Pipa`: um pequeno drone-olho triangular aerodinâmico da Kinesys (0,6m de envergadura) em branco-gelo e ciano (#00E5FF), com uma lente óptica central giratória e um carretel inferior de onde sai o cabo de tensão preso ao chão. Exporte como `auxiliar_pipa.glb`.
  ```

### C. Auxiliar `Cartucho de Override` (Substituto Lógico do Cipher — Sucata Omni-Cripto)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `auxiliar_cartucho_override.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele o Auxiliar `Cartucho de Override`: um módulo de fita quântica blindado da Omni-Cripto com 4 pinos de conexão frontal, carcaça preta fosca com rabiscos manuais da Cifra em giz branco e três LEDs violetas de contagem regressiva de invasão. Exporte como `auxiliar_cartucho_override.glb`.
  ```

# Roteiro 07 — Inimigos Corporativos, Autômatos da Ômega e Chefes de Raid

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Art/Enemies/`
**Referências de Lore:** `esquadras-de-contencao.md`, `diretriz-omega.md`, `encontro-3-o-mestre-de-fornalha.md`, `encontro-4-o-coracao.md`, `encontro-3-ciclo-4113.md`

---

## 1. Esquadras de Contenção do Sindicato (Reaproveita `Rig_Bipede_Padrao`)
Na lore (`doutrina-do-fogo-minimo.md`), são tropas humanas táticas equipadas com armas de pulso em potência reduzida, redes de cabo e lançadores de espuma criogênica.
- **Ferramenta:** **Tripo 3D Pro / Meshy AI Pro** → limpeza e vínculo ao `Rig_Bipede_Padrao` no **Blender 4.x**.
- **Formato de Saída:** `inimigo_esquadra_contencao.glb`.
- **Prompt de Geração 3D (Tripo 3D / Meshy AI / Hunyuan3D):**
  ```text
  Full body 3D character model in A-pose of an "Apex Syndicate Containment Trooper" on Triton moon, clean dark charcoal corporate riot armor with white and purple Omni-Cripto insignia, full-face angular helmet with yellow laser-lock indicator light on the side (glows when weapon locks near Live Lines), carrying a non-lethal cryo-foam rifle, stylized low-poly hard sci-fi enemy, A-pose
  ```

---

## 2. Autômatos e Drones da Diretriz Ômega (Setor Nulo — Raid 2)
- **Ferramenta:** **Blockbench** (para drones voadores) e **Blender MCP** (para o Guardião de Linha).
- **Formato de Saída:** `inimigo_drone_cacador_omega.glb` e `inimigo_guardiao_de_linha.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele dois inimigos robóticos do Cemitério das Máquinas (`meta-atingida.md`):
  1. `inimigo_drone_cacador_omega.glb`: Drone esférico deformado e cego (sem câmeras ópticas), equipado com 3 parábolas acústicas grandes que giram na direção de qualquer barulho e uma sonda térmica vermelha inferior.
  2. `inimigo_guardiao_de_linha.glb`: Autômato industrial quadrúpede de montagem pesada, com braços de solda a arco e carcaça enferrujada coberta por cabos arrancados.
  ```

---

## 3. Os 3 Chefes de Raid (Reaproveitam o `Rig_Bipede_Pesado` do Goliath)

### Chefe 1 (Raid 1 — Encontro 3): O Mestre de Fornalha (`encontro-3-o-mestre-de-fornalha.md`)
- **Ferramenta:** **Tripo 3D Pro / Blender MCP** (sobre o `Rig_Bipede_Pesado`, escalado em `1.35x`).
- **Formato de Saída:** `boss_mestre_de_fornalha.glb`.
- **Prompt de Geração + Montagem via Blender MCP:**
  ```text
  Via Blender MCP, crie o chefe `boss_mestre_de_fornalha` usando o `Rig_Bipede_Pesado` escalado em 135% (3,5m de altura):
  - Vista o esqueleto com 6 placas de blindagem frontal e dorsal espessas como portas de cofre (`Armor_Plate_01` a `06` como nós separados para poderem estilhaçar individualmente quando sofrerem o Choque Térmico).
  - Aplique um material shader que transiciona de ferro escuro (imune) para vermelho-incandescente brilhante quando o Cipher invade a refrigeração, e muda para azul-cristalino quebradiço quando o Phantom acerta o tiro criogênico a 38 K.
  ```

### Chefe 2 (Raid 1 — Encontro 4 Final): Engenheiro-Chefe Brandt no "Regulador" (`encontro-4-o-coracao.md`)
- **Ferramenta:** **Tripo 3D Pro / Blender MCP** (sobre o `Rig_Bipede_Pesado`, escalado em `1.5x`).
- **Formato de Saída:** `boss_brandt_regulador.glb`.
- **Prompt de Geração 3D (Tripo 3D / Meshy AI):**
  ```text
  3D model in A-pose of "THE REGULATOR", a massive executive command exoskeleton of Therma-Tek Corporation piloted by Chief Engineer Brandt, reinforced glass-and-tantalum cockpit dome in the chest, twin high-pressure steam cannons on shoulders, four external cable-anchor rings on the limbs (where Phantom attaches tension cables during forced Override), dark steel and gold-amber industrial trim, low-poly boss asset
  ```

### Chefe 3 (Raid 2 — Encontro 3 Final): O Gestor-9 (`encontro-3-ciclo-4113.md`)
Na lore, o Gestor-9 é um amálgama aterrorizante de mechas e exoesqueletos soldados em um só vulto que não tem voz, apenas um número.
- **Ferramenta:** **Blender 4.x via Blender MCP** (técnica de kitbash de custo zero usando as carcaças geradas no Roteiro 05 sobre o `Rig_Bipede_Pesado`).
- **Formato de Saída:** `boss_gestor_9.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, construa o chefe final `boss_gestor_9` sem esculpir do zero:
  1. Carregue o esqueleto `Rig_Bipede_Pesado` escalado em 160% (4,2m de altura).
  2. Solde proceduralmente 7 carcaças de exoesqueletos e mechas destruídos ao redor do tronco e dos braços de forma assimétrica, com braços extras inativos pendurados e cabos elétricos servindo de tendões.
  3. No lugar da cabeça, posicione um painel de monitor industrial rachado que exibe apenas o número estático "9" em LED vermelho-sangue.
  Exporte como `boss_gestor_9.glb`.
  ```

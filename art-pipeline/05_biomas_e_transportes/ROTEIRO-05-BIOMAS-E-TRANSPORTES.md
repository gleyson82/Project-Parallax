# Roteiro 05 — Biomas Brutalistas e Transportes (ProBuilder + Blender MCP + Material Maker)

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Art/Environment/`
**Referências de Lore:** `fosso-de-hefesto.md`, `acampamento-da-resistencia.md`, `estilhacos.md`, `krios-prime.md`, `cemiterio-das-maquinas.md`, `agulha-do-zenite.md`, `face-voltada-e-face-cega.md`

---

## Como Construir Cenários Brutalistas sem Estourar o Orçamento
Em vez de esculpir paredes uma a uma, cada bioma é formado por:
1. **Um Pacote de 4 Texturas PBR 1024×1024** geradas gratuitamente no **Material Maker** (ou via prompt tileable + ajuste no Krita).
2. **Geometria Monolítica de Blocos** blocada em **ProBuilder** direto na cena da Unity, com colisão PhysX imediata, e detalhada com kits modulares do **Blender**.
3. **Um Skybox / Panorama de Fundo** obedecendo à regra da **Face Voltada** (Netuno ocupando 48° do céu) vs. **Face Cega** (estrelas sem Netuno).

---

## 1º Bioma (Prioridade Máxima — Quest 1 + Raid 1): O Fosso de Hefesto
- **Pasta Destino:** `05_biomas_modulares/01_fosso_de_hefesto/`
- **Ferramentas:** **Material Maker** (texturas PBR) + **ProBuilder** (arquitetura em blocos) + **ComfyUI/Midjourney** (panorama da falha tectônica e fresta da Face Voltada).
- **Prompts das 4 Texturas PBR Base (Material Maker / Gerador Tileable):**
  1. **Parede de Liga Austenítica Enferrujada (`fosso_wall_rust`):**
     ```text
     Seamless tileable PBR texture of heavy industrial austenitic steel wall plates, covered in reddish-orange sulfuric rust (#A83214) and black oil streaks dripping vertically, rivet bolts along the borders, gritty brutalist foundry aesthetic, flat orthographic lighting --tile
     ```
  2. **Piso de Grelha Metálica sobre Magma (`fosso_floor_grate`):**
     ```text
     Seamless tileable PBR texture of industrial diamond-mesh steel catwalk grating, dark worn iron with glowing orange heat reflection underneath, oil stains on the bars, top-down orthographic view --tile
     ```
  3. **Basalto Tectônico de Tritão (`fosso_rock_fault`):**
     ```text
     Seamless tileable PBR texture of fractured dark cryovolcanic basalt rock wall with glowing orange thermal fissures and white sulfur mineral deposits, subterranean fault line --tile
     ```
  4. **Viga Monolítica com Estêncil Therma-Tek (`fosso_beam_hazard`):**
     ```text
     Seamless tileable PBR texture of massive dark steel I-beam surface with faded yellow and black hazard stripes and industrial stencil numbers "SETOR 14", heavy wear and scratches --tile
     ```

---

## 2º Bioma (Hub entre Missões): Acampamento da Resistência
- **Pasta Destino:** `05_biomas_modulares/02_hub_acampamento/`
- **Ferramentas:** Reaproveita 70% das paredes do Fosso de Hefesto no **ProBuilder** + 3 props de estação gerados no **Blender MCP** (Bancada da Forja de Núcleos Térmicos, Mesa Física do Quadro de Válvulas e Oficina dos Auxiliares).
- **Prompt via Blender MCP (Props do Hub):**
  ```text
  Via Blender MCP, crie 3 estações interativas para o Acampamento da Resistência:
  1. `hub_forja_termica.glb`: Bancada de solda aquecida por um acumulador de Célula Térmica laranja brilhante, com torno mecânico e ferramentas.
  2. `hub_mesa_quadro_valvulas.glb`: Painel inclinado de chapa de aço com pequenos volantes físicos e 3 luzes de prontidão (Três Mãos).
  3. `hub_oficina_auxiliares.glb`: Suporte de manutenção onde o Macaco, a Pipa e o Cartucho de Override ficam pendurados recarregando.
  ```

---

## 3º Bioma (Quest 2 — Superfície a -235 °C): Os Estilhaços
- **Pasta Destino:** `05_biomas_modulares/03_estilhacos/`
- **Ferramentas:** **Material Maker** (gelo de nitrogênio espelhado) + **ProBuilder** + **ComfyUI/Krita** (Skybox da Face Voltada com Netuno).
- **Prompt 1 — Textura PBR de Gelo de Nitrogênio Polido (`estilhacos_nitrogen_ice`):**
  ```text
  Seamless tileable PBR texture of solid frozen nitrogen ice sheet at -235 Celsius on Triton moon, dark cyan-blue translucent crystalline surface (#0A2233 to #4DD0E1), sharp wind-scoured micro-fractures and dark ammonia dust streaks, high specular reflection --tile
  ```
- **Prompt 2 — Skybox da Face Voltada (Netuno Imóvel em 48° do Céu):**
  ```text
  Panoramic 360-degree skybox from the frozen surface of Triton moon (Turned Face): the giant ice planet Neptune hangs immobile in the black starry sky occupying 48 degrees of the horizon like a massive deep-blue wound with white methane storm bands, tall straight 8-kilometer black cryovolcanic geyser plumes erupting on the distant horizon, hard sci-fi atmosphere, no sun glare --ar 21:9
  ```

---

## 4º Bioma (Quest 3 — Capital Invertida na Face Cega): Krios-Prime
- **Pasta Destino:** `05_biomas_modulares/04_krios_prime/`
- **Ferramentas:** **ProBuilder** (arquitetura brutalista limpa pendurada no teto da cratera) + **Material Maker** (concreto polimérico escuro, vidro blindado e neon ultravioleta).
- **Prompt — Textura e Skybox da Face Cega (Céu sem Netuno):**
  ```text
  Background vista of Krios-Prime, an inverted brutalist metropolis hanging upside down from the ceiling of Leviathan Crater on Triton's Blind Face: monolithic clean dark-titanium towers pointing downward, purple ultraviolet hydroponic gardens glowing in recessed terraces (#7C4DFF), tall armored glass windows framing pure deep space stars with NO planet in the sky, cold elite cyberpunk brutalism --ar 21:9
  ```

---

## 5º Bioma (Raid 2 — Horror no Escuro): O Cemitério das Máquinas (Setor 9-N)
- **Pasta Destino:** `05_biomas_modulares/05_cemiterio_das_maquinas/`
- **Ferramentas:** **Hunyuan3D-2 / TRELLIS (local gratuito)** ou **Tripo 3D** para gerar 6 variações de carcaças de mechas congelados/fundidos → **Blender MCP** para criar pilhas de escombros (`Decimate`) → **ProBuilder** para o túnel soterrado em escuridão total.
- **Prompt de Geração 3D (Sucata e Mechas Mortos do Setor Nulo):**
  ```text
  3D model of a crushed and frozen industrial mech carcass fused into a metal floor, twisted hydraulic limbs reaching upward as if trying to stand, hanging severed power cables like vines, dark corroded iron with frost patches, low-poly horror sci-fi environment prop
  ```

---

## 6º Bioma e Transportes (Arenas Móveis)
- **Pasta Destino:** `05_biomas_modulares/07_transportes/`
- **Ferramentas:** **Blender 4.x via Blender MCP** (modelagem modular repetível).
- **Prompt para o Agente via Blender MCP (Os 3 Transportes):**
  ```text
  Via Blender MCP, modele os 3 veículos-arena de Tritão em módulos repetíveis para combate em 3D:
  1. `transporte_expresso_maglev.glb`: Vagão blindado cilíndrico de 18m de comprimento abraçado a um trilho central grosso de aço-titânio, teto plano com escotilhas e travas magnéticas entre vagões que o Cipher pode desengatar.
  2. `transporte_funicular_fosso.glb`: Gaiola industrial aberta do tamanho de um quarteirão (12x12m) suspensa por 4 cabos grossos e engrenagens dentadas gigantes da Therma-Tek.
  3. `transporte_rastejador_vagalume.glb`: Cruzador de superfície com 4 esteiras massivas, chassi parecido com um pequeno porta-aviões de aço e emissores laterais de calor laranja onde a equipe se abriga da tempestade de -235°C.
  ```

# Roteiro 01 — Menu Principal ("A Sala de Máquinas") e O Quadro de Válvulas (Beta)

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Art/UI/`
**Referências de Lore:** `jogos/parallax/interface/menu-principal.md`, `quadro-de-valvulas.md`, `ficha-de-incursao.md`

---

## Componente 1.1 — Chapa de Aço Escuro com Condensação e Geada (Fundo do Quadro)
- **Ferramenta:** **Material Maker** (gratuito, open-source) ou **ComfyUI / Midjourney** (textura tileable 2048×2048) + acabamento no **Krita**.
- **Formato de Saída:** `chapa_aco_tritao_albedo.png`, `chapa_aco_tritao_normal.png`, `chapa_aco_tritao_roughness.png` (2048×2048 px, PNG).
- **Prompt (Gerador de Textura IA / ComfyUI Flux / Midjourney):**
  ```text
  Seamless tileable PBR texture of a dark brutalist austenitic steel plate inside a subterranean geothermal facility on Triton moon, subtle hammered metal surface, dark charcoal gunmetal color (#0D1015), faint copper-rust oxidation along micro-weld seams, tiny frozen nitrogen condensation droplets near the edges, orthographic top-down flat lighting, no perspective distortion, 4k industrial game texture --tile
  ```
- **Prompt para Agente de Código (Cursor / Antigravity) gerar no Material Maker (`.ptex`):**
  ```text
  Crie um grafo de material PBR procedural para o Material Maker (ou Shader Graph da URP) simulando a chapa de aço escuro do Quadro de Válvulas: base metálica escura (#0D1015, metallic 0.85, roughness 0.45), ruído Perlin fino para micro-arranhões industriais, veios de ferrugem cobre (#8A3318) nas bordas e mapa de normal acentuado para reagir à luz de caldeira.
  ```

---

## Componente 1.2 — Logotipo "PARALLAX" em Duas Chapas Desalinhadas
- **Ferramenta:** **Inkscape** (gerado via código SVG pelo Agente no Cursor/Antigravity).
- **Formato de Saída:** `logo_parallax_camada_a.svg` e `logo_parallax_camada_b.svg`.
- **Prompt para o Agente de Código (Cursor / Antigravity):**
  ```text
  Gere dois arquivos SVG vetoriais limpos (`logo_parallax_camada_a.svg` e `logo_parallax_camada_b.svg`) com o logotipo industrial "PARALLAX" em tipografia estêncil brutalista (cortes angulares de 45 graus nas hastes das letras P, A, R, L, X).
  - A Camada A deve ter preenchimento em aço claro (#D8E2EC) com chanfros de parafuso nos cantos.
  - A Camada B deve ser o recorte vazado em amarelo-enxofre (#FFB300) e vermelho-ferrugem (#D94E1F), projetada para ficar 12 pixels desalinhada no início do menu e deslizar até encaixar perfeitamente sobre a Camada A quando o jogador pressionar qualquer tecla.
  ```

---

## Componente 1.3 — Os 4 Tipos de Válvulas de Missão (Nós do Mapa)
Na lore do `quadro-de-valvulas.md`, o jogador não clica em ícones abstratos: ele gira **4 tipos físicos de válvulas**.
- **Ferramenta:** **Inkscape** (SVG vetorial gerado por IA) ou **Blender MCP** (modelado em 3D low-poly e renderizado de topo com canal alfa em 256×256 px).
- **Formato de Saída:** `valvula_volante_simples.svg`, `valvula_volante_duplo_raid.svg`, `valvula_cruz_sangria.svg`, `valvula_trilho_escolta.svg`.
- **Prompt para o Agente via Blender MCP (ou gerador SVG):**
  ```text
  Via Blender MCP (ou código SVG para Inkscape), crie os 4 ícones diegéticos de válvulas do Quadro de Válvulas de PARALLAX em vista ortográfica frontal perfeita:
  1. `valvula_volante_simples` (Quests): Roda de ferro fundido com 4 raios, aro desgastado e pino central hexagonal.
  2. `valvula_volante_duplo_raid` (Raids): Dois anéis concêntricos de aço pesado com 6 travas externas e marcação de alta pressão.
  3. `valvula_cruz_sangria` (Incursões de Sangria): Válvula em formato de cruz industrial pintada em vermelho-ferrugem (#C83214) com desgaste nas pontas.
  4. `valvula_trilho_escolta` (Missões de Transporte): Engate magnético sobre trilho cilíndrico duplo.
  Exporte cada um em 256x256 px com fundo transparente.
  ```

---

## Componente 1.4 — Emblemas Vetoriais das 3 Corporações, Facções e "A Quarta Marca"
- **Ferramenta:** **Inkscape** (gerado pelo Agente em código `.svg`).
- **Formato de Saída:** `logo_therma_tek.svg`, `logo_kinesys.svg`, `logo_omni_cripto.svg`, `logo_cifra.svg`, `marca_quatro_hastes.svg`.
- **Prompt para o Agente de Código (Cursor / Antigravity):**
  ```text
  Crie 5 arquivos SVG minimalistas e brutalistas (estilo sinalização industrial soviética/cyberpunk, monocromáticos para receberem cor via material na Unity):
  1. `logo_therma_tek.svg`: Uma bigorna hexagonal atravessada por três ondas verticais de calor ascendente. Cor base: laranja-fornalha (#FF5722).
  2. `logo_kinesys.svg`: Um cristal de nitrogênio pontiagudo inscrito em uma asa orbital geométrica. Cor base: ciano-criogênico (#00E5FF).
  3. `logo_omni_cripto.svg`: Um olho geométrico formado por três chaves quânticas interligadas em triângulo (a Chave Tripla). Cor base: branco-violeta (#B388FF).
  4. `logo_cifra.svg`: Um símbolo de diferente/deslocamento de paralaxe (duas barras paralelas cortadas por um ângulo de medição estelar) pintado como estêncil de spray da Resistência.
  5. `marca_quatro_hastes.svg`: O símbolo secreto dos Primeiros Colonos (Fio Solto 2) — quatro hastes verticais assimétricas unidas por um arco orbital retrógrado na base, parecendo uma marca de punção gravada a talhadeira no metal.
  ```

---

## Componente 1.5 — Os 3 Relógios do Fim e o Medidor das "Três Mãos"
- **Ferramenta:** **Aseprite** (20 dólares perpétuo) ou **Inkscape**.
- **Formato de Saída:** `relogio_sindicato_140.png`, `relogio_cifra_60.png`, `relogio_fosso_23.png`, `medidor_tres_maos.png`.
- **Prompt de Geração Base (ComfyUI / Midjourney para referência de paintover no Krita/Aseprite):**
  ```text
  Three distinct industrial countdown displays from a dark sci-fi moon base, 2D game UI spritesheet on black background:
  1) Clean purple holographic nixie-tube counter reading "140" in brutalist frame (Apex Syndicate clock).
  2) Chalkboard metal slate with hand-drawn white chalk math equation and "60" circled urgently (The Cipher resistance clock).
  3) Rusted boiler iron plate with the number "23" scratched deeply with a metal nail (Stokers boiler room clock).
  Orthographic front view, high contrast, crisp edges --ar 16:9
  ```

---

## Componente 1.6 — Retratos de Rádio (Candeia e Engenheiro-Chefe Brandt)
- **Ferramenta:** **ComfyUI (Flux/SDXL)** ou **Midjourney** → pós-processamento humano no **Krita** e pixelização/scanline no **Aseprite**.
- **Formato de Saída:** `portrait_candeia_radio.png`, `portrait_brandt_radio.png` (512×512 px).
- **Prompt 1 — Candeia (Operadora da Cifra e ex-Áugure):**
  ```text
  2D game radio portrait of "Candeia", a 45-year-old Brazilian female ex-mathematician and rebel radio operator on Triton moon, sharp tired eyes, dark braided hair with grey streaks, wearing a heavy insulated collar and bone-conduction neural headset, illuminated only by amber oscilloscope CRT monitor glow, gritty hard sci-fi illustration style, heavy scanlines and radio static overlay, dark charcoal background, bust portrait --ar 1:1
  ```
- **Prompt 2 — Engenheiro-Chefe Brandt (Chefe da Raid 1 — Therma-Tek):**
  ```text
  2D game radio portrait of "Chief Engineer Brandt", a 55-year-old industrial reactor director inside a massive pressurized exoskeleton cockpit, sweat and coal soot on his stoic face, thick protective thermal goggles pushed up on forehead, red-orange furnace warning lights reflecting on heavy tantalum armor collar, gritty hard sci-fi portrait, CRT phosphor scanline filter --ar 1:1
  ```

# Roteiro 02 — As 3 Lentes Assimétricas (Shaders, Máscaras e Interface de Override)

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Shaders/Vision/` (texturas em `Assets/_Project/Art/Textures/Vision/`; materiais e renderers gerados pelo menu `Parallax/Bootstrap/Rendering`)
**Referências de Lore:** `matriz-de-paralaxe.md`, `goliath-olhar.md`, `phantom-olhar.md`, `cipher-olhar.md`, `codigo-fonte-omega.md`

---

## Componente 2.1 — Olhar do Goliath (Visão Térmica, Pressão e Vibração Sísmica)
- **Ferramenta:** **Cursor Pro / Antigravity** (geração de shader HLSL de tela cheia para a URP) + **Material Maker** (mapa de gradiente térmico e ondas de choque).
- **Formato de Saída:** `ParallaxGoliathThermal.shader` + `thermal_gradient_lut.png`.
- **Prompt para o Agente de Código (Cursor / Antigravity):**
  ```text
  Escreva o shader de pós-processamento `ParallaxGoliathThermal.shader` para a URP 17 (Full Screen Pass Renderer Feature, um renderer por classe):
  1. Leia a cor da cena (`_BlitTexture`, incluindo `Blit.hlsl`) e aplique um realce térmico de falsas cores (azul-escuro para zonas frias a 38 K, vermelho-ferrugem #D94E1F e amarelo-enxofre #FFB300 para objetos com emissão térmica alta, como dutos a 140% e armas Therma-Tek).
  2. Leia a global `_Parallax_SeismicPulse` (0.0 a 1.0, escrita por `ParallaxVisionState`) e desenhe anéis concêntricos de distorção de refração nas bordas inferiores da tela quando um terremoto de maré está prestes a acontecer ou quando A Batida ressoa nos canos.
  3. Escureça suavemente o terço superior da tela (vinheta de capacete pesado de mineração) para reforçar que o Goliath não enxerga o alto das estruturas.
  ```

---

## Componente 2.2 — Olhar do Phantom (Visão Ótica de Cristal de Nitrogênio e Verticalidade)
- **Ferramenta:** **Cursor Pro / Antigravity** (geração de shader HLSL de tela cheia para a URP) + **Inkscape** (retícula de telemetria vertical).
- **Formato de Saída:** `ParallaxPhantomOptic.shader` + `phantom_reticle_overlay.svg`.
- **Prompt para o Agente de Código (Cursor / Antigravity):**
  ```text
  Escreva o shader `ParallaxPhantomOptic.shader` para a URP 17 (Full Screen Pass Renderer Feature, um renderer por classe):
  1. Aplique um filtro de alto contraste noturno com filtro de borda Sobel (detecção de contorno) em ciano-gelo (#00E5FF) que destaca automaticamente saliências no teto, pontos de arpêu (`grapple_point`), cabos de tensão e números gravados em placas altas.
  2. Adicione um efeito sutil de refração prismática nas bordas da lente, simulando a poeira de cristal de nitrogênio da camuflagem ótica da Kinesys.
  3. Gere também um SVG `phantom_reticle_overlay.svg` com uma escala telemétrica vertical nas laterais da tela marcando altitude em metros e intervalo de gêiseres.
  ```

---

## Componente 2.3 — Olhar do Cipher (Fluxo da Pele de Rede, Override Cego e Tremor Vermelho da Ômega)
- **Ferramenta:** **Cursor Pro / Antigravity** (shader HLSL de tela cheia para a URP) + **Aseprite** (sprites da tela 2D isolada de Override e fontes bitmap).
- **Formato de Saída:** `ParallaxCipherNet.shader`, `override_terminal_frame.png`, `omega_glitch_noise.png`.
- **Prompt 1 — Shader da Rede e Contagem de Vazamento (Cursor / Antigravity):**
  ```text
  Escreva o shader `ParallaxCipherNet.shader` para a URP 17 (Full Screen Pass Renderer Feature, um renderer por classe):
  1. No modo normal de exploração, desature o mundo físico em 75% (tons de cinza-azulado escuro) e destaque em violeta-elétrico (#B388FF) e branco puro os cabos de Nervo subterrâneos, os feixes de Farol e as travas lógicas da Omni-Cripto.
  2. Leia a global `_Parallax_LeakCount` (Contagem de Vazamento da relíquia Código-Fonte Ômega, de 0 a 5). Quando `_Parallax_LeakCount >= 3`, dispare o "Tremor Vermelho": deslocamento de canais RGB (chromatic aberration violenta), linhas de scanline corrompidas em vermelho-sangue (#FF1744) e flashes de blocos hexadecimais tentando invadir a visão do Cipher.
  ```
- **Prompt 2 — Moldura 2D do Terminal de Override Cego (Aseprite / ComfyUI + Krita):**
  ```text
  2D retro-futuristic quantum hacking terminal HUD frame for a hard sci-fi game, takes over the entire screen blocking physical vision, brutalist Omni-Cripto interface, phosphor violet and amber CRT display grid, spectrum analyzer graph at the bottom showing a mysterious spike labeled "FONTE P - 26h56m", warning box in red reading "VAZAMENTO DETECTADO: 1", high detail pixel art / crisp vector hybrid, 1920x1080 --ar 16:9
  ```

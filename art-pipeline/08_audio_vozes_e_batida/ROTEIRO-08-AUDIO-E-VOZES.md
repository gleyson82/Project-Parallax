# Roteiro 08 — Áudio, Vozes de Rádio, "A Batida" e O Sinal de Proteu (Fonte P)

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Art/UI/` e `client/parallax-unity/Assets/_Project/Shaders/Vision/`
**Referências de Lore:** `audio-e-feedback.md`, `a-batida.md`, `fio-01-sinal-de-proteu.md`, `candeia.md`, `engenheiro-chefe-brandt.md`

---

## 1. A Nota do Piso (40 a 60 Hz) e o Pulso da Fonte P (26h56)
- **Ferramenta:** **Vital Synth** (100% gratuito) + **Audacity** (gratuito).
- **Formato de Saída:** `loop_nota_do_piso_50hz.ogg` e `sinal_fonte_p_proteu.ogg`.
- **Receita de Síntese no Vital Synth (Zero IA, 100% Autoria Humana):**
  1. **Nota do Piso (`loop_nota_do_piso_50hz.ogg`):** Oscilador 1 em onda senoidal pura + harmônico suave de onda triangular em `50 Hz` (`G1` grave), com modulação LFO ultralenta (`0.08 Hz`) no filtro Low-Pass para simular a respiração da fornalha geotérmica da Cidade-Radiador. Na Unity, o `pitch` do AudioSource varia (ou use o `FloorNoteSynth` procedural) de `0.8` (40 Hz nos setores frios/Apagão) a `1.2` (60 Hz no Fosso de Hefesto).
  2. **Portadora da Fonte P (`sinal_fonte_p_proteu.ogg`):** Tom telemétrico curto em `1.420 Hz` modulado em FSK (Frequency Shift Keying) com chiado de magnetosfera ao fundo, durando 2,4 segundos.

---

## 2. Efeitos Sonoros Industriais e "A Batida" nos Canos
- **Ferramenta:** **ElevenLabs Sound Effects** (no sprint comercial) + edição e normalização no **Audacity**.
- **Formato de Saída:** `sfx_batida_cano_curta.ogg`, `sfx_batida_cano_longa.ogg`, `sfx_valvula_pesada_girando.ogg`, `sfx_choque_termico_vidro.ogg`, `sfx_fuzil_trava_linha_viva.ogg`.
- **Prompts de Geração de SFX (ElevenLabs Sound Effects):**
  - **Martelada da Batida (`sfx_batida_cano_curta.ogg`):**
    ```text
    Single sharp heavy metallic hammer strike on a thick hollow pressurized steel pipe inside a cavernous underground boiler room, resonant metallic ring that travels through iron, clean transient for rhythmic morse code
    ```
  - **Volante de Válvula no Menu (`sfx_valvula_pesada_girando.ogg`):**
    ```text
    Heavy rusted cast-iron industrial valve wheel turning under extreme hydraulic pressure, metal groaning followed by a heavy mechanical lock clunk and a short hiss of high-pressure steam
    ```
  - **Choque Térmico a 38 K (`sfx_choque_termico_vidro.ogg`):**
    ```text
    Red-hot thick steel armor plate suddenly flash-frozen at -235 Celsius and shattering like thick brittle glass with a deep metallic crack and crystalline ice explosion
    ```
  - **Clique da Trava de Linha Viva (`sfx_fuzil_trava_linha_viva.ogg`):**
    ```text
    Sci-fi military rifle electronic safety solenoid locking shut with a distinct sharp metallic double-click and a low warning beep
    ```

---

## 3. Vozes de Rádio (Candeia, Brandt e as 3 Classes) + Filtro de Rádio Militar
- **Ferramenta:** **ElevenLabs (Plano Starter/Creator comercial)** → pós-processamento obrigatório no **Audacity**.
- **Design de Vozes (Configuração ElevenLabs):**
  - **[[candeia]] (Operadora da Cifra):** Voz feminina brasileira, 45 anos, timbre firme, seco, sem dramatização exagerada (`Stability: 70%`, `Clarity: 85%`).
  - **[[engenheiro-chefe-brandt]] (Chefe da Raid 1):** Voz masculina grave, 55 anos, cansada e pragmática, com leve ressonância de capacete fechado.
- **Macro do Audacity (Filtro de Rádio de Tritão):**
  1. **Filter Curve EQ:** Corte total abaixo de `300 Hz` e acima de `3.200 Hz` (banda de rádio militar).
  2. **Distortion (Hard Clipping suave):** `-12 dB` para dar aspereza de transmissor de Nervo subterrâneo.
  3. **Camada de Fundo:** Mixar em `-24 dB` um leve estalo de estática magnética de Netuno no início e no fim da transmissão (clique de PTT — *Push-To-Talk*).

---

## 4. Camadas Musicais Adaptativas (Drones Industriais e Combate 3D)
- **Ferramenta:** **Suno AI (Plano Pro Comercial)** no sprint concentrado → separação de *stems* (camadas de bumbo/percussão, baixo synth e ambiência) para mixagem dinâmica no **Audio Mixer da Unity** (snapshots).
- **Prompt 1 — Ambiência do Fosso de Hefesto e Menu (`bgm_fosso_ambient`):**
  ```text
  Dark industrial hard sci-fi ambient soundtrack, 90 BPM, deep sub-bass drone at 50 Hz, distant metallic anvil clangs echoing in a subterranean geothermal cavern, tense analog synthesizer swells, no vocals, atmospheric and oppressive like a living factory
  ```
- **Prompt 2 — Combate de Raid: Sangria de Hefesto (`bgm_raid_hefesto_combat`):**
  ```text
  Tactical industrial cyberpunk combat music, 128 BPM, heavy distorted mechanical percussion made of steam pistons and iron hammers, aggressive dark synth bassline, urgent rhythmic pulse for a third-person co-op raid boss fight, no vocals, high tension
  ```

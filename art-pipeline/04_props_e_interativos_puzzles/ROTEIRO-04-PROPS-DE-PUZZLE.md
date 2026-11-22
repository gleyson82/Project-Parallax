# Roteiro 04 — Props Mecânicos e Interativos de Puzzle (A Matriz de Paralaxe no Cenário)

**Destino de Exportação no Godot 4:** `client/parallax/04_props_e_interativos_puzzles/`
**Referências de Lore:** `matriz-de-paralaxe.md`, `chave-tripla.md`, `doutrina-do-fogo-minimo.md`, `pele-de-rede.md`, `fio-02-quarta-fechadura.md`

---

## Componente 4.1 — Válvula Industrial Therma-Tek com Número no Topo
Na `matriz-de-paralaxe.md` e no `encontro-1-os-tubos-falam.md`, uma válvula tem 3 leituras: esforço físico no volante (Goliath), número gravado no topo visível só das passarelas altas (Phantom) e cabos de trava lógica (Cipher).
- **Ferramenta:** **Blender 4.x via Blender MCP** + texturas PBR do **Material Maker**.
- **Formato de Saída:** `prop_valvula_mestra_thermatek.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, construa o prop 3D `prop_valvula_mestra_thermatek` em escala real (2,4 metros de altura):
  1. Corpo principal: Tubulação vertical grossa em ferro fundido escuro com flanges rebitados.
  2. Frente (Altura 1,2m - para o Goliath): Um volante hidráulico frontal de 1,1m de diâmetro pintado em vermelho-ferrugem (#C83214), separado como nó filho `Wheel_Hinge` para girar no eixo Z via HingeJoint3D no Godot.
  3. Topo (Altura 2,4m - para o Phantom): Uma placa metálica horizontal virada exclusivamente para cima (invisível da câmera do chão), onde o número da válvula (01 a 09) é aplicado via decalque/UV, com camada de fuligem nas laterais.
  4. Base lateral (para o Cipher): Uma caixa de junção blindada com 3 cabos emissivos violetas da Omni-Cripto.
  Exporte como `prop_valvula_mestra_thermatek.glb` para `client/parallax/04_props_e_interativos_puzzles/valvulas_e_portas/`.
  ```

---

## Componente 4.2 — Porta de Explosão de Chumbo de 10 Toneladas e a "Chave Tripla" (+ 4º Receptáculo)
- **Ferramenta:** **Blender 4.x via Blender MCP**.
- **Formato de Saída:** `prop_porta_chave_tripla.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele a `prop_porta_chave_tripla` (4,5 metros de altura por 3,5 metros de largura, espessura de 0,8m de chumbo industrial):
  1. Folha da porta: Bloco monolítico chanfrado com barras de impacto inferiores onde o Goliath apoia os ombros (ou encaixa o Auxiliar Macaco).
  2. As 3 Fechaduras Corporativas incompatíveis na mesma moldura:
     - Esquerda (Therma-Tek): Trava de pressão e volante térmico laranja.
     - Topo a 4 metros de altura (Kinesys): Flange estrutural criogênico ciano acessível apenas pelo Phantom.
     - Direita (Omni-Cripto): Painel óptico de Override violeta para o Cipher.
  3. O Fio Solto 2 (`fio-02-quarta-fechadura.md`): Abaixo das três fechaduras, esculpa um **quarto receptáculo circular profundo, vazio, sem logotipo corporativo**, com um leve vestígio da Marca de Quatro Hastes no interior.
  Exporte como `prop_porta_chave_tripla.glb`.
  ```

---

## Componente 4.3 — "Linha Viva" (Duto Pressurizado com Faixa Amarela e Manômetro em 140%)
Na `doutrina-do-fogo-minimo.md` e em `manometro-da-estacao-14.md`, as armas das Esquadras de Contenção travam ao mirar perto de uma Linha Viva, e os manômetros denunciam o excesso de calor (`140%`).
- **Ferramenta:** **Blender 4.x via Blender MCP** + **Material Maker**.
- **Formato de Saída:** `prop_linha_viva_modular.glb` e `prop_manometro_140.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, crie um kit modular de tubulação pressurizada `prop_linha_viva_modular` (segmento reto de 4m, curva de 90 graus e junção em T):
  - O tubo de 0,9m de diâmetro deve ter aço escuro suado de condensação e uma **faixa diagonal dupla em amarelo-enxofre de alta visibilidade (#FFB300)** com o estêncil "LINHA VIVA - FOGO PROIBIDO".
  - Anexe o sub-prop `prop_manometro_140`: um mostrador analógico redondo com vidro rachado, ponteiro travado além da faixa vermelha no pino de 140% e um pedaço de fita isolante amarela colada sobre o número impresso.
  Exporte para `client/parallax/04_props_e_interativos_puzzles/linhas_vivas_e_relogios/`.
  ```

---

## Componente 4.4 — Mastro de Farol de Laser (Kinesys) e Terminal da Pele de Rede (Omni-Cripto)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `prop_farol_laser_kinesys.glb` e `prop_terminal_omnicripto.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele dois props da Pele de Rede (`pele-de-rede.md`):
  1. `prop_farol_laser_kinesys`: Torre treliçada brutalista de 12 metros de altura coberta de gelo nas arestas, com um cabeçote giratório no topo contendo um espelho parabólico e emissor de feixe laser vermelho/ciano desalinhado pelo tremor.
  2. `prop_terminal_omnicripto`: Console brutalista angular de 1,5m de altura com tela CRT inclinada, entrada física para o Cartucho de Override e cabo de Nervo em serpentina de titânio saindo pela base.
  Exporte para `client/parallax/04_props_e_interativos_puzzles/terminais_e_farois/`.
  ```

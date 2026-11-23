---
jogo: parallax
tipo: fundacao
status: cânone
camada: base
liga: ["[[indice]]", "[[contrato-mecanico]]", "[[blueprint-de-investimento]]", "[[quadro-de-valvulas]]", "[[sangria-de-hefesto]]", "[[meta-atingida]]", "[[0005-migracao-unity-3d]]"]
---

# Utility Belt — Arsenal Tecnológico e Pipeline com IA de PARALLAX

**Estúdio:** HTL Games
**Teto Orçamentário:** US\$ 2.000,00 (Desembolso total em 18 meses)
**Motor:** Unity 6000.3 LTS + URP, 3D real em terceira pessoa com primeira pessoa contextual (decisão [[0005-migracao-unity-3d]])
**Regra de Ouro Operacional:** Tudo o que puder ser gerado, automatizado ou prototipado por Agentes de IA será feito por Agentes de IA; tudo o que entrar na build final passará por curadoria, montagem e polimento humano dentro de ferramentas gratuitas.

---

## 1. Mapeamento: O que a Lore de Tritão exige que seja construído

Antes de escolher uma ferramenta, listamos exatamente o que o universo de [[nome-parallax]] nos obriga a colocar na tela e nos fones de ouvido do jogador:

| Categoria da Lore | Ativos Concretos Necessários | Desafio Técnico Principal |
| --- | --- | --- |
| **As 3 Classes e 3 Auxiliares** | [[goliath]] (exo pesado), [[phantom]] (traje leve com capa/cabos), [[cipher]] (manoplas/fios neurais), mais [[macaco]], [[pipa]] e [[cartucho-de-override]]. | Silhuetas inconfundíveis em terceira pessoa a qualquer ângulo e animações compartilháveis. |
| **Arsenal das 3 Corporações e Grau-Zero** | Armas de fogo/escória da [[therma-tek]], rifles criogênicos da [[kinesys]], feixes de íons/EMP da [[omni-cripto]], e as 3 Relíquias ([[coracao-de-hefesto]], [[lagrima-do-leviata]], [[codigo-fonte-omega]]). | Identidade visual por cor/shader corporativo e efeitos térmicos (vapor, gelo, arco voltaico). |
| **Inimigos e Chefes de Raid** | [[esquadras-de-contencao]] (humanos táticos), drones e autômatos deformados da [[diretriz-omega]], Mestre de Fornalha, [[engenheiro-chefe-brandt]] no Regulador e o Gestor-9 (amálgama de mechas). | Reaproveitamento de esqueletos (rigs) para não animar 20 inimigos do zero. |
| **5 Biomas e 3 Transportes** | [[krios-prime]] (neon roxo invertido), [[fosso-de-hefesto]] (industrial vermelho), [[estilhacos]] (gelo a -235 °C), [[agulha-do-zenite]] (vácuo), [[cemiterio-das-maquinas]] (escuridão total), [[expresso-maglev]], [[funiculares-de-fosso]] e [[rastejadores]]. | Arquitetura brutalista colossal, explorável em 3D e na vertical, que pareceria cara demais se modelada peça por peça à mão. |
| **Física Pesada e Termodinâmica** | Portas de chumbo de 10 t, volantes hidráulicos, cabos de tensão, plataformas que inclinam em terremotos, gêiseres e choque térmico (aquecer a vermelho e estilhaçar com gelo). | Física 3D estável sincronizada em rede para até 3 jogadores. |
| **Interface Assimétrica (HUD e Menu)** | [[quadro-de-valvulas]] em 3 camadas de paralaxe, interface cega de Override do Cipher, tremor vermelho de erro da Ômega, visão de pressão do Goliath e visão ótica do Phantom. | Pós-processamento distinto para cada jogador e UI diegética industrial no espaço 3D. |
| **Áudio, Vozes e "A Batida"** | Nota grave do piso (40–60 Hz), código rítmico da [[a-batida]] nos canos, voz de rádio de [[candeia]], voz de Brandt e som de fábrica viva. | Precisão rítmica para os puzzles sonoros e timbre de rádio sujo de estática magnética. |
| **Fragmentos, Textos e Localização** | Logs de terminais, etiquetas de [[cota-familiar]], placas de turno, manifesto das Arcas e tradução consistente em 4 idiomas (PT-BR, EN, ZH-CN, ES). | Manter a coerência estrita do glossário e nunca entregar a resposta mastigada. |

---

## 2. O Cérebro: Motor, IDEs e Pontes de Agentes (MCP)

Para que a IA não seja apenas um "chat onde copiamos código", usamos o padrão **MCP (Model Context Protocol)** e a linha de comando do motor. O agente abre o editor, recompila, lê o console, cria cenas e prefabs, roda testes e modela no Blender pelo terminal.

### Avaliação de Mercado (IDEs e Agentes de Código)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Cursor Pro (US\$ 20/mês) + Antigravity / Google AI Pro (US\$ 19,99/mês)** | **US\$ 0 nos meses 1 a 6 (já pagos pelo estúdio)**; US\$ 40/mês nos meses 7 a 18 (+ US\$ 20/mês de reserva para API) | Ambos já estão ativos e quitados para os primeiros 6 meses. Leem o repositório inteiro (código + Project Daedalus), editam múltiplos arquivos e conectam aos servidores MCP da Unity e do Blender. | Exigem disciplina de commits pequenos para evitar regressões. | **ESCOLHIDO (Espinha Dorsal)** |
| **GitHub Copilot Individual** | US\$ 10/mês | Bom autocomplete linha a linha. | Mais fraco em orquestração autônoma de cenas, prefabs e lore. | Descartado |
| **Modelos Locais (Ollama / Qwen-Coder)** | US\$ 0 | Custo zero e offline. | A GPU local (8 GB) e a janela de contexto menor limitam o cruzamento de 114 notas de lore com código de rede. | **APOIO GRATUITO** |

### O Pipeline de Engenharia
- **Motor:** **Unity 6000.3 LTS** (Unity Personal, gratuita até US\$ 200 mil de receita anual) com **URP 17.3**. Cenas, prefabs e ScriptableObjects são salvos em **texto YAML** (*Force Text*) e versionados com **Visible Meta Files**, o que permite ao agente ler, comparar e revisar cada mudança no Git.
- **Arquitetura de código:** C# em `client/parallax-unity/Assets/_Project/`, dividido em Assembly Definitions (`Parallax.Core`, `Networking`, `Gameplay`, `Rendering`, `UI`, `Editor`). Regras de jogo puras vivem em `Parallax.Core` e são cobertas por testes EditMode.
- **Ponte com o Motor:** **Unity CLI + `unity mcp`** (pacote oficial `com.unity.pipeline`, US\$ 0). O agente recompila, roda testes, entra em Play e consulta o editor aberto. O menu `Parallax/Bootstrap` reconstrói as configurações do projeto de forma idempotente.
- **Controle de Versão:** **Git + Git LFS** (Gratuito). Monorepo: o Project Daedalus e o cliente Unity no mesmo repositório.

---

## 3. Cenários e Level Design Brutalista (As Cidades-Radiador)

Modelar metrópoles colossais de aço escuro no Blender do zero levaria anos. A solução para o estilo brutalista de Tritão é a geometria de blocos sólida combinada com kits modulares e automação via IA.

### Avaliação de Mercado (Construção de Cenários)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **ProBuilder (Unity)** | **US\$ 0** (pacote oficial) | Blockout de paredes monolíticas, vigas, dutos e abismos verticais direto na cena, com colisão imediata e escala métrica real. Scriptável pelo agente via editor. | Menos indicado para escultura orgânica. | **ESCOLHIDO (Níveis Brutalistas)** |
| **Blender 5.x + Blender MCP** | **US\$ 0** (Open-source) | O servidor **Blender MCP** permite pedir ao agente: *"Crie uma fileira de 12 tubulações industriais com flanges e válvulas a cada 4 metros"*, e o agente executa via Python (`bpy`) dentro do Blender ao vivo. | Curva de aprendizado maior se usado manualmente sem scripts. | **ESCOLHIDO (Props, Kits e Dutos)** |
| **Kits Comprados em Loja** | US\$ 50 a US\$ 500 | Prontos para uso. | Estouram o orçamento e deixam o jogo com visual genérico de "asset flip". | Descartado |

### Fluxo de Importação (Cenários → Unity)
1. **Blockout e Arquitetura:** o nível (como a Estação 14 ou o Pátio das Válvulas da [[sangria-de-hefesto]]) é blocado em **ProBuilder** na própria cena, em metros: Goliath com 2,6 m e Phantom/Cipher com 1,8 m.
2. **Props Modulares:** válvulas, terminais da Omni-Cripto, trilhos do [[expresso-maglev]] e gaiolas dos [[funiculares-de-fosso]] são gerados no **Blender** (scripts do agente via **Blender MCP**) e exportados em **GLB** ou **FBX** para `Assets/_Project/Art/`. Malhas com sufixo `_col` viram colisores automaticamente.
3. **Entidades de Puzzle:** são prefabs com componentes (`HeavyDoor`, `LiveLine`, `GrapplePoint`, `OverrideTerminal`, `LockActuator`).

---

## 4. Personagens, Armas, Inimigos e Relíquias (Pipeline 3D Low-Poly)

Os personagens e inimigos usam **3D Low-Poly Estilizado** com iluminação dura, legível em terceira pessoa a qualquer ângulo e de perto, na primeira pessoa contextual.

### Avaliação de Mercado (Geração e Modelagem 3D com IA)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Tripo 3D (Pro) ou Meshy AI (Pro)** | **US\$ 20/mês** (usado apenas no sprint de 3 meses = US\$ 60) | Gera malhas 3D base a partir de imagem ou texto (ex.: carcaças de mechas do Setor Nulo, sucata, forma bruta do Regulador) com **licença comercial privada** no plano Pro. | A topologia bruta da IA tem triângulos demais e precisa de redução (decimate/retopo) antes de animar. | **ESCOLHIDO (Sprint de Malhas Base)** |
| **Hunyuan3D-2 mini (Local)** | **US\$ 0** (Open-weights) | Roda na GPU local de 8 GB; ótimo para dezenas de destroços para o [[cemiterio-das-maquinas]]. | Modelos maiores (TRELLIS, Hunyuan completo) pedem 16 GB+. | **ALTERNATIVA GRATUITA LOCAL** |
| **Blockbench** | **US\$ 0** (Open-source) | Low-poly industrial limpo (armas da [[therma-tek]], [[kinesys]] e [[omni-cripto]], drones, os Auxiliares [[macaco]], [[pipa]] e [[cartucho-de-override]]). Exporta GLB com animação. | Menos indicado para esculturas orgânicas complexas. | **ESCOLHIDO (Armas, Drones e Auxiliares)** |
| **Mixamo + Rigify (Blender) + Humanoid (Unity)** | **US\$ 0** | Biblioteca gratuita de animações bípedes redirecionadas pelo sistema Humanoid da Unity a um único esqueleto compartilhado. | Exige ajuste para o peso exagerado do Goliath. | **ESCOLHIDO (Rigging e Animação)** |
| **Animation Rigging (Unity)** | **US\$ 0** (pacote oficial) | IK de mãos e pés: o Goliath agarra a porta de 10 t e o volante de verdade; o Phantom alinha o corpo ao cabo. | — | **ESCOLHIDO (IK procedural)** |

### Como criar cada elemento da Lore gastando quase nada
- **As 3 Classes e Soldados do Sindicato:** usam **2 esqueletos (rigs) base apenas**: um **Bípede Padrão** (para [[phantom]], [[cipher]] e [[esquadras-de-contencao]]) e um **Bípede Pesado** (para [[goliath]], Mestre de Fornalha e o Regulador de [[engenheiro-chefe-brandt]]). Isso corta 80% do trabalho de animação.
- **O Chefe Gestor-9 (Raid 2):** na lore ([[encontro-3-ciclo-4113]]), o Gestor-9 é um amálgama de mechas e exoesqueletos soldados em um só vulto. Técnica de baixo orçamento: geramos carcaças com IA (Tripo/Hunyuan), aplicamos *Decimate* no Blender e "soldamos" 8 carcaças ao redor do mesmo rig Bípede Pesado, com partes soltas animadas por script procedural na Unity.
- **Armas e Relíquias de Grau-Zero:** low-poly afiado no **Blockbench** ou **Blender**. A diferença entre uma arma comum e a [[lagrima-do-leviata]] (um único cristal de nitrogênio que não projeta sombra) não custa polígonos extras: o renderer tem *Cast Shadows* desligado e um shader de refração cristalina escrito pelo agente.

---

## 5. Imagens, Texturas PBR, Shaders e a Interface Assimétrica

Em Tritão, a identidade visual vem da luz, da ferrugem, do gelo e das 3 camadas de visão das classes.

### Avaliação de Mercado (Imagem, Textura e UI)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Material Maker** | **US\$ 0** (Open-source) | O "Substance Designer gratuito". Gera texturas PBR (aço escuro, ferrugem sulfúrica do Fosso, gelo de nitrogênio, grade metálica). Os mapas de roughness viram smoothness do URP com `convert_roughness_to_smoothness.py`. | Focado em materiais procedurais, não em ilustração de personagens. | **ESCOLHIDO (Texturas 3D)** |
| **ComfyUI + SDXL / Flux quantizado (Local) ou Midjourney (Sprint)** | **US\$ 0** (Local) ou **US\$ 30** (1 mês no sprint) | Conceitos visuais, ícones de talentos, retratos estilizados de rádio ([[candeia]], Brandt) e bases para pintura por cima (*paintover*). | Nunca deve entrar cru no jogo para não violar a proteção de copyright nem parecer genérico. | **ESCOLHIDO (Conceito e Base de Ícones)** |
| **Krita + Inkscape** | **US\$ 0** (Open-source) | Krita para *paintover* humano e pintura de texturas; Inkscape para a iconografia vetorial das corporações (Therma-Tek, Kinesys, Omni-Cripto, a [[marca-de-quatro-hastes]] e os volantes do [[quadro-de-valvulas]]), importada como SVG na Unity. | — | **ESCOLHIDO (UI Vetorial e Paintover)** |
| **Aseprite** | **US\$ 19,99** (Perpétuo) ou **US\$ 0** (Compilado do código-fonte) | Sprites de interface, cursores, fontes bitmap de terminais e animações frame a frame dos painéis de invasão do Cipher. | — | **ESCOLHIDO (Terminais e Sub-UI)** |

### Segredo Técnico: Os 3 Olhares via Renderers (Custo Zero de Arte)
Em vez de desenhar três versões de cada cenário para a [[matriz-de-paralaxe]], cada classe tem o próprio **Universal Renderer** na URP, com um passe de tela cheia (*Full Screen Pass Renderer Feature*) escrito em HLSL pelo agente. A câmera local de cada jogador usa só o renderer da própria classe, então ninguém vê o Olhar do outro:
1. **Olhar do Goliath (`Parallax/Vision/GoliathThermal`):** LUT térmico vermelho/laranja, ondas visuais nos pontos de vibração do piso no ritmo da Batida e vinheta do capacete pesado.
2. **Olhar do Phantom (`Parallax/Vision/PhantomOptic`):** contornos em ciano nos pontos altos, cabos, rotas verticais e placas numéricas no teto.
3. **Olhar do Cipher (`Parallax/Vision/CipherNet`):** desatura o mundo físico, realça a Pele de Rede e, durante o Override, troca o mundo pela interface isolada do terminal; aplica o "tremor vermelho" quando a Contagem de Vazamento do [[codigo-fonte-omega]] chega a 3.

O mesmo esquema serve à geada do [[quadro-de-valvulas]], que avança pelas bordas quando o jogador fica parado.

---

## 6. Física, Engenharia Pesada e Termodinâmica

A jogabilidade de PARALLAX exige que uma porta de chumbo pareça pesar 10 toneladas e que um cabo de tensão segure uma plataforma sobre magma.

### Avaliação de Mercado (Física e Simulação)

| Ferramenta / Módulo | Custo | Aplicação na Lore | Veredito |
| --- | --- | --- | --- |
| **PhysX (nativo na Unity)** | **US\$ 0** | Controla o peso do [[goliath]], colisões de carcaças no Setor Nulo e plataformas que inclinam em [[encontro-2-mare-baixa]]. Simulada só no anfitrião e replicada aos outros. | **ESCOLHIDO (Física 3D Padrão)** |
| **Juntas (`ConfigurableJoint`, `HingeJoint`) e `CharacterController`** | **US\$ 0** | Volantes de válvula com resistência, portas de pressão que descem se o Goliath soltar, e os cabos de tensão que o [[phantom]] prende nas paredes. | **ESCOLHIDO (Engenharia Pesada)** |
| **NavMesh (AI Navigation)** | **US\$ 0** (pacote oficial) | Drones e autômatos da [[diretriz-omega]] e as máquinas que o [[cipher]] converte em aliadas. | **ESCOLHIDO (Autômatos)** |
| **Sistema de Termodinâmica via Script (Agente de IA)** | **US\$ 0** | Um modelo puro (`ThermalModel`) e um componente de rede (`ThermalNode`) anexado a jogadores, armas, inimigos e salas. Calcula troca de calor, congelamento a -235 °C, superaquecimento das armas [[therma-tek]], decaimento de 2% das [[celula-termica]] e o **Choque Térmico** ([[encontro-3-o-mestre-de-fornalha]]), avançando no tick de rede. | **ESCOLHIDO (Termodinâmica)** |

---

## 7. Áudio, Música, Falas e "A Batida"

O som em PARALLAX não é enfeite: ele é mecânica de puzzle ([[a-batida]] nos canos, os sensores acústicos da Ômega em [[encontro-1-descida-ao-setor-nulo]] e a portadora de 26h56 do [[fio-01-sinal-de-proteu]]).

### Avaliação de Mercado (Voz, Efeitos Sonoros e Música com IA)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **ElevenLabs (Starter / Creator)** | **US\$ 6 a US\$ 22/mês** (usado no sprint) | Licença comercial incluída nos planos pagos. Gera as falas de [[candeia]] e [[engenheiro-chefe-brandt]], além de efeitos sonoros brutos (explosão de vapor, metal rangendo, estalo de gelo). | Vozes de IA puras podem soar limpas demais se não forem pós-processadas. | **ESCOLHIDO (Vozes e SFX Brutos)** |
| **Suno AI (Pro) ou Udio (Standard)** | **US\$ 10/mês** (usado no sprint) | Direitos comerciais sobre faixas geradas durante a assinatura paga. Drones industriais sombrios, percussão metálica e ambiências tensas para as Raids. | Precisa ser separada em *stems* para reagir dinamicamente ao combate. | **ESCOLHIDO (Camadas Musicais Base)** |
| **Reaper / Audacity + Vital Synth** | **US\$ 0** (Vital Free / Audacity) | O **Vital** cria o pulso hexadecimal da **Fonte P**; o Audacity aplica filtro de rádio militar (band-pass + distorção + estática magnética) nas vozes. | — | **ESCOLHIDO (Design Sonoro e Rádio)** |
| **Unity Audio Mixer (FMOD Studio Indie opcional)** | **US\$ 0** | Mixagem reativa por *snapshots*: quando o Goliath entra em "modo brasa" ou o Phantom coloca uma Surdina, o grupo abafa as frequências altas em tempo real. O FMOD Indie (gratuito abaixo de US\$ 200 mil) fica como opção se a música adaptativa crescer. | — | **ESCOLHIDO (Áudio Adaptativo)** |

### Como programar "A Batida" com precisão matemática
Para os puzzles da [[a-batida]] ([[encontro-1-os-tubos-falam]] e [[segredo-primeiro-exo]]), **não usamos música gerada por IA com tempo frouxo**. O `BeatSequencer` agenda cada martelada com `AudioSource.PlayScheduled` sobre o relógio de áudio (`AudioSettings.dspTime`), com antecedência fixa. O metrônomo visual recebe o mesmo horário exato, e a cadência que o Goliath escuta fica alinhada na casa do milissegundo, sem a oscilação de um tick de física. A **Nota do Piso (40–60 Hz)** é sintetizada em tempo real (`FloorNoteSynth`) e muda de altura com o calor do setor, sem arquivo de áudio.

---

## 8. Textos, Fragmentos In-World, Diálogos e Localização

Toda a história é contada por arqueologia: logs curtos, etiquetas, manifestos e falas de rádio de duas linhas.

### Avaliação de Mercado (Narrativa e Tradução)

| Ferramenta | Custo | Função no Projeto | Veredito |
| --- | --- | --- | --- |
| **Obsidian + Project Daedalus (Git)** | **US\$ 0** | Banco de dados vivo da lore (114 notas canônicas). Fonte única da verdade para os agentes de IA. | **ESCOLHIDO (Cérebro Narrativo)** |
| **Unity Localization** | **US\$ 0** (pacote oficial) | String Tables para PT-BR, EN, ZH-CN e ES; briefings de [[candeia]], leituras das classes no Quadro e fragmentos in-world. | **ESCOLHIDO (Textos e Localização)** |
| **Rádio e terminais com ScriptableObjects** | **US\$ 0** | Falas curtas de rádio e logs de terminal como dados versionados, sem sistema de diálogo pesado. | **ESCOLHIDO (Sistema de Diálogo)** |
| **Pipeline Python + LLM API (Tradução Contextual)** | Incluído no envelope de IA | Um script Python lê o CSV de textos do jogo junto com `lore/glossario/` do Daedalus e traduz para inglês, chinês simplificado e espanhol, mantendo os termos sagrados intocados. | **ESCOLHIDO (Localização 4 Idiomas)** |

### Fluxo Automático: Do Daedalus para dentro do Jogo
O script `studio/scripts/sync_daedalus.py` lê `jogos/parallax/fragmentos/` (separada em `goliath/`, `phantom/` e `cipher/`), extrai o frontmatter (`olhar`, `status`, `liga`) e o texto do fragmento, bloqueia qualquer nota restrita ou ligada a uma verdade, e gera dentro da Unity o arquivo `Assets/_Project/Data/Lore/Generated/fragments.json` e o CSV de tradução `Data/Localization/Source/daedalus_strings.csv`. No editor, o menu `Parallax/Daedalus/Import Generated JSON` cria um `LoreFragment` por nota e preenche a tabela `Lore` nos quatro idiomas. Quando criamos um fragmento novo no Obsidian, ele já nasce pronto para aparecer no terminal ou na parede certa do jogo.

---

## 9. Resumo Financeiro do Utility Belt (Dentro dos US\$ 2.000)

Esta tabela consolida todas as ferramentas escolhidas e prova como o arsenal tecnológico completo cabe exatamente no orçamento de dois mil dólares definido no [[blueprint-de-investimento]]:

| Categoria | Ferramenta Principal (Custo Zero / Já Pago) | Ferramenta Paga / IA (Novo Desembolso) | Desembolso Total (18 meses) |
| --- | --- | --- | --- |
| **Publicação na Loja** | Steamworks SDK / SteamPipe | Taxa **Steam Direct** (reembolsável aos US\$ 1.000) | **US\$ 100,00** |
| **Motor, Rede e Física** | **Unity 6 LTS (Personal)**, **URP**, **PhysX**, **Netcode for GameObjects**, **Steamworks.NET**, **Steam Datagram Relay**, **Unity CLI + MCP** | — | **US\$ 0,00** |
| **Agentes de Código, Arquitetura e Texto** | **Cursor Pro + Antigravity (Google AI Pro) já quitados nos meses 1 a 6** (economia de US\$ 240), Ollama local, scripts Python | **Renovação Cursor Pro (US\$ 20) + Antigravity / Google AI Pro (US\$ 20) nos meses 7 a 18** (US\$ 480) + **Cota de API / Tokens extras** (US\$ 240) | **US\$ 720,00** |
| **Cenários e Level Design** | **ProBuilder**, **Blender 5.x**, **Blender MCP** | — | **US\$ 0,00** |
| **Personagens, Armas e Inimigos 3D** | **Blockbench**, **Blender + Rigify**, **Mixamo**, **Animation Rigging**, **Hunyuan3D mini (local)** | **Tripo 3D Pro ou Meshy Pro** (Sprint de 3 meses × US\$ 20) | **US\$ 60,00** |
| **Imagens, Texturas, Shaders e UI** | **Material Maker**, **Krita**, **Inkscape**, **ComfyUI (local)** | **Aseprite** (US\$ 20 perpétuo) + **Sprint de Imagem Comercial** (US\$ 120) + **Arte Humana da Cápsula Steam e Key Art** (US\$ 320) | **US\$ 460,00** |
| **Áudio, Vozes e Música** | **Vital Synth**, **Audacity**, **Unity Audio Mixer / FMOD Indie** | **ElevenLabs + Suno Pro** (Sprint concentrado de 5 meses = US\$ 140) | **US\$ 140,00** |
| **Lore, Diálogos e Web ARG** | **Obsidian**, **Git**, **Unity Localization**, **GitHub/Cloudflare Pages** | **Domínio Web (3 anos)** para o ARG do [[fio-01-sinal-de-proteu]] | **US\$ 40,00** |
| **Reserva de Contingência (24%)** | — | Folga ampliada pela economia dos 6 meses iniciais de IDE/IA já quitados (câmbio, IOF, taxas ou créditos extras na reta final) | **US\$ 480,00** |
| **TOTAL DO PROJETO** | | | **US\$ 2.000,00** |

**Licença do motor.** A Unity Personal é gratuita enquanto a receita anual ficar abaixo de US\$ 200 mil. Se o jogo passar desse teto, a licença Unity Pro passa a ser paga com a receita do próprio jogo, e não com o orçamento de produção.

---

## 10. O Pipeline de Importação Fim a Fim (Checklist de Produção)

Para garantir que nenhuma ferramenta vire gargalo e que tudo entre limpo na Unity:

1. **Lore e Fragmentos:** escritos no **Obsidian (Project Daedalus)** → exportados por `sync_daedalus.py` → importados como `LoreFragment` e String Tables em `Assets/_Project/Data/` e `Settings/Localization/`.
2. **Arquitetura dos Biomas:** blocada em **ProBuilder** na cena → detalhada com kits do **Blender** → salva em `Assets/_Project/Scenes/Missions/`.
3. **Props, Classes, Armas e Inimigos:** gerados ou modelados via **Tripo/Blockbench/Blender MCP** → limpos e animados no **Blender** → exportados em **GLB/FBX** com texturas do **Material Maker** para `Assets/_Project/Art/`.
4. **Visão Assimétrica e Efeitos:** escritos pelo **Agente de Código** em **HLSL/Shader Graph** → registrados nos renderers de Goliath, Phantom e Cipher (`Settings/Rendering/Parallax_URP`).
5. **Vozes, Batida e Som:** gerados no sprint **ElevenLabs/Suno/Vital** → filtrados como rádio industrial no **Audacity** → exportados em **`.ogg` (Vorbis)** para `Assets/_Project/Audio/`.
6. **Lógica, Rede e Build:** programados com **Agentes + Unity CLI/MCP** → testados com o **Multiplayer Play Mode** (até 3 jogadores no mesmo editor) e depois via **Steam Datagram Relay** → empacotados em IL2CPP e enviados pelo **SteamPipe (`steamcmd`)** direto para a Steam.

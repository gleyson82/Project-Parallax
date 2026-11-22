---
jogo: parallax
tipo: fundacao
status: cânone
camada: base
liga: ["[[indice]]", "[[contrato-mecanico]]", "[[blueprint-de-investimento]]", "[[quadro-de-valvulas]]", "[[sangria-de-hefesto]]", "[[meta-atingida]]"]
---

# Utility Belt — Arsenal Tecnológico e Pipeline com IA de PARALLAX

**Estúdio:** HTL Games
**Teto Orçamentário:** US\$ 2.000,00 (Desembolso total em 18 meses)
**Regra de Ouro Operacional:** Tudo o que puder ser gerado, automatizado ou prototipado por Agentes de IA será feito por Agentes de IA; tudo o que entrar na build final passará por curadoria, montagem e polimento humano dentro de ferramentas gratuitas e de código aberto.

---

## 1. Mapeamento: O que a Lore de Tritão exige que seja construído

Antes de escolher uma ferramenta, listamos exatamente o que o universo de [[nome-parallax]] nos obriga a colocar na tela e nos fones de ouvido do jogador:

| Categoria da Lore | Ativos Concretos Necessários | Desafio Técnico Principal |
| --- | --- | --- |
| **As 3 Classes e 3 Auxiliares** | [[goliath]] (exo pesado), [[phantom]] (traje leve com capa/cabos), [[cipher]] (manoplas/fios neurais), mais [[macaco]], [[pipa]] e [[cartucho-de-override]]. | Silhuetas inconfundíveis em câmera lateral 2.5D e animações compartilháveis. |
| **Arsenal das 3 Corporações e Grau-Zero** | Armas de fogo/escória da [[therma-tek]], rifles criogênicos da [[kinesys]], feixes de íons/EMP da [[omni-cripto]], e as 3 Relíquias ([[coracao-de-hefesto]], [[lagrima-do-leviata]], [[codigo-fonte-omega]]). | Identidade visual por cor/shader corporativo e efeitos térmicos (vapor, gelo, arco voltaico). |
| **Inimigos e Chefes de Raid** | [[esquadras-de-contencao]] (humanos táticos), drones e autômatos deformados da [[diretriz-omega]], Mestre de Fornalha, [[engenheiro-chefe-brandt]] no Regulador e o Gestor-9 (amálgama de mechas). | Reaproveitamento de esqueletos (rigs) para não animar 20 inimigos do zero. |
| **5 Biomas e 3 Transportes** | [[krios-prime]] (neon roxo invertido), [[fosso-de-hefesto]] (industrial vermelho), [[estilhacos]] (gelo a -235 °C), [[agulha-do-zenite]] (vácuo), [[cemiterio-das-maquinas]] (escuridão total), [[expresso-maglev]], [[funiculares-de-fosso]] e [[rastejadores]]. | Arquitetura brutalista colossal que pareceria cara demais se modelada peça por peça à mão. |
| **Física Pesada e Termodinâmica** | Portas de chumbo de 10 t, volantes hidráulicos, cabos de tensão, plataformas que inclinam em terremotos, gêiseres e choque térmico (aquecer a vermelho e estilhaçar com gelo). | Física 3D estável no eixo 2.5D sincronizada em rede para até 3 jogadores. |
| **Interface Assimétrica (HUD e Menu)** | [[quadro-de-valvulas]] em 3 camadas de paralaxe, interface cega de Override do Cipher, tremor vermelho de erro da Ômega, visão de pressão do Goliath e visão ótica do Phantom. | Shaders de pós-processamento distintos para cada jogador e UI diegética industrial. |
| **Áudio, Vozes e "A Batida"** | Nota grave do piso (40–60 Hz), código rítmico da [[a-batida]] nos canos, voz de rádio de [[candeia]], voz de Brandt e som de fábrica viva. | Precisão rítmica para os puzzles sonoros e timbre de rádio sujo de estática magnética. |
| **Fragmentos, Textos e Localização** | Logs de terminais, etiquetas de [[cota-familiar]], placas de turno, manifesto das Arcas e tradução consistente em 4 idiomas (PT-BR, EN, ZH-CN, ES). | Manter a coerência estrita do glossário e nunca entregar a resposta mastigada. |

---

## 2. O Cérebro: Motor, IDEs e Pontes de Agentes (MCP)

Para que a IA não seja apenas um "chat onde copiamos código", usamos o padrão **MCP (Model Context Protocol)**. Isso permite que o agente de IA clique, crie cenas, rode testes e modele dentro do motor e do Blender diretamente pelo terminal.

### Avaliação de Mercado (IDEs e Agentes de Código)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Cursor Pro (US\$ 20/mês) + Antigravity / Google AI Pro (US\$ 19,99/mês)** | **US\$ 0 nos meses 1 a 6 (já pagos pelo estúdio)**; US\$ 40/mês nos meses 7 a 18 (+ US\$ 20/mês de reserva para API) | Ambos já estão ativos e quitados para os primeiros 6 meses. Leem o repositório inteiro (código + Project Daedalus), editam múltiplos arquivos e conectam a servidores MCP do Godot e do Blender. | Exigem disciplina de commits pequenos para evitar regressões. | **ESCOLHIDO (Espinha Dorsal)** |
| **GitHub Copilot Individual** | US\$ 10/mês | Bom autocomplete linha a linha. | Mais fraco em orquestração autônoma de múltiplos arquivos de cena (`.tscn`) e lore. | Descartado |
| **Modelos Locais (Ollama / Qwen-Coder)** | US\$ 0 | Custo zero e offline. | Exigem GPU cara e têm janela de contexto menor para cruzar 114 notas de lore com código de rede. | **APOIO GRATUITO** |

### O Pipeline de Engenharia e Importação Direta
- **Motor:** **Godot 4.4+** (Gratuito, MIT). Seus arquivos de cena (`.tscn`), recursos (`.tres`) e scripts (`.gd`) são **texto puro estruturado**. Um agente de IA gera uma cena inteira com colisores, luzes e scripts escrevendo um arquivo `.tscn`, sem que o desenvolvedor precise montar nós no mouse.
- **Ponte com o Motor:** **Godot MCP** (Open-source, US\$ 0). Conecta o agente de IA ao editor do Godot 4 para criar árvore de nós, inspecionar erros de console em tempo real, rodar 2 ou 3 instâncias locais do jogo e testar o multiplayer automaticamente.
- **Controle de Versão:** **Git + Git LFS** (Gratuito). Repositório unificado ou submódulo conectando o `Project-Daedalus` à pasta `data/lore/` do jogo.

---

## 3. Cenários e Level Design Brutalista (As Cidades-Radiador)

Modelar metrópoles colossais de aço escuro no Blender do zero levaria anos. A solução para o estilo brutalista de Tritão é a geometria de blocos sólida (CSG / Brush-based) combinada com automação via IA.

### Avaliação de Mercado (Construção de Cenários)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **TrenchBroom + FuncGodot** | **US\$ 0** (Open-source) | O melhor editor do mundo para arquitetura brutalista (paredes monolíticas, vigas, dutos, abismos verticais). Exporta `.map` que o plugin **FuncGodot** converte automaticamente em malhas 3D e colisores perfeitos no Godot 4. | Focado em geometria angular/industrial (não faz terreno orgânico suave). | **ESCOLHIDO (Níveis Brutalistas)** |
| **Blender 4.x + Blender MCP** | **US\$ 0** (Open-source) | O servidor **Blender MCP** permite pedir ao agente de IA: *"Crie uma fileira de 12 tubulações industriais com flanges e válvulas a cada 4 metros"*, e o agente executa via Python (`bpy`) dentro do Blender ao vivo. | Curva de aprendizado maior se usado manualmente sem scripts. | **ESCOLHIDO (Props e Dutos)** |
| **Unreal / Kits Comprados em Loja** | US\$ 50 a US\$ 500 | Prontos para uso. | Estouram o orçamento, pesam gigabytes e deixam o jogo com visual genérico de "asset flip". | Descartado |

### Fluxo de Importação (Cenários → Godot 4)
1. **Blockout e Arquitetura:** O nível (como a Estação 14 ou o Pátio das Válvulas da [[sangria-de-hefesto]]) é desenhado em horas no **TrenchBroom**. O arquivo `.map` é salvo dentro da pasta do Godot e o **FuncGodot** gera a geometria, as camadas de colisão e as entidades de puzzle automaticamente.
2. **Props Modulares:** Válvulas, terminais da Omni-Cripto, trilhos do [[expresso-maglev]] e gaiolas dos [[funiculares-de-fosso]] são gerados no **Blender** (com scripts Python escritos pelo agente via **Blender MCP**) e exportados em **`.glb` (glTF 2.0 binário)** diretamente para a pasta `res://assets/props/`, onde o Godot 4 os importa instantaneamente.

---

## 4. Personagens, Armas, Inimigos e Relíquias (Pipeline 3D Low-Poly)

Para manter a câmera 2.5D fluida (estilo *Metal Slug* moderno em 3D ortográfico), os personagens e inimigos usam **3D Low-Poly Estilizado** com shaders de contorno e iluminação dura.

### Avaliação de Mercado (Geração e Modelagem 3D com IA)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Tripo 3D (Pro) ou Meshy AI (Pro)** | **US\$ 20/mês** (usado apenas no sprint de 2 meses = US\$ 40) | Gera malhas 3D base a partir de imagem ou texto (ex.: carcaças de mechas do Setor Nulo, sucata, forma bruta do Regulador) com **licença comercial privada** no plano Pro. | A topologia bruta da IA tem triângulos demais e precisa de redução (decimate/retopo) antes de animar. | **ESCOLHIDO (Sprint de Malhas Base)** |
| **Hunyuan3D-2 / Microsoft TRELLIS (Local)** | **US\$ 0** (Open-weights) | Roda localmente na GPU sem pagar mensalidade; ótimo para gerar dezenas de destroços para o [[cemiterio-das-maquinas]]. | Exige placa de vídeo local com boa VRAM (12 GB+ recomendados). | **ALTERNATIVA GRATUITA LOCAL** |
| **Blockbench** | **US\$ 0** (Open-source) | Perfeito para modelar low-poly industrial limpo (armas da [[therma-tek]], [[kinesys]] e [[omni-cripto]], drones, os Auxiliares [[macaco]], [[pipa]] e [[cartucho-de-override]]). Exporta `.glb` com animação pronta para o Godot. | Menos indicado para esculturas orgânicas complexas. | **ESCOLHIDO (Armas, Drones e Auxiliares)** |
| **Mixamo + Rigify (Blender)** | **US\$ 0** | Biblioteca gratuita de animações bípedes (correr, atirar, agachar, cair, carregar peso) que podem ser aplicadas a um único esqueleto humanoide compartilhado. | Exige ajuste para o peso exagerado do Goliath. | **ESCOLHIDO (Rigging e Animação)** |

### Como criar cada elemento da Lore gastando quase nada
- **As 3 Classes e Soldados do Sindicato:** Usam **2 esqueletos (rigs) base apenas** no Blender: um rig **Bípede Padrão** (para [[phantom]], [[cipher]] e [[esquadras-de-contencao]]) e um rig **Bípede Pesado** (para [[goliath]], Mestre de Fornalha e o Regulador de [[engenheiro-chefe-brandt]]). Isso corta 80% do trabalho de animação.
- **O Chefe Gestor-9 (Raid 2):** Na lore ([[encontro-3-ciclo-4113]]), o Gestor-9 é um amálgama de mechas e exoesqueletos soldados em um só vulto. Técnica de baixo orçamento: geramos carcaças com IA (Tripo/Hunyuan), aplicamos o modificador *Decimate* no Blender e "soldamos" 8 carcaças ao redor do mesmo rig Bípede Pesado, animando partes soltas via script procedural no Godot.
- **Armas e Relíquias de Grau-Zero:** Modeladas em low-poly afiado no **Blockbench** ou **Blender**. A diferença entre uma arma comum e [[lagrima-do-leviata]] (feita de um único cristal de nitrogênio que não projeta sombra) não custa polígonos extras: é feita desativando a flag `cast_shadow` no Godot e aplicando um **Shader de Refração Cristalina** escrito pelo agente de IA.

---

## 5. Imagens, Texturas PBR, Shaders e a Interface Assimétrica

Em Tritão, a identidade visual vem da luz, da ferrugem, do gelo e das 3 camadas de visão das classes.

### Avaliação de Mercado (Imagem, Textura e UI)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **Material Maker** | **US\$ 0** (Open-source, feito em Godot) | O "Substance Designer gratuito". Gera texturas PBR perfeitas e infinitas (aço escuro, ferrugem sulfúrica do Fosso, gelo de nitrogênio, grade metálica) baseadas em nós e exporta direto no formato do Godot 4. | Focado em materiais procedurais, não em ilustração de personagens. | **ESCOLHIDO (Texturas 3D)** |
| **ComfyUI + Flux / SDXL (Local) ou Midjourney (Sprint)** | **US\$ 0** (Local) ou **US\$ 30** (1 mês no sprint) | Gera conceitos visuais, ícones de talentos, retratos estilizados de rádio ([[candeia]], Brandt) e bases para pintura por cima (*paintover*). | Nunca deve entrar cru no jogo para não violar a proteção de copyright nem parecer genérico. | **ESCOLHIDO (Conceito e Base de Ícones)** |
| **Krita + Inkscape** | **US\$ 0** (Open-source) | Krita para *paintover* humano e pintura de texturas; Inkscape para criar toda a iconografia vetorial das corporações (Therma-Tek, Kinesys, Omni-Cripto, a [[marca-de-quatro-hastes]] e os volantes do [[quadro-de-valvulas]]). | — | **ESCOLHIDO (UI Vetorial e Paintover)** |
| **Aseprite** | **US\$ 19,99** (Perpétuo) ou **US\$ 0** (Compilado do código-fonte) | Imbatível para sprites de interface, cursores, fontes bitmap de terminais e animações frame a frame dos painéis de invasão do Cipher. | — | **ESCOLHIDO (Terminais e Sub-UI)** |

### Segredo Técnico: Os 3 Olhares via Shaders (Custo Zero de Arte)
Em vez de desenhar três versões de cada cenário para a [[matriz-de-paralaxe]], o agente de IA escreve **3 Shaders de Pós-Processamento (`.gdshader`)** no Godot 4:
1. **Olhar do Goliath (`goliath_thermal.gdshader`):** Destaca em vermelho/laranja objetos do grupo `thermal_hazard` e pulsa ondas visuais nos pontos de vibração do piso.
2. **Olhar do Phantom (`phantom_optic.gdshader`):** Realça contornos em ciano de pontos altos (`grapple_point`), rotas verticais e placas numéricas no teto.
3. **Olhar do Cipher (`cipher_net.gdshader`):** Desatura o mundo físico, desenha linhas luminosas conectando terminais às travas e, durante o Override, substitui a câmera 3D pela cena 2D isolada do terminal (e aplica o shader de "tremor vermelho" quando a Contagem de Vazamento do [[codigo-fonte-omega]] chega a 3).

---

## 6. Física, Engenharia Pesada e Termodinâmica

A jogabilidade de PARALLAX exige que uma porta de chumbo pareça pesar 10 toneladas e que um cabo de tensão segure uma plataforma sobre magma.

### Avaliação de Mercado (Física e Simulação)

| Ferramenta / Módulo | Custo | Aplicação na Lore | Veredito |
| --- | --- | --- | --- |
| **Jolt Physics (Nativo no Godot 4.4+)** | **US\$ 0** | Motor físico industrial de altíssima estabilidade. Controla o peso do [[goliath]], colisões de carcaças no Setor Nulo e plataformas que inclinam em [[encontro-2-mare-baixa]]. | **ESCOLHIDO (Física 3D Padrão)** |
| **Joints 3D do Godot (`HingeJoint3D`, `PinJoint3D`, `Generic6DOFJoint3D`)** | **US\$ 0** | Simulam volantes de válvulas com resistência mecânica, portas de pressão que descem se o Goliath soltar e os cabos de tensão que o [[phantom]] prende nas paredes. | **ESCOLHIDO (Engenharia Pesada)** |
| **Sistema de Termodinâmica via Script (Agente de IA)** | **US\$ 0** | Um componente `ThermalNode.gd` escrito pela IA e anexado a jogadores, armas, inimigos e salas. Calcula troca de calor, congelamento a -235 °C, superaquecimento das armas [[therma-tek]], decaimento de 2% das [[celula-termica]] e o **Choque Térmico** ([[encontro-3-o-mestre-de-fornalha]]). | **ESCOLHIDO (Termodinâmica)** |

---

## 7. Áudio, Música, Falas e "A Batida"

O som em PARALLAX não é enfeite: ele é mecânica de puzzle ([[a-batida]] nos canos, os sensores acústicos da Ômega em [[encontro-1-descida-ao-setor-nulo]] e a portadora de 26h56 do [[fio-01-sinal-de-proteu]]).

### Avaliação de Mercado (Voz, Efeitos Sonoros e Música com IA)

| Ferramenta | Custo | Prós | Contras | Veredito |
| --- | --- | --- | --- | --- |
| **ElevenLabs (Starter / Creator)** | **US\$ 6 a US\$ 22/mês** (usado por 4 meses no sprint = ~US\$ 60) | Licença comercial incluída nos planos pagos. Gera as falas de [[candeia]] e [[engenheiro-chefe-brandt]], além de efeitos sonoros brutos (explosão de vapor, metal rangendo, estalo de gelo) via texto. | Vozes de IA puras podem soar limpas demais se não forem pós-processadas. | **ESCOLHIDO (Vozes e SFX Brutos)** |
| **Suno AI (Pro) ou Udio (Standard)** | **US\$ 10/mês** (usado por 4 meses no sprint = US\$ 40) | Direitos comerciais totais sobre faixas geradas durante a assinatura paga. Excelente para gerar drones industriais sombrios, percussão metálica e ambiências tensas para as Raids. | Precisa ser separada em *stems* (camadas) para reagir dinamicamente ao combate. | **ESCOLHIDO (Camadas Musicais Base)** |
| **Reaper / Audacity + Vital Synth** | **US\$ 0** (Vital Free / Audacity) | O sintetizador **Vital** cria do zero a **Nota do Piso (40–60 Hz)** e o pulso hexadecimal da **Fonte P** com custo zero e 100% de autoria humana. O Audacity aplica filtro de rádio militar (band-pass + distorção + estática magnética) nas vozes da ElevenLabs. | — | **ESCOLHIDO (Design Sonoro e Rádio)** |
| **Godot 4 Audio Buses / FMOD Studio Indie** | **US\$ 0** (Gratuito até US\$ 200k/ano) | O sistema nativo do Godot 4 (ou FMOD Indie) controla a mixagem reativa: quando o Goliath entra em "modo brasa" ou o Phantom coloca uma Surdina, o barramento abafa as frequências altas em tempo real. | — | **ESCOLHIDO (Áudio Adaptativo)** |

### Como programar "A Batida" com precisão matemática
Para os puzzles da [[a-batida]] ([[encontro-1-os-tubos-falam]] e [[segredo-primeiro-exo]]), **não usamos música gerada por IA com tempo frouxo**. O agente de código escreve um `BeatSequencer.gd` no Godot 4 que dispara amostras de martelada metálica (`AudioStreamPlayer3D`) sincronizadas ao relógio interno de física (`_physics_process`). Assim, a cadência que o Goliath escuta no fone e o metrônomo visual na tela têm precisão de milissegundos.

---

## 8. Textos, Fragmentos In-World, Diálogos e Localização

Toda a história é contada por arqueologia: logs curtos, etiquetas, manifestos e falas de rádio de duas linhas.

### Avaliação de Mercado (Narrativa e Tradução)

| Ferramenta | Custo | Função no Projeto | Veredito |
| --- | --- | --- | --- |
| **Obsidian + Project Daedalus (Git)** | **US\$ 0** | Banco de dados vivo da lore (114 notas canônicas). Fonte única da verdade para os agentes de IA. | **ESCOLHIDO (Cérebro Narrativo)** |
| **Dialogic 2 ou Dialogue Manager (Addon Godot 4)** | **US\$ 0** (Open-source, MIT) | Gerencia os briefings de [[candeia]], avisos de rádio das classes e terminais interativos do Cipher diretamente no Godot. | **ESCOLHIDO (Sistema de Diálogo)** |
| **Pipeline Python + LLM API (Tradução Contextual)** | Incluído no envelope de IA | Um script Python lê todos os arquivos `.csv` / `.po` de texto do jogo junto com a pasta `lore/glossario/` do Daedalus e traduz para Inglês, Chinês Simplificado e Espanhol mantendo termos sagrados intocados. | **ESCOLHIDO (Localização 4 Idiomas)** |

### Fluxo Automático: Do Daedalus para dentro do Jogo
Um script construído pelo agente (`tools/sync_daedalus.py`) lê a pasta `jogos/parallax/fragmentos/` deste repositório (separada em `goliath/`, `phantom/` e `cipher/`), extrai o frontmatter YAML (`olhar`, `status`, `liga`) e o texto do fragmento, e gera automaticamente o arquivo `res://data/fragments.json` dentro do Godot 4. Quando criamos um fragmento novo no Obsidian, ele já nasce pronto para aparecer no terminal ou na parede correta do jogo.

---

## 9. Resumo Financeiro do Utility Belt (Dentro dos US\$ 2.000)

Esta tabela consolida todas as ferramentas escolhidas e prova como o arsenal tecnológico completo cabe exatamente no orçamento de dois mil dólares definido no [[blueprint-de-investimento]]:

| Categoria | Ferramenta Principal (Custo Zero / Já Pago) | Ferramenta Paga / IA (Novo Desembolso) | Desembolso Total (18 meses) |
| --- | --- | --- | --- |
| **Publicação na Loja** | Steamworks SDK / SteamPipe | Taxa **Steam Direct** (reembolsável aos US\$ 1.000) | **US\$ 100,00** |
| **Motor, Rede e Física** | **Godot 4.4+**, **Jolt Physics**, **GodotSteam**, **Steam Datagram Relay**, **Godot MCP** | — | **US\$ 0,00** |
| **Agentes de Código, Arquitetura e Texto** | **Cursor Pro + Antigravity (Google AI Pro) já quitados nos meses 1 a 6** (economia de US\$ 240), Ollama local, scripts Python | **Renovação Cursor Pro (US\$ 20) + Antigravity / Google AI Pro (US\$ 20) nos meses 7 a 18** (US\$ 480) + **Cota de API / Tokens extras** (US\$ 240) | **US\$ 720,00** |
| **Cenários e Level Design** | **TrenchBroom**, **FuncGodot**, **Blender 4.x**, **Blender MCP** | — | **US\$ 0,00** |
| **Personagens, Armas e Inimigos 3D** | **Blockbench**, **Blender + Rigify**, **Mixamo**, **Hunyuan3D / TRELLIS (local)** | **Tripo 3D Pro ou Meshy Pro** (Sprint de 3 meses × US\$ 20) | **US\$ 60,00** |
| **Imagens, Texturas, Shaders e UI** | **Material Maker**, **Krita**, **Inkscape**, **ComfyUI (local)** | **Aseprite** (US\$ 20 perpétuo) + **Sprint de Imagem Comercial** (US\$ 120) + **Arte Humana da Cápsula Steam e Key Art** (US\$ 320) | **US\$ 460,00** |
| **Áudio, Vozes e Música** | **Vital Synth**, **Audacity**, **Godot Audio Buses / FMOD Indie** | **ElevenLabs + Suno Pro** (Sprint concentrado de 5 meses = US\$ 140) | **US\$ 140,00** |
| **Lore, Diálogos e Web ARG** | **Obsidian**, **Git**, **Dialogue Manager**, **GitHub/Cloudflare Pages** | **Domínio Web (3 anos)** para o ARG do [[fio-01-sinal-de-proteu]] | **US\$ 40,00** |
| **Reserva de Contingência (24%)** | — | Folga ampliada pela economia dos 6 meses iniciais de IDE/IA já quitados (câmbio, IOF, taxas ou créditos extras na reta final) | **US\$ 480,00** |
| **TOTAL DO PROJETO** | | | **US\$ 2.000,00** |

---

## 10. O Pipeline de Importação Fim a Fim (Checklist de Produção)

Para garantir que nenhuma ferramenta vire gargalo e que tudo entre limpo no Godot 4:

1. **Lore e Fragmentos:** Escritos no **Obsidian (Project Daedalus)** → exportados via `sync_daedalus.py` → importados como `.json` e `.po` em `res://data/`.
2. **Arquitetura dos Biomas:** Blocada no **TrenchBroom** → salva como `.map` em `res://maps/` → convertida em malha e colisão pelo **FuncGodot**.
3. **Props, Classes, Armas e Inimigos:** Gerados/modelados via **Tripo/Blockbench/Blender MCP** → limpos e animados no **Blender** → exportados em **`.glb` (glTF 2.0)** com texturas do **Material Maker** para `res://assets/models/`.
4. **Visão Assimétrica e Efeitos:** Escritos pelo **Agente de Código** em **`.gdshader`** → aplicados nas câmeras de Goliath, Phantom e Cipher.
5. **Vozes, Batida e Som:** Gerados no sprint **ElevenLabs/Suno/Vital** → filtrados como rádio industrial no **Audacity** → exportados em **`.ogg` (Vorbis)** para `res://assets/audio/`.
6. **Lógica, Rede e Build:** Programados com **Agentes + Godot MCP** → testados em 3 instâncias locais via **Steam Datagram Relay** → empacotados via **SteamPipe (`steamcmd`)** direto para a Steam.

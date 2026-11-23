# Project Daedalus & PARALLAX — Regras Globais de Agentes (AGENTS.md / GEMINI.md)

Você está operando no monorepo **Project Daedalus** da **HTL Games**, que governa a arquitetura narrativa (`jogos/parallax/`) e o cliente **Unity 6 LTS** do jogo **PARALLAX** (*Parallax: Co-op Infiltration*) em `client/parallax-unity/`.
Toda edição de lore, código, cena, prefab, shader ou ativo deve obedecer às leis abaixo. A decisão de migração está em `studio/decisoes/0005-migracao-unity-3d.md`.

---

## 1. Os 3 Pilares Inegociáveis da HTL Games

1. **A Mecânica Dita, a Lore Justifica (Gameplay First):** nenhum fato narrativo entra no cânone sem servir a uma restrição de `jogos/parallax/fundacao/contrato-mecanico.md`.
2. **Informação Assimétrica como Ferramenta:** nenhum jogador e nenhuma classe (Goliath, Phantom, Cipher) tem a resposta completa sozinho. Todo puzzle segue `jogos/parallax/fundacao/matriz-de-paralaxe.md`.
3. **O Jogador é o Arqueólogo (Show, Don't Tell):** proibidas exposições longas. A história vem de cenário, manômetros, etiquetas, logs curtos (`jogos/parallax/fragmentos/`) e regras matemáticas.

---

## 2. Idioma e Voz (`studio/guia-de-voz.md`)

- **Documentação, texto in-world e respostas:** Português do Brasil estrito. Gerúndio brasileiro (`fazendo`), nunca lusitano (`a fazer`). Proibido `facto`, `equipa`, `ecrã`, `utilizador`, `contacto`.
- **Código:** classes, métodos, variáveis, pastas, assets e namespaces em **inglês**, nas convenções da Unity/C#. Comentários de código e logs em PT-BR.
- **Zero AI-speak.** Tom de Hard Sci-Fi físico e sensorial.

---

## 3. Vault Obsidian (`studio/convencoes.md`)

- Frontmatter obrigatório em `jogos/parallax/` (`jogo`, `tipo`, `status`, `camada`, `liga`). Fragmentos levam também `olhar`.
- Nomes únicos, minúsculos, sem acento, com hífen. Todo `[[wikilink]]` aponta para arquivo real.
- `camada: restrito` (pasta `restrito/`) **nunca** vaza para fragmentos, quests, raids, interface, JSON exportado ou String Tables da Unity.
- `isca` e `cânone` nunca no mesmo arquivo.
- Depois de editar lore: `python studio/scripts/validate_daedalus.py` (0 erros é condição de entrega).

---

## 4. Engenharia Unity 6 LTS (`client/parallax-unity/`)

### 4.1 Pilha travada
Unity **6000.3 LTS** + **URP 17.3** · **Netcode for GameObjects 2.x** (host autoritativo) · **Steamworks.NET** + `SteamSocketsTransport` próprio sobre **Steam Datagram Relay** · **UnityTransport** local para editor e Multiplayer Play Mode · **Cinemachine 3** · **Input System** · **PhysX** · **Animation Rigging** · **AI Navigation** · **ProBuilder** · **Unity Localization** (PT-BR, EN, ZH-CN, ES) · **glTFast**. Nada pago, nada de UGS Relay/Lobby, nada de servidor dedicado.

### 4.2 Estrutura e fronteiras
Tudo do jogo mora em `Assets/_Project/`:

| Pasta | Conteúdo |
| --- | --- |
| `Data/` | ScriptableObjects (classes, Auxiliares, corporações, armas, relíquias, missões, perfis), JSON gerado do Daedalus, CSV de tradução |
| `Scripts/Core` | `Parallax.Core`: modelos puros (termodinâmica, Batida, Fila de Comando, Três Mãos, escala de sessão) e tipos de dados |
| `Scripts/Networking` | `Parallax.Networking`: Steam, transportes, lançador de sessão |
| `Scripts/Gameplay` | `Parallax.Gameplay`: NetworkBehaviours de classes, puzzles, mundo, câmera, áudio |
| `Scripts/Rendering` | `Parallax.Rendering`: estado global dos Olhares e escolha de renderer |
| `Scripts/UI` | `Parallax.UI`: Quadro de Válvulas e interfaces diegéticas |
| `Scripts/Editor` | `Parallax.Editor`: bootstrap, importador do Daedalus, pós-processador de assets |
| `Tests/EditMode` | `Parallax.Core.Tests` |
| `Tests/PlayMode` | `Parallax.PlayMode.Tests` (sessão real no `Lab_Jogavel`) |
| `Shaders/`, `Art/`, `Audio/`, `Prefabs/`, `Scenes/`, `Settings/` | ativos |

Dependências só descem: `Core` ← `Rendering` ← `Gameplay` ← `UI` ← `Editor`; `Networking` depende só de `Core` e é usado por `UI` e `Editor`. **Nunca** referencie `Gameplay` a partir de `Core`, nem `UnityEditor` fora de `Editor`.

### 4.3 Padrões de C#
- `[SerializeField] private` para campos de inspetor; propriedades públicas só de leitura. Sem campos públicos mutáveis.
- **Dados em ScriptableObject** (armas, corporações, relíquias, missões, perfis térmicos e de câmera). Nenhum número de balanceamento hard-coded em MonoBehaviour.
- **Proibido lógica de jogo em `Update()`.** Regras rodam em eventos, RPCs, callbacks do Input System, corrotinas ou no tick de rede (`NetworkManager.NetworkTickSystem.Tick`, ex.: `ThermalSystem`). `Update`/`LateUpdate` só para integrar movimento do dono, ler câmera e interpolar visual, sempre com comentário dizendo isso.
- Sem `FindObjectOfType`/`Find` em runtime de jogo (permitido em ferramentas de laboratório e editor). Estáticos de runtime são zerados em `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` (o projeto roda com Domain Reload desligado).
- Lógica nova e testável vai primeiro para `Parallax.Core` com teste EditMode.
- Um tipo `MonoBehaviour`/`ScriptableObject` por arquivo, com o mesmo nome.

### 4.4 Rede (NGO 2.x)
- Host autoritativo: estado de puzzle, temperatura, Fila de Comando e Contagem de Vazamento vivem no servidor (`NetworkVariable`/`NetworkList`).
- RPCs com o atributo universal: `[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]` para pedidos de clientes e `[Rpc(SendTo.ClientsAndHost)]` para avisos. O método termina com `Rpc`. `ServerRpc`/`ClientRpc` legados e `RequireOwnership` estão proibidos.
- O servidor sempre revalida a classe do remetente via `SessionRoster` antes de aceitar uma ação de puzzle.
- Movimento de personagem: autoridade do dono (`NetworkTransform` em modo Owner). Física pesada (portas de 10 t, cabos, plataformas): só o servidor simula.
- Todo puzzle de três pontas implementa `IRehearsableAction` e funciona com 1, 2 e 3 humanos (Janela de Ensaio → Fila de Comando → execução; Janela de Memória 12 s base, +50% e +100%).

### 4.5 Render e Olhares
- Cada classe tem um Universal Renderer (`Parallax_URP`: 0 Neutro, 1 Goliath, 2 Phantom, 3 Cipher) com um Full Screen Pass. A câmera local troca o renderer via `VisionController`; ninguém vê o Olhar do outro.
- Sistemas de jogo alteram o visual só por `ParallaxVisionState` (globais `_Parallax_*`).
- Meta de desempenho: 60 FPS no Steam Deck. Sem loops pesados por fragmento.

### 4.6 Versionamento
- `Asset Serialization: Force Text` e `Visible Meta Files` (garantidos por `Parallax/Bootstrap`). Todo asset entra com o seu `.meta`.
- Binários de arte e áudio vão por **Git LFS** (`.gitattributes`). Nunca versione `Library/`, `Temp/`, `Logs/`, `UserSettings/`.

---

## 5. Fluxo com MCP e CLI

1. **Unity:** use o **Unity CLI** (`unity`) e o servidor `unity mcp` para abrir o editor, recompilar (`unity recompile`), ler o console, criar cenas e prefabs e rodar testes (`unity test --platform EditMode`). Depois de qualquer mudança via MCP: recompile, confira o console com **0 erros** e rode os testes EditMode.
2. **Blender MCP (Blender 5.3 + addon `blender_mcp`):** confirme a conexão antes de rodar `bpy`. Exporte FBX ou GLB direto para `Assets/_Project/Art/<categoria>/`. Arquivos `.blend` ficam em `art-pipeline/source/blender/`, fora de `Assets/`.
3. **ComfyUI MCP:** só conceito e base de textura. **Nenhum ativo bruto de IA entra no jogo** sem paintover ou edição humana (Krita, Aseprite, Material Maker, Blender).
4. **Daedalus → Unity:** `python studio/scripts/sync_daedalus.py` e depois o menu `Parallax/Daedalus/Import Generated JSON`. A Unity nunca escreve no vault.

---

## 6. Orçamento Teto (US$ 2.000)

Nunca proponha plugin, asset de loja, serviço de nuvem ou assinatura fora do envelope de `jogos/parallax/fundacao/utility-belt-tecnologias.md`. A Unity Personal é gratuita até US$ 200 mil de receita anual; acima disso, a Unity Pro vira custo pago pela receita, e não pelo orçamento de produção.

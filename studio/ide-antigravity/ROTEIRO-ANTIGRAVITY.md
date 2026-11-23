# Roteiro de Configuração e Uso — Google Antigravity (PARALLAX em Unity 6 LTS)

Esta pasta (`studio/ide-antigravity/`) documenta e guarda os espelhos de configuração do **Google Antigravity** (com assinatura **Google AI Pro** já ativa por 6 meses) para o desenvolvimento de **PARALLAX**. Os arquivos ativos ficam na raiz do projeto em `GEMINI.md`, `AGENTS.md` e `.agents/`.

---

## 1. Papel do Antigravity na Estratégia Dual-IDE

Enquanto o Cursor Pro atua na edição rápida linha a linha, o **Antigravity** é o nosso **Centro de Comando Agentic**: ele possui **Skills sob demanda (Progressive Disclosure)**, **Subagentes paralelos** (`research`, `self`, `browser`, `image-generator`), **Workflows programáveis em Python (`run_workflow`)** e **Lifecycle Hooks (`hooks.json`)**.

| Recurso Exclusivo do Antigravity | Como usamos em PARALLAX |
| --- | --- |
| **Hierarchical Rules (`GEMINI.md` + `AGENTS.md`)** | Carrega automaticamente os 3 Pilares, o Guia de Voz PT-BR, as regras de engenharia Unity e as restrições de US$ 2.000. |
| **Workspace Skills (`.agents/skills/*/SKILL.md`)** | Mantém 3 runbooks fora da janela de contexto até o momento exato em que são necessários (`daedalus-lore-architect`, `parallax-unity-engineer`, `parallax-asset-pipeline`). |
| **Plugin & MCP (`mcp_config.json`)** | Conecta o agente à **Unity** (Unity CLI `unity mcp`), ao **Blender MCP**, ao **ComfyUI** e ao **Filesystem MCP** via plugin local (`.agents/plugins/daedalus-parallax/`). |
| **Lifecycle Hooks (`.agents/hooks.json`)** | Roda o validador do Daedalus após cada escrita em `jogos/parallax/`. |
| **Slash Commands (`/plan`, `/boost`, `/goal`, `/grill-me`)** | `/grill-me` para alinhar decisões de design de Raids; `/plan` para desenhar encontros complexos; `/goal` para tarefas longas de geração e validação. |

---

## 2. Arquitetura de Arquivos Ativos do Antigravity no Repositório

```text
Project-Parallax/
├── GEMINI.md                                     # Regra raiz do Antigravity (inclui AGENTS.md e diretrizes de subagentes)
├── AGENTS.md                                     # Constituição universal compartilhada entre Antigravity e Cursor
└── .agents/
    ├── hooks.json                                # Hook PostToolUse para auditoria pós-edição
    ├── plugins/
    │   └── daedalus-parallax/
    │       ├── plugin.json                       # Manifesto do plugin local do projeto
    │       └── mcp_config.json                   # Servidores MCP (Unity, Blender, ComfyUI, Filesystem)
    └── skills/
        ├── daedalus-lore-architect/SKILL.md      # Lore, fragmentos e sincronização com a Unity
        ├── parallax-unity-engineer/SKILL.md      # C#, NGO + Steam, ThermalNode, BeatSequencer, Olhares na URP, testes
        └── parallax-asset-pipeline/SKILL.md      # ProBuilder, Blender MCP, 2 Rigs, texturas PBR e áudio de rádio
```

---

## 3. Passo a Passo de Configuração no Google Antigravity

### Passo 1: Verificar a descoberta automática de Rules e Skills
1. Abra o workspace `C:\opt\Project-Parallax` no **Antigravity**.
2. O Antigravity descobre `GEMINI.md`, `AGENTS.md`, as 3 skills em `.agents/skills/` e o hook em `.agents/hooks.json`.

### Passo 2: Servidores MCP
Os MCPs ficam no plugin local para evitar instâncias duplicadas. Rode `.\studio\scripts\setup_windows_pipeline.ps1 -SyncMcp` para zerar o `~/.gemini/config/mcp_config.json` global e confirme, em **Additional Options (...) > MCP Servers**, que `daedalus-filesystem`, `parallax-unity`, `blender-mcp` e `comfyui` estão conectados. O `parallax-unity` exige o editor aberto (`unity open client/parallax-unity`).

### Passo 3: Como acionar o Antigravity no dia a dia
- **Lore e fragmentos:** *"Usando a skill daedalus-lore-architect, crie 3 novos fragmentos para o Fosso de Hefesto (um para cada olhar) e sincronize com a Unity."* O agente cria as notas, roda `validate_daedalus.py` e `sync_daedalus.py` e importa pelo menu `Parallax/Daedalus/Import Generated JSON`.
- **Sistemas na Unity:** *"Usando a skill parallax-unity-engineer, crie o puzzle das 9 válvulas de Os Tubos Falam com IRehearsableAction e testes EditMode."*
- **Modelagem via MCP:** com o Blender aberto e o addon conectado, *"Usando a skill parallax-asset-pipeline e o blender-mcp, gere um módulo de tubulação da Therma-Tek com volante hidráulico e exporte em GLB para Assets/_Project/Art/Props/."*

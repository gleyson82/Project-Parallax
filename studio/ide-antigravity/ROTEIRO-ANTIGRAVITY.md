# Roteiro de Configuração e Uso — Google Antigravity (PARALLAX)

Esta pasta (`studio/ide-antigravity/`) documenta e guarda os espelhos de configuração do **Google Antigravity** (com assinatura **Google AI Pro** já ativa por 6 meses) para o desenvolvimento de **PARALLAX**. Os arquivos ativos já estão instalados na raiz do projeto em `GEMINI.md`, `AGENTS.md` e `.agents/`.

---

## 1. Papel do Antigravity na Estratégia Dual-IDE

Enquanto o Cursor Pro atua na edição rápida linha a linha, o **Antigravity** é o nosso **Centro de Comando Agentic**: ele possui **Skills sob demanda (Progressive Disclosure)**, **Subagentes paralelos** (`research`, `self`, `browser`, `image-generator`), **Workflows programáveis em Python (`run_workflow`)** e **Lifecycle Hooks (`hooks.json`)**.

| Recurso Exclusivo do Antigravity | Como usamos em PARALLAX |
| --- | --- |
| **Hierarchical Rules (`GEMINI.md` + `AGENTS.md`)** | Carrega automaticamente os 3 Pilares, o Guia de Voz PT-BR e as restrições de US$ 2.000 sem estourar o orçamento de 20.000 tokens de regras. |
| **Workspace Skills (`.agents/skills/*/SKILL.md`)** | Mantém 3 runbooks especializados fora da janela de contexto até o momento exato em que são necessários (`daedalus-lore-architect`, `parallax-godot-engineer`, `parallax-asset-pipeline`). |
| **Plugin & MCP (`mcp_config.json`)** | Conecta o agente ao **Godot MCP**, **Blender MCP** e **Filesystem MCP** globalmente (`~/.gemini/config/mcp_config.json`) ou via plugin local (`.agents/plugins/daedalus-parallax/`). |
| **Lifecycle Hooks (`.agents/hooks.json`)** | Executa scripts automáticos após chamadas de escrita (`write_file` / `edit_file`). |
| **Slash Commands (`/plan`, `/boost`, `/goal`, `/grill-me`)** | `/grill-me` para alinhar decisões de design de Raids; `/plan` para desenhar encontros complexos; `/goal` para tarefas longas de geração e validação. |

---

## 2. Arquitetura de Arquivos Ativos do Antigravity no Repositório

```text
Project-Daedalus/
├── GEMINI.md                                               # Regra raiz do Antigravity (inclui @[AGENTS.md] e diretrizes de subagentes)
├── AGENTS.md                                               # Constituição universal compartilhada entre Antigravity e Cursor
└── .agents/
    ├── hooks.json                                          # Hook PostToolUse para auditoria pós-edição
    ├── plugins/
    │   └── daedalus-parallax/
    │       ├── plugin.json                                 # Manifesto do plugin local do projeto
    │       └── mcp_config.json                             # Servidores MCP empacotados (Godot, Blender, Filesystem)
    └── skills/
        ├── daedalus-lore-architect/
        │   └── SKILL.md                                    # Runbook de criação/validação de lore, fragmentos e sincronização JSON
        ├── parallax-godot-engineer/
        │   └── SKILL.md                                    # Runbook de GDScript tipado, Jolt Physics 2.5D, ThermalNode, BeatSequencer e GodotSteam
        └── parallax-asset-pipeline/
            └── SKILL.md                                    # Runbook de TrenchBroom/FuncGodot, Blender MCP, 2 Rigs e áudio de rádio
```

---

## 3. Passo a Passo de Configuração no Google Antigravity

### Passo 1: Verificar a Descoberta Automática de Rules e Skills
1. Abra o workspace `/Users/macbookair/opt/repository/Project-Daedalus` no **Antigravity**.
2. O Antigravity descobre automaticamente:
   - `GEMINI.md` e `AGENTS.md` na raiz do repositório.
   - As 3 skills em `.agents/skills/`:
     - `daedalus-lore-architect`
     - `parallax-godot-engineer`
     - `parallax-asset-pipeline`
   - O hook de ciclo de vida em `.agents/hooks.json`.

### Passo 2: Ativar os Servidores MCP Globais no Antigravity (`mcp_config.json`)
Para que os servidores MCP (`godot-mcp`, `blender-mcp` e `daedalus-filesystem`) fiquem disponíveis em todas as sessões do Antigravity na sua máquina:
1. Certifique-se de ter o **Node.js** e o **`uv`** instalados:
   ```bash
   brew install node uv
   ```
2. Copie o arquivo de configuração MCP preparado nesta pasta para o diretório global do Antigravity (`~/.gemini/config/mcp_config.json`):
   ```bash
   mkdir -p ~/.gemini/config
   cp studio/ide-antigravity/mcp_config.json ~/.gemini/config/mcp_config.json
   ```
3. Na interface do Antigravity, clique em **Additional Options (...) > MCP Servers** para confirmar que `daedalus-filesystem`, `godot-mcp` e `blender-mcp` estão listados e conectados.

### Passo 3: Como Acionar o Máximo Poder do Antigravity no Dia a Dia
- **Para criar ou auditar Lore e Fragmentos:**
  Peça naturalmente: *"Usando a skill daedalus-lore-architect, crie 3 novos fragmentos para o Fosso de Hefesto (um para cada olhar) e sincronize o JSON."* O agente lerá a skill, criará os arquivos com frontmatter `cânone`, rodará `validate_daedalus.py` e exportará `fragments_export.json`.
- **Para programar sistemas no Godot 4.4+:**
  Peça: *"Usando a skill parallax-godot-engineer, crie a cena e o script do ThermalNode.gd e os 3 shaders de visão assimétrica."*
- **Para modelar ou montar mapas via MCP:**
  Com o Blender aberto e o addon `blender-mcp` ativo, peça: *"Usando a skill parallax-asset-pipeline e o blender-mcp, gere um módulo de tubulação da Therma-Tek com volante hidráulico e exporte em .glb."*

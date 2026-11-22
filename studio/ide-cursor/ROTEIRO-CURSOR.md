# Roteiro de Configuração e Uso — Cursor Pro (PARALLAX)

Esta pasta (`studio/ide-cursor/`) documenta e guarda os espelhos de segurança de toda a configuração do **Cursor Pro** para o desenvolvimento de **PARALLAX**. Os arquivos ativos já estão instalados na raiz do projeto em `.cursor/` e `AGENTS.md`.

---

## 1. Papel do Cursor Pro na Estratégia Dual-IDE

Como o estúdio já possui **6 meses quitados** do **Cursor Pro (US$ 20/mês)** e do **Antigravity (Google AI Pro)**, dividimos o trabalho para extrair 100% de cada ferramenta sem esgotar a cota rápida de nenhuma delas:

| Quando usar o **Cursor Pro** | Quando usar o **Antigravity** |
| --- | --- |
| Edição rápida linha a linha (**Cursor Tab**) em `.gd` e `.gdshader`. | Orquestração pesada com **Subagentes** (`research`, `self`, `browser`) e **Workflows Python** (`run_workflow`). |
| Refatoração multi-arquivo cirúrgica via **Composer (`Cmd+I` / `Ctrl+I`)** com regras `.mdc` engatilhadas por extensão (`globs`). | Auditoria completa das 114+ notas de lore, geração em lote de fragmentos e criação de sistemas inteiros via **Skills** (`.agents/skills/`). |
| Iteração ao vivo com o editor do Godot 4 aberto lado a lado usando o **Godot MCP**. | Planejamento arquitetural profundo, revisão de física/matemática de puzzles e geração de ativos assistida. |

---

## 2. Arquitetura de Arquivos Ativos do Cursor no Repositório

```text
Project-Daedalus/
├── AGENTS.md                                      # Constituição compartilhada lida automaticamente pelo Cursor e pelo Antigravity
└── .cursor/
    ├── mcp.json                                   # Configuração local dos 3 servidores MCP (Filesystem, Godot 4, Blender)
    └── rules/
        ├── 00-daedalus-constitution.mdc           # (alwaysApply: true) Os 3 Pilares, PT-BR estrito, teto de US$ 2.000
        ├── 01-lore-vault-obsidian.mdc             # (globs: jogos/parallax/**/*.md) Frontmatter YAML, proteção de restrito/, física de Tritão
        ├── 02-godot4-gdscript.mdc                 # (globs: **/*.{gd,tscn,tres}) GDScript tipado, Jolt Physics 2.5D, GodotSteam, Auxiliares
        └── 03-shaders-asymmetric-vision.mdc       # (globs: **/*.gdshader) Shaders dos 3 Olhares (Goliath, Phantom, Cipher) e Lágrima do Leviatã
```

---

## 3. Passo a Passo de Configuração no Cursor Pro

### Passo 1: Verificar o Carregamento das Rules (`.cursor/rules/*.mdc`)
1. Abra a pasta `/Users/macbookair/opt/repository/Project-Daedalus` no **Cursor**.
2. Vá em **Cursor Settings (`Cmd+Shift+J` no macOS) > General > Rules for AI**.
3. Confirme que as **4 Project Rules** aparecem listadas sob *Project Rules*:
   - `00-daedalus-constitution` (*Always Apply*)
   - `01-lore-vault-obsidian` (*Auto Attached em `jogos/parallax/**/*.md`*)
   - `02-godot4-gdscript` (*Auto Attached em `**/*.{gd,tscn,tres}`*)
   - `03-shaders-asymmetric-vision` (*Auto Attached em `**/*.gdshader`*)
4. Dica de uso: No chat ou no Composer, você também pode invocar uma regra manualmente digitando `#01-lore-vault-obsidian` ou `#02-godot4-gdscript`.

### Passo 2: Ativar os Servidores MCP (`.cursor/mcp.json`)
1. Certifique-se de ter o **Node.js (v18+)** e o gerenciador Python **`uv`** instalados no macOS:
   ```bash
   brew install node uv
   ```
2. Vá em **Cursor Settings > MCP**.
3. O Cursor lerá automaticamente o arquivo `.cursor/mcp.json` do projeto e exibirá os três servidores:
   - **`daedalus-filesystem`**: Acesso estruturado às notas do Daedalus.
   - **`godot-mcp`**: Ponte direta com o Godot 4.4+ (para ativar no Godot, instale o addon `godot_mcp` em `addons/` e ative em *Project > Project Settings > Plugins*).
   - **`blender-mcp`**: Ponte direta com o Blender 4.x (para ativar no Blender, instale o addon `addon.py` do `blender-mcp` em *Edit > Preferences > Add-ons* e clique em *Start MCP Server* na aba lateral `N`).
4. Clique no botão de atualização ao lado de cada servidor até a luz ficar **verde**.

### Passo 3: Configurar Indexação e Exclusões do Codebase
1. Em **Cursor Settings > Features > Codebase Indexing**, garanta que a indexação automática esteja ativada.
2. Caso a pasta do motor Godot seja criada dentro deste repositório, adicione `.godot/`, `*.import` e `*.glb` ao arquivo `.cursorignore` para que o indexador semântico foque 100% nas notas de lore (`.md`), scripts (`.gd`), shaders (`.gdshader`) e cenas (`.tscn`).

### Passo 4: Atalhos de Produtividade Diária no Cursor
- **Criar ou revisar Lore:** Abra qualquer arquivo em `jogos/parallax/`, pressione `Cmd+I` (Composer) e peça a alteração. A regra `01-lore-vault-obsidian.mdc` será injetada sozinha. Ao terminar, rode no terminal integrado:
  ```bash
  python3 studio/scripts/validate_daedalus.py
  ```
- **Exportar Fragmentos para o Godot 4:**
  ```bash
  python3 studio/scripts/sync_daedalus.py
  ```

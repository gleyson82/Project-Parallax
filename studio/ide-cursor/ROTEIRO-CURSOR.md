# Roteiro de Configuração e Uso — Cursor Pro (PARALLAX em Unity 6 LTS)

Esta pasta (`studio/ide-cursor/`) documenta e guarda os espelhos de segurança de toda a configuração do **Cursor Pro** para o desenvolvimento de **PARALLAX**. Os arquivos ativos ficam na raiz do projeto em `.cursor/` e `AGENTS.md`.

---

## 1. Papel do Cursor Pro na Estratégia Dual-IDE

Como o estúdio já possui **6 meses quitados** do **Cursor Pro (US$ 20/mês)** e do **Antigravity (Google AI Pro)**, dividimos o trabalho para extrair 100% de cada ferramenta sem esgotar a cota rápida de nenhuma delas:

| Quando usar o **Cursor Pro** | Quando usar o **Antigravity** |
| --- | --- |
| Edição rápida linha a linha (**Cursor Tab**) em `.cs`, `.shader` e `.hlsl`. | Orquestração pesada com **Subagentes** (`research`, `self`, `browser`) e **Workflows Python** (`run_workflow`). |
| Refatoração multi-arquivo cirúrgica via **Composer (`Ctrl+I`)** com regras `.mdc` engatilhadas por caminho (`globs`). | Auditoria completa das 114+ notas de lore, geração em lote de fragmentos e criação de sistemas inteiros via **Skills** (`.agents/skills/`). |
| Iteração ao vivo com o editor da Unity aberto, via **Unity CLI** (`unity recompile`, `unity test`) e o servidor MCP `parallax-unity`. | Planejamento arquitetural profundo, revisão de física/matemática de puzzles e geração de ativos assistida. |

---

## 2. Arquitetura de Arquivos Ativos do Cursor no Repositório

```text
Project-Parallax/
├── AGENTS.md                                      # Constituição compartilhada (Cursor e Antigravity)
└── .cursor/
    ├── mcp.json                                   # Servidores MCP: Filesystem, parallax-unity (Unity CLI), Blender, ComfyUI
    └── rules/
        ├── 00-daedalus-constitution.mdc           # (alwaysApply) Os 3 Pilares, PT-BR estrito, pilha Unity, teto de US$ 2.000
        ├── 01-lore-vault-obsidian.mdc             # (jogos/parallax/**/*.md) Frontmatter, proteção de restrito/, física de Tritão
        ├── 02-unity-csharp.mdc                    # (client/parallax-unity/**) Assemblies, ScriptableObjects, sem lógica em Update, testes
        ├── 03-shaders-asymmetric-vision.mdc       # (*.shader, *.hlsl, *.shadergraph) Os 3 Olhares na URP
        └── 04-unity-networking.mdc                # (Scripts/Networking e Gameplay) NGO + Steam, RPCs, Auxiliares
```

---

## 3. Passo a Passo de Configuração no Cursor Pro

### Passo 1: Verificar o carregamento das Rules
1. Abra a pasta `C:\opt\Project-Parallax` no **Cursor**.
2. Vá em **Cursor Settings > Rules** e confirme as **5 Project Rules** acima.
3. Você também pode invocar uma regra manualmente no chat, por exemplo `@02-unity-csharp`.

### Passo 2: Ativar os Servidores MCP (`.cursor/mcp.json`)
1. Pré-requisitos (já presentes nesta máquina): **Node.js**, **uv**, **Unity CLI** (`unity` no PATH) e Unity **6000.3.29f1 LTS**.
2. Vá em **Cursor Settings > MCP**. O Cursor lê `.cursor/mcp.json` e mostra:
   - **`daedalus-filesystem`**: acesso estruturado às notas do Daedalus.
   - **`parallax-unity`**: `unity mcp --project-path client/parallax-unity`. Exige o editor aberto (`unity open client/parallax-unity`) e o pacote `com.unity.pipeline`, que já está no manifest.
   - **`blender-mcp`**: ponte com o Blender 5.3 (addon `blender_mcp` ativo; clique em *Connect* na aba lateral `N`).
   - **`comfyui`**: geração local de conceitos e bases de textura (ComfyUI em `http://127.0.0.1:8188`).
3. As entradas globais `unity` e `unity-mcp` do `~/.cursor/mcp.json` são do Unity AI Assistant e apontam para o mesmo relay; mantenha só uma delas para não duplicar ferramentas.

### Passo 3: Indexação
O `.gitignore` já exclui `Library/`, `Temp/`, `Logs/` e `UserSettings/` do projeto Unity. Se a indexação ficar lenta, crie um `.cursorignore` com `client/parallax-unity/Library/` e `*.png`, `*.glb`, `*.fbx`.

### Passo 4: Atalhos de Produtividade Diária
- **Lore:** edite em `jogos/parallax/` (a regra `01` entra sozinha) e rode `python studio/scripts/validate_daedalus.py`.
- **Código:** depois de cada mudança, `unity recompile --project-path client/parallax-unity` e `unity test --project-path client/parallax-unity`.
- **Fragmentos para a Unity:** `python studio/scripts/sync_daedalus.py` e, no editor, `Parallax/Daedalus/Import Generated JSON`.
- **Reconfigurar o projeto:** menu `Parallax/Bootstrap/Run Full Bootstrap` (idempotente).

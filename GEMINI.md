# Project Daedalus & PARALLAX — Regras do Workspace Antigravity (GEMINI.md)

Aplique integralmente as regras de `AGENTS.md` (carregado automaticamente) somadas às convenções abaixo:
- `@[studio/convencoes.md](./studio/convencoes.md)`
- `@[studio/guia-de-voz.md](./studio/guia-de-voz.md)`

> Para consultas de pilha tecnológica e teto orçamentário de US$ 2.000, leia sob demanda `jogos/parallax/fundacao/utility-belt-tecnologias.md` (não expandido inline para respeitar o orçamento de 24 KB das regras).

## Diretrizes Específicas para o Agente Antigravity

1. **Orquestração e Subagentes:**
   - Use o subagente `research` ou workflows (`run_workflow`) para auditorias amplas em todas as 114+ notas de `jogos/parallax/`.
   - Ao editar ou criar notas de lore, execute sempre o validador:
     `uv run --no-project python studio/scripts/validate_daedalus.py`
     para garantir zero wikilinks quebrados e zero termos em PT-PT.
2. **Skills do Projeto (`.agents/skills/`):**
   - Ative `daedalus-lore-architect` sempre que criar ou revisar lore, facções, biomas, quests, raids ou fragmentos in-world.
   - Ative `parallax-godot-engineer` sempre que gerar cenas `.tscn`, scripts `.gd`, shaders `.gdshader` ou lógica de rede GodotSteam.
   - Ative `parallax-asset-pipeline` sempre que automatizar **Blender MCP** (`mcp-for-blender` v2), **ComfyUI MCP**, TrenchBroom/FuncGodot, áudio (`BeatSequencer`) ou exportação JSON do Daedalus.
3. **Servidores MCP (Fonte Única no Plugin Local):**
   - Os servidores MCP do projeto (`daedalus-filesystem`, `godot-mcp`, `blender-mcp`, `comfyui`) ficam concentrados em `.agents/plugins/daedalus-parallax/mcp_config.json` para evitar instâncias duplicadas.

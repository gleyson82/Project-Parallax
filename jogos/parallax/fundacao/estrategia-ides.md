---
jogo: parallax
tipo: fundacao
status: cânone
camada: base
liga: ["[[indice]]", "[[utility-belt-tecnologias]]", "[[blueprint-de-investimento]]", "[[contrato-mecanico]]"]
---

# Estratégia Dual-IDE — Cursor Pro & Google Antigravity

**Estúdio:** HTL Games
**Assinaturas Ativas (Quitadas pelos primeiros 6 meses):**
- **Cursor Pro** (US\$ 20/mês — já pago nos meses 1 a 6)
- **Google Antigravity / Google AI Pro** (US\$ 19,99/mês — já pago nos meses 1 a 6)

---

## 1. Visão Geral: Como as duas IDEs trabalham juntas no mesmo repositório

Para que o desenvolvedor possa alternar entre o **Cursor Pro** e o **Google Antigravity** a qualquer minuto sem perder contexto, sem gerar conflitos de formatação e aproveitando o melhor de cada plataforma, o repositório **Project Daedalus** foi configurado com uma **Arquitetura Espelhada de Fonte Única**:

1. **Constituição Compartilhada ([AGENTS.md](file:///Users/macbookair/opt/repository/Project-Daedalus/AGENTS.md)):**
   Lida nativamente tanto pelo Cursor quanto pelo Antigravity. Contém os 3 Pilares da HTL Games, as proibições de PT-PT ([guia-de-voz.md](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/guia-de-voz.md)), o esquema YAML do Obsidian ([convencoes.md](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/convencoes.md)) e as travas do teto orçamentário de US\$ 2.000 ([utility-belt-tecnologias.md](file:///Users/macbookair/opt/repository/Project-Daedalus/jogos/parallax/fundacao/utility-belt-tecnologias.md)).
2. **Camada Nativa do Cursor Pro (`.cursor/` e `studio/ide-cursor/`):**
   Usa regras `.mdc` com ativação automática por padrão de arquivo (`globs`) para injetar apenas o contexto necessário quando você abre uma nota de lore (`.md`), um script do Godot (`.gd`), uma cena (`.tscn`) ou um shader (`.gdshader`).
3. **Camada Nativa do Google Antigravity (`GEMINI.md`, `.agents/` e `studio/ide-antigravity/`):**
   Usa **Progressive Disclosure** por meio de 3 **Skills** sob demanda (`.agents/skills/`), **Lifecycle Hooks** (`.agents/hooks.json`), **Subagentes** e **Plugin MCP** (`.agents/plugins/daedalus-parallax/`).
4. **Validador e Sincronizador Unificados (`studio/scripts/`):**
   Ambas as IDEs executam os mesmos dois scripts Python para garantir integridade:
   - `python3 studio/scripts/validate_daedalus.py` (valida YAML, wikilinks do Obsidian, bloqueio de PT-PT e isolamento da pasta `restrito/`).
   - `python3 studio/scripts/sync_daedalus.py` (compila os fragmentos in-world de Goliath, Phantom e Cipher em JSON para o Godot 4).

---

## 2. Divisão Tática de Trabalho (Maximizando Cota e Velocidade)

| Tarefa do Projeto PARALLAX | IDE Recomendada | Recurso Utilizado | Por quê? |
| --- | --- | --- | --- |
| **Escrever, expandir ou auditar lotes de Lore e Raids** | **Antigravity** | Skill `daedalus-lore-architect` + Subagente `research` / `run_workflow` | Analisa dezenas de notas em paralelo sem poluir o contexto principal e valida os pares cruzados da [[matriz-de-paralaxe]]. |
| **Ajuste rápido em uma nota de Lore ou correção pontual** | **Cursor Pro** | Composer (`Cmd+I`) + Regra `01-lore-vault-obsidian.mdc` | A regra `.mdc` é anexada automaticamente ao abrir qualquer arquivo em `jogos/parallax/**/*.md`. |
| **Arquitetar um sistema novo do Godot 4 (ex.: Rede GodotSteam, Auxiliares, Quadro de Válvulas)** | **Antigravity** | Skill `parallax-godot-engineer` + `/plan` ou `/boost` | Planejamento profundo multi-etapas e geração completa de cenas `.tscn` em texto puro. |
| **Programar e ajustar gameplay minuto a minuto (.gd / .gdshader)** | **Cursor Pro** | **Cursor Tab** + Regra `02-godot4-gdscript.mdc` e `03-shaders-asymmetric-vision.mdc` | O autocomplete preditivo (`Tab`) e o `Cmd+K` inline são imbatíveis para ajustar parâmetros de física Jolt e shaders com o jogo aberto. |
| **Gerar tubulações 3D no Blender e montar níveis brutalistas** | **Ambas (via MCP)** | Skill `parallax-asset-pipeline` + `blender-mcp` / `godot-mcp` | O mesmo protocolo MCP controla o Blender (`bpy`) e o Godot 4 a partir de qualquer uma das duas IDEs. |

---

## 3. Mapa das Pastas Exclusivas Criadas

- **Pasta Exclusiva do Cursor Pro:** [`studio/ide-cursor/`](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/ide-cursor/ROTEIRO-CURSOR.md)
  - Contém o [ROTEIRO-CURSOR.md](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/ide-cursor/ROTEIRO-CURSOR.md) e cópias de referência de `.cursor/mcp.json` e `.cursor/rules/*.mdc`.
- **Pasta Exclusiva do Google Antigravity:** [`studio/ide-antigravity/`](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/ide-antigravity/ROTEIRO-ANTIGRAVITY.md)
  - Contém o [ROTEIRO-ANTIGRAVITY.md](file:///Users/macbookair/opt/repository/Project-Daedalus/studio/ide-antigravity/ROTEIRO-ANTIGRAVITY.md) e cópias de referência de `mcp_config.json`, `hooks.json` e das skills de `.agents/`.

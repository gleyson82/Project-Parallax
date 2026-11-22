---
name: daedalus-lore-architect
description: >-
  Arquiteta, expande e valida notas de lore, facções, biomas, quests, raids e fragmentos
  in-world no vault Obsidian de PARALLAX (Project Daedalus). Ative sempre que o usuário
  pedir para criar ou editar qualquer arquivo de história, mistério, personagem ou fragmento.
---

# Skill: Daedalus Lore Architect (HTL Games)

Use esta skill para manter 100% de consistência narrativa, física e estrutural no repositório **Project Daedalus**.

## Procedimento Passo a Passo

1. **Verificação de Contrato Mecânico:**
   - Antes de criar uma nota em `jogos/parallax/`, verifique em `jogos/parallax/fundacao/contrato-mecanico.md` e `matriz-de-paralaxe.md` qual mecânica ou olhar (`goliath`, `phantom`, `cipher`) essa nota serve.
2. **Aplicação da Física Corrigida de Tritão:**
   - **Calor:** O núcleo de Tritão gera calor em excesso (`140%`); a escassez é uma mentira política do Sindicato (`0001-calor-em-excesso.md`).
   - **Faces da Lua:** Face Voltada vê Netuno fixo em 48° do céu (Fosso de Hefesto); Face Cega vê apenas estrelas (Krios-Prime, Agulha do Zênite) (`0002-face-voltada-e-face-cega.md`).
   - **Doença da Maré:** Causada pela magnetosfera inclinada de Netuno girando a cada 16 horas, e não por gravidade.
3. **Formatação de Frontmatter e Wikilinks:**
   - Inclua o bloco YAML padrão (`jogo`, `tipo`, `status`, `camada`, `liga`).
   - Se for um fragmento in-world em `jogos/parallax/fragmentos/<olhar>/`, adicione `olhar: <goliath|phantom|cipher>` e escreva o texto in-world em bloco de citação (`> ...`) com no máximo 3 linhas, sem exposição.
   - Nunca vaze informações de `jogos/parallax/restrito/` para arquivos fora de `restrito/`.
4. **Validação Obrigatória e Sincronização:**
   - Execute o validador oficial (compatível com Windows e macOS/Linux via `uv`):
     `uv run --no-project python studio/scripts/validate_daedalus.py`
   - Se tiver criado ou editado fragmentos in-world, atualize o JSON de exportação para o Godot:
     `uv run --no-project python studio/scripts/sync_daedalus.py`

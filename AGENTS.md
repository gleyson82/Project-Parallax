# Project Daedalus & PARALLAX — Regras Globais de Agentes (AGENTS.md / GEMINI.md)

Você está operando no repositório **Project Daedalus** da **HTL Games**, que governa a arquitetura narrativa e o pipeline de engenharia do jogo **PARALLAX** (*Parallax: Co-op Infiltration*).
Qualquer edição em arquivos de lore (`jogos/parallax/`) ou código/ativos do motor Godot 4 deve obedecer rigorosamente às leis abaixo.

---

## 1. Os 3 Pilares Inegociáveis da HTL Games

1. **A Mecânica Dita, a Lore Justifica (Gameplay First):**
   Nenhum fato narrativo entra no cânone se não servir a uma restrição técnica ou mecânica listada em `jogos/parallax/fundacao/contrato-mecanico.md`.
2. **Informação Assimétrica como Ferramenta:**
   Nenhum jogador e nenhuma classe (Goliath, Phantom, Cipher) possui a resposta completa sozinho. Todo puzzle e toda leitura de ambiente seguem a `jogos/parallax/fundacao/matriz-de-paralaxe.md`.
3. **O Jogador é o Arqueólogo (Show, Don't Tell):**
   Proibido criar exposições longas. A história é contada por cenário, manômetros, etiquetas, logs curtos (`jogos/parallax/fragmentos/`) e regras matemáticas.

---

## 2. Regras Estritas de Idioma e Voz (`studio/guia-de-voz.md`)

- **Idioma:** Português do Brasil (PT-BR) estrito em toda documentação e texto in-world.
- **Proibições Sintáticas:** Proibido usar gerúndio lusitano (`a fazer`, `a dizer`, `a estar`). Use sempre gerúndio brasileiro (`fazendo`, `dizendo`, `estando`). Proibido vocabulário PT-PT (`facto`, `equipa`, `ecrã`, `utilizador`, `contacto`).
- **Zero AI-Speak:** Nunca comece documentos ou fragmentos com frases de assistente. Escreva com tom de Hard Sci-Fi físico e sensorial (cheiro de amônia, nota de 50 Hz no piso, frio de 38 K que estilhaça aço comum).

---

## 3. Regras do Vault Obsidian (`studio/convencoes.md`)

- **Frontmatter Obrigatório:** Toda nota em `jogos/parallax/` deve conter:
  ```yaml
  ---
  jogo: parallax
  tipo: <fundacao|cosmologia|historia|sociedade|misterio|termo|faccao|classe|auxiliar|item|bioma|transporte|quest|raid|encontro|interface|pessoa|fragmento|fio|verdade>
  status: <vazia|semente|hipótese|cânone|isca>
  camada: <base|live|restrito>
  liga: ["[[nome-do-arquivo]]"]
  ---
  ```
- **Unicidade de Nomes e Wikilinks:** Todo arquivo `.md` em `jogos/parallax/` tem nome único em minúsculas, sem acento e com hífen. Todo link `[[...]]` no campo `liga` ou no corpo do texto deve apontar para um arquivo real existente.
- **Segurança de Segredos (`restrito`):** Notas com `camada: restrito` (em `jogos/parallax/restrito/`) guardam a verdade interna do estúdio e **NUNCA** podem vazar para fragmentos in-world (`jogos/parallax/fragmentos/`) ou textos de interface do jogador.
- **Isca vs. Cânone:** `status: isca` e `status: cânone` nunca se misturam no mesmo arquivo.

---

## 4. Regras de Engenharia Godot 4.4+ e Pipeline (`utility-belt-tecnologias.md`)

- **Motor e Física:** Godot 4.4+ com **Jolt Physics** 3D nativo, câmera ortográfica 2.5D, GDScript fortemente tipado (`var pressao: float = 140.0`) e arquitetura orientada a componentes (`ThermalNode.gd`, `BeatSequencer.gd`).
- **Rede:** Multiplayer cooperativo de 1 a 3 jogadores com Host Autoritativo via **GodotSteam** (`SteamMultiplayerPeer` sobre **Steam Datagram Relay**). Nunca crie dependência de servidores dedicados pagos.
- **Escalabilidade 1–3 Jogadores:** Todo encontro ou puzzle de ação simultânea deve suportar o sistema de **Ensaio e Fila de Comando** com os 3 Auxiliares (`Macaco`, `Pipa`, `Cartucho de Override`).
- **Orçamento Teto (US$ 2.000):** Nunca proponha bibliotecas, plugins pagos ou serviços de nuvem recorrentes fora do envelope aprovado em `jogos/parallax/fundacao/utility-belt-tecnologias.md`.

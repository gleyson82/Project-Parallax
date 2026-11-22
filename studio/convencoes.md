# Convenções do arquivo

Regras para quem abre uma sala nova no Project Daedalus. Os três pilares do estúdio estão no [README](../README.md) e valem para todo jogo. O tom e o idioma estão em [`guia-de-voz.md`](guia-de-voz.md).

## Abrir um jogo

1. Copie [`modelo/`](../modelo/README.md) para `jogos/<slug>/`.
2. O slug é minúsculo, sem acento e sem espaço. O título de produto pode ficar no idioma do jogo.
3. Preencha `fundacao/` (nome, contrato mecânico, escalabilidade) antes de escrever lore, facções, biomas ou quests.
4. Um fato entra no arquivo quando a mecânica precisa dele. Texto que não justifica uma restrição do contrato fica de fora.

## Frontmatter

Todo arquivo de lore abre com este bloco:

```yaml
---
jogo: parallax
tipo: bioma
status: hipótese
camada: base
liga: ["[[fosso-de-hefesto]]", "[[goliath]]"]
---
```

`tipo` aceita: `fundacao`, `cosmologia`, `historia`, `sociedade`, `misterio`, `termo`, `faccao`, `classe`, `auxiliar`, `item`, `bioma`, `transporte`, `quest`, `raid`, `encontro`, `interface`, `pessoa`, `fragmento`, `fio`, `verdade`.

### Status

| Status | Uso |
| --- | --- |
| `vazia` | A sala existe e ainda não guarda um fato. |
| `semente` | Só o que já foi decidido. Sem desenvolvimento. |
| `hipótese` | Proposta de escrita ainda não adotada. |
| `cânone` | Vale no jogo. |
| `isca` | Trilha falsa plantada para o jogador. Quem escreve vê este status. O fragmento in-world não avisa. |

`isca` e `cânone` não se misturam no mesmo arquivo. Uma verdade e a sua trilha falsa são dois arquivos.

### Camada

| Camada | Uso |
| --- | --- |
| `base` | Entra no jogo base. |
| `live` | Planta no jogo base, resolução em temporada ou DLC. |
| `restrito` | Verdade de estúdio. Nunca vira texto que o jogador lê. Mora só em `restrito/`. |

## Arquivos

- **Nome de arquivo único no vault.** Links do Obsidian resolvem pelo nome: `krios-prime.md`, nunca `ficha.md`.
- Minúsculas, com hífen, sem acento: `deslocamento.md`.
- Um conceito por arquivo. Uma ficha curta pode dividir arquivo com o conceito pai.
- Fragmentos in-world moram em `fragmentos/`, separados pelo ponto de vista que os lê. Um fragmento pertence a um único olhar.
- O glossário guarda o sentido de trabalho do termo. A definição que o jogador encontra é um fragmento, e pode divergir.
- Decisões que mudam o cânone ficam em [`decisoes/`](decisoes/), uma por arquivo, numeradas.

# Modelo de jogo

Planta vazia de um jogo no Project Daedalus. Para abrir um jogo, copie esta pasta para `jogos/<slug>/` e troque os campos `{{...}}`.

As regras estão em [`studio/convencoes.md`](../studio/convencoes.md) e o tom em [`studio/guia-de-voz.md`](../studio/guia-de-voz.md).

## Ordem de escrita

1. `fundacao/` — nome, contrato mecânico, escalabilidade (1 a N jogadores) e matriz de paralaxe.
2. `lore/` — cosmologia, história, sociedade, mistérios e glossário.
3. `faccoes/`, `classes/`, `itens/`, `biomas/` — só quando a mecânica pedir.
4. `quests/`, `raids/`, `interface/` — o jogo em sessão.
5. `fios-soltos/` e `restrito/` — o que se planta e o que se guarda.

```
modelo/
├── fundacao/      nome, contrato, escalabilidade, matriz de paralaxe
├── lore/          cosmologia, historia, sociedade, misterios, glossario
├── faccoes/       quem quer o quê
├── classes/       olhares e auxiliares
├── itens/         grau-zero, armamento, consumíveis, recursos
├── biomas/        level design e transporte
├── quests/        arco de onboarding e quests
├── raids/         raids e encontros
├── interface/     menu, mapa de missões, áudio
├── pessoas/       quem fala, quem guarda
├── fragmentos/    texto que o jogador encontra (um olhar por pasta)
├── fios-soltos/   plantas para temporadas e DLC
└── restrito/      verdades de estúdio, nunca viram texto de jogo
```

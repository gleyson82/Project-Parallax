---
jogo: parallax
tipo: interface
status: cânone
camada: base
liga: ["[[menu-principal]]", "[[ficha-de-incursao]]", "[[audio-e-feedback]]", "[[matriz-de-paralaxe]]", "[[os-tres-relogios]]"]
---

# O Quadro de Válvulas (o Diretor de Tritão)

O mapa de missões é um **esquema de dutos de Tritão**, riscado em chapa de metal. Cada missão é uma válvula. O jogador não escolhe um ícone: escolhe qual válvula abrir.

## O que o jogador vê

Uma chapa grande sob luz de caldeira, quase preta. Linhas de duto em cobre e ferrugem ligam cinco regiões:

- **[[krios-prime]]** no alto, invertida, em violeta.
- **[[fosso-de-hefesto]]** embaixo, em vermelho-ferrugem.
- **[[estilhacos]]** em um anel externo, em ciano-gelo.
- **[[agulha-do-zenite]]** em um raio que sobe, em branco.
- **[[cemiterio-das-maquinas]]** apagado, só contorno, até ser liberado.

| Forma da válvula | Tipo |
| --- | --- |
| Volante simples | Quest |
| Volante duplo | Raid |
| Válvula em cruz, vermelha | Incursão de Sangria |
| Trilho | Escolta |

A cor de cada duto mostra o calor corrente, com hachura de reforço para daltonismo. Setores em Apagão ficam escuros e cobertos de geada.

## Camadas de leitura (a paralaxe do menu)

Três camadas deslizam umas sobre as outras com profundidade real. A paralaxe revela o que cada olhar enxerga.

| Camada | Olhar | O que mostra |
| --- | --- | --- |
| **Física** | [[goliath-olhar]] | Pressão, calor, esforço das válvulas |
| **Espacial** | [[phantom-olhar]] | Relevo, rotas verticais, acessos |
| **Lógica** | [[cipher-olhar]] | Rede, Faróis, Nervos, fraturas |

Em sessão cooperativa, cada jogador vê **a sua camada por padrão** e precisa conversar para decidir. Em sessão solo, o jogador alterna à vontade.

## Navegação

- O direcional gira o cursor entre válvulas, com um clique seco em cada nó.
- Segurar confirmar gira o volante e abre a [[ficha-de-incursao]].
- O ping de equipe marca um nó para os outros jogadores.
- Em cooperativo, cada jogador tem um cursor de cor própria.
- Para lançar, todos os presentes seguram confirmar ao mesmo tempo (**Três Mãos**). Em sessão menor, a Mão vazia aparece como o Auxiliar correspondente, já pronto.

## Estado vivo

- Válvulas abertas ficam quentes, e as cidades associadas ganham luz.
- Uma Noite de Sal escurece um setor por um tempo.
- Três **Relógios do Fim** ocupam os cantos: o oficial desde o início, e os da Cifra e do Fosso à medida que o jogador os acha. Ver [[os-tres-relogios]].

## Wireframe

```text
┌────────────────────────────────────────────────────────────────────┐
│ [RELÓGIO OFICIAL 140a]  [RELÓGIO CIFRA --]  [RELÓGIO FOSSO --]      │
│                                                                    │
│                      ◈ KRIOS-PRIME (violeta, invertida)            │
│                         │                                          │
│       ⊕ ESTILHAÇOS ─────┼───── ◎ AGULHA DO ZÊNITE                  │
│       (ciano, anel)     │       (branco, raio)                     │
│                         │                                          │
│                      ⊗ FOSSO DE HEFESTO (vermelho)                 │
│                                                                    │
│                      ░ CEMITÉRIO DAS MÁQUINAS (apagado)            │
│                                                                    │
│ [FÍSICA] [ESPACIAL] [LÓGICA]         ● ● ●  três mãos para lançar │
└────────────────────────────────────────────────────────────────────┘
```

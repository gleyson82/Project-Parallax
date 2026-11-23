---
jogo: parallax
tipo: fundacao
status: cânone
camada: base
liga: ["[[contrato-mecanico]]", "[[utility-belt-tecnologias]]", "[[blueprint-de-investimento]]", "[[matriz-de-paralaxe]]", "[[escalabilidade-1-a-3]]", "[[quadro-de-valvulas]]"]
---

# 0005 — 3D real na Unity 6 LTS

**Problema.** A ação lateral em 2.5D prendia a leitura do espaço a um único eixo. A informação assimétrica rende mais quando cada classe ocupa o espaço de um jeito: o Goliath no chão, o Phantom nas vigas e o Cipher dentro da rede. O estúdio também quer um motor que os agentes de IA operem de ponta a ponta (CLI, MCP, testes em lote).

**Decisão.**

| Tema | Pilha adotada |
| --- | --- |
| Perspectiva | Ação tática 3D em **3ª pessoa**, com **1ª pessoa contextual** em leitura de manômetro, mira e terminal |
| Motor | **Unity 6000.3 LTS** + URP 17.3 + PhysX |
| Rede | **Netcode for GameObjects 2.x** + **Steamworks.NET** sobre Steam Datagram Relay, host autoritativo |
| Olhares | 3 Universal Renderers com Full Screen Pass, um por classe |
| Cenários | ProBuilder + kits do Blender |
| Áudio | Unity Audio Mixer; Batida agendada em `dspTime` |
| Cooperativo | **Online primeiro** (Steam e Remote Play Together); tela dividida local vira marco futuro |

**O que não muda.** Toda a lore, a cronologia, as facções, as corporações, as quests e as raids. As três classes, os três Auxiliares, o Ensaio e Fila de Comando, a Chave Tripla e a Matriz de Paralaxe. O teto de US\$ 2.000, com Cursor Pro e Antigravity já quitados nos meses 1 a 6. A regra de que nenhum ativo bruto de IA entra no jogo sem edição humana.

**Risco aceito.** A Unity Personal é gratuita até US\$ 200 mil de receita anual. Acima disso, a licença Pro passa a ser custo pago pela receita do jogo, e não pelo orçamento de produção.

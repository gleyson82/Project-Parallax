---
name: parallax-godot-engineer
description: >-
  Projeta e implementa cenas (.tscn), scripts GDScript (.gd), shaders de visão assimétrica
  (.gdshader), física Jolt, termodinâmica e multiplayer GodotSteam para o jogo PARALLAX no Godot 4.4+.
  Ative sempre que o usuário pedir código do jogo, protótipos de mecânicas, UI do Quadro de Válvulas ou puzzles.
---

# Skill: PARALLAX Godot 4.4+ Engineer

Use esta skill para construir os sistemas de gameplay de **PARALLAX** seguindo a pilha de custo zero definida em `jogos/parallax/fundacao/utility-belt-tecnologias.md`.

## Arquitetura Padrão dos Sistemas Core

1. **Câmera 2.5D e Movimento (`Camera3D` Ortográfica):**
   - Ação lateral em planos 3D com `PROJECTION_ORTHOGONAL`.
   - O **Goliath** opera no plano pesado (chão/ancoragem), o **Phantom** usa cabos (`PinJoint3D`) para alcançar vigas superiores e o **Cipher** alterna entre o plano físico e a interface 2D isolada de Override.
2. **Componente Termodinâmico (`ThermalNode.gd`):**
   - Gerencia temperatura em Kelvin (ambiente externo a `38.0 K` / `-235 °C`).
   - Controla superaquecimento das armas **Therma-Tek**, congelamento de atuadores pelas armas **Kinesys**, decaimento de `2%` ao dia das Células Térmicas e o **Choque Térmico** (aquecer blindagem a vermelho via Override do Cipher + estilhaçar com tiro criogênico do Phantom).
3. **Sequenciador da Batida (`BeatSequencer.gd`):**
   - Dispara pulsos acústicos nos dutos sincronizados ao `_physics_process` para os puzzles da **Batida dos Foguistas** e para o metrônomo visual de acessibilidade.
4. **Escalabilidade de 1 a 3 Jogadores (`AuxiliaryController.gd`):**
   - Implementa a **Janela de Ensaio** e a **Fila de Comando** para os 3 Auxiliares (**Macaco**, **Pipa**, **Cartucho de Override**), permitindo que 1 ou 2 jogadores resolvam os puzzles finais de 3 ações simultâneas.
5. **Rede Cooperativa (`SteamLobbyManager.gd`):**
   - Host autoritativo usando **GodotSteam** + `SteamMultiplayerPeer` sobre **Steam Datagram Relay** (custo zero de servidor).

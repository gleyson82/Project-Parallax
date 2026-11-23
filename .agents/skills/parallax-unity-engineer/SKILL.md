---
name: parallax-unity-engineer
description: >-
  Projeta e implementa C#, cenas, prefabs, ScriptableObjects, shaders URP dos 3 Olhares, física PhysX,
  termodinâmica e multiplayer NGO + Steam (SDR) do jogo PARALLAX em Unity 6 LTS (client/parallax-unity).
  Ative sempre que o usuário pedir código do jogo, protótipos de mecânicas, puzzles, Quadro de Válvulas,
  rede, câmera ou testes.
---

# Skill: PARALLAX Unity 6 LTS Engineer

Use esta skill para construir os sistemas de **PARALLAX** em `client/parallax-unity/`, seguindo `AGENTS.md` e a pilha de custo zero de `jogos/parallax/fundacao/utility-belt-tecnologias.md`.

## Antes de escrever código

1. Leia a nota de lore que justifica a mecânica (`contrato-mecanico.md`, `matriz-de-paralaxe.md`, `escalabilidade-1-a-3.md`, a quest ou raid).
2. Ache o sistema existente: `Parallax.Core` (modelos puros), `Parallax.Gameplay` (NetworkBehaviours), `Parallax.Rendering`, `Parallax.UI`, `Parallax.Networking`, `Parallax.Editor`.
3. Regra pura e testável vai para `Core` com teste em `Tests/EditMode`. Só depois vem o componente Unity.

## Sistemas existentes (reutilize, não duplique)

| Necessidade | Use |
| --- | --- |
| Calor, congelamento, Choque Térmico, Célula Térmica | `ThermalModel` (Core) + `ThermalNode` (rede, tick via `ThermalSystem`) + `ThermalProfile` (SO) |
| A Batida e a Nota do Piso | `BeatSchedule`/`FloorNote` (Core) + `BeatSequencer` (`PlayScheduled` em `dspTime`) + `FloorNoteSynth` |
| Quem joga qual classe | `SessionRoster` (servidor; revalida o remetente) |
| Ensaio e Fila de Comando | `CommandQueueModel` (Core) + `CommandQueueNetwork` + `IRehearsableAction` |
| Três ações simultâneas | `MemoryWindow` + `TripleLockPuzzle` + `LockActuator` |
| Três Mãos | `TriHandsConsensus` (Core) + `TriHandsLaunch` |
| Olhares | `VisionController.SetViewer` + `ParallaxVisionState` (globais `_Parallax_*`) |
| Câmera 3ª/1ª pessoa | `ParallaxCameraRig.RequestFirstPerson(this)` / `Release(this)` + `CameraProfile` |
| Movimento | `ParallaxCharacterMotor` (dono) + `GoliathAnchor`, `PhantomGrapple`, `CipherOverride` |
| Mundo | `HeavyDoor` (10 t), `TensionCable`, `GrapplePoint`, `LiveLine` (Fogo Mínimo), `OverrideTerminal`, `OverrideDrone` |
| Entrada | `ParallaxInput` (`Settings/Input/ParallaxControls.inputactions`, cópia por instância) |
| Jogador | Prefab `Prefabs/Characters/ParallaxPlayer` (3 classes num prefab): `PlayerClassBinder` (classe ↔ modelo, habilidade, cápsula, câmera, Olhar), `PlayerLook`, `CharacterAnimator` (Playables, sem AnimatorController) |
| Laboratório jogável | Cena `Scenes/Labs/Lab_Jogavel` + `DevSessionMenu`; regerar com `Parallax/Bootstrap/Playable Lab`; teste `Tests/PlayMode/PlayableLabTests` |

## Regras de implementação

- Sem lógica de jogo em `Update`; use eventos, RPCs, corrotinas ou `NetworkTickSystem.Tick`.
- RPC: `[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]` com `RpcParams` e revalidação no servidor.
- Balanceamento sempre em ScriptableObject; um tipo por arquivo com o mesmo nome.
- Todo puzzle de três pontas funciona com 1, 2 e 3 humanos.
- Shaders de tela cheia: HLSL com `Blit.hlsl`, registrados em um renderer de `Parallax_URP` (0 Neutro, 1 Goliath, 2 Phantom, 3 Cipher).

## Validação obrigatória (nesta ordem)

1. `unity recompile --project-path client/parallax-unity` → 0 erros.
2. `unity test --project-path client/parallax-unity` (EditMode e PlayMode) → 100% verde.
   - Sem editor aberto: `Unity.exe -batchmode -projectPath client/parallax-unity -runTests -testPlatform EditMode -testResults Logs/TestResults-EditMode.xml` (repita com `-testPlatform PlayMode`).
   - NetworkObjects de cena criados por script exigem cena salva e reaberta (`PlayableLabBuilder.RefreshInSceneNetworkObjects`), senão o NGO não os reconhece.
3. Mudou rede? Teste no Multiplayer Play Mode com 2 e 3 instâncias.
4. Mudou lore? `python studio/scripts/validate_daedalus.py`.

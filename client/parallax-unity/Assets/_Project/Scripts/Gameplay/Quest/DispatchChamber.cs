using System;
using Parallax.Core.Commands;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Puzzles;
using Parallax.Gameplay.World;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>
    /// Câmara de Despacho da Estação 14 (Quest 1, objetivo 1): o Goliath ergue a porta de chumbo, o Phantom abre a
    /// sangria superior e o Cipher destrava o terminal, todos dentro da Janela de Memória (20/30/40 s).
    /// Papéis sem humano são cobertos por Auxiliares marcados no ensaio e confirmados na Fila de Comando.
    /// </summary>
    public sealed class DispatchChamber : NetworkBehaviour
    {
        [SerializeField] private TripleLockPuzzle puzzle;
        [SerializeField] private HeavyDoor door;
        [SerializeField] private OverrideTerminal terminal;
        [SerializeField] private CommandQueueNetwork commandQueue;

        public event Action ServerOpened;

        /// <summary>Todos os clientes: a janela fechou sem as três travas.</summary>
        public static event Action WindowExpired;

        public TripleLockPuzzle Puzzle => puzzle;
        public CommandQueueNetwork Queue => commandQueue;
        public bool IsOpen => puzzle != null && puzzle.IsOpen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() => WindowExpired = null;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            door.SupportChanged += OnDoorSupportChanged;
            terminal.BreachedChanged += OnTerminalBreached;
            puzzle.Opened += OnPuzzleOpened;
            puzzle.ServerWindowExpired += OnWindowExpired;
            foreach (ParallaxClass role in new[] { ParallaxClass.Goliath, ParallaxClass.Phantom, ParallaxClass.Cipher })
            {
                LockActuator actuator = puzzle.LockFor(role);
                actuator.ServerActivated += OnLockActivated;
                actuator.ServerAuxiliaryStarted += OnAuxiliaryStarted;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer)
            {
                return;
            }

            door.SupportChanged -= OnDoorSupportChanged;
            terminal.BreachedChanged -= OnTerminalBreached;
            puzzle.Opened -= OnPuzzleOpened;
            puzzle.ServerWindowExpired -= OnWindowExpired;
            foreach (ParallaxClass role in new[] { ParallaxClass.Goliath, ParallaxClass.Phantom, ParallaxClass.Cipher })
            {
                LockActuator actuator = puzzle.LockFor(role);
                actuator.ServerActivated -= OnLockActivated;
                actuator.ServerAuxiliaryStarted -= OnAuxiliaryStarted;
            }
        }

        private void OnDoorSupportChanged(bool supported)
        {
            // Sem Goliath humano, quem sustenta é o Macaco; a trava é acionada pela rotina do Auxiliar.
            bool humanGoliath = Session.SessionRoster.Active != null && Session.SessionRoster.Active.IsHuman(ParallaxClass.Goliath);
            if (supported && humanGoliath && !door.IsLatched && !puzzle.IsOpen)
            {
                puzzle.LockFor(ParallaxClass.Goliath).ServerEngage();
            }
        }

        private void OnTerminalBreached(bool breached)
        {
            if (breached && !puzzle.IsOpen)
            {
                puzzle.LockFor(ParallaxClass.Cipher).ServerEngage();
            }
        }

        private void OnLockActivated(LockActuator actuator)
        {
            LockFx(actuator.transform.position);
            if (!actuator.EngagedByAuxiliary && commandQueue != null && commandQueue.Phase == CommandQueuePhase.Confirmed)
            {
                puzzle.ServerBeginFinalAction();
            }
        }

        private void OnAuxiliaryStarted(LockActuator actuator)
        {
            if (actuator.Role == ParallaxClass.Goliath)
            {
                door.ExecuteAsAuxiliary(AuxiliaryId.Macaco, null);
            }
        }

        private void OnPuzzleOpened()
        {
            door.ServerLatchOpen();
            OpenedFxRpc(door.transform.position);
            ServerOpened?.Invoke();
        }

        private void OnWindowExpired()
        {
            terminal.ServerSetBreached(false);
            if (commandQueue != null)
            {
                commandQueue.ServerRehearse();
            }

            WindowExpiredRpc();
            bool humanGoliath = Session.SessionRoster.Active != null && Session.SessionRoster.Active.IsHuman(ParallaxClass.Goliath);
            if (humanGoliath && door.IsSatisfied && !door.IsLatched)
            {
                puzzle.LockFor(ParallaxClass.Goliath).ServerEngage();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LockFxRpc(Vector3 position) => AudioCues.Play(AudioCue.LockEngage, position);

        private void LockFx(Vector3 position) => LockFxRpc(position);

        [Rpc(SendTo.ClientsAndHost)]
        private void OpenedFxRpc(Vector3 position) => AudioCues.Play(AudioCue.DoorRumble, position);

        [Rpc(SendTo.ClientsAndHost)]
        private void WindowExpiredRpc()
        {
            AudioCues.Play2D(AudioCue.WindowExpired);
            WindowExpired?.Invoke();
        }
    }
}

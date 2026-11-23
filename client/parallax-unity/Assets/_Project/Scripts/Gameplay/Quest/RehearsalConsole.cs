using Parallax.Core.Commands;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Interaction;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>Painel da Fila de Comando: confirma as marcas do ensaio. Nada acontece antes da confirmação.</summary>
    public sealed class RehearsalConsole : NetworkInteractable
    {
        [SerializeField] private CommandQueueNetwork commandQueue;

        public override string GetPrompt(ParallaxClass viewer)
        {
            if (commandQueue == null || commandQueue.Count == 0 || commandQueue.Phase != CommandQueuePhase.Rehearsal)
            {
                return string.Empty;
            }

            return $"Confirmar a Fila de Comando ({commandQueue.Count} comando{(commandQueue.Count > 1 ? "s" : string.Empty)})";
        }

        public override void OnLocalInteract(ParallaxClass viewer) => AudioCues.Play(AudioCue.LockEngage, FocusPosition);

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player) => commandQueue.ServerConfirm();
    }
}

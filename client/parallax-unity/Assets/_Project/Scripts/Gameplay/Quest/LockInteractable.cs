using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Puzzles;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>
    /// Comando físico de uma trava da Chave Tripla. O humano da classe certa aciona (quando a ação é com Interagir);
    /// se o papel não tem humano, qualquer operador marca aqui o Auxiliar correspondente (Janela de Ensaio).
    /// </summary>
    public sealed class LockInteractable : NetworkInteractable
    {
        [SerializeField] private LockActuator lockActuator;
        [SerializeField] private CommandQueueNetwork commandQueue;
        [SerializeField] private string humanActionLabel = "Abrir a sangria superior";
        [SerializeField, Tooltip("Quando a classe dona age por habilidade (Q), Interagir não aciona a trava.")]
        private bool humanActsViaAbility;

        public LockActuator Lock => lockActuator;

        public override string GetPrompt(ParallaxClass viewer)
        {
            if (lockActuator == null || lockActuator.IsSatisfied)
            {
                return string.Empty;
            }

            SessionRoster roster = SessionRoster.Active;
            ParallaxClass role = lockActuator.Role;
            if (viewer == role)
            {
                return humanActsViaAbility ? string.Empty : humanActionLabel;
            }

            if (roster != null && roster.IsHuman(role))
            {
                return string.Empty;
            }

            AuxiliaryId auxiliary = ParallaxRoles.AuxiliaryFor(role);
            if (commandQueue != null && commandQueue.IsMarked(auxiliary))
            {
                return string.Empty;
            }

            return "Ensaio · marcar " + AuxiliaryName(auxiliary) + " nesta trava";
        }

        public override void OnLocalInteract(ParallaxClass viewer) => AudioCues.Play(AudioCue.UiTick, FocusPosition);

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            ParallaxClass role = lockActuator.Role;
            if (parallaxClass == role)
            {
                if (!humanActsViaAbility)
                {
                    lockActuator.ServerEngage();
                }

                return;
            }

            SessionRoster roster = SessionRoster.Active;
            if (roster != null && !roster.IsHuman(role) && commandQueue != null)
            {
                commandQueue.ServerMark(ParallaxRoles.AuxiliaryFor(role), lockActuator.RehearsalTargetId);
            }
        }

        public static string AuxiliaryName(AuxiliaryId auxiliary)
        {
            switch (auxiliary)
            {
                case AuxiliaryId.Macaco: return "o Macaco";
                case AuxiliaryId.Pipa: return "a Pipa";
                default: return "o Cartucho de Override";
            }
        }
    }
}

using System;
using Parallax.Core.Definitions;
using Parallax.Core.Quest;
using Parallax.Core.Roles;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Interaction
{
    /// <summary>
    /// Planta de lore inspecionável: um fragmento do Daedalus (legível só pelo olhar dono dele) ou uma
    /// inscrição física curta. Ler conta como objetivo secundário quando configurado.
    /// </summary>
    public sealed class LoreInspectable : NetworkInteractable
    {
        [SerializeField] private string title = "Inscrição";
        [SerializeField] private LoreFragment fragment;
        [SerializeField, TextArea] private string inscription;
        [SerializeField] private Quest01Secondary secondary = Quest01Secondary.None;

        /// <summary>Local: o jogador abriu a inspeção (a HUD mostra o texto).</summary>
        public static event Action<LoreInspectable, ParallaxClass> LocalInspected;

        /// <summary>Servidor: um operador com o olhar certo leu a planta.</summary>
        public static event Action<LoreInspectable, ParallaxClass> ServerRead;

        public string Title => title;
        public LoreFragment Fragment => fragment;
        public string Inscription => inscription;
        public Quest01Secondary Secondary => secondary;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents()
        {
            LocalInspected = null;
            ServerRead = null;
        }

        public bool IsReadableBy(ParallaxClass viewer) => fragment == null || fragment.Viewer == viewer;

        public override string GetPrompt(ParallaxClass viewer) => "Inspecionar · " + title;

        public override void OnLocalInteract(ParallaxClass viewer) => LocalInspected?.Invoke(this, viewer);

        protected override void OnServerInteract(ulong clientId, ParallaxClass parallaxClass, NetworkObject player)
        {
            if (IsReadableBy(parallaxClass))
            {
                ServerRead?.Invoke(this, parallaxClass);
            }
        }
    }
}

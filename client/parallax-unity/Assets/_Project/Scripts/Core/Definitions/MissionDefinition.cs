using Parallax.Core.Roles;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Mission", fileName = "Mission_")]
    public sealed class MissionDefinition : ParallaxDefinition
    {
        [SerializeField] private MissionKind kind;
        [SerializeField] private CorporationId corporation;
        [SerializeField] private bool corporationIsCifra;
        [SerializeField, Tooltip("Mostra a Marca de Quatro Hastes, discreta, na ficha.")]
        private bool showsFourProngMark;
        [SerializeField] private string radioSpeaker = "candeia";
        [SerializeField] private string biomeKey;
        [SerializeField] private string briefingKey;
        [SerializeField] private string readingGoliathKey;
        [SerializeField] private string readingPhantomKey;
        [SerializeField] private string readingCipherKey;
        [SerializeField] private string rewardKey;
        [SerializeField] private string sceneName;
        [SerializeField, Range(0f, 1f), Tooltip("Calor do setor; define a altura da Nota do Piso no Quadro.")]
        private float sectorHeat = 0.5f;

        public MissionKind Kind => kind;
        public CorporationId Corporation => corporation;
        public bool CorporationIsCifra => corporationIsCifra;
        public bool ShowsFourProngMark => showsFourProngMark;
        public string RadioSpeaker => radioSpeaker;
        public string BiomeKey => biomeKey;
        public string BriefingKey => briefingKey;
        public string RewardKey => rewardKey;
        public string SceneName => sceneName;
        public float SectorHeat => sectorHeat;

        public string ReadingKeyFor(ParallaxClass viewer)
        {
            switch (viewer)
            {
                case ParallaxClass.Goliath: return readingGoliathKey;
                case ParallaxClass.Phantom: return readingPhantomKey;
                default: return readingCipherKey;
            }
        }

#if UNITY_EDITOR
        public void EditorSetup(MissionKind newKind, CorporationId newCorporation, bool cifra, bool fourProng, string speaker, string scene, float heat)
        {
            kind = newKind;
            corporation = newCorporation;
            corporationIsCifra = cifra;
            showsFourProngMark = fourProng;
            radioSpeaker = speaker;
            sceneName = scene;
            sectorHeat = heat;
            biomeKey = Id + ".biome";
            briefingKey = Id + ".briefing";
            readingGoliathKey = Id + ".reading.goliath";
            readingPhantomKey = Id + ".reading.phantom";
            readingCipherKey = Id + ".reading.cipher";
            rewardKey = Id + ".reward";
        }
#endif
    }
}

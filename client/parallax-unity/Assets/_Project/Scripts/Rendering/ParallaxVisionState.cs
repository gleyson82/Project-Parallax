using UnityEngine;

namespace Parallax.Rendering
{
    /// <summary>
    /// Variáveis globais de shader dos 3 Olhares. Sistemas de jogo escrevem aqui; os shaders de tela cheia leem.
    /// Valores são por máquina: cada jogador só vê o próprio Olhar.
    /// </summary>
    public static class ParallaxVisionState
    {
        public const int OmegaTremorLeakThreshold = 3;
        public const int MaxLeakCount = 5;

        public static readonly int LeakCountId = Shader.PropertyToID("_Parallax_LeakCount");
        public static readonly int SeismicPulseId = Shader.PropertyToID("_Parallax_SeismicPulse");
        public static readonly int OverrideBlindId = Shader.PropertyToID("_Parallax_OverrideBlind");
        public static readonly int FrostProgressId = Shader.PropertyToID("_Parallax_FrostProgress");
        public static readonly int SectorHeatId = Shader.PropertyToID("_Parallax_SectorHeat");

        public static int LeakCount { get; private set; }
        public static float SeismicPulse { get; private set; }
        public static bool OverrideBlind { get; private set; }
        public static float FrostProgress { get; private set; }

        public static bool OmegaTremorActive => LeakCount >= OmegaTremorLeakThreshold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SetLeakCount(0);
            SetSeismicPulse(0f);
            SetOverrideBlind(false);
            SetFrostProgress(0f);
            SetSectorHeat(0.5f);
        }

        /// <summary>Contagem de Vazamento do Código-Fonte Ômega (0–5). Em 3+, dispara o Tremor Vermelho.</summary>
        public static void SetLeakCount(int count)
        {
            LeakCount = Mathf.Clamp(count, 0, MaxLeakCount);
            Shader.SetGlobalFloat(LeakCountId, LeakCount);
        }

        /// <summary>Pulso sísmico da Batida no Olhar do Goliath (0–1).</summary>
        public static void SetSeismicPulse(float intensity)
        {
            SeismicPulse = Mathf.Clamp01(intensity);
            Shader.SetGlobalFloat(SeismicPulseId, SeismicPulse);
        }

        /// <summary>Override Cego: o Cipher perde a leitura do mundo físico.</summary>
        public static void SetOverrideBlind(bool blind)
        {
            OverrideBlind = blind;
            Shader.SetGlobalFloat(OverrideBlindId, blind ? 1f : 0f);
        }

        /// <summary>Geada que avança pelas bordas do Quadro de Válvulas com a inatividade (0–1).</summary>
        public static void SetFrostProgress(float progress)
        {
            FrostProgress = Mathf.Clamp01(progress);
            Shader.SetGlobalFloat(FrostProgressId, FrostProgress);
        }

        public static void SetSectorHeat(float normalizedHeat) => Shader.SetGlobalFloat(SectorHeatId, Mathf.Clamp01(normalizedHeat));
    }
}

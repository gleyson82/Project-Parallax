using System;
using Parallax.Core.Session;

namespace Parallax.Core.Quest
{
    /// <summary>Etapas de Calor de Contrabando (quest-01-calor-de-contrabando.md), em ordem.</summary>
    public enum Quest01Stage : byte
    {
        Briefing = 0,
        OpenDispatchChamber = 1,
        DeliverCargo = 2,
        OpenValve = 3,
        HoldTheLine = 4,
        Complete = 5
    }

    [Flags]
    public enum Quest01Secondary : byte
    {
        None = 0,
        FogoMinimo = 1 << 0,
        Manometro = 1 << 1,
        Relogio = 1 << 2,
        Etiqueta = 1 << 3
    }

    /// <summary>Números da quest que mudam com o tamanho da sessão.</summary>
    [Serializable]
    public struct Quest01Tuning
    {
        public float DispatchWindowBaseSeconds;
        public float HoldTheLineSeconds;
        public int[] WaveSizes;

        public static Quest01Tuning Canon => new Quest01Tuning
        {
            DispatchWindowBaseSeconds = 20f,
            HoldTheLineSeconds = 75f,
            WaveSizes = new[] { 3, 4, 5 }
        };

        /// <summary>20 s com 3 humanos, 30 s com 2, 40 s solo (mesmos multiplicadores da escalabilidade).</summary>
        public float DispatchWindowSeconds(int humans, SessionScalingTable table) =>
            DispatchWindowBaseSeconds * SessionScaling.ActionWindowMultiplier(humans, table);

        /// <summary>Solo enfrenta uma onda a menos; o tamanho das ondas segue o multiplicador de inimigos.</summary>
        public int[] WavesFor(int humans, SessionScalingTable table)
        {
            int count = SessionScaling.ClampPlayers(humans) == 1 ? Math.Max(1, WaveSizes.Length - 1) : WaveSizes.Length;
            float multiplier = SessionScaling.EnemyMultiplier(humans, table);
            var waves = new int[count];
            for (int i = 0; i < count; i++)
            {
                waves[i] = Math.Max(1, (int)Math.Ceiling(WaveSizes[i] * multiplier));
            }

            return waves;
        }
    }

    public enum Quest01Rating : byte
    {
        Bronze = 0,
        Prata = 1,
        Ouro = 2
    }

    /// <summary>
    /// Máquina de estados pura da Quest 1. O servidor é dono do progresso; a interface só lê.
    /// </summary>
    public sealed class Quest01Progress
    {
        public Quest01Stage Stage { get; private set; } = Quest01Stage.Briefing;
        public Quest01Secondary Secondary { get; private set; } = Quest01Secondary.FogoMinimo;
        public double StartedAt { get; private set; } = -1;
        public double CompletedAt { get; private set; } = -1;
        public int CoresCollected { get; private set; }
        public float CoreChargeSum { get; private set; }

        public bool IsComplete => Stage == Quest01Stage.Complete;
        public float AverageCoreCharge => CoresCollected == 0 ? 0f : CoreChargeSum / CoresCollected;

        public void Begin(double time)
        {
            if (Stage != Quest01Stage.Briefing)
            {
                return;
            }

            StartedAt = time;
            Stage = Quest01Stage.OpenDispatchChamber;
        }

        /// <summary>Avança só se a etapa atual for a esperada, para eventos fora de ordem não pularem a quest.</summary>
        public bool CompleteStage(Quest01Stage expected, double time)
        {
            if (Stage != expected || Stage == Quest01Stage.Complete || Stage == Quest01Stage.Briefing)
            {
                return false;
            }

            Stage = (Quest01Stage)((byte)Stage + 1);
            if (Stage == Quest01Stage.Complete)
            {
                CompletedAt = time;
            }

            return true;
        }

        public bool Mark(Quest01Secondary flag)
        {
            if (flag == Quest01Secondary.FogoMinimo || (Secondary & flag) != 0)
            {
                return false;
            }

            Secondary |= flag;
            return true;
        }

        /// <summary>Uma Linha Viva rompida apaga o bônus Fogo Mínimo de vez.</summary>
        public bool BreakFogoMinimo()
        {
            if ((Secondary & Quest01Secondary.FogoMinimo) == 0)
            {
                return false;
            }

            Secondary &= ~Quest01Secondary.FogoMinimo;
            return true;
        }

        public void CollectCore(float charge01)
        {
            CoresCollected++;
            CoreChargeSum += Math.Min(1f, Math.Max(0f, charge01));
        }

        public double ElapsedSeconds(double now)
        {
            if (StartedAt < 0)
            {
                return 0;
            }

            return Math.Max(0, (CompletedAt >= 0 ? CompletedAt : now) - StartedAt);
        }

        public int SecondaryCount => CountSecondaries(Secondary);

        public Quest01Rating Rating => RatingFor(Secondary);

        public static int CountSecondaries(Quest01Secondary secondary)
        {
            int count = 0;
            for (int bits = (int)secondary; bits != 0; bits >>= 1)
            {
                count += bits & 1;
            }

            return count;
        }

        /// <summary>Ouro: os 4 secundários. Prata: Fogo Mínimo intacto ou 3 secundários. Bronze: o resto.</summary>
        public static Quest01Rating RatingFor(Quest01Secondary secondary)
        {
            int count = CountSecondaries(secondary);
            if (count >= 4)
            {
                return Quest01Rating.Ouro;
            }

            bool fogoMinimo = (secondary & Quest01Secondary.FogoMinimo) != 0;
            return fogoMinimo || count >= 3 ? Quest01Rating.Prata : Quest01Rating.Bronze;
        }
    }
}

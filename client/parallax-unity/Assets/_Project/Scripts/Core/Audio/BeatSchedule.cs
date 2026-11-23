using System;

namespace Parallax.Core.Audio
{
    /// <summary>
    /// Agenda matemática de "A Batida" dos Foguistas. Trabalha em segundos de relógio de áudio (dspTime),
    /// e não em ticks de física: a precisão fica na casa da amostra, e não em ±1 tick.
    /// </summary>
    public sealed class BeatSchedule
    {
        private readonly bool[] _accents;

        public BeatSchedule(double bpm, bool[] accents, double startTime, int subdivision = 2)
        {
            if (accents == null || accents.Length == 0)
            {
                throw new ArgumentException("O padrão da Batida precisa de ao menos um passo.", nameof(accents));
            }

            Bpm = Math.Max(1.0, bpm);
            Subdivision = Math.Max(1, subdivision);
            StartTime = startTime;
            _accents = (bool[])accents.Clone();
        }

        public double Bpm { get; }
        public int Subdivision { get; }
        public double StartTime { get; }
        public int PatternLength => _accents.Length;
        public double StepDuration => 60.0 / Bpm / Subdivision;

        public double TimeOfStep(long stepIndex) => StartTime + stepIndex * StepDuration;

        /// <summary>Índice do primeiro passo cujo horário é maior ou igual a <paramref name="time"/>.</summary>
        public long NextStepAtOrAfter(double time)
        {
            if (time <= StartTime)
            {
                return 0;
            }

            return (long)Math.Ceiling((time - StartTime) / StepDuration - 1e-9);
        }

        public int PatternIndex(long stepIndex)
        {
            long index = stepIndex % _accents.Length;
            return (int)(index < 0 ? index + _accents.Length : index);
        }

        public bool IsAccent(long stepIndex) => _accents[PatternIndex(stepIndex)];
    }

    /// <summary>A Nota do Piso (40–60 Hz): setor mais frio, nota mais grave (audio-e-feedback).</summary>
    public static class FloorNote
    {
        public const float MinFrequencyHz = 40f;
        public const float MaxFrequencyHz = 60f;

        /// <param name="normalizedHeat">0 = setor no frio de Tritão; 1 = setor no limite de calor.</param>
        public static float FrequencyFor(float normalizedHeat)
        {
            float t = normalizedHeat < 0f ? 0f : normalizedHeat > 1f ? 1f : normalizedHeat;
            return MinFrequencyHz + (MaxFrequencyHz - MinFrequencyHz) * t;
        }
    }
}

using System;
using Parallax.Core.Roles;

namespace Parallax.Core.Commands
{
    /// <summary>
    /// "Três Mãos" do Quadro de Válvulas: todos os humanos presentes seguram confirmar ao mesmo tempo.
    /// A Mão de um papel sem humano é o Auxiliar correspondente, já pronto.
    /// </summary>
    public sealed class TriHandsConsensus
    {
        public const double DefaultHoldSeconds = 0.4;

        private readonly bool[] _isHuman = new bool[ParallaxRoles.ClassCount];
        private readonly double[] _pressStart = new double[ParallaxRoles.ClassCount];
        private readonly bool[] _pressed = new bool[ParallaxRoles.ClassCount];

        public TriHandsConsensus(double holdSeconds = DefaultHoldSeconds)
        {
            HoldSeconds = Math.Max(0.05, holdSeconds);
        }

        /// <summary>Ajustável por acessibilidade ("tempo de segurar ajustável").</summary>
        public double HoldSeconds { get; set; }

        public void SetHuman(ParallaxClass seat, bool isHuman)
        {
            _isHuman[(int)seat] = isHuman;
            if (!isHuman)
            {
                _pressed[(int)seat] = false;
            }
        }

        public bool IsHuman(ParallaxClass seat) => _isHuman[(int)seat];

        public bool IsPressed(ParallaxClass seat) => _pressed[(int)seat];

        public void Press(ParallaxClass seat, double time)
        {
            int i = (int)seat;
            if (!_isHuman[i] || _pressed[i])
            {
                return;
            }

            _pressed[i] = true;
            _pressStart[i] = time;
        }

        public void Release(ParallaxClass seat)
        {
            _pressed[(int)seat] = false;
        }

        /// <summary>Progresso 0..1 da válvula mestra: limitado pela Mão que segurou por menos tempo.</summary>
        public float Progress(double time)
        {
            bool anyHuman = false;
            double minHeld = double.MaxValue;
            for (int i = 0; i < ParallaxRoles.ClassCount; i++)
            {
                if (!_isHuman[i])
                {
                    continue;
                }

                anyHuman = true;
                double held = _pressed[i] ? Math.Max(0.0, time - _pressStart[i]) : 0.0;
                minHeld = Math.Min(minHeld, held);
            }

            if (!anyHuman)
            {
                return 0f;
            }

            return (float)Math.Min(1.0, minHeld / HoldSeconds);
        }

        public bool IsComplete(double time) => Progress(time) >= 1f;

        public void ResetPresses()
        {
            for (int i = 0; i < _pressed.Length; i++)
            {
                _pressed[i] = false;
            }
        }
    }
}

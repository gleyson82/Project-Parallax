using System;

namespace Parallax.Core.Commands
{
    /// <summary>
    /// Janela de Memória: em sessões menores, uma ação ativada fica aberta por tempo limitado
    /// para que as três ações não precisem ser literalmente simultâneas.
    /// </summary>
    public sealed class MemoryWindow
    {
        private readonly double[] _activatedAt;
        private readonly bool[] _active;

        public MemoryWindow(int slots, double windowSeconds)
        {
            if (slots <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slots));
            }

            _activatedAt = new double[slots];
            _active = new bool[slots];
            WindowSeconds = windowSeconds;
        }

        public double WindowSeconds { get; set; }
        public int Slots => _active.Length;

        public void Activate(int slot, double time)
        {
            _active[slot] = true;
            _activatedAt[slot] = time;
        }

        public void Deactivate(int slot) => _active[slot] = false;

        public bool IsOpen(int slot, double time) => _active[slot] && time - _activatedAt[slot] <= WindowSeconds;

        public bool AllOpen(double time)
        {
            for (int i = 0; i < _active.Length; i++)
            {
                if (!IsOpen(i, time))
                {
                    return false;
                }
            }

            return true;
        }

        public int OpenCount(double time)
        {
            int count = 0;
            for (int i = 0; i < _active.Length; i++)
            {
                if (IsOpen(i, time))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Momento em que a ativação aberta mais antiga expira. Falso se nenhuma estiver aberta.</summary>
        public bool TryGetDeadline(double time, out double deadline)
        {
            deadline = double.MaxValue;
            for (int i = 0; i < _active.Length; i++)
            {
                if (IsOpen(i, time))
                {
                    deadline = Math.Min(deadline, _activatedAt[i] + WindowSeconds);
                }
            }

            return deadline < double.MaxValue;
        }

        public void Reset()
        {
            for (int i = 0; i < _active.Length; i++)
            {
                _active[i] = false;
            }
        }
    }
}

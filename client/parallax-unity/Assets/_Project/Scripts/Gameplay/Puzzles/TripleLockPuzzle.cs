using System;
using Parallax.Core.Commands;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Core.Session;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Puzzles
{
    /// <summary>
    /// Porta de Chave Tripla: três fechaduras (Therma-Tek, Kinesys, Omni-Cripto), três operadores de naturezas
    /// diferentes. Abre quando as três ativações estão abertas ao mesmo tempo dentro da Janela de Memória.
    /// A janela escala com os humanos (base, +50% em dupla, +100% solo). Se expira, as travas rearmam.
    /// </summary>
    public sealed class TripleLockPuzzle : NetworkBehaviour
    {
        [SerializeField] private LockActuator thermaTekLock;
        [SerializeField] private LockActuator kinesysLock;
        [SerializeField] private LockActuator omniCriptoLock;
        [SerializeField] private CommandQueueNetwork commandQueue;
        [SerializeField] private SessionScalingProfile scalingProfile;
        [SerializeField, Min(0f), Tooltip("Janela base da quest (ex.: 20 s na Câmara de Despacho). 0 usa a ação final padrão de 12 s.")]
        private float baseWindowSeconds;

        private readonly NetworkVariable<bool> _isOpen = new NetworkVariable<bool>();
        private readonly NetworkVariable<double> _deadline = new NetworkVariable<double>(-1);
        private readonly NetworkVariable<float> _windowSeconds = new NetworkVariable<float>();
        private readonly NetworkVariable<byte> _openMask = new NetworkVariable<byte>();
        private MemoryWindow _window;

        public event Action Opened;

        /// <summary>Servidor: a janela expirou sem as três ativações; a interface deve pedir novo ensaio.</summary>
        public event Action ServerWindowExpired;

        public bool IsOpen => _isOpen.Value;
        public float WindowSeconds => _windowSeconds.Value;
        public CommandQueueNetwork CommandQueue => commandQueue;

        /// <summary>Horário do servidor em que a janela atual fecha, ou negativo se nenhuma trava está aberta.</summary>
        public double Deadline => _deadline.Value;

        public bool IsSlotOpen(ParallaxClass role) => (_openMask.Value & (1 << (int)role)) != 0;

        public LockActuator LockFor(ParallaxClass role)
        {
            switch (role)
            {
                case ParallaxClass.Goliath: return thermaTekLock;
                case ParallaxClass.Phantom: return kinesysLock;
                default: return omniCriptoLock;
            }
        }

        public override void OnNetworkSpawn()
        {
            _isOpen.OnValueChanged += OnOpenChanged;
            if (!IsServer)
            {
                return;
            }

            _window = new MemoryWindow(ParallaxRoles.ClassCount, CurrentWindowSeconds());
            _windowSeconds.Value = (float)_window.WindowSeconds;
            Bind(thermaTekLock);
            Bind(kinesysLock);
            Bind(omniCriptoLock);
            NetworkManager.NetworkTickSystem.Tick += OnServerTick;
        }

        public override void OnNetworkDespawn()
        {
            _isOpen.OnValueChanged -= OnOpenChanged;
            Unbind(thermaTekLock);
            Unbind(kinesysLock);
            Unbind(omniCriptoLock);
            if (IsServer && NetworkManager != null && NetworkManager.NetworkTickSystem != null)
            {
                NetworkManager.NetworkTickSystem.Tick -= OnServerTick;
            }
        }

        /// <summary>Momento crítico: os Auxiliares executam a fila enquanto os humanos agem.</summary>
        public void ServerBeginFinalAction()
        {
            if (!IsServer || _isOpen.Value)
            {
                return;
            }

            if (commandQueue != null)
            {
                commandQueue.ServerBeginExecution();
            }
        }

        public void ServerResetLocks()
        {
            if (!IsServer)
            {
                return;
            }

            _window.Reset();
            _window.WindowSeconds = CurrentWindowSeconds();
            _windowSeconds.Value = (float)_window.WindowSeconds;
            _deadline.Value = -1;
            _openMask.Value = 0;
            foreach (ParallaxClass role in new[] { ParallaxClass.Goliath, ParallaxClass.Phantom, ParallaxClass.Cipher })
            {
                LockActuator actuator = LockFor(role);
                if (actuator != null)
                {
                    actuator.ServerReset();
                }
            }
        }

        private float CurrentWindowSeconds()
        {
            SessionScalingTable table = scalingProfile != null ? scalingProfile.Table : SessionScalingTable.Canon;
            int humans = SessionRoster.Active != null ? Math.Max(1, SessionRoster.Active.HumanCount) : 1;
            return baseWindowSeconds > 0f
                ? baseWindowSeconds * SessionScaling.ActionWindowMultiplier(humans, table)
                : SessionScaling.FinalActionWindowSeconds(humans, table);
        }

        private void Bind(LockActuator actuator)
        {
            if (actuator != null)
            {
                actuator.ServerActivated += OnLockActivated;
            }
        }

        private void Unbind(LockActuator actuator)
        {
            if (actuator != null)
            {
                actuator.ServerActivated -= OnLockActivated;
            }
        }

        private void OnLockActivated(LockActuator actuator)
        {
            if (_isOpen.Value)
            {
                return;
            }

            double now = NetworkManager.ServerTime.Time;
            if (_window.OpenCount(now) == 0)
            {
                _window.WindowSeconds = CurrentWindowSeconds();
                _windowSeconds.Value = (float)_window.WindowSeconds;
            }

            _window.Activate((int)actuator.Role, now);
            Publish(now);
            if (_window.AllOpen(now))
            {
                _isOpen.Value = true;
                _deadline.Value = -1;
            }
        }

        private void OnServerTick()
        {
            if (_isOpen.Value || _deadline.Value < 0)
            {
                return;
            }

            double now = NetworkManager.ServerTime.Time;
            if (now >= _deadline.Value)
            {
                ServerResetLocks();
                ServerWindowExpired?.Invoke();
                return;
            }

            Publish(now);
        }

        private void Publish(double now)
        {
            byte mask = 0;
            for (int i = 0; i < ParallaxRoles.ClassCount; i++)
            {
                if (_window.IsOpen(i, now))
                {
                    mask |= (byte)(1 << i);
                }
            }

            _openMask.Value = mask;
            _deadline.Value = _window.TryGetDeadline(now, out double deadline) ? deadline : -1;
        }

        private void OnOpenChanged(bool previous, bool current)
        {
            if (current)
            {
                Opened?.Invoke();
            }
        }
    }
}

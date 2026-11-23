using System;
using System.Collections;
using Parallax.Core.Definitions;
using Parallax.Core.Quest;
using Parallax.Core.Roles;
using Parallax.Core.Session;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Enemies;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Session;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>
    /// Orquestra Calor de Contrabando na Estação 14. Só o servidor avança etapas; clientes leem
    /// NetworkVariables para HUD, rádio e objetivos. A lore justifica a ordem: Câmara → contêiner → válvula → defesa.
    /// </summary>
    public sealed class Quest01Director : NetworkBehaviour
    {
        [SerializeField] private DispatchChamber dispatchChamber;
        [SerializeField] private PushableCargo cargo;
        [SerializeField] private ValveWheel valve;
        [SerializeField] private TrooperWaveSpawner waveSpawner;
        [SerializeField] private SessionScalingProfile scalingProfile;
        [SerializeField] private float autoBriefingDelaySeconds = 2.5f;

        private readonly NetworkVariable<Quest01Stage> _stage = new NetworkVariable<Quest01Stage>();
        private readonly NetworkVariable<Quest01Secondary> _secondary = new NetworkVariable<Quest01Secondary>(Quest01Secondary.FogoMinimo);
        private readonly NetworkVariable<Quest01Rating> _rating = new NetworkVariable<Quest01Rating>(Quest01Rating.Prata);
        private readonly NetworkVariable<int> _cores = new NetworkVariable<int>();
        private readonly NetworkVariable<float> _coreChargeSum = new NetworkVariable<float>();
        private readonly NetworkVariable<float> _holdRemaining = new NetworkVariable<float>();
        private readonly NetworkVariable<int> _waveIndex = new NetworkVariable<int>(-1);
        private readonly NetworkVariable<bool> _started = new NetworkVariable<bool>();

        private Quest01Progress _progress;
        private Quest01Tuning _tuning = Quest01Tuning.Canon;
        private int[] _waves = Array.Empty<int>();
        private Coroutine _holdRoutine;
        private string _radioLine = string.Empty;

        public static Quest01Director Active { get; private set; }

        /// <summary>Local: nova fala de rádio (Candeia / console).</summary>
        public static event Action<string> RadioSpoken;

        /// <summary>Local: etapa mudou.</summary>
        public static event Action StageChanged;

        public Quest01Stage Stage => _stage.Value;
        public Quest01Secondary Secondary => _secondary.Value;
        public Quest01Rating Rating => _rating.Value;
        public int CoresCollected => _cores.Value;
        public float AverageCoreCharge => _cores.Value == 0 ? 0f : _coreChargeSum.Value / _cores.Value;
        public float HoldRemaining => _holdRemaining.Value;
        public int WaveIndex => _waveIndex.Value;
        public bool HasStarted => _started.Value;
        public string RadioLine => _radioLine;
        public DispatchChamber Chamber => dispatchChamber;
        public PushableCargo Cargo => cargo;
        public ValveWheel Valve => valve;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active = null;
            RadioSpoken = null;
            StageChanged = null;
        }

        public override void OnNetworkSpawn()
        {
            Active = this;
            _stage.OnValueChanged += OnStageReplicated;
            if (!IsServer)
            {
                return;
            }

            _progress = new Quest01Progress();
            Publish();
            BindServer();
            StartCoroutine(AutoBriefing());
        }

        public override void OnNetworkDespawn()
        {
            if (Active == this)
            {
                Active = null;
            }

            _stage.OnValueChanged -= OnStageReplicated;
            if (!IsServer)
            {
                return;
            }

            UnbindServer();
            if (_holdRoutine != null)
            {
                StopCoroutine(_holdRoutine);
            }
        }

        public string PrimaryObjectiveText()
        {
            switch (_stage.Value)
            {
                case Quest01Stage.Briefing:
                    return "Aguarde o briefing da Candeia.";
                case Quest01Stage.OpenDispatchChamber:
                    return "Abrir a Câmara de Despacho (porta de chumbo · Chave Tripla).";
                case Quest01Stage.DeliverCargo:
                    return "Levar o contêiner de acumuladores até o duto de Sangria.";
                case Quest01Stage.OpenValve:
                    return "Abrir a Válvula Mestra 14 e liberar o calor.";
                case Quest01Stage.HoldTheLine:
                    return $"Aguentar as Esquadras · {_holdRemaining.Value:0}s" +
                           (_waveIndex.Value >= 0 ? $" · onda {_waveIndex.Value + 1}" : string.Empty);
                case Quest01Stage.Complete:
                    return $"Missão concluída · {_rating.Value} · núcleos {_cores.Value}";
                default:
                    return string.Empty;
            }
        }

        private IEnumerator AutoBriefing()
        {
            yield return new WaitForSeconds(autoBriefingDelaySeconds);
            if (!_started.Value)
            {
                ServerBegin();
            }
        }

        private void ServerBegin()
        {
            if (!IsServer || _started.Value)
            {
                return;
            }

            SessionScalingTable table = scalingProfile != null ? scalingProfile.Table : SessionScalingTable.Canon;
            int humans = SessionRoster.Active != null ? Math.Max(1, SessionRoster.Active.HumanCount) : 1;
            _waves = _tuning.WavesFor(humans, table);
            _progress.Begin(NetworkManager.ServerTime.Time);
            _started.Value = true;
            Publish();
            SpeakRpc("Candeia: Setor 7-Baixo faz três dias sem calor. Estação 14 — entra, pega o contêiner e joga no duto.");
            AudioCues.Play2D(AudioCue.UiTick);
        }

        private void BindServer()
        {
            if (dispatchChamber != null)
            {
                dispatchChamber.ServerOpened += OnDispatchOpened;
            }

            if (cargo != null)
            {
                cargo.ServerDelivered += OnCargoDelivered;
            }

            if (valve != null)
            {
                valve.ServerOpened += OnValveOpened;
            }

            LoreInspectable.ServerRead += OnLoreRead;
            PlayerWeapon.ServerLiveLineHit += OnLiveLineHit;
            ThermalCorePickup.ServerCollected += OnCoreCollected;
        }

        private void UnbindServer()
        {
            if (dispatchChamber != null)
            {
                dispatchChamber.ServerOpened -= OnDispatchOpened;
            }

            if (cargo != null)
            {
                cargo.ServerDelivered -= OnCargoDelivered;
            }

            if (valve != null)
            {
                valve.ServerOpened -= OnValveOpened;
            }

            LoreInspectable.ServerRead -= OnLoreRead;
            PlayerWeapon.ServerLiveLineHit -= OnLiveLineHit;
            ThermalCorePickup.ServerCollected -= OnCoreCollected;
        }

        private void OnDispatchOpened()
        {
            if (!_progress.CompleteStage(Quest01Stage.OpenDispatchChamber, NetworkManager.ServerTime.Time))
            {
                return;
            }

            if (cargo != null)
            {
                cargo.ServerSetActive(true);
            }

            Publish();
            SpeakRpc("Candeia: Câmara aberta. Empurra o contêiner no trilho — sem Goliath, engata o Macaco.");
        }

        private void OnCargoDelivered()
        {
            if (!_progress.CompleteStage(Quest01Stage.DeliverCargo, NetworkManager.ServerTime.Time))
            {
                return;
            }

            if (valve != null)
            {
                valve.ServerSetArmed(true);
            }

            Publish();
            SpeakRpc("Candeia: Contêiner no duto. Gira a Válvula Mestra 14. O calor vai acordar as Esquadras.");
        }

        private void OnValveOpened(ParallaxClass opener)
        {
            if (!_progress.CompleteStage(Quest01Stage.OpenValve, NetworkManager.ServerTime.Time))
            {
                return;
            }

            Publish();
            SpeakRpc("Candeia: Sangria aberta. Aguenta a linha até o calor passar. Não fura Linha Viva.");
            _holdRoutine = StartCoroutine(HoldTheLine());
        }

        private IEnumerator HoldTheLine()
        {
            SessionScalingTable table = scalingProfile != null ? scalingProfile.Table : SessionScalingTable.Canon;
            int humans = SessionRoster.Active != null ? Math.Max(1, SessionRoster.Active.HumanCount) : 1;
            float healthMul = SessionScaling.EnemyMultiplier(humans, table);
            double holdStarted = NetworkManager.ServerTime.Time;
            float total = _tuning.HoldTheLineSeconds;
            _holdRemaining.Value = total;

            for (int i = 0; i < _waves.Length; i++)
            {
                _waveIndex.Value = i;
                if (waveSpawner != null)
                {
                    waveSpawner.ServerSpawn(_waves[i], healthMul, true);
                }

                SpeakRpc(i == 0
                    ? "Console: Esquadra de Contenção na porta de serviço."
                    : $"Console: Reforço {_waves[i]} — espuma criogênica.");

                float slice = total / Math.Max(1, _waves.Length);
                double until = NetworkManager.ServerTime.Time + slice;
                while (NetworkManager.ServerTime.Time < until)
                {
                    _holdRemaining.Value = Mathf.Max(0f, total - (float)(NetworkManager.ServerTime.Time - holdStarted));
                    yield return null;
                }
            }

            if (waveSpawner != null)
            {
                waveSpawner.ServerRetreatAll();
            }

            _holdRemaining.Value = 0f;
            _waveIndex.Value = -1;
            if (_progress.CompleteStage(Quest01Stage.HoldTheLine, NetworkManager.ServerTime.Time))
            {
                Publish();
                SpeakRpc($"Candeia: Calor no duto. Classificação {_progress.Rating}. Núcleos {_progress.CoresCollected}. Volta pra casa.");
                AudioCues.Play2D(AudioCue.QuestComplete);
            }
        }

        private void OnLoreRead(LoreInspectable inspectable, ParallaxClass viewer)
        {
            if (inspectable == null || inspectable.Secondary == Quest01Secondary.None)
            {
                return;
            }

            if (_progress.Mark(inspectable.Secondary))
            {
                Publish();
                SpeakRpc("Console: leitura registrada — " + inspectable.Title + ".");
            }
        }

        private void OnLiveLineHit(Vector3 point)
        {
            if (_progress.BreakFogoMinimo())
            {
                Publish();
                SpeakRpc("Candeia: Linha Viva rompida. Bônus Fogo Mínimo perdido.");
                AudioCues.Play(AudioCue.SteamBurst, point);
            }
        }

        private void OnCoreCollected(float charge01)
        {
            _progress.CollectCore(charge01);
            Publish();
        }

        private void Publish()
        {
            _stage.Value = _progress.Stage;
            _secondary.Value = _progress.Secondary;
            _rating.Value = _progress.Rating;
            _cores.Value = _progress.CoresCollected;
            _coreChargeSum.Value = _progress.CoreChargeSum;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SpeakRpc(string line)
        {
            _radioLine = line;
            RadioSpoken?.Invoke(line);
        }

        private void OnStageReplicated(Quest01Stage previous, Quest01Stage current) => StageChanged?.Invoke();
    }
}

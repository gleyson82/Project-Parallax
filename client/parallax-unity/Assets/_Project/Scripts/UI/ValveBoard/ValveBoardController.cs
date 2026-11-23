using System;
using System.Collections;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.Session;
using Parallax.Rendering;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Localization.Settings;

namespace Parallax.UI.ValveBoard
{
    /// <summary>
    /// O Quadro de Válvulas como cenário 3D: chapa de aço com volantes, três camadas de leitura em
    /// profundidade real (Física/Goliath, Espacial/Phantom, Lógica/Cipher), geada por inatividade,
    /// Nota do Piso conforme o calor do setor e lançamento pelas Três Mãos.
    /// </summary>
    public sealed class ValveBoardController : MonoBehaviour
    {
        private const string MissionTable = "Missions";

        [Serializable]
        private struct ReadingLayer
        {
            public ParallaxClass viewer;
            public Transform plane;
            public Vector3 focusedOffset;
            public Vector3 backgroundOffset;
        }

        [SerializeField] private ParallaxInput input;
        [SerializeField] private ValveNode[] valves = new ValveNode[0];
        [SerializeField] private ReadingLayer[] layers = new ReadingLayer[3];
        [SerializeField] private TriHandsLaunch triHands;
        [SerializeField] private FloorNoteSynth floorNote;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text briefingText;
        [SerializeField] private TMP_Text readingText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField, Min(0.01f)] private float layerSlideSeconds = 0.25f;
        [SerializeField, Min(0.001f)] private float frostIdleSpeed = 0.055f;
        [SerializeField, Range(0f, 1f)] private float frostInputRecovery = 0.12f;
        [SerializeField, Range(0.1f, 3f), Tooltip("Sessão offline: tempo de segurar ajustável.")]
        private float offlineHoldSeconds = 0.4f;

        private int _selected;
        private ParallaxClass _activeLayer = ParallaxClass.Goliath;
        private float _frost = 0.16f;
        private IDisposable _anyButton;
        private Coroutine _layerSlide;
        private Coroutine _frostLoop;
        private Coroutine _offlineHold;

        public event Action<MissionDefinition> MissionLaunched;

        public MissionDefinition SelectedMission => valves.Length > 0 ? valves[_selected].Mission : null;

        private void OnEnable()
        {
            input.UseBoardMap();
            input.BoardNavigate.performed += OnNavigate;
            input.BoardSwitchLayer.performed += OnSwitchLayer;
            input.BoardConfirm.started += OnConfirmStarted;
            input.BoardConfirm.canceled += OnConfirmCanceled;
            _anyButton = InputSystem.onAnyButtonPress.Call(_ => RecoverFromFrost());
            if (triHands != null)
            {
                triHands.LaunchConfirmed += OnLaunchConfirmed;
            }

            _frostLoop = StartCoroutine(FrostLoop());
            ApplyDefaultLayerForLocalPlayer();
            Select(_selected);
        }

        private void OnDisable()
        {
            input.BoardNavigate.performed -= OnNavigate;
            input.BoardSwitchLayer.performed -= OnSwitchLayer;
            input.BoardConfirm.started -= OnConfirmStarted;
            input.BoardConfirm.canceled -= OnConfirmCanceled;
            _anyButton?.Dispose();
            if (triHands != null)
            {
                triHands.LaunchConfirmed -= OnLaunchConfirmed;
            }

            StopAllCoroutines();
            ParallaxVisionState.SetFrostProgress(0f);
        }

        public void Select(int index)
        {
            if (valves.Length == 0)
            {
                return;
            }

            _selected = (index % valves.Length + valves.Length) % valves.Length;
            for (int i = 0; i < valves.Length; i++)
            {
                valves[i].SetSelected(i == _selected);
            }

            MissionDefinition mission = valves[_selected].Mission;
            if (mission == null)
            {
                return;
            }

            if (floorNote != null)
            {
                floorNote.SectorHeat = mission.SectorHeat;
            }

            ParallaxVisionState.SetSectorHeat(mission.SectorHeat);
            SetLocalized(titleText, mission.DisplayNameKey);
            SetLocalized(briefingText, mission.BriefingKey);
            SetLocalized(rewardText, mission.RewardKey);
            SetLocalized(readingText, mission.ReadingKeyFor(_activeLayer));
        }

        /// <summary>Em cooperativo, cada jogador vê a própria camada; em solo, alterna à vontade.</summary>
        public void ShowLayer(ParallaxClass viewer)
        {
            _activeLayer = viewer;
            if (_layerSlide != null)
            {
                StopCoroutine(_layerSlide);
            }

            _layerSlide = StartCoroutine(SlideLayers());
            MissionDefinition mission = SelectedMission;
            if (mission != null)
            {
                SetLocalized(readingText, mission.ReadingKeyFor(viewer));
            }
        }

        private void ApplyDefaultLayerForLocalPlayer()
        {
            SessionRoster roster = SessionRoster.Active;
            if (roster != null && roster.NetworkManager != null &&
                roster.TryGetClassOf(roster.NetworkManager.LocalClientId, out ParallaxClass localClass))
            {
                ShowLayer(localClass);
                return;
            }

            ShowLayer(_activeLayer);
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            if (Mathf.Abs(direction.y) > 0.5f)
            {
                Select(_selected + (direction.y < 0f ? 1 : -1));
            }
        }

        private void OnSwitchLayer(InputAction.CallbackContext context)
        {
            SessionRoster roster = SessionRoster.Active;
            bool solo = roster == null || roster.HumanCount <= 1;
            if (solo)
            {
                ShowLayer((ParallaxClass)(((int)_activeLayer + 1) % ParallaxRoles.ClassCount));
            }
        }

        private void OnConfirmStarted(InputAction.CallbackContext context)
        {
            if (triHands != null && triHands.IsSpawned)
            {
                triHands.Press(_selected);
                return;
            }

            _offlineHold = StartCoroutine(OfflineHold());
        }

        private void OnConfirmCanceled(InputAction.CallbackContext context)
        {
            if (triHands != null && triHands.IsSpawned)
            {
                triHands.Release();
                return;
            }

            if (_offlineHold != null)
            {
                StopCoroutine(_offlineHold);
                _offlineHold = null;
                valves[_selected].SetTurnProgress(0f);
            }
        }

        private IEnumerator OfflineHold()
        {
            float held = 0f;
            while (held < offlineHoldSeconds)
            {
                held += Time.unscaledDeltaTime;
                valves[_selected].SetTurnProgress(held / offlineHoldSeconds);
                yield return null;
            }

            _offlineHold = null;
            OnLaunchConfirmed(_selected);
        }

        private void OnLaunchConfirmed(int valveIndex)
        {
            if (valveIndex < 0 || valveIndex >= valves.Length)
            {
                return;
            }

            valves[valveIndex].PlayOpen();
            MissionLaunched?.Invoke(valves[valveIndex].Mission);
        }

        private void RecoverFromFrost()
        {
            _frost = Mathf.Max(0.08f, _frost - frostInputRecovery);
        }

        private IEnumerator FrostLoop()
        {
            const float interval = 0.1f;
            var wait = new WaitForSecondsRealtime(interval);
            while (true)
            {
                _frost = Mathf.Clamp(_frost + frostIdleSpeed * interval, 0.08f, 0.95f);
                ParallaxVisionState.SetFrostProgress(_frost);
                if (triHands != null && triHands.IsSpawned && valves.Length > 0)
                {
                    valves[_selected].SetTurnProgress(triHands.Progress);
                }

                yield return wait;
            }
        }

        private IEnumerator SlideLayers()
        {
            float elapsed = 0f;
            var starts = new Vector3[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                starts[i] = layers[i].plane != null ? layers[i].plane.localPosition : Vector3.zero;
            }

            while (elapsed < layerSlideSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / layerSlideSeconds), 3f);
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].plane == null)
                    {
                        continue;
                    }

                    Vector3 target = layers[i].viewer == _activeLayer ? layers[i].focusedOffset : layers[i].backgroundOffset;
                    layers[i].plane.localPosition = Vector3.Lerp(starts[i], target, t);
                }

                yield return null;
            }

            _layerSlide = null;
        }

        private static void SetLocalized(TMP_Text target, string key)
        {
            if (target == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(MissionTable, key);
            operation.Completed += handle => target.text = handle.Result;
        }
    }
}

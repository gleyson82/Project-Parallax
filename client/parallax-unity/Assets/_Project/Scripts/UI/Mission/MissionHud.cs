using Parallax.Core.Quest;
using Parallax.Core.Roles;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Quest;
using Unity.Netcode;
using UnityEngine;

namespace Parallax.UI.Mission
{
    /// <summary>
    /// HUD diegética da Quest 1: objetivo, prompt de interação, calor do cano, integridade, rádio e secundários.
    /// Desenhada para sessão de teste — legível a 1080p e no Steam Deck, sem menus flutuantes.
    /// </summary>
    public sealed class MissionHud : MonoBehaviour
    {
        private GUIStyle _panel;
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _radio;
        private GUIStyle _prompt;
        private string _radioLine = string.Empty;
        private float _radioUntil;

        private void OnEnable() => Quest01Director.RadioSpoken += OnRadio;

        private void OnDisable() => Quest01Director.RadioSpoken -= OnRadio;

        private void OnRadio(string line)
        {
            _radioLine = line;
            _radioUntil = Time.unscaledTime + 8f;
        }

        private void OnGUI()
        {
            EnsureStyles();
            Quest01Director quest = Quest01Director.Active;
            PlayerInteractor interactor = PlayerInteractor.Local;
            PlayerClassBinder binder = PlayerClassBinder.Local;
            PlayerWeapon weapon = binder != null ? binder.GetComponent<PlayerWeapon>() : null;
            PlayerVitals vitals = binder != null ? binder.GetComponent<PlayerVitals>() : null;

            float left = 16f;
            float top = 16f;
            float width = Mathf.Min(520f, Screen.width * 0.42f);

            GUILayout.BeginArea(new Rect(left, top, width, Screen.height - 32f));
            if (quest != null && NetworkManager.Singleton != null &&
                (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                GUILayout.Box("ESTAÇÃO 14 · CALOR DE CONTRABANDO", _title);
                GUILayout.Box(quest.PrimaryObjectiveText(), _body);
                GUILayout.Box(SecondaryLine(quest), _panel);
            }

            if (weapon != null && weapon.Weapon != null)
            {
                DrawBar("Cano · " + (weapon.IsOverheated ? "SUPERAQUECIDO" : "calor"), weapon.Heat01,
                    weapon.IsOverheated ? new Color(1f, 0.25f, 0.1f) : new Color(1f, 0.55f, 0.2f));
            }

            if (vitals != null)
            {
                DrawBar(vitals.IsDowned ? "TRAJE · CAÍDO" : "Integridade", vitals.Integrity01,
                    vitals.IsDowned ? new Color(0.7f, 0.1f, 0.1f) : new Color(0.3f, 0.85f, 0.45f));
                if (vitals.Chill01 > 0.02f)
                {
                    DrawBar("Frio · espuma", vitals.Chill01, new Color(0.45f, 0.85f, 1f));
                }
            }

            GUILayout.EndArea();

            if (interactor != null && !string.IsNullOrEmpty(interactor.FocusPrompt))
            {
                string hold = interactor.HoldProgress >= 0f
                    ? $"  [{Mathf.RoundToInt(interactor.HoldProgress * 100f)}%]"
                    : string.Empty;
                Vector2 size = _prompt.CalcSize(new GUIContent("[E]  " + interactor.FocusPrompt + hold));
                Rect promptRect = new Rect((Screen.width - size.x) * 0.5f - 12f, Screen.height * 0.72f, size.x + 24f, size.y + 16f);
                GUI.Box(promptRect, "[E]  " + interactor.FocusPrompt + hold, _prompt);
                if (interactor.HoldProgress >= 0f)
                {
                    Rect fill = new Rect(promptRect.x + 8f, promptRect.yMax - 6f, (promptRect.width - 16f) * interactor.HoldProgress, 4f);
                    GUI.DrawTexture(fill, Texture2D.whiteTexture);
                }
            }

            if (!string.IsNullOrEmpty(_radioLine) && Time.unscaledTime <= _radioUntil)
            {
                float radioWidth = Mathf.Min(640f, Screen.width * 0.55f);
                GUI.Box(new Rect((Screen.width - radioWidth) * 0.5f, Screen.height - 110f, radioWidth, 72f), _radioLine, _radio);
            }

            LoreInspectable inspect = LorePanelTarget();
            if (inspect != null)
            {
                string text = !string.IsNullOrEmpty(inspect.Inscription)
                    ? inspect.Inscription
                    : (inspect.Fragment != null ? inspect.Fragment.TextKey : inspect.Title);
                float loreWidth = Mathf.Min(480f, Screen.width * 0.4f);
                GUI.Box(new Rect(Screen.width - loreWidth - 20f, 20f, loreWidth, 140f),
                    inspect.Title + "\n\n" + text, _body);
            }
        }

        private static string SecondaryLine(Quest01Director quest)
        {
            Quest01Secondary s = quest.Secondary;
            string fogo = (s & Quest01Secondary.FogoMinimo) != 0 ? "Fogo Mínimo OK" : "Fogo Mínimo perdido";
            string man = (s & Quest01Secondary.Manometro) != 0 ? " · Manômetro" : string.Empty;
            string rel = (s & Quest01Secondary.Relogio) != 0 ? " · Relógio" : string.Empty;
            string eti = (s & Quest01Secondary.Etiqueta) != 0 ? " · Etiqueta" : string.Empty;
            return $"{fogo}{man}{rel}{eti} · {quest.Rating} · núcleos {quest.CoresCollected}";
        }

        private LoreInspectable _lastInspect;
        private float _inspectUntil;

        private LoreInspectable LorePanelTarget()
        {
            if (Time.unscaledTime > _inspectUntil)
            {
                _lastInspect = null;
            }

            return _lastInspect;
        }

        private void Awake() => LoreInspectable.LocalInspected += OnInspect;

        private void OnDestroy() => LoreInspectable.LocalInspected -= OnInspect;

        private void OnInspect(LoreInspectable inspectable, ParallaxClass viewer)
        {
            _lastInspect = inspectable;
            _inspectUntil = Time.unscaledTime + 6f;
        }

        private void DrawBar(string label, float fill01, Color color)
        {
            GUILayout.Label(label, _panel);
            Rect rect = GUILayoutUtility.GetRect(180f, 14f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, new Color(0.08f, 0.08f, 0.08f, 0.85f), 0f, 0f);
            Rect fill = new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * Mathf.Clamp01(fill01), rect.height - 2f);
            GUI.DrawTexture(fill, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
        }

        private void EnsureStyles()
        {
            if (_panel != null)
            {
                return;
            }

            _panel = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                fontSize = 13,
                padding = new RectOffset(10, 10, 6, 6)
            };
            _title = new GUIStyle(_panel) { fontSize = 15, fontStyle = FontStyle.Bold };
            _body = new GUIStyle(_panel) { fontSize = 14, wordWrap = true };
            _radio = new GUIStyle(_panel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };
            _prompt = new GUIStyle(_panel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }
    }
}

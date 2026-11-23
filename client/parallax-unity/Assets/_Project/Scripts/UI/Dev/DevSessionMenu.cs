using Parallax.Core.Roles;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Session;
using Parallax.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parallax.UI.Dev
{
    /// <summary>
    /// Painel de desenvolvimento do laboratório jogável: hospedar ou entrar na sessão local, trocar de classe
    /// (teclas 1/2/3 ou F1/F2/F3, ou botões com o cursor solto) e lembrar os controles. Ferramenta de teste.
    /// </summary>
    public sealed class DevSessionMenu : MonoBehaviour
    {
        private const float Width = 430f;
        private const float RequestFeedbackSeconds = 1.5f;

        [SerializeField] private SessionNetworkLauncher launcher;
        [SerializeField] private NetworkManager networkManager;

        private GUIStyle _box;
        private string _status = string.Empty;
        private ParallaxClass _requested;
        private float _requestedAt = -1f;

        // Ferramenta de laboratório: atalhos de teclado diretos, sem regra de jogo.
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || PlayerClassBinder.Local == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.f1Key.wasPressedThisFrame) Request(ParallaxClass.Goliath);
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.f2Key.wasPressedThisFrame) Request(ParallaxClass.Phantom);
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.f3Key.wasPressedThisFrame) Request(ParallaxClass.Cipher);
        }

        private void Request(ParallaxClass parallaxClass)
        {
            PlayerClassBinder local = PlayerClassBinder.Local;
            if (local == null || (local.HasClass && local.CurrentClass == parallaxClass))
            {
                return;
            }

            if (!BrasaPoint.AllowsClassSwapAt(local.transform.position))
            {
                _status = "Troca de classe só no Ponto de Brasa.";
                return;
            }

            _requested = parallaxClass;
            _requestedAt = Time.unscaledTime;
            local.RequestClass(parallaxClass);
        }

        private void OnGUI()
        {
            _box ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, wordWrap = true, fontSize = 13 };
            GUILayout.BeginArea(new Rect(12f, 12f, Width, Screen.height - 24f));

            bool running = networkManager != null && (networkManager.IsServer || networkManager.IsClient);
            if (!running)
            {
                DrawLobby();
            }
            else
            {
                DrawSession();
            }

            GUILayout.EndArea();
        }

        private void DrawLobby()
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string title = scene == "Quest01_Estacao14"
                ? "PARALLAX · Quest 1 · Estação 14 (Calor de Contrabando)"
                : "PARALLAX · Laboratório Jogável";
            GUILayout.Box(title + "\nUma instância hospeda; as outras (Multiplayer Play Mode) entram pelo 127.0.0.1.", _box);
            if (GUILayout.Button("Hospedar sessão (Host)", GUILayout.Height(32f)))
            {
                _status = launcher.StartHost() ? string.Empty : "Falha ao hospedar. A porta 7777 está livre?";
            }

            if (GUILayout.Button("Entrar como cliente (127.0.0.1:7777)", GUILayout.Height(32f)))
            {
                _status = launcher.StartLocalClient() ? string.Empty : "Falha ao conectar.";
            }

            if (!string.IsNullOrEmpty(_status))
            {
                GUILayout.Label(_status);
            }
        }

        private void DrawSession()
        {
            PlayerClassBinder local = PlayerClassBinder.Local;
            SessionRoster roster = SessionRoster.Active;
            string role = networkManager.IsHost ? "Anfitrião" : "Cliente";
            string classLabel = local != null && local.HasClass ? local.CurrentClass.ToString() : "aguardando classe";
            int humans = roster != null ? roster.HumanCount : 0;

            GUILayout.Box(
                $"{role} · Classe: {classLabel} · Operadores: {humans}/3\n" +
                "WASD mover · Mouse olhar · Espaço pular · Clique esquerdo atirar\n" +
                "Q habilidade (Goliath ancora · Phantom arpéu · Cipher Override)\n" +
                "E interagir / segurar · Botão direito ou F: 1ª pessoa\n" +
                "1/2/3 troca de classe (Ponto de Brasa) · Esc solta o cursor", _box);

            GUILayout.BeginHorizontal();
            DrawClassButton(ParallaxClass.Goliath, local, roster);
            DrawClassButton(ParallaxClass.Phantom, local, roster);
            DrawClassButton(ParallaxClass.Cipher, local, roster);
            GUILayout.EndHorizontal();

            DrawRequestFeedback(local, roster);

            if (GUILayout.Button("Sair da sessão", GUILayout.Height(26f)))
            {
                launcher.Leave();
                PlayerLook.SetCursorLocked(false);
            }
        }

        private void DrawClassButton(ParallaxClass parallaxClass, PlayerClassBinder local, SessionRoster roster)
        {
            bool mine = local != null && local.HasClass && local.CurrentClass == parallaxClass;
            bool takenByOther = !mine && roster != null && roster.IsHuman(parallaxClass);
            string label = mine ? $"[{parallaxClass}]" : takenByOther ? $"{parallaxClass} (ocupado)" : parallaxClass.ToString();
            GUI.enabled = !mine && !takenByOther;
            if (GUILayout.Button(label, GUILayout.Height(26f)))
            {
                Request(parallaxClass);
            }

            GUI.enabled = true;
        }

        private void DrawRequestFeedback(PlayerClassBinder local, SessionRoster roster)
        {
            if (_requestedAt < 0f || local == null)
            {
                return;
            }

            if (local.CurrentClass == _requested)
            {
                _requestedAt = -1f;
                return;
            }

            if (Time.unscaledTime - _requestedAt > RequestFeedbackSeconds)
            {
                bool taken = roster != null && roster.IsHuman(_requested);
                GUILayout.Label(taken
                    ? $"{_requested} já está com outro operador. Nenhuma classe se repete na sessão."
                    : $"O anfitrião não confirmou a troca para {_requested}.");
            }
        }
    }
}

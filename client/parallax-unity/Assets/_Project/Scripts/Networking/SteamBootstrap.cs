#if !DISABLESTEAMWORKS && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX)
#define PARALLAX_STEAM
#endif

using UnityEngine;
#if PARALLAX_STEAM
using Steamworks;
#endif

namespace Parallax.Networking
{
    /// <summary>
    /// Inicializa a Steam e o Steam Datagram Relay. Sem Steam aberta, o jogo cai para o UnityTransport local
    /// (Multiplayer Play Mode e testes de agente). Em desenvolvimento, o AppID vem de steam_appid.txt (480).
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class SteamBootstrap : MonoBehaviour
    {
        [SerializeField] private bool initializeOnAwake = true;

        public static bool IsReady { get; private set; }

        public static ulong LocalSteamId
        {
            get
            {
#if PARALLAX_STEAM
                return IsReady ? SteamUser.GetSteamID().m_SteamID : 0UL;
#else
                return 0UL;
#endif
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (initializeOnAwake)
            {
                Initialize();
            }
        }

        public bool Initialize()
        {
            if (IsReady)
            {
                return true;
            }

#if PARALLAX_STEAM
            ESteamAPIInitResult result = SteamAPI.InitEx(out string error);
            if (result != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
            {
                Debug.LogWarning($"[Parallax/Steam] Steam indisponível ({result}: {error}). Usando transporte local.");
                return false;
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();
            IsReady = true;
            return true;
#else
            return false;
#endif
        }

        // Infraestrutura: a Steam exige bombear callbacks a cada frame. Não é lógica de jogo.
        private void Update()
        {
#if PARALLAX_STEAM
            if (IsReady)
            {
                SteamAPI.RunCallbacks();
            }
#endif
        }

        private void OnDestroy()
        {
#if PARALLAX_STEAM
            if (IsReady)
            {
                SteamAPI.Shutdown();
                IsReady = false;
            }
#endif
        }
    }
}

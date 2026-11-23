#if !DISABLESTEAMWORKS && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX)
#define PARALLAX_STEAM
#endif

using System;
using Parallax.Core.Session;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
#if PARALLAX_STEAM
using Steamworks;
#endif

namespace Parallax.Networking
{
    /// <summary>
    /// Ponto único para abrir e entrar em sessões de 1 a 3 jogadores (Host Autoritativo).
    /// Steam (lobby de amigos + SDR) em builds; UnityTransport local no editor e no Multiplayer Play Mode.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    public sealed class SessionNetworkLauncher : MonoBehaviour
    {
        private const string LobbyHostKey = "parallax_host";

        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport localTransport;
        [SerializeField] private SteamSocketsTransport steamTransport;
        [SerializeField, Tooltip("No editor, o padrão é transporte local para testes com Multiplayer Play Mode.")]
        private bool useSteamInEditor;
        [SerializeField] private string localAddress = "127.0.0.1";
        [SerializeField] private ushort localPort = 7777;

        public event Action<string> SessionRejected;

        public bool UsesSteam => SteamBootstrap.IsReady && (!Application.isEditor || useSteamInEditor) && steamTransport != null;

#if PARALLAX_STEAM
        private CallResult<LobbyCreated_t> _lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> _joinRequested;
        private Callback<LobbyEnter_t> _lobbyEntered;
        private CSteamID _currentLobby;
#endif

        private void Awake()
        {
            if (networkManager == null)
            {
                networkManager = GetComponent<NetworkManager>();
            }

            networkManager.NetworkConfig.ConnectionApproval = true;
            networkManager.ConnectionApprovalCallback = ApproveConnection;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void Start()
        {
#if PARALLAX_STEAM
            if (UsesSteam)
            {
                _lobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
                _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
                _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            }
#endif
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        public bool StartHost()
        {
            SelectTransport();
            if (!networkManager.StartHost())
            {
                return false;
            }

#if PARALLAX_STEAM
            if (UsesSteam)
            {
                _lobbyCreated.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, SessionScaling.MaxPlayers));
            }
#endif
            return true;
        }

        /// <summary>Entra pela rede local (editor, MPPM ou LAN de teste).</summary>
        public bool StartLocalClient()
        {
            networkManager.NetworkConfig.NetworkTransport = localTransport;
            localTransport.SetConnectionData(localAddress, localPort);
            return networkManager.StartClient();
        }

        public bool StartSteamClient(ulong hostSteamId)
        {
            if (!UsesSteam)
            {
                return false;
            }

            steamTransport.HostSteamId = hostSteamId;
            networkManager.NetworkConfig.NetworkTransport = steamTransport;
            return networkManager.StartClient();
        }

        public void Leave()
        {
#if PARALLAX_STEAM
            if (UsesSteam && _currentLobby.IsValid())
            {
                SteamMatchmaking.LeaveLobby(_currentLobby);
                _currentLobby = CSteamID.Nil;
            }
#endif
            networkManager.Shutdown();
        }

        private void SelectTransport()
        {
            if (UsesSteam)
            {
                networkManager.NetworkConfig.NetworkTransport = steamTransport;
                return;
            }

            networkManager.NetworkConfig.NetworkTransport = localTransport;
            localTransport.SetConnectionData(localAddress, localPort, "0.0.0.0");
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            bool isHostItself = request.ClientNetworkId == networkManager.LocalClientId;
            bool hasSeat = networkManager.ConnectedClientsIds.Count < SessionScaling.MaxPlayers;
            response.Approved = isHostItself || hasSeat;
            response.CreatePlayerObject = response.Approved;
            response.Reason = response.Approved ? string.Empty : "Esquadra completa: três operadores já estão na sessão.";
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!networkManager.IsServer && clientId == networkManager.LocalClientId && !string.IsNullOrEmpty(networkManager.DisconnectReason))
            {
                SessionRejected?.Invoke(networkManager.DisconnectReason);
            }
        }

#if PARALLAX_STEAM
        private void OnLobbyCreated(LobbyCreated_t result, bool ioFailure)
        {
            if (ioFailure || result.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogWarning("[Parallax/Steam] Falha ao criar o lobby de amigos.");
                return;
            }

            _currentLobby = new CSteamID(result.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(_currentLobby, LobbyHostKey, SteamBootstrap.LocalSteamId.ToString());
        }

        private void OnJoinRequested(GameLobbyJoinRequested_t request)
        {
            SteamMatchmaking.JoinLobby(request.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t entered)
        {
            _currentLobby = new CSteamID(entered.m_ulSteamIDLobby);
            if (networkManager.IsServer)
            {
                return;
            }

            CSteamID owner = SteamMatchmaking.GetLobbyOwner(_currentLobby);
            StartSteamClient(owner.m_SteamID);
        }
#endif
    }
}

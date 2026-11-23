#if !DISABLESTEAMWORKS && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX)
#define PARALLAX_STEAM
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
#if PARALLAX_STEAM
using Steamworks;
#endif

namespace Parallax.Networking
{
    /// <summary>
    /// Transporte NGO sobre Steam Networking Sockets (P2P via Steam Datagram Relay, custo de servidor zero).
    /// O anfitrião escuta em P2P; os clientes conectam pelo SteamID do anfitrião (obtido do lobby).
    /// </summary>
    public sealed class SteamSocketsTransport : NetworkTransport
    {
        private const int MaxMessagesPerPoll = 64;

        [SerializeField, Tooltip("SteamID do anfitrião. Preenchido pelo lobby antes de StartClient.")]
        private ulong hostSteamId;

        public ulong HostSteamId
        {
            get => hostSteamId;
            set => hostSteamId = value;
        }

        public override ulong ServerClientId => 0;

        public override bool IsSupported => SteamBootstrap.IsReady;

#if PARALLAX_STEAM
        private readonly Queue<PendingEvent> _events = new Queue<PendingEvent>();
        private readonly Dictionary<ulong, HSteamNetConnection> _clients = new Dictionary<ulong, HSteamNetConnection>();
        private readonly IntPtr[] _messageBuffer = new IntPtr[MaxMessagesPerPoll];
        private readonly List<ulong> _clientIdScratch = new List<ulong>(4);

        private Callback<SteamNetConnectionStatusChangedCallback_t> _statusCallback;
        private HSteamListenSocket _listenSocket = HSteamListenSocket.Invalid;
        private HSteamNetConnection _serverConnection = HSteamNetConnection.Invalid;
        private bool _isServer;

        private struct PendingEvent
        {
            public NetworkEvent Type;
            public ulong ClientId;
            public ArraySegment<byte> Payload;
            public float ReceiveTime;
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
            _statusCallback ??= Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
        }

        public override bool StartServer()
        {
            if (!SteamBootstrap.IsReady)
            {
                return false;
            }

            _isServer = true;
            _listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, new SteamNetworkingConfigValue_t[0]);
            return _listenSocket != HSteamListenSocket.Invalid;
        }

        public override bool StartClient()
        {
            if (!SteamBootstrap.IsReady || hostSteamId == 0)
            {
                Debug.LogError("[Parallax/Steam] StartClient sem SteamID de anfitrião.");
                return false;
            }

            _isServer = false;
            var identity = new SteamNetworkingIdentity();
            identity.SetSteamID(new CSteamID(hostSteamId));
            _serverConnection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, new SteamNetworkingConfigValue_t[0]);
            return _serverConnection != HSteamNetConnection.Invalid;
        }

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
            HSteamNetConnection connection = _isServer ? FindClient(clientId) : _serverConnection;
            if (connection == HSteamNetConnection.Invalid || payload.Array == null)
            {
                return;
            }

            int flags = networkDelivery == NetworkDelivery.Unreliable || networkDelivery == NetworkDelivery.UnreliableSequenced
                ? Constants.k_nSteamNetworkingSend_UnreliableNoNagle
                : Constants.k_nSteamNetworkingSend_ReliableNoNagle;

            GCHandle handle = GCHandle.Alloc(payload.Array, GCHandleType.Pinned);
            try
            {
                IntPtr start = IntPtr.Add(handle.AddrOfPinnedObject(), payload.Offset);
                SteamNetworkingSockets.SendMessageToConnection(connection, start, (uint)payload.Count, flags, out long _);
            }
            finally
            {
                handle.Free();
            }
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            if (_events.Count == 0)
            {
                ReceiveAll();
            }

            if (_events.Count == 0)
            {
                clientId = 0;
                payload = default;
                receiveTime = Time.realtimeSinceStartup;
                return NetworkEvent.Nothing;
            }

            PendingEvent next = _events.Dequeue();
            clientId = next.ClientId;
            payload = next.Payload;
            receiveTime = next.ReceiveTime;
            return next.Type;
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            HSteamNetConnection connection = FindClient(clientId);
            if (connection != HSteamNetConnection.Invalid)
            {
                SteamNetworkingSockets.CloseConnection(connection, 0, "Desconectado pelo anfitrião", false);
                _clients.Remove(clientId);
            }
        }

        public override void DisconnectLocalClient()
        {
            if (_serverConnection != HSteamNetConnection.Invalid)
            {
                SteamNetworkingSockets.CloseConnection(_serverConnection, 0, "Saindo da sessão", false);
                _serverConnection = HSteamNetConnection.Invalid;
            }
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            HSteamNetConnection connection = _isServer ? FindClient(clientId) : _serverConnection;
            if (connection == HSteamNetConnection.Invalid)
            {
                return 0;
            }

            var status = new SteamNetConnectionRealTimeStatus_t();
            var lanes = new SteamNetConnectionRealTimeLaneStatus_t();
            SteamNetworkingSockets.GetConnectionRealTimeStatus(connection, ref status, 0, ref lanes);
            return (ulong)Mathf.Max(0, status.m_nPing);
        }

        public override void Shutdown()
        {
            foreach (HSteamNetConnection connection in _clients.Values)
            {
                SteamNetworkingSockets.CloseConnection(connection, 0, "Sessão encerrada", false);
            }

            _clients.Clear();
            DisconnectLocalClient();

            if (_listenSocket != HSteamListenSocket.Invalid)
            {
                SteamNetworkingSockets.CloseListenSocket(_listenSocket);
                _listenSocket = HSteamListenSocket.Invalid;
            }

            _statusCallback?.Dispose();
            _statusCallback = null;
            _events.Clear();
        }

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
        {
            HSteamNetConnection connection = data.m_hConn;
            ulong id = ClientIdOf(connection);
            switch (data.m_info.m_eState)
            {
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                    if (_isServer && data.m_info.m_hListenSocket == _listenSocket)
                    {
                        SteamNetworkingSockets.AcceptConnection(connection);
                    }

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                    if (_isServer)
                    {
                        _clients[id] = connection;
                        Enqueue(NetworkEvent.Connect, id, default);
                    }
                    else
                    {
                        Enqueue(NetworkEvent.Connect, ServerClientId, default);
                    }

                    break;

                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                    SteamNetworkingSockets.CloseConnection(connection, 0, string.Empty, false);
                    if (_isServer)
                    {
                        if (_clients.Remove(id))
                        {
                            Enqueue(NetworkEvent.Disconnect, id, default);
                        }
                    }
                    else
                    {
                        _serverConnection = HSteamNetConnection.Invalid;
                        Enqueue(NetworkEvent.Disconnect, ServerClientId, default);
                    }

                    break;
            }
        }

        private void ReceiveAll()
        {
            if (_isServer)
            {
                _clientIdScratch.Clear();
                _clientIdScratch.AddRange(_clients.Keys);
                foreach (ulong id in _clientIdScratch)
                {
                    ReceiveFrom(_clients[id], id);
                }
            }
            else if (_serverConnection != HSteamNetConnection.Invalid)
            {
                ReceiveFrom(_serverConnection, ServerClientId);
            }
        }

        private void ReceiveFrom(HSteamNetConnection connection, ulong clientId)
        {
            int count = SteamNetworkingSockets.ReceiveMessagesOnConnection(connection, _messageBuffer, MaxMessagesPerPoll);
            for (int i = 0; i < count; i++)
            {
                SteamNetworkingMessage_t message = SteamNetworkingMessage_t.FromIntPtr(_messageBuffer[i]);
                var data = new byte[message.m_cbSize];
                Marshal.Copy(message.m_pData, data, 0, message.m_cbSize);
                SteamNetworkingMessage_t.Release(_messageBuffer[i]);
                Enqueue(NetworkEvent.Data, clientId, new ArraySegment<byte>(data));
            }
        }

        private void Enqueue(NetworkEvent type, ulong clientId, ArraySegment<byte> payload)
        {
            _events.Enqueue(new PendingEvent
            {
                Type = type,
                ClientId = clientId,
                Payload = payload,
                ReceiveTime = Time.realtimeSinceStartup
            });
        }

        private HSteamNetConnection FindClient(ulong clientId) =>
            _clients.TryGetValue(clientId, out HSteamNetConnection connection) ? connection : HSteamNetConnection.Invalid;

        private static ulong ClientIdOf(HSteamNetConnection connection) => connection.m_HSteamNetConnection;
#else
        public override void Initialize(NetworkManager networkManager = null) { }
        public override bool StartServer() => false;
        public override bool StartClient() => false;
        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery) { }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = 0;
            payload = default;
            receiveTime = 0f;
            return NetworkEvent.Nothing;
        }

        public override void DisconnectRemoteClient(ulong clientId) { }
        public override void DisconnectLocalClient() { }
        public override ulong GetCurrentRtt(ulong clientId) => 0;
        public override void Shutdown() { }
#endif
    }
}

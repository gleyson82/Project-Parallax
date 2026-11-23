using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Parallax.Gameplay.World
{
    /// <summary>
    /// Drone ou autômato da Diretriz Ômega com NavMeshAgent (servidor). Depois de invadido pelo Cipher com o
    /// Código-Fonte Ômega, segue o Cipher como aliado permanente.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class OverrideDrone : NetworkBehaviour
    {
        [SerializeField, Min(0.1f)] private float repathSeconds = 0.5f;
        [SerializeField, Min(0f)] private float followDistance = 2.5f;

        private readonly NetworkVariable<bool> _allied = new NetworkVariable<bool>();
        private NavMeshAgent _agent;
        private Transform _leader;

        public bool IsAllied => _allied.Value;

        private void Awake() => _agent = GetComponent<NavMeshAgent>();

        public override void OnNetworkSpawn()
        {
            _agent.enabled = IsServer;
            if (IsServer)
            {
                InvokeRepeating(nameof(Repath), repathSeconds, repathSeconds);
            }
        }

        public override void OnNetworkDespawn() => CancelInvoke(nameof(Repath));

        public void ServerSetAllied(Transform leader)
        {
            if (!IsServer)
            {
                return;
            }

            _leader = leader;
            _allied.Value = leader != null;
        }

        private void Repath()
        {
            if (!_allied.Value || _leader == null || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.stoppingDistance = followDistance;
            _agent.SetDestination(_leader.position);
        }
    }
}

using Unity.Netcode;
using UnityEngine;

namespace Parallax.Gameplay.Enemies
{
    /// <summary>Entrada das Esquadras de Contenção por portas de serviço. Só o servidor instancia.</summary>
    public sealed class TrooperWaveSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject trooperPrefab;
        [SerializeField] private Transform[] spawnPoints = new Transform[0];

        private int _nextPoint;

        public int ServerSpawn(int count, float healthMultiplier) => ServerSpawn(count, healthMultiplier, true);

        public int ServerSpawn(int count, float healthMultiplier, bool alerted)
        {
            if (!IsServer || trooperPrefab == null || spawnPoints.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < count; i++)
            {
                Transform point = spawnPoints[_nextPoint];
                _nextPoint = (_nextPoint + 1) % spawnPoints.Length;
                Vector3 jitter = new Vector3(Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.2f, 1.2f));
                NetworkObject spawned = trooperPrefab.InstantiateAndSpawn(NetworkManager, position: point.position + jitter, rotation: point.rotation);
                ContainmentTrooper trooper = spawned.GetComponent<ContainmentTrooper>();
                if (trooper != null)
                {
                    trooper.ServerConfigure(healthMultiplier);
                    if (alerted)
                    {
                        trooper.ServerAlert();
                    }
                }
            }

            return count;
        }

        public void ServerRetreatAll()
        {
            if (!IsServer)
            {
                return;
            }

            for (int i = ContainmentTrooper.All.Count - 1; i >= 0; i--)
            {
                ContainmentTrooper trooper = ContainmentTrooper.All[i];
                if (trooper != null)
                {
                    trooper.ServerRetreat();
                }
            }
        }
    }
}

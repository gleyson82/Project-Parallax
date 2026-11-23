using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    /// <summary>Onde os operadores entram na missão; os assentos se espalham lateralmente a partir daqui.</summary>
    public sealed class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float spacing = 2.5f;

        public static PlayerSpawnPoint Active { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;

        private void OnEnable() => Active = this;

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        public Vector3 SlotPosition(int slot) => transform.position + transform.right * ((slot - 1) * spacing) + Vector3.up * 0.05f;

        public Quaternion Facing => Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }
}

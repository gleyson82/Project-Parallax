using Parallax.Core.Session;
using UnityEngine;

namespace Parallax.Core.Definitions
{
    [CreateAssetMenu(menuName = "Parallax/Definitions/Session Scaling", fileName = "SessionScaling")]
    public sealed class SessionScalingProfile : ScriptableObject
    {
        [SerializeField] private SessionScalingTable table = SessionScalingTable.Canon;

        public SessionScalingTable Table => table;
    }
}

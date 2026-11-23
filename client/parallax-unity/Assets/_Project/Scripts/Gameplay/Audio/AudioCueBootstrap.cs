using UnityEngine;

namespace Parallax.Gameplay.Audio
{
    /// <summary>Registra a biblioteca de efeitos da cena no AudioCues.</summary>
    [DefaultExecutionOrder(-900)]
    public sealed class AudioCueBootstrap : MonoBehaviour
    {
        [SerializeField] private AudioCueLibrary library;

        private void Awake()
        {
            if (library != null)
            {
                AudioCues.Register(library, transform);
            }
        }
    }
}

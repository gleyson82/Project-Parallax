using System;
using UnityEngine;

namespace Parallax.Gameplay.Audio
{
    [CreateAssetMenu(menuName = "Parallax/Audio Cue Library", fileName = "AudioCueLibrary")]
    public sealed class AudioCueLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public AudioCue cue;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
            [Range(0f, 1f)] public float spatial;
        }

        [SerializeField] private Entry[] entries = new Entry[0];

        public bool TryGet(AudioCue cue, out Entry entry)
        {
            foreach (Entry candidate in entries)
            {
                if (candidate.cue == cue && candidate.clip != null)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = default;
            return false;
        }

#if UNITY_EDITOR
        public void EditorSetEntries(Entry[] newEntries) => entries = newEntries;
#endif
    }
}

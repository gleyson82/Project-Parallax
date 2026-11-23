using UnityEngine;

namespace Parallax.Gameplay.Audio
{
    public enum AudioCue : byte
    {
        ShotSlag = 0,
        ShotCryo = 1,
        ShotPulse = 2,
        Overheat = 3,
        Hit = 4,
        TrooperShot = 5,
        WeaponLockClick = 6,
        ValveCreak = 7,
        SteamBurst = 8,
        DoorRumble = 9,
        LockEngage = 10,
        RadioStatic = 11,
        Pickup = 12,
        UiTick = 13,
        Downed = 14,
        BeatStrike = 15,
        QuestComplete = 16,
        WindowExpired = 17
    }

    /// <summary>
    /// Toca efeitos com fontes reaproveitadas. A biblioteca é registrada por AudioCueBootstrap na cena.
    /// Sem biblioteca, as chamadas não fazem nada (testes e cenas de laboratório).
    /// </summary>
    public static class AudioCues
    {
        private const int PoolSize = 24;

        private static AudioCueLibrary _library;
        private static AudioSource[] _pool;
        private static int _next;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _library = null;
            _pool = null;
            _next = 0;
        }

        public static void Register(AudioCueLibrary library, Transform poolParent)
        {
            _library = library;
            _pool = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var voice = new GameObject("AudioCueVoice_" + i);
                voice.transform.SetParent(poolParent, false);
                AudioSource source = voice.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 2f;
                source.maxDistance = 45f;
                _pool[i] = source;
            }
        }

        public static void Play(AudioCue cue, Vector3 position, float volumeScale = 1f)
        {
            if (_library == null || _pool == null || !_library.TryGet(cue, out AudioCueLibrary.Entry entry))
            {
                return;
            }

            AudioSource source = _pool[_next];
            _next = (_next + 1) % _pool.Length;
            if (source == null)
            {
                return;
            }

            source.transform.position = position;
            source.spatialBlend = entry.spatial;
            source.pitch = UnityEngine.Random.Range(0.94f, 1.06f);
            source.PlayOneShot(entry.clip, entry.volume * volumeScale);
        }

        public static void Play2D(AudioCue cue, float volumeScale = 1f)
        {
            if (_library == null || _pool == null || !_library.TryGet(cue, out AudioCueLibrary.Entry entry))
            {
                return;
            }

            AudioSource source = _pool[_next];
            _next = (_next + 1) % _pool.Length;
            if (source == null)
            {
                return;
            }

            source.spatialBlend = 0f;
            source.pitch = 1f;
            source.PlayOneShot(entry.clip, entry.volume * volumeScale);
        }
    }
}

using Parallax.Core.Audio;
using UnityEngine;

namespace Parallax.Gameplay.Audio
{
    /// <summary>
    /// Nota do Piso sintetizada (40–60 Hz), sem arquivo de áudio. A altura acompanha o calor do setor.
    /// Roteie a saída para o grupo "FloorNote" do Audio Mixer.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class FloorNoteSynth : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float sectorHeat = 0.5f;
        [SerializeField, Range(0f, 1f)] private float amplitude = 0.25f;
        [SerializeField, Range(0.1f, 5f), Tooltip("Segundos para a nota deslizar até a nova altura.")]
        private float glideSeconds = 1.5f;

        private volatile float _targetFrequency = FloorNote.FrequencyFor(0.5f);
        private volatile float _amplitude = 0.25f;
        private double _phase;
        private float _currentFrequency = FloorNote.FrequencyFor(0.5f);
        private int _sampleRate;
        private float _glideSeconds;

        public float SectorHeat
        {
            get => sectorHeat;
            set
            {
                sectorHeat = Mathf.Clamp01(value);
                _targetFrequency = FloorNote.FrequencyFor(sectorHeat);
            }
        }

        private void Awake()
        {
            _sampleRate = AudioSettings.outputSampleRate;
            _glideSeconds = glideSeconds;
            SectorHeat = sectorHeat;
            _amplitude = amplitude;
            _currentFrequency = _targetFrequency;

            AudioSource source = GetComponent<AudioSource>();
            source.playOnAwake = true;
            source.loop = true;
            if (!source.isPlaying)
            {
                source.Play();
            }
        }

        private void OnValidate()
        {
            _targetFrequency = FloorNote.FrequencyFor(sectorHeat);
            _amplitude = amplitude;
            _glideSeconds = glideSeconds;
        }

        // Thread de áudio: sem APIs da Unity aqui.
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_sampleRate <= 0)
            {
                return;
            }

            float target = _targetFrequency;
            float amp = _amplitude;
            float glideStep = Mathf.Abs(FloorNote.MaxFrequencyHz - FloorNote.MinFrequencyHz) / Mathf.Max(1f, _glideSeconds * _sampleRate);
            double twoPi = 2.0 * System.Math.PI;

            for (int i = 0; i < data.Length; i += channels)
            {
                _currentFrequency = Mathf.MoveTowards(_currentFrequency, target, glideStep);
                _phase += twoPi * _currentFrequency / _sampleRate;
                if (_phase > twoPi)
                {
                    _phase -= twoPi;
                }

                float sample = (float)System.Math.Sin(_phase) * amp;
                for (int c = 0; c < channels; c++)
                {
                    data[i + c] += sample;
                }
            }
        }
    }
}

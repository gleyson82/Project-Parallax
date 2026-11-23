using System;
using System.Collections;
using Parallax.Core.Audio;
using UnityEngine;

namespace Parallax.Gameplay.Audio
{
    /// <summary>
    /// "A Batida" nos canos. Agenda as marteladas com AudioSource.PlayScheduled sobre AudioSettings.dspTime,
    /// com antecedência fixa, para que o som e o metrônomo visual fiquem alinhados na casa do milissegundo.
    /// </summary>
    public sealed class BeatSequencer : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float bpm = 96f;
        [SerializeField] private bool[] pattern = { true, false, true, true, false, false, true, false };
        [SerializeField] private AudioSource[] strikeVoices = new AudioSource[0];
        [SerializeField] private AudioClip strikeClip;
        [SerializeField] private AudioClip accentClip;
        [SerializeField, Range(0.02f, 0.5f)] private float scheduleAheadSeconds = 0.12f;
        [SerializeField, Range(0.005f, 0.1f)] private float schedulerIntervalSeconds = 0.025f;
        [SerializeField] private bool playOnEnable;

        private BeatSchedule _schedule;
        private long _nextStep;
        private int _voiceIndex;
        private Coroutine _scheduler;

        /// <summary>Disparado quando um passo é agendado: (índice, acento, horário dsp exato do som).</summary>
        public event Action<long, bool, double> StepScheduled;

        public bool IsRunning => _scheduler != null;
        public BeatSchedule Schedule => _schedule;

        private void OnEnable()
        {
            if (playOnEnable)
            {
                StartAt(AudioSettings.dspTime + scheduleAheadSeconds);
            }
        }

        private void OnDisable() => Stop();

        /// <summary>Para sincronizar em rede: converta o horário do servidor para dspTime local antes de chamar.</summary>
        public void StartAt(double dspStartTime)
        {
            Stop();
            _schedule = new BeatSchedule(bpm, pattern, dspStartTime);
            _nextStep = _schedule.NextStepAtOrAfter(AudioSettings.dspTime);
            _scheduler = StartCoroutine(SchedulerLoop());
        }

        public void Stop()
        {
            if (_scheduler != null)
            {
                StopCoroutine(_scheduler);
                _scheduler = null;
            }
        }

        /// <summary>Martelada manual (resposta do Goliath com o martelo hidráulico).</summary>
        public void StrikeNow(bool accent = true)
        {
            double when = AudioSettings.dspTime + 0.01;
            PlayAt(when, accent);
            StepScheduled?.Invoke(-1, accent, when);
        }

        private IEnumerator SchedulerLoop()
        {
            var wait = new WaitForSecondsRealtime(schedulerIntervalSeconds);
            while (true)
            {
                double horizon = AudioSettings.dspTime + scheduleAheadSeconds;
                while (_schedule.TimeOfStep(_nextStep) < horizon)
                {
                    double when = _schedule.TimeOfStep(_nextStep);
                    bool accent = _schedule.IsAccent(_nextStep);
                    if (accent || strikeClip != null)
                    {
                        PlayAt(when, accent);
                    }

                    StepScheduled?.Invoke(_nextStep, accent, when);
                    _nextStep++;
                }

                yield return wait;
            }
        }

        private void PlayAt(double dspTime, bool accent)
        {
            if (strikeVoices.Length == 0)
            {
                return;
            }

            AudioClip clip = accent && accentClip != null ? accentClip : strikeClip;
            if (clip == null)
            {
                return;
            }

            AudioSource voice = strikeVoices[_voiceIndex];
            _voiceIndex = (_voiceIndex + 1) % strikeVoices.Length;
            voice.clip = clip;
            voice.PlayScheduled(dspTime);
        }
    }
}

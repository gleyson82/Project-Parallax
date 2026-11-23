using System;
using Parallax.Core.Roles;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Parallax.Gameplay.Players
{
    /// <summary>Ações pontuais tocadas por cima do parado/andando.</summary>
    public enum CharacterAction : byte
    {
        Fire = 0,
        LiftDoor = 1,
        TurnValve = 2,
        Grapple = 3,
        Override = 4,
        Aim = 5,
        WeaponLock = 6,
        Hold = 7
    }

    /// <summary>
    /// Mistura parado/andando e ações pontuais dos modelos importados (clipes genéricos do glTF) via Playables,
    /// sem AnimatorController. A velocidade vem do deslocamento real, então funciona no dono e nas réplicas.
    /// Serve a jogadores, inimigos e Auxiliares.
    /// </summary>
    public sealed class CharacterAnimator : MonoBehaviour
    {
        [Serializable]
        public struct ActionClip
        {
            public CharacterAction action;
            public AnimationClip clip;
            public bool loop;
        }

        [Serializable]
        public struct ClassAnimationSet
        {
            public ParallaxClass parallaxClass;
            public Animator animator;
            public AnimationClip idle;
            public AnimationClip move;
            public ActionClip[] actions;
        }

        [SerializeField] private ClassAnimationSet[] sets = new ClassAnimationSet[0];
        [SerializeField, Min(0.1f)] private float speedForFullMove = 3f;
        [SerializeField, Min(0.1f)] private float blendSharpness = 8f;
        [SerializeField, Min(0.01f)] private float actionFadeSeconds = 0.15f;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _idle;
        private AnimationClipPlayable _move;
        private AnimationClipPlayable _action;
        private ClassAnimationSet _current;
        private Vector3 _lastPosition;
        private float _blend;
        private float _actionWeight;
        private float _actionEndsAt = -1f;
        private bool _actionLoops;

        public void Use(ParallaxClass parallaxClass)
        {
            for (int i = 0; i < sets.Length; i++)
            {
                if (sets[i].parallaxClass == parallaxClass)
                {
                    UseSet(i);
                    return;
                }
            }
        }

        public void UseSet(int index)
        {
            DestroyGraph();
            if (index < 0 || index >= sets.Length || sets[index].animator == null)
            {
                return;
            }

            _current = sets[index];
            AnimationClip idle = _current.idle != null ? _current.idle : _current.move;
            AnimationClip move = _current.move != null ? _current.move : _current.idle;
            if (idle == null)
            {
                return;
            }

            _current.animator.applyRootMotion = false;
            _graph = PlayableGraph.Create("ParallaxCharacter_" + name);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Animation", _current.animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 3);
            _idle = AnimationClipPlayable.Create(_graph, idle);
            _move = AnimationClipPlayable.Create(_graph, move);
            _action = AnimationClipPlayable.Create(_graph, idle);
            _graph.Connect(_idle, 0, _mixer, 0);
            _graph.Connect(_move, 0, _mixer, 1);
            _graph.Connect(_action, 0, _mixer, 2);
            output.SetSourcePlayable(_mixer);
            _graph.Play();
            _lastPosition = transform.position;
            _blend = 0f;
            _actionWeight = 0f;
            _actionEndsAt = -1f;
            ApplyWeights();
        }

        /// <summary>Toca a ação se o modelo atual tiver o clipe. Ações em loop seguem até StopAction.</summary>
        public bool PlayAction(CharacterAction action)
        {
            if (!_graph.IsValid() || _current.actions == null)
            {
                return false;
            }

            foreach (ActionClip entry in _current.actions)
            {
                if (entry.action != action || entry.clip == null)
                {
                    continue;
                }

                _graph.Disconnect(_mixer, 2);
                _action.Destroy();
                _action = AnimationClipPlayable.Create(_graph, entry.clip);
                _graph.Connect(_action, 0, _mixer, 2);
                _actionLoops = entry.loop;
                _actionEndsAt = entry.loop ? float.MaxValue : Time.time + Mathf.Max(0.1f, entry.clip.length);
                return true;
            }

            return false;
        }

        public void StopAction() => _actionEndsAt = -1f;

        // Visual: mistura e repete os clipes conforme a velocidade horizontal medida.
        private void LateUpdate()
        {
            if (!_graph.IsValid())
            {
                return;
            }

            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            Vector3 position = transform.position;
            Vector3 planar = position - _lastPosition;
            planar.y = 0f;
            _lastPosition = position;

            float target = Mathf.Clamp01(planar.magnitude / dt / speedForFullMove);
            _blend = Mathf.Lerp(_blend, target, 1f - Mathf.Exp(-blendSharpness * dt));
            float actionTarget = Time.time < _actionEndsAt ? 1f : 0f;
            _actionWeight = Mathf.MoveTowards(_actionWeight, actionTarget, dt / actionFadeSeconds);
            ApplyWeights();
            Loop(_idle);
            Loop(_move);
            if (_actionLoops)
            {
                Loop(_action);
            }
        }

        private void ApplyWeights()
        {
            float locomotion = 1f - _actionWeight;
            _mixer.SetInputWeight(0, (1f - _blend) * locomotion);
            _mixer.SetInputWeight(1, _blend * locomotion);
            _mixer.SetInputWeight(2, _actionWeight);
        }

        private void OnDestroy() => DestroyGraph();

        private static void Loop(AnimationClipPlayable playable)
        {
            AnimationClip clip = playable.GetAnimationClip();
            if (clip == null || clip.length <= 0f)
            {
                return;
            }

            double time = playable.GetTime();
            if (time > clip.length)
            {
                playable.SetTime(time % clip.length);
            }
        }

        private void DestroyGraph()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }
    }
}

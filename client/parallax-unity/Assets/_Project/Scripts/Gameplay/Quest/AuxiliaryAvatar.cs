using System.Collections;
using Parallax.Core.Roles;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Puzzles;
using Parallax.Gameplay.Session;
using UnityEngine;

namespace Parallax.Gameplay.Quest
{
    /// <summary>
    /// Presença visual de um Auxiliar junto da trava que ele cobre. Aparece só quando o papel não tem humano
    /// e executa o verbo (a Pipa sobe, o Macaco segura, o Cartucho pulsa) quando a fila o aciona. Só visual.
    /// </summary>
    public sealed class AuxiliaryAvatar : MonoBehaviour
    {
        [SerializeField] private ParallaxClass covers;
        [SerializeField] private LockActuator lockActuator;
        [SerializeField] private GameObject body;
        [SerializeField] private CharacterAnimator characterAnimator;
        [SerializeField] private Vector3 executeOffset = Vector3.zero;
        [SerializeField, Min(0.1f)] private float moveSeconds = 1.2f;
        [SerializeField] private Light statusLight;

        private Vector3 _home;
        private SessionRoster _roster;
        private Coroutine _motion;

        private void Start()
        {
            _home = transform.localPosition;
            if (characterAnimator != null)
            {
                characterAnimator.UseSet(0);
            }

            if (lockActuator != null)
            {
                lockActuator.EngagedChanged += OnEngagedChanged;
            }

            StartCoroutine(WatchRoster());
        }

        private void OnDestroy()
        {
            if (lockActuator != null)
            {
                lockActuator.EngagedChanged -= OnEngagedChanged;
            }

            if (_roster != null)
            {
                _roster.RosterChanged -= Refresh;
            }
        }

        private IEnumerator WatchRoster()
        {
            var wait = new WaitForSeconds(0.25f);
            while (true)
            {
                if (SessionRoster.Active != _roster)
                {
                    if (_roster != null)
                    {
                        _roster.RosterChanged -= Refresh;
                    }

                    _roster = SessionRoster.Active;
                    if (_roster != null)
                    {
                        _roster.RosterChanged += Refresh;
                    }

                    Refresh();
                }

                yield return wait;
            }
        }

        private void Refresh()
        {
            bool needed = _roster != null && _roster.IsSpawned && !_roster.IsHuman(covers);
            if (body != null)
            {
                body.SetActive(needed);
            }

            if (statusLight != null)
            {
                statusLight.enabled = needed;
            }
        }

        private void OnEngagedChanged(bool engaged)
        {
            bool byAuxiliary = engaged && lockActuator.EngagedByAuxiliary;
            if (_motion != null)
            {
                StopCoroutine(_motion);
            }

            _motion = StartCoroutine(MoveTo(byAuxiliary ? _home + executeOffset : _home));
            if (characterAnimator == null)
            {
                return;
            }

            if (byAuxiliary)
            {
                characterAnimator.PlayAction(CharacterAction.Hold);
            }
            else
            {
                characterAnimator.StopAction();
            }
        }

        private IEnumerator MoveTo(Vector3 target)
        {
            Vector3 start = transform.localPosition;
            for (float t = 0f; t < moveSeconds; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / moveSeconds));
                yield return null;
            }

            transform.localPosition = target;
        }
    }
}

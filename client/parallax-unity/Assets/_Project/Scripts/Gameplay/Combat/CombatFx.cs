using System.Collections;
using UnityEngine;

namespace Parallax.Gameplay.Combat
{
    public enum ImpactKind : byte
    {
        Sparks = 0,
        Steam = 1,
        Cryo = 2
    }

    /// <summary>
    /// Efeitos locais de combate (traçantes e impactos). Só apresentação: nada aqui afeta regra de jogo.
    /// A biblioteca é registrada pelo componente na cena.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class CombatFx : MonoBehaviour
    {
        private const int TracerPool = 16;

        [SerializeField] private CombatFxLibrary library;
        [SerializeField, Min(0.01f)] private float tracerSeconds = 0.07f;

        private static CombatFx _instance;
        private LineRenderer[] _tracers;
        private int _next;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        private void Awake()
        {
            _instance = this;
            _tracers = new LineRenderer[TracerPool];
            for (int i = 0; i < TracerPool; i++)
            {
                var line = new GameObject("Tracer_" + i).AddComponent<LineRenderer>();
                line.transform.SetParent(transform, false);
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.widthMultiplier = 0.045f;
                line.numCapVertices = 2;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.sharedMaterial = library != null ? library.TracerMaterial : null;
                line.enabled = false;
                _tracers[i] = line;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static void Tracer(Vector3 from, Vector3 to, Color color)
        {
            if (_instance == null)
            {
                return;
            }

            LineRenderer line = _instance._tracers[_instance._next];
            _instance._next = (_instance._next + 1) % TracerPool;
            _instance.StartCoroutine(_instance.ShowTracer(line, from, to, color));
        }

        public static void Impact(Vector3 point, Vector3 normal, ImpactKind kind)
        {
            if (_instance == null || _instance.library == null)
            {
                return;
            }

            ParticleSystem prefab = kind == ImpactKind.Steam
                ? _instance.library.SteamBurst
                : kind == ImpactKind.Cryo ? _instance.library.CryoPuff : _instance.library.ImpactSparks;
            if (prefab == null)
            {
                return;
            }

            ParticleSystem fx = Instantiate(prefab, point, Quaternion.LookRotation(normal.sqrMagnitude > 0.01f ? normal : Vector3.up));
            fx.Play();
            Destroy(fx.gameObject, 4f);
        }

        private IEnumerator ShowTracer(LineRenderer line, Vector3 from, Vector3 to, Color color)
        {
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            Color hdr = color * 4f;
            line.startColor = hdr;
            line.endColor = hdr * 0.4f;
            line.enabled = true;
            float elapsed = 0f;
            while (elapsed < tracerSeconds)
            {
                elapsed += Time.deltaTime;
                line.widthMultiplier = Mathf.Lerp(0.06f, 0.01f, elapsed / tracerSeconds);
                yield return null;
            }

            line.enabled = false;
        }
    }
}

using Parallax.Core.Roles;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Parallax.Rendering
{
    /// <summary>
    /// Escolhe o Universal Renderer da câmera local conforme a classe do jogador. Cada renderer carrega o
    /// Full Screen Pass do seu Olhar (Goliath térmico, Phantom ótico, Cipher Pele de Rede).
    /// A ordem dos índices segue a lista de renderers do Parallax_URP (ver ProjectBootstrap).
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class VisionController : MonoBehaviour
    {
        public const int NeutralRendererIndex = 0;
        public const int GoliathRendererIndex = 1;
        public const int PhantomRendererIndex = 2;
        public const int CipherRendererIndex = 3;

        [SerializeField] private bool neutralView = true;
        [SerializeField] private ParallaxClass viewer = ParallaxClass.Goliath;

        private UniversalAdditionalCameraData _cameraData;

        public ParallaxClass Viewer => viewer;
        public bool IsNeutral => neutralView;

        private void Awake()
        {
            _cameraData = GetComponent<UnityEngine.Camera>().GetUniversalAdditionalCameraData();
            Apply();
        }

        public void SetViewer(ParallaxClass newViewer)
        {
            viewer = newViewer;
            neutralView = false;
            Apply();
        }

        /// <summary>Menus, cinemáticas e espectador usam o renderer neutro.</summary>
        public void SetNeutral()
        {
            neutralView = true;
            Apply();
        }

        public static int RendererIndexFor(ParallaxClass parallaxClass)
        {
            switch (parallaxClass)
            {
                case ParallaxClass.Goliath: return GoliathRendererIndex;
                case ParallaxClass.Phantom: return PhantomRendererIndex;
                default: return CipherRendererIndex;
            }
        }

        private void Apply()
        {
            if (_cameraData == null)
            {
                return;
            }

            _cameraData.SetRenderer(neutralView ? NeutralRendererIndex : RendererIndexFor(viewer));
            if (neutralView || viewer != ParallaxClass.Cipher)
            {
                ParallaxVisionState.SetOverrideBlind(false);
            }
        }
    }
}

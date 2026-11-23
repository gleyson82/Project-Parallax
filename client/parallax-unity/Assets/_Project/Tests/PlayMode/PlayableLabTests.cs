using System;
using System.Collections;
using NUnit.Framework;
using Parallax.Core.Roles;
using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Session;
using Parallax.Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Parallax.PlayMode.Tests
{
    public sealed class PlayableLabTests
    {
        private const string LabScene = "Lab_Jogavel";

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            NetworkManager manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.Shutdown();
                yield return null;
                Object.Destroy(manager.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HostSpawnsPlayableGoliathWithOwnVisionAndCanSwapClass()
        {
            SceneManager.LoadScene(LabScene);
            yield return null;
            yield return null;

            SessionNetworkLauncher launcher = Object.FindFirstObjectByType<SessionNetworkLauncher>();
            Assert.IsNotNull(launcher, "Lab_Jogavel precisa de um SessionNetworkLauncher.");
            Assert.IsTrue(launcher.StartHost(), "O anfitrião local não subiu.");

            yield return WaitUntil(() => PlayerClassBinder.Local != null && PlayerClassBinder.Local.HasClass, 5f);
            PlayerClassBinder local = PlayerClassBinder.Local;
            Assert.AreEqual(ParallaxClass.Goliath, local.CurrentClass, "O primeiro operador ocupa a primeira classe livre.");
            Assert.AreEqual(1, SessionRoster.Active.HumanCount);

            ParallaxCameraRig rig = ParallaxCameraRig.Active;
            Assert.IsNotNull(rig);
            Assert.IsFalse(rig.Vision.IsNeutral);
            Assert.AreEqual(ParallaxClass.Goliath, rig.Vision.Viewer);

            local.RequestClass(ParallaxClass.Cipher);
            yield return WaitUntil(() => local.CurrentClass == ParallaxClass.Cipher, 5f);
            Assert.AreEqual(ParallaxClass.Cipher, local.CurrentClass);
            Assert.AreEqual(ParallaxClass.Cipher, rig.Vision.Viewer);

            yield return new WaitForSeconds(1f);
            Assert.Greater(local.transform.position.y, -0.5f, "O operador atravessou o piso.");
            CharacterController controller = local.GetComponent<CharacterController>();
            Assert.IsTrue(controller.isGrounded,
                $"O operador deveria estar apoiado no piso. pos={local.transform.position} dt={Time.deltaTime}");
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}

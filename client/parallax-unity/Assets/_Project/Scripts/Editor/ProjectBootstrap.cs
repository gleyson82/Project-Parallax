using System;
using System.Reflection;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Session;
using Parallax.Networking;
using Parallax.Rendering;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Configura o projeto de forma idempotente: settings agent-first (Force Text, Visible Meta Files,
    /// Input System), URP com um renderer por Olhar, dados base e cenas de laboratório.
    /// Linha de comando: Unity -batchmode -quit -projectPath client/parallax-unity
    ///   -executeMethod Parallax.EditorTools.ProjectBootstrap.RunBatch
    /// </summary>
    public static class ProjectBootstrap
    {
        public const float GameplayGravity = 7.79f;

        private static readonly string[] Folders =
        {
            ParallaxPaths.Data + "/Classes", ParallaxPaths.Data + "/Auxiliaries", ParallaxPaths.Data + "/Corporations",
            ParallaxPaths.Data + "/Weapons", ParallaxPaths.Data + "/Relics", ParallaxPaths.Data + "/Missions",
            ParallaxPaths.Data + "/Thermal", ParallaxPaths.Data + "/Session", ParallaxPaths.Data + "/Camera",
            ParallaxPaths.LoreGenerated, ParallaxPaths.LoreFragments, ParallaxPaths.LocalizationSource,
            ParallaxPaths.Root + "/Prefabs/Characters", ParallaxPaths.Root + "/Prefabs/Props", ParallaxPaths.Root + "/Prefabs/World",
            ParallaxPaths.Root + "/Prefabs/Session", ParallaxPaths.Scenes + "/Boot", ParallaxPaths.Scenes + "/Labs",
            ParallaxPaths.Scenes + "/Missions", ParallaxPaths.Root + "/Audio/Mixers", ParallaxPaths.Root + "/Audio/Beat",
            ParallaxPaths.Root + "/Audio/Radio", ParallaxPaths.Root + "/Audio/Ambience", ParallaxPaths.Materials + "/Vision",
            ParallaxPaths.Rendering, ParallaxPaths.Localization
        };

        [MenuItem("Parallax/Bootstrap/Run Full Bootstrap")]
        public static void RunInteractive()
        {
            Run();
            EditorUtility.DisplayDialog("PARALLAX", "Bootstrap concluído. Veja o Console para o relatório.", "OK");
        }

        public static void RunBatch()
        {
            try
            {
                Run();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void Run()
        {
            foreach (string folder in Folders)
            {
                ParallaxPaths.EnsureFolder(folder);
            }

            ConfigureEditorAndPlayer();
            UniversalRenderPipelineAsset pipeline = ConfigureRendering();
            SeedDefinitions.Run();
            LocalizationSetup.Run();
            CreateLabScene(pipeline);
            CreateBootScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Parallax/Bootstrap] Projeto configurado: Force Text, Visible Meta Files, URP com 4 renderers, dados base, Localization e cenas Boot/Lab_Olhares.");
        }

        [MenuItem("Parallax/Bootstrap/Editor And Player Settings")]
        public static void ConfigureEditorAndPlayer()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

            PlayerSettings.companyName = "HTL Games";
            PlayerSettings.productName = "PARALLAX";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = true;

            SetSerializedInt("ProjectSettings/ProjectSettings.asset", "activeInputHandler", 1);
            SetSerializedVector3("ProjectSettings/DynamicsManager.asset", "m_Gravity", new Vector3(0f, -GameplayGravity, 0f));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Parallax/Bootstrap/Rendering (URP + 3 Olhares)")]
        public static void ConfigureRenderingMenu() => ConfigureRendering();

        private static UniversalRenderPipelineAsset ConfigureRendering()
        {
            Material goliath = CreateVisionMaterial("Parallax/Vision/GoliathThermal", "Vision_GoliathThermal");
            Material phantom = CreateVisionMaterial("Parallax/Vision/PhantomOptic", "Vision_PhantomOptic");
            Material cipher = CreateVisionMaterial("Parallax/Vision/CipherNet", "Vision_CipherNet");
            Material frost = CreateVisionMaterial("Parallax/Vision/FrostBorder", "Vision_FrostBorder");

            AssignTexture(goliath, "_ThermalLut", ParallaxPaths.Root + "/Art/Textures/Vision/thermal_gradient_lut.png");
            AssignTexture(cipher, "_GlitchNoise", ParallaxPaths.Root + "/Art/Textures/Vision/omega_glitch_noise.png");
            AssignTexture(cipher, "_OverrideFrame", ParallaxPaths.Root + "/Art/UI/Vision/override_terminal_frame.png");

            UniversalRendererData neutral = CreateRenderer("Parallax_Renderer_Neutral", frost, "Frost Border");
            UniversalRendererData goliathRenderer = CreateRenderer("Parallax_Renderer_Goliath", goliath, "Olhar Goliath");
            UniversalRendererData phantomRenderer = CreateRenderer("Parallax_Renderer_Phantom", phantom, "Olhar Phantom");
            UniversalRendererData cipherRenderer = CreateRenderer("Parallax_Renderer_Cipher", cipher, "Olhar Cipher");

            string pipelinePath = ParallaxPaths.Rendering + "/Parallax_URP.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(neutral);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            var serialized = new SerializedObject(pipeline);
            SerializedProperty list = serialized.FindProperty("m_RendererDataList");
            ScriptableRendererData[] ordered = { neutral, goliathRenderer, phantomRenderer, cipherRenderer };
            list.arraySize = ordered.Length;
            for (int i = 0; i < ordered.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = ordered[i];
            }

            serialized.FindProperty("m_DefaultRendererIndex").intValue = VisionController.NeutralRendererIndex;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            var quality = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
            SerializedProperty levels = quality.FindProperty("m_QualitySettings");
            for (int i = 0; i < levels.arraySize; i++)
            {
                levels.GetArrayElementAtIndex(i).FindPropertyRelative("customRenderPipeline").objectReferenceValue = pipeline;
            }

            quality.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return pipeline;
        }

        private static Material CreateVisionMaterial(string shaderName, string assetName)
        {
            string path = ParallaxPaths.Materials + "/Vision/" + assetName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{shaderName}' não encontrado. Verifique erros de compilação de shader.");
            }

            material = new Material(shader) { name = assetName };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void AssignTexture(Material material, string property, string texturePath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null && material.HasProperty(property))
            {
                material.SetTexture(property, texture);
                EditorUtility.SetDirty(material);
            }
        }

        private static UniversalRendererData CreateRenderer(string assetName, Material fullScreenMaterial, string featureName)
        {
            string path = ParallaxPaths.Rendering + "/" + assetName + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (data == null)
            {
                data = CreateRendererDataWithDefaults(path);
            }

            if (data.rendererFeatures.Exists(f => f is FullScreenPassRendererFeature))
            {
                return data;
            }

            var feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
            feature.name = featureName;
            feature.passMaterial = fullScreenMaterial;
            feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            feature.fetchColorBuffer = true;
            AssetDatabase.AddObjectToAsset(feature, data);
            data.rendererFeatures.Add(feature);

            var serialized = new SerializedObject(data);
            SerializedProperty map = serialized.FindProperty("m_RendererFeatureMap");
            map.arraySize = data.rendererFeatures.Count;
            for (int i = 0; i < data.rendererFeatures.Count; i++)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(data.rendererFeatures[i], out string _, out long localId);
                map.GetArrayElementAtIndex(i).longValue = localId;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static UniversalRendererData CreateRendererDataWithDefaults(string path)
        {
            MethodInfo factory = typeof(UniversalRenderPipelineAsset).GetMethod(
                "CreateRendererAsset", BindingFlags.NonPublic | BindingFlags.Static);
            if (factory != null)
            {
                object created = factory.Invoke(null, new object[] { path, RendererType.UniversalRenderer, false, "Renderer" });
                if (created is UniversalRendererData data)
                {
                    return data;
                }
            }

            Debug.LogWarning("[Parallax/Bootstrap] Fábrica interna da URP indisponível; renderer criado sem PostProcessData padrão.");
            var fallback = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(fallback, path);
            return fallback;
        }

        [MenuItem("Parallax/Bootstrap/Lab Scene (3 Olhares)")]
        public static void CreateLabSceneMenu() => CreateLabScene(GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset);

        private static void CreateLabScene(UniversalRenderPipelineAsset pipeline)
        {
            string path = ParallaxPaths.Scenes + "/Labs/Lab_Olhares.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Sol de Netuno (Face Voltada)");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.62f, 0.72f, 1.0f);
            light.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Piso_Grelha";
            floor.transform.localScale = new Vector3(4f, 1f, 4f);

            CreateLabProp("Valvula_Mestra_Quente", PrimitiveType.Cylinder, new Vector3(-3f, 1f, 4f), new Color(1f, 0.35f, 0.1f));
            CreateLabProp("Porta_Chumbo_10t", PrimitiveType.Cube, new Vector3(0f, 2f, 6f), new Color(0.25f, 0.27f, 0.3f), new Vector3(4f, 4f, 0.5f));
            CreateLabProp("Viga_Alta_Arpeu", PrimitiveType.Cube, new Vector3(3f, 6f, 4f), new Color(0.4f, 0.42f, 0.45f), new Vector3(0.4f, 0.4f, 6f));
            CreateLabProp("Terminal_OmniCripto", PrimitiveType.Cube, new Vector3(4.5f, 1f, 2f), new Color(0.55f, 0.4f, 0.9f), new Vector3(1f, 2f, 0.6f));

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraObject.AddComponent<CinemachineBrain>();
            VisionController vision = cameraObject.AddComponent<VisionController>();
            vision.SetViewer(ParallaxClass.Goliath);

            var anchor = new GameObject("Operador_Alvo");
            anchor.transform.position = new Vector3(0f, 1.6f, 0f);

            var thirdPerson = new GameObject("CM_TerceiraPessoa");
            CinemachineCamera tpCamera = thirdPerson.AddComponent<CinemachineCamera>();
            CinemachineThirdPersonFollow follow = thirdPerson.AddComponent<CinemachineThirdPersonFollow>();
            var firstPerson = new GameObject("CM_PrimeiraPessoa");
            CinemachineCamera fpCamera = firstPerson.AddComponent<CinemachineCamera>();
            CinemachinePanTilt panTilt = firstPerson.AddComponent<CinemachinePanTilt>();
            firstPerson.AddComponent<CinemachineHardLockToTarget>();

            var rigObject = new GameObject("ParallaxCameraRig");
            ParallaxCameraRig rig = rigObject.AddComponent<ParallaxCameraRig>();
            var rigSerialized = new SerializedObject(rig);
            rigSerialized.FindProperty("thirdPersonCamera").objectReferenceValue = tpCamera;
            rigSerialized.FindProperty("thirdPersonFollow").objectReferenceValue = follow;
            rigSerialized.FindProperty("firstPersonCamera").objectReferenceValue = fpCamera;
            rigSerialized.FindProperty("firstPersonPanTilt").objectReferenceValue = panTilt;
            rigSerialized.FindProperty("profile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<CameraProfile>(ParallaxPaths.Data + "/Camera/CameraProfile_Goliath.asset");
            rigSerialized.ApplyModifiedPropertiesWithoutUndo();
            tpCamera.Target.TrackingTarget = anchor.transform;
            fpCamera.Target.TrackingTarget = anchor.transform;

            var lab = new GameObject("VisionLab");
            lab.AddComponent<Parallax.Gameplay.Labs.VisionLab>();

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateLabProp(string name, PrimitiveType type, Vector3 position, Color color, Vector3? scale = null)
        {
            GameObject prop = GameObject.CreatePrimitive(type);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = scale ?? Vector3.one;
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                return;
            }

            string materialPath = ParallaxPaths.Materials + "/Lab_" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(lit);
                material.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            prop.GetComponent<Renderer>().sharedMaterial = material;
        }

        [MenuItem("Parallax/Bootstrap/Boot Scene (Rede)")]
        public static void CreateBootScene()
        {
            string path = ParallaxPaths.Scenes + "/Boot/Boot.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var steam = new GameObject("SteamBootstrap");
            steam.AddComponent<SteamBootstrap>();

            var network = new GameObject("NetworkManager");
            NetworkManager manager = network.AddComponent<NetworkManager>();
            UnityTransport local = network.AddComponent<UnityTransport>();
            SteamSocketsTransport steamTransport = network.AddComponent<SteamSocketsTransport>();
            manager.NetworkConfig = new NetworkConfig { NetworkTransport = local, ConnectionApproval = true, TickRate = 30 };
            SessionNetworkLauncher launcher = network.AddComponent<SessionNetworkLauncher>();
            var launcherSerialized = new SerializedObject(launcher);
            launcherSerialized.FindProperty("networkManager").objectReferenceValue = manager;
            launcherSerialized.FindProperty("localTransport").objectReferenceValue = local;
            launcherSerialized.FindProperty("steamTransport").objectReferenceValue = steamTransport;
            launcherSerialized.ApplyModifiedPropertiesWithoutUndo();

            var session = new GameObject("Session");
            session.AddComponent<NetworkObject>();
            session.AddComponent<SessionRoster>();
            session.AddComponent<TriHandsLaunch>();
            session.AddComponent<Parallax.Gameplay.Commands.CommandQueueNetwork>();

            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(path, true),
                new EditorBuildSettingsScene(ParallaxPaths.Scenes + "/Labs/Lab_Olhares.unity", true)
            };
        }

        private static void SetSerializedInt(string settingsPath, string property, int value)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
            if (assets.Length == 0)
            {
                return;
            }

            var serialized = new SerializedObject(assets[0]);
            SerializedProperty prop = serialized.FindProperty(property);
            if (prop != null)
            {
                prop.intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetSerializedVector3(string settingsPath, string property, Vector3 value)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
            if (assets.Length == 0)
            {
                return;
            }

            var serialized = new SerializedObject(assets[0]);
            SerializedProperty prop = serialized.FindProperty(property);
            if (prop != null)
            {
                prop.vector3Value = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}

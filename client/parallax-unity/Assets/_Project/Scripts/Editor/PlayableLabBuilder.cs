using System;
using System.Linq;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Core.Definitions;
using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Characters;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Input;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Quest;
using Parallax.Gameplay.Session;
using Parallax.Gameplay.World;
using Parallax.Networking;
using Parallax.Rendering;
using Parallax.UI.Dev;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Monta a primeira versão jogável: prefab ParallaxPlayer (3 classes num só prefab), lista de prefabs de
    /// rede, cena Lab_Jogavel (host/cliente local, Olhares, arpéu, porta de 10 t, terminal) e registro no Boot.
    /// Regerável a qualquer momento: sobrescreve o prefab e a cena do laboratório.
    /// Linha de comando: -executeMethod Parallax.EditorTools.PlayableLabBuilder.RunBatch
    /// </summary>
    public static class PlayableLabBuilder
    {
        public const string PlayerPrefabPath = ParallaxPaths.Root + "/Prefabs/Characters/ParallaxPlayer.prefab";
        public const string PrefabListPath = ParallaxPaths.Root + "/Prefabs/Session/ParallaxNetworkPrefabs.asset";
        public const string LabScenePath = ParallaxPaths.Scenes + "/Labs/Lab_Jogavel.unity";
        private const string BootScenePath = ParallaxPaths.Scenes + "/Boot/Boot.unity";
        private const string OlharesScenePath = ParallaxPaths.Scenes + "/Labs/Lab_Olhares.unity";
        private const string ArtRoot = ParallaxPaths.Root + "/Art";

        private struct ClassSpec
        {
            public ParallaxClass Class;
            public string Model;
            public string Idle;
            public string Move;
            public float Height;
            public float Radius;
            public float Eye;
        }

        private static readonly ClassSpec[] Specs =
        {
            new ClassSpec { Class = ParallaxClass.Goliath, Model = ArtRoot + "/Characters/Goliath/goliath_rigged.glb", Idle = "idle_heavy", Move = "walk_heavy", Height = 2.6f, Radius = 0.55f, Eye = 2.3f },
            new ClassSpec { Class = ParallaxClass.Phantom, Model = ArtRoot + "/Characters/Phantom/phantom_rigged.glb", Idle = "idle_stealth", Move = "run_agile", Height = 1.8f, Radius = 0.35f, Eye = 1.62f },
            new ClassSpec { Class = ParallaxClass.Cipher, Model = ArtRoot + "/Characters/Cipher/cipher_rigged.glb", Idle = "idle_hack", Move = "walk_tactical", Height = 1.8f, Radius = 0.35f, Eye = 1.62f }
        };

        [MenuItem("Parallax/Bootstrap/Playable Lab (Player + Lab_Jogavel)")]
        public static void Run()
        {
            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Prefabs/Characters");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Prefabs/Session");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Scenes + "/Labs");

            GameObject player = BuildPlayerPrefab();
            NetworkPrefabsList list = BuildPrefabList(player);
            BuildLabScene(player, list);
            RegisterInBoot(player, list);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(LabScenePath, true),
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(OlharesScenePath, true)
            };

            AssetDatabase.SaveAssets();
            Debug.Log("[Parallax/PlayableLab] ParallaxPlayer, ParallaxNetworkPrefabs e Lab_Jogavel gerados; jogador registrado no Boot.");
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

        private static GameObject BuildPlayerPrefab()
        {
            var root = new GameObject("ParallaxPlayer") { tag = "Player" };
            try
            {
                CharacterController controller = root.AddComponent<CharacterController>();
                controller.height = 1.8f;
                controller.radius = 0.35f;
                controller.center = new Vector3(0f, 0.9f, 0f);
                controller.stepOffset = 0.4f;
                controller.slopeLimit = 45f;
                // Com FPS alto, o passo de gravidade de um frame fica abaixo de 1 mm e seria descartado.
                controller.minMoveDistance = 0f;

                root.AddComponent<NetworkObject>();
                NetworkTransform networkTransform = root.AddComponent<NetworkTransform>();
                networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
                networkTransform.SyncRotAngleX = false;
                networkTransform.SyncRotAngleZ = false;
                networkTransform.SyncScaleX = false;
                networkTransform.SyncScaleY = false;
                networkTransform.SyncScaleZ = false;

                var cameraTarget = new GameObject("CameraTarget").transform;
                cameraTarget.SetParent(root.transform, false);
                cameraTarget.localPosition = new Vector3(0f, 1.62f, 0f);

                ParallaxInput input = root.AddComponent<ParallaxInput>();
                Set(input, "actionsAsset", AssetDatabase.LoadAssetAtPath<InputActionAsset>(ParallaxPaths.Settings + "/Input/ParallaxControls.inputactions"));

                ParallaxCharacterMotor motor = root.AddComponent<ParallaxCharacterMotor>();
                Set(motor, "input", input);
                Set(motor, "cameraYawSource", cameraTarget);
                Set(motor, "classDefinition", LoadClass(ParallaxClass.Phantom));

                GoliathAnchor anchor = root.AddComponent<GoliathAnchor>();
                Set(anchor, "input", input);
                Set(anchor, "motor", motor);

                PhantomGrapple grapple = root.AddComponent<PhantomGrapple>();
                Set(grapple, "input", input);
                Set(grapple, "motor", motor);
                Set(grapple, "aimSource", cameraTarget);

                CipherOverride cipher = root.AddComponent<CipherOverride>();
                Set(cipher, "input", input);
                Set(cipher, "motor", motor);

                CharacterAnimator animator = root.AddComponent<CharacterAnimator>();
                PlayerClassBinder binder = root.AddComponent<PlayerClassBinder>();
                PlayerLook look = root.AddComponent<PlayerLook>();
                Set(look, "input", input);
                Set(look, "cameraTarget", cameraTarget);
                Set(look, "binder", binder);

                PlayerVitals vitals = root.AddComponent<PlayerVitals>();
                Set(vitals, "motor", motor);
                Set(vitals, "controller", controller);

                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(cameraTarget, false);
                muzzle.localPosition = new Vector3(0.2f, -0.1f, 0.35f);

                PlayerWeapon weapon = root.AddComponent<PlayerWeapon>();
                Set(weapon, "input", input);
                Set(weapon, "vitals", vitals);
                Set(weapon, "characterAnimator", animator);
                Set(weapon, "muzzle", muzzle);

                PlayerInteractor interactor = root.AddComponent<PlayerInteractor>();
                Set(interactor, "input", input);
                Set(interactor, "binder", binder);
                Set(interactor, "vitals", vitals);

                ReviveInteractable revive = root.AddComponent<ReviveInteractable>();
                Set(revive, "vitals", vitals);

                CargoPusher cargoPusher = root.AddComponent<CargoPusher>();
                Set(cargoPusher, "binder", binder);

                PlayerCombatLoadout loadout = root.AddComponent<PlayerCombatLoadout>();
                Set(loadout, "binder", binder);
                Set(loadout, "weapon", weapon);
                Set(loadout, "vitals", vitals);
                Set(loadout, "revive", revive);
                Set(loadout, "thermaTekSlag", AssetDatabase.LoadAssetAtPath<WeaponDefinition>(ParallaxPaths.Data + "/Weapons/Weapon_ThermaTek_Slag.asset"));
                Set(loadout, "kinesysCryo", AssetDatabase.LoadAssetAtPath<WeaponDefinition>(ParallaxPaths.Data + "/Weapons/Weapon_Kinesys_Cryo.asset"));
                Set(loadout, "omniPulse", AssetDatabase.LoadAssetAtPath<WeaponDefinition>(ParallaxPaths.Data + "/Weapons/Weapon_OmniCripto_Pulse.asset"));

                var animatorSerialized = new SerializedObject(animator);
                SerializedProperty sets = animatorSerialized.FindProperty("sets");
                var binderSerialized = new SerializedObject(binder);
                SerializedProperty bodies = binderSerialized.FindProperty("bodies");
                sets.arraySize = Specs.Length;
                bodies.arraySize = Specs.Length;

                Behaviour[] abilities = { anchor, grapple, cipher };
                for (int i = 0; i < Specs.Length; i++)
                {
                    ClassSpec spec = Specs[i];
                    GameObject model = InstantiateModel(spec.Model, root.transform, "Model_" + spec.Class);
                    Animator modelAnimator = model.GetComponent<Animator>();
                    if (modelAnimator == null)
                    {
                        modelAnimator = model.AddComponent<Animator>();
                    }

                    modelAnimator.runtimeAnimatorController = null;
                    modelAnimator.applyRootMotion = false;
                    AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(spec.Model).OfType<AnimationClip>().ToArray();

                    SerializedProperty set = sets.GetArrayElementAtIndex(i);
                    set.FindPropertyRelative("parallaxClass").enumValueIndex = (int)spec.Class;
                    set.FindPropertyRelative("animator").objectReferenceValue = modelAnimator;
                    set.FindPropertyRelative("idle").objectReferenceValue = FindClip(clips, spec.Idle);
                    set.FindPropertyRelative("move").objectReferenceValue = FindClip(clips, spec.Move);

                    SerializedProperty body = bodies.GetArrayElementAtIndex(i);
                    body.FindPropertyRelative("parallaxClass").enumValueIndex = (int)spec.Class;
                    body.FindPropertyRelative("definition").objectReferenceValue = LoadClass(spec.Class);
                    body.FindPropertyRelative("cameraProfile").objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<CameraProfile>($"{ParallaxPaths.Data}/Camera/CameraProfile_{spec.Class}.asset");
                    body.FindPropertyRelative("model").objectReferenceValue = model;
                    body.FindPropertyRelative("ability").objectReferenceValue = abilities[i];
                    body.FindPropertyRelative("capsuleHeight").floatValue = spec.Height;
                    body.FindPropertyRelative("capsuleRadius").floatValue = spec.Radius;
                    body.FindPropertyRelative("eyeHeight").floatValue = spec.Eye;
                }

                animatorSerialized.ApplyModifiedPropertiesWithoutUndo();
                binderSerialized.FindProperty("controller").objectReferenceValue = controller;
                binderSerialized.FindProperty("motor").objectReferenceValue = motor;
                binderSerialized.FindProperty("input").objectReferenceValue = input;
                binderSerialized.FindProperty("characterAnimator").objectReferenceValue = animator;
                binderSerialized.FindProperty("cameraTarget").objectReferenceValue = cameraTarget;
                binderSerialized.ApplyModifiedPropertiesWithoutUndo();

                return PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static NetworkPrefabsList BuildPrefabList(GameObject player)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PrefabListPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(list, PrefabListPath);
            }

            if (!list.PrefabList.Any(p => p.Prefab == player))
            {
                list.Add(new NetworkPrefab { Prefab = player });
            }

            EditorUtility.SetDirty(list);
            return list;
        }

        private static void BuildLabScene(GameObject player, NetworkPrefabsList list)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Luz de Netuno (Face Voltada)");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.62f, 0.72f, 1.0f);
            light.intensity = 1.3f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            RenderSettings.ambientLight = new Color(0.16f, 0.12f, 0.11f);

            Material floor = SurfaceMaterial("Lab_FossoPiso", "fosso_floor_grate", 12f);
            Material wall = SurfaceMaterial("Lab_FossoParede", "fosso_wall_rust", 4f);
            Material beam = SurfaceMaterial("Lab_FossoViga", "fosso_beam_hazard", 2f);

            var world = new GameObject("Mundo").transform;
            Block("Piso_Grelha", world, new Vector3(0f, -0.5f, 6f), new Vector3(48f, 1f, 48f), floor);
            Block("Parede_Norte", world, new Vector3(0f, 5f, 30f), new Vector3(48f, 10f, 1f), wall);
            Block("Parede_Sul", world, new Vector3(0f, 5f, -18f), new Vector3(48f, 10f, 1f), wall);
            Block("Parede_Leste", world, new Vector3(24f, 5f, 6f), new Vector3(1f, 10f, 48f), wall);
            Block("Parede_Oeste", world, new Vector3(-24f, 5f, 6f), new Vector3(1f, 10f, 48f), wall);

            // Passarela alta (leitura do Phantom) + rampa para Goliath e Cipher.
            Block("Passarela_Alta", world, new Vector3(0f, 6f, 13f), new Vector3(22f, 0.4f, 3f), beam);
            GameObject ramp = Block("Rampa", world, new Vector3(-9.5f, 3f, 5f), new Vector3(3f, 0.3f, 13.5f), beam);
            ramp.transform.rotation = Quaternion.Euler(-26.6f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                var point = new GameObject("PontoDeArpeu_" + (i + 1));
                point.transform.SetParent(world, false);
                point.transform.position = new Vector3(-6f + i * 4f, 6.4f, 11.6f);
                point.AddComponent<GrapplePoint>();
            }

            PlaceArt("Pilar_I_Esquerdo", ArtRoot + "/Environment/FossoDeHefesto/Meshes/fosso_pilar_ibeam.glb", world, new Vector3(-11.5f, 4f, 13f), true);
            PlaceArt("Pilar_I_Direito", ArtRoot + "/Environment/FossoDeHefesto/Meshes/fosso_pilar_ibeam.glb", world, new Vector3(11.5f, 4f, 13f), true);
            PlaceArt("Valvula_Mestra_14", ArtRoot + "/Props/ValvesAndDoors/prop_valvula_mestra_thermatek.glb", world, new Vector3(-4f, 1.17f, 9f), true);
            PlaceArt("Manometro_140", ArtRoot + "/Props/LinhasVivas/prop_manometro_140.glb", world, new Vector3(-6f, 1.4f, 9.6f), false);

            GameObject liveLine = PlaceArt("Linha_Viva", ArtRoot + "/Props/LinhasVivas/prop_linha_viva_modular.glb", world, new Vector3(6f, 0.6f, -6f), true);
            liveLine.AddComponent<LiveLine>();

            BuildHeavyDoor(world);
            BuildTerminal(world);
            BuildHeatShimmer(world);

            BuildCameraRig();
            BuildNetworking(player, list);

            EditorSceneManager.SaveScene(scene, LabScenePath);
            RefreshInSceneNetworkObjects(LabScenePath);
        }

        /// <summary>
        /// NetworkObjects de cena só ganham o GlobalObjectIdHash correto quando a cena já tem caminho salvo.
        /// Reabrir dispara o OnValidate do NGO com o caminho definitivo.
        /// </summary>
        public static void RefreshInSceneNetworkObjects(string scenePath)
        {
            Scene reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (NetworkObject networkObject in UnityEngine.Object.FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                EditorUtility.SetDirty(networkObject);
            }

            EditorSceneManager.MarkSceneDirty(reopened);
            EditorSceneManager.SaveScene(reopened);
        }

        private static void BuildHeavyDoor(Transform parent)
        {
            var door = new GameObject("Porta_Chumbo_10t");
            door.transform.SetParent(parent, false);
            door.transform.position = new Vector3(8f, 0f, 6f);
            BoxCollider collider = door.AddComponent<BoxCollider>();
            collider.size = new Vector3(4f, 4.6f, 0.5f);
            collider.center = new Vector3(0f, 2.3f, 0f);
            Rigidbody body = door.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            door.AddComponent<ConfigurableJoint>();
            door.AddComponent<NetworkObject>();
            door.AddComponent<NetworkTransform>();
            door.AddComponent<HeavyDoor>();
            InstantiateModel(ArtRoot + "/Props/ValvesAndDoors/prop_porta_chumbo_10t.glb", door.transform, "Visual").transform.localPosition = new Vector3(0f, 2.3f, 0f);
        }

        private static void BuildTerminal(Transform parent)
        {
            var terminal = new GameObject("Terminal_OmniCripto");
            terminal.transform.SetParent(parent, false);
            terminal.transform.position = new Vector3(-8f, 0f, -4f);
            BoxCollider collider = terminal.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 1.6f, 0.8f);
            collider.center = new Vector3(0f, 0.8f, 0f);
            terminal.AddComponent<NetworkObject>();
            terminal.AddComponent<OverrideTerminal>();
            GameObject visual = InstantiateModel(ArtRoot + "/Props/Terminals/prop_terminal_omnicripto.glb", terminal.transform, "Visual");
            visual.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            visual.transform.localScale = Vector3.one * 1.7f;
        }

        private static void BuildHeatShimmer(Transform parent)
        {
            Shader shader = Shader.Find("Parallax/World/HeatShimmer");
            if (shader == null)
            {
                return;
            }

            string path = ParallaxPaths.Materials + "/Lab_VaporSulfurico.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Vapor_Sulfurico";
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.position = new Vector3(-4f, 2f, 7.5f);
            quad.transform.localScale = new Vector3(3f, 4f, 1f);
            Renderer quadRenderer = quad.GetComponent<Renderer>();
            quadRenderer.sharedMaterial = material;
            quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void BuildCameraRig()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 3f, -10f);
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.nearClipPlane = 0.05f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.35f);
            VisionController vision = cameraObject.AddComponent<VisionController>();

            var thirdPerson = new GameObject("CM_TerceiraPessoa");
            CinemachineCamera tpCamera = thirdPerson.AddComponent<CinemachineCamera>();
            CinemachineThirdPersonFollow follow = thirdPerson.AddComponent<CinemachineThirdPersonFollow>();
            follow.AvoidObstacles = new CinemachineThirdPersonFollow.ObstacleSettings
            {
                Enabled = true,
                CollisionFilter = 1,
                IgnoreTag = "Player",
                CameraRadius = 0.2f,
                DampingIntoCollision = 0.05f,
                DampingFromCollision = 0.4f
            };

            var firstPerson = new GameObject("CM_PrimeiraPessoa");
            CinemachineCamera fpCamera = firstPerson.AddComponent<CinemachineCamera>();
            firstPerson.AddComponent<CinemachineHardLockToTarget>();
            firstPerson.AddComponent<CinemachineRotateWithFollowTarget>();

            var rigObject = new GameObject("ParallaxCameraRig");
            ParallaxCameraRig rig = rigObject.AddComponent<ParallaxCameraRig>();
            Set(rig, "thirdPersonCamera", tpCamera);
            Set(rig, "thirdPersonFollow", follow);
            Set(rig, "firstPersonCamera", fpCamera);
            Set(rig, "vision", vision);
            Set(rig, "profile", AssetDatabase.LoadAssetAtPath<CameraProfile>(ParallaxPaths.Data + "/Camera/CameraProfile_Phantom.asset"));
        }

        private static void BuildNetworking(GameObject player, NetworkPrefabsList list)
        {
            var network = new GameObject("NetworkManager");
            NetworkManager manager = network.AddComponent<NetworkManager>();
            UnityTransport transport = network.AddComponent<UnityTransport>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                ConnectionApproval = true,
                TickRate = 30,
                PlayerPrefab = player
            };
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);

            SessionNetworkLauncher launcher = network.AddComponent<SessionNetworkLauncher>();
            Set(launcher, "networkManager", manager);
            Set(launcher, "localTransport", transport);

            DevSessionMenu menu = network.AddComponent<DevSessionMenu>();
            Set(menu, "launcher", launcher);
            Set(menu, "networkManager", manager);

            var session = new GameObject("Session");
            session.AddComponent<NetworkObject>();
            session.AddComponent<SessionRoster>();
            session.AddComponent<TriHandsLaunch>();
            session.AddComponent<CommandQueueNetwork>();
        }

        private static void RegisterInBoot(GameObject player, NetworkPrefabsList list)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath) == null)
            {
                ProjectBootstrap.CreateBootScene();
            }

            Scene boot = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            NetworkManager manager = UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
            if (manager != null)
            {
                manager.NetworkConfig.PlayerPrefab = player;
                if (!manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(list))
                {
                    manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
                }

                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(boot);
                EditorSceneManager.SaveScene(boot);
                RefreshInSceneNetworkObjects(BootScenePath);
            }
        }

        private static GameObject InstantiateModel(string assetPath, Transform parent, string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                throw new InvalidOperationException("Modelo não encontrado: " + assetPath);
            }

            var instance = PrefabUtility.InstantiatePrefab(asset, parent) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(asset, parent);
            }

            instance.name = name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        private static GameObject PlaceArt(string name, string assetPath, Transform parent, Vector3 position, bool solid)
        {
            GameObject instance = InstantiateModel(assetPath, parent, name);
            instance.transform.position = position;
            if (solid)
            {
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    foreach (Renderer meshRenderer in renderers)
                    {
                        bounds.Encapsulate(meshRenderer.bounds);
                    }

                    BoxCollider collider = instance.AddComponent<BoxCollider>();
                    collider.center = instance.transform.InverseTransformPoint(bounds.center);
                    collider.size = bounds.size;
                }
            }

            return instance;
        }

        private static GameObject Block(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = scale;
            if (material != null)
            {
                block.GetComponent<Renderer>().sharedMaterial = material;
            }

            return block;
        }

        private static Material SurfaceMaterial(string assetName, string textureSet, float tiling)
        {
            string path = ParallaxPaths.Materials + "/" + assetName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader lit = Shader.Find("Universal Render Pipeline/Lit");
                if (lit == null)
                {
                    return null;
                }

                material = new Material(lit);
                AssetDatabase.CreateAsset(material, path);
            }

            string folder = ArtRoot + "/Environment/FossoDeHefesto/Textures/";
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + textureSet + "_albedo.png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + textureSet + "_normal.png");
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + textureSet + "_metallicSmoothness.png");
            material.SetTexture("_BaseMap", albedo);
            material.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }

            if (mask != null)
            {
                material.SetTexture("_MetallicGlossMap", mask);
                material.SetFloat("_Smoothness", 1f);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static ClassDefinition LoadClass(ParallaxClass parallaxClass) =>
            AssetDatabase.LoadAssetAtPath<ClassDefinition>($"{ParallaxPaths.Data}/Classes/Class_{parallaxClass}.asset");

        private static AnimationClip FindClip(AnimationClip[] clips, string name) =>
            clips.FirstOrDefault(c => c.name == name) ?? clips.FirstOrDefault(c => c.name.Contains(name));

        private static void Set(UnityEngine.Object target, string property, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty prop = serialized.FindProperty(property);
            if (prop == null)
            {
                throw new InvalidOperationException($"Campo '{property}' não existe em {target.GetType().Name}.");
            }

            prop.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

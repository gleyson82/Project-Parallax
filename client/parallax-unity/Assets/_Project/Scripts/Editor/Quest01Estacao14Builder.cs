using System;
using System.Linq;
using Parallax.Core.Definitions;
using Parallax.Core.Quest;
using Parallax.Core.Roles;
using Parallax.Gameplay.Audio;
using Parallax.Gameplay.Cameras;
using Parallax.Gameplay.Combat;
using Parallax.Gameplay.Commands;
using Parallax.Gameplay.Enemies;
using Parallax.Gameplay.Interaction;
using Parallax.Gameplay.Players;
using Parallax.Gameplay.Puzzles;
using Parallax.Gameplay.Quest;
using Parallax.Gameplay.Session;
using Parallax.Gameplay.Thermal;
using Parallax.Gameplay.World;
using Parallax.Networking;
using Parallax.Rendering;
using Parallax.UI.Dev;
using Parallax.UI.Mission;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Monta a fatia vertical apresentável da Quest 1 — Estação de Válvula 14.
    /// Prefabs de inimigo/núcleo, cena com 3 zonas, Chave Tripla, contêiner, válvula, HUD e build settings.
    /// Linha de comando: -executeMethod Parallax.EditorTools.Quest01Estacao14Builder.RunBatch
    /// </summary>
    public static class Quest01Estacao14Builder
    {
        public const string ScenePath = ParallaxPaths.Scenes + "/Missions/Quest01_Estacao14.unity";
        public const string TrooperPrefabPath = ParallaxPaths.Root + "/Prefabs/Enemies/ContainmentTrooper.prefab";
        public const string CorePrefabPath = ParallaxPaths.Root + "/Prefabs/Items/ThermalCore.prefab";

        private const string ArtRoot = ParallaxPaths.Root + "/Art";
        private const string BootScenePath = ParallaxPaths.Scenes + "/Boot/Boot.unity";

        [MenuItem("Parallax/Bootstrap/Quest 1 · Estação 14 (fatia vertical)")]
        public static void Run()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            ProjectBootstrap.ConfigureEditorAndPlayer();
            SeedDefinitions.Run();
            PlayableLabBuilder.Run();

            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Prefabs/Enemies");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Prefabs/Items");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Scenes + "/Missions");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Data + "/Combat");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Audio/Cues");

            NetworkObject core = BuildThermalCorePrefab();
            NetworkObject trooper = BuildTrooperPrefab(core);
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayableLabBuilder.PlayerPrefabPath);
            NetworkPrefabsList list = EnsurePrefabList(player, trooper.gameObject, core.gameObject);
            BuildMissionScene(player, list, trooper, core);
            RegisterBuildSettings();

            AssetDatabase.SaveAssets();
            Debug.Log("[Parallax/Quest01] Quest01_Estacao14, ContainmentTrooper e ThermalCore gerados. Abra a cena e Hospede a sessão.");
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

        private static NetworkPrefabsList EnsurePrefabList(GameObject player, GameObject trooper, GameObject core)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(PlayableLabBuilder.PrefabListPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(list, PlayableLabBuilder.PrefabListPath);
            }

            foreach (GameObject prefab in new[] { player, trooper, core })
            {
                if (prefab != null && !list.PrefabList.Any(p => p.Prefab == prefab))
                {
                    list.Add(new NetworkPrefab { Prefab = prefab });
                }
            }

            EditorUtility.SetDirty(list);
            return list;
        }

        private static NetworkObject BuildThermalCorePrefab()
        {
            var root = new GameObject("ThermalCore");
            try
            {
                root.AddComponent<NetworkObject>();
                SphereCollider trigger = root.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.45f;

                ThermalNode thermal = root.AddComponent<ThermalNode>();
                Set(thermal, "profile", AssetDatabase.LoadAssetAtPath<ThermalProfile>(ParallaxPaths.Data + "/Thermal/Thermal_NucleoTermico.asset"));

                ThermalCorePickup pickup = root.AddComponent<ThermalCorePickup>();
                GameObject visual = InstantiateModel(ArtRoot + "/Props/Items/item_nucleo_termico.glb", root.transform, "Visual");
                visual.transform.localScale = Vector3.one * 1.4f;
                Set(pickup, "visual", visual.transform);

                var lightObject = new GameObject("Glow");
                lightObject.transform.SetParent(root.transform, false);
                Light glow = lightObject.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = new Color(1f, 0.45f, 0.15f);
                glow.intensity = 5f;
                glow.range = 4f;
                Set(pickup, "glow", glow);

                return PrefabUtility.SaveAsPrefabAsset(root, CorePrefabPath).GetComponent<NetworkObject>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static NetworkObject BuildTrooperPrefab(NetworkObject corePrefab)
        {
            var root = new GameObject("ContainmentTrooper");
            try
            {
                root.AddComponent<NetworkObject>();
                CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
                capsule.height = 1.8f;
                capsule.radius = 0.35f;
                capsule.center = new Vector3(0f, 0.9f, 0f);

                NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
                agent.speed = 3.4f;
                agent.angularSpeed = 220f;
                agent.acceleration = 10f;
                agent.stoppingDistance = 7f;
                agent.radius = 0.35f;
                agent.height = 1.8f;

                GameObject model = InstantiateModel(ArtRoot + "/Enemies/EsquadraContencao/inimigo_esquadra_contencao.glb", root.transform, "Visual");
                CharacterAnimator animator = root.AddComponent<CharacterAnimator>();
                Animator modelAnimator = model.GetComponentInChildren<Animator>() ?? model.AddComponent<Animator>();
                var animatorSerialized = new SerializedObject(animator);
                SerializedProperty sets = animatorSerialized.FindProperty("sets");
                sets.arraySize = 1;
                SerializedProperty set = sets.GetArrayElementAtIndex(0);
                set.FindPropertyRelative("parallaxClass").enumValueIndex = 0;
                set.FindPropertyRelative("animator").objectReferenceValue = modelAnimator;
                animatorSerialized.ApplyModifiedPropertiesWithoutUndo();

                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(root.transform, false);
                muzzle.localPosition = new Vector3(0.25f, 1.35f, 0.55f);

                var ledObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ledObject.name = "LockLed";
                UnityEngine.Object.DestroyImmediate(ledObject.GetComponent<Collider>());
                ledObject.transform.SetParent(root.transform, false);
                ledObject.transform.localPosition = new Vector3(0f, 1.55f, 0.25f);
                ledObject.transform.localScale = Vector3.one * 0.08f;

                ContainmentTrooper trooper = root.AddComponent<ContainmentTrooper>();
                Set(trooper, "characterAnimator", animator);
                Set(trooper, "muzzle", muzzle);
                Set(trooper, "lockLed", ledObject.GetComponent<Renderer>());
                Set(trooper, "visualRoot", model.transform);
                Set(trooper, "thermalCorePrefab", corePrefab);

                return PrefabUtility.SaveAsPrefabAsset(root, TrooperPrefabPath).GetComponent<NetworkObject>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void BuildMissionScene(GameObject player, NetworkPrefabsList list, NetworkObject trooper, NetworkObject core)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material floor = SurfaceMaterial("Quest01_Piso", "fosso_floor_grate", 10f);
            Material wall = SurfaceMaterial("Quest01_Parede", "fosso_wall_rust", 3.5f);
            Material beam = SurfaceMaterial("Quest01_Viga", "fosso_beam_hazard", 2f);
            Material hazard = SurfaceMaterial("Quest01_Hazard", "fosso_beam_hazard", 1f);

            BuildLighting();
            var world = new GameObject("Estacao14").transform;

            // Zona A — Exterior / Câmara de Despacho
            Block("Piso_A", world, new Vector3(0f, -0.5f, -6f), new Vector3(36f, 1f, 28f), floor);
            Block("Parede_A_N", world, new Vector3(0f, 5f, 8f), new Vector3(36f, 10f, 1f), wall);
            Block("Parede_A_S", world, new Vector3(0f, 5f, -20f), new Vector3(36f, 10f, 1f), wall);
            Block("Parede_A_L", world, new Vector3(18f, 5f, -6f), new Vector3(1f, 10f, 28f), wall);
            Block("Parede_A_O", world, new Vector3(-18f, 5f, -6f), new Vector3(1f, 10f, 28f), wall);
            Block("Passarela_Phantom", world, new Vector3(0f, 6.2f, 2f), new Vector3(18f, 0.35f, 2.4f), beam);
            GameObject ramp = Block("Rampa_Passarela", world, new Vector3(-10f, 3f, -4f), new Vector3(3f, 0.3f, 12f), beam);
            ramp.transform.rotation = Quaternion.Euler(-28f, 0f, 0f);

            // Zona B — Casa de caldeiras / trilho do contêiner
            Block("Piso_B", world, new Vector3(0f, -0.5f, 22f), new Vector3(36f, 1f, 28f), floor);
            Block("Parede_B_N", world, new Vector3(0f, 5f, 36f), new Vector3(36f, 10f, 1f), wall);
            Block("Trilho", world, new Vector3(0f, 0.05f, 16f), new Vector3(14f, 0.1f, 0.8f), hazard);

            // Zona C — Duto de Sangria / defesa
            Block("Piso_C", world, new Vector3(22f, -0.5f, 22f), new Vector3(20f, 1f, 20f), floor);
            Block("Barreira_Defesa", world, new Vector3(14f, 1f, 22f), new Vector3(0.5f, 2f, 8f), beam);

            PlaceArt("Pilar_I_1", ArtRoot + "/Environment/FossoDeHefesto/Meshes/fosso_pilar_ibeam.glb", world, new Vector3(-12f, 4f, 0f), true);
            PlaceArt("Pilar_I_2", ArtRoot + "/Environment/FossoDeHefesto/Meshes/fosso_pilar_ibeam.glb", world, new Vector3(12f, 4f, 0f), true);
            PlaceArt("Pilar_I_3", ArtRoot + "/Environment/FossoDeHefesto/Meshes/fosso_pilar_ibeam.glb", world, new Vector3(-8f, 4f, 20f), true);
            PlaceArt("Duto_Sangria", ArtRoot + "/Props/ValvesAndDoors/prop_duto_sangria_estacao14.glb", world, new Vector3(22f, 0f, 22f), true);

            BuildHeatShimmer(world, new Vector3(20f, 2.2f, 22f));
            BuildNavMesh(world);

            var systems = new GameObject("Sistemas").transform;
            systems.SetParent(world, false);

            CommandQueueNetwork queue = BuildCommandQueue(systems);
            DispatchChamber chamber = BuildDispatchChamber(world, queue);
            PushableCargo cargo = BuildCargo(world);
            ValveWheel valve = BuildValve(world);
            TrooperWaveSpawner spawner = BuildWaveSpawner(world, trooper);
            BuildLore(world);
            BuildLiveLines(world);
            BuildBrasaAndSpawn(world);
            BuildFx(systems);

            Quest01Director director = systems.gameObject.AddComponent<Quest01Director>();
            Set(director, "dispatchChamber", chamber);
            Set(director, "cargo", cargo);
            Set(director, "valve", valve);
            Set(director, "waveSpawner", spawner);
            Set(director, "scalingProfile", AssetDatabase.LoadAssetAtPath<SessionScalingProfile>(ParallaxPaths.Data + "/Session/SessionScaling.asset"));
            systems.gameObject.AddComponent<NetworkObject>();

            BuildCameraRig();
            BuildNetworking(player, list);
            new GameObject("MissionHud").AddComponent<MissionHud>();

            // Salva em YAML antes do bake: NavMeshData embutido em sessão binária previa corrompia o ForceText.
            EditorSceneManager.SaveScene(scene, ScenePath);
            PlayableLabBuilder.RefreshInSceneNetworkObjects(ScenePath);
            BakeNavMeshToAsset();
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(new[] { ScenePath }, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
        }

        [MenuItem("Parallax/Bootstrap/Reserialize Quest01 Scene (Force Text)")]
        public static void ReserializeQuest01Scene()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            ProjectBootstrap.ConfigureEditorAndPlayer();
            if (!System.IO.File.Exists(ScenePath.Replace("Assets/", Application.dataPath + "/").Replace('/', System.IO.Path.DirectorySeparatorChar))
                && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogError("Cena Quest01 ausente. Rode Parallax/Bootstrap/Quest 1 primeiro.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.ForceReserializeAssets(new[] { ScenePath }, ForceReserializeAssetsOptions.ReserializeAssets);
            Debug.Log("[Parallax/Quest01] Cena resserializada em Force Text.");
        }

        public static void ReserializeQuest01SceneBatch()
        {
            try
            {
                ReserializeQuest01Scene();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void BuildLighting()
        {
            var sun = new GameObject("Luz de Netuno");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.55f, 0.68f, 1f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, -48f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.14f, 0.1f, 0.09f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.08f, 0.07f, 0.09f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;

            var fill = new GameObject("Fill_ThermaTek");
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.color = new Color(1f, 0.45f, 0.2f);
            fillLight.intensity = 4.5f;
            fillLight.range = 18f;
            fill.transform.position = new Vector3(-4f, 4f, 10f);

            var bleed = new GameObject("Fill_Sangria");
            Light bleedLight = bleed.AddComponent<Light>();
            bleedLight.type = LightType.Point;
            bleedLight.color = new Color(1f, 0.35f, 0.12f);
            bleedLight.intensity = 7f;
            bleedLight.range = 16f;
            bleed.transform.position = new Vector3(22f, 3.5f, 22f);
        }

        private static CommandQueueNetwork BuildCommandQueue(Transform parent)
        {
            var session = new GameObject("Session");
            session.transform.SetParent(parent, false);
            session.AddComponent<NetworkObject>();
            session.AddComponent<SessionRoster>();
            session.AddComponent<TriHandsLaunch>();
            return session.AddComponent<CommandQueueNetwork>();
        }

        private static DispatchChamber BuildDispatchChamber(Transform world, CommandQueueNetwork queue)
        {
            var root = new GameObject("Camara_Despacho");
            root.transform.SetParent(world, false);
            root.transform.position = new Vector3(0f, 0f, 6f);
            root.AddComponent<NetworkObject>();

            // Porta de chumbo (Goliath)
            var door = new GameObject("Porta_Chumbo");
            door.transform.SetParent(root.transform, false);
            door.transform.localPosition = new Vector3(0f, 0f, 0f);
            BoxCollider doorCollider = door.AddComponent<BoxCollider>();
            doorCollider.size = new Vector3(4.2f, 4.6f, 0.55f);
            doorCollider.center = new Vector3(0f, 2.3f, 0f);
            Rigidbody body = door.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            door.AddComponent<ConfigurableJoint>();
            door.AddComponent<NetworkObject>();
            door.AddComponent<NetworkTransform>();
            HeavyDoor heavyDoor = door.AddComponent<HeavyDoor>();
            InstantiateModel(ArtRoot + "/Props/ValvesAndDoors/prop_porta_chumbo_10t.glb", door.transform, "Visual").transform.localPosition = new Vector3(0f, 2.3f, 0f);

            // Terminal Cipher
            var terminal = new GameObject("Terminal_OmniCripto");
            terminal.transform.SetParent(root.transform, false);
            terminal.transform.localPosition = new Vector3(-3.2f, 0f, -2.2f);
            BoxCollider termCollider = terminal.AddComponent<BoxCollider>();
            termCollider.size = new Vector3(1.2f, 1.6f, 0.8f);
            termCollider.center = new Vector3(0f, 0.8f, 0f);
            terminal.AddComponent<NetworkObject>();
            OverrideTerminal overrideTerminal = terminal.AddComponent<OverrideTerminal>();
            GameObject termVisual = InstantiateModel(ArtRoot + "/Props/Terminals/prop_terminal_omnicripto.glb", terminal.transform, "Visual");
            termVisual.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            termVisual.transform.localScale = Vector3.one * 1.6f;

            // Travas
            LockActuator goliathLock = MakeLock(root.transform, "Trava_ThermaTek", ParallaxClass.Goliath, new Vector3(1.8f, 1.4f, -0.4f));
            LockActuator phantomLock = MakeLock(root.transform, "Trava_Kinesys", ParallaxClass.Phantom, new Vector3(0f, 6.4f, -0.2f));
            LockActuator cipherLock = MakeLock(root.transform, "Trava_OmniCripto", ParallaxClass.Cipher, new Vector3(-3.2f, 1.4f, -1.4f));

            TripleLockPuzzle puzzle = root.AddComponent<TripleLockPuzzle>();
            Set(puzzle, "thermaTekLock", goliathLock);
            Set(puzzle, "kinesysLock", phantomLock);
            Set(puzzle, "omniCriptoLock", cipherLock);
            Set(puzzle, "commandQueue", queue);
            Set(puzzle, "scalingProfile", AssetDatabase.LoadAssetAtPath<SessionScalingProfile>(ParallaxPaths.Data + "/Session/SessionScaling.asset"));
            var puzzleSerialized = new SerializedObject(puzzle);
            puzzleSerialized.FindProperty("baseWindowSeconds").floatValue = 20f;
            puzzleSerialized.ApplyModifiedPropertiesWithoutUndo();

            MakeLockInteractable(root.transform, "Interagir_Goliath", goliathLock, queue, new Vector3(1.8f, 1f, -1.2f),
                "Ancorar · sustentar a porta", true);
            MakeLockInteractable(root.transform, "Interagir_Phantom", phantomLock, queue, new Vector3(0f, 6.2f, -1f),
                "Abrir a sangria superior", false);
            MakeLockInteractable(root.transform, "Interagir_Cipher", cipherLock, queue, new Vector3(-3.2f, 1f, -2.8f),
                "Invadir o terminal", true);

            var console = new GameObject("Console_Ensaio");
            console.transform.SetParent(root.transform, false);
            console.transform.localPosition = new Vector3(3.5f, 0f, -2.5f);
            BoxCollider consoleCollider = console.AddComponent<BoxCollider>();
            consoleCollider.size = new Vector3(1f, 1.4f, 0.7f);
            consoleCollider.center = new Vector3(0f, 0.7f, 0f);
            console.AddComponent<NetworkObject>();
            RehearsalConsole rehearsal = console.AddComponent<RehearsalConsole>();
            Set(rehearsal, "commandQueue", queue);
            GameObject consoleVisual = Block("Console_Visual", console.transform, console.transform.position + Vector3.up * 0.7f,
                new Vector3(0.9f, 1.2f, 0.5f), SurfaceMaterial("Quest01_Console", "fosso_wall_rust", 1f));
            consoleVisual.transform.SetParent(console.transform, true);

            MakeAuxAvatar(root.transform, ParallaxClass.Goliath, goliathLock, ArtRoot + "/Auxiliaries/auxiliar_macaco.glb", new Vector3(2.4f, 0f, -0.8f));
            MakeAuxAvatar(root.transform, ParallaxClass.Phantom, phantomLock, ArtRoot + "/Auxiliaries/auxiliar_pipa.glb", new Vector3(0.8f, 6.2f, -0.6f));
            MakeAuxAvatar(root.transform, ParallaxClass.Cipher, cipherLock, ArtRoot + "/Auxiliaries/auxiliar_cartucho_override.glb", new Vector3(-2.4f, 0.6f, -2f));

            for (int i = 0; i < 3; i++)
            {
                var point = new GameObject("PontoDeArpeu_" + (i + 1));
                point.transform.SetParent(root.transform, false);
                point.transform.localPosition = new Vector3(-4f + i * 4f, 6.5f, 1.2f);
                point.AddComponent<GrapplePoint>();
            }

            DispatchChamber chamber = root.AddComponent<DispatchChamber>();
            Set(chamber, "puzzle", puzzle);
            Set(chamber, "door", heavyDoor);
            Set(chamber, "terminal", overrideTerminal);
            Set(chamber, "commandQueue", queue);
            return chamber;
        }

        private static LockActuator MakeLock(Transform parent, string name, ParallaxClass role, Vector3 localPos)
        {
            var lockObject = new GameObject(name);
            lockObject.transform.SetParent(parent, false);
            lockObject.transform.localPosition = localPos;
            lockObject.AddComponent<NetworkObject>();
            LockActuator actuator = lockObject.AddComponent<LockActuator>();
            var serialized = new SerializedObject(actuator);
            serialized.FindProperty("role").enumValueIndex = (int)role;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cube);
            led.name = "Led";
            UnityEngine.Object.DestroyImmediate(led.GetComponent<Collider>());
            led.transform.SetParent(lockObject.transform, false);
            led.transform.localScale = new Vector3(0.2f, 0.2f, 0.08f);
            return actuator;
        }

        private static void MakeLockInteractable(Transform parent, string name, LockActuator actuator, CommandQueueNetwork queue,
            Vector3 localPos, string label, bool viaAbility)
        {
            var interact = new GameObject(name);
            interact.transform.SetParent(parent, false);
            interact.transform.localPosition = localPos;
            BoxCollider collider = interact.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 1.6f, 1.2f);
            collider.center = new Vector3(0f, 0.8f, 0f);
            interact.AddComponent<NetworkObject>();
            LockInteractable lockInteractable = interact.AddComponent<LockInteractable>();
            Set(lockInteractable, "lockActuator", actuator);
            Set(lockInteractable, "commandQueue", queue);
            var serialized = new SerializedObject(lockInteractable);
            serialized.FindProperty("humanActionLabel").stringValue = label;
            serialized.FindProperty("humanActsViaAbility").boolValue = viaAbility;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MakeAuxAvatar(Transform parent, ParallaxClass covers, LockActuator actuator, string modelPath, Vector3 localPos)
        {
            var avatar = new GameObject("Auxiliar_" + covers);
            avatar.transform.SetParent(parent, false);
            avatar.transform.localPosition = localPos;
            GameObject body = InstantiateModel(modelPath, avatar.transform, "Body");
            body.transform.localScale = Vector3.one * 0.9f;
            AuxiliaryAvatar component = avatar.AddComponent<AuxiliaryAvatar>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("covers").enumValueIndex = (int)covers;
            serialized.FindProperty("lockActuator").objectReferenceValue = actuator;
            serialized.FindProperty("body").objectReferenceValue = body;
            serialized.FindProperty("executeOffset").vector3Value = new Vector3(0f, 0f, 0.8f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PushableCargo BuildCargo(Transform world)
        {
            var cargo = new GameObject("Conteiner_Acumuladores");
            cargo.transform.SetParent(world, false);
            cargo.transform.position = new Vector3(-5f, 0.7f, 12f);
            BoxCollider collider = cargo.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.2f, 1.6f, 1.4f);
            Rigidbody body = cargo.AddComponent<Rigidbody>();
            body.mass = 420f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            cargo.AddComponent<NetworkObject>();
            cargo.AddComponent<NetworkTransform>();
            InstantiateModel(ArtRoot + "/Props/ValvesAndDoors/prop_conteiner_acumuladores.glb", cargo.transform, "Visual");

            var delivery = new GameObject("Ponto_Entrega");
            delivery.transform.SetParent(world, false);
            delivery.transform.position = new Vector3(8f, 0.5f, 16f);

            var macaco = new GameObject("Macaco_Reboque");
            macaco.transform.SetParent(cargo.transform, false);
            macaco.transform.localPosition = new Vector3(-1.4f, 0f, 0f);
            InstantiateModel(ArtRoot + "/Auxiliaries/auxiliar_macaco.glb", macaco.transform, "Visual");
            macaco.SetActive(false);

            PushableCargo pushable = cargo.AddComponent<PushableCargo>();
            Set(pushable, "deliveryPoint", delivery.transform);
            Set(pushable, "macacoTowVisual", macaco);
            var serialized = new SerializedObject(pushable);
            serialized.FindProperty("railAxis").vector3Value = Vector3.right;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return pushable;
        }

        private static ValveWheel BuildValve(Transform world)
        {
            var valve = new GameObject("Valvula_Mestra_14");
            valve.transform.SetParent(world, false);
            valve.transform.position = new Vector3(18f, 1.1f, 22f);
            BoxCollider collider = valve.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.4f, 1.8f, 1.4f);
            collider.center = new Vector3(0f, 0.2f, 0f);
            valve.AddComponent<NetworkObject>();
            GameObject visual = InstantiateModel(ArtRoot + "/Props/ValvesAndDoors/prop_valvula_mestra_thermatek.glb", valve.transform, "Wheel");
            ValveWheel wheel = valve.AddComponent<ValveWheel>();
            Set(wheel, "wheel", visual.transform);

            var steam = new GameObject("Steam");
            steam.transform.SetParent(valve.transform, false);
            steam.transform.localPosition = new Vector3(0f, 0.8f, 0.6f);
            ParticleSystem ps = steam.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.7f, 0.4f, 0.45f);
            main.startSize = 0.6f;
            main.startLifetime = 4f;
            main.loop = false;
            main.playOnAwake = false;
            Set(wheel, "releaseSteam", ps);
            return wheel;
        }

        private static TrooperWaveSpawner BuildWaveSpawner(Transform world, NetworkObject trooper)
        {
            var spawnerObject = new GameObject("Esquadra_Spawner");
            spawnerObject.transform.SetParent(world, false);
            spawnerObject.AddComponent<NetworkObject>();
            TrooperWaveSpawner spawner = spawnerObject.AddComponent<TrooperWaveSpawner>();
            Set(spawner, "trooperPrefab", trooper);

            var points = new Transform[4];
            Vector3[] positions =
            {
                new Vector3(30f, 0f, 28f),
                new Vector3(30f, 0f, 18f),
                new Vector3(26f, 0f, 32f),
                new Vector3(34f, 0f, 22f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                var point = new GameObject("Spawn_" + (i + 1));
                point.transform.SetParent(spawnerObject.transform, false);
                point.transform.position = positions[i];
                point.transform.LookAt(new Vector3(18f, 0f, 22f));
                points[i] = point.transform;
            }

            var serialized = new SerializedObject(spawner);
            SerializedProperty array = serialized.FindProperty("spawnPoints");
            array.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return spawner;
        }

        private static void BuildLore(Transform world)
        {
            MakeLore(world, "Manometro_140", new Vector3(-6f, 1.4f, 14f),
                ArtRoot + "/Props/LinhasVivas/prop_manometro_140.glb",
                "Manômetro 140%",
                "Casa de caldeiras · pressão 140%. O metal canta.",
                Quest01Secondary.Manometro,
                "Fragment_manometro-da-estacao-14");

            MakeLore(world, "Relogio_23", new Vector3(4f, 1.6f, 20f), null,
                "Relógio riscado",
                "Mostrador riscado a prego. Vinte e três anos. O ponteiro não anda — conta.",
                Quest01Secondary.Relogio,
                null);

            MakeLore(world, "Etiqueta_Cota", new Vector3(10f, 1.2f, 18f), null,
                "Etiqueta de cota",
                "Válvula de moradia · sobrenome raspado · cota abaixo do mínimo.",
                Quest01Secondary.Etiqueta,
                "Fragment_etiqueta-de-cota");
        }

        private static void MakeLore(Transform world, string name, Vector3 position, string artPath, string title,
            string inscription, Quest01Secondary secondary, string fragmentName)
        {
            var lore = new GameObject(name);
            lore.transform.SetParent(world, false);
            lore.transform.position = position;
            BoxCollider collider = lore.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.8f, 1.2f, 0.4f);
            collider.center = new Vector3(0f, 0.6f, 0f);
            lore.AddComponent<NetworkObject>();
            if (!string.IsNullOrEmpty(artPath))
            {
                InstantiateModel(artPath, lore.transform, "Visual");
            }
            else
            {
                Block(name + "_Placa", lore.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.6f, 0.5f, 0.08f),
                    SurfaceMaterial("Quest01_Placa", "fosso_beam_hazard", 1f));
            }

            LoreInspectable inspectable = lore.AddComponent<LoreInspectable>();
            var serialized = new SerializedObject(inspectable);
            serialized.FindProperty("title").stringValue = title;
            serialized.FindProperty("inscription").stringValue = inscription;
            serialized.FindProperty("secondary").intValue = (int)secondary;
            if (!string.IsNullOrEmpty(fragmentName))
            {
                serialized.FindProperty("fragment").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<LoreFragment>($"{ParallaxPaths.LoreFragments}/{fragmentName}.asset");
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildLiveLines(Transform world)
        {
            Vector3[] positions =
            {
                new Vector3(12f, 0.6f, 20f),
                new Vector3(16f, 0.6f, 26f),
                new Vector3(20f, 0.6f, 18f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject line = PlaceArt("Linha_Viva_" + (i + 1), ArtRoot + "/Props/LinhasVivas/prop_linha_viva_modular.glb",
                    world, positions[i], true);
                line.AddComponent<LiveLine>();
            }
        }

        private static void BuildBrasaAndSpawn(Transform world)
        {
            var spawn = new GameObject("Spawn_Operadores");
            spawn.transform.SetParent(world, false);
            spawn.transform.position = new Vector3(0f, 0f, -14f);
            spawn.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            spawn.AddComponent<PlayerSpawnPoint>();

            var brasa = new GameObject("Ponto_de_Brasa");
            brasa.transform.SetParent(world, false);
            brasa.transform.position = new Vector3(-3f, 0f, -12f);
            BoxCollider collider = brasa.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.5f, 1.2f, 1.5f);
            collider.center = new Vector3(0f, 0.6f, 0f);
            brasa.AddComponent<NetworkObject>();
            BrasaPoint point = brasa.AddComponent<BrasaPoint>();
            var anchor = new GameObject("Respawn");
            anchor.transform.SetParent(brasa.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            Set(point, "respawnAnchor", anchor.transform);
            Block("Brasa_Visual", brasa.transform, new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.2f, 1.2f),
                SurfaceMaterial("Quest01_Brasa", "fosso_beam_hazard", 1f));
        }

        private static void BuildFx(Transform parent)
        {
            CombatFxLibrary combatLibrary = EnsureCombatFxLibrary();
            AudioCueLibrary audioLibrary = EnsureAudioCueLibrary();

            var combatFx = new GameObject("CombatFx");
            combatFx.transform.SetParent(parent, false);
            CombatFx fx = combatFx.AddComponent<CombatFx>();
            Set(fx, "library", combatLibrary);

            var audio = new GameObject("AudioCues");
            audio.transform.SetParent(parent, false);
            AudioCueBootstrap bootstrap = audio.AddComponent<AudioCueBootstrap>();
            Set(bootstrap, "library", audioLibrary);
        }

        private static CombatFxLibrary EnsureCombatFxLibrary()
        {
            string path = ParallaxPaths.Data + "/Combat/CombatFxLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<CombatFxLibrary>(path);
            if (library != null)
            {
                return library;
            }

            string matPath = ParallaxPaths.Materials + "/Combat_Tracer.mat";
            var tracerMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (tracerMat == null)
            {
                Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                tracerMat = new Material(unlit) { color = Color.white };
                AssetDatabase.CreateAsset(tracerMat, matPath);
            }

            library = ScriptableObject.CreateInstance<CombatFxLibrary>();
            library.EditorSetup(tracerMat, null, null, null);
            AssetDatabase.CreateAsset(library, path);
            return library;
        }

        private static AudioCueLibrary EnsureAudioCueLibrary()
        {
            string path = ParallaxPaths.Root + "/Audio/Cues/AudioCueLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<AudioCueLibrary>(path);
            if (library != null)
            {
                return library;
            }

            library = ScriptableObject.CreateInstance<AudioCueLibrary>();
            AssetDatabase.CreateAsset(library, path);
            return library;
        }

        private static void BuildNavMesh(Transform world)
        {
            var surfaceObject = new GameObject("NavMesh");
            surfaceObject.transform.SetParent(world, false);
            NavMeshSurface surface = surfaceObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }

        private static void BakeNavMeshToAsset()
        {
            NavMeshSurface surface = UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();
            if (surface == null)
            {
                return;
            }

            string navPath = ParallaxPaths.Root + "/Prefabs/Session/Quest01_NavMesh.asset";
            ParallaxPaths.EnsureFolder(ParallaxPaths.Root + "/Prefabs/Session");
            surface.BuildNavMesh();
            NavMeshData data = surface.navMeshData;
            if (data != null)
            {
                AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(data), navPath);
                surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            }

            EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
            EditorSceneManager.SaveScene(surface.gameObject.scene);
        }

        private static void BuildHeatShimmer(Transform parent, Vector3 position)
        {
            Shader shader = Shader.Find("Parallax/World/HeatShimmer");
            if (shader == null)
            {
                return;
            }

            string path = ParallaxPaths.Materials + "/Quest01_VaporSangria.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Vapor_Sangria";
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(parent, false);
            quad.transform.position = position;
            quad.transform.localScale = new Vector3(4f, 5f, 1f);
            Renderer quadRenderer = quad.GetComponent<Renderer>();
            quadRenderer.sharedMaterial = material;
            quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void BuildCameraRig()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.position = new Vector3(0f, 3f, -18f);
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
            Set(rig, "profile", AssetDatabase.LoadAssetAtPath<CameraProfile>(ParallaxPaths.Data + "/Camera/CameraProfile_Goliath.asset"));
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
        }

        private static void RegisterBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(PlayableLabBuilder.LabScenePath, true),
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(ParallaxPaths.Scenes + "/Labs/Lab_Olhares.unity", true)
            };
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

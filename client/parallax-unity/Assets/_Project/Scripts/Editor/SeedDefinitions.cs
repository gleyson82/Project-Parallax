using System;
using System.IO;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using Parallax.Gameplay.Cameras;
using UnityEditor;
using UnityEngine;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Cria os ScriptableObjects base a partir do cânone (classes, Auxiliares, corporações, perfis e missões).
    /// Idempotente: só preenche assets que ainda não existem, para não sobrescrever ajustes de design.
    /// </summary>
    public static class SeedDefinitions
    {
        [Serializable]
        private sealed class MissionSeedFile
        {
            public MissionSeed[] missions = new MissionSeed[0];
        }

        [Serializable]
        public sealed class MissionSeed
        {
            public string id;
            public string kind;
            public string corporation;
            public bool cifra;
            public bool fourProngMark;
            public string speaker;
            public string scene;
            public float sectorHeat;
            public string title;
            public string biome;
            public string briefing;
            public string readingGoliath;
            public string readingPhantom;
            public string readingCipher;
            public string reward;
        }

        [MenuItem("Parallax/Bootstrap/Seed Definitions")]
        public static void Run()
        {
            SeedClass(ParallaxClass.Goliath, "goliath", true, 2.6f, 420f);
            SeedClass(ParallaxClass.Phantom, "phantom", false, 5.6f, 78f);
            SeedClass(ParallaxClass.Cipher, "cipher", false, 4.4f, 72f);

            SeedAuxiliary(AuxiliaryId.Macaco, "macaco", 1, 6f);
            SeedAuxiliary(AuxiliaryId.Pipa, "pipa", 1, 3f);
            SeedAuxiliary(AuxiliaryId.CartuchoOverride, "cartucho-de-override", 1, 2f);

            SeedCorporation(CorporationId.ThermaTek, "therma-tek", new Color(1.0f, 0.42f, 0.21f));
            SeedCorporation(CorporationId.Kinesys, "kinesys", new Color(0.2f, 0.9f, 1.0f));
            SeedCorporation(CorporationId.OmniCripto, "omni-cripto", new Color(0.75f, 0.55f, 1.0f));

            SeedThermal("Thermal_Default", 290f, 38f, 6f);
            SeedThermal("Thermal_NucleoTermico", 680f, 38f, 4.5f);
            SeedThermal("Thermal_CanoThermaTek", 300f, 38f, 24f);
            ParallaxPaths.LoadOrCreate<SessionScalingProfile>(ParallaxPaths.Data + "/Session/SessionScaling.asset");

            SeedWeapon("Weapon_ThermaTek_Slag", "armamento-therma-tek", CorporationId.ThermaTek, DamageKind.Slag,
                22f, 0.16f, 55f, 1.1f, 0.11f, 0.38f, 0f, new Color(1f, 0.45f, 0.15f));
            SeedWeapon("Weapon_Kinesys_Cryo", "armamento-kinesys", CorporationId.Kinesys, DamageKind.Cryo,
                16f, 0.22f, 48f, 0.6f, 0.07f, 0.42f, 0.28f, new Color(0.45f, 0.9f, 1f));
            SeedWeapon("Weapon_OmniCripto_Pulse", "armamento-omni-cripto", CorporationId.OmniCripto, DamageKind.Pulse,
                18f, 0.2f, 60f, 0.4f, 0.08f, 0.4f, 0f, new Color(0.75f, 0.55f, 1f));

            SeedCamera("CameraProfile_Goliath", new Vector3(0.9f, 0.4f, 0f), 0.6f, 5.2f, 58f, 62f);
            SeedCamera("CameraProfile_Phantom", new Vector3(0.5f, 0.3f, 0f), 0.9f, 3.6f, 60f, 70f);
            SeedCamera("CameraProfile_Cipher", new Vector3(0.55f, 0.2f, 0f), 0.4f, 3.8f, 55f, 65f);

            foreach (MissionSeed seed in LoadMissionSeeds())
            {
                SeedMission(seed);
            }

            AssetDatabase.SaveAssets();
        }

        public static MissionSeed[] LoadMissionSeeds()
        {
            string fullPath = Path.GetFullPath(ParallaxPaths.MissionSeed);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("[Parallax/Seed] missions.pt-BR.json ausente.");
                return new MissionSeed[0];
            }

            return JsonUtility.FromJson<MissionSeedFile>(File.ReadAllText(fullPath)).missions;
        }

        private static void SeedClass(ParallaxClass parallaxClass, string note, bool heavyRig, float speed, float mass)
        {
            string path = $"{ParallaxPaths.Data}/Classes/Class_{parallaxClass}.asset";
            if (AssetDatabase.LoadAssetAtPath<ClassDefinition>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<ClassDefinition>(path);
            asset.EditorInitialize(note, "class." + note, note);
            asset.EditorSetup(parallaxClass, heavyRig, speed, mass);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedAuxiliary(AuxiliaryId auxiliary, string note, int uses, float duration)
        {
            string path = $"{ParallaxPaths.Data}/Auxiliaries/Auxiliary_{auxiliary}.asset";
            if (AssetDatabase.LoadAssetAtPath<AuxiliaryDefinition>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<AuxiliaryDefinition>(path);
            asset.EditorInitialize(note, "auxiliary." + note, note);
            asset.EditorSetup(auxiliary, uses, duration);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedCorporation(CorporationId corporation, string note, Color color)
        {
            string path = $"{ParallaxPaths.Data}/Corporations/Corporation_{corporation}.asset";
            if (AssetDatabase.LoadAssetAtPath<CorporationDefinition>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<CorporationDefinition>(path);
            asset.EditorInitialize(note, "corporation." + note, note);
            asset.EditorSetup(corporation, color);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedThermal(string name, float initial, float ambient, float cooling)
        {
            string path = $"{ParallaxPaths.Data}/Thermal/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<ThermalProfile>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<ThermalProfile>(path);
            asset.EditorSetup(initial, ambient, cooling);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedWeapon(string name, string note, CorporationId corporation, DamageKind kind,
            float damage, float interval, float range, float spread, float heat, float cooling, float chill, Color tracer)
        {
            string path = $"{ParallaxPaths.Data}/Weapons/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path) != null)
            {
                return;
            }

            ParallaxPaths.EnsureFolder(ParallaxPaths.Data + "/Weapons");
            var asset = ParallaxPaths.LoadOrCreate<WeaponDefinition>(path);
            asset.EditorInitialize(name.Replace("Weapon_", string.Empty).ToLowerInvariant(), "weapon." + note, note);
            asset.EditorSetup(corporation, kind, damage, interval, range, spread, heat, cooling, chill, tracer);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedCamera(string name, Vector3 shoulder, float arm, float distance, float tpFov, float fpFov)
        {
            string path = $"{ParallaxPaths.Data}/Camera/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<CameraProfile>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<CameraProfile>(path);
            asset.EditorSetup(shoulder, arm, distance, tpFov, fpFov);
            EditorUtility.SetDirty(asset);
        }

        private static void SeedMission(MissionSeed seed)
        {
            string path = $"{ParallaxPaths.Data}/Missions/Mission_{seed.id}.asset";
            if (AssetDatabase.LoadAssetAtPath<MissionDefinition>(path) != null)
            {
                return;
            }

            var asset = ParallaxPaths.LoadOrCreate<MissionDefinition>(path);
            asset.EditorInitialize(seed.id, seed.id + ".title", seed.id);
            asset.EditorSetup(
                Enum.TryParse(seed.kind, out MissionKind kind) ? kind : MissionKind.Quest,
                Enum.TryParse(seed.corporation, out CorporationId corporation) ? corporation : CorporationId.ThermaTek,
                seed.cifra,
                seed.fourProngMark,
                seed.speaker,
                seed.scene,
                seed.sectorHeat);
            EditorUtility.SetDirty(asset);
        }
    }
}

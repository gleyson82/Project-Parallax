using System.IO;
using UnityEditor;
using UnityEngine;

namespace Parallax.EditorTools
{
    /// <summary>Caminhos canônicos do projeto. Agentes e ferramentas usam só estes.</summary>
    public static class ParallaxPaths
    {
        public const string Root = "Assets/_Project";
        public const string Data = Root + "/Data";
        public const string Settings = Root + "/Settings";
        public const string Rendering = Settings + "/Rendering";
        public const string Localization = Settings + "/Localization";
        public const string Materials = Root + "/Art/Materials";
        public const string Scenes = Root + "/Scenes";
        public const string LoreGenerated = Data + "/Lore/Generated";
        public const string LoreFragments = Data + "/Lore/Fragments";
        public const string LocalizationSource = Data + "/Localization/Source";
        public const string MissionSeed = Data + "/Missions/Seed/missions.pt-BR.json";

        /// <summary>Raiz do monorepo (Project-Parallax), dois níveis acima do projeto Unity.</summary>
        public static string RepositoryRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));

        public static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }

        public static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }
    }
}

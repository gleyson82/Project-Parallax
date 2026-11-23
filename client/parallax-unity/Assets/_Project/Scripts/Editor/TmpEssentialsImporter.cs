using System.IO;
using UnityEditor;
using UnityEngine;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Importa os recursos essenciais do TextMeshPro (fontes SDF e shaders) sem diálogo, para a HUD.
    /// Linha de comando: -executeMethod Parallax.EditorTools.TmpEssentialsImporter.RunBatch
    /// </summary>
    public static class TmpEssentialsImporter
    {
        private const string PackageRelative = "Package Resources/TMP Essential Resources.unitypackage";

        public static bool IsImported => AssetDatabase.IsValidFolder("Assets/TextMesh Pro");

        [MenuItem("Parallax/Bootstrap/Import TMP Essentials")]
        public static void Import()
        {
            if (IsImported)
            {
                return;
            }

            UnityEditor.PackageManager.PackageInfo info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
            string path = info != null ? Path.Combine(info.resolvedPath, PackageRelative) : null;
            if (path == null || !File.Exists(path))
            {
                Debug.LogError("[Parallax/TMP] Pacote de recursos essenciais do TextMeshPro não encontrado.");
                return;
            }

            AssetDatabase.ImportPackage(path, false);
            AssetDatabase.Refresh();
        }

        public static void RunBatch()
        {
            Import();
            Debug.Log("[Parallax/TMP] Essentials importados: " + IsImported);
            EditorApplication.Exit(IsImported ? 0 : 1);
        }
    }
}

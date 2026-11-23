using UnityEditor;
using UnityEngine;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Convenções de importação automáticas para arte vinda do Blender, Material Maker, Krita e ComfyUI:
    /// sufixo define o tipo do mapa; malhas "_col" viram colisores; UI vira sprite.
    /// </summary>
    public sealed class ParallaxAssetPostprocessor : AssetPostprocessor
    {
        private const string ArtRoot = ParallaxPaths.Root + "/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            string lower = assetPath.ToLowerInvariant();

            if (lower.Contains("/art/ui/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                return;
            }

            if (lower.EndsWith("_normal.png"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else if (lower.EndsWith("_roughness.png") || lower.EndsWith("_metallicsmoothness.png") || lower.EndsWith("_mask.png") || lower.Contains("_lut"))
            {
                importer.sRGBTexture = false;
            }
            else if (lower.EndsWith("_albedo.png"))
            {
                importer.sRGBTexture = true;
            }

            if (lower.Contains("_lut") || lower.Contains("glitch_noise"))
            {
                importer.mipmapEnabled = false;
                importer.wrapMode = lower.Contains("_lut") ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                importer.filterMode = lower.Contains("_lut") ? FilterMode.Bilinear : FilterMode.Point;
            }
        }

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ArtRoot))
            {
                return;
            }

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            if (assetPath.Contains("/Characters/"))
            {
                importer.animationType = ModelImporterAnimationType.Human;
            }
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(ArtRoot))
            {
                return;
            }

            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.name.EndsWith("_col"))
                {
                    continue;
                }

                MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                Object.DestroyImmediate(filter.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(filter);
            }
        }
    }
}

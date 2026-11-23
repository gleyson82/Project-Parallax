using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Parallax.Core.Definitions;
using Parallax.Core.Roles;
using UnityEditor;
using UnityEditor.Localization;
using Debug = UnityEngine.Debug;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Ponte Project Daedalus -> Unity. O Python (studio/scripts/sync_daedalus.py) é o único que lê o vault
    /// e já bloqueia notas restritas; aqui só importamos o JSON gerado para LoreFragment e para a tabela "Lore".
    /// A Unity nunca escreve no vault.
    /// </summary>
    public static class DaedalusImporter
    {
        private const string FragmentsJson = ParallaxPaths.LoreGenerated + "/fragments.json";
        private const string TranslationsCsv = ParallaxPaths.LocalizationSource + "/daedalus_strings.csv";

        [Serializable]
        private sealed class FragmentFile
        {
            public string game;
            public int count;
            public FragmentEntry[] fragments = new FragmentEntry[0];
        }

        [Serializable]
        private sealed class FragmentEntry
        {
            public string id;
            public string title;
            public string olhar;
            public string status;
            public string camada;
            public string text;
            public string[] links = new string[0];
        }

        [MenuItem("Parallax/Daedalus/Sync Vault (Python + Import)")]
        public static void SyncAndImport()
        {
            if (RunPythonSync())
            {
                AssetDatabase.Refresh();
                Import();
            }
        }

        [MenuItem("Parallax/Daedalus/Import Generated JSON")]
        public static void Import()
        {
            string fullPath = Path.GetFullPath(FragmentsJson);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("[Parallax/Daedalus] fragments.json não encontrado. Rode Parallax/Daedalus/Sync Vault.");
                return;
            }

            FragmentFile file = UnityEngine.JsonUtility.FromJson<FragmentFile>(File.ReadAllText(fullPath));
            StringTableCollection lore = LocalizationEditorSettings.GetStringTableCollection(LocalizationSetup.LoreTable);
            int imported = 0;

            foreach (FragmentEntry entry in file.fragments)
            {
                if (string.IsNullOrEmpty(entry.id) || entry.camada == "restrito")
                {
                    continue;
                }

                string path = $"{ParallaxPaths.LoreFragments}/Fragment_{entry.id}.asset";
                var asset = ParallaxPaths.LoadOrCreate<LoreFragment>(path);
                asset.EditorSetup(entry.id, ParseViewer(entry.olhar), entry.status == "isca", entry.links);
                EditorUtility.SetDirty(asset);

                LocalizationSetup.SetEntry(lore, "pt-BR", asset.TitleKey, entry.title);
                LocalizationSetup.SetEntry(lore, "pt-BR", asset.TextKey, entry.text);
                imported++;
            }

            int translated = ImportTranslations(lore);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Parallax/Daedalus] {imported} fragmentos importados; {translated} traduções aplicadas.");
        }

        private static ParallaxClass ParseViewer(string olhar)
        {
            switch (olhar)
            {
                case "phantom": return ParallaxClass.Phantom;
                case "cipher": return ParallaxClass.Cipher;
                default: return ParallaxClass.Goliath;
            }
        }

        /// <summary>CSV: Key,pt-BR,en,zh-Hans,es. Colunas vazias ficam sem tradução (fallback para PT-BR).</summary>
        private static int ImportTranslations(StringTableCollection lore)
        {
            string fullPath = Path.GetFullPath(TranslationsCsv);
            if (!File.Exists(fullPath) || lore == null)
            {
                return 0;
            }

            List<string[]> rows = ParseCsv(File.ReadAllText(fullPath));
            if (rows.Count < 2)
            {
                return 0;
            }

            string[] header = rows[0];
            int applied = 0;
            for (int r = 1; r < rows.Count; r++)
            {
                string[] row = rows[r];
                for (int c = 2; c < header.Length && c < row.Length; c++)
                {
                    if (!string.IsNullOrWhiteSpace(row[c]))
                    {
                        LocalizationSetup.SetEntry(lore, header[c], row[0], row[c]);
                        applied++;
                    }
                }
            }

            return applied;
        }

        private static List<string[]> ParseCsv(string content)
        {
            var rows = new List<string[]>();
            var fields = new List<string>();
            var field = new System.Text.StringBuilder();
            bool quoted = false;
            for (int i = 0; i < content.Length; i++)
            {
                char ch = content[i];
                if (quoted)
                {
                    if (ch == '"' && i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else if (ch == '"')
                    {
                        quoted = false;
                    }
                    else
                    {
                        field.Append(ch);
                    }
                }
                else if (ch == '"')
                {
                    quoted = true;
                }
                else if (ch == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else if (ch == '\n')
                {
                    fields.Add(field.ToString().TrimEnd('\r'));
                    field.Clear();
                    rows.Add(fields.ToArray());
                    fields.Clear();
                }
                else
                {
                    field.Append(ch);
                }
            }

            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString());
                rows.Add(fields.ToArray());
            }

            return rows;
        }

        private static bool RunPythonSync()
        {
            string repo = ParallaxPaths.RepositoryRoot;
            string script = Path.Combine(repo, "studio", "scripts", "sync_daedalus.py");
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{script}\"",
                WorkingDirectory = repo,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        Debug.LogError($"[Parallax/Daedalus] sync_daedalus.py falhou ({process.ExitCode}):\n{error}");
                        return false;
                    }

                    Debug.Log("[Parallax/Daedalus] " + output.Trim());
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("[Parallax/Daedalus] Não foi possível executar o Python: " + exception.Message);
                return false;
            }
        }
    }
}

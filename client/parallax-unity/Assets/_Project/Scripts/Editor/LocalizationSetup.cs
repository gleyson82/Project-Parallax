using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Parallax.EditorTools
{
    /// <summary>
    /// Unity Localization com os 4 idiomas do lançamento. PT-BR é a língua-fonte; EN, ZH-CN e ES chegam
    /// pelo pipeline de tradução contextual (CSV em Data/Localization/Source) com revisão humana.
    /// </summary>
    public static class LocalizationSetup
    {
        public const string MissionsTable = "Missions";
        public const string LoreTable = "Lore";
        public const string UiTable = "UI";

        public static readonly string[] LocaleCodes = { "pt-BR", "en", "zh-Hans", "es" };

        [MenuItem("Parallax/Bootstrap/Localization (PT-BR, EN, ZH-CN, ES)")]
        public static void Run()
        {
            ParallaxPaths.EnsureFolder(ParallaxPaths.Localization + "/Locales");
            ParallaxPaths.EnsureFolder(ParallaxPaths.Localization + "/Tables");

            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                var settings = ParallaxPaths.LoadOrCreate<LocalizationSettings>(ParallaxPaths.Localization + "/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            var locales = new List<Locale>();
            foreach (string code in LocaleCodes)
            {
                locales.Add(EnsureLocale(code));
            }

            LocalizationSettings.ProjectLocale = locales[0];
            EnsureTable(MissionsTable, locales);
            EnsureTable(LoreTable, locales);
            EnsureTable(UiTable, locales);
            ImportMissionStrings();
            AssetDatabase.SaveAssets();
        }

        public static StringTableCollection EnsureTable(string name, IList<Locale> locales)
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(name);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(name, ParallaxPaths.Localization + "/Tables", locales);
            }

            return collection;
        }

        public static void SetEntry(StringTableCollection collection, string localeCode, string key, string value)
        {
            if (collection == null || string.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            if (!(collection.GetTable(new LocaleIdentifier(localeCode)) is StringTable table))
            {
                return;
            }

            StringTableEntry entry = table.GetEntry(key);
            if (entry == null)
            {
                table.AddEntry(key, value);
            }
            else
            {
                entry.Value = value;
            }

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(collection.SharedData);
        }

        private static Locale EnsureLocale(string code)
        {
            var identifier = new LocaleIdentifier(code);
            Locale existing = LocalizationEditorSettings.GetLocale(identifier);
            if (existing != null)
            {
                return existing;
            }

            string path = $"{ParallaxPaths.Localization}/Locales/{code}.asset";
            Locale locale = AssetDatabase.LoadAssetAtPath<Locale>(path);
            if (locale == null)
            {
                locale = Locale.CreateLocale(identifier);
                AssetDatabase.CreateAsset(locale, path);
            }

            LocalizationEditorSettings.AddLocale(locale, false);
            return locale;
        }

        private static void ImportMissionStrings()
        {
            StringTableCollection missions = LocalizationEditorSettings.GetStringTableCollection(MissionsTable);
            foreach (SeedDefinitions.MissionSeed seed in SeedDefinitions.LoadMissionSeeds())
            {
                SetEntry(missions, "pt-BR", seed.id + ".title", seed.title);
                SetEntry(missions, "pt-BR", seed.id + ".biome", seed.biome);
                SetEntry(missions, "pt-BR", seed.id + ".briefing", seed.briefing);
                SetEntry(missions, "pt-BR", seed.id + ".reading.goliath", seed.readingGoliath);
                SetEntry(missions, "pt-BR", seed.id + ".reading.phantom", seed.readingPhantom);
                SetEntry(missions, "pt-BR", seed.id + ".reading.cipher", seed.readingCipher);
                SetEntry(missions, "pt-BR", seed.id + ".reward", seed.reward);
            }
        }
    }
}

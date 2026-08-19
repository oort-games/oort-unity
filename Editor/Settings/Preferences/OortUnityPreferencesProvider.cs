#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    internal static class OortUnityPreferencesProvider
    {
        internal const string PreferencesPath = "Preferences/Oort Unity";

        private const string PreferencesTitle = "Oort Unity Preferences";

        private static readonly IOortUnityPreferencesSection[] Sections =
        {
            new GameViewScreenshotPreferencesSection(),
            new GameObjectIconGeneratorPreferencesSection(),
        };

        private static Vector2 _scrollPosition;

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                label = "Oort Unity",
                guiHandler = _ => DrawPreferences(),
                keywords = CollectKeywords(),
            };
        }

        private static void DrawPreferences()
        {
            OortUnityUserSettings userSettings = OortUnityUserSettings.instance;

            EditorGUILayout.HelpBox(
                "These settings are stored per user and per project in "
                + "UserSettings/OortUnityUserSettings.asset.",
                MessageType.Info
            );
            EditorGUILayout.Space(8f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (IOortUnityPreferencesSection section in Sections)
            {
                DrawSection(section, userSettings);
                EditorGUILayout.Space(10f);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8f);
            DrawResetAllButton(userSettings);
            EditorGUILayout.Space(8f);
        }

        private static void DrawSection(
            IOortUnityPreferencesSection section,
            OortUnityUserSettings userSettings
        )
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(section.Title, EditorStyles.boldLabel);
            EditorGUILayout.Space(3f);

            section.Draw(userSettings);

            EditorGUILayout.EndVertical();
        }

        private static HashSet<string> CollectKeywords()
        {
            var keywords = new HashSet<string>
            {
                "Oort Unity",
                "Preferences",
                "Settings",
            };

            foreach (IOortUnityPreferencesSection section in Sections)
            {
                foreach (string keyword in section.Keywords)
                {
                    keywords.Add(keyword);
                }
            }

            return keywords;
        }

        private static void DrawResetAllButton(OortUnityUserSettings userSettings)
        {
            if (!GUILayout.Button("Reset All Oort Unity Settings", GUILayout.Height(28f)))
            {
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                PreferencesTitle,
                "Reset all Oort Unity user settings to their defaults?",
                "Reset",
                "Cancel"
            );

            if (!confirmed)
            {
                return;
            }

            userSettings.ResetAllSettings();
            OortUnityUserSettings.NotifyPreferencesChanged();
            GUIUtility.ExitGUI();
        }
    }
}

#endif

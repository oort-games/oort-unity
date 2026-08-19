#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    internal sealed class GameViewScreenshotPreferencesSection
        : IOortUnityPreferencesSection
    {
        public string Title => "Game View Screenshot";

        public IEnumerable<string> Keywords => new[]
        {
            "Game View Screenshot",
            "Screenshot",
            "Watermark",
            "Directory",
        };

        public void Draw(OortUnityUserSettings userSettings)
        {
            GameViewScreenshotSettings settings = userSettings.GameViewScreenshot;
            settings.Validate();

            EditorGUI.BeginChangeCheck();

            settings.OutputDirectory = OortUnityPreferencesGUI.DrawDirectoryField(
                "Save Directory",
                "Select Screenshot Directory",
                settings.OutputDirectory,
                GameViewScreenshotSettings.DefaultDirectoryName
            );
            settings.WatermarkEnabled = EditorGUILayout.Toggle(
                "Enable Watermark",
                settings.WatermarkEnabled
            );

            using (new EditorGUI.DisabledScope(!settings.WatermarkEnabled))
            {
                settings.WatermarkTexture = (Texture2D)EditorGUILayout.ObjectField(
                    "Texture",
                    settings.WatermarkTexture,
                    typeof(Texture2D),
                    false
                );
                settings.WatermarkAnchor = (ScreenshotWatermarkAnchor)EditorGUILayout.EnumPopup(
                    "Position",
                    settings.WatermarkAnchor
                );
                settings.WatermarkSizeRatio = EditorGUILayout.Slider(
                    "Size (% Width)",
                    settings.WatermarkSizeRatio * 100f,
                    1f,
                    100f
                ) / 100f;
                settings.WatermarkOpacity = EditorGUILayout.Slider(
                    "Opacity (%)",
                    settings.WatermarkOpacity * 100f,
                    0f,
                    100f
                ) / 100f;
                settings.WatermarkMargin = EditorGUILayout.IntField(
                    "Margin (px)",
                    settings.WatermarkMargin
                );
            }

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings(userSettings);
            }

            EditorGUILayout.Space(5f);

            if (GUILayout.Button("Reset Game View Screenshot Settings"))
            {
                userSettings.ResetGameViewScreenshotSettings();
                OortUnityUserSettings.NotifyPreferencesChanged();
                GUIUtility.ExitGUI();
            }
        }

        private static void SaveSettings(OortUnityUserSettings userSettings)
        {
            userSettings.SaveGameViewScreenshotSettings();
            OortUnityUserSettings.NotifyPreferencesChanged();
        }
    }
}

#endif

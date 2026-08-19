#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    internal sealed class GameObjectIconGeneratorPreferencesSection
        : IOortUnityPreferencesSection
    {
        public string Title => "GameObject Icon Generator";

        public IEnumerable<string> Keywords => new[]
        {
            "GameObject Icon Generator",
            "Icon",
            "Render",
            "Camera",
            "Lighting",
            "Directory",
        };

        public void Draw(OortUnityUserSettings userSettings)
        {
            GameObjectIconGeneratorSettings settings = userSettings.GameObjectIconGenerator;
            settings.Validate();

            EditorGUI.BeginChangeCheck();

            DrawOutputSettings(settings);
            EditorGUILayout.Space(7f);
            DrawRenderSettings(settings.RenderSettings);
            EditorGUILayout.Space(7f);
            DrawLightingSettings(settings.RenderSettings);

            if (EditorGUI.EndChangeCheck())
            {
                SaveSettings(userSettings);
            }

            EditorGUILayout.Space(5f);

            if (GUILayout.Button("Reset GameObject Icon Generator Settings"))
            {
                userSettings.ResetGameObjectIconGeneratorSettings();
                OortUnityUserSettings.NotifyPreferencesChanged();
                GUIUtility.ExitGUI();
            }
        }

        private static void DrawOutputSettings(GameObjectIconGeneratorSettings settings)
        {
            settings.OutputDirectory = OortUnityPreferencesGUI.DrawDirectoryField(
                "Save Directory",
                "Select Icon Directory",
                settings.OutputDirectory,
                GameObjectIconGeneratorSettings.DefaultDirectoryName
            );
            settings.FileName = EditorGUILayout.TextField("File Name", settings.FileName);
        }

        private static void DrawRenderSettings(IconRenderSettings renderSettings)
        {
            EditorGUILayout.LabelField("Render Settings", EditorStyles.boldLabel);

            renderSettings.Resolution = EditorGUILayout.IntSlider(
                "Resolution",
                renderSettings.Resolution,
                16,
                4096
            );
            renderSettings.BackgroundMode = (IconBackgroundMode)EditorGUILayout.EnumPopup(
                "Background",
                renderSettings.BackgroundMode
            );

            using (
                new EditorGUI.DisabledScope(
                    renderSettings.BackgroundMode != IconBackgroundMode.SolidColor
                )
            )
            {
                renderSettings.BackgroundColor = OortUnityPreferencesGUI.DrawOpaqueColorField(
                    "Background Color",
                    renderSettings.BackgroundColor
                );
            }

            renderSettings.Padding = EditorGUILayout.Slider(
                "Padding (%)",
                renderSettings.Padding * 100f,
                0f,
                IconRenderSettings.MaximumPadding * 100f
            ) / 100f;

            IconViewPreset previousPreset = renderSettings.ViewPreset;
            renderSettings.ViewPreset = (IconViewPreset)EditorGUILayout.EnumPopup(
                "View",
                renderSettings.ViewPreset
            );

            if (
                renderSettings.ViewPreset != previousPreset
                && renderSettings.ViewPreset != IconViewPreset.Custom
            )
            {
                renderSettings.Rotation = IconRenderSettings.GetPresetRotation(
                    renderSettings.ViewPreset
                );
            }

            Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", renderSettings.Rotation);

            if (rotation != renderSettings.Rotation)
            {
                renderSettings.Rotation = rotation;
                renderSettings.ViewPreset = IconViewPreset.Custom;
            }

            renderSettings.Projection = (IconProjection)EditorGUILayout.EnumPopup(
                "Projection",
                renderSettings.Projection
            );
        }

        private static void DrawLightingSettings(IconRenderSettings renderSettings)
        {
            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);

            renderSettings.LightingSource = (IconLightingSource)EditorGUILayout.EnumPopup(
                "Source",
                renderSettings.LightingSource
            );

            using (
                new EditorGUI.DisabledScope(
                    renderSettings.LightingSource != IconLightingSource.Studio
                )
            )
            {
                renderSettings.MainLightRotation = EditorGUILayout.Vector3Field(
                    "Main Rotation",
                    renderSettings.MainLightRotation
                );
                renderSettings.MainLightColor = OortUnityPreferencesGUI.DrawOpaqueColorField(
                    "Main Color",
                    renderSettings.MainLightColor
                );
                renderSettings.MainLightIntensity = EditorGUILayout.Slider(
                    "Main Intensity",
                    renderSettings.MainLightIntensity,
                    0f,
                    IconRenderSettings.MaximumLightIntensity
                );
                renderSettings.FillLightRotation = EditorGUILayout.Vector3Field(
                    "Fill Rotation",
                    renderSettings.FillLightRotation
                );
                renderSettings.FillLightColor = OortUnityPreferencesGUI.DrawOpaqueColorField(
                    "Fill Color",
                    renderSettings.FillLightColor
                );
                renderSettings.FillLightIntensity = EditorGUILayout.Slider(
                    "Fill Intensity",
                    renderSettings.FillLightIntensity,
                    0f,
                    IconRenderSettings.MaximumLightIntensity
                );
            }
        }

        private static void SaveSettings(OortUnityUserSettings userSettings)
        {
            userSettings.SaveGameObjectIconGeneratorSettings();
            OortUnityUserSettings.NotifyPreferencesChanged();
        }
    }
}

#endif

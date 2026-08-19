#if UNITY_EDITOR

using System;
using OortUnity.Utilities;
using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    internal static class OortUnityPreferencesGUI
    {
        private const string PreferencesTitle = "Oort Unity Preferences";

        public static string DrawDirectoryField(
            string label,
            string panelTitle,
            string savedDirectory,
            string defaultDirectoryName
        )
        {
            string defaultDirectory = EditorDirectoryUtility.GetDefaultOutputDirectory(
                defaultDirectoryName
            );
            string directory = string.IsNullOrWhiteSpace(savedDirectory)
                ? defaultDirectory
                : savedDirectory;

            EditorGUILayout.BeginHorizontal();

            string enteredDirectory = EditorGUILayout.TextField(label, directory);

            if (!string.Equals(enteredDirectory, directory, StringComparison.Ordinal))
            {
                savedDirectory = PathUtility.NormalizePath(enteredDirectory);
                directory = savedDirectory;
            }

            if (
                GUILayout.Button("Browse", GUILayout.Width(64f))
                && EditorDirectoryUtility.TryBrowseDirectory(
                    panelTitle,
                    directory,
                    out string selectedDirectory
                )
            )
            {
                savedDirectory = PathUtility.NormalizePath(selectedDirectory);
                directory = savedDirectory;
                GUI.changed = true;
            }

            if (GUILayout.Button("Open", GUILayout.Width(52f)))
            {
                EditorDirectoryUtility.OpenDirectory(PreferencesTitle, directory);
            }

            if (GUILayout.Button("Reset", GUILayout.Width(52f)))
            {
                savedDirectory = string.Empty;
                GUI.changed = true;
            }

            EditorGUILayout.EndHorizontal();

            return savedDirectory;
        }

        public static Color DrawOpaqueColorField(string label, Color color)
        {
            Color selectedColor = EditorGUILayout.ColorField(label, color);
            selectedColor.a = 1f;

            return selectedColor;
        }
    }
}

#endif

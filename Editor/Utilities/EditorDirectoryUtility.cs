#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.IO;
using OortUnity.Utilities;
using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    public static class EditorDirectoryUtility
    {
        public static string GetDefaultOutputDirectory(string directoryName)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            return PathUtility.NormalizePath(
                Path.Combine(documentsPath, Application.productName, directoryName)
            );
        }

        public static bool TryBrowseDirectory(
            string panelTitle,
            string currentDirectory,
            out string selectedDirectory
        )
        {
            selectedDirectory = EditorUtility.OpenFolderPanel(
                panelTitle,
                currentDirectory,
                string.Empty
            );

            return !string.IsNullOrEmpty(selectedDirectory);
        }

        public static void OpenDirectory(string windowTitle, string directoryPath)
        {
            OpenDirectory(windowTitle, directoryPath, false);
        }

        public static void OpenProjectDirectory(string windowTitle, string directoryPath)
        {
            OpenDirectory(windowTitle, directoryPath, true);
        }

        private static void OpenDirectory(
            string windowTitle,
            string directoryPath,
            bool relativeToProject
        )
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                EditorUtility.DisplayDialog(windowTitle, "Enter a directory.", "OK");
                return;
            }

            try
            {
                string resolvedPath = relativeToProject
                    ? GetProjectAbsolutePath(directoryPath)
                    : directoryPath;
                FileUtility.CreateDirectory(resolvedPath);
                Process.Start(new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    UseShellExecute = true,
                });
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    windowTitle,
                    $"Failed to open the directory.\n\n{exception.Message}",
                    "OK"
                );
            }
        }

        private static string GetProjectAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;

            if (string.IsNullOrEmpty(projectPath))
            {
                throw new InvalidOperationException("Failed to resolve the Unity project directory.");
            }

            return Path.GetFullPath(Path.Combine(projectPath, path));
        }
    }
}

#endif

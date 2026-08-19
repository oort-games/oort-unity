#if UNITY_EDITOR

using UnityEditor;

namespace OortUnity.Editor
{
    internal static class OortUnityPreferencesWindow
    {
        [MenuItem("Oort/Preferences", false, 1)]
        public static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences(OortUnityPreferencesProvider.PreferencesPath);
        }
    }
}

#endif

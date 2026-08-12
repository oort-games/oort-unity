#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR_WIN
using Microsoft.Win32;
using UnityEditor;
#endif

namespace OortUnity.Editor
{
    internal static class PlayerPrefsStorage
    {
        #region Read

        public static List<PlayerPrefsEntry> LoadAll()
        {
#if UNITY_EDITOR_WIN
            return LoadAllWindows();
#else
            Debug.LogWarning(
                "[PlayerPrefs Manager] PlayerPrefs enumeration is not supported on this platform yet.");

            return new List<PlayerPrefsEntry>();
#endif
        }

#if UNITY_EDITOR_WIN

        private static List<PlayerPrefsEntry> LoadAllWindows()
        {
            var entries = new List<PlayerPrefsEntry>();

            string registryPath =
                $@"Software\Unity\UnityEditor\{PlayerSettings.companyName}\{PlayerSettings.productName}";

            using RegistryKey registryKey =
                Registry.CurrentUser.OpenSubKey(registryPath);

            if (registryKey == null)
            {
                return entries;
            }

            foreach (string valueName in registryKey.GetValueNames())
            {
                string key = GetPlayerPrefsKey(valueName);

                if (TryCreateEntry(key, out PlayerPrefsEntry entry))
                {
                    entries.Add(entry);
                }
            }

            entries.Sort((a, b) =>
                string.Compare(
                    a.Key,
                    b.Key,
                    StringComparison.OrdinalIgnoreCase));

            return entries;
        }

        private static bool TryCreateEntry(
            string key,
            out PlayerPrefsEntry entry)
        {
            entry = null;

            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            if (TryGetString(key, out string stringValue))
            {
                entry = new PlayerPrefsEntry(
                    key,
                    PlayerPrefsValueType.String,
                    stringValue);

                return true;
            }

            if (TryGetInt(key, out int intValue))
            {
                entry = new PlayerPrefsEntry(
                    key,
                    PlayerPrefsValueType.Int,
                    intValue.ToString(CultureInfo.InvariantCulture));

                return true;
            }

            if (TryGetFloat(key, out float floatValue))
            {
                entry = new PlayerPrefsEntry(
                    key,
                    PlayerPrefsValueType.Float,
                    floatValue.ToString(CultureInfo.InvariantCulture));

                return true;
            }

            return false;
        }

        private static bool TryGetString(
            string key,
            out string value)
        {
            const string defaultA =
                "__OORTUNITY_PLAYERPREFS_STRING_DEFAULT_A__";

            const string defaultB =
                "__OORTUNITY_PLAYERPREFS_STRING_DEFAULT_B__";

            string valueA = PlayerPrefs.GetString(key, defaultA);
            string valueB = PlayerPrefs.GetString(key, defaultB);

            if (valueA != valueB)
            {
                value = null;
                return false;
            }

            value = valueA;
            return true;
        }

        private static bool TryGetInt(
            string key,
            out int value)
        {
            int valueA = PlayerPrefs.GetInt(key, int.MinValue);
            int valueB = PlayerPrefs.GetInt(key, int.MaxValue);

            if (valueA != valueB)
            {
                value = default;
                return false;
            }

            value = valueA;
            return true;
        }

        private static bool TryGetFloat(
            string key,
            out float value)
        {
            float valueA = PlayerPrefs.GetFloat(key, float.MinValue);
            float valueB = PlayerPrefs.GetFloat(key, float.MaxValue);

            if (!valueA.Equals(valueB))
            {
                value = default;
                return false;
            }

            value = valueA;
            return true;
        }

        private static string GetPlayerPrefsKey(
            string registryValueName)
        {
            return Regex.Replace(
                registryValueName,
                @"_h\d+$",
                string.Empty);
        }

#endif

        #endregion

        #region Write

        public static bool TrySetValue(
            PlayerPrefsEntry entry,
            string value,
            out string error)
        {
            error = null;

            if (entry == null)
            {
                error = "PlayerPrefs entry is null.";
                return false;
            }

            switch (entry.Type)
            {
                case PlayerPrefsValueType.Int:
                    {
                        if (!int.TryParse(
                            value,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out int intValue))
                        {
                            error = $"'{value}' is not a valid integer.";
                            return false;
                        }

                        PlayerPrefs.SetInt(entry.Key, intValue);
                        break;
                    }

                case PlayerPrefsValueType.Float:
                    {
                        if (!float.TryParse(
                            value,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float floatValue))
                        {
                            error = $"'{value}' is not a valid float.";
                            return false;
                        }

                        PlayerPrefs.SetFloat(entry.Key, floatValue);
                        break;
                    }

                case PlayerPrefsValueType.String:
                    {
                        PlayerPrefs.SetString(
                            entry.Key,
                            value ?? string.Empty);

                        break;
                    }

                default:
                    {
                        error =
                            $"Unsupported PlayerPrefs type: {entry.Type}";

                        return false;
                    }
            }

            PlayerPrefs.Save();
            return true;
        }

        #endregion

        #region Delete

        public static void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        #endregion
    }
}

#endif
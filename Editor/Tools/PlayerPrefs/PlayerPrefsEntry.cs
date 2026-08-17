#if UNITY_EDITOR

namespace OortUnity.Editor
{
    internal enum PlayerPrefsValueType
    {
        Int,
        Float,
        String,
    }

    internal sealed class PlayerPrefsEntry
    {
        public string Key { get; }
        public PlayerPrefsValueType Type { get; }
        public string Value { get; set; }

        public PlayerPrefsEntry(string key, PlayerPrefsValueType type, string value)
        {
            Key = key;
            Type = type;
            Value = value;
        }
    }
}

#endif

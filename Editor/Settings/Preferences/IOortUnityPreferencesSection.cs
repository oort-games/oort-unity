#if UNITY_EDITOR

using System.Collections.Generic;

namespace OortUnity.Editor
{
    internal interface IOortUnityPreferencesSection
    {
        string Title { get; }
        IEnumerable<string> Keywords { get; }
        void Draw(OortUnityUserSettings userSettings);
    }
}

#endif

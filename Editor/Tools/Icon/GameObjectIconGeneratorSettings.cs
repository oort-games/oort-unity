#if UNITY_EDITOR

using System;

namespace OortUnity.Editor
{
    [Serializable]
    internal sealed class GameObjectIconGeneratorSettings
    {
        public const string DefaultFileName = "GameObjectIcon";

        public string OutputDirectory;
        public string FileName = DefaultFileName;
        public IconRenderSettings RenderSettings = new IconRenderSettings();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                FileName = DefaultFileName;
            }

            RenderSettings ??= new IconRenderSettings();
            RenderSettings.Validate();
        }
    }
}

#endif

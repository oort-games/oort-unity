#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    [FilePath("UserSettings/OortUnityUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class OortUnityUserSettings : ScriptableSingleton<OortUnityUserSettings>
    {
        [SerializeField]
        private GameViewScreenshotSettings _gameViewScreenshot = new GameViewScreenshotSettings();

        [SerializeField]
        private GameObjectIconGeneratorSettings _gameObjectIconGenerator = new GameObjectIconGeneratorSettings();

        public GameViewScreenshotSettings GameViewScreenshot
        {
            get
            {
                _gameViewScreenshot ??= new GameViewScreenshotSettings();

                return _gameViewScreenshot;
            }
        }

        public GameObjectIconGeneratorSettings GameObjectIconGenerator
        {
            get
            {
                _gameObjectIconGenerator ??= new GameObjectIconGeneratorSettings();

                return _gameObjectIconGenerator;
            }
        }

        public void SaveGameViewScreenshotSettings()
        {
            GameViewScreenshot.Validate();
            Save(true);
        }

        public void SaveGameObjectIconGeneratorSettings()
        {
            GameObjectIconGenerator.Validate();
            Save(true);
        }
    }
}

#endif

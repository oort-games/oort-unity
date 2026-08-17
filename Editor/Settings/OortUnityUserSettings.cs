#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    [FilePath("UserSettings/OortUnityUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class OortUnityUserSettings : ScriptableSingleton<OortUnityUserSettings>
    {
        #region Fields

        [SerializeField]
        private string _gameViewScreenshotOutputDirectory;

        #endregion

        #region Properties

        public string GameViewScreenshotOutputDirectory
        {
            get => _gameViewScreenshotOutputDirectory;
            set
            {
                if (_gameViewScreenshotOutputDirectory == value)
                    return;

                _gameViewScreenshotOutputDirectory = value;
                Save(true);
            }
        }

        #endregion
    }
}

#endif

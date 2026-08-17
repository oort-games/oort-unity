#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace OortUnity.Editor
{
    internal enum ScreenshotWatermarkAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    [FilePath("UserSettings/OortUnityUserSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class OortUnityUserSettings : ScriptableSingleton<OortUnityUserSettings>
    {
        #region Constants

        public const float DefaultGameViewScreenshotWatermarkSizeRatio = 0.15f;
        public const float DefaultGameViewScreenshotWatermarkOpacity = 0.7f;
        public const int DefaultGameViewScreenshotWatermarkMargin = 24;
        public const ScreenshotWatermarkAnchor DefaultGameViewScreenshotWatermarkAnchor = ScreenshotWatermarkAnchor.BottomRight;

        #endregion

        #region Fields

        [SerializeField]
        private string _gameViewScreenshotOutputDirectory;

        [SerializeField]
        private bool _gameViewScreenshotWatermarkEnabled;

        [SerializeField]
        private Texture2D _gameViewScreenshotWatermarkTexture;

        [SerializeField]
        private ScreenshotWatermarkAnchor _gameViewScreenshotWatermarkAnchor = DefaultGameViewScreenshotWatermarkAnchor;

        [SerializeField]
        private float _gameViewScreenshotWatermarkSizeRatio = DefaultGameViewScreenshotWatermarkSizeRatio;

        [SerializeField]
        private float _gameViewScreenshotWatermarkOpacity = DefaultGameViewScreenshotWatermarkOpacity;

        [SerializeField]
        private int _gameViewScreenshotWatermarkMargin = DefaultGameViewScreenshotWatermarkMargin;

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

        public bool GameViewScreenshotWatermarkEnabled
        {
            get => _gameViewScreenshotWatermarkEnabled;
            set
            {
                if (_gameViewScreenshotWatermarkEnabled == value)
                    return;

                _gameViewScreenshotWatermarkEnabled = value;
                Save(true);
            }
        }

        public Texture2D GameViewScreenshotWatermarkTexture
        {
            get => _gameViewScreenshotWatermarkTexture;
            set
            {
                if (_gameViewScreenshotWatermarkTexture == value)
                    return;

                _gameViewScreenshotWatermarkTexture = value;
                Save(true);
            }
        }

        public ScreenshotWatermarkAnchor GameViewScreenshotWatermarkAnchor
        {
            get => _gameViewScreenshotWatermarkAnchor;
            set
            {
                if (_gameViewScreenshotWatermarkAnchor == value)
                    return;

                _gameViewScreenshotWatermarkAnchor = value;
                Save(true);
            }
        }

        public float GameViewScreenshotWatermarkSizeRatio
        {
            get => _gameViewScreenshotWatermarkSizeRatio;
            set
            {
                value = Mathf.Clamp(value, 0.01f, 1f);

                if (Mathf.Approximately(_gameViewScreenshotWatermarkSizeRatio, value))
                    return;

                _gameViewScreenshotWatermarkSizeRatio = value;
                Save(true);
            }
        }

        public float GameViewScreenshotWatermarkOpacity
        {
            get => _gameViewScreenshotWatermarkOpacity;
            set
            {
                value = Mathf.Clamp01(value);

                if (Mathf.Approximately(_gameViewScreenshotWatermarkOpacity, value))
                    return;

                _gameViewScreenshotWatermarkOpacity = value;
                Save(true);
            }
        }

        public int GameViewScreenshotWatermarkMargin
        {
            get => _gameViewScreenshotWatermarkMargin;
            set
            {
                value = Mathf.Max(0, value);

                if (_gameViewScreenshotWatermarkMargin == value)
                    return;

                _gameViewScreenshotWatermarkMargin = value;
                Save(true);
            }
        }

        #endregion

        #region Reset

        public void ResetGameViewScreenshotWatermark()
        {
            _gameViewScreenshotWatermarkEnabled = false;
            _gameViewScreenshotWatermarkTexture = null;
            _gameViewScreenshotWatermarkAnchor = DefaultGameViewScreenshotWatermarkAnchor;
            _gameViewScreenshotWatermarkSizeRatio = DefaultGameViewScreenshotWatermarkSizeRatio;
            _gameViewScreenshotWatermarkOpacity = DefaultGameViewScreenshotWatermarkOpacity;
            _gameViewScreenshotWatermarkMargin = DefaultGameViewScreenshotWatermarkMargin;

            Save(true);
        }

        #endregion
    }
}

#endif

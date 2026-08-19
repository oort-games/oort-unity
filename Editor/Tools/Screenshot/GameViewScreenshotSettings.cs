#if UNITY_EDITOR

using System;
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

    [Serializable]
    internal sealed class GameViewScreenshotSettings
    {
        public const string DefaultDirectoryName = "Screenshots";
        public const float DefaultWatermarkSizeRatio = 0.15f;
        public const float DefaultWatermarkOpacity = 0.7f;
        public const int DefaultWatermarkMargin = 24;
        public const ScreenshotWatermarkAnchor DefaultWatermarkAnchor = ScreenshotWatermarkAnchor.BottomRight;

        public string OutputDirectory;
        public bool WatermarkEnabled;
        public Texture2D WatermarkTexture;
        public ScreenshotWatermarkAnchor WatermarkAnchor = DefaultWatermarkAnchor;
        public float WatermarkSizeRatio = DefaultWatermarkSizeRatio;
        public float WatermarkOpacity = DefaultWatermarkOpacity;
        public int WatermarkMargin = DefaultWatermarkMargin;

        public void Validate()
        {
            WatermarkSizeRatio = Mathf.Clamp(WatermarkSizeRatio, 0.01f, 1f);
            WatermarkOpacity = Mathf.Clamp01(WatermarkOpacity);
            WatermarkMargin = Mathf.Max(0, WatermarkMargin);
        }

        public void ResetWatermark()
        {
            WatermarkEnabled = false;
            WatermarkTexture = null;
            WatermarkAnchor = DefaultWatermarkAnchor;
            WatermarkSizeRatio = DefaultWatermarkSizeRatio;
            WatermarkOpacity = DefaultWatermarkOpacity;
            WatermarkMargin = DefaultWatermarkMargin;
        }

        public void Reset()
        {
            OutputDirectory = string.Empty;
            ResetWatermark();
        }
    }
}

#endif

using NUnit.Framework;
using OortUnity.Editor;
using UnityEngine;

namespace OortUnity.Tests
{
    public class GameViewScreenshotWindowTests
    {
        [Test]
        public void ScreenshotSettings_ValidateAndResetWatermarkValues()
        {
            var settings = new GameViewScreenshotSettings
            {
                WatermarkEnabled = true,
                WatermarkAnchor = ScreenshotWatermarkAnchor.TopLeft,
                WatermarkSizeRatio = 2f,
                WatermarkOpacity = -1f,
                WatermarkMargin = -10,
            };

            settings.Validate();

            Assert.AreEqual(1f, settings.WatermarkSizeRatio);
            Assert.AreEqual(0f, settings.WatermarkOpacity);
            Assert.AreEqual(0, settings.WatermarkMargin);

            settings.ResetWatermark();

            Assert.IsFalse(settings.WatermarkEnabled);
            Assert.IsNull(settings.WatermarkTexture);
            Assert.AreEqual(GameViewScreenshotSettings.DefaultWatermarkAnchor, settings.WatermarkAnchor);
            Assert.AreEqual(GameViewScreenshotSettings.DefaultWatermarkSizeRatio, settings.WatermarkSizeRatio);
            Assert.AreEqual(GameViewScreenshotSettings.DefaultWatermarkOpacity, settings.WatermarkOpacity);
            Assert.AreEqual(GameViewScreenshotSettings.DefaultWatermarkMargin, settings.WatermarkMargin);
        }

        [Test]
        public void BottomRightDefaults_ReturnExpectedPlacement_ForFullHdCapture()
        {
            var watermark = new Texture2D(100, 50, TextureFormat.RGBA32, false);

            try
            {
                Vector2Int size = GameViewScreenshotWindow.CalculateWatermarkSize(
                    watermark,
                    1920,
                    1080,
                    0.15f,
                    24
                );
                Vector2Int position = GameViewScreenshotWindow.CalculateWatermarkPosition(
                    ScreenshotWatermarkAnchor.BottomRight,
                    1920,
                    1080,
                    size.x,
                    size.y,
                    24
                );

                Assert.AreEqual(new Vector2Int(288, 144), size);
                Assert.AreEqual(new Vector2Int(1608, 24), position);
            }
            finally
            {
                Object.DestroyImmediate(watermark);
            }
        }
    }
}

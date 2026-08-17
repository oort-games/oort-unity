using NUnit.Framework;
using OortUnity.Editor;
using UnityEngine;

namespace OortUnity.Tests
{
    public class GameViewScreenshotWindowTests
    {
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

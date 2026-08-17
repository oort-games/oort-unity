using System.Collections.Generic;
using NUnit.Framework;
using OortUnity.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace OortUnity.Tests
{
    public class TextureUtilityTests
    {
        private readonly List<Texture2D> _textures = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Texture2D texture in _textures)
            {
                Object.DestroyImmediate(texture);
            }

            _textures.Clear();
        }

        [Test]
        public void Blend_AppliesSourceAlphaAndOpacity()
        {
            Texture2D destination = CreateTexture(1, 1, new Color32(0, 0, 0, 255));
            Texture2D source = CreateTexture(1, 1, new Color32(255, 0, 0, 128));

            TextureUtility.Blend(destination, source, 0, 0, 0.5f);

            Color32 result = destination.GetPixel(0, 0);

            Assert.That(result.r, Is.EqualTo(64).Within(1));
            Assert.AreEqual(0, result.g);
            Assert.AreEqual(0, result.b);
            Assert.AreEqual(255, result.a);
        }

        [Test]
        public void Blend_AppliesTint()
        {
            Texture2D destination = CreateTexture(1, 1, new Color32(0, 0, 0, 255));
            Texture2D source = CreateTexture(1, 1, new Color32(255, 255, 255, 255));

            TextureUtility.Blend(destination, source, 0, 0, Color.green);

            Assert.AreEqual(new Color32(0, 255, 0, 255), (Color32)destination.GetPixel(0, 0));
        }

        [Test]
        public void Blend_PreservesStraightAlpha_OnTransparentDestination()
        {
            Texture2D destination = CreateTexture(1, 1, new Color32(0, 0, 0, 0));
            Texture2D source = CreateTexture(1, 1, new Color32(255, 0, 0, 128));

            TextureUtility.Blend(destination, source, 0, 0);

            Color32 result = destination.GetPixel(0, 0);

            Assert.AreEqual(255, result.r);
            Assert.AreEqual(0, result.g);
            Assert.AreEqual(0, result.b);
            Assert.AreEqual(128, result.a);
        }

        [Test]
        public void Blend_ClipsSourceOutsideDestination()
        {
            Texture2D destination = CreateTexture(2, 2, new Color32(0, 0, 0, 255));
            Texture2D source = CreateTexture(2, 2, new Color32(255, 255, 255, 255));

            TextureUtility.Blend(destination, source, 1, 1);

            Color32[] result = destination.GetPixels32();

            Assert.AreEqual(new Color32(0, 0, 0, 255), result[0]);
            Assert.AreEqual(new Color32(0, 0, 0, 255), result[1]);
            Assert.AreEqual(new Color32(0, 0, 0, 255), result[2]);
            Assert.AreEqual(new Color32(255, 255, 255, 255), result[3]);
        }

        [Test]
        public void CreateReadableCopy_ReturnsReadableTexture_ForNonReadableSource()
        {
            RequireGraphicsDevice();

            Texture2D source = CreateTexture(1, 1, new Color32(255, 0, 0, 255));
            source.Apply(false, true);

            Texture2D result = TextureUtility.CreateReadableCopy(source);
            _textures.Add(result);

            Assert.IsTrue(result.isReadable);
            Assert.That(result.GetPixel(0, 0).r, Is.EqualTo(1f).Within(0.02f));
        }

        [Test]
        public void CreateReadableCopy_PreservesMidToneColor_ForNonReadableSource()
        {
            RequireGraphicsDevice();

            var expected = new Color32(64, 128, 192, 255);
            Texture2D source = CreateTexture(1, 1, expected);
            source.Apply(false, true);

            Texture2D result = TextureUtility.CreateReadableCopy(source);
            _textures.Add(result);

            AssertColorApproximately(expected, result.GetPixels32()[0], 2);
        }

        [Test]
        public void Resize_ReturnsRequestedDimensions()
        {
            RequireGraphicsDevice();

            Texture2D source = CreateTexture(2, 2, new Color32(255, 255, 255, 255));

            Texture2D result = TextureUtility.Resize(source, 4, 3);
            _textures.Add(result);

            Assert.AreEqual(4, result.width);
            Assert.AreEqual(3, result.height);
            Assert.IsTrue(result.isReadable);
        }

        [Test]
        public void Resize_PreservesSolidMidToneColor()
        {
            RequireGraphicsDevice();

            var expected = new Color32(64, 128, 192, 255);
            Texture2D source = CreateTexture(2, 2, expected);

            Texture2D result = TextureUtility.Resize(source, 4, 3);
            _textures.Add(result);

            foreach (Color32 pixel in result.GetPixels32())
            {
                AssertColorApproximately(expected, pixel, 2);
            }
        }

        private Texture2D CreateTexture(int width, int height, Color32 color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];

            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = color;
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            _textures.Add(texture);

            return texture;
        }

        private static void RequireGraphicsDevice()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore("A graphics device is required for RenderTexture tests.");
            }
        }

        private static void AssertColorApproximately(Color32 expected, Color32 actual, int tolerance)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(tolerance));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(tolerance));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(tolerance));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(tolerance));
        }
    }
}

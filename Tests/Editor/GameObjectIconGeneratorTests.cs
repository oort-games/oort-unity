using NUnit.Framework;
using OortUnity.Editor;
using OortUnity.Utilities;
using UnityEngine;

namespace OortUnity.Tests
{
    public class GameObjectIconGeneratorTests
    {
        [Test]
        public void IsUIObject_ReturnsTrue_ForRectTransformWithCanvasRenderer()
        {
            var source = new GameObject("UI Source", typeof(RectTransform), typeof(CanvasRenderer));

            try
            {
                Assert.IsTrue(GameObjectBoundsUtility.IsUIObject(source));
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void IsUIObject_ReturnsFalse_For3DRenderer()
        {
            GameObject source = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                Assert.IsFalse(GameObjectBoundsUtility.IsUIObject(source));
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void Is2DObject_ReturnsTrue_ForSpriteRendererHierarchy()
        {
            var root = new GameObject("2D Source");
            var child = new GameObject("Sprite", typeof(SpriteRenderer));

            try
            {
                child.transform.SetParent(root.transform);

                Assert.IsTrue(GameObjectBoundsUtility.Is2DObject(root));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Is2DObject_ReturnsFalse_ForMixedRendererHierarchy()
        {
            var root = new GameObject("Mixed Source", typeof(SpriteRenderer));
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                mesh.transform.SetParent(root.transform);

                Assert.IsFalse(GameObjectBoundsUtility.Is2DObject(root));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Is2DObject_ReturnsFalse_ForUIObject()
        {
            var source = new GameObject("UI Source", typeof(RectTransform), typeof(CanvasRenderer));

            try
            {
                Assert.IsFalse(GameObjectBoundsUtility.Is2DObject(source));
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void TryGetRendererBounds_CombinesEnabledChildRenderers()
        {
            var root = new GameObject("Root");
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                left.transform.SetParent(root.transform);
                right.transform.SetParent(root.transform);
                left.transform.localPosition = Vector3.left;
                right.transform.localPosition = Vector3.right;

                Assert.IsTrue(GameObjectBoundsUtility.TryGetRendererBounds(root, out Bounds bounds));
                Assert.That(bounds.center.x, Is.EqualTo(0f).Within(0.001f));
                Assert.That(bounds.size.x, Is.EqualTo(3f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GetFramingScale_ReservesPaddingOnBothSides()
        {
            Assert.That(IconRenderSettings.GetFramingScale(0f), Is.EqualTo(1f));
            Assert.That(IconRenderSettings.GetFramingScale(0.1f), Is.EqualTo(1.25f));
        }

        [Test]
        public void Defaults_UseFrontViewAndStudioLighting()
        {
            var settings = new IconRenderSettings();

            Assert.AreEqual(IconViewPreset.Front, settings.ViewPreset);
            Assert.AreEqual(Vector3.zero, settings.Rotation);
            Assert.AreEqual(IconLightingSource.Studio, settings.LightingSource);
            Assert.AreEqual(IconRenderSettings.DefaultMainLightRotation, settings.MainLightRotation);
            Assert.AreEqual(IconRenderSettings.DefaultMainLightIntensity, settings.MainLightIntensity);
            Assert.AreEqual(IconRenderSettings.DefaultFillLightRotation, settings.FillLightRotation);
            Assert.AreEqual(IconRenderSettings.DefaultFillLightIntensity, settings.FillLightIntensity);
        }

        [Test]
        public void GeneratorSettings_ValidateMissingValues()
        {
            var settings = new GameObjectIconGeneratorSettings
            {
                FileName = string.Empty,
                RenderSettings = null,
            };

            settings.Validate();

            Assert.AreEqual(GameObjectIconGeneratorSettings.DefaultFileName, settings.FileName);
            Assert.IsNotNull(settings.RenderSettings);
        }

        [Test]
        public void GetPresetRotation_BackFacesTheOppositeDirection()
        {
            Assert.AreEqual(
                new Vector3(0f, 180f, 0f),
                IconRenderSettings.GetPresetRotation(IconViewPreset.Back)
            );
        }

        [Test]
        public void Clone_PreservesLightingSettings()
        {
            var settings = new IconRenderSettings
            {
                LightingSource = IconLightingSource.Scene,
                MainLightRotation = new Vector3(10f, 20f, 30f),
                MainLightColor = Color.red,
                MainLightIntensity = 2f,
                FillLightRotation = new Vector3(40f, 50f, 60f),
                FillLightColor = Color.blue,
                FillLightIntensity = 0.75f,
            };

            IconRenderSettings clone = settings.Clone();

            Assert.AreEqual(settings.LightingSource, clone.LightingSource);
            Assert.AreEqual(settings.MainLightRotation, clone.MainLightRotation);
            Assert.AreEqual(settings.MainLightColor, clone.MainLightColor);
            Assert.AreEqual(settings.MainLightIntensity, clone.MainLightIntensity);
            Assert.AreEqual(settings.FillLightRotation, clone.FillLightRotation);
            Assert.AreEqual(settings.FillLightColor, clone.FillLightColor);
            Assert.AreEqual(settings.FillLightIntensity, clone.FillLightIntensity);
        }

        [Test]
        public void Validate_ClampsResolutionAndAppliesPresetRotation()
        {
            var settings = new IconRenderSettings
            {
                Resolution = 2,
                Padding = 1f,
                ViewPreset = IconViewPreset.Isometric,
                Rotation = Vector3.zero,
                MainLightIntensity = -1f,
                FillLightIntensity = IconRenderSettings.MaximumLightIntensity + 1f,
            };

            settings.Validate();

            Assert.AreEqual(16, settings.Resolution);
            Assert.AreEqual(IconRenderSettings.MaximumPadding, settings.Padding);
            Assert.AreEqual(
                IconRenderSettings.GetPresetRotation(IconViewPreset.Isometric),
                settings.Rotation
            );
            Assert.AreEqual(0f, settings.MainLightIntensity);
            Assert.AreEqual(IconRenderSettings.MaximumLightIntensity, settings.FillLightIntensity);
        }
    }
}

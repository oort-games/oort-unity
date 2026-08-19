#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using OortUnity.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OortUnity.Editor
{
    internal static class GameObjectIconRenderer
    {
        private const int PreviewLayer = 31;

        public static Texture2D Render(GameObject source, IconRenderSettings settings)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            IconRenderSettings safeSettings = settings.Clone();
            safeSettings.Validate();

            Scene previewScene = default;
            RenderTexture renderTexture = null;

            try
            {
                previewScene = EditorSceneManager.NewPreviewScene();
                renderTexture = RenderTexture.GetTemporary(
                    safeSettings.Resolution,
                    safeSettings.Resolution,
                    24,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Default,
                    1
                );
                renderTexture.name = $"{source.name} Icon Render";
                renderTexture.filterMode = FilterMode.Bilinear;
                renderTexture.wrapMode = TextureWrapMode.Clamp;

                Camera camera = CreateCamera(previewScene, renderTexture, safeSettings);

                if (GameObjectBoundsUtility.IsUIObject(source))
                {
                    PrepareUI(previewScene, camera, source, safeSettings);
                }
                else if (GameObjectBoundsUtility.Is2DObject(source))
                {
                    Prepare2D(previewScene, camera, source, safeSettings);
                }
                else
                {
                    Prepare3D(previewScene, camera, source, safeSettings);
                }

                RenderCamera(camera, renderTexture);

                return TextureUtility.CreateReadableCopy(renderTexture);
            }
            finally
            {
                if (renderTexture != null)
                {
                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                if (previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
        }

        public static byte[] RenderToPng(GameObject source, IconRenderSettings settings)
        {
            Texture2D texture = null;

            try
            {
                texture = Render(source, settings);
                byte[] pngBytes = ImageConversion.EncodeToPNG(texture);

                if (pngBytes == null || pngBytes.Length == 0)
                {
                    throw new InvalidOperationException("Unity failed to encode the icon as PNG.");
                }

                return pngBytes;
            }
            finally
            {
                DestroyTemporaryObject(texture);
            }
        }

        private static Camera CreateCamera(
            Scene scene,
            RenderTexture target,
            IconRenderSettings settings
        )
        {
            var cameraObject = new GameObject("Icon Camera", typeof(Camera))
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(cameraObject, scene);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.cameraType = CameraType.Game;
            camera.scene = scene;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = settings.BackgroundMode == IconBackgroundMode.Transparent
                ? Color.clear
                : settings.BackgroundColor;
            camera.cullingMask = 1 << PreviewLayer;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.useOcclusionCulling = false;
            camera.targetTexture = target;

            return camera;
        }

        private static void Prepare3D(
            Scene scene,
            Camera camera,
            GameObject source,
            IconRenderSettings settings
        )
        {
            var stagingObject = new GameObject("Icon Source")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(stagingObject, scene);
            stagingObject.SetActive(false);

            GameObject clone = UnityEngine.Object.Instantiate(source, stagingObject.transform, false);
            clone.name = source.name;
            clone.SetActive(true);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.Euler(settings.Rotation);
            SetLayerAndHideFlagsRecursively(clone.transform);
            DisableUserBehaviours(clone);
            DisableClonedLights(clone);

            stagingObject.SetActive(true);

            if (!GameObjectBoundsUtility.TryGetRendererBounds(clone, out Bounds bounds))
            {
                throw new InvalidOperationException(
                    $"'{source.name}' does not contain an enabled Renderer."
                );
            }

            if (settings.LightingSource == IconLightingSource.Scene)
            {
                CreateSceneLights(scene, source, clone);
            }
            else
            {
                CreateDirectionalLight(
                    scene,
                    settings.MainLightRotation,
                    settings.MainLightColor,
                    settings.MainLightIntensity
                );
                CreateDirectionalLight(
                    scene,
                    settings.FillLightRotation,
                    settings.FillLightColor,
                    settings.FillLightIntensity
                );
            }

            FrameCamera(camera, bounds, settings);
        }

        private static void Prepare2D(
            Scene scene,
            Camera camera,
            GameObject source,
            IconRenderSettings settings
        )
        {
            var stagingObject = new GameObject("Icon Source")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(stagingObject, scene);
            stagingObject.SetActive(false);

            GameObject clone = UnityEngine.Object.Instantiate(source, stagingObject.transform, false);
            clone.name = source.name;
            clone.SetActive(true);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
            SetLayerAndHideFlagsRecursively(clone.transform);
            DisableUserBehaviours(clone);
            DisableClonedLights(clone);

            stagingObject.SetActive(true);

            if (!GameObjectBoundsUtility.TryGetRendererBounds(clone, out Bounds bounds))
            {
                throw new InvalidOperationException(
                    $"'{source.name}' does not contain an enabled SpriteRenderer."
                );
            }

            Frame2DCamera(camera, bounds, settings);
        }

        private static void PrepareUI(
            Scene scene,
            Camera camera,
            GameObject source,
            IconRenderSettings settings
        )
        {
            if (!GameObjectBoundsUtility.TryGetRectSize(source, out Vector2 sourceSize))
            {
                throw new InvalidOperationException(
                    $"'{source.name}' does not have a usable RectTransform size."
                );
            }

            var canvasObject = new GameObject("Icon Canvas", typeof(RectTransform), typeof(Canvas))
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            canvasObject.SetActive(false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 0;

            GameObject clone = UnityEngine.Object.Instantiate(source, canvasObject.transform, false);
            clone.name = source.name;
            clone.SetActive(true);
            SetLayerAndHideFlagsRecursively(clone.transform);
            DisableUserBehaviours(clone);

            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            cloneRect.anchorMin = Vector2.one * 0.5f;
            cloneRect.anchorMax = Vector2.one * 0.5f;
            cloneRect.pivot = Vector2.one * 0.5f;
            cloneRect.anchoredPosition = Vector2.zero;
            cloneRect.sizeDelta = sourceSize;
            cloneRect.localRotation = Quaternion.identity;

            float availableSize = settings.Resolution / IconRenderSettings.GetFramingScale(settings.Padding);
            float scale = availableSize / Mathf.Max(sourceSize.x, sourceSize.y);
            cloneRect.localScale = Vector3.one * scale;

            camera.orthographic = true;
            camera.orthographicSize = settings.Resolution * 0.5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;

            canvasObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }

        private static void FrameCamera(Camera camera, Bounds bounds, IconRenderSettings settings)
        {
            float framingScale = IconRenderSettings.GetFramingScale(settings.Padding);
            float radius = Mathf.Max(0.001f, bounds.extents.magnitude);
            Vector3 center = bounds.center;

            if (settings.Projection == IconProjection.Orthographic)
            {
                float halfSize = Mathf.Max(bounds.extents.x, bounds.extents.y);
                float distance = Mathf.Max(1f, bounds.extents.z + radius * 2f);

                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(0.001f, halfSize * framingScale);
                camera.transform.position = center + Vector3.forward * distance;
                camera.transform.LookAt(center, Vector3.up);
                camera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 1.5f);
                camera.farClipPlane = distance + radius * 3f;
                return;
            }

            float halfFieldOfView = IconRenderSettings.DefaultPerspectiveFieldOfView * 0.5f * Mathf.Deg2Rad;
            float cameraDistance = radius * framingScale / Mathf.Sin(halfFieldOfView);

            camera.orthographic = false;
            camera.fieldOfView = IconRenderSettings.DefaultPerspectiveFieldOfView;
            camera.transform.position = center + Vector3.forward * cameraDistance;
            camera.transform.LookAt(center, Vector3.up);
            camera.nearClipPlane = Mathf.Max(0.01f, cameraDistance - radius * 1.5f);
            camera.farClipPlane = cameraDistance + radius * 3f;
        }

        private static void Frame2DCamera(
            Camera camera,
            Bounds bounds,
            IconRenderSettings settings
        )
        {
            float framingScale = IconRenderSettings.GetFramingScale(settings.Padding);
            float radius = Mathf.Max(0.001f, bounds.extents.magnitude);
            float halfSize = Mathf.Max(bounds.extents.x, bounds.extents.y);
            float distance = Mathf.Max(1f, bounds.extents.z + radius * 2f);
            Vector3 center = bounds.center;

            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(0.001f, halfSize * framingScale);
            camera.transform.position = center + Vector3.back * distance;
            camera.transform.LookAt(center, Vector3.up);
            camera.nearClipPlane = Mathf.Max(0.01f, distance - radius * 1.5f);
            camera.farClipPlane = distance + radius * 3f;
        }

        private static void CreateDirectionalLight(
            Scene scene,
            Vector3 rotation,
            Color color,
            float intensity
        )
        {
            var lightObject = new GameObject("Icon Light", typeof(Light))
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(lightObject, scene);

            lightObject.transform.rotation = Quaternion.Euler(rotation);

            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << PreviewLayer;
        }

        private static void CreateSceneLights(
            Scene previewScene,
            GameObject source,
            GameObject clone
        )
        {
            var copiedLightIds = new HashSet<int>();
            Scene sourceScene = source.scene.IsValid() && source.scene.isLoaded
                ? source.scene
                : SceneManager.GetActiveScene();

            if (sourceScene.IsValid() && sourceScene.isLoaded)
            {
                foreach (GameObject root in sourceScene.GetRootGameObjects())
                {
                    foreach (Light sourceLight in root.GetComponentsInChildren<Light>(true))
                    {
                        TryCreateSceneLight(
                            previewScene,
                            source,
                            clone,
                            sourceLight,
                            copiedLightIds
                        );
                    }
                }
            }

            foreach (Light sourceLight in source.GetComponentsInChildren<Light>(true))
            {
                TryCreateSceneLight(
                    previewScene,
                    source,
                    clone,
                    sourceLight,
                    copiedLightIds
                );
            }
        }

        private static void TryCreateSceneLight(
            Scene previewScene,
            GameObject source,
            GameObject clone,
            Light sourceLight,
            HashSet<int> copiedLightIds
        )
        {
            bool isActive = sourceLight != null
                && (sourceLight.gameObject.scene.IsValid()
                    ? sourceLight.gameObject.activeInHierarchy
                    : sourceLight.gameObject.activeSelf);

            if (sourceLight == null
                || !sourceLight.enabled
                || !isActive
                || !IsSupportedSceneLight(sourceLight.type)
                || !copiedLightIds.Add(sourceLight.GetInstanceID()))
            {
                return;
            }

            var lightObject = new GameObject($"Scene Light - {sourceLight.name}", typeof(Light))
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = PreviewLayer,
            };
            SceneManager.MoveGameObjectToScene(lightObject, previewScene);

            Transform sourceRoot = source.transform;
            Transform cloneRoot = clone.transform;
            Vector3 relativePosition = sourceRoot.InverseTransformPoint(sourceLight.transform.position);
            Quaternion relativeRotation = Quaternion.Inverse(sourceRoot.rotation)
                * sourceLight.transform.rotation;

            lightObject.transform.position = cloneRoot.TransformPoint(relativePosition);
            lightObject.transform.rotation = cloneRoot.rotation * relativeRotation;

            Light targetLight = lightObject.GetComponent<Light>();
            targetLight.type = sourceLight.type;
            targetLight.color = sourceLight.color;
            targetLight.intensity = sourceLight.intensity;
            targetLight.bounceIntensity = sourceLight.bounceIntensity;
            targetLight.range = sourceLight.range;
            targetLight.spotAngle = sourceLight.spotAngle;
            targetLight.innerSpotAngle = sourceLight.innerSpotAngle;
            targetLight.cookie = sourceLight.cookie;
#if UNITY_6000_3_OR_NEWER
            targetLight.cookieSize2D = sourceLight.cookieSize2D;
#else
            targetLight.cookieSize = sourceLight.cookieSize;
#endif
            targetLight.shadows = sourceLight.shadows;
            targetLight.shadowStrength = sourceLight.shadowStrength;
            targetLight.shadowBias = sourceLight.shadowBias;
            targetLight.shadowNormalBias = sourceLight.shadowNormalBias;
            targetLight.shadowNearPlane = sourceLight.shadowNearPlane;
            targetLight.renderMode = sourceLight.renderMode;
            targetLight.useColorTemperature = sourceLight.useColorTemperature;
            targetLight.colorTemperature = sourceLight.colorTemperature;
            targetLight.renderingLayerMask = sourceLight.renderingLayerMask;
            targetLight.cullingMask = 1 << PreviewLayer;
        }

        private static bool IsSupportedSceneLight(LightType lightType)
        {
            return lightType == LightType.Directional
                || lightType == LightType.Point
                || lightType == LightType.Spot;
        }

        private static void RenderCamera(Camera camera, RenderTexture target)
        {
            Canvas.ForceUpdateCanvases();
            camera.targetTexture = target;
            camera.Render();
        }

        private static void SetLayerAndHideFlagsRecursively(Transform root)
        {
            root.gameObject.layer = PreviewLayer;
            root.gameObject.hideFlags = HideFlags.HideAndDontSave;

            foreach (Transform child in root)
            {
                SetLayerAndHideFlagsRecursively(child);
            }
        }

        private static void DisableUserBehaviours(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                string namespaceName = behaviour.GetType().Namespace ?? string.Empty;
                bool isUIBehaviour = namespaceName.StartsWith("UnityEngine.UI", StringComparison.Ordinal)
                    || namespaceName.StartsWith("TMPro", StringComparison.Ordinal);

                if (!isUIBehaviour)
                {
                    behaviour.enabled = false;
                }
            }
        }

        private static void DisableClonedLights(GameObject root)
        {
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                light.enabled = false;
            }
        }

        private static void DestroyTemporaryObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}

#endif

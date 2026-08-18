#if UNITY_EDITOR

using System;
using UnityEngine;

namespace OortUnity.Editor
{
    internal enum IconBackgroundMode
    {
        Transparent,
        SolidColor,
    }

    internal enum IconViewPreset
    {
        Front = 0,
        Isometric = 1,
        Custom = 2,
        Back = 3,
    }

    internal enum IconProjection
    {
        Perspective,
        Orthographic,
    }

    internal enum IconLightingSource
    {
        Studio,
        Scene,
    }

    [Serializable]
    internal sealed class IconRenderSettings
    {
        public const int DefaultResolution = 256;
        public const float DefaultPadding = 0.1f;
        public const float MaximumPadding = 0.45f;
        public const float DefaultPerspectiveFieldOfView = 30f;
        public const float DefaultMainLightIntensity = 1.15f;
        public const float DefaultFillLightIntensity = 0.45f;
        public const float MaximumLightIntensity = 8f;

        public static readonly Vector3 DefaultMainLightRotation = new Vector3(35f, -30f, 0f);
        public static readonly Vector3 DefaultFillLightRotation = new Vector3(340f, 145f, 0f);

        public int Resolution = DefaultResolution;
        public IconBackgroundMode BackgroundMode = IconBackgroundMode.Transparent;
        public Color BackgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        public float Padding = DefaultPadding;
        public IconViewPreset ViewPreset = IconViewPreset.Front;
        public Vector3 Rotation = GetPresetRotation(IconViewPreset.Front);
        public IconProjection Projection = IconProjection.Orthographic;
        public IconLightingSource LightingSource = IconLightingSource.Studio;
        public Vector3 MainLightRotation = DefaultMainLightRotation;
        public Color MainLightColor = Color.white;
        public float MainLightIntensity = DefaultMainLightIntensity;
        public Vector3 FillLightRotation = DefaultFillLightRotation;
        public Color FillLightColor = Color.white;
        public float FillLightIntensity = DefaultFillLightIntensity;

        public void Validate()
        {
            Resolution = Mathf.Clamp(Resolution, 16, 4096);
            Padding = Mathf.Clamp(Padding, 0f, MaximumPadding);
            MainLightColor.a = 1f;
            MainLightIntensity = Mathf.Clamp(MainLightIntensity, 0f, MaximumLightIntensity);
            FillLightColor.a = 1f;
            FillLightIntensity = Mathf.Clamp(FillLightIntensity, 0f, MaximumLightIntensity);

            if (ViewPreset != IconViewPreset.Custom)
            {
                Rotation = GetPresetRotation(ViewPreset);
            }
        }

        public IconRenderSettings Clone()
        {
            return new IconRenderSettings
            {
                Resolution = Resolution,
                BackgroundMode = BackgroundMode,
                BackgroundColor = BackgroundColor,
                Padding = Padding,
                ViewPreset = ViewPreset,
                Rotation = Rotation,
                Projection = Projection,
                LightingSource = LightingSource,
                MainLightRotation = MainLightRotation,
                MainLightColor = MainLightColor,
                MainLightIntensity = MainLightIntensity,
                FillLightRotation = FillLightRotation,
                FillLightColor = FillLightColor,
                FillLightIntensity = FillLightIntensity,
            };
        }

        public static Vector3 GetPresetRotation(IconViewPreset preset)
        {
            return preset switch
            {
                IconViewPreset.Back => new Vector3(0f, 180f, 0f),
                IconViewPreset.Isometric => new Vector3(20f, -30f, 0f),
                _ => Vector3.zero,
            };
        }

        public static float GetFramingScale(float padding)
        {
            float safePadding = Mathf.Clamp(padding, 0f, MaximumPadding);

            return 1f / Mathf.Max(0.1f, 1f - safePadding * 2f);
        }
    }
}

#endif

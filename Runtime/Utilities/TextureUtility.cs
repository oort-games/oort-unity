using System;
using UnityEngine;

namespace OortUnity.Utilities
{
    public static class TextureUtility
    {
        #region Copy

        /// <summary>
        /// GPU 텍스처를 CPU에서 읽을 수 있는 RGBA32 Texture2D로 복사하고 원본의 색 공간을 유지합니다.
        /// 반환된 텍스처는 호출자가 해제해야 합니다.
        /// </summary>
        /// <param name="source">복사할 원본 텍스처입니다.</param>
        /// <returns>읽기 가능한 Texture2D 복사본입니다.</returns>
        public static Texture2D CreateReadableCopy(Texture source)
        {
            ValidateTexture(source, nameof(source));

            return CopyToReadableTexture(source, source.width, source.height, $"{source.name} (Readable)");
        }

        #endregion

        #region Resize

        /// <summary>
        /// 텍스처를 지정한 크기의 읽기 가능한 RGBA32 Texture2D로 리사이즈합니다.
        /// 원본 텍스처의 Read/Write 설정과 관계없이 사용할 수 있으며, 반환된 텍스처는 호출자가 해제해야 합니다.
        /// </summary>
        /// <param name="source">리사이즈할 원본 텍스처입니다.</param>
        /// <param name="width">결과 너비입니다.</param>
        /// <param name="height">결과 높이입니다.</param>
        /// <returns>리사이즈된 읽기 가능한 Texture2D입니다.</returns>
        public static Texture2D Resize(Texture source, int width, int height)
        {
            ValidateTexture(source, nameof(source));

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
            }

            return CopyToReadableTexture(source, width, height, $"{source.name} ({width}x{height})");
        }

        #endregion

        #region Blend

        /// <summary>
        /// 원본 텍스처를 목적 텍스처의 지정 위치에 알파 합성합니다.
        /// 두 텍스처 모두 읽기 가능해야 합니다.
        /// </summary>
        /// <param name="destination">합성 결과가 기록될 목적 텍스처입니다.</param>
        /// <param name="source">합성할 원본 텍스처입니다.</param>
        /// <param name="x">목적 텍스처 왼쪽 아래를 기준으로 한 X 좌표입니다.</param>
        /// <param name="y">목적 텍스처 왼쪽 아래를 기준으로 한 Y 좌표입니다.</param>
        /// <param name="opacity">원본 텍스처에 적용할 불투명도입니다.</param>
        public static void Blend(Texture2D destination, Texture2D source, int x, int y, float opacity = 1f)
        {
            Blend(destination, source, x, y, Color.white, opacity);
        }

        /// <summary>
        /// 원본 텍스처에 틴트와 불투명도를 적용한 뒤 목적 텍스처의 지정 위치에 알파 합성합니다.
        /// 두 텍스처 모두 읽기 가능해야 합니다.
        /// </summary>
        /// <param name="destination">합성 결과가 기록될 목적 텍스처입니다.</param>
        /// <param name="source">합성할 원본 텍스처입니다.</param>
        /// <param name="x">목적 텍스처 왼쪽 아래를 기준으로 한 X 좌표입니다.</param>
        /// <param name="y">목적 텍스처 왼쪽 아래를 기준으로 한 Y 좌표입니다.</param>
        /// <param name="tint">원본 색상과 알파에 곱할 틴트입니다.</param>
        /// <param name="opacity">원본 텍스처에 적용할 불투명도입니다.</param>
        public static void Blend(Texture2D destination, Texture2D source, int x, int y, Color tint, float opacity = 1f)
        {
            ValidateReadableTexture(destination, nameof(destination));
            ValidateReadableTexture(source, nameof(source));

            int sourceStartX = Mathf.Max(0, -x);
            int sourceStartY = Mathf.Max(0, -y);
            int destinationStartX = Mathf.Max(0, x);
            int destinationStartY = Mathf.Max(0, y);

            int blendWidth = Mathf.Min(source.width - sourceStartX, destination.width - destinationStartX);
            int blendHeight = Mathf.Min(source.height - sourceStartY, destination.height - destinationStartY);

            if (blendWidth <= 0 || blendHeight <= 0)
            {
                return;
            }

            float tintRed = Mathf.Clamp01(tint.r);
            float tintGreen = Mathf.Clamp01(tint.g);
            float tintBlue = Mathf.Clamp01(tint.b);
            float alphaMultiplier = Mathf.Clamp01(tint.a) * Mathf.Clamp01(opacity);

            Color32[] destinationPixels = destination.GetPixels32();
            Color32[] sourcePixels = source.GetPixels32();

            for (int row = 0; row < blendHeight; row++)
            {
                int sourceRow = (sourceStartY + row) * source.width + sourceStartX;
                int destinationRow = (destinationStartY + row) * destination.width + destinationStartX;

                for (int column = 0; column < blendWidth; column++)
                {
                    int sourceIndex = sourceRow + column;
                    int destinationIndex = destinationRow + column;
                    Color32 destinationPixel = destinationPixels[destinationIndex];
                    Color32 sourcePixel = sourcePixels[sourceIndex];

                    destinationPixels[destinationIndex] = BlendPixel(destinationPixel, sourcePixel, tintRed, tintGreen, tintBlue, alphaMultiplier);
                }
            }

            destination.SetPixels32(destinationPixels);
            destination.Apply(false, false);
        }

        private static Color32 BlendPixel(Color32 destination, Color32 source, float tintRed, float tintGreen, float tintBlue, float alphaMultiplier)
        {
            float sourceAlpha = source.a / 255f * alphaMultiplier;

            if (sourceAlpha <= 0f)
            {
                return destination;
            }

            float destinationAlpha = destination.a / 255f;
            float inverseSourceAlpha = 1f - sourceAlpha;
            float outputAlpha = sourceAlpha + destinationAlpha * inverseSourceAlpha;

            if (outputAlpha <= 0f)
            {
                return new Color32(0, 0, 0, 0);
            }

            float sourceRed = source.r / 255f * tintRed;
            float sourceGreen = source.g / 255f * tintGreen;
            float sourceBlue = source.b / 255f * tintBlue;

            float destinationRed = destination.r / 255f;
            float destinationGreen = destination.g / 255f;
            float destinationBlue = destination.b / 255f;

            float outputRed = (sourceRed * sourceAlpha + destinationRed * destinationAlpha * inverseSourceAlpha) / outputAlpha;
            float outputGreen = (sourceGreen * sourceAlpha + destinationGreen * destinationAlpha * inverseSourceAlpha) / outputAlpha;
            float outputBlue = (sourceBlue * sourceAlpha + destinationBlue * destinationAlpha * inverseSourceAlpha) / outputAlpha;

            return new Color32(ToByte(outputRed), ToByte(outputGreen), ToByte(outputBlue), ToByte(outputAlpha));
        }

        #endregion

        #region Internal

        private static Texture2D CopyToReadableTexture(Texture source, int width, int height, string textureName)
        {
            bool isDataSrgb = source.isDataSRGB;
            RenderTextureReadWrite readWrite = isDataSrgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
            RenderTexture temporary = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                readWrite
            );
            RenderTexture previous = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            Texture2D result = null;

            try
            {
                temporary.filterMode = FilterMode.Bilinear;
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear && isDataSrgb;
                Graphics.Blit(source, temporary);

                RenderTexture.active = temporary;

                result = new Texture2D(width, height, TextureFormat.RGBA32, false, !isDataSrgb) { name = textureName };
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                result.Apply(false, false);

                return result;
            }
            catch
            {
                if (result != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(result);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(result);
                    }
                }

                throw;
            }
            finally
            {
                GL.sRGBWrite = previousSrgbWrite;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static void ValidateTexture(Texture texture, string parameterName)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (texture.width <= 0 || texture.height <= 0)
            {
                throw new ArgumentException("Texture dimensions must be greater than zero.", parameterName);
            }
        }

        private static void ValidateReadableTexture(Texture2D texture, string parameterName)
        {
            ValidateTexture(texture, parameterName);

            if (!texture.isReadable)
            {
                throw new ArgumentException("Texture must be readable.", parameterName);
            }
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        }

        #endregion
    }
}

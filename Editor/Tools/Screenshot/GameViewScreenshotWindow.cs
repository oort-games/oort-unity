#if UNITY_EDITOR

using System;
using System.Collections;
using System.IO;
using OortUnity.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OortUnity.Editor
{
    public class GameViewScreenshotWindow : EditorWindow
    {
        #region Constants

        private const string MenuPath = "Oort/Tools/Game View Screenshot";
        private const string WindowTitle = "Game View Screenshot";
        private const string HeaderTitle = "Game View Screenshot";

        private const string DefaultFileName = "GameView";
        private const string DefaultDirectoryName = "Screenshots";

        private const int MinimumWatermarkPercent = 1;
        private const int MaximumWatermarkPercent = 100;

        #endregion

        #region Fields

        [SerializeField]
        private string _fileName = DefaultFileName;

        private string _outputDirectory;

        private TextField _fileNameField;
        private TextField _outputDirectoryField;
        private Toggle _watermarkEnabledToggle;
        private ObjectField _watermarkTextureField;
        private EnumField _watermarkAnchorField;
        private SliderInt _watermarkSizeField;
        private SliderInt _watermarkOpacityField;
        private IntegerField _watermarkMarginField;
        private VisualElement _watermarkOptions;
        private Label _watermarkPreviewLabel;
        private Button _captureButton;

        private bool _isCapturing;

        #endregion

        #region Window

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            GameViewScreenshotWindow window = GetWindow<GameViewScreenshotWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(460f, 500f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOutputDirectory();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            root.Clear();

            VisualElementUtility.ApplyRootStyle(root);

            root.Add(VisualElementUtility.CreateHeader(HeaderTitle));
            root.Add(CreateContent());
        }

        #endregion

        #region UI

        private VisualElement CreateContent()
        {
            var content = new VisualElement();
            content.AddToClassList(OortStyleClasses.Content);

            content.Add(CreateOutputDirectorySection());
            content.Add(CreateFileNameSection());
            content.Add(CreateWatermarkSection());
            content.Add(CreateCaptureButton());

            return content;
        }

        private VisualElement CreateOutputDirectorySection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);

            section.Add(new Label("Save Directory"));

            var row = new VisualElement();
            row.AddToClassList(OortStyleClasses.PathRow);

            _outputDirectoryField = new TextField { value = _outputDirectory };

            _outputDirectoryField.AddToClassList(OortStyleClasses.PathField);
            _outputDirectoryField.RegisterValueChangedCallback(evt => SetOutputDirectory(evt.newValue));

            var browseButton = new Button(BrowseOutputDirectory) { text = "Browse" };
            browseButton.AddToClassList(OortStyleClasses.SmallButton);

            var openButton = new Button(OpenOutputDirectory) { text = "Open" };
            openButton.AddToClassList(OortStyleClasses.SmallButton);

            var resetButton = new Button(ResetOutputDirectory) { text = "Reset" };
            resetButton.AddToClassList(OortStyleClasses.SmallButton);

            row.Add(_outputDirectoryField);
            row.Add(browseButton);
            row.Add(openButton);
            row.Add(resetButton);

            section.Add(row);

            return section;
        }

        private VisualElement CreateFileNameSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);

            _fileNameField = new TextField("File Name") { value = _fileName };

            _fileNameField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);

            section.Add(_fileNameField);

            return section;
        }

        private Button CreateCaptureButton()
        {
            _captureButton = new Button(CaptureScreenshot) { text = "Capture Screenshot" };

            _captureButton.AddToClassList(OortStyleClasses.PrimaryButton);

            return _captureButton;
        }

        private VisualElement CreateWatermarkSection()
        {
            OortUnityUserSettings settings = OortUnityUserSettings.instance;

            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Watermark"));

            _watermarkEnabledToggle = new Toggle("Enable Watermark")
            {
                value = settings.GameViewScreenshotWatermarkEnabled,
            };
            _watermarkEnabledToggle.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkEnabled = evt.newValue;
                UpdateWatermarkOptions();
            });

            _watermarkOptions = new VisualElement();
            _watermarkOptions.AddToClassList(OortStyleClasses.Screenshot.WatermarkOptions);

            _watermarkTextureField = new ObjectField("Texture")
            {
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
                value = settings.GameViewScreenshotWatermarkTexture,
                tooltip = "PNG or Texture2D asset used as the watermark",
            };
            _watermarkTextureField.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkTexture = evt.newValue as Texture2D;
                UpdateWatermarkPreview();
            });

            _watermarkAnchorField = new EnumField("Position", settings.GameViewScreenshotWatermarkAnchor);
            _watermarkAnchorField.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkAnchor = (ScreenshotWatermarkAnchor)evt.newValue;
                UpdateWatermarkPreview();
            });

            _watermarkSizeField = new SliderInt(
                "Size (% Width)",
                MinimumWatermarkPercent,
                MaximumWatermarkPercent
            )
            {
                value = Mathf.RoundToInt(settings.GameViewScreenshotWatermarkSizeRatio * 100f),
                showInputField = true,
            };
            _watermarkSizeField.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkSizeRatio = evt.newValue / 100f;
                UpdateWatermarkPreview();
            });

            _watermarkOpacityField = new SliderInt("Opacity (%)", 0, MaximumWatermarkPercent)
            {
                value = Mathf.RoundToInt(settings.GameViewScreenshotWatermarkOpacity * 100f),
                showInputField = true,
            };
            _watermarkOpacityField.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkOpacity = evt.newValue / 100f;
                UpdateWatermarkPreview();
            });

            _watermarkMarginField = new IntegerField("Margin (px)")
            {
                value = settings.GameViewScreenshotWatermarkMargin,
            };
            _watermarkMarginField.RegisterValueChangedCallback(evt =>
            {
                settings.GameViewScreenshotWatermarkMargin = Mathf.Max(0, evt.newValue);
                _watermarkMarginField.SetValueWithoutNotify(settings.GameViewScreenshotWatermarkMargin);
                UpdateWatermarkPreview();
            });

            _watermarkPreviewLabel = new Label();
            _watermarkPreviewLabel.AddToClassList(OortStyleClasses.Screenshot.WatermarkPreview);

            var playModeNotice = new Label("Watermarked capture is available in Play Mode.");
            playModeNotice.AddToClassList(OortStyleClasses.Screenshot.WatermarkNotice);

            var resetButton = new Button(ResetWatermarkOptions)
            {
                text = "Reset Watermark",
                tooltip = "Reset all watermark settings",
            };
            resetButton.AddToClassList(OortStyleClasses.SmallButton);
            resetButton.AddToClassList(OortStyleClasses.Screenshot.WatermarkResetButton);

            _watermarkOptions.Add(_watermarkTextureField);
            _watermarkOptions.Add(_watermarkAnchorField);
            _watermarkOptions.Add(_watermarkSizeField);
            _watermarkOptions.Add(_watermarkOpacityField);
            _watermarkOptions.Add(_watermarkMarginField);
            _watermarkOptions.Add(_watermarkPreviewLabel);
            _watermarkOptions.Add(playModeNotice);

            section.Add(_watermarkEnabledToggle);
            section.Add(_watermarkOptions);
            section.Add(resetButton);

            UpdateWatermarkOptions();

            return section;
        }

        private void UpdateWatermarkOptions()
        {
            _watermarkOptions?.SetEnabled(_watermarkEnabledToggle?.value ?? false);
            UpdateWatermarkPreview();
        }

        private void UpdateWatermarkPreview()
        {
            if (_watermarkPreviewLabel == null)
            {
                return;
            }

            OortUnityUserSettings settings = OortUnityUserSettings.instance;
            string textureName = settings.GameViewScreenshotWatermarkTexture != null
                ? settings.GameViewScreenshotWatermarkTexture.name
                : "No texture selected";
            string position = ObjectNames.NicifyVariableName(settings.GameViewScreenshotWatermarkAnchor.ToString());
            int sizePercent = Mathf.RoundToInt(settings.GameViewScreenshotWatermarkSizeRatio * 100f);
            int opacityPercent = Mathf.RoundToInt(settings.GameViewScreenshotWatermarkOpacity * 100f);

            _watermarkPreviewLabel.text =
                $"{textureName} · {position} · {sizePercent}% width · "
                + $"{opacityPercent}% opacity · {settings.GameViewScreenshotWatermarkMargin}px margin";
        }

        private void ResetWatermarkOptions()
        {
            OortUnityUserSettings settings = OortUnityUserSettings.instance;
            settings.ResetGameViewScreenshotWatermark();

            _watermarkEnabledToggle?.SetValueWithoutNotify(settings.GameViewScreenshotWatermarkEnabled);
            _watermarkTextureField?.SetValueWithoutNotify(settings.GameViewScreenshotWatermarkTexture);
            _watermarkAnchorField?.SetValueWithoutNotify(settings.GameViewScreenshotWatermarkAnchor);
            _watermarkSizeField?.SetValueWithoutNotify(Mathf.RoundToInt(settings.GameViewScreenshotWatermarkSizeRatio * 100f));
            _watermarkOpacityField?.SetValueWithoutNotify(Mathf.RoundToInt(settings.GameViewScreenshotWatermarkOpacity * 100f));
            _watermarkMarginField?.SetValueWithoutNotify(settings.GameViewScreenshotWatermarkMargin);

            UpdateWatermarkOptions();
        }

        #endregion

        #region Directory

        private string GetDefaultOutputDirectory()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            return PathUtility.NormalizePath(Path.Combine(documentsPath, Application.productName, DefaultDirectoryName));
        }

        private void LoadOutputDirectory()
        {
            string savedDirectory = OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory;

            _outputDirectory = string.IsNullOrWhiteSpace(savedDirectory) ? GetDefaultOutputDirectory() : savedDirectory;
        }

        private void SetOutputDirectory(string directory)
        {
            _outputDirectory = PathUtility.NormalizePath(directory);

            OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory = _outputDirectory;
        }

        private void ResetOutputDirectory()
        {
            OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory = string.Empty;

            _outputDirectory = GetDefaultOutputDirectory();
            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        private void BrowseOutputDirectory()
        {
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Select Screenshot Directory",
                _outputDirectory,
                string.Empty
            );

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            SetOutputDirectory(selectedPath);

            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        private void OpenOutputDirectory()
        {
            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                Debug.LogWarning("[Game View Screenshot] Save directory is empty.");

                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);

            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _outputDirectory,
                    UseShellExecute = true,
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(WindowTitle, $"Failed to open the save directory.\n\n{exception.Message}", "OK");
            }
        }

        #endregion

        #region Screenshot

        private void CaptureScreenshot()
        {
            if (!ValidateSettings())
            {
                return;
            }

            OortUnityUserSettings settings = OortUnityUserSettings.instance;

            if (!settings.GameViewScreenshotWatermarkEnabled)
            {
                CaptureScreenshotWithoutWatermark();
                return;
            }

            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    WindowTitle,
                    "Watermarked screenshots can only be captured in Play Mode.",
                    "OK"
                );

                return;
            }

            if (settings.GameViewScreenshotWatermarkTexture == null)
            {
                EditorUtility.DisplayDialog(WindowTitle, "Select a watermark texture before capturing.", "OK");
                return;
            }

            if (_isCapturing)
            {
                Debug.LogWarning("[Game View Screenshot] A watermarked capture is already in progress.");
                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);

            string filePath = CreateScreenshotPath();
            Texture2D watermark = settings.GameViewScreenshotWatermarkTexture;
            ScreenshotWatermarkAnchor anchor = settings.GameViewScreenshotWatermarkAnchor;
            float sizeRatio = settings.GameViewScreenshotWatermarkSizeRatio;
            float opacity = settings.GameViewScreenshotWatermarkOpacity;
            int margin = settings.GameViewScreenshotWatermarkMargin;

            SetCaptureInProgress(true);

            GameViewScreenshotCaptureRunner.Run(
                CaptureWatermarkedScreenshot(filePath, watermark, anchor, sizeRatio, opacity, margin),
                () => SetCaptureInProgress(false)
            );
        }

        private void CaptureScreenshotWithoutWatermark()
        {
            FileUtility.CreateDirectory(_outputDirectory);

            string filePath = CreateScreenshotPath();

            ScreenCapture.CaptureScreenshot(filePath);

            Debug.Log($"<color=cyan>[Game View Screenshot]</color> {filePath}");
        }

        private IEnumerator CaptureWatermarkedScreenshot(
            string filePath,
            Texture2D watermark,
            ScreenshotWatermarkAnchor anchor,
            float sizeRatio,
            float opacity,
            int margin
        )
        {
            yield return new WaitForEndOfFrame();

            Texture2D capturedScreenshot = null;
            Texture2D resizedWatermark = null;

            try
            {
                capturedScreenshot = ScreenCapture.CaptureScreenshotAsTexture();

                if (capturedScreenshot == null)
                {
                    throw new InvalidOperationException("Unity did not return a screenshot texture.");
                }

                int safeMargin = Mathf.Clamp(
                    margin,
                    0,
                    Mathf.Max(0, (Mathf.Min(capturedScreenshot.width, capturedScreenshot.height) - 1) / 2)
                );
                Vector2Int watermarkSize = CalculateWatermarkSize(
                    watermark,
                    capturedScreenshot.width,
                    capturedScreenshot.height,
                    sizeRatio,
                    safeMargin
                );

                resizedWatermark = TextureUtility.Resize(watermark, watermarkSize.x, watermarkSize.y);

                Vector2Int position = CalculateWatermarkPosition(
                    anchor,
                    capturedScreenshot.width,
                    capturedScreenshot.height,
                    resizedWatermark.width,
                    resizedWatermark.height,
                    safeMargin
                );

                TextureUtility.Blend(
                    capturedScreenshot,
                    resizedWatermark,
                    position.x,
                    position.y,
                    Color.white,
                    opacity
                );

                byte[] pngBytes = ImageConversion.EncodeToPNG(capturedScreenshot);

                if (pngBytes == null || pngBytes.Length == 0)
                {
                    throw new InvalidOperationException("Unity failed to encode the screenshot as PNG.");
                }

                FileUtility.WriteAllBytes(filePath, pngBytes);

                Debug.Log($"<color=cyan>[Game View Screenshot]</color> {filePath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    WindowTitle,
                    $"Failed to capture the watermarked screenshot.\n\n{exception.Message}",
                    "OK"
                );
            }
            finally
            {
                DestroyTemporaryTexture(resizedWatermark);
                DestroyTemporaryTexture(capturedScreenshot);
            }
        }

        internal static Vector2Int CalculateWatermarkSize(
            Texture2D watermark,
            int screenshotWidth,
            int screenshotHeight,
            float sizeRatio,
            int margin
        )
        {
            int availableWidth = Mathf.Max(1, screenshotWidth - margin * 2);
            int availableHeight = Mathf.Max(1, screenshotHeight - margin * 2);
            int width = Mathf.Clamp(
                Mathf.RoundToInt(screenshotWidth * Mathf.Clamp(sizeRatio, 0.01f, 1f)),
                1,
                availableWidth
            );
            float aspectRatio = watermark.height / (float)watermark.width;
            int height = Mathf.Max(1, Mathf.RoundToInt(width * aspectRatio));

            if (height > availableHeight)
            {
                height = availableHeight;
                width = Mathf.Max(1, Mathf.RoundToInt(height / aspectRatio));
            }

            return new Vector2Int(width, height);
        }

        internal static Vector2Int CalculateWatermarkPosition(
            ScreenshotWatermarkAnchor anchor,
            int screenshotWidth,
            int screenshotHeight,
            int watermarkWidth,
            int watermarkHeight,
            int margin
        )
        {
            int left = margin;
            int centerX = (screenshotWidth - watermarkWidth) / 2;
            int right = screenshotWidth - watermarkWidth - margin;

            int bottom = margin;
            int centerY = (screenshotHeight - watermarkHeight) / 2;
            int top = screenshotHeight - watermarkHeight - margin;

            switch (anchor)
            {
                case ScreenshotWatermarkAnchor.TopLeft:
                    return new Vector2Int(left, top);
                case ScreenshotWatermarkAnchor.TopCenter:
                    return new Vector2Int(centerX, top);
                case ScreenshotWatermarkAnchor.TopRight:
                    return new Vector2Int(right, top);
                case ScreenshotWatermarkAnchor.MiddleLeft:
                    return new Vector2Int(left, centerY);
                case ScreenshotWatermarkAnchor.Center:
                    return new Vector2Int(centerX, centerY);
                case ScreenshotWatermarkAnchor.MiddleRight:
                    return new Vector2Int(right, centerY);
                case ScreenshotWatermarkAnchor.BottomLeft:
                    return new Vector2Int(left, bottom);
                case ScreenshotWatermarkAnchor.BottomCenter:
                    return new Vector2Int(centerX, bottom);
                default:
                    return new Vector2Int(right, bottom);
            }
        }

        private void SetCaptureInProgress(bool isCapturing)
        {
            _isCapturing = isCapturing;

            if (_captureButton == null)
            {
                return;
            }

            _captureButton.SetEnabled(!isCapturing);
            _captureButton.text = isCapturing ? "Capturing..." : "Capture Screenshot";
        }

        private static void DestroyTemporaryTexture(Texture2D texture)
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        private string CreateScreenshotPath()
        {
            string safeFileName = PathUtility.SanitizeFileName(_fileName);

            return PathUtility.GetUniqueFilePath(_outputDirectory, safeFileName, "png");
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                Debug.LogError("[Game View Screenshot] Save directory is empty.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(_fileName))
            {
                Debug.LogError("[Game View Screenshot] File name is empty.");

                return false;
            }

            string safeFileName = PathUtility.SanitizeFileName(_fileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                Debug.LogError("[Game View Screenshot] File name is invalid.");

                return false;
            }

            return true;
        }

        #endregion
    }
}

#endif

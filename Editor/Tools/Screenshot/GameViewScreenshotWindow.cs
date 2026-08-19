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
    internal sealed class GameViewScreenshotWindow : EditorWindow
    {
        #region Constants

        private const string MenuPath = "Oort/Tools/Game View Screenshot";
        private const string WindowTitle = "Game View Screenshot";
        private const string HeaderTitle = "Game View Screenshot";

        private const string DefaultFileName = "GameView";

        private const int MinimumWatermarkPercent = 1;
        private const int MaximumWatermarkPercent = 100;

        #endregion

        #region Fields

        [SerializeField]
        private string _fileName = DefaultFileName;

        private string _outputDirectory;
        private GameViewScreenshotSettings _settings;

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
            OortUnityUserSettings.PreferencesChanged += ReloadPreferences;
            _settings = OortUnityUserSettings.instance.GameViewScreenshot;
            _settings.Validate();
            LoadOutputDirectory();
        }

        private void OnDisable()
        {
            OortUnityUserSettings.PreferencesChanged -= ReloadPreferences;
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

        private void ReloadPreferences()
        {
            _settings = OortUnityUserSettings.instance.GameViewScreenshot;
            _settings.Validate();
            LoadOutputDirectory();
            CreateGUI();
        }

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
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Watermark"));

            _watermarkEnabledToggle = new Toggle("Enable Watermark")
            {
                value = _settings.WatermarkEnabled,
            };
            _watermarkEnabledToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkEnabled = evt.newValue;
                SaveSettings();
                UpdateWatermarkOptions();
            });

            _watermarkOptions = new VisualElement();
            _watermarkOptions.AddToClassList(OortStyleClasses.Screenshot.WatermarkOptions);

            _watermarkTextureField = new ObjectField("Texture")
            {
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
                value = _settings.WatermarkTexture,
                tooltip = "PNG or Texture2D asset used as the watermark",
            };
            _watermarkTextureField.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkTexture = evt.newValue as Texture2D;
                SaveSettings();
                UpdateWatermarkPreview();
            });

            _watermarkAnchorField = new EnumField("Position", _settings.WatermarkAnchor);
            _watermarkAnchorField.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkAnchor = (ScreenshotWatermarkAnchor)evt.newValue;
                SaveSettings();
                UpdateWatermarkPreview();
            });

            _watermarkSizeField = new SliderInt(
                "Size (% Width)",
                MinimumWatermarkPercent,
                MaximumWatermarkPercent
            )
            {
                value = Mathf.RoundToInt(_settings.WatermarkSizeRatio * 100f),
                showInputField = true,
            };
            _watermarkSizeField.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkSizeRatio = evt.newValue / 100f;
                SaveSettings();
                UpdateWatermarkPreview();
            });

            _watermarkOpacityField = new SliderInt("Opacity (%)", 0, MaximumWatermarkPercent)
            {
                value = Mathf.RoundToInt(_settings.WatermarkOpacity * 100f),
                showInputField = true,
            };
            _watermarkOpacityField.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkOpacity = evt.newValue / 100f;
                SaveSettings();
                UpdateWatermarkPreview();
            });

            _watermarkMarginField = new IntegerField("Margin (px)")
            {
                value = _settings.WatermarkMargin,
            };
            _watermarkMarginField.RegisterValueChangedCallback(evt =>
            {
                _settings.WatermarkMargin = Mathf.Max(0, evt.newValue);
                _watermarkMarginField.SetValueWithoutNotify(_settings.WatermarkMargin);
                SaveSettings();
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

            string textureName = _settings.WatermarkTexture != null
                ? _settings.WatermarkTexture.name
                : "No texture selected";
            string position = ObjectNames.NicifyVariableName(_settings.WatermarkAnchor.ToString());
            int sizePercent = Mathf.RoundToInt(_settings.WatermarkSizeRatio * 100f);
            int opacityPercent = Mathf.RoundToInt(_settings.WatermarkOpacity * 100f);

            _watermarkPreviewLabel.text =
                $"{textureName} · {position} · {sizePercent}% width · "
                + $"{opacityPercent}% opacity · {_settings.WatermarkMargin}px margin";
        }

        private void ResetWatermarkOptions()
        {
            _settings.ResetWatermark();
            SaveSettings();

            _watermarkEnabledToggle?.SetValueWithoutNotify(_settings.WatermarkEnabled);
            _watermarkTextureField?.SetValueWithoutNotify(_settings.WatermarkTexture);
            _watermarkAnchorField?.SetValueWithoutNotify(_settings.WatermarkAnchor);
            _watermarkSizeField?.SetValueWithoutNotify(Mathf.RoundToInt(_settings.WatermarkSizeRatio * 100f));
            _watermarkOpacityField?.SetValueWithoutNotify(Mathf.RoundToInt(_settings.WatermarkOpacity * 100f));
            _watermarkMarginField?.SetValueWithoutNotify(_settings.WatermarkMargin);

            UpdateWatermarkOptions();
        }

        private void SaveSettings()
        {
            OortUnityUserSettings.instance.SaveGameViewScreenshotSettings();
        }

        #endregion

        #region Directory

        private string GetDefaultOutputDirectory()
        {
            return EditorDirectoryUtility.GetDefaultOutputDirectory(
                GameViewScreenshotSettings.DefaultDirectoryName
            );
        }

        private void LoadOutputDirectory()
        {
            string savedDirectory = _settings.OutputDirectory;

            _outputDirectory = string.IsNullOrWhiteSpace(savedDirectory) ? GetDefaultOutputDirectory() : savedDirectory;
        }

        private void SetOutputDirectory(string directory)
        {
            _outputDirectory = PathUtility.NormalizePath(directory);
            _settings.OutputDirectory = _outputDirectory;
            SaveSettings();
        }

        private void ResetOutputDirectory()
        {
            _settings.OutputDirectory = string.Empty;
            SaveSettings();

            _outputDirectory = GetDefaultOutputDirectory();
            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        private void BrowseOutputDirectory()
        {
            if (!EditorDirectoryUtility.TryBrowseDirectory(
                "Select Screenshot Directory",
                _outputDirectory,
                out string selectedPath
            ))
            {
                return;
            }

            SetOutputDirectory(selectedPath);
            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        private void OpenOutputDirectory()
        {
            EditorDirectoryUtility.OpenDirectory(WindowTitle, _outputDirectory);
        }

        #endregion

        #region Screenshot

        private void CaptureScreenshot()
        {
            if (!ValidateSettings())
            {
                return;
            }

            if (!_settings.WatermarkEnabled)
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

            if (_settings.WatermarkTexture == null)
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
            Texture2D watermark = _settings.WatermarkTexture;
            ScreenshotWatermarkAnchor anchor = _settings.WatermarkAnchor;
            float sizeRatio = _settings.WatermarkSizeRatio;
            float opacity = _settings.WatermarkOpacity;
            int margin = _settings.WatermarkMargin;

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

            return anchor switch
            {
                ScreenshotWatermarkAnchor.TopLeft => new Vector2Int(left, top),
                ScreenshotWatermarkAnchor.TopCenter => new Vector2Int(centerX, top),
                ScreenshotWatermarkAnchor.TopRight => new Vector2Int(right, top),
                ScreenshotWatermarkAnchor.MiddleLeft => new Vector2Int(left, centerY),
                ScreenshotWatermarkAnchor.Center => new Vector2Int(centerX, centerY),
                ScreenshotWatermarkAnchor.MiddleRight => new Vector2Int(right, centerY),
                ScreenshotWatermarkAnchor.BottomLeft => new Vector2Int(left, bottom),
                ScreenshotWatermarkAnchor.BottomCenter => new Vector2Int(centerX, bottom),
                _ => new Vector2Int(right, bottom),
            };
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

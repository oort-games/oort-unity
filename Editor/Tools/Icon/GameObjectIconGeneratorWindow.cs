#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using OortUnity.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace OortUnity.Editor
{
    internal sealed class GameObjectIconGeneratorWindow : EditorWindow
    {
        #region Constants

        private const string MenuPath = "Oort/Tools/GameObject Icon Generator";
        private const string WindowTitle = "GameObject Icon Generator";
        private const string DefaultFileName = "GameObjectIcon";
        private const string DefaultDirectoryName = "Icons";
        private const int PreviewTextureSize = 256;

        private static readonly List<string> ResolutionChoices = new List<string>
        {
            "64",
            "128",
            "256",
            "512",
            "Custom",
        };

        #endregion

        #region Fields

        [SerializeField]
        private GameObject _source;

        private string _fileName;
        private string _outputDirectory;
        private GameObjectIconGeneratorSettings _settings;
        private IconRenderSettings _renderSettings;

        private ObjectField _sourceField;
        private Label _sourceTypeLabel;
        private DropdownField _resolutionField;
        private IntegerField _customResolutionField;
        private EnumField _backgroundModeField;
        private ColorField _backgroundColorField;
        private SliderInt _paddingField;
        private EnumField _viewPresetField;
        private Vector3Field _rotationField;
        private EnumField _projectionField;
        private bool _are3DControlsEnabled;
        private EnumField _lightingSourceField;
        private VisualElement _lightingOptions;
        private Vector3Field _mainLightRotationField;
        private ColorField _mainLightColorField;
        private Slider _mainLightIntensityField;
        private Vector3Field _fillLightRotationField;
        private ColorField _fillLightColorField;
        private Slider _fillLightIntensityField;
        private TextField _fileNameField;
        private TextField _outputDirectoryField;
        private Image _previewImage;
        private Label _previewMessageLabel;
        private Label _statusLabel;

        private Texture2D _previewTexture;
        private Texture2D _checkerboardTexture;
        private bool _previewQueued;

        #endregion

        #region Window

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            GameObjectIconGeneratorWindow window = GetWindow<GameObjectIconGeneratorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 720f);
            window.Show();
        }

        private void OnEnable()
        {
            _settings = OortUnityUserSettings.instance.GameObjectIconGenerator;
            _renderSettings = _settings.RenderSettings;
            _renderSettings.Validate();
            _fileName = string.IsNullOrWhiteSpace(_settings.FileName)
                ? DefaultFileName
                : _settings.FileName;
            _outputDirectory = string.IsNullOrWhiteSpace(_settings.OutputDirectory)
                ? GetDefaultOutputDirectory()
                : _settings.OutputDirectory;

            if (_source == null)
            {
                _source = Selection.activeGameObject;
            }
        }

        private void OnDisable()
        {
            SaveRenderSettings();
            DestroyTexture(ref _previewTexture);
            DestroyTexture(ref _checkerboardTexture);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            VisualElementUtility.ApplyRootStyle(root);
            root.Add(VisualElementUtility.CreateHeader(WindowTitle));

            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1f;
            scrollView.Add(CreateContent());
            root.Add(scrollView);

            UpdateSourceUI();
            UpdateRenderSettingFields();
            QueuePreview();
        }

        #endregion

        #region UI

        private VisualElement CreateContent()
        {
            var content = new VisualElement();
            content.AddToClassList(OortStyleClasses.Content);
            content.Add(CreateSourceSection());
            content.Add(CreatePreviewSection());
            content.Add(CreateOutputSection());
            content.Add(CreateRenderSection());
            content.Add(CreateLightingSection());
            content.Add(CreateGenerateSection());

            return content;
        }

        private VisualElement CreateSourceSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Source"));

            var row = new VisualElement();
            row.AddToClassList(OortStyleClasses.PathRow);

            _sourceField = new ObjectField
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                value = _source,
                tooltip = "Hierarchy GameObject or Project Prefab",
            };
            _sourceField.AddToClassList(OortStyleClasses.PathField);
            _sourceField.RegisterValueChangedCallback(evt => SetSource(evt.newValue as GameObject));

            var selectionButton = new Button(() => SetSource(Selection.activeGameObject))
            {
                text = "Use Selection",
                tooltip = "Use the active Hierarchy or Project selection",
            };
            selectionButton.AddToClassList(OortStyleClasses.SmallButton);

            row.Add(_sourceField);
            row.Add(selectionButton);

            _sourceTypeLabel = new Label();
            _sourceTypeLabel.AddToClassList(OortStyleClasses.Icon.SourceType);

            section.Add(row);
            section.Add(_sourceTypeLabel);

            return section;
        }

        private VisualElement CreateLightingSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Lighting"));

            _lightingSourceField = new EnumField("Source", _renderSettings.LightingSource);
            _lightingSourceField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.LightingSource = (IconLightingSource)evt.newValue;
                UpdateLightingOptions();
                SaveAndRefreshPreview();
            });

            _lightingOptions = new VisualElement();

            _mainLightRotationField = new Vector3Field("Main Rotation")
            {
                value = _renderSettings.MainLightRotation,
            };
            _mainLightRotationField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.MainLightRotation = evt.newValue;
                SaveAndRefreshPreview();
            });

            _mainLightColorField = new ColorField("Main Color")
            {
                value = _renderSettings.MainLightColor,
                showAlpha = false,
            };
            _mainLightColorField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.MainLightColor = new Color(
                    evt.newValue.r,
                    evt.newValue.g,
                    evt.newValue.b,
                    1f
                );
                SaveAndRefreshPreview();
            });

            _mainLightIntensityField = new Slider(
                "Main Intensity",
                0f,
                IconRenderSettings.MaximumLightIntensity
            )
            {
                value = _renderSettings.MainLightIntensity,
                showInputField = true,
            };
            _mainLightIntensityField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.MainLightIntensity = evt.newValue;
                SaveAndRefreshPreview();
            });

            _fillLightRotationField = new Vector3Field("Fill Rotation")
            {
                value = _renderSettings.FillLightRotation,
            };
            _fillLightRotationField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.FillLightRotation = evt.newValue;
                SaveAndRefreshPreview();
            });

            _fillLightColorField = new ColorField("Fill Color")
            {
                value = _renderSettings.FillLightColor,
                showAlpha = false,
            };
            _fillLightColorField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.FillLightColor = new Color(
                    evt.newValue.r,
                    evt.newValue.g,
                    evt.newValue.b,
                    1f
                );
                SaveAndRefreshPreview();
            });

            _fillLightIntensityField = new Slider(
                "Fill Intensity",
                0f,
                IconRenderSettings.MaximumLightIntensity
            )
            {
                value = _renderSettings.FillLightIntensity,
                showInputField = true,
            };
            _fillLightIntensityField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.FillLightIntensity = evt.newValue;
                SaveAndRefreshPreview();
            });

            _lightingOptions.Add(_mainLightRotationField);
            _lightingOptions.Add(_mainLightColorField);
            _lightingOptions.Add(_mainLightIntensityField);
            _lightingOptions.Add(_fillLightRotationField);
            _lightingOptions.Add(_fillLightColorField);
            _lightingOptions.Add(_fillLightIntensityField);

            var resetButton = new Button(ResetRenderSettings)
            {
                text = "Reset Render Settings",
                tooltip = "Restore the default icon render settings",
            };
            resetButton.AddToClassList(OortStyleClasses.SmallButton);
            resetButton.AddToClassList(OortStyleClasses.Icon.ResetButton);

            section.Add(_lightingSourceField);
            section.Add(_lightingOptions);
            section.Add(resetButton);

            return section;
        }

        private VisualElement CreatePreviewSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Preview"));

            var previewContainer = new VisualElement();
            previewContainer.AddToClassList(OortStyleClasses.Icon.PreviewContainer);

            var checkerboardImage = new Image
            {
                image = GetCheckerboardTexture(),
                scaleMode = ScaleMode.StretchToFill,
                pickingMode = PickingMode.Ignore,
            };
            checkerboardImage.AddToClassList(OortStyleClasses.Icon.PreviewLayer);

            _previewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore,
            };
            _previewImage.AddToClassList(OortStyleClasses.Icon.PreviewLayer);

            _previewMessageLabel = new Label("Select a GameObject or Prefab to render a preview.");
            _previewMessageLabel.AddToClassList(OortStyleClasses.Icon.PreviewMessage);

            previewContainer.Add(checkerboardImage);
            previewContainer.Add(_previewImage);
            previewContainer.Add(_previewMessageLabel);
            section.Add(previewContainer);

            return section;
        }

        private VisualElement CreateOutputSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Output"));

            _fileNameField = new TextField("File Name") { value = _fileName };
            _fileNameField.RegisterValueChangedCallback(evt =>
            {
                _fileName = evt.newValue;
                _settings.FileName = _fileName;
                SaveSettings();
            });

            var directoryRow = new VisualElement();
            directoryRow.AddToClassList(OortStyleClasses.PathRow);

            _outputDirectoryField = new TextField { value = _outputDirectory };
            _outputDirectoryField.AddToClassList(OortStyleClasses.PathField);
            _outputDirectoryField.RegisterValueChangedCallback(evt => SetOutputDirectory(evt.newValue));

            var browseButton = new Button(BrowseOutputDirectory) { text = "Browse" };
            browseButton.AddToClassList(OortStyleClasses.SmallButton);

            var openButton = new Button(OpenOutputDirectory) { text = "Open" };
            openButton.AddToClassList(OortStyleClasses.SmallButton);

            var resetButton = new Button(ResetOutputDirectory) { text = "Reset" };
            resetButton.AddToClassList(OortStyleClasses.SmallButton);

            directoryRow.Add(_outputDirectoryField);
            directoryRow.Add(browseButton);
            directoryRow.Add(openButton);
            directoryRow.Add(resetButton);

            section.Add(_fileNameField);
            section.Add(new Label("Save Directory"));
            section.Add(directoryRow);

            return section;
        }

        private VisualElement CreateRenderSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);
            section.Add(new Label("Render Settings"));

            _resolutionField = new DropdownField("Resolution")
            {
                choices = ResolutionChoices,
            };
            _resolutionField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != "Custom" && int.TryParse(evt.newValue, out int resolution))
                {
                    _renderSettings.Resolution = resolution;
                }

                UpdateResolutionField();
                SaveAndRefreshPreview();
            });

            _customResolutionField = new IntegerField("Custom Resolution");
            _customResolutionField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.Resolution = Mathf.Clamp(evt.newValue, 16, 4096);
                _customResolutionField.SetValueWithoutNotify(_renderSettings.Resolution);
                SaveAndRefreshPreview();
            });

            _backgroundModeField = new EnumField("Background", _renderSettings.BackgroundMode);
            _backgroundModeField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.BackgroundMode = (IconBackgroundMode)evt.newValue;
                UpdateBackgroundField();
                SaveAndRefreshPreview();
            });

            _backgroundColorField = new ColorField("Background Color")
            {
                value = _renderSettings.BackgroundColor,
                showAlpha = false,
            };
            _backgroundColorField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.BackgroundColor = new Color(evt.newValue.r, evt.newValue.g, evt.newValue.b, 1f);
                SaveAndRefreshPreview();
            });

            _paddingField = new SliderInt("Padding (%)", 0, 45)
            {
                value = Mathf.RoundToInt(_renderSettings.Padding * 100f),
                showInputField = true,
            };
            _paddingField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.Padding = evt.newValue / 100f;
                SaveAndRefreshPreview();
            });

            _viewPresetField = new EnumField("View", _renderSettings.ViewPreset);
            _viewPresetField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.ViewPreset = (IconViewPreset)evt.newValue;

                if (_renderSettings.ViewPreset != IconViewPreset.Custom)
                {
                    _renderSettings.Rotation = IconRenderSettings.GetPresetRotation(_renderSettings.ViewPreset);
                    _rotationField.SetValueWithoutNotify(_renderSettings.Rotation);
                }

                SaveAndRefreshPreview();
            });

            _rotationField = new Vector3Field("Rotation") { value = _renderSettings.Rotation };
            _rotationField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.Rotation = evt.newValue;
                _renderSettings.ViewPreset = IconViewPreset.Custom;
                _viewPresetField.SetValueWithoutNotify(IconViewPreset.Custom);
                SaveAndRefreshPreview();
            });

            _projectionField = new EnumField("Projection", _renderSettings.Projection);
            _projectionField.RegisterValueChangedCallback(evt =>
            {
                _renderSettings.Projection = (IconProjection)evt.newValue;
                SaveAndRefreshPreview();
            });

            section.Add(_resolutionField);
            section.Add(_customResolutionField);
            section.Add(_backgroundModeField);
            section.Add(_backgroundColorField);
            section.Add(_paddingField);
            section.Add(_viewPresetField);
            section.Add(_rotationField);
            section.Add(_projectionField);

            return section;
        }

        private VisualElement CreateGenerateSection()
        {
            var section = new VisualElement();
            section.AddToClassList(OortStyleClasses.Section);

            var generateButton = new Button(GenerateIcon) { text = "Generate PNG Icon" };
            generateButton.AddToClassList(OortStyleClasses.PrimaryButton);

            _statusLabel = new Label();
            _statusLabel.AddToClassList(OortStyleClasses.Icon.Status);

            section.Add(generateButton);
            section.Add(_statusLabel);

            return section;
        }

        #endregion

        #region Source

        private void SetSource(GameObject source)
        {
            string previousSourceName = _source != null ? _source.name : null;
            bool useSourceName = string.IsNullOrWhiteSpace(_fileName)
                || string.Equals(_fileName, DefaultFileName, StringComparison.Ordinal)
                || string.Equals(_fileName, previousSourceName, StringComparison.Ordinal);

            _source = source;
            _sourceField?.SetValueWithoutNotify(source);

            if (source != null && useSourceName)
            {
                _fileName = source.name;
                _fileNameField?.SetValueWithoutNotify(_fileName);
                _settings.FileName = _fileName;
                SaveSettings();
            }

            UpdateSourceUI();
            QueuePreview();
        }

        private void UpdateSourceUI()
        {
            if (_sourceTypeLabel == null)
            {
                return;
            }

            if (_source == null)
            {
                _sourceTypeLabel.text = "No source selected";
                Set3DControlsEnabled(false);
                return;
            }

            bool isUI = GameObjectBoundsUtility.IsUIObject(_source);
            bool is2D = !isUI && GameObjectBoundsUtility.Is2DObject(_source);
            string sourceKind = isUI ? "UI" : is2D ? "2D" : "3D";
            string sourceLocation = EditorUtility.IsPersistent(_source) ? "Prefab Asset" : "Hierarchy";
            _sourceTypeLabel.text = $"Detected: {sourceKind} · {sourceLocation}";
            Set3DControlsEnabled(!isUI && !is2D);
        }

        private void Set3DControlsEnabled(bool enabled)
        {
            _are3DControlsEnabled = enabled;
            _viewPresetField?.SetEnabled(enabled);
            _rotationField?.SetEnabled(enabled);
            _projectionField?.SetEnabled(enabled);
            _lightingSourceField?.SetEnabled(enabled);
            UpdateLightingOptions();
        }

        #endregion

        #region Render Settings

        private void UpdateRenderSettingFields()
        {
            if (_renderSettings == null || _resolutionField == null)
            {
                return;
            }

            _renderSettings.Validate();
            string resolutionValue = ResolutionChoices.Contains(_renderSettings.Resolution.ToString())
                ? _renderSettings.Resolution.ToString()
                : "Custom";
            _resolutionField.SetValueWithoutNotify(resolutionValue);
            _customResolutionField.SetValueWithoutNotify(_renderSettings.Resolution);
            _backgroundModeField.SetValueWithoutNotify(_renderSettings.BackgroundMode);
            _backgroundColorField.SetValueWithoutNotify(_renderSettings.BackgroundColor);
            _paddingField.SetValueWithoutNotify(Mathf.RoundToInt(_renderSettings.Padding * 100f));
            _viewPresetField.SetValueWithoutNotify(_renderSettings.ViewPreset);
            _rotationField.SetValueWithoutNotify(_renderSettings.Rotation);
            _projectionField.SetValueWithoutNotify(_renderSettings.Projection);
            _lightingSourceField.SetValueWithoutNotify(_renderSettings.LightingSource);
            _mainLightRotationField.SetValueWithoutNotify(_renderSettings.MainLightRotation);
            _mainLightColorField.SetValueWithoutNotify(_renderSettings.MainLightColor);
            _mainLightIntensityField.SetValueWithoutNotify(_renderSettings.MainLightIntensity);
            _fillLightRotationField.SetValueWithoutNotify(_renderSettings.FillLightRotation);
            _fillLightColorField.SetValueWithoutNotify(_renderSettings.FillLightColor);
            _fillLightIntensityField.SetValueWithoutNotify(_renderSettings.FillLightIntensity);

            UpdateResolutionField();
            UpdateBackgroundField();
            UpdateLightingOptions();
        }

        private void UpdateResolutionField()
        {
            if (_resolutionField == null || _customResolutionField == null)
            {
                return;
            }

            _customResolutionField.style.display = _resolutionField.value == "Custom"
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void UpdateBackgroundField()
        {
            if (_backgroundColorField != null)
            {
                _backgroundColorField.SetEnabled(
                    _renderSettings.BackgroundMode == IconBackgroundMode.SolidColor
                );
            }
        }

        private void UpdateLightingOptions()
        {
            _lightingOptions?.SetEnabled(
                _are3DControlsEnabled
                && _renderSettings.LightingSource == IconLightingSource.Studio
            );
        }

        private void SaveAndRefreshPreview()
        {
            SaveRenderSettings();
            QueuePreview();
        }

        private void SaveRenderSettings()
        {
            if (_renderSettings != null)
            {
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            OortUnityUserSettings.instance.SaveGameObjectIconGeneratorSettings();
        }

        private void ResetRenderSettings()
        {
            IconRenderSettings defaults = new IconRenderSettings();
            _renderSettings.Resolution = defaults.Resolution;
            _renderSettings.BackgroundMode = defaults.BackgroundMode;
            _renderSettings.BackgroundColor = defaults.BackgroundColor;
            _renderSettings.Padding = defaults.Padding;
            _renderSettings.ViewPreset = defaults.ViewPreset;
            _renderSettings.Rotation = defaults.Rotation;
            _renderSettings.Projection = defaults.Projection;
            _renderSettings.LightingSource = defaults.LightingSource;
            _renderSettings.MainLightRotation = defaults.MainLightRotation;
            _renderSettings.MainLightColor = defaults.MainLightColor;
            _renderSettings.MainLightIntensity = defaults.MainLightIntensity;
            _renderSettings.FillLightRotation = defaults.FillLightRotation;
            _renderSettings.FillLightColor = defaults.FillLightColor;
            _renderSettings.FillLightIntensity = defaults.FillLightIntensity;

            UpdateRenderSettingFields();
            SaveAndRefreshPreview();
        }

        #endregion

        #region Preview

        private void QueuePreview()
        {
            if (_previewQueued || rootVisualElement == null)
            {
                return;
            }

            _previewQueued = true;
            rootVisualElement.schedule.Execute(() =>
            {
                _previewQueued = false;
                RefreshPreview();
            }).StartingIn(75);
        }

        private void RefreshPreview()
        {
            DestroyTexture(ref _previewTexture);

            if (_previewImage == null || _previewMessageLabel == null)
            {
                return;
            }

            _previewImage.image = null;

            if (_source == null)
            {
                _previewMessageLabel.text = "Select a GameObject or Prefab to render a preview.";
                _previewMessageLabel.style.display = DisplayStyle.Flex;
                return;
            }

            try
            {
                IconRenderSettings previewSettings = _renderSettings.Clone();
                previewSettings.Resolution = PreviewTextureSize;
                _previewTexture = GameObjectIconRenderer.Render(_source, previewSettings);
                _previewTexture.hideFlags = HideFlags.HideAndDontSave;
                _previewImage.image = _previewTexture;
                _previewMessageLabel.style.display = DisplayStyle.None;
                SetStatus(string.Empty);
            }
            catch (Exception exception)
            {
                _previewMessageLabel.text = exception.Message;
                _previewMessageLabel.style.display = DisplayStyle.Flex;
                SetStatus($"Preview failed: {exception.Message}");
            }
        }

        #endregion

        #region Generation

        private void GenerateIcon()
        {
            if (!TryValidateOutput(out string baseName))
            {
                return;
            }

            try
            {
                FileUtility.CreateDirectory(_outputDirectory);
                string filePath = PathUtility.GetUniqueFilePath(_outputDirectory, baseName, ".png");
                byte[] pngBytes = GameObjectIconRenderer.RenderToPng(_source, _renderSettings);
                FileUtility.WriteAllBytes(filePath, pngBytes);

                RefreshAssetDatabaseIfNeeded(filePath);
                SetStatus($"Saved: {filePath}");
                Debug.Log($"<color=cyan>[GameObject Icon Generator]</color> {filePath}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetStatus($"Failed: {exception.Message}");
                EditorUtility.DisplayDialog(WindowTitle, $"Failed to generate the icon.\n\n{exception.Message}", "OK");
            }
        }

        private bool TryValidateOutput(out string baseName)
        {
            baseName = null;

            if (_source == null)
            {
                EditorUtility.DisplayDialog(WindowTitle, "Select a GameObject or Prefab.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                EditorUtility.DisplayDialog(WindowTitle, "Enter a save directory.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_fileName))
            {
                EditorUtility.DisplayDialog(WindowTitle, "Enter a file name.", "OK");
                return false;
            }

            baseName = Path.GetFileNameWithoutExtension(_fileName.Trim());
            baseName = PathUtility.SanitizeFileName(baseName);

            if (string.IsNullOrWhiteSpace(baseName))
            {
                EditorUtility.DisplayDialog(WindowTitle, "Enter a valid file name.", "OK");
                return false;
            }

            return true;
        }

        #endregion

        #region Directory

        private string GetDefaultOutputDirectory()
        {
            return EditorDirectoryUtility.GetDefaultOutputDirectory(DefaultDirectoryName);
        }

        private void SetOutputDirectory(string directory)
        {
            _outputDirectory = PathUtility.NormalizePath(directory);
            _settings.OutputDirectory = _outputDirectory;
            SaveSettings();
        }

        private void BrowseOutputDirectory()
        {
            if (!EditorDirectoryUtility.TryBrowseDirectory(
                "Select Icon Directory",
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

        private void ResetOutputDirectory()
        {
            _settings.OutputDirectory = string.Empty;
            SaveSettings();
            _outputDirectory = GetDefaultOutputDirectory();
            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        #endregion

        #region Helpers

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
        }

        private static void RefreshAssetDatabaseIfNeeded(string filePath)
        {
            string assetsPath = Path.GetFullPath(Application.dataPath);
            string fullFilePath = Path.GetFullPath(filePath);

            if (PathUtility.IsSamePath(fullFilePath, assetsPath)
                || PathUtility.IsSubPathOf(fullFilePath, assetsPath))
            {
                AssetDatabase.Refresh();
            }
        }

        private Texture2D GetCheckerboardTexture()
        {
            if (_checkerboardTexture != null)
            {
                return _checkerboardTexture;
            }

            _checkerboardTexture = new Texture2D(
                PreviewTextureSize,
                PreviewTextureSize,
                TextureFormat.RGB24,
                false,
                true
            )
            {
                name = "Icon Preview Checkerboard",
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave,
            };

            var pixels = new Color32[PreviewTextureSize * PreviewTextureSize];
            var light = new Color32(92, 92, 92, 255);
            var dark = new Color32(66, 66, 66, 255);
            const int tileSize = 16;

            for (int y = 0; y < PreviewTextureSize; y++)
            {
                for (int x = 0; x < PreviewTextureSize; x++)
                {
                    bool useLight = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                    pixels[y * PreviewTextureSize + x] = useLight ? light : dark;
                }
            }

            _checkerboardTexture.SetPixels32(pixels);
            _checkerboardTexture.Apply(false, true);

            return _checkerboardTexture;
        }

        private static void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }

            texture = null;
        }

        #endregion
    }
}

#endif

#if UNITY_EDITOR

using System;
using System.IO;
using OortUnity.Utilities;
using UnityEditor;
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

        #endregion

        #region Fields

        [SerializeField]
        private string _fileName = DefaultFileName;

        private string _outputDirectory;

        private TextField _fileNameField;
        private TextField _outputDirectoryField;

        #endregion

        #region Window

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            GameViewScreenshotWindow window = GetWindow<GameViewScreenshotWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(420f, 250f);
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

            var openButton = new Button(RevealOutputDirectory) { text = "Open" };
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
            var button = new Button(CaptureScreenshot) { text = "Capture Screenshot" };

            button.AddToClassList(OortStyleClasses.PrimaryButton);

            return button;
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

        private void RevealOutputDirectory()
        {
            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                Debug.LogWarning("[Game View Screenshot] Save directory is empty.");

                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);
            EditorUtility.RevealInFinder(_outputDirectory);
        }

        #endregion

        #region Screenshot

        private void CaptureScreenshot()
        {
            if (!ValidateSettings())
            {
                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);

            string filePath = CreateScreenshotPath();

            ScreenCapture.CaptureScreenshot(filePath);

            Debug.Log($"<color=cyan>[Game View Screenshot]</color> {filePath}");
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

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

        private const string ContentClass = "oort-content";
        private const string SectionClass = "oort-section";
        private const string PathRowClass = "oort-path-row";
        private const string PathFieldClass = "oort-path-field";
        private const string SmallButtonClass = "oort-small-button";
        private const string PrimaryButtonClass = "oort-primary-button";

        #endregion

        #region Fields

        [SerializeField]
        private string _fileName = DefaultFileName;

        private string _outputDirectory;

        private TextField _fileNameField;
        private TextField _outputDirectoryField;

        #endregion

        #region Window

        /// <summary>
        /// Game View Screenshot 창을 엽니다.
        /// </summary>
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

        /// <summary>
        /// 스크린샷 설정 UI를 생성합니다.
        /// </summary>
        /// <returns>생성된 콘텐츠 VisualElement를 반환합니다.</returns>
        private VisualElement CreateContent()
        {
            var content = new VisualElement();
            content.AddToClassList(ContentClass);

            content.Add(CreateOutputDirectorySection());
            content.Add(CreateFileNameSection());
            content.Add(CreateCaptureButton());

            return content;
        }

        /// <summary>
        /// 저장 폴더 설정 영역을 생성합니다.
        /// </summary>
        /// <returns>생성된 저장 폴더 설정 VisualElement를 반환합니다.</returns>
        private VisualElement CreateOutputDirectorySection()
        {
            var section = new VisualElement();
            section.AddToClassList(SectionClass);

            section.Add(new Label("Save Directory"));

            var row = new VisualElement();
            row.AddToClassList(PathRowClass);

            _outputDirectoryField = new TextField
            {
                value = _outputDirectory
            };

            _outputDirectoryField.AddToClassList(PathFieldClass);
            _outputDirectoryField.RegisterValueChangedCallback(
                evt => SetOutputDirectory(evt.newValue));

            var browseButton = new Button(BrowseOutputDirectory)
            {
                text = "Browse"
            };
            browseButton.AddToClassList(SmallButtonClass);

            var openButton = new Button(RevealOutputDirectory)
            {
                text = "Open"
            };
            openButton.AddToClassList(SmallButtonClass);

            var resetButton = new Button(ResetOutputDirectory)
            {
                text = "Reset"
            };
            resetButton.AddToClassList(SmallButtonClass);

            row.Add(_outputDirectoryField);
            row.Add(browseButton);
            row.Add(openButton);
            row.Add(resetButton);

            section.Add(row);

            return section;
        }

        /// <summary>
        /// 파일명 입력 영역을 생성합니다.
        /// </summary>
        /// <returns>생성된 파일명 입력 VisualElement를 반환합니다.</returns>
        private VisualElement CreateFileNameSection()
        {
            var section = new VisualElement();
            section.AddToClassList(SectionClass);

            _fileNameField = new TextField("File Name")
            {
                value = _fileName
            };

            _fileNameField.RegisterValueChangedCallback(
                evt => _fileName = evt.newValue);

            section.Add(_fileNameField);

            return section;
        }

        /// <summary>
        /// 스크린샷 캡처 버튼을 생성합니다.
        /// </summary>
        /// <returns>생성된 캡처 Button을 반환합니다.</returns>
        private Button CreateCaptureButton()
        {
            var button = new Button(CaptureScreenshot)
            {
                text = "Capture Screenshot"
            };

            button.AddToClassList(PrimaryButtonClass);

            return button;
        }

        #endregion

        #region Directory

        /// <summary>
        /// 기본 스크린샷 저장 경로를 반환합니다.
        /// </summary>
        /// <returns>기본 저장 경로를 반환합니다.</returns>
        private string GetDefaultOutputDirectory()
        {
            string documentsPath = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);

            return PathUtility.NormalizePath(
                Path.Combine(
                    documentsPath,
                    "OortUnity",
                    DefaultDirectoryName));
        }

        /// <summary>
        /// EditorPrefs에 저장된 출력 경로를 불러옵니다.
        /// 저장된 값이 없으면 기본 경로를 사용합니다.
        /// </summary>
        private void LoadOutputDirectory()
        {
            string savedDirectory =
                OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory;

            _outputDirectory = string.IsNullOrWhiteSpace(savedDirectory)
                ? GetDefaultOutputDirectory()
                : savedDirectory;
        }

        /// <summary>
        /// 출력 경로를 변경하고 EditorPrefs에 저장합니다.
        /// </summary>
        /// <param name="directory">변경할 출력 경로입니다.</param>
        private void SetOutputDirectory(string directory)
        {
            _outputDirectory = PathUtility.NormalizePath(directory);

            OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory =
                _outputDirectory;
        }

        /// <summary>
        /// 출력 경로를 기본 경로로 초기화합니다.
        /// </summary>
        private void ResetOutputDirectory()
        {
            OortUnityUserSettings.instance.GameViewScreenshotOutputDirectory =
                string.Empty;

            _outputDirectory = GetDefaultOutputDirectory();
            _outputDirectoryField?.SetValueWithoutNotify(_outputDirectory);
        }

        /// <summary>
        /// 스크린샷을 저장할 폴더를 선택합니다.
        /// </summary>
        private void BrowseOutputDirectory()
        {
            string selectedPath = EditorUtility.OpenFolderPanel(
                "Select Screenshot Directory",
                _outputDirectory,
                string.Empty);

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            SetOutputDirectory(selectedPath);

            _outputDirectoryField?.SetValueWithoutNotify(
                _outputDirectory);
        }

        /// <summary>
        /// 현재 출력 경로를 파일 탐색기에서 엽니다.
        /// 폴더가 존재하지 않으면 생성합니다.
        /// </summary>
        private void RevealOutputDirectory()
        {
            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                Debug.LogWarning(
                    "[Game View Screenshot] Save directory is empty.");

                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);
            EditorUtility.RevealInFinder(_outputDirectory);
        }

        #endregion

        #region Screenshot

        /// <summary>
        /// 현재 Game View를 PNG 파일로 캡처합니다.
        /// </summary>
        private void CaptureScreenshot()
        {
            if (!ValidateSettings())
            {
                return;
            }

            FileUtility.CreateDirectory(_outputDirectory);

            string filePath = CreateScreenshotPath();

            ScreenCapture.CaptureScreenshot(filePath);

            Debug.Log(
                $"<color=cyan>[Game View Screenshot]</color> {filePath}");
        }

        /// <summary>
        /// 중복되지 않는 스크린샷 파일 경로를 생성합니다.
        /// </summary>
        /// <returns>스크린샷 파일 경로를 반환합니다.</returns>
        private string CreateScreenshotPath()
        {
            string safeFileName = PathUtility.SanitizeFileName(_fileName);

            return PathUtility.GetUniqueFilePath(
                _outputDirectory,
                safeFileName,
                "png");
        }

        /// <summary>
        /// 스크린샷 설정값이 유효한지 확인합니다.
        /// </summary>
        /// <returns>설정값이 유효하면 true를 반환합니다.</returns>
        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_outputDirectory))
            {
                Debug.LogError(
                    "[Game View Screenshot] Save directory is empty.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(_fileName))
            {
                Debug.LogError(
                    "[Game View Screenshot] File name is empty.");

                return false;
            }

            string safeFileName = PathUtility.SanitizeFileName(_fileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                Debug.LogError(
                    "[Game View Screenshot] File name is invalid.");

                return false;
            }

            return true;
        }

        #endregion
    }
}

#endif
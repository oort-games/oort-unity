#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OortUnity.Editor
{
    public static class VisualElementUtility
    {
        #region Constants

        private const string StyleSheetPath = "Packages/com.oortgamestudio.oortunity/Editor/UI/Styles/OortStyles.uss";
        private const string DefaultIconPath = "Packages/com.oortgamestudio.oortunity/Editor/UI/Icons/icon_oort.png";

        #endregion

        #region Cache

        private static StyleSheet _styleSheet;
        private static Texture2D _defaultIcon;

        #endregion

        #region Assets

        /// <summary>
        /// OortUnity Editor UI에서 사용하는 공통 스타일 시트를 반환합니다.
        /// </summary>
        /// <returns>공통 스타일 시트를 반환합니다. 로드에 실패하면 null을 반환합니다.</returns>
        public static StyleSheet StyleSheet
        {
            get
            {
                if (_styleSheet != null)
                {
                    return _styleSheet;
                }

                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);

                if (_styleSheet == null)
                {
                    Debug.LogWarning($"Failed to load OortUnity style sheet: {StyleSheetPath}");
                }

                return _styleSheet;
            }
        }

        /// <summary>
        /// OortUnity Editor UI에서 사용하는 기본 아이콘을 반환합니다.
        /// </summary>
        /// <returns>기본 아이콘을 반환합니다. 로드에 실패하면 null을 반환합니다.</returns>
        public static Texture2D DefaultIcon
        {
            get
            {
                if (_defaultIcon != null)
                {
                    return _defaultIcon;
                }

                _defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultIconPath);

                if (_defaultIcon == null)
                {
                    Debug.LogWarning($"Failed to load OortUnity icon: {DefaultIconPath}");
                }

                return _defaultIcon;
            }
        }

        #endregion

        #region Style

        /// <summary>
        /// 지정된 VisualElement에 OortUnity 공통 스타일 시트를 적용합니다.
        /// </summary>
        /// <param name="element">스타일 시트를 적용할 VisualElement입니다.</param>
        public static void ApplyStyleSheet(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            StyleSheet styleSheet = StyleSheet;

            if (styleSheet != null && !element.styleSheets.Contains(styleSheet))
            {
                element.styleSheets.Add(styleSheet);
            }
        }

        /// <summary>
        /// 지정된 VisualElement에 OortUnity 루트 스타일을 적용합니다.
        /// 공통 스타일 시트도 함께 적용됩니다.
        /// </summary>
        /// <param name="root">루트 스타일을 적용할 VisualElement입니다.</param>
        public static void ApplyRootStyle(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            ApplyStyleSheet(root);
            root.AddToClassList(OortStyleClasses.Root);
        }

        #endregion

        #region Header

        /// <summary>
        /// OortUnity 기본 아이콘과 지정된 제목을 표시하는 공통 헤더를 생성합니다.
        /// </summary>
        /// <param name="title">헤더에 표시할 제목입니다.</param>
        /// <returns>생성된 헤더 VisualElement를 반환합니다.</returns>
        public static VisualElement CreateHeader(string title)
        {
            return CreateHeader(title, DefaultIcon);
        }

        /// <summary>
        /// 지정된 아이콘과 제목을 표시하는 OortUnity 공통 헤더를 생성합니다.
        /// </summary>
        /// <param name="title">헤더에 표시할 제목입니다.</param>
        /// <param name="icon">헤더에 표시할 아이콘입니다. null이면 아이콘을 표시하지 않습니다.</param>
        /// <returns>생성된 헤더 VisualElement를 반환합니다.</returns>
        public static VisualElement CreateHeader(string title, Texture2D icon)
        {
            var header = new VisualElement();
            header.AddToClassList(OortStyleClasses.Header);

            if (icon != null)
            {
                var iconImage = new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore,
                };

                iconImage.AddToClassList(OortStyleClasses.HeaderIcon);

                header.Add(iconImage);
            }

            var titleLabel = new Label(title);
            titleLabel.AddToClassList(OortStyleClasses.HeaderTitle);

            header.Add(titleLabel);

            return header;
        }

        #endregion
    }
}

#endif

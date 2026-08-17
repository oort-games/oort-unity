#if UNITY_EDITOR

namespace OortUnity.Editor
{
    public static class OortStyleClasses
    {
        public const string Root = "oort-root";

        public const string Header = "oort-header";
        public const string HeaderIcon = "oort-header-icon";
        public const string HeaderTitle = "oort-header-title";

        public const string Content = "oort-content";
        public const string Section = "oort-section";

        public const string PathRow = "oort-path-row";
        public const string PathField = "oort-path-field";

        public const string SmallButton = "oort-small-button";
        public const string PrimaryButton = "oort-primary-button";

        public static class Screenshot
        {
            public const string WatermarkOptions = "oort-screenshot-watermark-options";
            public const string WatermarkPreview = "oort-screenshot-watermark-preview";
            public const string WatermarkNotice = "oort-screenshot-watermark-notice";
            public const string WatermarkResetButton = "oort-screenshot-watermark-reset-button";
        }

        public static class PlayerPrefs
        {
            public const string Toolbar = "oort-prefs-toolbar";
            public const string Search = "oort-prefs-search";
            public const string ListHeader = "oort-prefs-list-header";
            public const string SortButton = "oort-prefs-sort-button";
            public const string Row = "oort-prefs-row";
            public const string Key = "oort-prefs-key";
            public const string Type = "oort-prefs-type";
            public const string Value = "oort-prefs-value";
            public const string ActionButton = "oort-prefs-action-button";
        }
    }
}

#endif

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OortUnity.Editor
{
    public class PlayerPrefsManagerWindow : EditorWindow
    {
        #region Constants

        private const string MenuPath = "Oort/Tools/PlayerPrefs Manager";
        private const string WindowTitle = "PlayerPrefs Manager";
        private const string HeaderTitle = "PlayerPrefs Manager";

        private const string ContentClass = "oort-content";
        private const string ToolbarClass = "oort-prefs-toolbar";
        private const string SearchFieldClass = "oort-prefs-search";
        private const string ListHeaderClass = "oort-prefs-list-header";
        private const string RowClass = "oort-prefs-row";
        private const string KeyClass = "oort-prefs-key";
        private const string TypeClass = "oort-prefs-type";
        private const string ValueClass = "oort-prefs-value";
        private const string SmallButtonClass = "oort-small-button";
        private const string ActionButtonClass = "oort-prefs-action-button";

        #endregion

        #region Fields

        private readonly List<PlayerPrefsEntry> _entries = new();
        private readonly List<PlayerPrefsEntry> _filteredEntries = new();

        private TextField _searchField;
        private ListView _listView;
        private Label _countLabel;

        #endregion

        #region Window

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            PlayerPrefsManagerWindow window =
                GetWindow<PlayerPrefsManagerWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(600f, 350f);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            root.Clear();
            VisualElementUtility.ApplyRootStyle(root);

            root.Add(VisualElementUtility.CreateHeader(HeaderTitle));
            root.Add(CreateContent());

            Refresh();
        }

        #endregion

        #region UI

        private VisualElement CreateContent()
        {
            var content = new VisualElement();
            content.AddToClassList(ContentClass);

            content.Add(CreateToolbar());
            content.Add(CreateListHeader());
            content.Add(CreateList());

            return content;
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList(ToolbarClass);

            _searchField = new TextField();
            _searchField.AddToClassList(SearchFieldClass);
            _searchField.RegisterValueChangedCallback(_ => ApplyFilter());

            _countLabel = new Label();

            var refreshButton = new Button(Refresh)
            {
                text = "Refresh"
            };
            refreshButton.AddToClassList(SmallButtonClass);

            toolbar.Add(_searchField);
            toolbar.Add(_countLabel);
            toolbar.Add(refreshButton);

            return toolbar;
        }

        private VisualElement CreateListHeader()
        {
            var header = new VisualElement();
            header.AddToClassList(ListHeaderClass);

            Label keyLabel = new Label("Key");
            keyLabel.AddToClassList(KeyClass);

            Label typeLabel = new Label("Type");
            typeLabel.AddToClassList(TypeClass);

            Label valueLabel = new Label("Value");
            valueLabel.AddToClassList(ValueClass);

            header.Add(keyLabel);
            header.Add(typeLabel);
            header.Add(valueLabel);

            return header;
        }

        private ListView CreateList()
        {
            _listView = new ListView
            {
                fixedItemHeight = 30f,
                virtualizationMethod =
                    CollectionVirtualizationMethod.FixedHeight,

                makeItem = () =>
                    new PlayerPrefsRow(
                        SaveEntry,
                        DeleteEntry),

                bindItem = (element, index) =>
                {
                    var row = (PlayerPrefsRow)element;
                    row.Bind(_filteredEntries[index]);
                }
            };

            _listView.style.flexGrow = 1f;

            return _listView;
        }

        #endregion

        #region Data

        private void Refresh()
        {
            _entries.Clear();
            _entries.AddRange(PlayerPrefsStorage.LoadAll());

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filteredEntries.Clear();

            string search = _searchField?.value;

            foreach (PlayerPrefsEntry entry in _entries)
            {
                if (string.IsNullOrWhiteSpace(search) ||
                    entry.Key.IndexOf(
                        search,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredEntries.Add(entry);
                }
            }

            if (_listView != null)
            {
                _listView.itemsSource = _filteredEntries;
                _listView.Rebuild();
            }

            if (_countLabel != null)
            {
                _countLabel.text = $"{_filteredEntries.Count} items";
            }
        }

        #endregion

        #region Actions

        private void SaveEntry(
            PlayerPrefsEntry entry,
            string value)
        {
            if (!PlayerPrefsStorage.TrySetValue(
                entry,
                value,
                out string error))
            {
                EditorUtility.DisplayDialog(
                    "PlayerPrefs Manager",
                    error,
                    "OK");

                return;
            }

            Refresh();
        }

        private void DeleteEntry(PlayerPrefsEntry entry)
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete PlayerPrefs",
                $"Delete '{entry.Key}'?",
                "Delete",
                "Cancel");

            if (!confirmed) return;

            PlayerPrefsStorage.Delete(entry.Key);
            Refresh();
        }

        #endregion

        #region Row

        private sealed class PlayerPrefsRow : VisualElement
        {
            private readonly Label _keyLabel;
            private readonly Label _typeLabel;
            private readonly TextField _valueField;

            private PlayerPrefsEntry _entry;

            public PlayerPrefsRow(
                Action<PlayerPrefsEntry, string> saveAction,
                Action<PlayerPrefsEntry> deleteAction)
            {
                AddToClassList(RowClass);

                _keyLabel = new Label();
                _keyLabel.AddToClassList(KeyClass);

                _typeLabel = new Label();
                _typeLabel.AddToClassList(TypeClass);

                _valueField = new TextField();
                _valueField.AddToClassList(ValueClass);

                var saveButton = new Button(
                    () => saveAction?.Invoke(
                        _entry,
                        _valueField.value))
                {
                    text = "Save"
                };
                saveButton.AddToClassList(SmallButtonClass);
                saveButton.AddToClassList(ActionButtonClass);

                var deleteButton = new Button(
                    () => deleteAction?.Invoke(_entry))
                {
                    text = "Delete"
                };
                deleteButton.AddToClassList(SmallButtonClass);
                deleteButton.AddToClassList(ActionButtonClass);

                Add(_keyLabel);
                Add(_typeLabel);
                Add(_valueField);
                Add(saveButton);
                Add(deleteButton);
            }

            public void Bind(PlayerPrefsEntry entry)
            {
                _entry = entry;

                _keyLabel.text = entry.Key;
                _typeLabel.text = entry.Type.ToString();

                _valueField.SetValueWithoutNotify(entry.Value);
            }
        }

        #endregion
    }
}

#endif
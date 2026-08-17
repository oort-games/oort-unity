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
        private enum SortColumn
        {
            Key,
            Type,
        }

        #region Constants

        private const string MenuPath = "Oort/Tools/PlayerPrefs Manager";
        private const string WindowTitle = "PlayerPrefs Manager";
        private const string HeaderTitle = "PlayerPrefs Manager";

        #endregion

        #region Fields

        private readonly List<PlayerPrefsEntry> _entries = new();
        private readonly List<PlayerPrefsEntry> _filteredEntries = new();

        private TextField _searchField;
        private ListView _listView;
        private Label _countLabel;
        private Button _keySortButton;
        private Button _typeSortButton;

        private SortColumn _sortColumn = SortColumn.Key;
        private bool _sortAscending = true;

        #endregion

        #region Window

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            PlayerPrefsManagerWindow window = GetWindow<PlayerPrefsManagerWindow>();

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
            content.AddToClassList(OortStyleClasses.Content);

            content.Add(CreateToolbar());
            content.Add(CreateListHeader());
            content.Add(CreateList());

            return content;
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList(OortStyleClasses.PlayerPrefs.Toolbar);

            _searchField = new TextField("Search") { tooltip = "Search by PlayerPrefs key" };
            _searchField.AddToClassList(OortStyleClasses.PlayerPrefs.Search);
            _searchField.RegisterValueChangedCallback(_ => ApplyFilter());

            _countLabel = new Label();

            var refreshButton = new Button(Refresh) { text = "Refresh" };
            refreshButton.AddToClassList(OortStyleClasses.SmallButton);

            var deleteAllButton = new Button(DeleteAllEntries) { text = "Delete All" };
            deleteAllButton.AddToClassList(OortStyleClasses.SmallButton);

            toolbar.Add(_searchField);
            toolbar.Add(_countLabel);
            toolbar.Add(refreshButton);
            toolbar.Add(deleteAllButton);

            return toolbar;
        }

        private VisualElement CreateListHeader()
        {
            var header = new VisualElement();
            header.AddToClassList(OortStyleClasses.PlayerPrefs.ListHeader);

            _keySortButton = new Button(() => ToggleSort(SortColumn.Key)) { tooltip = "Sort by key" };
            _keySortButton.AddToClassList(OortStyleClasses.PlayerPrefs.Key);
            _keySortButton.AddToClassList(OortStyleClasses.PlayerPrefs.SortButton);

            _typeSortButton = new Button(() => ToggleSort(SortColumn.Type)) { tooltip = "Sort by type" };
            _typeSortButton.AddToClassList(OortStyleClasses.PlayerPrefs.Type);
            _typeSortButton.AddToClassList(OortStyleClasses.PlayerPrefs.SortButton);

            Label valueLabel = new Label("Value");
            valueLabel.AddToClassList(OortStyleClasses.PlayerPrefs.Value);

            UpdateSortButtons();

            header.Add(_keySortButton);
            header.Add(_typeSortButton);
            header.Add(valueLabel);

            return header;
        }

        private ListView CreateList()
        {
            _listView = new ListView
            {
                fixedItemHeight = 30f,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,

                makeItem = () => new PlayerPrefsRow(SaveEntry, DeleteEntry),

                bindItem = (element, index) =>
                {
                    var row = (PlayerPrefsRow)element;
                    row.Bind(_filteredEntries[index]);
                },
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
                if (
                    string.IsNullOrWhiteSpace(search)
                    || entry.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                )
                {
                    _filteredEntries.Add(entry);
                }
            }

            SortFilteredEntries();
            UpdateSortButtons();

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

        private void ToggleSort(SortColumn column)
        {
            if (_sortColumn == column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortColumn = column;
                _sortAscending = true;
            }

            ApplyFilter();
        }

        private void SortFilteredEntries()
        {
            _filteredEntries.Sort(CompareEntries);
        }

        private int CompareEntries(PlayerPrefsEntry a, PlayerPrefsEntry b)
        {
            int comparison;

            if (_sortColumn == SortColumn.Type)
            {
                comparison = string.Compare(a.Type.ToString(), b.Type.ToString(), StringComparison.OrdinalIgnoreCase);

                if (comparison == 0)
                {
                    comparison = string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                comparison = string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
            }

            return _sortAscending ? comparison : -comparison;
        }

        private void UpdateSortButtons()
        {
            if (_keySortButton == null || _typeSortButton == null)
            {
                return;
            }

            string indicator = _sortAscending ? "\u25B2" : "\u25BC";

            _keySortButton.text = _sortColumn == SortColumn.Key ? $"Key {indicator}" : "Key";
            _typeSortButton.text = _sortColumn == SortColumn.Type ? $"Type {indicator}" : "Type";
        }

        #endregion

        #region Actions

        private void SaveEntry(PlayerPrefsEntry entry, string value)
        {
            if (!PlayerPrefsStorage.TrySetValue(entry, value, out string error))
            {
                EditorUtility.DisplayDialog("PlayerPrefs Manager", error, "OK");

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
                "Cancel"
            );

            if (!confirmed)
                return;

            PlayerPrefsStorage.Delete(entry.Key);
            Refresh();
        }

        private void DeleteAllEntries()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete All PlayerPrefs",
                "Delete all user-created PlayerPrefs entries?\n\n"
                    + "Unity-generated entries will be preserved.\n\n"
                    + "This action cannot be undone.",
                "Delete All",
                "Cancel"
            );

            if (!confirmed)
            {
                return;
            }

            int deletedCount = PlayerPrefsStorage.DeleteAllUserEntries();

            Debug.Log($"[PlayerPrefs Manager] Deleted {deletedCount} user-created PlayerPrefs entries.");

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

            public PlayerPrefsRow(Action<PlayerPrefsEntry, string> saveAction, Action<PlayerPrefsEntry> deleteAction)
            {
                AddToClassList(OortStyleClasses.PlayerPrefs.Row);

                _keyLabel = new Label();
                _keyLabel.AddToClassList(OortStyleClasses.PlayerPrefs.Key);

                _typeLabel = new Label();
                _typeLabel.AddToClassList(OortStyleClasses.PlayerPrefs.Type);

                _valueField = new TextField();
                _valueField.AddToClassList(OortStyleClasses.PlayerPrefs.Value);

                var saveButton = new Button(() => saveAction?.Invoke(_entry, _valueField.value)) { text = "Save" };
                saveButton.AddToClassList(OortStyleClasses.SmallButton);
                saveButton.AddToClassList(OortStyleClasses.PlayerPrefs.ActionButton);

                var deleteButton = new Button(() => deleteAction?.Invoke(_entry)) { text = "Delete" };
                deleteButton.AddToClassList(OortStyleClasses.SmallButton);
                deleteButton.AddToClassList(OortStyleClasses.PlayerPrefs.ActionButton);

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

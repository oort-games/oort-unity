# Editor Tools

## Game View Screenshot

Game View Screenshot captures the current Unity Game View and saves it as a PNG file.

### Open

```text
Oort > Tools > Game View Screenshot
```

### Features

- Capture the current Game View
- Configure the output file name
- Configure the save directory
- Browse for a directory
- Open the current output directory
- Reset the output directory
- Automatically generate unique file names
- Store the output directory per user and per project

### Default Directory

```text
Documents/OortUnity/Screenshots
```

### Unique File Naming

When a file already exists, Oort Unity automatically appends a numeric suffix.

```text
GameView.png
GameView_1.png
GameView_2.png
```

### Usage

1. Open **Game View Screenshot**.
2. Choose the save directory.
3. Enter the file name.
4. Click **Capture Screenshot**.
5. The screenshot is saved to the selected directory.

### User Settings

The selected output directory is stored in:

```text
UserSettings/OortUnityUserSettings.asset
```

This setting is local to the current Unity project and user.

---

## PlayerPrefs Manager

PlayerPrefs Manager provides an editor interface for inspecting and modifying PlayerPrefs used by the current Unity project.

### Open

```text
Oort > Tools > PlayerPrefs Manager
```

### Features

- View PlayerPrefs keys
- View value types
- View stored values
- Search by key
- Refresh the list
- Edit individual values
- Delete individual entries

### Supported Types

- `Int`
- `Float`
- `String`

### Edit a Value

1. Find the PlayerPrefs entry.
2. Edit the value.
3. Click **Save**.

The value is written through Unity's `PlayerPrefs` API.

### Delete a Value

1. Find the PlayerPrefs entry.
2. Click **Delete**.
3. Confirm the deletion.

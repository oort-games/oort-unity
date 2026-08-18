# Editor Tools

## GameObject Icon Generator

GameObject Icon Generator converts a Hierarchy GameObject or Project Prefab into a square PNG icon without modifying the source object.

### Open

```text
Oort > Tools > GameObject Icon Generator
```

### Features

- Select a Hierarchy GameObject or Project Prefab
- Use the current Hierarchy or Project selection
- Automatically detect UI objects through `RectTransform` and `CanvasRenderer`
- Preview transparent areas over a checkerboard
- Choose 64, 128, 256, or 512px output, or enter a custom resolution
- Use a transparent or solid-color background
- Configure proportional padding
- Use front, back, isometric, or custom rotation for 3D objects
- Use perspective or orthographic projection for 3D objects
- Configure studio main and fill light rotation, color, and intensity for 3D objects
- Use active Directional, Point, and Spot Lights from the source Scene
- Automatically frame enabled child Renderers
- Automatically generate unique PNG file names

### Usage

1. Open **GameObject Icon Generator**.
2. Assign a GameObject or Prefab, or select one and click **Use Selection**.
3. Confirm whether the source was detected as **3D** or **UI**.
4. Adjust the live preview and render settings.
5. Configure the file name and save directory.
6. Click **Generate PNG Icon**.

The default output directory is:

```text
Documents/{ProductName}/Icons
```

The last output and render settings are stored in `UserSettings/OortUnityUserSettings.asset`.

### Rendering Notes

3D sources are copied into a temporary Preview Scene and framed from their enabled Renderer bounds. The default view is Front, with Back, Isometric, and Custom rotations available. Studio lighting uses configurable main and fill Directional Lights without shadows. Scene lighting copies enabled Directional, Point, and Spot Lights from the source Scene; Prefab Assets use the active Scene. Point and Spot Light transforms are preserved relative to the source object, while baked lighting, ambient settings, and reflection probes are not copied. UI sources are copied under a temporary Screen Space - Camera Canvas and rendered from their current RectTransform size. Temporary cameras, lights, clones, RenderTextures, and readable textures are cleaned up after each render.

The source hierarchy is rendered independently. Parent `Mask`, `CanvasGroup`, or `LayoutGroup` effects that are not part of the selected hierarchy can therefore look different. Particle systems, trails, and animations are captured in their current static state. URP rendering has been verified; HDRP-specific output has not been validated.

### Sample

Import **GameObject Icon Generator Sample** from Package Manager and open:

```text
Scenes/GameObjectIconGeneratorSample.unity
```

The scene contains Unlit and URP/Lit 3D Prefab sources, a Directional Light for Scene lighting, and a UI Image/Button/Text source.

---

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
- Add an optional watermark in Play Mode

### Default Directory

```text
Documents/{ProductName}/Screenshots
```

`{ProductName}` is the Unity project's Product Name (`Application.productName`).

The default directory is used when no directory has been saved and when **Reset** is clicked. An existing custom directory remains unchanged until it is reset.

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

### Watermark

Watermarked capture is available in Play Mode. Normal capture remains available when the watermark is disabled.

1. Enable **Watermark**.
2. Select a PNG or `Texture2D` asset.
3. Choose one of the nine anchor positions.
4. Configure the size as a percentage of the captured width.
5. Configure opacity and edge margin.
6. Enter Play Mode and click **Capture Screenshot**.

The defaults are bottom right, 15% width, 70% opacity, and a 24px margin. Textures with **Read/Write** disabled are supported.

Click **Reset Watermark** to disable the watermark, clear the selected texture, and restore all watermark options to their defaults.

### User Settings

The selected output directory is stored in:

```text
UserSettings/OortUnityUserSettings.asset
```

The output directory and watermark settings are local to the current Unity project and user.

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
- Sort by key or value type
- Refresh the list
- Edit individual values
- Delete individual entries
- Delete all user-created entries while preserving Unity-generated entries

### Supported Types

- `Int`
- `Float`
- `String`

### Sort Entries

1. Click **Key** or **Type** in the list header.
2. Click the same header again to reverse the sort direction.

The active sort column displays `▲` for ascending order or `▼` for descending order. Entries are sorted by key in ascending order by default, and the selected sort remains active when searching or refreshing the list.

### Edit a Value

1. Find the PlayerPrefs entry.
2. Edit the value.
3. Click **Save**.

The value is written through Unity's `PlayerPrefs` API.

### Delete a Value

1. Find the PlayerPrefs entry.
2. Click **Delete**.
3. Confirm the deletion.

### Delete All User Entries

1. Click **Delete All**.
2. Review the warning that the operation cannot be undone.
3. Click **Delete All** in the confirmation dialog.

The manager deletes each user-created entry individually and does not call `PlayerPrefs.DeleteAll()`.
The operation applies to all enumerated user-created entries, regardless of the current search filter.
The following Unity-generated entries are preserved:

- Keys starting with `unity.`
- Keys starting with `Screenmanager `
- `UnityGraphicsQuality`
- `UnitySelectMonitor`

The list refreshes automatically after deletion.

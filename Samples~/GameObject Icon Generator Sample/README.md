# GameObject Icon Generator Sample

Open `Scenes/GameObjectIconGeneratorSample.unity` and use either of these objects as the source for **Oort > Tools > GameObject Icon Generator**:

- `3D Icon Prefab (Select This)` demonstrates automatic 3D bounds and camera framing. The original prefab is available in `Prefabs/Sample3DIcon.prefab`.
- `Lit Icon Prefab (Select This)` uses `Prefabs/LitIconSample.prefab` and a URP/Lit material to demonstrate Studio and Scene lighting changes.
- `UI Icon Card (Select This)` demonstrates independent rendering of a UI hierarchy containing an Image, Button, and child Text.

Select a source in the Hierarchy or Project window, click **Use Selection**, adjust the preview, and generate a PNG.

Select the Lit Prefab, switch **Lighting > Source** between **Studio** and **Scene**, and adjust the Scene's Directional Light to compare the results. The Lit sample requires Universal Render Pipeline.

The UI sample is intentionally self-contained. UI elements that depend on a parent Mask, CanvasGroup, or LayoutGroup can look different when rendered independently.

# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.4] - 2026-08-19

### Added

- Added SpriteRenderer-only 2D source detection and orthographic rendering to GameObject Icon Generator.
- Added a 2D Sprite source to the GameObject Icon Generator Sample.
- Added **Oort Unity** Preferences for editing Game View Screenshot and GameObject Icon Generator settings, including per-tool resets, a confirmed global reset, and an `Oort > Preferences` shortcut.

### Changed

- Updated open Game View Screenshot and GameObject Icon Generator windows immediately when values change in Oort Unity Preferences.

### Fixed

- Fixed Light cookie-size API compatibility by using `cookieSize2D` on Unity 6000.3 or later and `cookieSize` on earlier Unity versions.

## [0.0.3] - 2026-08-19

### Added

- Added `TextureUtility` for readable texture copies, resizing, and tinted alpha blending.
- Added `GameObjectBoundsUtility` for UI hierarchy detection, combined Renderer bounds, and RectTransform size calculation.
- Added optional Play Mode watermark capture to Game View Screenshot with configurable texture, position, size, opacity, margin, and a reset control.
- Added GameObject Icon Generator for rendering Hierarchy GameObjects and Prefabs as square PNG icons with automatic 3D/UI detection, live transparency preview, front, back, isometric, and custom views, configurable studio or Scene lighting, and reusable output settings.
- Added a GameObject Icon Generator Sample with Unlit and URP/Lit 3D Prefabs, Scene lighting, and UI icon-generation sources.

### Changed

- Changed Game View Screenshot and GameObject Icon Generator output-directory actions to create missing folders and open the configured directory directly.
- Reorganized editor tool preferences into per-tool setting groups. Existing Game View Screenshot output-directory preferences are reset when upgrading.

## [0.0.2] - 2026-08-17

### Added

- Added ascending and descending sorting by key and value type to PlayerPrefs Manager.
- Added a **Delete All** action that removes user-created PlayerPrefs after confirmation while preserving Unity-generated entries.

### Changed

- Changed the default Game View Screenshot output directory to `Documents/{ProductName}/Screenshots`.
- Centralized shared UI Toolkit class names used by the editor tool windows.
- Simplified editor tool and utility source formatting and made PlayerPrefs sorting controls visually consistent with the list labels.

### Fixed

- Restored corrupted Korean comments in `FileUtility`, `PathUtility`, and `VisualElementUtility`.

## [0.0.1] - 2026-08-13

### Added

- Initial release of **Oort Unity**.
- Added reusable `FileUtility` and `PathUtility` helpers.
- Added Game View Screenshot tool with configurable file name and save directory.
- Added PlayerPrefs Manager for viewing, editing, searching, refreshing, and deleting PlayerPrefs values.

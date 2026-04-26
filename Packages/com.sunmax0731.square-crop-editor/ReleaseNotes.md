# Release Notes

## v0.3.1

- Unity Editor menu entries are now standardized as `Tools > Square Crop Editor > メイン画面`, `ライセンス`, and `バージョン情報`.
- Added a dedicated Unity Editor information window for MIT License and version details.
- Added package `LICENSE.md` and updated README, manuals, validation checklist, release notes, and release copy for MIT License alignment.
- This patch release keeps the existing single-image crop/export behavior and refreshes the public documentation surface.


## v0.3.0

### Added

- Source preview scrollbars when the zoomed image exceeds the visible preview area.
- Numeric `Pan X` / `Pan Y` fields synchronized with Source preview scrollbars.
- Output Padding setting for transparent margins around the exported selection.

### Changed

- Output padding is applied consistently to Output preview and PNG export.
- Output mapping now places `Fit`, `Fill`, and `Stretch` results inside the padded content area.

## v0.2.0

### Added

- Free crop ratio mode for unconstrained crop selections.
- Drag-to-move behavior when starting a drag inside the current selection.
- Source preview zoom and pan controls.
- Manual selection fields for X/Y/W/H adjustment.
- Output folder controls, PNG extension helper, and export confirmation dialog.
- Auto/Japanese/English language setting.
- Parameter help window.
- Detached Output preview window.
- Tools menu entries for Output Preview, License, and Version.

### Changed

- Reorganized the editor layout so Source preview remains in the main window and Output preview opens separately.
- Improved status and validation messages.
- Selects and pings generated assets when exporting under `Assets`.

## v0.1.0

Initial MVP release.

### Added

- Editor window at `Tools > Square Crop Editor > メイン画面`.
- Single source `Texture2D` selection.
- Drag-based crop selection on the source preview.
- Crop aspect ratio presets and custom ratio.
- Output aspect ratio presets and custom ratio.
- Transparent output preview with checkerboard background.
- PNG export to `Assets/Generated/SquareCrop` by default.
- `Fit`, `Fill`, and `Stretch` mapping modes.
- Export conflict behavior: `Overwrite`, `Skip`, and `Duplicate`.
- Temporary readable copy path for Read/Write disabled source textures.
- EditMode tests for crop math, output planning, PNG export, conflict handling, and readback behavior.

### Known Limitations

- Single source image only.
- Single active selection only.
- No batch export.
- No automatic object detection.
- No mask editing.
- No atlas or grid slicing workflow.
- No session JSON or preset save/load.
- PNG output only.

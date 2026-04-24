# Release Notes

## v0.1.0

Initial MVP release.

### Added

- Editor window at `Tools > Square Crop Editor > Open`.
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

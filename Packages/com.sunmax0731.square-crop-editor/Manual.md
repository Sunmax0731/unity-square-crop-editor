# Unity Square Crop Editor Manual

Unity Square Crop Editor is a Unity Editor extension for drag-selecting a region from a transparent `Texture2D` and exporting that selection as a transparent PNG with a configurable aspect ratio.

## Requirements

- Unity 6000.0 or later
- Windows Unity Editor is the primary validation target
- Editor-only package

## Open The Tool

Open the Editor window from:

```text
Tools > Square Crop Editor > Open
```

## Basic Workflow

1. Assign a PNG or other `Texture2D` asset to `Source Image`.
2. Choose a `Crop Ratio`.
3. Drag on the source preview to select the crop region.
4. Choose an `Output Ratio`.
5. Set `Output Long Edge` in pixels.
6. Choose a `Mapping` mode.
7. Confirm `Output Folder` and `Output File`.
8. Press `Export PNG`.

Default output folder:

```text
Assets/Generated/SquareCrop
```

## Aspect Ratios

`Crop Ratio` and `Output Ratio` are independent settings.

Available presets:

- `Square`
- `Landscape4By3`
- `Portrait3By4`
- `Landscape16By9`
- `Portrait9By16`
- `Custom`

`Custom` uses positive integer width and height values.

## Mapping Modes

- `Fit`: keeps the full selection visible and adds transparent padding when needed.
- `Fill`: fills the output canvas and may crop part of the selected source region.
- `Stretch`: scales the selected region directly to the output dimensions.

## Read/Write Disabled Sources

When the source texture is not directly readable, the tool tries to create a temporary readable copy from the asset file. It does not permanently change the source importer settings.

If a readable copy cannot be created, the status message shows the reason.

## Manual Smoke

1. Open the Unity project.
2. Run `Tools > Square Crop Editor > Open`.
3. Assign the sample PNG or another transparent PNG to `Source Image`.
4. Select `Square` for `Crop Ratio` and drag on the source preview.
5. Confirm the selection outline is constrained to a square.
6. Change `Crop Ratio` to `Landscape16By9` and drag again.
7. Change `Output Ratio` to `Square`, then `Portrait9By16`.
8. Confirm the output preview dimensions and image update.
9. Switch `Mapping` through `Fit`, `Fill`, and `Stretch`.
10. Set `Output Folder` to `Assets/Generated/SquareCrop`.
11. Press `Export PNG`.
12. Confirm the exported PNG dimensions match the output ratio and long edge.
13. Confirm transparent pixels remain transparent.
14. Disable Read/Write on the source texture and confirm export still works without permanently changing importer settings.

## Known Limitations

- `v0.1.0` supports one source image and one active selection.
- Batch export is not supported.
- Automatic object detection, masks, atlases, and grid slicing are not supported.
- Session JSON and preset save/load are not supported.
- The source image is assumed to use transparency for the background.
- PNG is the only output format.

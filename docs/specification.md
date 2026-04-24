# Functional Specification

## 1. Tool Name

Unity Square Crop Editor

## 2. Menu

```text
Tools > Square Crop Editor > Open
```

## 3. Crop Coordinate System

Source pixel coordinates use top-left origin in UI-facing values.

Unity texture operations may use bottom-left origin internally, but public UI and session data should be consistent with the visible image.

## 4. Selection Rules

- Dragging from any direction is supported.
- Selection is clamped to source bounds.
- Minimum valid selection is `1 x 1` pixel.
- If the source image is missing, selection controls are disabled.
- Crop selection is constrained by the active crop aspect ratio.
- Default crop aspect ratio is `1:1`.
- Preset crop ratios are `1:1`, `4:3`, `3:4`, `16:9`, and `9:16`.
- Custom crop ratio uses positive numeric `width` and `height` values.

## 5. Output Rules

- Output is always PNG.
- Output canvas aspect ratio is configurable.
- Default output aspect ratio is `1:1`.
- Preset output ratios are `1:1`, `4:3`, `3:4`, `16:9`, and `9:16`.
- Custom output ratio uses positive numeric `width` and `height` values.
- Output size must be a positive integer.
- Output size means long edge length for non-square ratios.
- Output short edge is derived from output aspect ratio and rounded to the nearest positive integer.
- Recommended default output size: `256`.
- Default output folder: `Assets/Generated/SquareCrop`.
- Default output name: source file name plus `_crop`.
- Source alpha must be preserved.
- Canvas padding must be transparent.
- The MVP does not support solid matte background fill.

## 6. Conflict Behavior

Initial options:

- `Overwrite`
- `Skip`
- `Duplicate`

`Duplicate` should produce stable suffixes such as `_copy01`, `_copy02`.

## 7. Validation Messages

Validation should report:

- no source image selected
- source texture cannot be read
- no crop region selected
- crop region has invalid size
- crop aspect ratio is invalid
- output aspect ratio is invalid
- output size is invalid
- output folder is invalid
- output file name is invalid
- target file already exists when conflict mode blocks export

Validation warnings should not prevent preview when a usable crop region exists.

## 8. Quality and Preview

The output preview should render even if export settings are incomplete, as long as source image, crop region, output aspect ratio, and output size are valid.

Export may still be blocked by output path or file system errors.

Transparent pixels should be shown over a checkerboard or equivalent editor preview background.

## 9. Session JSON Candidate

Session JSON is deferred beyond `v0.1.0`.

The MVP should keep all state in the EditorWindow session only. This avoids committing to a persistence format before the crop math, export behavior, and validation UX are proven.

When persistence is added later, the candidate format is:

```json
{
  "formatVersion": 1,
  "source": {
    "assetPath": "Assets/source.png"
  },
  "selection": {
    "x": 0,
    "y": 0,
    "width": 128,
    "height": 128,
    "aspectRatio": {
      "mode": "Preset",
      "width": 1,
      "height": 1
    }
  },
  "output": {
    "size": 256,
    "aspectRatio": {
      "mode": "Preset",
      "width": 1,
      "height": 1
    },
    "mode": "Fit",
    "folder": "Assets/Generated/SquareCrop",
    "fileName": "source_crop.png"
  }
}
```

## 10. MVP Behavior Summary

The initial implementation should behave as follows:

- one source image at a time
- one active crop selection at a time
- transparent source image assumption
- crop selection constrained by square, preset ratio, or custom numeric ratio
- visible selection overlay on the source preview
- output preview updates from source, selection, output aspect ratio, size, and mapping mode
- export writes an alpha-preserving PNG and never edits the source asset
- `Overwrite`, `Skip`, and `Duplicate` are the initial file-conflict behaviors
- export refreshes the AssetDatabase when the output is under `Assets/`

## 11. Package Scaffold Handoff

Issue #3 can start from these fixed decisions:

- package path: `Packages/com.sunmax0731.square-crop-editor`
- menu path: `Tools > Square Crop Editor > Open`
- default output size: `256`
- default crop aspect ratio: `1:1`
- default output aspect ratio: `1:1`
- default output folder: `Assets/Generated/SquareCrop`
- default canvas mapping mode: `Fit`
- MVP persistence: EditorWindow memory only

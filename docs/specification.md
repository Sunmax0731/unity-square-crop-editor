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

## 5. Output Rules

- Output is always PNG.
- Output canvas is always square.
- Output size must be a positive integer.
- Recommended default output size: `256`.
- Default output folder: `Assets/Generated/SquareCrop`.
- Default output name: source file name plus `_crop`.

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
- output size is invalid
- output folder is invalid
- output file name is invalid
- target file already exists when conflict mode blocks export

Validation warnings should not prevent preview when a usable crop region exists.

## 8. Quality and Preview

The output preview should render even if export settings are incomplete, as long as source image and crop region are valid.

Export may still be blocked by output path or file system errors.

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
    "height": 96
  },
  "output": {
    "size": 256,
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
- visible selection overlay on the source preview
- output preview updates from source, selection, size, and conversion mode
- export writes a square PNG and never edits the source asset
- `Overwrite`, `Skip`, and `Duplicate` are the initial file-conflict behaviors
- export refreshes the AssetDatabase when the output is under `Assets/`

## 11. Package Scaffold Handoff

Issue #3 can start from these fixed decisions:

- package path: `Packages/com.sunmax0731.square-crop-editor`
- menu path: `Tools > Square Crop Editor > Open`
- default output size: `256`
- default output folder: `Assets/Generated/SquareCrop`
- default conversion mode: `Fit`
- MVP persistence: EditorWindow memory only

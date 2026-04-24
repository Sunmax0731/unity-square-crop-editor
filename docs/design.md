# Design

## 1. Architecture

The project should separate pure crop logic from Unity Editor UI.

```text
Packages/com.sunmax0731.square-crop-editor/
  Runtime/
    Models/
    Services/
  Editor/
    Windows/
    Export/
  Tests/
    Editor/
```

Runtime code should contain deterministic model and image-region calculations. Editor code should handle Unity-specific asset selection, preview rendering, PNG writing, and AssetDatabase refresh.

## 2. Core Models

### CropSelection

Represents the selected source image region in source pixel coordinates.

Fields:

- `X`
- `Y`
- `Width`
- `Height`

### SquareCropSettings

Represents output configuration.

Fields:

- `OutputSize`
- `ConversionMode`
- `PaddingColor`
- `OutputFolder`
- `OutputFileName`
- `ConflictBehavior`

### SquareConversionMode

Initial values:

- `Fit`
- `Fill`
- `Stretch`

## 3. Services

### CropRectCalculator

Converts drag coordinates in preview space into source pixel coordinates.

Responsibilities:

- account for preview scaling
- clamp selection to source image bounds
- normalize drag direction
- reject zero-sized regions

### SquareOutputPlanner

Calculates how the selected region maps onto the square output canvas.

Responsibilities:

- fit/fill/stretch source rect
- calculate destination rect
- calculate transparent padding
- return warnings when selection is too small or invalid

### PngSquareExporter

Exports the selected region as a PNG.

Responsibilities:

- read pixels
- resample into square texture
- encode PNG
- write file
- return exported/skipped/error result

## 4. Editor Window Layout

Candidate layout:

```text
Toolbar:
  Source Image | Preview | Export... | Save Preset | Load Preset | Language

Left pane:
  Source
  Crop Settings
  Output

Center pane:
  Source image preview
  draggable selection overlay

Right pane:
  Square output preview
  selection details
  validation report
```

## 5. Drag Behavior

- Mouse down starts selection.
- Drag updates selection.
- Mouse up commits selection.
- Shift-drag may constrain selection to square in source space as a future option.
- Escape may clear active selection as a future option.

## 6. Output Behavior

The source selection may be rectangular, but exported output is always square.

Mode behavior:

- `Fit`: no source pixels are lost; unused square area is transparent or padding color.
- `Fill`: square is filled; source region may be cropped.
- `Stretch`: direct scale; aspect ratio may change.

## 7. Readability Strategy

The first implementation should avoid permanent importer changes.

Preferred behavior:

1. If source texture is readable, use it directly.
2. If not readable and asset path is readable from disk, create a temporary readable copy.
3. Delete temporary copy after export.
4. Report failure reason if pixels still cannot be read.

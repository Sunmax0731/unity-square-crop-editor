# Requirements

## 1. Purpose

Unity Square Crop Editor is a Unity Editor extension that lets a user drag-select an area of a transparent image, crop that area, and output a transparent PNG. The default selection and output shape is square, but both can use preset or custom numeric aspect ratios.

The tool is intended for preparing item icons, portraits, thumbnails, UI images, and generated image fragments that need a consistent transparent PNG format.

## 2. Users

- Unity developers preparing project assets.
- Game developers creating item icons, character portraits, and UI images with consistent aspect ratios.
- Users who generate larger images and need to crop useful regions inside Unity.

## 3. Functional Requirements

### MVP Scope Decision

`v0.1.0` is limited to a single-image, single-selection, transparent PNG export workflow.

Included:

- one `Texture2D` source image
- transparent background source image assumption
- one active crop region
- mouse drag selection in the preview
- crop selection aspect ratio controls with square default
- output aspect ratio controls with square default
- transparent output preview
- `Fit`, `Fill`, and `Stretch` canvas mapping modes
- output size selection
- PNG export under the Unity project
- conflict handling for existing files
- clear validation messages for export blockers

Excluded from `v0.1.0`:

- batch processing
- automatic object detection
- masks
- atlas or grid slicing
- permanent source importer modification
- preset browser or session JSON save/load UI

### RQ-001 Tool Launch

The user can open the tool from:

```text
Tools > Square Crop Editor > メイン画面
```

### RQ-002 Source Image Selection

The user can select a Unity `Texture2D` asset as the source image.

The source image is assumed to use transparency for the background.

The tool must show the asset path, pixel size, readability status, and whether the source format has an alpha channel when that can be detected.

### RQ-003 Drag Selection

The user can drag over the image preview to define a rectangular crop region.

The selected region must be visible with an outline and optional dimmed outside area.

The selection shape must be constrained by a crop aspect ratio setting.

Initial crop aspect ratio options:

- `1:1` square default
- `4:3`
- `3:4`
- `16:9`
- `9:16`
- custom numeric width and height ratio, both positive

The active ratio should affect drag behavior directly, so the committed crop selection matches the chosen ratio after clamping to source bounds.

### RQ-004 Output Aspect Ratio

The crop result is exported as a PNG with a configurable output aspect ratio.

The output aspect ratio uses the same kind of options as crop selection:

- `1:1` square default
- `4:3`
- `3:4`
- `16:9`
- `9:16`
- custom numeric width and height ratio, both positive

The crop selection ratio and output ratio are independent settings. They may be the same, but the user must be able to change each setting separately.

The tool must support at least these canvas mapping modes:

- `Fit`: fit the selected region inside the output canvas, preserving aspect ratio and adding transparent padding.
- `Fill`: fill the output canvas, preserving aspect ratio and cropping overflow.
- `Stretch`: stretch the selected region to the output canvas dimensions.

### RQ-005 Output Size

The user can choose output pixel size.

Initial supported sizes:

- 64
- 128
- 256
- 512
- custom positive integer

For non-square output, the selected size represents the long edge. The short edge is derived from the output aspect ratio and rounded to the nearest positive integer. For `1:1`, width and height are equal.

### RQ-006 Preview

The tool shows:

- source image preview
- selected crop region
- transparent output preview

The output preview must update when crop region, crop aspect ratio, output aspect ratio, output size, or mapping mode changes.

The preview should use a checkerboard or equivalent editor pattern so transparent pixels are visually distinguishable.

### RQ-007 Export

The user can export the PNG to a project-relative folder.

The tool must support:

- output folder
- output file name
- overwrite confirmation or conflict handling
- AssetDatabase refresh after export under `Assets/`

### RQ-008 Non-Destructive Source Handling

The source texture importer settings must not be permanently changed without explicit user action.

If source pixels cannot be read directly, the tool should use a temporary readable copy or provide a clear recovery path.

### RQ-009 Alpha Preservation

The output image background must remain transparent.

Rules:

- Source pixel alpha must be preserved during crop and resampling.
- Empty canvas areas, including `Fit` padding, must be transparent.
- The MVP must not composite the output onto a solid background.
- Output PNG alpha values depend on the original image pixels and transparent padding, not on a user-selected matte color.

### RQ-010 Session Persistence

Session JSON is not part of the `v0.1.0` MVP.

Reason:

- The first release must prove the crop selection, square conversion, preview, and export path before adding persistence.
- Repeatability can be added after the core workflow is validated.

When implemented after MVP, it should store:

- source asset path
- crop region
- crop aspect ratio
- output aspect ratio
- output size
- conversion mode
- output folder and file name

## 4. UX Requirements

- The user must be able to complete the MVP workflow from one EditorWindow.
- Controls that cannot run because required input is missing should be disabled or paired with a clear validation message.
- The preview must prioritize direct manipulation over form-only input.
- Export should be blocked only for hard errors such as missing source, invalid crop, invalid output size, or invalid output path.
- Warnings such as a small selection should remain visible but should not prevent preview.

## 5. Non-Functional Requirements

- Unity `6000.0` or later.
- Windows Unity Editor is the primary supported environment.
- Editor-only package.
- Deterministic crop math with EditMode tests.
- UI operations should remain responsive for typical icon and portrait images.
- The package must remain independent from Unity Grid Asset Slicer.
- The tool must not require external image-processing dependencies for the MVP.

## 6. MVP Acceptance

- Tool opens from the Tools menu.
- User can select a PNG texture.
- User can drag-select a region on the preview.
- User can preview a transparent result.
- User can export a PNG.
- EditMode tests cover crop math, aspect ratio constraints, alpha-preserving output planning, and output rect calculation.

## 7. Aspect-Aware Implementation Start Criteria

Implementation can move to crop selection and aspect mapping when:

- `Fit`, `Fill`, and `Stretch` are the only MVP canvas mapping modes.
- Crop and output aspect ratios both support square default, presets, and custom numeric ratios.
- Session JSON is explicitly deferred beyond `v0.1.0`.
- Crop coordinates are defined as top-left UI-facing source pixel coordinates.
- Source alpha preservation and transparent padding behavior are defined.
- Export defaults are defined in the functional specification.
- Validation scope is defined in `docs/validation-plan.md`.

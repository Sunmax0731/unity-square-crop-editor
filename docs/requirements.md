# Requirements

## 1. Purpose

Unity Square Crop Editor is a Unity Editor extension that lets a user drag-select an area of an image, crop that area, and output a square PNG.

The tool is intended for preparing item icons, portraits, thumbnails, UI images, and generated image fragments that need a consistent square format.

## 2. Users

- Unity developers preparing project assets.
- Game developers creating square item icons or character portraits.
- Users who generate larger images and need to crop useful regions inside Unity.

## 3. Functional Requirements

### MVP Scope Decision

`v0.1.0` is limited to a single-image, single-selection, square-PNG export workflow.

Included:

- one `Texture2D` source image
- one active crop region
- mouse drag selection in the preview
- square output preview
- `Fit`, `Fill`, and `Stretch` conversion modes
- output size selection
- PNG export under the Unity project
- conflict handling for existing files
- clear validation messages for export blockers

Excluded from `v0.1.0`:

- batch processing
- automatic object detection
- masks
- non-square export
- atlas or grid slicing
- permanent source importer modification
- preset browser or session JSON save/load UI

### RQ-001 Tool Launch

The user can open the tool from:

```text
Tools > Square Crop Editor > Open
```

### RQ-002 Source Image Selection

The user can select a Unity `Texture2D` asset as the source image.

The tool must show the asset path, pixel size, and readability status.

### RQ-003 Drag Selection

The user can drag over the image preview to define a rectangular crop region.

The selected region must be visible with an outline and optional dimmed outside area.

### RQ-004 Square Output

The crop result is exported as a square PNG.

The tool must support at least these square conversion modes:

- `Fit`: fit the selected region inside a square canvas, preserving aspect ratio and adding transparent padding.
- `Fill`: fill the square canvas, preserving aspect ratio and cropping overflow.
- `Stretch`: stretch the selected region to the square size.

### RQ-005 Output Size

The user can choose output pixel size.

Initial supported sizes:

- 64
- 128
- 256
- 512
- custom positive integer

### RQ-006 Preview

The tool shows:

- source image preview
- selected crop region
- square output preview

The output preview must update when crop region, output size, or conversion mode changes.

### RQ-007 Export

The user can export the square PNG to a project-relative folder.

The tool must support:

- output folder
- output file name
- overwrite confirmation or conflict handling
- AssetDatabase refresh after export under `Assets/`

### RQ-008 Non-Destructive Source Handling

The source texture importer settings must not be permanently changed without explicit user action.

If source pixels cannot be read directly, the tool should use a temporary readable copy or provide a clear recovery path.

### RQ-009 Session Persistence

Session JSON is not part of the `v0.1.0` MVP.

Reason:

- The first release must prove the crop selection, square conversion, preview, and export path before adding persistence.
- Repeatability can be added after the core workflow is validated.

When implemented after MVP, it should store:

- source asset path
- crop region
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
- User can preview a square result.
- User can export a PNG.
- EditMode tests cover crop math and output rect calculation.

## 7. Implementation Start Criteria

Implementation can move to package scaffold when:

- `Fit`, `Fill`, and `Stretch` are the only MVP conversion modes.
- Session JSON is explicitly deferred beyond `v0.1.0`.
- Crop coordinates are defined as top-left UI-facing source pixel coordinates.
- Export defaults are defined in the functional specification.
- Validation scope is defined in `docs/validation-plan.md`.

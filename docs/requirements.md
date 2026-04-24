# Requirements

## 1. Purpose

Unity Square Crop Editor is a Unity Editor extension that lets a user drag-select an area of an image, crop that area, and output a square PNG.

The tool is intended for preparing item icons, portraits, thumbnails, UI images, and generated image fragments that need a consistent square format.

## 2. Users

- Unity developers preparing project assets.
- Game developers creating square item icons or character portraits.
- Users who generate larger images and need to crop useful regions inside Unity.

## 3. Functional Requirements

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

The MVP should decide whether session JSON is required.

If implemented, it should store:

- source asset path
- crop region
- output size
- conversion mode
- output folder and file name

## 4. Non-Functional Requirements

- Unity `6000.0` or later.
- Windows Unity Editor is the primary supported environment.
- Editor-only package.
- Deterministic crop math with EditMode tests.
- UI operations should remain responsive for typical icon and portrait images.

## 5. MVP Acceptance

- Tool opens from the Tools menu.
- User can select a PNG texture.
- User can drag-select a region on the preview.
- User can preview a square result.
- User can export a PNG.
- EditMode tests cover crop math and output rect calculation.

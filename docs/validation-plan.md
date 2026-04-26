# Validation Plan

## Automated Tests

Use EditMode tests for deterministic behavior.

Required areas:

- drag coordinate normalization
- source bounds clamping
- crop aspect ratio presets and custom numeric ratio
- output aspect ratio presets and custom numeric ratio
- output width/height derivation from long edge size
- Fit mapping
- Fill mapping
- Stretch mapping
- source alpha preservation
- transparent padding behavior
- invalid selection validation
- invalid aspect ratio validation
- output file name and conflict behavior
- default settings for output size, folder, file name suffix, conversion mode, and conflict behavior

Out of MVP automated scope:

- session JSON serialization
- preset save/load
- batch crop export

## Manual Smoke

1. Open Unity project.
2. Open `Tools > Square Crop Editor > メイン画面`.
3. Select a PNG source image.
4. Select crop aspect ratio: default square, then at least one non-square preset.
5. Drag a region.
6. Confirm selected region outline follows the chosen crop aspect ratio.
7. Select output aspect ratio: default square, then at least one non-square preset.
8. Confirm transparent output preview updates.
9. Test `Fit`, `Fill`, and `Stretch`.
10. Export to `Assets/Generated/SquareCrop`.
11. Confirm output PNG dimensions match output aspect ratio.
12. Confirm transparent pixels remain transparent.
13. Repeat with a source texture that has Read/Write disabled.

## Aspect Requirement Baseline Check

The requirements baseline is ready for implementation when:

- MVP scope is limited to one transparent source image and one crop selection.
- Crop and output aspect ratios both support square default, presets, and custom numeric ratios.
- `Fit`, `Fill`, and `Stretch` are the only initial canvas mapping modes.
- Alpha preservation and transparent padding behavior are specified.
- Session JSON is deferred beyond `v0.1.0`.
- Export blockers and warnings are listed in the functional specification.
- Crop selection and aspect mapping can be implemented without reopening product-scope decisions.

## Release Gate

Before release:

- automated EditMode tests pass
- manual smoke is complete
- release zip contains manual, terms, release notes, validation checklist, samples, and unitypackage
- GitHub Release and BOOTH artifacts are identical

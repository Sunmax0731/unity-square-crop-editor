# Validation Plan

## Automated Tests

Use EditMode tests for deterministic behavior.

Required areas:

- drag coordinate normalization
- source bounds clamping
- Fit mapping
- Fill mapping
- Stretch mapping
- invalid selection validation
- output file name and conflict behavior
- default settings for output size, folder, file name suffix, conversion mode, and conflict behavior

Out of MVP automated scope:

- session JSON serialization
- preset save/load
- batch crop export

## Manual Smoke

1. Open Unity project.
2. Open `Tools > Square Crop Editor > Open`.
3. Select a PNG source image.
4. Drag a rectangular region.
5. Confirm selected region outline is visible.
6. Confirm square output preview updates.
7. Test `Fit`, `Fill`, and `Stretch`.
8. Export to `Assets/Generated/SquareCrop`.
9. Confirm output PNG is square and imported by Unity.
10. Repeat with a source texture that has Read/Write disabled.

## Issue #2 Baseline Check

The requirements baseline is ready for implementation when:

- MVP scope is limited to one source image and one crop selection.
- `Fit`, `Fill`, and `Stretch` are the only initial conversion modes.
- Session JSON is deferred beyond `v0.1.0`.
- Export blockers and warnings are listed in the functional specification.
- The next issue can create the UPM package scaffold without reopening product-scope decisions.

## Release Gate

Before release:

- automated EditMode tests pass
- manual smoke is complete
- release zip contains manual, terms, release notes, validation checklist, samples, and unitypackage
- GitHub Release and BOOTH artifacts are identical

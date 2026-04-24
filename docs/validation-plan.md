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

## Release Gate

Before release:

- automated EditMode tests pass
- manual smoke is complete
- release zip contains manual, terms, release notes, validation checklist, samples, and unitypackage
- GitHub Release and BOOTH artifacts are identical

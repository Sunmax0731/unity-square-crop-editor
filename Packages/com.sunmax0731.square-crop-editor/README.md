# Unity Square Crop Editor

Unity Square Crop Editor is a Unity Editor extension for selecting a region of a transparent `Texture2D` asset and exporting the selected region as a PNG with a configurable aspect ratio. Square is the default ratio.

## MVP Scope

- Open from `Tools > Square Crop Editor > Open`.
- Select one source image.
- Drag one crop region constrained by square, preset aspect ratio, or custom numeric ratio.
- Preview transparent PNG output.
- Export PNG with the selected output aspect ratio.

Batch export, grid slicing, automatic detection, masks, and session JSON persistence are outside the `v0.1.0` MVP.

## Package Layout

```text
Runtime/
  Models/
  Services/
Editor/
  Windows/
Tests/
  Editor/
```

Runtime code should stay deterministic and testable. Editor code should handle Unity menu integration, windows, asset selection, export, and AssetDatabase refresh.

## User Documents

- [Manual](Manual.md)
- [Japanese Manual](Manual.ja.md)
- [Validation Checklist](ValidationChecklist.md)
- [Release Notes](ReleaseNotes.md)
- [Terms Of Use](TermsOfUse.md)

## Sample

Import `Transparent Icon Source` from Unity Package Manager to get a transparent PNG for manual smoke testing.

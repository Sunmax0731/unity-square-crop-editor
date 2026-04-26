# Unity Square Crop Editor

Unity Square Crop Editor is a planned Unity Editor extension for selecting an area of a transparent image by drag operation, cropping that area, and exporting it as a PNG with a configurable aspect ratio. The default selection and output ratio is square.

This repository starts from requirements and design work. Implementation should proceed issue by issue after the product behavior, data model, and validation strategy are agreed.

## Goals

- Open a Unity Editor tool window from `Tools > Square Crop Editor > メイン画面`.
- Select a source image asset.
- Drag on the image preview to choose a crop region.
- Convert the selected region into a configurable output aspect ratio.
- Preserve source alpha and export the result as PNG without modifying the original source asset.
- Keep the workflow separate from Unity Grid Asset Slicer.

## Documents

- [Requirements](docs/requirements.md)
- [Design](docs/design.md)
- [Functional Specification](docs/specification.md)
- [Implementation Roadmap](docs/roadmap.md)
- [Validation Plan](docs/validation-plan.md)

## Initial Scope

`v0.1.0` focuses on a single-image, single-crop workflow:

1. Select source image.
2. Drag to select a crop region constrained by square, preset aspect ratio, or custom numeric ratio.
3. Preview transparent PNG output.
4. Choose output aspect ratio, output size, and fit/fill/stretch mode.
5. Export PNG.

Batch processing, automatic object detection, masks, and atlas or grid slicing are out of scope for the first release.

## Menu

- `Tools > Square Crop Editor > メイン画面`
- `Tools > Square Crop Editor > ライセンス`
- `Tools > Square Crop Editor > バージョン情報`

## License

Unity Square Crop Editor is distributed under the MIT License. See [LICENSE.md](Packages/com.sunmax0731.square-crop-editor/LICENSE.md).

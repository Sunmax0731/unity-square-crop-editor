# Unity Square Crop Editor

Unity Square Crop Editor is a planned Unity Editor extension for selecting an area of an image by drag operation, cropping that area, and exporting it as a square PNG.

This repository starts from requirements and design work. Implementation should proceed issue by issue after the product behavior, data model, and validation strategy are agreed.

## Goals

- Open a Unity Editor tool window from the Tools menu.
- Select a source image asset.
- Drag on the image preview to choose a crop region.
- Convert the selected region into a square image.
- Export the result as PNG without modifying the original source asset.
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
2. Drag to select crop region.
3. Preview square output.
4. Choose square size and padding or fit mode.
5. Export PNG.

Batch processing, automatic object detection, masks, and non-square exports are out of scope for the first release.

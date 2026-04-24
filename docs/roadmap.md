# Implementation Roadmap

## P0 Planning

### 1. Requirements and Design Baseline

Goal: agree on MVP scope, crop modes, output rules, and validation plan.

Acceptance:

- requirements, design, and specification documents exist
- GitHub Issues are created in Japanese
- implementation can start from Issue 2

## P1 Core Logic

### 2. Package Scaffold

Goal: create UPM package structure.

Acceptance:

- package compiles in Unity
- Runtime / Editor / Tests asmdefs exist
- menu registration test can be added

### 3. Crop Selection and Square Mapping

Goal: implement pure C# crop rectangle and square mapping logic.

Acceptance:

- drag normalization is tested
- source-bound clamping is tested
- Fit / Fill / Stretch mapping is tested

### 4. PNG Export Service

Goal: export selected region to a square PNG.

Acceptance:

- output size is respected
- Fit / Fill / Stretch produce expected dimensions
- conflict behavior is tested

## P2 Editor Workflow

### 5. Editor Window MVP

Goal: implement source selection, drag preview, output preview, and export UI.

Acceptance:

- source image can be selected
- drag selection appears on image
- square preview updates
- export writes PNG under `Assets/`

### 6. Readability and Validation UX

Goal: handle unreadable textures and explain export blockers.

Acceptance:

- readable source is used directly
- unreadable source can be processed via temporary copy
- validation report shows actionable reasons

## P3 Release Preparation

### 7. Documentation and Samples

Goal: add manual, sample images, and validation checklist.

### 8. Release Packaging

Goal: create release zip and unitypackage flow.

### 9. BOOTH / GitHub Release

Goal: publish v0.1.0 assets and BOOTH copy.

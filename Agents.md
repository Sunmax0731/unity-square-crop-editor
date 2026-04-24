# Agents Guide

## Product

- Product name: Unity Square Crop Editor
- Repository: `unity-square-crop-editor`
- Target: Unity Editor extension
- Delivery target: UPM Git package and optional BOOTH distribution
- Main menu path candidate: `Tools > Square Crop Editor > Open`

## Workflow

- Work from GitHub Issues.
- Create one branch per Issue.
- Use `task/issue-<number>-<short-topic>` branch names.
- Post Issues and Issue comments in Japanese.
- Keep implementation, validation notes, and documentation updates in the same branch when they belong to the same Issue.
- Merge completed work into `main` after validation.
- Update `Agents.md` or `Skill.md` when workflow rules or durable project knowledge changes.

## Engineering Principles

- Keep this tool independent from Unity Grid Asset Slicer.
- Prefer a small, deterministic crop pipeline before adding advanced image-processing features.
- Do not modify the source texture importer permanently during export.
- If Read/Write is disabled, use a temporary readable copy or documented import path.
- Keep runtime-free Editor tooling unless a runtime API becomes a clear requirement.
- Validate pure crop math with EditMode tests before relying on UI smoke checks.

## Documentation Rules

- Requirements, design, and specification changes should be reflected in `docs/`.
- Public-facing copy should be Japanese-first when preparing BOOTH distribution.
- Known limitations must be written before release packaging.

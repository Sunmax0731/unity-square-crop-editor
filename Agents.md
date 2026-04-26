# Agents Guide

## Product

- Product name: Unity Square Crop Editor
- Repository: `unity-square-crop-editor`
- Target: Unity Editor extension
- Delivery target: UPM Git package and optional BOOTH distribution
- Main menu path candidate: `Tools > Square Crop Editor > メイン画面`

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

## Unity エディタ拡張の共通実務

- GitHub Issue、Issue コメント、検証サマリー、リリースノートは日本語で記載する。コード識別子、パス、コマンド、ブランチ名、エラー文は原文を維持する。
- `EditorWindow` はユーザー操作に集中させる。crop 計算、読み取り可能 Texture 生成、出力計画、競合処理、preset/session 永続化、release 検査は UI なしで検証できる service に分離する。
- 機能追加時は、影響する README、manual/specification、validation checklist、release notes を同じ Issue で更新する。
- Issue 単位の検証では、可能な限り `ISSUE<number>_<TOPIC>_VALIDATION=PASS` のような明示 marker を出し、validation script 側でも marker を確認する。
- ユーザーが追加した QA 画像や生成済み `Assets/` 出力は、Issue でサンプル採用すると明記された場合だけコミットする。
- Release artifact は tracked files から生成する。配布 ZIP に `Assets/`、`Library/`、`Logs/`、`Temp/`、`Validation/`、`ReleaseBuilds/` が含まれないことを検査する。
- UI Toolkit への全面移行は高リスクな移行作業として扱う。安定した IMGUI ウィンドウを置き換える前に、主要ワークフローとの同等性を試作で確認する。

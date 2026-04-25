# Project Skill

Use this skill when working on `unity-square-crop-editor`.

## Required Behavior

- Post GitHub Issues in Japanese.
- Use Japanese for Issue comments and validation summaries.
- Keep code identifiers, file paths, commands, and error messages in their original language when clarity requires it.
- Follow issue-first development.
- Create a branch for each Issue before implementation.
- Merge completed work into `main` after validation.

## Product Context

The tool lets Unity users drag-select part of an image and export it as a square PNG.

The first release should stay focused:

- single source image
- one crop region at a time
- square output
- PNG export
- session or preset persistence only if needed for repeatability

Avoid expanding into atlas slicing, grid slicing, automatic detection, or batch processing before the MVP is stable.

## 共有された実装ノウハウ

- Issue ごとに作業を分け、実装、検証、ドキュメント更新、日本語の Issue コメント、close / merge 状態の報告までを 1 つの完了単位にする。
- UI 接続より先に再利用可能な処理を service に分離する。このリポジトリでは crop rect 計算、正方形化、読み取り可能 Texture 生成、PNG export、命名、競合処理を `EditorWindow` の外で検証できるようにする。
- UI を変更したら、docs、menu path、shortcut、help text、validation checklist、release packaging への影響を確認する。
- 継続的な回帰確認のため、`ISSUE<number>_<TOPIC>_VALIDATION=PASS` のような明示的な validation marker を優先する。
- Release packaging は tracked files のみから行う。生成 QA 画像、temp validation folder、Unity cache folder を release ZIP に含めない。
- GitHub Issue と Issue コメントは日本語で記載する。必要な場合だけ、コード、パス、コマンド、エラー文字列は原文を保持する。

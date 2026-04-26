# Unity Square Crop Editor v0.3.1

Unity Editor上で透明PNGなどの画像をドラッグ選択し、指定したアスペクト比のPNGとして書き出すEditor拡張です。

## 内容物

- `com.sunmax0731.square-crop-editor/` UPM package
- `UnitySquareCropEditor-v0.3.1.unitypackage`
- Manual / Manual.ja
- TermsOfUse
- ReleaseNotes
- ValidationChecklist
- Transparent Icon Source sample
- BOOTHDescription.ja.md

## 主な機能

- `Tools > Square Crop Editor > メイン画面` からEditor Windowを起動
- Source Imageを選択してプレビュー上でドラッグ選択
- Free / Square / 4:3 / 3:4 / 16:9 / 9:16 / Custom のcrop ratio
- 選択範囲内ドラッグによる位置移動
- Source previewのZoom / Pan
- ズーム時の水平/垂直スクロールバーによるPan
- Pan X / Pan Yの数値入力
- Output ratioとlong edge sizeの指定
- Output Paddingによる透明余白設定
- Output previewの別ウィンドウ表示
- Auto / Japanese / English の表示言語設定
- パラメータHelp window
- `メイン画面` / `ライセンス` / `バージョン情報` のTools menu
- `Fit` / `Fill` / `Stretch` mapping
- 透明背景を維持したPNG export
- Read/Write disabled textureの一時readable copy処理

## 検証

- Unity 6000.4.0f1
- EditMode tests
- Manual smoke checklist: `ValidationChecklist.md`

## 制限事項

- 単一画像、単一選択のみ
- PNG exportのみ
- batch export、object detection、mask editing、atlas/grid slicingは対象外
- session JSON / preset persistenceは未対応

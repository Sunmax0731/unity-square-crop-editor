# Unity Square Crop Editor

Unity Editor上で画像の一部をドラッグ選択し、正方形や任意のアスペクト比のPNGとして書き出すEditor拡張です。

透明PNGのアイコン素材、UI素材、サムネイル素材などから、必要な範囲だけを切り出してPNG化する用途を想定しています。

## 主な機能

- Unity Editor内の専用Windowで操作
- Source Imageを指定してドラッグでcrop範囲を選択
- 正方形、4:3、3:4、16:9、9:16、Custom ratioに対応
- Output sizeをlong edge基準で指定
- `Fit` / `Fill` / `Stretch` の出力方式
- 透明背景を保持したPNG書き出し
- Read/Write disabled textureでも、import設定を恒久変更せず一時コピーで処理

## 内容物

- UPM package
- unitypackage
- Manual / Manual.ja
- ReleaseNotes
- TermsOfUse
- ValidationChecklist
- サンプル画像

## 動作環境

- Unity 6000.0 以降
- 検証環境: Unity 6000.4.0f1

## 注意事項

- v0.1.0では単一画像、単一選択のPNG exportに特化しています。
- batch export、object detection、mask editing、atlas/grid slicingには対応していません。
- 本ツールはUnity Grid Asset Slicerとは独立したEditor拡張です。

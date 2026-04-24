# Unity Square Crop Editor Manual

Unity Square Crop Editor は、透明 PNG などの `Texture2D` から範囲をドラッグ選択し、指定したアスペクト比の透明 PNG として書き出す Unity Editor 拡張です。

## 動作環境

- Unity 6000.0 以降
- Windows Unity Editor を主な検証対象とします
- Editor 専用 package です

## 起動

Unity Editor のメニューから開きます。

```text
Tools > Square Crop Editor > Open
```

## 基本手順

1. `Source Image` に PNG などの `Texture2D` asset を指定します。
2. `Crop Ratio` で選択範囲のアスペクト比を選びます。
3. 左側の source preview 上でドラッグし、切り出す範囲を選択します。
4. `Output Ratio` で書き出し PNG のアスペクト比を選びます。
5. `Output Long Edge` に出力画像の長辺 px を指定します。
6. `Mapping` を選びます。
7. `Output Folder` と `Output File` を確認します。
8. `Export PNG` を押します。

既定の出力先は次の通りです。

```text
Assets/Generated/SquareCrop
```

## アスペクト比

`Crop Ratio` と `Output Ratio` は別々に設定できます。

利用できるプリセット:

- `Square`
- `Landscape4By3`
- `Portrait3By4`
- `Landscape16By9`
- `Portrait9By16`
- `Custom`

`Custom` は正の整数の幅と高さを指定します。

## Mapping

- `Fit`: 選択範囲全体を出力 canvas 内に収め、余白を透明にします。
- `Fill`: 出力 canvas 全体を埋めます。必要に応じて選択範囲の一部が切り落とされます。
- `Stretch`: 選択範囲を出力サイズへ直接伸縮します。

## Read/Write disabled の source

source texture が直接読み取れない場合、tool は importer 設定を恒久変更せず、asset ファイルから一時 readable copy を作成して preview/export を試みます。

一時 copy を作れない場合は、status に原因が表示されます。

## Manual Smoke

1. Unity project を開きます。
2. `Tools > Square Crop Editor > Open` を実行します。
3. sample PNG または任意の透明 PNG を `Source Image` に指定します。
4. `Crop Ratio` を `Square` にして source preview 上でドラッグします。
5. selection outline が正方形に制約されることを確認します。
6. `Crop Ratio` を `Landscape16By9` に変えて再度ドラッグします。
7. `Output Ratio` を `Square`、`Portrait9By16` などに変更します。
8. output preview の寸法と見た目が更新されることを確認します。
9. `Mapping` の `Fit` / `Fill` / `Stretch` を切り替えます。
10. `Output Folder` を `Assets/Generated/SquareCrop` にします。
11. `Export PNG` を押します。
12. 書き出された PNG の寸法が output ratio と long edge に一致することを確認します。
13. 透明部分が透明のまま残ることを確認します。
14. source texture の Read/Write を無効にしても importer が恒久変更されずに export できることを確認します。

## 既知の制限

- `v0.1.0` は single source / single selection のみ対応します。
- batch export は未対応です。
- 自動 object detection、mask、atlas、grid slicing は未対応です。
- session JSON、preset save/load は未対応です。
- source は透明背景を持つ画像を想定しています。
- 出力は PNG のみです。

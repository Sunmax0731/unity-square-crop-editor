using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Localization
{
    public static class SquareCropLocalization
    {
        private static readonly IReadOnlyDictionary<string, string> Japanese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["language"] = "表示言語",
            ["language.auto"] = "自動",
            ["language.japanese"] = "日本語",
            ["language.english"] = "英語",
            ["windowTitle"] = "Square Crop Editor",
            ["help"] = "ヘルプ",
            ["parameterHelp"] = "パラメータヘルプ",
            ["outputPreview"] = "Outputプレビュー",
            ["openOutputWindow"] = "Outputを開く",
            ["license"] = "ライセンス",
            ["version"] = "バージョン",
            ["sourceImage"] = "Source画像",
            ["cropRatio"] = "Crop比率",
            ["outputRatio"] = "Output比率",
            ["outputLongEdge"] = "Output長辺",
            ["mapping"] = "配置",
            ["conflict"] = "競合",
            ["outputFolder"] = "出力フォルダ",
            ["select"] = "選択",
            ["default"] = "既定",
            ["outputFile"] = "出力ファイル",
            ["resetSelection"] = "選択解除",
            ["useFullSource"] = "全体選択",
            ["centerCrop"] = "中央切り抜き",
            ["selection"] = "選択範囲",
            ["source"] = "Source",
            ["output"] = "Output",
            ["noSourceImage"] = "Source画像がありません",
            ["noPreview"] = "プレビューなし",
            ["detachedOutputReconnect"] = "Tools > Square Crop Editor > Open を開き直して、Outputプレビューを再接続してください。",
            ["exportPng"] = "PNG出力",
            ["zoom"] = "ズーム",
            ["panX"] = "パン X",
            ["panY"] = "パン Y",
            ["confirmPngExport"] = "PNG出力の確認",
            ["export"] = "出力",
            ["cancel"] = "キャンセル",
            ["dialog.output"] = "出力",
            ["dialog.selection"] = "選択範囲",
            ["dialog.mapping"] = "配置",
            ["dialog.conflict"] = "競合",
            ["status.selectSource"] = "Source画像を選択してください。",
            ["status.sourceReadable"] = "Source: {0} x {1}",
            ["status.sourceTemporaryReadable"] = "Source texture は直接読み取りできません。可能な場合は一時的な読み取り可能コピーでプレビュー/出力します。",
            ["status.selectionReset"] = "選択範囲をリセットしました。",
            ["status.selection"] = "選択範囲: {0} x {1}",
            ["status.selectionTemporaryReadable"] = "選択範囲: {0} x {1}。一時的な読み取り可能コピーを使用しています。",
            ["status.exportCanceled"] = "出力をキャンセルしました。",
            ["status.exported"] = "出力しました: {0}",
            ["status.exportedTemporaryReadable"] = "一時的な読み取り可能コピーで出力しました: {0}",
            ["help.source.title"] = "Source / Crop",
            ["help.source.body"] = "Source画像を選択し、プレビュー上をドラッグして切り抜き範囲を作成します。既存の選択範囲の内側からドラッグすると、サイズを保ったまま位置を移動できます。",
            ["help.ratio.body"] = "Crop比率は選択範囲のアスペクト比です。Free は比率を固定せず、ドラッグした幅と高さをそのまま使います。Custom は任意の比率を指定します。",
            ["help.selection.body"] = "X/Y/W/H はSource画像上のピクセル座標です。数値入力でも選択範囲を調整できます。ズームとパンはプレビュー表示だけを変更し、出力座標には影響しません。",
            ["help.output.title"] = "Output",
            ["help.output.body"] = "Output比率とOutput長辺から出力PNGのキャンバスサイズを決定します。配置は選択範囲をキャンバスへ収める方法、競合は同名ファイルがある場合の処理です。",
            ["help.export.title"] = "Export",
            ["help.export.body"] = "PNG出力時は確認ダイアログでサイズ、選択範囲、保存先を確認できます。Assets配下へ出力した場合は生成アセットを選択して Ping します。Read/Write が無効なTextureは、一時コピーで処理できる場合があります。"
        };

        public static SquareCropDisplayLanguage ResolveLanguage(SquareCropLanguageMode mode)
        {
            switch (mode)
            {
                case SquareCropLanguageMode.Japanese:
                    return SquareCropDisplayLanguage.Japanese;
                case SquareCropLanguageMode.English:
                    return SquareCropDisplayLanguage.English;
                default:
                    return Application.systemLanguage == SystemLanguage.Japanese
                        ? SquareCropDisplayLanguage.Japanese
                        : SquareCropDisplayLanguage.English;
            }
        }

        public static string Get(SquareCropDisplayLanguage language, string key, string englishText)
        {
            if (language == SquareCropDisplayLanguage.Japanese
                && Japanese.TryGetValue(key, out var translated))
            {
                return translated;
            }

            return englishText;
        }

        public static string Format(SquareCropDisplayLanguage language, string key, string englishFormat, params object[] args)
        {
            return string.Format(Get(language, key, englishFormat), args);
        }

        public static string GetLanguageModeLabel(SquareCropDisplayLanguage language, SquareCropLanguageMode mode)
        {
            switch (mode)
            {
                case SquareCropLanguageMode.Auto:
                    return Get(language, "language.auto", "Auto");
                case SquareCropLanguageMode.Japanese:
                    return Get(language, "language.japanese", "Japanese");
                case SquareCropLanguageMode.English:
                    return Get(language, "language.english", "English");
                default:
                    return mode.ToString();
            }
        }
    }
}

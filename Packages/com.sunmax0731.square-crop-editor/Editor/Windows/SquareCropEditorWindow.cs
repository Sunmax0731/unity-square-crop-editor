using System;
using System.IO;
using System.Text;
using Sunmax0731.SquareCropEditor.Editor.Export;
using Sunmax0731.SquareCropEditor.Editor.Localization;
using Sunmax0731.SquareCropEditor.Models;
using Sunmax0731.SquareCropEditor.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Windows
{
    public sealed class SquareCropEditorWindow : EditorWindow
    {
        public const string WindowTitle = "Square Crop Editor";

        private const string ToolVersion = "0.2.0";
        private const float ControlPanelWidth = 300f;
        private const int MinPreviewHeight = 280;
        private const float PreviewScrollbarSize = 16f;
        private const string LanguageModePrefsKey = "Sunmax.SquareCropEditor.LanguageMode";
        private const string TermsOfUsePath = "Packages/com.sunmax0731.square-crop-editor/TermsOfUse.md";
        private static readonly Color SelectionColor = new Color(0.2f, 0.65f, 1f, 0.95f);
        private static readonly Color SelectionFillColor = new Color(0.2f, 0.65f, 1f, 0.16f);

        private Texture2D _sourceTexture;
        private Texture2D _outputPreview;
        private Texture2D _checkerboard;
        private CropSelection _selection;
        private SquareCropSettings _settings;
        private AspectPreset _cropPreset = AspectPreset.Square;
        private AspectPreset _outputPreset = AspectPreset.Square;
        private int _customCropWidth = 1;
        private int _customCropHeight = 1;
        private int _customOutputWidth = 1;
        private int _customOutputHeight = 1;
        private Vector2 _dragStartLocal;
        private Rect _dragImageRect;
        private CropSelection _dragStartSelection;
        private DragMode _dragMode = DragMode.None;
        private float _sourceZoom = 1f;
        private Vector2 _sourcePan;
        private bool _isDragging;
        private string _statusMessage;
        private MessageType _statusType = MessageType.Info;
        private SquareCropLanguageMode _languageMode = SquareCropLanguageMode.Auto;
        private SquareCropDisplayLanguage _displayLanguage = SquareCropDisplayLanguage.English;
        private DetachedOutputWindow _detachedOutputWindow;
        private ParameterHelpWindow _parameterHelpWindow;

        [MenuItem(SquareCropDefaults.MenuPath)]
        public static void Open()
        {
            var window = GetWindow<SquareCropEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        [MenuItem("Tools/Square Crop Editor/Open Output Preview")]
        public static void OpenOutputPreviewFromMenu()
        {
            var window = GetWindow<SquareCropEditorWindow>();
            window.OpenDetachedOutputWindow();
        }

        [MenuItem("Tools/Square Crop Editor/License")]
        public static void OpenLicense()
        {
            var termsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(TermsOfUsePath);
            if (termsAsset != null)
            {
                AssetDatabase.OpenAsset(termsAsset);
                return;
            }

            EditorUtility.DisplayDialog("Square Crop Editor License", "TermsOfUse.md was not found in the package.", "OK");
        }

        [MenuItem("Tools/Square Crop Editor/Version")]
        public static void ShowVersion()
        {
            EditorUtility.DisplayDialog("Square Crop Editor Version", $"Unity Square Crop Editor\nVersion: {ToolVersion}", "OK");
        }

        private void OnEnable()
        {
            minSize = new Vector2(900f, 560f);
            _settings = SquareCropDefaults.CreateSettings();
            if (string.IsNullOrEmpty(_settings.OutputFileName))
            {
                _settings.OutputFileName = "square_crop.png";
            }

            _checkerboard = CreateCheckerboardTexture();
            LoadLanguageMode();
            if (string.IsNullOrEmpty(_statusMessage))
            {
                SetStatus(T("status.selectSource", "Select a source texture."), MessageType.Info);
            }
        }

        private void OnDisable()
        {
            if (_detachedOutputWindow != null)
            {
                _detachedOutputWindow.SetOwner(null);
                _detachedOutputWindow = null;
            }

            if (_parameterHelpWindow != null)
            {
                _parameterHelpWindow.SetOwner(null);
                _parameterHelpWindow = null;
            }

            DestroyPreviewTexture();
            if (_checkerboard != null)
            {
                DestroyImmediate(_checkerboard);
                _checkerboard = null;
            }
        }

        private void OnGUI()
        {
            _settings ??= SquareCropDefaults.CreateSettings();

            DrawToolbar();
            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                DrawMainLayout();

                if (change.changed)
                {
                    RefreshOutputPreview();
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField(T("windowTitle", WindowTitle), EditorStyles.boldLabel, GUILayout.MinWidth(160));
                GUILayout.Space(8);
                EditorGUILayout.LabelField(T("sourceImage", "Source Image"), GUILayout.Width(90f));
                DrawSourceField();
                GUILayout.Space(8);
                using (new EditorGUI.DisabledScope(_sourceTexture == null || !_selection.IsValid))
                {
                    if (GUILayout.Button(T("openOutputWindow", "Output"), EditorStyles.toolbarButton, GUILayout.Width(82)))
                    {
                        OpenDetachedOutputWindow();
                    }

                    if (GUILayout.Button(T("exportPng", "Export PNG"), EditorStyles.toolbarButton, GUILayout.Width(86)))
                    {
                        ExportPng();
                    }
                }

                if (GUILayout.Button(T("help", "Help"), EditorStyles.toolbarButton, GUILayout.Width(56)))
                {
                    OpenHelpWindow();
                }

                DrawLanguagePopup();
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawLanguagePopup()
        {
            var modes = (SquareCropLanguageMode[])Enum.GetValues(typeof(SquareCropLanguageMode));
            var labels = new GUIContent[modes.Length];
            var selectedIndex = 0;
            for (var i = 0; i < modes.Length; i++)
            {
                labels[i] = new GUIContent(SquareCropLocalization.GetLanguageModeLabel(_displayLanguage, modes[i]));
                if (modes[i] == _languageMode)
                {
                    selectedIndex = i;
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                selectedIndex = EditorGUILayout.Popup(selectedIndex, labels, EditorStyles.toolbarPopup, GUILayout.Width(110));
                if (change.changed)
                {
                    _languageMode = modes[selectedIndex];
                    _displayLanguage = SquareCropLocalization.ResolveLanguage(_languageMode);
                    EditorPrefs.SetInt(LanguageModePrefsKey, (int)_languageMode);
                    titleContent = new GUIContent(T("windowTitle", WindowTitle));
                    Repaint();
                    _parameterHelpWindow?.Repaint();
                }
            }
        }

        private void DrawMainLayout()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawControlPanel();
                DrawSourcePreview();
            }
        }

        private void DrawControlPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(ControlPanelWidth)))
            {
                DrawSectionHeader(T("cropRatio", "Crop Ratio"));
                DrawAspectControls();
                DrawSectionSeparator();
                DrawSectionHeader(T("output", "Output"));
                DrawOutputControls();
                DrawSectionSeparator();
                DrawSectionHeader(T("selection", "Selection"));
                DrawSelectionActions();
                DrawSelectionFields();
            }
        }

        private void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void DrawSectionSeparator()
        {
            EditorGUILayout.Space(6);
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.25f, 0.25f, 0.25f, 0.8f));
            EditorGUILayout.Space(6);
        }

        private void DrawSourceField()
        {
            var texture = (Texture2D)EditorGUILayout.ObjectField(_sourceTexture, typeof(Texture2D), false, GUILayout.MinWidth(220), GUILayout.MaxWidth(440));
            if (texture != _sourceTexture)
            {
                _sourceTexture = texture;
                _selection = default;
                DestroyPreviewTexture();
                if (_sourceTexture != null)
                {
                    _settings.OutputFileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_sourceTexture)) + SquareCropSettings.DefaultFileNameSuffix + ".png";
                    if (TextureReadbackService.CanReadPixels(_sourceTexture))
                    {
                        SetStatus(TFormat("status.sourceReadable", "Source: {0} x {1}", _sourceTexture.width, _sourceTexture.height), MessageType.Info);
                    }
                    else
                    {
                        SetStatus(T("status.sourceTemporaryReadable", "Source texture is not directly readable. Preview/export will use a temporary readable copy when possible."), MessageType.Warning);
                    }
                }
                else
                {
                    SetStatus(T("status.selectSource", "Select a source texture."), MessageType.Info);
                }
            }
        }

        private void DrawAspectControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _cropPreset = (AspectPreset)EditorGUILayout.EnumPopup(T("cropRatio", "Crop Ratio"), _cropPreset);
                if (_cropPreset == AspectPreset.Custom)
                {
                    _customCropWidth = Mathf.Max(1, EditorGUILayout.IntField(_customCropWidth, GUILayout.Width(48)));
                    EditorGUILayout.LabelField(":", GUILayout.Width(8));
                    _customCropHeight = Mathf.Max(1, EditorGUILayout.IntField(_customCropHeight, GUILayout.Width(48)));
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _outputPreset = (AspectPreset)EditorGUILayout.EnumPopup(T("outputRatio", "Output Ratio"), _outputPreset);
                if (_outputPreset == AspectPreset.Custom)
                {
                    _customOutputWidth = Mathf.Max(1, EditorGUILayout.IntField(_customOutputWidth, GUILayout.Width(48)));
                    EditorGUILayout.LabelField(":", GUILayout.Width(8));
                    _customOutputHeight = Mathf.Max(1, EditorGUILayout.IntField(_customOutputHeight, GUILayout.Width(48)));
                }
            }
        }

        private void DrawOutputControls()
        {
            _settings.OutputSize = Mathf.Max(1, EditorGUILayout.IntField(T("outputLongEdge", "Output Long Edge"), _settings.OutputSize));
            _settings.MappingMode = (CanvasMappingMode)EditorGUILayout.EnumPopup(T("mapping", "Mapping"), _settings.MappingMode);
            _settings.ConflictBehavior = (ExportConflictBehavior)EditorGUILayout.EnumPopup(T("conflict", "Conflict"), _settings.ConflictBehavior);

            using (new EditorGUILayout.HorizontalScope())
            {
                _settings.OutputFolder = EditorGUILayout.TextField(T("outputFolder", "Output Folder"), _settings.OutputFolder);
                if (GUILayout.Button(T("select", "Select"), GUILayout.Width(64)))
                {
                    SelectOutputFolder();
                }

                if (GUILayout.Button(T("default", "Default"), GUILayout.Width(64)))
                {
                    _settings.OutputFolder = SquareCropSettings.DefaultOutputFolder;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _settings.OutputFileName = EditorGUILayout.TextField(T("outputFile", "Output File"), _settings.OutputFileName);
                if (GUILayout.Button(".png", GUILayout.Width(48)))
                {
                    _settings.OutputFileName = EnsurePngExtension(_settings.OutputFileName);
                }
            }
        }

        private void DrawSelectionActions()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    if (GUILayout.Button(T("resetSelection", "Reset Selection")))
                    {
                        _selection = default;
                        DestroyPreviewTexture();
                        SetStatus(T("status.selectionReset", "Selection reset."), MessageType.Info);
                    }

                    if (GUILayout.Button(T("useFullSource", "Use Full Source")))
                    {
                        _selection = CropRectCalculator.FullSource(new PixelSize(_sourceTexture.width, _sourceTexture.height));
                        RefreshOutputPreview();
                    }

                    if (GUILayout.Button(T("centerCrop", "Center Crop")))
                    {
                        var sourceSize = new PixelSize(_sourceTexture.width, _sourceTexture.height);
                        _selection = _cropPreset == AspectPreset.Free
                            ? CropRectCalculator.FullSource(sourceSize)
                            : CropRectCalculator.CenterCrop(
                                sourceSize,
                                GetAspectRatio(_cropPreset, _customCropWidth, _customCropHeight));
                        RefreshOutputPreview();
                    }
                }
            }
        }

        private void DrawSelectionFields()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null || !_selection.IsValid))
                {
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        int x;
                        int y;
                        int width;
                        int height;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            x = EditorGUILayout.IntField("X", _selection.IsValid ? _selection.X : 0);
                            y = EditorGUILayout.IntField("Y", _selection.IsValid ? _selection.Y : 0);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            width = EditorGUILayout.IntField("W", _selection.IsValid ? _selection.Width : 0);
                            height = EditorGUILayout.IntField("H", _selection.IsValid ? _selection.Height : 0);
                        }

                        if (change.changed)
                        {
                            var sourceSize = new PixelSize(_sourceTexture.width, _sourceTexture.height);
                            _selection = _cropPreset == AspectPreset.Free
                                ? CropRectCalculator.FromManualInput(x, y, width, height, sourceSize)
                                : CropRectCalculator.FromManualInput(
                                    x,
                                    y,
                                    width,
                                    height,
                                    sourceSize,
                                    GetAspectRatio(_cropPreset, _customCropWidth, _customCropHeight));
                            RefreshOutputPreview();
                        }
                    }
                }
            }
        }

        private void DrawSourcePreview()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(360f), GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField(T("source", "Source"), EditorStyles.boldLabel);
                DrawSourceViewControls();
                var previewRect = GUILayoutUtility.GetRect(10, 10000, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));

                if (_sourceTexture == null)
                {
                    DrawCenteredLabel(previewRect, T("noSourceImage", "No source image"));
                    return;
                }

                var viewRect = GetScrollableSourceViewRect(previewRect, _sourceTexture.width, _sourceTexture.height, _sourceZoom);
                EditorGUI.DrawRect(viewRect, new Color(0.13f, 0.13f, 0.13f));
                var imageRect = GetScrolledImageRect(viewRect, _sourceTexture.width, _sourceTexture.height, _sourceZoom, _sourcePan);
                GUI.BeginClip(viewRect);
                var localImageRect = new Rect(
                    imageRect.x - viewRect.x,
                    imageRect.y - viewRect.y,
                    imageRect.width,
                    imageRect.height);
                GUI.DrawTexture(localImageRect, _sourceTexture, ScaleMode.StretchToFill, true);
                DrawSelection(localImageRect);
                GUI.EndClip();
                DrawSourceScrollbars(previewRect, viewRect, imageRect);
                HandleDragSelection(imageRect, viewRect);
            }
        }

        private void DrawSourceViewControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    _sourceZoom = EditorGUILayout.Slider(T("zoom", "Zoom"), _sourceZoom, 1f, 8f);
                    if (GUILayout.Button("1:1", GUILayout.Width(44)))
                    {
                        _sourceZoom = 1f;
                        _sourcePan = Vector2.zero;
                    }
                }
            }

            if (_sourceZoom <= 1f)
            {
                _sourcePan = Vector2.zero;
            }
        }

        private void DrawOutputPreviewContent()
        {
            EditorGUILayout.LabelField(T("output", "Output"), EditorStyles.boldLabel);
            var previewRect = GUILayoutUtility.GetRect(260, 10000, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));

            if (_outputPreview == null)
            {
                DrawCenteredLabel(previewRect, T("noPreview", "No preview"));
                return;
            }

            var imageRect = FitRect(previewRect, _outputPreview.width, _outputPreview.height);
            GUI.DrawTextureWithTexCoords(imageRect, _checkerboard, new Rect(0, 0, imageRect.width / _checkerboard.width, imageRect.height / _checkerboard.height));
            GUI.DrawTexture(imageRect, _outputPreview, ScaleMode.StretchToFill, true);
            EditorGUILayout.LabelField($"{_outputPreview.width} x {_outputPreview.height}");
        }

        private void HandleDragSelection(Rect imageRect, Rect previewRect)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && previewRect.Contains(currentEvent.mousePosition) && imageRect.Contains(currentEvent.mousePosition))
            {
                _dragImageRect = imageRect;
                _dragStartLocal = ClampLocalPoint(currentEvent.mousePosition - _dragImageRect.position, _dragImageRect);
                _dragStartSelection = _selection;
                _dragMode = IsMouseOverSelection(currentEvent.mousePosition, imageRect) ? DragMode.Move : DragMode.Create;
                _isDragging = true;
                currentEvent.Use();
            }

            if (_isDragging && (currentEvent.type == EventType.MouseDrag || currentEvent.type == EventType.MouseUp))
            {
                var clampedMousePosition = ClampPointToRect(currentEvent.mousePosition, previewRect);
                var localStart = ClampLocalPoint(_dragStartLocal, _dragImageRect);
                var localEnd = ClampLocalPoint(clampedMousePosition - _dragImageRect.position, _dragImageRect);
                var sourceSize = new PixelSize(_sourceTexture.width, _sourceTexture.height);
                if (_dragMode == DragMode.Move && _dragStartSelection.IsValid)
                {
                    var delta = localEnd - localStart;
                    _selection = CropRectCalculator.MoveSelection(
                        _dragStartSelection,
                        sourceSize,
                        delta.x * _sourceTexture.width / _dragImageRect.width,
                        delta.y * _sourceTexture.height / _dragImageRect.height);
                }
                else
                {
                    var previewSize = new PixelSize(Mathf.RoundToInt(_dragImageRect.width), Mathf.RoundToInt(_dragImageRect.height));
                    _selection = _cropPreset == AspectPreset.Free
                        ? CropRectCalculator.FromPreviewDrag(
                            localStart.x,
                            localStart.y,
                            localEnd.x,
                            localEnd.y,
                            previewSize,
                            sourceSize)
                        : CropRectCalculator.FromPreviewDrag(
                            localStart.x,
                            localStart.y,
                            localEnd.x,
                            localEnd.y,
                            previewSize,
                            sourceSize,
                            GetAspectRatio(_cropPreset, _customCropWidth, _customCropHeight));
                }
                RefreshOutputPreview();
                Repaint();
                currentEvent.Use();

                if (currentEvent.type == EventType.MouseUp)
                {
                    _isDragging = false;
                    _dragMode = DragMode.None;
                }
            }
        }

        private bool IsMouseOverSelection(Vector2 mousePosition, Rect imageRect)
        {
            if (!_selection.IsValid || _sourceTexture == null)
            {
                return false;
            }

            return GetSelectionRect(imageRect).Contains(mousePosition);
        }

        private static Vector2 ClampPointToRect(Vector2 point, Rect rect)
        {
            return new Vector2(
                Mathf.Clamp(point.x, rect.xMin, rect.xMax),
                Mathf.Clamp(point.y, rect.yMin, rect.yMax));
        }

        private static Vector2 ClampLocalPoint(Vector2 point, Rect imageRect)
        {
            return new Vector2(
                Mathf.Clamp(point.x, 0f, imageRect.width),
                Mathf.Clamp(point.y, 0f, imageRect.height));
        }

        private void DrawSelection(Rect imageRect)
        {
            if (!_selection.IsValid || _sourceTexture == null)
            {
                return;
            }

            var x = imageRect.x + imageRect.width * _selection.X / _sourceTexture.width;
            var y = imageRect.y + imageRect.height * _selection.Y / _sourceTexture.height;
            var width = imageRect.width * _selection.Width / _sourceTexture.width;
            var height = imageRect.height * _selection.Height / _sourceTexture.height;
            var rect = new Rect(x, y, width, height);

            EditorGUI.DrawRect(rect, SelectionFillColor);
            Handles.BeginGUI();
            Handles.color = SelectionColor;
            Handles.DrawAAPolyLine(3f, new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMin, rect.yMin));
            Handles.EndGUI();
        }

        private Rect GetSelectionRect(Rect imageRect)
        {
            var x = imageRect.x + imageRect.width * _selection.X / _sourceTexture.width;
            var y = imageRect.y + imageRect.height * _selection.Y / _sourceTexture.height;
            var width = imageRect.width * _selection.Width / _sourceTexture.width;
            var height = imageRect.height * _selection.Height / _sourceTexture.height;
            return new Rect(x, y, width, height);
        }

        private void RefreshOutputPreview()
        {
            DestroyPreviewTexture();

            if (_sourceTexture == null || !_selection.IsValid)
            {
                return;
            }

            try
            {
                var readable = TextureReadbackService.GetReadableTexture(_sourceTexture);
                if (!readable.Success)
                {
                    SetStatus(readable.Message, MessageType.Error);
                    return;
                }

                try
                {
                    var outputSize = AspectOutputPlanner.CalculateOutputSize(_settings.OutputSize, GetAspectRatio(_outputPreset, _customOutputWidth, _customOutputHeight));
                    var plan = AspectOutputPlanner.Plan(_selection, outputSize, _settings.MappingMode);
                    _outputPreview = PngAspectExporter.Render(readable.Texture, plan);
                    SetStatus(readable.OwnsTexture
                        ? TFormat("status.selectionTemporaryReadable", "Selection: {0} x {1}. Using temporary readable copy.", _selection.Width, _selection.Height)
                        : TFormat("status.selection", "Selection: {0} x {1}", _selection.Width, _selection.Height), readable.OwnsTexture ? MessageType.Warning : MessageType.Info);
                    _detachedOutputWindow?.Repaint();
                }
                finally
                {
                    TextureReadbackService.DestroyIfOwned(readable);
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, MessageType.Error);
            }
        }

        private void ExportPng()
        {
            if (!ConfirmExport())
            {
                SetStatus(T("status.exportCanceled", "Export canceled."), MessageType.Info);
                return;
            }

            var readable = TextureReadbackService.GetReadableTexture(_sourceTexture);
            if (!readable.Success)
            {
                SetStatus(readable.Message, MessageType.Error);
                return;
            }

            try
            {
                var result = PngAspectExporter.Export(new PngExportRequest
                {
                    SourceTexture = readable.Texture,
                    Selection = _selection,
                    OutputLongEdge = _settings.OutputSize,
                    OutputAspectRatio = GetAspectRatio(_outputPreset, _customOutputWidth, _customOutputHeight),
                    MappingMode = _settings.MappingMode,
                    OutputFolder = _settings.OutputFolder,
                    OutputFileName = _settings.OutputFileName,
                    ConflictBehavior = _settings.ConflictBehavior
                });

                if (result.Status == PngExportStatus.Exported)
                {
                    SelectOutputAssetIfNeeded(result.OutputPath);
                    SetStatus(readable.OwnsTexture
                        ? TFormat("status.exportedTemporaryReadable", "Exported with temporary readable copy: {0}", result.OutputPath)
                        : TFormat("status.exported", "Exported: {0}", result.OutputPath), readable.OwnsTexture ? MessageType.Warning : MessageType.Info);
                }
                else if (result.Status == PngExportStatus.Skipped)
                {
                    SetStatus(result.Message, MessageType.Warning);
                }
                else
                {
                    SetStatus(result.Message, MessageType.Error);
                }
            }
            finally
            {
                TextureReadbackService.DestroyIfOwned(readable);
            }
        }

        private bool ConfirmExport()
        {
            var outputSize = AspectOutputPlanner.CalculateOutputSize(
                _settings.OutputSize,
                GetAspectRatio(_outputPreset, _customOutputWidth, _customOutputHeight));
            var outputPath = ResolveOutputPathForDisplay(_settings.OutputFolder, _settings.OutputFileName);
            var message = new StringBuilder()
                .AppendLine($"{T("dialog.output", "Output")}: {outputSize.Width} x {outputSize.Height}")
                .AppendLine($"{T("dialog.selection", "Selection")}: {_selection.X}, {_selection.Y}, {_selection.Width} x {_selection.Height}")
                .AppendLine($"{T("dialog.mapping", "Mapping")}: {_settings.MappingMode}")
                .AppendLine($"{T("dialog.conflict", "Conflict")}: {_settings.ConflictBehavior}")
                .AppendLine()
                .AppendLine(outputPath)
                .ToString();

            return EditorUtility.DisplayDialog(T("confirmPngExport", "Confirm PNG Export"), message, T("export", "Export"), T("cancel", "Cancel"));
        }

        private static string ResolveOutputPathForDisplay(string outputFolder, string outputFileName)
        {
            var fileName = Path.GetExtension(outputFileName).Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? outputFileName
                : outputFileName + ".png";
            return Path.GetFullPath(Path.Combine(outputFolder, fileName));
        }

        private static AspectRatioSpec GetAspectRatio(AspectPreset preset, int customWidth, int customHeight)
        {
            switch (preset)
            {
                case AspectPreset.Square:
                    return AspectRatioSpec.Square;
                case AspectPreset.Landscape4By3:
                    return AspectRatioSpec.Landscape4By3;
                case AspectPreset.Portrait3By4:
                    return AspectRatioSpec.Portrait3By4;
                case AspectPreset.Landscape16By9:
                    return AspectRatioSpec.Landscape16By9;
                case AspectPreset.Portrait9By16:
                    return AspectRatioSpec.Portrait9By16;
                case AspectPreset.Custom:
                    return new AspectRatioSpec(Mathf.Max(1, customWidth), Mathf.Max(1, customHeight));
                case AspectPreset.Free:
                    return AspectRatioSpec.Square;
                default:
                    return AspectRatioSpec.Square;
            }
        }

        private static Rect FitRect(Rect container, float width, float height)
        {
            var scale = Mathf.Min(container.width / width, container.height / height);
            var fittedWidth = width * scale;
            var fittedHeight = height * scale;
            return new Rect(
                container.x + (container.width - fittedWidth) * 0.5f,
                container.y + (container.height - fittedHeight) * 0.5f,
                fittedWidth,
                fittedHeight);
        }

        private static Rect GetScrollableSourceViewRect(Rect previewRect, float sourceWidth, float sourceHeight, float zoom)
        {
            var viewRect = previewRect;
            for (var index = 0; index < 2; index++)
            {
                var fittedRect = FitRect(viewRect, sourceWidth, sourceHeight);
                var zoomedWidth = fittedRect.width * Mathf.Max(1f, zoom);
                var zoomedHeight = fittedRect.height * Mathf.Max(1f, zoom);
                var needsHorizontal = zoomedWidth > viewRect.width + 0.5f;
                var needsVertical = zoomedHeight > viewRect.height + 0.5f;
                viewRect = new Rect(
                    previewRect.x,
                    previewRect.y,
                    previewRect.width - (needsVertical ? PreviewScrollbarSize : 0f),
                    previewRect.height - (needsHorizontal ? PreviewScrollbarSize : 0f));
            }

            return viewRect;
        }

        private static Rect GetScrolledImageRect(Rect viewRect, float sourceWidth, float sourceHeight, float zoom, Vector2 pan)
        {
            var fittedRect = FitRect(viewRect, sourceWidth, sourceHeight);
            zoom = Mathf.Max(1f, zoom);
            var imageWidth = fittedRect.width * zoom;
            var imageHeight = fittedRect.height * zoom;
            var maxOffsetX = Mathf.Max(0f, imageWidth - viewRect.width);
            var maxOffsetY = Mathf.Max(0f, imageHeight - viewRect.height);
            var offsetX = PanToScrollOffset(pan.x, maxOffsetX);
            var offsetY = PanToScrollOffset(pan.y, maxOffsetY);
            return new Rect(
                viewRect.x - offsetX,
                viewRect.y - offsetY,
                imageWidth,
                imageHeight);
        }

        private void DrawSourceScrollbars(Rect previewRect, Rect viewRect, Rect imageRect)
        {
            var maxOffsetX = Mathf.Max(0f, imageRect.width - viewRect.width);
            var maxOffsetY = Mathf.Max(0f, imageRect.height - viewRect.height);
            var needsHorizontal = maxOffsetX > 0.5f;
            var needsVertical = maxOffsetY > 0.5f;

            if (needsHorizontal)
            {
                var scrollbarRect = new Rect(
                    viewRect.x,
                    viewRect.yMax,
                    viewRect.width,
                    PreviewScrollbarSize);
                var offsetX = PanToScrollOffset(_sourcePan.x, maxOffsetX);
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    offsetX = GUI.HorizontalScrollbar(scrollbarRect, offsetX, viewRect.width, 0f, imageRect.width);
                    if (change.changed)
                    {
                        _sourcePan.x = ScrollOffsetToPan(offsetX, maxOffsetX);
                    }
                }
            }
            else
            {
                _sourcePan.x = 0f;
            }

            if (needsVertical)
            {
                var scrollbarRect = new Rect(
                    viewRect.xMax,
                    viewRect.y,
                    PreviewScrollbarSize,
                    viewRect.height);
                var offsetY = PanToScrollOffset(_sourcePan.y, maxOffsetY);
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    offsetY = GUI.VerticalScrollbar(scrollbarRect, offsetY, viewRect.height, 0f, imageRect.height);
                    if (change.changed)
                    {
                        _sourcePan.y = ScrollOffsetToPan(offsetY, maxOffsetY);
                    }
                }
            }
            else
            {
                _sourcePan.y = 0f;
            }

            if (needsHorizontal && needsVertical)
            {
                EditorGUI.DrawRect(new Rect(viewRect.xMax, viewRect.yMax, previewRect.xMax - viewRect.xMax, previewRect.yMax - viewRect.yMax), new Color(0.13f, 0.13f, 0.13f));
            }
        }

        private static float PanToScrollOffset(float pan, float maxOffset)
        {
            return maxOffset <= 0f
                ? 0f
                : Mathf.Clamp01((Mathf.Clamp(pan, -1f, 1f) + 1f) * 0.5f) * maxOffset;
        }

        private static float ScrollOffsetToPan(float offset, float maxOffset)
        {
            return maxOffset <= 0f
                ? 0f
                : Mathf.Clamp01(offset / maxOffset) * 2f - 1f;
        }

        private static void DrawCenteredLabel(Rect rect, string text)
        {
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(rect, text, style);
        }

        private static Texture2D CreateCheckerboardTexture()
        {
            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point
            };
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var light = ((x / 4) + (y / 4)) % 2 == 0;
                    texture.SetPixel(x, y, light ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.48f, 0.48f, 0.48f));
                }
            }

            texture.Apply();
            return texture;
        }

        private void SelectOutputFolder()
        {
            var currentFolder = ResolveOutputFolderForPanel(_settings.OutputFolder);
            var selectedFolder = EditorUtility.OpenFolderPanel(T("outputFolder", "Output Folder"), currentFolder, string.Empty);
            if (string.IsNullOrEmpty(selectedFolder))
            {
                return;
            }

            _settings.OutputFolder = ToProjectRelativePath(selectedFolder);
        }

        private static string ResolveOutputFolderForPanel(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                return Application.dataPath;
            }

            if (Path.IsPathRooted(outputFolder))
            {
                return outputFolder;
            }

            return Path.GetFullPath(outputFolder);
        }

        private static string ToProjectRelativePath(string folderPath)
        {
            var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var fullFolderPath = Path.GetFullPath(folderPath);
            var relativePath = Path.GetRelativePath(projectPath, fullFolderPath);
            if (!relativePath.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relativePath))
            {
                return relativePath.Replace('\\', '/');
            }

            return fullFolderPath;
        }

        private static string EnsurePngExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "square_crop.png";
            }

            return Path.GetExtension(fileName).Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".png";
        }

        private static void SelectOutputAssetIfNeeded(string outputPath)
        {
            var assetsPath = Path.GetFullPath("Assets");
            var fullOutputPath = Path.GetFullPath(outputPath);
            if (!fullOutputPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            AssetDatabase.Refresh();
            var assetPath = "Assets" + fullOutputPath.Substring(assetsPath.Length).Replace('\\', '/');
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void DestroyPreviewTexture()
        {
            if (_outputPreview != null)
            {
                DestroyImmediate(_outputPreview);
                _outputPreview = null;
                _detachedOutputWindow?.Repaint();
            }
        }

        private void SetStatus(string message, MessageType type)
        {
            _statusMessage = message;
            _statusType = type;
        }

        private void LoadLanguageMode()
        {
            var modeValue = EditorPrefs.GetInt(LanguageModePrefsKey, (int)SquareCropLanguageMode.Auto);
            _languageMode = Enum.IsDefined(typeof(SquareCropLanguageMode), modeValue)
                ? (SquareCropLanguageMode)modeValue
                : SquareCropLanguageMode.Auto;
            _displayLanguage = SquareCropLocalization.ResolveLanguage(_languageMode);
            titleContent = new GUIContent(T("windowTitle", WindowTitle));
        }

        private void OpenHelpWindow()
        {
            if (_parameterHelpWindow == null)
            {
                _parameterHelpWindow = GetWindow<ParameterHelpWindow>();
            }

            _parameterHelpWindow.SetOwner(this);
            _parameterHelpWindow.titleContent = new GUIContent(T("parameterHelp", "Parameter Help"));
            _parameterHelpWindow.Show();
            _parameterHelpWindow.Focus();
        }

        private void OpenDetachedOutputWindow()
        {
            if (_detachedOutputWindow == null)
            {
                _detachedOutputWindow = GetWindow<DetachedOutputWindow>();
            }

            _detachedOutputWindow.SetOwner(this);
            _detachedOutputWindow.titleContent = new GUIContent(T("outputPreview", "Output Preview"));
            _detachedOutputWindow.Show();
            _detachedOutputWindow.Focus();
        }

        private string T(string key, string englishText)
        {
            return SquareCropLocalization.Get(_displayLanguage, key, englishText);
        }

        private string TFormat(string key, string englishFormat, params object[] args)
        {
            return SquareCropLocalization.Format(_displayLanguage, key, englishFormat, args);
        }

        private sealed class ParameterHelpWindow : EditorWindow
        {
            private SquareCropEditorWindow _owner;
            private Vector2 _scroll;

            public void SetOwner(SquareCropEditorWindow owner)
            {
                _owner = owner;
                titleContent = new GUIContent(owner != null ? owner.T("parameterHelp", "Parameter Help") : "Parameter Help");
                minSize = new Vector2(360f, 260f);
            }

            private void OnGUI()
            {
                if (_owner == null)
                {
                    EditorGUILayout.HelpBox("Open Tools > Square Crop Editor > Open to reconnect this help window.", MessageType.Info);
                    return;
                }

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                DrawHelpSection(_owner.T("help.source.title", "Source / Crop"), _owner.T("help.source.body", "Choose a source image, then drag on the preview to create a crop selection. Drag from inside the existing selection to move it while keeping its size."));
                DrawHelpSection(_owner.T("cropRatio", "Crop Ratio"), _owner.T("help.ratio.body", "Crop Ratio controls the selection aspect. Free keeps the dragged width and height unconstrained. Custom uses the entered ratio."));
                DrawHelpSection(_owner.T("selection", "Selection"), _owner.T("help.selection.body", "X/Y/W/H are source-image pixel coordinates. You can edit them numerically. Zoom and pan affect only the preview view, not exported coordinates."));
                DrawHelpSection(_owner.T("help.output.title", "Output"), _owner.T("help.output.body", "Output Ratio and Output Long Edge determine the PNG canvas size. Mapping controls how the selection fits the canvas. Conflict controls behavior when the output file already exists."));
                DrawHelpSection(_owner.T("help.export.title", "Export"), _owner.T("help.export.body", "Export PNG shows a confirmation dialog with size, selection, and destination. Assets output is selected and pinged after export. Textures with Read/Write disabled may be processed through a temporary readable copy."));
                EditorGUILayout.EndScrollView();
            }

            private static void DrawHelpSection(string title, string body)
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(body, MessageType.None);
                EditorGUILayout.Space(4);
            }
        }

        private sealed class DetachedOutputWindow : EditorWindow
        {
            private SquareCropEditorWindow _owner;

            public void SetOwner(SquareCropEditorWindow owner)
            {
                _owner = owner;
                titleContent = new GUIContent(owner != null ? owner.T("outputPreview", "Output Preview") : "Output Preview");
                minSize = new Vector2(320f, 320f);
            }

            private void OnGUI()
            {
                if (_owner == null)
                {
                    var language = SquareCropLocalization.ResolveLanguage(SquareCropLanguageMode.Auto);
                    EditorGUILayout.HelpBox(SquareCropLocalization.Get(language, "detachedOutputReconnect", "Open Tools > Square Crop Editor > Open again to reconnect the output preview."), MessageType.Info);
                    return;
                }

                _owner.DrawOutputPreviewContent();
            }
        }

        private enum AspectPreset
        {
            Free,
            Square,
            Landscape4By3,
            Portrait3By4,
            Landscape16By9,
            Portrait9By16,
            Custom
        }

        private enum DragMode
        {
            None,
            Create,
            Move
        }
    }
}

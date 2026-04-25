using System;
using System.IO;
using System.Text;
using Sunmax0731.SquareCropEditor.Editor.Export;
using Sunmax0731.SquareCropEditor.Models;
using Sunmax0731.SquareCropEditor.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Windows
{
    public sealed class SquareCropEditorWindow : EditorWindow
    {
        public const string WindowTitle = "Square Crop Editor";

        private const int MinPreviewHeight = 280;
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
        private string _statusMessage = "Select a source texture.";
        private MessageType _statusType = MessageType.Info;

        [MenuItem(SquareCropDefaults.MenuPath)]
        public static void Open()
        {
            var window = GetWindow<SquareCropEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            _settings = SquareCropDefaults.CreateSettings();
            if (string.IsNullOrEmpty(_settings.OutputFileName))
            {
                _settings.OutputFileName = "square_crop.png";
            }

            _checkerboard = CreateCheckerboardTexture();
        }

        private void OnDisable()
        {
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

            EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_statusMessage, _statusType);

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                DrawSourceControls();
                DrawAspectControls();
                DrawOutputControls();

                if (change.changed)
                {
                    RefreshOutputPreview();
                }
            }

            DrawSelectionActions();
            DrawSelectionFields();
            DrawPreviewArea();
            DrawExportButton();
        }

        private void DrawSourceControls()
        {
            EditorGUILayout.Space(4);
            var texture = (Texture2D)EditorGUILayout.ObjectField("Source Image", _sourceTexture, typeof(Texture2D), false);
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
                        SetStatus($"Source: {_sourceTexture.width} x {_sourceTexture.height}", MessageType.Info);
                    }
                    else
                    {
                        SetStatus("Source texture is not directly readable. Preview/export will use a temporary readable copy when possible.", MessageType.Warning);
                    }
                }
                else
                {
                    SetStatus("Select a source texture.", MessageType.Info);
                }
            }
        }

        private void DrawAspectControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _cropPreset = (AspectPreset)EditorGUILayout.EnumPopup("Crop Ratio", _cropPreset);
                if (_cropPreset == AspectPreset.Custom)
                {
                    _customCropWidth = Mathf.Max(1, EditorGUILayout.IntField(_customCropWidth, GUILayout.Width(48)));
                    EditorGUILayout.LabelField(":", GUILayout.Width(8));
                    _customCropHeight = Mathf.Max(1, EditorGUILayout.IntField(_customCropHeight, GUILayout.Width(48)));
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _outputPreset = (AspectPreset)EditorGUILayout.EnumPopup("Output Ratio", _outputPreset);
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
            _settings.OutputSize = Mathf.Max(1, EditorGUILayout.IntField("Output Long Edge", _settings.OutputSize));
            _settings.MappingMode = (CanvasMappingMode)EditorGUILayout.EnumPopup("Mapping", _settings.MappingMode);
            _settings.ConflictBehavior = (ExportConflictBehavior)EditorGUILayout.EnumPopup("Conflict", _settings.ConflictBehavior);

            using (new EditorGUILayout.HorizontalScope())
            {
                _settings.OutputFolder = EditorGUILayout.TextField("Output Folder", _settings.OutputFolder);
                if (GUILayout.Button("Select", GUILayout.Width(64)))
                {
                    SelectOutputFolder();
                }

                if (GUILayout.Button("Default", GUILayout.Width(64)))
                {
                    _settings.OutputFolder = SquareCropSettings.DefaultOutputFolder;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _settings.OutputFileName = EditorGUILayout.TextField("Output File", _settings.OutputFileName);
                if (GUILayout.Button(".png", GUILayout.Width(48)))
                {
                    _settings.OutputFileName = EnsurePngExtension(_settings.OutputFileName);
                }
            }
        }

        private void DrawSelectionActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    if (GUILayout.Button("Reset Selection"))
                    {
                        _selection = default;
                        DestroyPreviewTexture();
                        SetStatus("Selection reset.", MessageType.Info);
                    }

                    if (GUILayout.Button("Use Full Source"))
                    {
                        _selection = CropRectCalculator.FullSource(new PixelSize(_sourceTexture.width, _sourceTexture.height));
                        RefreshOutputPreview();
                    }

                    if (GUILayout.Button("Center Crop"))
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
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null || !_selection.IsValid))
                {
                    EditorGUILayout.LabelField("Selection", GUILayout.Width(64));
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var x = EditorGUILayout.IntField("X", _selection.IsValid ? _selection.X : 0, GUILayout.MaxWidth(96));
                        var y = EditorGUILayout.IntField("Y", _selection.IsValid ? _selection.Y : 0, GUILayout.MaxWidth(96));
                        var width = EditorGUILayout.IntField("W", _selection.IsValid ? _selection.Width : 0, GUILayout.MaxWidth(96));
                        var height = EditorGUILayout.IntField("H", _selection.IsValid ? _selection.Height : 0, GUILayout.MaxWidth(96));

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

        private void DrawPreviewArea()
        {
            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSourcePreview();
                DrawOutputPreview();
            }
        }

        private void DrawSourcePreview()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(position.width * 0.58f)))
            {
                EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
                DrawSourceViewControls();
                var previewRect = GUILayoutUtility.GetRect(10, 10000, MinPreviewHeight, MinPreviewHeight, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));

                if (_sourceTexture == null)
                {
                    DrawCenteredLabel(previewRect, "No source image");
                    return;
                }

                var fittedRect = FitRect(previewRect, _sourceTexture.width, _sourceTexture.height);
                var imageRect = ApplyZoomAndPan(previewRect, fittedRect, _sourceZoom, _sourcePan);
                GUI.BeginClip(previewRect);
                var localImageRect = new Rect(
                    imageRect.x - previewRect.x,
                    imageRect.y - previewRect.y,
                    imageRect.width,
                    imageRect.height);
                GUI.DrawTexture(localImageRect, _sourceTexture, ScaleMode.StretchToFill, true);
                DrawSelection(localImageRect);
                GUI.EndClip();
                HandleDragSelection(imageRect, previewRect);
            }
        }

        private void DrawSourceViewControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null))
                {
                    _sourceZoom = EditorGUILayout.Slider("Zoom", _sourceZoom, 1f, 8f);
                    if (GUILayout.Button("1:1", GUILayout.Width(44)))
                    {
                        _sourceZoom = 1f;
                        _sourcePan = Vector2.zero;
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_sourceTexture == null || _sourceZoom <= 1f))
                {
                    _sourcePan.x = EditorGUILayout.Slider("Pan X", _sourcePan.x, -1f, 1f);
                    _sourcePan.y = EditorGUILayout.Slider("Pan Y", _sourcePan.y, -1f, 1f);
                }
            }

            if (_sourceZoom <= 1f)
            {
                _sourcePan = Vector2.zero;
            }
        }

        private void DrawOutputPreview()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Max(260, position.width * 0.32f))))
            {
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                var previewRect = GUILayoutUtility.GetRect(240, MinPreviewHeight, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));

                if (_outputPreview == null)
                {
                    DrawCenteredLabel(previewRect, "No preview");
                    return;
                }

                var imageRect = FitRect(previewRect, _outputPreview.width, _outputPreview.height);
                GUI.DrawTextureWithTexCoords(imageRect, _checkerboard, new Rect(0, 0, imageRect.width / _checkerboard.width, imageRect.height / _checkerboard.height));
                GUI.DrawTexture(imageRect, _outputPreview, ScaleMode.StretchToFill, true);
                EditorGUILayout.LabelField($"{_outputPreview.width} x {_outputPreview.height}");
            }
        }

        private void DrawExportButton()
        {
            using (new EditorGUI.DisabledScope(_sourceTexture == null || !_selection.IsValid))
            {
                if (GUILayout.Button("Export PNG", GUILayout.Height(28)))
                {
                    ExportPng();
                }
            }
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
                    SetStatus(readable.OwnsTexture ? $"Selection: {_selection.Width} x {_selection.Height}. Using temporary readable copy." : $"Selection: {_selection.Width} x {_selection.Height}", readable.OwnsTexture ? MessageType.Warning : MessageType.Info);
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
                SetStatus("Export canceled.", MessageType.Info);
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
                    SetStatus(readable.OwnsTexture ? $"Exported with temporary readable copy: {result.OutputPath}" : $"Exported: {result.OutputPath}", readable.OwnsTexture ? MessageType.Warning : MessageType.Info);
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
                .AppendLine($"Output: {outputSize.Width} x {outputSize.Height}")
                .AppendLine($"Selection: {_selection.X}, {_selection.Y}, {_selection.Width} x {_selection.Height}")
                .AppendLine($"Mapping: {_settings.MappingMode}")
                .AppendLine($"Conflict: {_settings.ConflictBehavior}")
                .AppendLine()
                .AppendLine(outputPath)
                .ToString();

            return EditorUtility.DisplayDialog("Confirm PNG Export", message, "Export", "Cancel");
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

        private static Rect ApplyZoomAndPan(Rect container, Rect fittedRect, float zoom, Vector2 pan)
        {
            zoom = Mathf.Max(1f, zoom);
            var width = fittedRect.width * zoom;
            var height = fittedRect.height * zoom;
            var maxPanX = Mathf.Max(0f, (width - container.width) * 0.5f);
            var maxPanY = Mathf.Max(0f, (height - container.height) * 0.5f);
            return new Rect(
                container.center.x - width * 0.5f + Mathf.Clamp(pan.x, -1f, 1f) * maxPanX,
                container.center.y - height * 0.5f + Mathf.Clamp(pan.y, -1f, 1f) * maxPanY,
                width,
                height);
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
            var selectedFolder = EditorUtility.OpenFolderPanel("Select Output Folder", currentFolder, string.Empty);
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
            }
        }

        private void SetStatus(string message, MessageType type)
        {
            _statusMessage = message;
            _statusType = type;
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

using System;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public sealed class PngExportRequest
    {
        public Texture2D SourceTexture { get; set; }

        public CropSelection Selection { get; set; }

        public int OutputLongEdge { get; set; } = SquareCropSettings.DefaultOutputSize;

        public AspectRatioSpec OutputAspectRatio { get; set; } = AspectRatioSpec.Square;

        public CanvasMappingMode MappingMode { get; set; } = CanvasMappingMode.Fit;

        public string OutputFolder { get; set; } = SquareCropSettings.DefaultOutputFolder;

        public string OutputFileName { get; set; } = string.Empty;

        public ExportConflictBehavior ConflictBehavior { get; set; } = ExportConflictBehavior.Duplicate;
    }
}

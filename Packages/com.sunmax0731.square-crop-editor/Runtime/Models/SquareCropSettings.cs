using System;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public sealed class SquareCropSettings
    {
        public const int DefaultOutputSize = 256;
        public const string DefaultOutputFolder = "Assets/Generated/SquareCrop";
        public const string DefaultFileNameSuffix = "_crop";

        public int OutputSize { get; set; } = DefaultOutputSize;

        public SquareConversionMode ConversionMode { get; set; } = SquareConversionMode.Fit;

        public string OutputFolder { get; set; } = DefaultOutputFolder;

        public string OutputFileName { get; set; } = string.Empty;

        public ExportConflictBehavior ConflictBehavior { get; set; } = ExportConflictBehavior.Duplicate;
    }
}

using System;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public readonly struct PngExportResult
    {
        public PngExportResult(PngExportStatus status, string outputPath, PixelSize outputSize, string message)
        {
            Status = status;
            OutputPath = outputPath;
            OutputSize = outputSize;
            Message = message;
        }

        public PngExportStatus Status { get; }

        public string OutputPath { get; }

        public PixelSize OutputSize { get; }

        public string Message { get; }
    }
}

using System;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public readonly struct OutputMappingPlan
    {
        public OutputMappingPlan(PixelSize outputSize, CropSelection sourceRect, CropSelection destinationRect, CanvasMappingMode mappingMode)
        {
            OutputSize = outputSize;
            SourceRect = sourceRect;
            DestinationRect = destinationRect;
            MappingMode = mappingMode;
        }

        public PixelSize OutputSize { get; }

        public CropSelection SourceRect { get; }

        public CropSelection DestinationRect { get; }

        public CanvasMappingMode MappingMode { get; }

        public bool IsValid => OutputSize.IsValid && SourceRect.IsValid && DestinationRect.IsValid;
    }
}

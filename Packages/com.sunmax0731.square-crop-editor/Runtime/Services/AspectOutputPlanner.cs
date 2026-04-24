using System;
using Sunmax0731.SquareCropEditor.Models;

namespace Sunmax0731.SquareCropEditor.Services
{
    public static class AspectOutputPlanner
    {
        public static PixelSize CalculateOutputSize(int longEdge, AspectRatioSpec aspectRatio)
        {
            if (longEdge <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(longEdge), "Output long edge must be positive.");
            }

            if (!aspectRatio.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "Aspect ratio must be positive.");
            }

            if (aspectRatio.Width >= aspectRatio.Height)
            {
                return new PixelSize(longEdge, Math.Max(1, (int)Math.Round((double)longEdge * aspectRatio.Height / aspectRatio.Width)));
            }

            return new PixelSize(Math.Max(1, (int)Math.Round((double)longEdge * aspectRatio.Width / aspectRatio.Height)), longEdge);
        }

        public static OutputMappingPlan Plan(CropSelection selection, int outputLongEdge, AspectRatioSpec outputAspectRatio, CanvasMappingMode mappingMode)
        {
            if (!selection.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(selection), "Selection must be positive.");
            }

            var outputSize = CalculateOutputSize(outputLongEdge, outputAspectRatio);
            return Plan(selection, outputSize, mappingMode);
        }

        public static OutputMappingPlan Plan(CropSelection selection, PixelSize outputSize, CanvasMappingMode mappingMode)
        {
            if (!selection.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(selection), "Selection must be positive.");
            }

            if (!outputSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size must be positive.");
            }

            switch (mappingMode)
            {
                case CanvasMappingMode.Fit:
                    return PlanFit(selection, outputSize);
                case CanvasMappingMode.Fill:
                    return PlanFill(selection, outputSize);
                case CanvasMappingMode.Stretch:
                    return new OutputMappingPlan(
                        outputSize,
                        selection,
                        new CropSelection(0, 0, outputSize.Width, outputSize.Height),
                        CanvasMappingMode.Stretch);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mappingMode), mappingMode, "Unsupported mapping mode.");
            }
        }

        private static OutputMappingPlan PlanFit(CropSelection selection, PixelSize outputSize)
        {
            var scale = Math.Min((double)outputSize.Width / selection.Width, (double)outputSize.Height / selection.Height);
            var destinationWidth = Math.Max(1, (int)Math.Round(selection.Width * scale));
            var destinationHeight = Math.Max(1, (int)Math.Round(selection.Height * scale));
            var destinationX = (outputSize.Width - destinationWidth) / 2;
            var destinationY = (outputSize.Height - destinationHeight) / 2;

            return new OutputMappingPlan(
                outputSize,
                selection,
                new CropSelection(destinationX, destinationY, destinationWidth, destinationHeight),
                CanvasMappingMode.Fit);
        }

        private static OutputMappingPlan PlanFill(CropSelection selection, PixelSize outputSize)
        {
            var outputRatio = (double)outputSize.Width / outputSize.Height;
            var sourceRatio = (double)selection.Width / selection.Height;

            var sourceX = selection.X;
            var sourceY = selection.Y;
            var sourceWidth = selection.Width;
            var sourceHeight = selection.Height;

            if (sourceRatio > outputRatio)
            {
                sourceWidth = Math.Max(1, (int)Math.Round(selection.Height * outputRatio));
                sourceX = selection.X + (selection.Width - sourceWidth) / 2;
            }
            else if (sourceRatio < outputRatio)
            {
                sourceHeight = Math.Max(1, (int)Math.Round(selection.Width / outputRatio));
                sourceY = selection.Y + (selection.Height - sourceHeight) / 2;
            }

            return new OutputMappingPlan(
                outputSize,
                new CropSelection(sourceX, sourceY, sourceWidth, sourceHeight),
                new CropSelection(0, 0, outputSize.Width, outputSize.Height),
                CanvasMappingMode.Fill);
        }
    }
}

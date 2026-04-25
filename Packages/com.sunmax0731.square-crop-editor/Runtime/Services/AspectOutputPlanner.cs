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
            return Plan(selection, outputLongEdge, outputAspectRatio, mappingMode, 0);
        }

        public static OutputMappingPlan Plan(CropSelection selection, int outputLongEdge, AspectRatioSpec outputAspectRatio, CanvasMappingMode mappingMode, int outputPadding)
        {
            if (!selection.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(selection), "Selection must be positive.");
            }

            var outputSize = CalculateOutputSize(outputLongEdge, outputAspectRatio);
            return Plan(selection, outputSize, mappingMode, outputPadding);
        }

        public static OutputMappingPlan Plan(CropSelection selection, PixelSize outputSize, CanvasMappingMode mappingMode)
        {
            return Plan(selection, outputSize, mappingMode, 0);
        }

        public static OutputMappingPlan Plan(CropSelection selection, PixelSize outputSize, CanvasMappingMode mappingMode, int outputPadding)
        {
            if (!selection.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(selection), "Selection must be positive.");
            }

            if (!outputSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(outputSize), "Output size must be positive.");
            }

            var contentRect = CalculateContentRect(outputSize, outputPadding);
            switch (mappingMode)
            {
                case CanvasMappingMode.Fit:
                    return PlanFit(selection, outputSize, contentRect);
                case CanvasMappingMode.Fill:
                    return PlanFill(selection, outputSize, contentRect);
                case CanvasMappingMode.Stretch:
                    return new OutputMappingPlan(
                        outputSize,
                        selection,
                        contentRect,
                        CanvasMappingMode.Stretch);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mappingMode), mappingMode, "Unsupported mapping mode.");
            }
        }

        private static CropSelection CalculateContentRect(PixelSize outputSize, int outputPadding)
        {
            var maxPadding = Math.Max(0, Math.Min((outputSize.Width - 1) / 2, (outputSize.Height - 1) / 2));
            var padding = Math.Min(Math.Max(0, outputPadding), maxPadding);
            return new CropSelection(
                padding,
                padding,
                outputSize.Width - padding * 2,
                outputSize.Height - padding * 2);
        }

        private static OutputMappingPlan PlanFit(CropSelection selection, PixelSize outputSize, CropSelection contentRect)
        {
            var scale = Math.Min((double)contentRect.Width / selection.Width, (double)contentRect.Height / selection.Height);
            var destinationWidth = Math.Max(1, (int)Math.Round(selection.Width * scale));
            var destinationHeight = Math.Max(1, (int)Math.Round(selection.Height * scale));
            var destinationX = contentRect.X + (contentRect.Width - destinationWidth) / 2;
            var destinationY = contentRect.Y + (contentRect.Height - destinationHeight) / 2;

            return new OutputMappingPlan(
                outputSize,
                selection,
                new CropSelection(destinationX, destinationY, destinationWidth, destinationHeight),
                CanvasMappingMode.Fit);
        }

        private static OutputMappingPlan PlanFill(CropSelection selection, PixelSize outputSize, CropSelection contentRect)
        {
            var outputRatio = (double)contentRect.Width / contentRect.Height;
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
                contentRect,
                CanvasMappingMode.Fill);
        }
    }
}

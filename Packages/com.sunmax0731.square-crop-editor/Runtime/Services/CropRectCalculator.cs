using System;
using Sunmax0731.SquareCropEditor.Models;

namespace Sunmax0731.SquareCropEditor.Services
{
    public static class CropRectCalculator
    {
        public static CropSelection FromPreviewDrag(
            double startX,
            double startY,
            double endX,
            double endY,
            PixelSize previewSize,
            PixelSize sourceSize,
            AspectRatioSpec aspectRatio)
        {
            if (!previewSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(previewSize), "Preview size must be positive.");
            }

            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            if (!aspectRatio.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "Aspect ratio must be positive.");
            }

            var sourceStartX = Clamp(startX * sourceSize.Width / previewSize.Width, 0, sourceSize.Width);
            var sourceStartY = Clamp(startY * sourceSize.Height / previewSize.Height, 0, sourceSize.Height);
            var sourceEndX = Clamp(endX * sourceSize.Width / previewSize.Width, 0, sourceSize.Width);
            var sourceEndY = Clamp(endY * sourceSize.Height / previewSize.Height, 0, sourceSize.Height);

            var directionX = sourceEndX >= sourceStartX ? 1 : -1;
            var directionY = sourceEndY >= sourceStartY ? 1 : -1;
            var requestedWidth = Math.Abs(sourceEndX - sourceStartX);
            var requestedHeight = Math.Abs(sourceEndY - sourceStartY);

            if (requestedWidth <= 0 && requestedHeight <= 0)
            {
                return new CropSelection((int)Math.Round(sourceStartX), (int)Math.Round(sourceStartY), 0, 0);
            }

            var maxWidth = directionX > 0 ? sourceSize.Width - sourceStartX : sourceStartX;
            var maxHeight = directionY > 0 ? sourceSize.Height - sourceStartY : sourceStartY;
            requestedWidth = Math.Min(requestedWidth, maxWidth);
            requestedHeight = Math.Min(requestedHeight, maxHeight);

            var ratio = aspectRatio.Value;
            var width = requestedWidth;
            var height = requestedHeight;

            if (width <= 0)
            {
                width = height * ratio;
            }
            else if (height <= 0)
            {
                height = width / ratio;
            }
            else if (width / height > ratio)
            {
                width = height * ratio;
            }
            else
            {
                height = width / ratio;
            }

            if (width > maxWidth)
            {
                width = maxWidth;
                height = width / ratio;
            }

            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * ratio;
            }

            var x = directionX > 0 ? sourceStartX : sourceStartX - width;
            var y = directionY > 0 ? sourceStartY : sourceStartY - height;

            return RoundSelection(x, y, width, height, sourceSize, aspectRatio);
        }

        private static CropSelection RoundSelection(double x, double y, double width, double height, PixelSize sourceSize, AspectRatioSpec aspectRatio)
        {
            var roundedWidth = Math.Max(1, (int)Math.Round(width));
            var roundedHeight = Math.Max(1, (int)Math.Round(roundedWidth / aspectRatio.Value));

            if (roundedHeight < 1)
            {
                roundedHeight = 1;
            }

            if (roundedHeight > sourceSize.Height)
            {
                roundedHeight = sourceSize.Height;
                roundedWidth = Math.Max(1, (int)Math.Round(roundedHeight * aspectRatio.Value));
            }

            if (roundedWidth > sourceSize.Width)
            {
                roundedWidth = sourceSize.Width;
                roundedHeight = Math.Max(1, (int)Math.Round(roundedWidth / aspectRatio.Value));
            }

            var roundedX = (int)Math.Round(x);
            var roundedY = (int)Math.Round(y);

            roundedX = Math.Max(0, Math.Min(roundedX, sourceSize.Width - roundedWidth));
            roundedY = Math.Max(0, Math.Min(roundedY, sourceSize.Height - roundedHeight));

            return new CropSelection(roundedX, roundedY, roundedWidth, roundedHeight);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}

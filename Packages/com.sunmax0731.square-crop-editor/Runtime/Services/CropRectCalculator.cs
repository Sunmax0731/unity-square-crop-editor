using System;
using Sunmax0731.SquareCropEditor.Models;

namespace Sunmax0731.SquareCropEditor.Services
{
    public static class CropRectCalculator
    {
        public static CropSelection FullSource(PixelSize sourceSize)
        {
            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            return new CropSelection(0, 0, sourceSize.Width, sourceSize.Height);
        }

        public static CropSelection CenterCrop(PixelSize sourceSize, AspectRatioSpec aspectRatio)
        {
            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            if (!aspectRatio.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "Aspect ratio must be positive.");
            }

            var sourceRatio = (double)sourceSize.Width / sourceSize.Height;
            var targetRatio = aspectRatio.Value;
            var width = sourceSize.Width;
            var height = sourceSize.Height;

            if (sourceRatio > targetRatio)
            {
                width = Math.Max(1, (int)Math.Round(sourceSize.Height * targetRatio));
            }
            else if (sourceRatio < targetRatio)
            {
                height = Math.Max(1, (int)Math.Round(sourceSize.Width / targetRatio));
            }

            var x = Math.Max(0, (sourceSize.Width - width) / 2);
            var y = Math.Max(0, (sourceSize.Height - height) / 2);
            return new CropSelection(x, y, width, height);
        }

        public static CropSelection FromManualInput(
            int x,
            int y,
            int width,
            int height,
            PixelSize sourceSize)
        {
            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            var clampedWidth = Math.Max(1, Math.Min(width, sourceSize.Width));
            var clampedHeight = Math.Max(1, Math.Min(height, sourceSize.Height));
            var clampedX = Math.Max(0, Math.Min(x, sourceSize.Width - clampedWidth));
            var clampedY = Math.Max(0, Math.Min(y, sourceSize.Height - clampedHeight));
            return new CropSelection(clampedX, clampedY, clampedWidth, clampedHeight);
        }

        public static CropSelection FromManualInput(
            int x,
            int y,
            int width,
            int height,
            PixelSize sourceSize,
            AspectRatioSpec aspectRatio)
        {
            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            if (!aspectRatio.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(aspectRatio), "Aspect ratio must be positive.");
            }

            var clampedWidth = Math.Max(1, Math.Min(width, sourceSize.Width));
            var clampedHeight = Math.Max(1, Math.Min(height, sourceSize.Height));
            var ratio = aspectRatio.Value;

            if ((double)clampedWidth / clampedHeight > ratio)
            {
                clampedWidth = Math.Max(1, (int)Math.Round(clampedHeight * ratio));
            }
            else
            {
                clampedHeight = Math.Max(1, (int)Math.Round(clampedWidth / ratio));
            }

            if (clampedWidth > sourceSize.Width)
            {
                clampedWidth = sourceSize.Width;
                clampedHeight = Math.Max(1, (int)Math.Round(clampedWidth / ratio));
            }

            if (clampedHeight > sourceSize.Height)
            {
                clampedHeight = sourceSize.Height;
                clampedWidth = Math.Max(1, (int)Math.Round(clampedHeight * ratio));
            }

            var clampedX = Math.Max(0, Math.Min(x, sourceSize.Width - clampedWidth));
            var clampedY = Math.Max(0, Math.Min(y, sourceSize.Height - clampedHeight));

            return new CropSelection(clampedX, clampedY, clampedWidth, clampedHeight);
        }

        public static CropSelection FromPreviewDrag(
            double startX,
            double startY,
            double endX,
            double endY,
            PixelSize previewSize,
            PixelSize sourceSize)
        {
            if (!previewSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(previewSize), "Preview size must be positive.");
            }

            if (!sourceSize.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceSize), "Source size must be positive.");
            }

            var sourceStartX = Clamp(startX * sourceSize.Width / previewSize.Width, 0, sourceSize.Width);
            var sourceStartY = Clamp(startY * sourceSize.Height / previewSize.Height, 0, sourceSize.Height);
            var sourceEndX = Clamp(endX * sourceSize.Width / previewSize.Width, 0, sourceSize.Width);
            var sourceEndY = Clamp(endY * sourceSize.Height / previewSize.Height, 0, sourceSize.Height);

            var x = Math.Min(sourceStartX, sourceEndX);
            var y = Math.Min(sourceStartY, sourceEndY);
            var width = Math.Abs(sourceEndX - sourceStartX);
            var height = Math.Abs(sourceEndY - sourceStartY);

            if (width <= 0 && height <= 0)
            {
                return new CropSelection((int)Math.Round(sourceStartX), (int)Math.Round(sourceStartY), 0, 0);
            }

            var roundedWidth = Math.Max(1, (int)Math.Round(width));
            var roundedHeight = Math.Max(1, (int)Math.Round(height));
            var roundedX = Math.Max(0, Math.Min((int)Math.Round(x), sourceSize.Width - roundedWidth));
            var roundedY = Math.Max(0, Math.Min((int)Math.Round(y), sourceSize.Height - roundedHeight));
            return new CropSelection(roundedX, roundedY, roundedWidth, roundedHeight);
        }

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

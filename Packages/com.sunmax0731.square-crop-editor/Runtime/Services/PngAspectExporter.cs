using System;
using System.IO;
using Sunmax0731.SquareCropEditor.Models;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Services
{
    public static class PngAspectExporter
    {
        public static PngExportResult Export(PngExportRequest request)
        {
            var validationError = Validate(request);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new PngExportResult(PngExportStatus.Error, string.Empty, default, validationError);
            }

            var outputSize = AspectOutputPlanner.CalculateOutputSize(request.OutputLongEdge, request.OutputAspectRatio);
            var outputPath = ResolveOutputPath(request.OutputFolder, request.OutputFileName);
            var conflictPath = ResolveConflict(outputPath, request.ConflictBehavior);

            if (string.IsNullOrEmpty(conflictPath))
            {
                return new PngExportResult(PngExportStatus.Skipped, outputPath, outputSize, $"Output file already exists and Conflict is set to Skip: {outputPath}");
            }

            try
            {
                var plan = AspectOutputPlanner.Plan(request.Selection, outputSize, request.MappingMode, request.OutputPadding);
                var outputTexture = Render(request.SourceTexture, plan);
                var pngBytes = ImageConversion.EncodeToPNG(outputTexture);
                UnityEngine.Object.DestroyImmediate(outputTexture);

                Directory.CreateDirectory(Path.GetDirectoryName(conflictPath));
                File.WriteAllBytes(conflictPath, pngBytes);

                return new PngExportResult(PngExportStatus.Exported, conflictPath, outputSize, "Exported.");
            }
            catch (Exception ex)
            {
                return new PngExportResult(PngExportStatus.Error, conflictPath, outputSize, $"Failed to export PNG to '{conflictPath}': {ex.Message}");
            }
        }

        public static Texture2D Render(Texture2D sourceTexture, OutputMappingPlan plan)
        {
            if (sourceTexture == null)
            {
                throw new ArgumentNullException(nameof(sourceTexture));
            }

            if (!plan.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(plan), "Mapping plan must be valid.");
            }

            var output = new Texture2D(plan.OutputSize.Width, plan.OutputSize.Height, TextureFormat.RGBA32, false);
            var clearPixels = new Color32[plan.OutputSize.Width * plan.OutputSize.Height];
            for (var i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = new Color32(0, 0, 0, 0);
            }

            output.SetPixels32(clearPixels);

            for (var destY = 0; destY < plan.DestinationRect.Height; destY++)
            {
                for (var destX = 0; destX < plan.DestinationRect.Width; destX++)
                {
                    var sourceX = plan.SourceRect.X + Math.Min(
                        plan.SourceRect.Width - 1,
                        (int)Math.Floor((destX + 0.5d) * plan.SourceRect.Width / plan.DestinationRect.Width));
                    var sourceTopY = plan.SourceRect.Y + Math.Min(
                        plan.SourceRect.Height - 1,
                        (int)Math.Floor((destY + 0.5d) * plan.SourceRect.Height / plan.DestinationRect.Height));

                    var outputX = plan.DestinationRect.X + destX;
                    var outputTopY = plan.DestinationRect.Y + destY;

                    var sourceBottomY = sourceTexture.height - 1 - sourceTopY;
                    var outputBottomY = plan.OutputSize.Height - 1 - outputTopY;
                    output.SetPixel(outputX, outputBottomY, sourceTexture.GetPixel(sourceX, sourceBottomY));
                }
            }

            output.Apply(false, false);
            return output;
        }

        private static string Validate(PngExportRequest request)
        {
            if (request == null)
            {
                return "Export request is missing.";
            }

            if (request.SourceTexture == null)
            {
                return "Source texture is missing.";
            }

            if (!request.Selection.IsValid)
            {
                return "Crop selection is invalid.";
            }

            if (request.OutputLongEdge <= 0)
            {
                return "Output size must be positive.";
            }

            if (request.OutputPadding < 0)
            {
                return "Output padding must be zero or greater.";
            }

            if (!request.OutputAspectRatio.IsValid)
            {
                return "Output aspect ratio is invalid.";
            }

            if (string.IsNullOrWhiteSpace(request.OutputFolder))
            {
                return "Output folder is missing. Choose an output folder or restore the default folder.";
            }

            if (string.IsNullOrWhiteSpace(request.OutputFileName))
            {
                return "Output file name is missing. Enter a PNG file name.";
            }

            if (request.OutputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return $"Output file name contains invalid characters: {request.OutputFileName}";
            }

            return string.Empty;
        }

        private static string ResolveOutputPath(string outputFolder, string outputFileName)
        {
            var fileName = Path.GetExtension(outputFileName).Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? outputFileName
                : outputFileName + ".png";
            return Path.GetFullPath(Path.Combine(outputFolder, fileName));
        }

        private static string ResolveConflict(string outputPath, ExportConflictBehavior conflictBehavior)
        {
            if (!File.Exists(outputPath) || conflictBehavior == ExportConflictBehavior.Overwrite)
            {
                return outputPath;
            }

            if (conflictBehavior == ExportConflictBehavior.Skip)
            {
                return string.Empty;
            }

            var directory = Path.GetDirectoryName(outputPath);
            var fileName = Path.GetFileNameWithoutExtension(outputPath);
            var extension = Path.GetExtension(outputPath);

            for (var index = 1; index <= 999; index++)
            {
                var candidate = Path.Combine(directory, $"{fileName}_copy{index:00}{extension}");
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new IOException("Could not resolve a duplicate output path.");
        }
    }
}

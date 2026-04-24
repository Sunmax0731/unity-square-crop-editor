using System.IO;
using NUnit.Framework;
using Sunmax0731.SquareCropEditor.Models;
using Sunmax0731.SquareCropEditor.Services;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Tests.Editor
{
    public sealed class PngAspectExporterTests
    {
        private string _tempFolder;

        [SetUp]
        public void SetUp()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "UnitySquareCropEditorTests", TestContext.CurrentContext.Test.ID);
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }

            Directory.CreateDirectory(_tempFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }
        }

        [Test]
        public void ExportCreatesPngWithRequestedAspectDimensions()
        {
            var source = CreateSourceTexture();
            var result = PngAspectExporter.Export(new PngExportRequest
            {
                SourceTexture = source,
                Selection = new CropSelection(0, 0, 4, 4),
                OutputLongEdge = 16,
                OutputAspectRatio = AspectRatioSpec.Landscape16By9,
                MappingMode = CanvasMappingMode.Stretch,
                OutputFolder = _tempFolder,
                OutputFileName = "landscape"
            });

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(File.Exists(result.OutputPath), Is.True);

            var exported = LoadPng(result.OutputPath);
            Assert.That(exported.width, Is.EqualTo(16));
            Assert.That(exported.height, Is.EqualTo(9));

            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(exported);
        }

        [Test]
        public void FitLeavesTransparentPadding()
        {
            var source = CreateSourceTexture();
            var output = PngAspectExporter.Render(
                source,
                AspectOutputPlanner.Plan(new CropSelection(0, 0, 4, 2), new PixelSize(8, 8), CanvasMappingMode.Fit));

            Assert.That(output.GetPixel(0, 0).a, Is.EqualTo(0f).Within(0.001f));
            Assert.That(output.GetPixel(4, 4).a, Is.GreaterThan(0f));

            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(output);
        }

        [Test]
        public void ExportPreservesSourceAlpha()
        {
            var source = CreateSourceTexture();
            var output = PngAspectExporter.Render(
                source,
                AspectOutputPlanner.Plan(new CropSelection(0, 0, 4, 4), new PixelSize(4, 4), CanvasMappingMode.Stretch));

            Assert.That(output.GetPixel(0, 0).a, Is.EqualTo(1f / 7f).Within(0.01f));
            Assert.That(output.GetPixel(3, 3).a, Is.EqualTo(1f).Within(0.01f));

            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(output);
        }

        [Test]
        public void RenderUsesTopLeftSelectionCoordinates()
        {
            var source = CreatePositionTexture();
            var output = PngAspectExporter.Render(
                source,
                AspectOutputPlanner.Plan(new CropSelection(1, 0, 2, 2), new PixelSize(2, 2), CanvasMappingMode.Stretch));

            AssertColor(output, 0, 0, Color.red);
            AssertColor(output, 1, 0, Color.green);
            AssertColor(output, 0, 1, Color.blue);
            AssertColor(output, 1, 1, Color.yellow);

            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(output);
        }

        [Test]
        public void SkipConflictDoesNotOverwriteExistingFile()
        {
            var existingPath = Path.Combine(_tempFolder, "icon.png");
            File.WriteAllText(existingPath, "existing");

            var source = CreateSourceTexture();
            var result = PngAspectExporter.Export(new PngExportRequest
            {
                SourceTexture = source,
                Selection = new CropSelection(0, 0, 4, 4),
                OutputLongEdge = 4,
                OutputAspectRatio = AspectRatioSpec.Square,
                MappingMode = CanvasMappingMode.Stretch,
                OutputFolder = _tempFolder,
                OutputFileName = "icon.png",
                ConflictBehavior = ExportConflictBehavior.Skip
            });

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Skipped));
            Assert.That(File.ReadAllText(existingPath), Is.EqualTo("existing"));

            UnityEngine.Object.DestroyImmediate(source);
        }

        [Test]
        public void DuplicateConflictCreatesStableCopySuffix()
        {
            File.WriteAllText(Path.Combine(_tempFolder, "icon.png"), "existing");

            var source = CreateSourceTexture();
            var result = PngAspectExporter.Export(new PngExportRequest
            {
                SourceTexture = source,
                Selection = new CropSelection(0, 0, 4, 4),
                OutputLongEdge = 4,
                OutputAspectRatio = AspectRatioSpec.Square,
                MappingMode = CanvasMappingMode.Stretch,
                OutputFolder = _tempFolder,
                OutputFileName = "icon.png",
                ConflictBehavior = ExportConflictBehavior.Duplicate
            });

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(Path.GetFileName(result.OutputPath), Is.EqualTo("icon_copy01.png"));
            Assert.That(File.Exists(result.OutputPath), Is.True);

            UnityEngine.Object.DestroyImmediate(source);
        }

        [Test]
        public void OverwriteConflictReplacesExistingFile()
        {
            var existingPath = Path.Combine(_tempFolder, "icon.png");
            File.WriteAllText(existingPath, "existing");

            var source = CreateSourceTexture();
            var result = PngAspectExporter.Export(new PngExportRequest
            {
                SourceTexture = source,
                Selection = new CropSelection(0, 0, 4, 4),
                OutputLongEdge = 4,
                OutputAspectRatio = AspectRatioSpec.Square,
                MappingMode = CanvasMappingMode.Stretch,
                OutputFolder = _tempFolder,
                OutputFileName = "icon.png",
                ConflictBehavior = ExportConflictBehavior.Overwrite
            });

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Exported));
            Assert.That(result.OutputPath, Is.EqualTo(Path.GetFullPath(existingPath)));
            Assert.That(File.ReadAllText(existingPath), Is.Not.EqualTo("existing"));

            UnityEngine.Object.DestroyImmediate(source);
        }

        [Test]
        public void InvalidRequestRecordsErrorStatus()
        {
            var source = CreateSourceTexture();
            var result = PngAspectExporter.Export(new PngExportRequest
            {
                SourceTexture = source,
                Selection = new CropSelection(0, 0, 4, 4),
                OutputLongEdge = 4,
                OutputAspectRatio = AspectRatioSpec.Square,
                MappingMode = CanvasMappingMode.Stretch,
                OutputFolder = _tempFolder,
                OutputFileName = string.Empty
            });

            Assert.That(result.Status, Is.EqualTo(PngExportStatus.Error));
            Assert.That(result.Message, Is.EqualTo("Output file name is missing."));

            UnityEngine.Object.DestroyImmediate(source);
        }

        private static Texture2D CreateSourceTexture()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    pixels[y * 4 + x] = new Color(x / 3f, y / 3f, 1f, (x + y + 1) / 7f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreatePositionTexture()
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            SetVisualPixel(texture, 1, 0, Color.red);
            SetVisualPixel(texture, 2, 0, Color.green);
            SetVisualPixel(texture, 1, 1, Color.blue);
            SetVisualPixel(texture, 2, 1, Color.yellow);
            texture.Apply();
            return texture;
        }

        private static void AssertColor(Texture2D texture, int visualX, int visualY, Color expected)
        {
            var actual = GetVisualPixel(texture, visualX, visualY);
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.001f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.001f));
        }

        private static Color GetVisualPixel(Texture2D texture, int visualX, int visualY)
        {
            return texture.GetPixel(visualX, texture.height - 1 - visualY);
        }

        private static void SetVisualPixel(Texture2D texture, int visualX, int visualY, Color color)
        {
            texture.SetPixel(visualX, texture.height - 1 - visualY, color);
        }

        private static Texture2D LoadPng(string path)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, File.ReadAllBytes(path));
            return texture;
        }
    }
}

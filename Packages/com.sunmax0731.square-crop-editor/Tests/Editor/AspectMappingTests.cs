using NUnit.Framework;
using Sunmax0731.SquareCropEditor.Models;
using Sunmax0731.SquareCropEditor.Services;

namespace Sunmax0731.SquareCropEditor.Tests.Editor
{
    public sealed class AspectMappingTests
    {
        [Test]
        public void OutputSizeUsesLongEdgeForLandscapeRatio()
        {
            var size = AspectOutputPlanner.CalculateOutputSize(256, AspectRatioSpec.Landscape16By9);

            Assert.That(size.Width, Is.EqualTo(256));
            Assert.That(size.Height, Is.EqualTo(144));
        }

        [Test]
        public void OutputSizeUsesLongEdgeForPortraitRatio()
        {
            var size = AspectOutputPlanner.CalculateOutputSize(256, AspectRatioSpec.Portrait9By16);

            Assert.That(size.Width, Is.EqualTo(144));
            Assert.That(size.Height, Is.EqualTo(256));
        }

        [Test]
        public void CropDragConstrainsToSquareRatio()
        {
            var selection = CropRectCalculator.FromPreviewDrag(
                10,
                10,
                90,
                50,
                new PixelSize(100, 100),
                new PixelSize(200, 200),
                AspectRatioSpec.Square);

            Assert.That(selection.X, Is.EqualTo(20));
            Assert.That(selection.Y, Is.EqualTo(20));
            Assert.That(selection.Width, Is.EqualTo(80));
            Assert.That(selection.Height, Is.EqualTo(80));
        }

        [Test]
        public void CropDragPreservesDirectionAndClampsToBounds()
        {
            var selection = CropRectCalculator.FromPreviewDrag(
                95,
                95,
                30,
                10,
                new PixelSize(100, 100),
                new PixelSize(100, 100),
                AspectRatioSpec.Landscape4By3);

            Assert.That(selection.X, Is.EqualTo(30));
            Assert.That(selection.Y, Is.EqualTo(46));
            Assert.That(selection.Width, Is.EqualTo(65));
            Assert.That(selection.Height, Is.EqualTo(49));
        }

        [Test]
        public void FitMapsFullSourceInsideTransparentCanvasArea()
        {
            var plan = AspectOutputPlanner.Plan(
                new CropSelection(0, 0, 200, 100),
                new PixelSize(256, 256),
                CanvasMappingMode.Fit);

            Assert.That(plan.SourceRect, Is.EqualTo(new CropSelection(0, 0, 200, 100)));
            Assert.That(plan.DestinationRect, Is.EqualTo(new CropSelection(0, 64, 256, 128)));
        }

        [Test]
        public void FillCropsSourceToOutputRatio()
        {
            var plan = AspectOutputPlanner.Plan(
                new CropSelection(0, 0, 200, 100),
                new PixelSize(256, 256),
                CanvasMappingMode.Fill);

            Assert.That(plan.SourceRect, Is.EqualTo(new CropSelection(50, 0, 100, 100)));
            Assert.That(plan.DestinationRect, Is.EqualTo(new CropSelection(0, 0, 256, 256)));
        }

        [Test]
        public void StretchUsesFullSourceAndFullCanvas()
        {
            var plan = AspectOutputPlanner.Plan(
                new CropSelection(10, 20, 200, 100),
                new PixelSize(256, 144),
                CanvasMappingMode.Stretch);

            Assert.That(plan.SourceRect, Is.EqualTo(new CropSelection(10, 20, 200, 100)));
            Assert.That(plan.DestinationRect, Is.EqualTo(new CropSelection(0, 0, 256, 144)));
        }
    }
}

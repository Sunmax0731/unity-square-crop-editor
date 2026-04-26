using NUnit.Framework;
using Sunmax0731.SquareCropEditor.Models;
using Sunmax0731.SquareCropEditor.Services;

namespace Sunmax0731.SquareCropEditor.Tests.Editor
{
    public sealed class SquareCropPackageScaffoldTests
    {
        [Test]
        public void DefaultsMatchRequirementsBaseline()
        {
            var settings = SquareCropDefaults.CreateSettings();

            Assert.That(SquareCropDefaults.MenuPath, Is.EqualTo("Tools/Square Crop Editor/メイン画面"));
            Assert.That(settings.OutputSize, Is.EqualTo(256));
            Assert.That(settings.CropAspectRatio, Is.EqualTo(AspectRatioSpec.Square));
            Assert.That(settings.OutputAspectRatio, Is.EqualTo(AspectRatioSpec.Square));
            Assert.That(settings.OutputFolder, Is.EqualTo("Assets/Generated/SquareCrop"));
            Assert.That(settings.MappingMode, Is.EqualTo(CanvasMappingMode.Fit));
            Assert.That(settings.ConflictBehavior, Is.EqualTo(ExportConflictBehavior.Duplicate));
        }

        [Test]
        public void CropSelectionRequiresPositiveSize()
        {
            Assert.That(new CropSelection(0, 0, 1, 1).IsValid, Is.True);
            Assert.That(new CropSelection(0, 0, 0, 1).IsValid, Is.False);
            Assert.That(new CropSelection(0, 0, 1, 0).IsValid, Is.False);
        }
    }
}

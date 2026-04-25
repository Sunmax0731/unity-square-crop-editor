using NUnit.Framework;
using Sunmax0731.SquareCropEditor.Editor.Localization;

namespace Sunmax0731.SquareCropEditor.Tests.Editor
{
    public sealed class SquareCropLocalizationTests
    {
        [Test]
        public void JapaneseLanguageModeLabelIsLocalized()
        {
            Assert.AreEqual(
                "日本語",
                SquareCropLocalization.GetLanguageModeLabel(
                    SquareCropDisplayLanguage.Japanese,
                    SquareCropLanguageMode.Japanese));
        }

        [Test]
        public void EnglishFallsBackToProvidedText()
        {
            Assert.AreEqual(
                "Help",
                SquareCropLocalization.Get(
                    SquareCropDisplayLanguage.English,
                    "help",
                    "Help"));
        }
    }
}

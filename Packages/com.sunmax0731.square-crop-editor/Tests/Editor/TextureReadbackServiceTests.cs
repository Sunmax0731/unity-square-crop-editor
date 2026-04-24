using System.IO;
using NUnit.Framework;
using Sunmax0731.SquareCropEditor.Editor.Export;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Tests.Editor
{
    public sealed class TextureReadbackServiceTests
    {
        private const string TestFolder = "Assets/TempSquareCropReadbackTests";
        private const string TestTexturePath = TestFolder + "/source.png";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            AssetDatabase.Refresh();
        }

        [Test]
        public void CreatesTemporaryReadableCopyWithoutChangingImporter()
        {
            Directory.CreateDirectory(TestFolder);
            File.WriteAllBytes(TestTexturePath, CreatePngBytes());
            AssetDatabase.ImportAsset(TestTexturePath, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TestTexturePath);
            importer.isReadable = false;
            importer.SaveAndReimport();

            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(TestTexturePath);
            Assert.That(source, Is.Not.Null);
            Assert.That(TextureReadbackService.CanReadPixels(source), Is.False);

            var result = TextureReadbackService.GetReadableTexture(source);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.OwnsTexture, Is.True);
            Assert.That(TextureReadbackService.CanReadPixels(result.Texture), Is.True);
            Assert.That(((TextureImporter)AssetImporter.GetAtPath(TestTexturePath)).isReadable, Is.False);

            TextureReadbackService.DestroyIfOwned(result);
        }

        private static byte[] CreatePngBytes()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[]
            {
                Color.clear,
                Color.red,
                Color.green,
                Color.blue
            });
            texture.Apply();
            var bytes = ImageConversion.EncodeToPNG(texture);
            Object.DestroyImmediate(texture);
            return bytes;
        }
    }
}

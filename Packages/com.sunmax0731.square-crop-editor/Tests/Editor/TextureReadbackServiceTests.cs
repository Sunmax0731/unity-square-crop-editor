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

        [Test]
        public void TemporaryReadableCopyUsesImportedTextureDimensions()
        {
            Directory.CreateDirectory(TestFolder);
            File.WriteAllBytes(TestTexturePath, CreatePngBytes(64, 32));
            AssetDatabase.ImportAsset(TestTexturePath, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TestTexturePath);
            importer.isReadable = false;
            importer.maxTextureSize = 16;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(TestTexturePath);
            Assert.That(source, Is.Not.Null);
            Assert.That(source.width, Is.LessThan(64));
            Assert.That(source.height, Is.LessThan(32));
            Assert.That(TextureReadbackService.CanReadPixels(source), Is.False);

            var result = TextureReadbackService.GetReadableTexture(source);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.OwnsTexture, Is.True);
            Assert.That(result.Texture.width, Is.EqualTo(source.width));
            Assert.That(result.Texture.height, Is.EqualTo(source.height));
            Assert.That(TextureReadbackService.CanReadPixels(result.Texture), Is.True);

            TextureReadbackService.DestroyIfOwned(result);
        }

        private static byte[] CreatePngBytes()
        {
            return CreatePngBytes(2, 2);
        }

        private static byte[] CreatePngBytes(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    pixels[y * width + x] = new Color((float)x / width, (float)y / height, 1f, 1f);
                }
            }

            if (width == 2 && height == 2)
            {
                pixels = new[]
                {
                    Color.clear,
                    Color.red,
                    Color.green,
                    Color.blue
                };
            }

            texture.SetPixels(pixels);
            texture.Apply();
            var bytes = ImageConversion.EncodeToPNG(texture);
            Object.DestroyImmediate(texture);
            return bytes;
        }
    }
}

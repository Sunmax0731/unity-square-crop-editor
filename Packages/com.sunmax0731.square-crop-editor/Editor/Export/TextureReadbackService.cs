using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Export
{
    public readonly struct ReadableTextureResult
    {
        public ReadableTextureResult(bool success, Texture2D texture, bool ownsTexture, string message)
        {
            Success = success;
            Texture = texture;
            OwnsTexture = ownsTexture;
            Message = message;
        }

        public bool Success { get; }

        public Texture2D Texture { get; }

        public bool OwnsTexture { get; }

        public string Message { get; }
    }

    public static class TextureReadbackService
    {
        public static ReadableTextureResult GetReadableTexture(Texture2D sourceTexture)
        {
            if (sourceTexture == null)
            {
                return new ReadableTextureResult(false, null, false, "No source image selected.");
            }

            if (CanReadPixels(sourceTexture))
            {
                return new ReadableTextureResult(true, sourceTexture, false, "Source texture is readable.");
            }

            var assetPath = AssetDatabase.GetAssetPath(sourceTexture);
            if (string.IsNullOrEmpty(assetPath))
            {
                return new ReadableTextureResult(false, null, false, "Source texture cannot be read and is not a project asset.");
            }

            var fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                return new ReadableTextureResult(false, null, false, $"Source texture file was not found: {assetPath}");
            }

            try
            {
                var readableTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    name = sourceTexture.name + "_ReadableCopy",
                    hideFlags = HideFlags.HideAndDontSave
                };

                if (!ImageConversion.LoadImage(readableTexture, File.ReadAllBytes(fullPath), false))
                {
                    UnityEngine.Object.DestroyImmediate(readableTexture);
                    return new ReadableTextureResult(false, null, false, $"Source texture file could not be decoded: {assetPath}");
                }

                return new ReadableTextureResult(true, readableTexture, true, "Created a temporary readable copy without changing importer settings.");
            }
            catch (Exception ex)
            {
                return new ReadableTextureResult(false, null, false, ex.Message);
            }
        }

        public static bool CanReadPixels(Texture2D texture)
        {
            if (texture == null)
            {
                return false;
            }

            try
            {
                texture.GetPixel(0, 0);
                return true;
            }
            catch (UnityException)
            {
                return false;
            }
        }

        public static void DestroyIfOwned(ReadableTextureResult result)
        {
            if (result.OwnsTexture && result.Texture != null)
            {
                UnityEngine.Object.DestroyImmediate(result.Texture);
            }
        }
    }
}

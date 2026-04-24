using System;
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

            try
            {
                var readableTexture = CreateReadableCopy(sourceTexture);
                return new ReadableTextureResult(true, readableTexture, true, "Created a temporary readable copy from the imported texture without changing importer settings.");
            }
            catch (Exception ex)
            {
                return new ReadableTextureResult(false, null, false, ex.Message);
            }
        }

        private static Texture2D CreateReadableCopy(Texture2D sourceTexture)
        {
            var previousActive = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            try
            {
                Graphics.Blit(sourceTexture, renderTexture);
                RenderTexture.active = renderTexture;

                var readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false)
                {
                    name = sourceTexture.name + "_ReadableCopy",
                    hideFlags = HideFlags.HideAndDontSave
                };
                readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
                readableTexture.Apply(false, false);
                return readableTexture;
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
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

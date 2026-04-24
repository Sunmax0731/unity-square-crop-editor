using System;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public readonly struct PixelSize
    {
        public PixelSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsValid => Width > 0 && Height > 0;
    }
}

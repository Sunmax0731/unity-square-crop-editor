using System;

namespace Sunmax0731.SquareCropEditor.Models
{
    [Serializable]
    public readonly struct AspectRatioSpec
    {
        public static readonly AspectRatioSpec Square = new AspectRatioSpec(1, 1);
        public static readonly AspectRatioSpec Landscape4By3 = new AspectRatioSpec(4, 3);
        public static readonly AspectRatioSpec Portrait3By4 = new AspectRatioSpec(3, 4);
        public static readonly AspectRatioSpec Landscape16By9 = new AspectRatioSpec(16, 9);
        public static readonly AspectRatioSpec Portrait9By16 = new AspectRatioSpec(9, 16);

        public AspectRatioSpec(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsValid => Width > 0 && Height > 0;

        public double Value
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException("Aspect ratio width and height must be positive.");
                }

                return (double)Width / Height;
            }
        }
    }
}

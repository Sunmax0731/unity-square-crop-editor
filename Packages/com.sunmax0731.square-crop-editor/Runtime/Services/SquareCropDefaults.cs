using Sunmax0731.SquareCropEditor.Models;

namespace Sunmax0731.SquareCropEditor.Services
{
    public static class SquareCropDefaults
    {
        public const string MenuPath = "Tools/Square Crop Editor/メイン画面";

        public static SquareCropSettings CreateSettings()
        {
            return new SquareCropSettings();
        }
    }
}

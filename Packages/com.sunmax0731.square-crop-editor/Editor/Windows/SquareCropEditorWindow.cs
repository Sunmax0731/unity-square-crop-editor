using Sunmax0731.SquareCropEditor.Services;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Windows
{
    public sealed class SquareCropEditorWindow : EditorWindow
    {
        public const string WindowTitle = "Square Crop Editor";

        [MenuItem(SquareCropDefaults.MenuPath)]
        public static void Open()
        {
            var window = GetWindow<SquareCropEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Package scaffold is ready. Crop UI implementation starts from the next issues.", MessageType.Info);
        }
    }
}

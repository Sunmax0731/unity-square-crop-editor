using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sunmax0731.SquareCropEditor.Editor.Release
{
    public static class UnityPackageExporter
    {
        private const string PackageRoot = "Packages/com.sunmax0731.square-crop-editor";
        private const string OutputArgument = "-squareCropUnityPackageOutput";

        public static void ExportFromCommandLine()
        {
            var outputPath = GetArgumentValue(OutputArgument);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException($"{OutputArgument} is required.");
            }

            outputPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            AssetDatabase.ExportPackage(
                PackageRoot,
                outputPath,
                ExportPackageOptions.Recurse);

            if (!File.Exists(outputPath))
            {
                throw new IOException($"Unity package was not created: {outputPath}");
            }

            Debug.Log($"Exported Unity package: {outputPath}");
        }

        private static string GetArgumentValue(string argumentName)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (args[index] == argumentName)
                {
                    return args[index + 1];
                }
            }

            return string.Empty;
        }
    }
}

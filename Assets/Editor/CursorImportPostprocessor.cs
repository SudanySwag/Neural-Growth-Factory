using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class CursorImportPostprocessor : AssetPostprocessor
{
    // Matches files like GameplayCursor1.png, IntroCursor3.png, etc.
    // Group 1: base name (e.g. "GameplayCursor")
    // Group 2: number (1-4)
    private static readonly Regex pattern = new Regex(@"^(.+Cursor)(\d)\.png$");

    private static readonly string[] suffixes = { "", "Active", "Clicked", "Disabled" };

    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith("Assets/Resources/Cursors/"))
            ((TextureImporter) assetImporter).textureType = TextureImporterType.Cursor;
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (!path.StartsWith("Assets/Resources/Cursors/")) continue;

            string filename = Path.GetFileName(path);
            Match match = pattern.Match(filename);
            if (!match.Success) continue;

            string baseName = match.Groups[1].Value;
            int number = int.Parse(match.Groups[2].Value);

            if (number < 1 || number > 4) continue;

            string newName = baseName + suffixes[number - 1] + ".png";

            string dir = Path.GetDirectoryName(path);
            string newPath = Path.Combine(dir, newName).Replace('\\', '/');

            if (File.Exists(newPath))
                AssetDatabase.DeleteAsset(newPath);

            string result = AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(newName));
            if (string.IsNullOrEmpty(result))
                UnityEngine.Debug.Log($"[CursorImport] Renamed {filename} -> {newName}");
            else {
                UnityEngine.Debug.LogError($"[CursorImport] Failed to import {filename}: {result}");
                AssetDatabase.DeleteAsset(filename);
            }
        }
    }
}

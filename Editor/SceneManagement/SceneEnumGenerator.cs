#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace MAF.SceneManagement.Editor
{
    /// <summary>
    /// An editor tool to automatically generate an enum containing all scenes from the Build Settings.
    /// This allows for type-safe scene management and easy scene selection in the Inspector.
    /// </summary>
    public static class SceneEnumGenerator
    {
        // Path to save the generated enum script.
        private const string ENUM_SCRIPT_PATH = "Assets/MAF/Runtime/SceneManagement/ScenesEnum.cs"; // Adjust path as needed for your project structure

        /// <summary>
        /// Creates a menu item in the Unity Editor to trigger the enum generation.
        /// </summary>
        [MenuItem("Tools/MAF/Generate Scene Enum")]
        public static void GenerateSceneEnum()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                Debug.LogWarning("No scenes found in Build Settings. Scene enum not generated.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// This is an auto-generated file. Do not edit manually.");
            sb.AppendLine("// To regenerate, use 'Tools/MAF/Generate Scene Enum' in the Unity Editor.");
            sb.AppendLine();
            sb.AppendLine("namespace MAF.SceneManagement");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// An enum representing all scenes included in the Build Settings.");
            sb.AppendLine("    /// The integer value of each enum member corresponds to its Build Index.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public enum Scenes");
            sb.AppendLine("    {");

            for (int i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (scene.enabled)
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    // Sanitize the name to be a valid C# enum identifier
                    string enumName = SanitizeIdentifier(sceneName);
                    sb.AppendLine($"        {enumName} = {i},");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(ENUM_SCRIPT_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(ENUM_SCRIPT_PATH, sb.ToString());
            AssetDatabase.Refresh(); // Tell Unity to re-import assets

            Debug.Log($"Scene enum successfully generated at: {ENUM_SCRIPT_PATH}");
        }

        /// <summary>
        /// Cleans a string to make it a valid C# identifier (e.g., removes spaces, special characters).
        /// </summary>
        private static string SanitizeIdentifier(string name)
        {
            name = name.Replace(" ", "_");
            // Add more sanitization rules as needed
            // For example, remove invalid characters
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }
            // Ensure it doesn't start with a number
            if (sb.Length > 0 && char.IsDigit(sb[0]))
            {
                sb.Insert(0, "_");
            }
            return sb.ToString();
        }
    }
}
#endif
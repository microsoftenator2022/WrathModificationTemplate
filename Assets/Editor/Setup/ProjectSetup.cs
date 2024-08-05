using System;
using System.IO;
using System.Linq;

using UnityEditor;

namespace OwlcatModification.Editor.Setup
{
    public static class ProjectSetup
    {
        public const string WotrDirectoryKey = "wotr_directory";

        [MenuItem("Modification Tools/Setup project", false, -1000)]
        public static void Setup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Setup project", "", 0);

                string wotrDirectory = EditorUtility.OpenFolderPanel(
                    "Pathfinder: Wrath of the Righteous folder", EditorPrefs.GetString(WotrDirectoryKey, ""), "");
                if (!Directory.Exists(wotrDirectory))
                {
                    throw new Exception("WotR folder is missing!");
                }

                EditorPrefs.SetString(WotrDirectoryKey, wotrDirectory);
                SetupAssemblies(wotrDirectory);

                File.Copy(
                    Path.Combine(wotrDirectory, "Bundles/utility_shaders"),
                    "Assets/RenderPipeline/utility_shaders",
                    true);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}\n\n{e.StackTrace}", "Close");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void SetupAssemblies(string wotrDirectory)
        {
            string[] skipAssemblies = {
                "mscorlib.dll",
                "Unity.ScriptableBuildPipeline.dll",
                "Owlcat.SharedTypes.dll",
                "Unity.TextMeshPro.dll"
            };

            bool SkipAssembly(string filename)
            {
                return
                    skipAssemblies.Contains(filename) ||
                    filename.StartsWith("System") ||
                    (filename.StartsWith("UnityEngine"));
            }

            const string targetAssembliesDirectory = "Assets/PathfinderAssemblies";
            Directory.CreateDirectory(targetAssembliesDirectory);

            string assembliesDirectory = Path.Combine(wotrDirectory, "Wrath_Data/Managed");

            int newAssemblies = 0;

            foreach (string assemblyPath in Directory.GetFiles(assembliesDirectory, "*.dll"))
            {
                string filename = Path.GetFileName(assemblyPath);

                if (SkipAssembly(filename))
                {
                    continue;
                }

                if (!File.Exists(Path.Combine(targetAssembliesDirectory, filename)))
                {
                    File.Copy(assemblyPath, Path.Combine(targetAssembliesDirectory, filename), true);
                    newAssemblies++;
                }
            }

            if (newAssemblies > 0)
                AssetDatabase.Refresh();
        }

        public static string WrathPath
        {
            get
            {
                var wrathPath = EditorPrefs.GetString(ProjectSetup.WotrDirectoryKey, "");

                if (String.IsNullOrEmpty(wrathPath))
                {
                    wrathPath = Environment.GetEnvironmentVariable("WrathPath");
                }

                return wrathPath;
            }
        }
    }
}
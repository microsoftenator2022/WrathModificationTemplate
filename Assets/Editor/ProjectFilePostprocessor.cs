using UnityEditor;

namespace OwlcatModification.Assets.Editor
{
    public class ProjectFilePostprocessor : AssetPostprocessor
    {
        public static string OnGeneratedSlnSolution(string path, string content)
        {
            return content;
        }

        public static string OnGeneratedCSProject(string path, string content)
        {
            content = content.Replace("<TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>", "<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>");
            content = content.Replace("DebugType>full</DebugType>", "DebugType>portable</DebugType>");

            return content;
        }
    }
}

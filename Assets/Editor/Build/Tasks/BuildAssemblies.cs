using OwlcatModification.Editor.Build.Context;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Player;
using UnityEditorInternal;

namespace OwlcatModification.Editor.Build.Tasks
{
    public class BuildAssemblies : IBuildTask
    {
        private static readonly Regex AsmdefNameRegex = new Regex("\"name\":[\\s]*\"([^\"]+)\"");

        public int Version
            => 1;

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        private IBuildParameters m_BuildParameters;

        [InjectContext(ContextUsage.In)]
        private IModificationParameters m_ModificationParameters;
#pragma warning restore 649

        public ReturnCode Run()
        {
            var msBuildPath = @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe";
            string projectPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                $"{m_ModificationParameters.Manifest.DisplayName}.csproj" 
            );

            var settings = m_BuildParameters.GetScriptCompilationSettings();
            string outputFolder = m_BuildParameters.GetOutputFilePathForIdentifier(BuilderConsts.OutputAssemblies);

            if (File.Exists(msBuildPath) && File.Exists(projectPath))
            {
                ProcessStartInfo msBuildInfo =
                    new ProcessStartInfo(msBuildPath, $"\"{projectPath}\" \"/p:OutputPath={outputFolder}\" /p:DefineConstants=\"{RemoveUnityEditorPreprocessor(projectPath)}\"")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                var msBuild = Process.Start(msBuildInfo);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "MSBuildOutput.log"), msBuild.StandardOutput.ReadToEnd());
                msBuild.WaitForExit();
            }
            else
            {
                var results = PlayerBuildInterface.CompilePlayerScripts(settings, outputFolder);
                if (results.assemblies == null || !results.assemblies.Any())
                {
                    return ReturnCode.Error;
                }
            }

            string[] asmdefGuids = AssetDatabase.FindAssets("t:Asmdef", new[] { m_ModificationParameters.ScriptsPath });
            string[] asmdefNames = asmdefGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(GetAssemblyName)
                .ToArray();
            foreach (string filePath in Directory.GetFiles(outputFolder))
            {
                if (asmdefNames.All(i => !filePath.Contains(i)))
                {
                    File.Delete(filePath);
                }
            }

            return ReturnCode.Success;
        }

        private static string RemoveUnityEditorPreprocessor(string projFilePath) => 
            GetPreprocessorDefines(projFilePath)
            .Replace("UNITY_EDITOR;", "");

        private static string GetPreprocessorDefines(string projFilePath)
        {
            string defineOpenTag = "<DefineConstants>";
            string defineCloseTag = "</DefineConstants>";

            var text = File.ReadAllText(projFilePath);
            var startIndex = text.IndexOf(defineOpenTag) + defineOpenTag.Length;
            var endIndex = text.IndexOf(defineCloseTag, startIndex);

            return text.Substring(startIndex, endIndex - startIndex);
        }

        private static string GetAssemblyName(AssemblyDefinitionAsset asmdef)
        {
            var m = AsmdefNameRegex.Match(asmdef.text);
            if (!m.Success)
            {
                throw new Exception($"Assembly name is missing: {asmdef.name}");
            }

            return m.Groups[1].Value;
        }
    }
}
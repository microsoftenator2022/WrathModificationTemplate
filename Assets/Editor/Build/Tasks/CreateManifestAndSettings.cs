using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Modding;

using Newtonsoft.Json;

using OwlcatModification.Editor.Build.Context;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace OwlcatModification.Editor.Build.Tasks
{
	public class CreateManifestAndSettings : IBuildTask
	{
#pragma warning disable 649
		[InjectContext(ContextUsage.In)]
		private IBuildParameters m_BuildParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationParameters m_ModificationParameters;
		
		[InjectContext(ContextUsage.In)]
		private IModificationRuntimeSettings m_ModificationSettings;
#pragma warning restore 649
		
		public int Version
			=> 1;

		public ReturnCode Run()
		{
			string buildFolderPath = m_BuildParameters.GetOutputFilePathForIdentifier("");

			var blueprintPatches =
				AssetDatabase.FindAssets($"t:{nameof(BlueprintPatches)}", new[] {m_ModificationParameters.SourcePath})
					.Select(AssetDatabase.GUIDToAssetPath)
					.Select(AssetDatabase.LoadAssetAtPath<BlueprintPatches>)
					.FirstOrDefault();
			if (blueprintPatches != null)
			{
				m_ModificationSettings.Settings.BlueprintPatches = blueprintPatches.Entries.ToList();
			}

			string manifestJsonFilePath = Path.Combine(buildFolderPath, Kingmaker.Modding.OwlcatModification.ManifestFileName);
			string manifestJsonContent = JsonUtility.ToJson(m_ModificationParameters.Manifest, true);
			File.WriteAllText(manifestJsonFilePath, manifestJsonContent);
			
			string settingsJsonFilePath = Path.Combine(buildFolderPath, Kingmaker.Modding.OwlcatModification.SettingsFileName);
			string settingsJsonContent = JsonUtility.ToJson(m_ModificationSettings.Settings, true);
			File.WriteAllText(settingsJsonFilePath, settingsJsonContent);

			// yolo
			//         IEnumerable<(string, IEnumerable<(string AssetPath, string AssetGuid)>)> makeBundleLayout()
			//{
			//	foreach (var value in m_ModificationSettings.Settings.BundlesLayout.GuidToBundle.Values)
			//	{
			//		IEnumerable<(string, string)> makeAssetsList()
			//		{
			//			foreach (var guid in m_ModificationSettings.Settings.BundlesLayout.GuidToBundle.Keys
			//                         .Where(key => m_ModificationSettings.Settings.BundlesLayout.GuidToBundle[key] == value))
			//			{
			//				yield return (AssetDatabase.GUIDToAssetPath(guid), guid);
			//                     }
			//		}

			//		yield return (value, makeAssetsList());
			//             }
			//}

			//         File.WriteAllText(Path.Combine(buildFolderPath, "BundlesLayout.json"),
			//	JsonConvert.SerializeObject(makeBundleLayout()
			//		.ToDictionary(
			//			pair => pair.Item1,
			//			pair => pair.Item2)));

			File.WriteAllText(Path.Combine(buildFolderPath, "BundlesLayout.json"),
				JsonConvert.SerializeObject(m_ModificationSettings.Settings.BundlesLayout.GuidToBundle.Values
					.ToDictionary(
						value => value,
						value => m_ModificationSettings.Settings.BundlesLayout.GuidToBundle.Keys
							.Where(key => m_ModificationSettings.Settings.BundlesLayout.GuidToBundle[key] == value)
							.Select(guid => new { AssetPath = AssetDatabase.GUIDToAssetPath(guid), AssetGuid = guid }))));

			return ReturnCode.Success;
		}
	}
}
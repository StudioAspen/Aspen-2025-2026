using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Aspen.Tools.Assets
{
	/// <summary>
	///  Studio Aspen's custom Asset Post Processor. Used to handle importing FBX files.
	/// </summary>
	public class AspenAssetPostprocessor : AssetPostprocessor
	{
		/// <summary>
		/// Used to process assets after being imported.
		/// </summary>
		/// <param name="importedAssets"></param>
		/// <param name="deletedAssets"></param>
		/// <param name="movedAssets"></param>
		/// <param name="movedFromAssetPaths"></param>
		static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			foreach (string assetPath in importedAssets)
			{
				// If an FBX file was imported into Assets/Art, create a prefab from the FBX file.
				if (assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)
				    && assetPath.StartsWith("Assets/Art/models"))
				{
					ExtractTextures(assetPath);
				}
			}
		}

		/// <summary>
		///	Create a prefab from an FBX.
		/// The FBX prefab will be a nested object in the new prefab.
		/// </summary>
		/// <param name="fbxPath">The path of the FBX to create a prefab from.</param>
		private static void ExtractTextures(string fbxPath)
		{
			string extractPath = Path.GetDirectoryName(fbxPath);

			var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
			if (importer == null)
			{
				Debug.LogError("No ModelImporter found at: " + fbxPath);
				return;
			}
			
			importer.ExtractTextures(extractPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}
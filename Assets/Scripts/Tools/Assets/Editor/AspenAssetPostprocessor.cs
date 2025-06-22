using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using Codice.Client.Common;

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
				    && (assetPath.StartsWith("Assets/Art/Models") || assetPath.StartsWith("Assets/Art/Rigs")))
				{
					CreatePrefabFromFBX(assetPath);
				}
			}
		}
		
		/// <summary>
		///	Create a prefab from an FBX.
		/// The FBX prefab will be a nested object in the new prefab.
		/// </summary>
		/// <param name="fbxPath">The path of the FBX to create a prefab from.</param>
		static void CreatePrefabFromFBX(string fbxPath)
		{
			// Convert the fbx path to the prefab path
			string fileName = Path.GetFileName(fbxPath);
			string prefabPath = fbxPath
				.Replace($"/{Path.GetFileName(Path.GetDirectoryName(fbxPath))}", "")	// Remove asset-specific folder
				.Replace("Art/Models", "Prefabs")										// Replace Art/Models folder with Prefabs
				.Replace(fileName, $"Static/{fileName}")								// Add static folder before the asset file
				.Replace(".fbx", ".prefab");											// Change .fbx extension to .prefab
			Directory.CreateDirectory(Path.GetFullPath(Path.GetDirectoryName(prefabPath)));
			
			// If the prefab already exists, don't create one
			if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
				return;
			
			// Try to load FBX
			GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
			if (fbxPrefab == null)
			{
				Debug.LogError($"No base prefab found at {fbxPath}. Unable to create prefab.");
				return;
			}

			// Create root object for new prefab. FBX is a child object.
			GameObject root = new GameObject();
			GameObject tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
			tempInstance.transform.parent = root.transform;
			
			PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
			GameObject.DestroyImmediate(root);
			
			Debug.Log($"✅ Created prefab variant: {prefabPath}");
		}
	}
}
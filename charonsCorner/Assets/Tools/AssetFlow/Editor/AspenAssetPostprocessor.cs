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
				// If asset is FBX and in the models folder
				if (assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)
				    && assetPath.StartsWith("Assets/Art/models"))
				{
					ExtractTexturesFromFBX(assetPath);

					// Create prefabs for actors
					if (assetPath.StartsWith("Assets/Art/models/actors"))
					{
						CreatePrefabFromFBX(assetPath);
					}
					
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
		}

		/// <summary>
		///    Extract textures from FBX. The textures will be extracted to same folder as FBX.
		/// </summary>
		/// <param name="fbxPath">The path of the FBX to extract textures from.</param>
		private static void ExtractTexturesFromFBX(string fbxPath)
		{
			string extractPath = Path.GetDirectoryName(fbxPath);

			// Get model importer
			var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
			if (importer == null)
			{
				Debug.LogError("No ModelImporter found at: " + fbxPath);
				return;
			}
					
			// Extract textures from model
			importer.ExtractTextures(extractPath);
		}
		

		/// <summary>
		///    Create a prefab from an FBX.
		/// The FBX prefab will be a nested object in the new prefab.
		/// </summary>
		/// <param name="fbxPath">The path of the FBX to create a prefab from.</param>
		private static void CreatePrefabFromFBX(string fbxPath)
		{
			// Convert the fbx path to the prefab path
			string fileName = Path.GetFileNameWithoutExtension(fbxPath);
			string prefabPath = $"Assets/Prefabs/actors/{fileName}.prefab";
			Directory.CreateDirectory(Path.GetFullPath(Path.GetDirectoryName(prefabPath)));
            
			// If the prefab already exists, don't create one
			if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
				return;
            
			// Try to load FBX
			GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
			if (fbxPrefab == null)
			{
				Debug.LogError($"No base prefab found at {fbxPath}. Unable to create prefab.");
			}

			// Create root object for new prefab. FBX is a child object.
			GameObject root = new GameObject();
			GameObject tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
			tempInstance.transform.parent = root.transform;
            
			PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
			GameObject.DestroyImmediate(root);
			Debug.Log($"Created prefab variant: {prefabPath}");
		}
	}
}
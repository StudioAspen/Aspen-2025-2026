using UnityEngine;
using UnityEditor;
using System.IO;

namespace Aspen.Tools.FileSync
{
	public class FileSyncApi : MonoBehaviour
	{
		private const string EXTERNAL_MODELS_DIR = "G:/Shared drives/StudioAspen/Art/Models";
		private const string INTERNAL_MODELS_DIR = "Assets/Art/Models";
		private const string EXTERNAL_ANIMATIONS_DIR = "G:/Shared drives/StudioAspen/Art/Animation";
		private const string INTERNAL_ANIMATIONS_DIR = "Assets/Art/Animations";
		private const string EXTERNAL_RIGS_DIR = "G:/Shared drives/StudioAspen/TechArt/Rigs";
		private const string INTERNAL_RIGS_DIR = "Assets/Art/Rigs";
		
		/// <summary>
		/// Sync all assets from external assets folder.
		/// </summary>
		public static void SyncAll()
		{
			SyncModels();
			SyncAnimations();
			SyncRigs();
			
			Debug.Log("Syncing all completed.");
		}
		
		/// <summary>
		/// Sync all models from external models folder
		/// </summary>
		public static void SyncModels()
		{
			SyncFbxFiles(EXTERNAL_MODELS_DIR, INTERNAL_MODELS_DIR);
			Debug.Log("Syncing models completed.");
		}
		
		/// <summary>
		/// Sync all animations from external animations folder
		/// </summary>
		public static void SyncAnimations()
		{
			SyncFbxFiles(EXTERNAL_ANIMATIONS_DIR, INTERNAL_ANIMATIONS_DIR);
			Debug.Log("Syncing animations completed.");
		}
		
		/// <summary>
		/// Sync all rigs from external rigs folder
		/// </summary>
		public static void SyncRigs()
		{
			SyncFbxFiles(EXTERNAL_RIGS_DIR, INTERNAL_RIGS_DIR);
			Debug.Log("Syncing rigs completed.");
		}

		/// <summary>
		/// Sync FBX files from the source directory hierarchy to the destination directory.
		/// </summary>
		/// <param name="sourceDir">The directory to copy FBX files from.</param>
		/// <param name="destinationDir">The directory to copy FBX files to.</param>
		private static void SyncFbxFiles(string sourceDir, string destinationDir)
		{
			Debug.Log($"Syncing from {sourceDir} to {destinationDir}");
			
			// Iterate over all FBX files in the directory hierarchy
			string[] filePaths = Directory.GetFiles(sourceDir, "*.fbx", SearchOption.AllDirectories);
			foreach (string filePath in filePaths)
			{
				// Get relative path to copy over to destination
				string relativePath = Path.GetRelativePath(sourceDir, filePath);
				string destPath = Path.Combine(destinationDir, relativePath).Replace("\\", "/");

				// Get and create directory
				string fullDestPath = Path.Combine(Application.dataPath, "../", destPath);
				Directory.CreateDirectory(Path.GetDirectoryName(fullDestPath));

				// Sync file if not synced yet or there are new saved changes
				if (!File.Exists(fullDestPath) ||
				    File.GetLastWriteTimeUtc(filePath) > File.GetLastWriteTimeUtc(fullDestPath))
				{
					File.Copy(filePath, fullDestPath, true);
					Debug.Log("📦 Synced: " + relativePath);
				}
			}

			AssetDatabase.Refresh();
		}
	}
}
using UnityEditor;
using UnityEngine;

namespace Aspen.Tools.Utils.FolderUtils
{
	public static class FolderUtils
	{
		/// <summary>
		/// If any directories in the path don't exist, make the directory.
		/// </summary>
		/// <param name="path">The path to be verified.</param>
		public static void EnsureDirectoriesExist(string path)
		{
			// Get list of directories
			string[] parts = path.Split('\\');

			// Iterate over all directories
			string current = "Assets";
			for (int i = 1; i < parts.Length; i++)
			{
				string next = current + "/" + parts[i];
				
				// Create directory if it doesn't exist
				if (!AssetDatabase.IsValidFolder(next))
				{
					AssetDatabase.CreateFolder(current, parts[i]);
				}

				current = next;
			}

			AssetDatabase.Refresh();
		}
	}
}
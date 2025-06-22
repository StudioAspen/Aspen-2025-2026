using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspen.Tools.FileSync
{
	public class FileSyncUI : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset;

		/// <summary>
		/// Shows the File Sync window.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync")]
		public static void ShowFileSyncWindow()
		{
			FileSyncUI window = GetWindow<FileSyncUI>();
			window.maxSize = new Vector2(300, 75);
			window.titleContent = new GUIContent("FileSync");
		}

		/// <summary>
		/// Initialize the GUI.
		/// </summary>
		public void CreateGUI()
		{
			// Add visual tree asset to ui
			VisualElement root = rootVisualElement;
			VisualElement visualTreeAsset = m_VisualTreeAsset.Instantiate();
			root.Add(visualTreeAsset);

			// Sync all setup
			Button syncAllButton = root.Query<Button>(name = "SyncAllButton");
			syncAllButton.clicked += FileSyncAPI.SyncAll;
			
			// Sync models setup
			Button syncModelsButton = root.Query<Button>(name = "SyncModelsButton");
			syncModelsButton.clicked += FileSyncAPI.SyncModels;
			
			// Sync animations setup
			Button syncAnimationsButton = root.Query<Button>(name = "SyncAnimationsButton");
			syncAnimationsButton.clicked += FileSyncAPI.SyncAnimations;
		}
	}
}

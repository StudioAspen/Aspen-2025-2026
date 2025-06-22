using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspen.Tools.FileSync
{
	public class FileSyncUi : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset;
	
		/// <summary>
		/// Creates a menu item for 'Sync All'.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync/Sync All", false, 21)]
		public static void syncAll()
		{
			FileSyncApi.SyncAll();
		}
		
		/// <summary>
		/// Creates a menu item for 'Sync Models'.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync/Sync Models", false, 22)]
		public static void SyncModels()
		{
			FileSyncApi.SyncModels();
		}

		/// <summary>
		/// Creates a menu item for 'Sync Animations'.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync/Sync Animations", false, 23)]
		public static void SyncAnimations()
		{
			FileSyncApi.SyncAnimations();
		}
		
		/// <summary>
		/// Creates a menu item for 'Sync Rigs'.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync/Sync Rigs", false, 24)]
		public static void SyncRigs()
		{
			FileSyncApi.SyncRigs();
		}
		
		/// <summary>
		/// Shows the File Sync window.
		/// </summary>
		[MenuItem("Aspen/Assets/FileSync/Open Window", false, 1)]
		public static void ShowFileSyncWindow()
		{
			FileSyncUi window = GetWindow<FileSyncUi>();
			window.maxSize = new Vector2(300, 100);
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
			syncAllButton.clicked += FileSyncApi.SyncAll;
			
			// Sync models setup
			Button syncModelsButton = root.Query<Button>(name = "SyncModelsButton");
			syncModelsButton.clicked += FileSyncApi.SyncModels;
			
			// Sync animations setup
			Button syncAnimationsButton = root.Query<Button>(name = "SyncAnimationsButton");
			syncAnimationsButton.clicked += FileSyncApi.SyncAnimations;
			
			// Sync animations setup
			Button syncRigsButton = root.Query<Button>(name = "SyncRigsButton");
			syncRigsButton.clicked += FileSyncApi.SyncRigs;
		}
	}
}

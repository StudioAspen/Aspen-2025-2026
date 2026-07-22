using UnityEditor;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Editor preferences specific to the MMF Player .
	/// </summary>
	public static class MMF_PlayerEditorSettings
	{
		public static bool PreviewBarSettingCached;
		public static bool CachedPreviewBarEnabled;

		private const string _showPreviewBarPrefKey = "MMShowMMFPlayerPreview";

		[MenuItem("Tools/More Mountains/MMFeedbacks/Enable MMF Player Preview", false, 50)]
		private static void EnableMMFPlayerPreviewBar()
		{
			SetPreviewBarEnabled(true);
		}

		[MenuItem("Tools/More Mountains/MMFeedbacks/Enable MMF Player Preview", true)]
		private static bool EnableMMFPlayerPreviewBarValidation()
		{
			return !MMFPlayerPreviewBarEnabled;
		}

		[MenuItem("Tools/More Mountains/MMFeedbacks/Disable MMF Player Preview", false, 51)]
		private static void DisableMMFPlayerPreviewBar()
		{
			SetPreviewBarEnabled(false);
		}

		[MenuItem("Tools/More Mountains/MMFeedbacks/Disable MMF Player Preview", true)]
		private static bool DisableMMFPlayerPreviewBarValidation()
		{
			return MMFPlayerPreviewBarEnabled;
		}

		/// <summary>
		/// Checks editor prefs to see if the MMF Player preview bar is enabled or not, defaults to false
		/// </summary>
		public static bool MMFPlayerPreviewBarEnabled
		{
			get
			{
				if (PreviewBarSettingCached)
				{
					return CachedPreviewBarEnabled;
				}

				if (EditorPrefs.HasKey(_showPreviewBarPrefKey))
				{
					CachedPreviewBarEnabled = EditorPrefs.GetBool(_showPreviewBarPrefKey);
				}
				else
				{
					EditorPrefs.SetBool(_showPreviewBarPrefKey, false);
					CachedPreviewBarEnabled = false;
				}

				PreviewBarSettingCached = true;
				return CachedPreviewBarEnabled;
			}
		}

		private static void SetPreviewBarEnabled(bool status)
		{
			EditorPrefs.SetBool(_showPreviewBarPrefKey, status);
			CachedPreviewBarEnabled = status;
			PreviewBarSettingCached = true;
			SceneView.RepaintAll();
		}
	}
}


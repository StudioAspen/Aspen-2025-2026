using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
#if MM_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	public class MMHDRPHelpers : MonoBehaviour
	{
		#if UNITY_EDITOR && MM_HDRP
		public static void GetOrCreateVolume<T, U>(MMF_Player owner, string feedbackName) where T:VolumeComponent where U:MMShaker
		{
			string additions = owner.name + " "+feedbackName+" feedback automatic shaker setup : ";
			
			// looks for the main camera 
			/*HDAdditionalCameraData cameraData = Camera.main.get();
			cameraData.renderPostProcessing = true;
			additions += "Set PostProcessing:true on the "+Camera.main.name+" camera. ";*/
			
			if (Application.isPlaying)
			{
				Debug.LogWarning("Automatic shaker setup is only available outside of play mode.");
				return;
			}
				
			// looks for a Volume
			Volume volume = (Volume)Object.FindAnyObjectByType(typeof(Volume));
			if (volume == null)
			{
				GameObject postProcessingObject = GameObject.Instantiate(Resources.Load<GameObject>("MMDefaultHDRPVolume"));
				volume = postProcessingObject.GetComponent<Volume>();
				additions += "Added a Volume to the scene. ";
			}
			
			// looks for a setting on the volume
			T effect;
			if (!volume.sharedProfile.TryGet(out effect))
			{
				effect = volume.sharedProfile.Add<T>();
				AssetDatabase.AddObjectToAsset(effect, volume.sharedProfile);
				EditorUtility.SetDirty(volume.sharedProfile);
				AssetDatabase.SaveAssets();
				additions += "Added a "+feedbackName+" post process effect to the "+volume.gameObject.name+" Volume. ";
			}
			
			// looks for a matching shaker
			U shaker = volume.GetComponent<U>();
			if (shaker == null)
			{
				shaker = volume.gameObject.AddComponent<U>();
				additions += "Added a "+feedbackName+" Shaker to the "+volume.gameObject.name+" Post Process Volume. ";
			}
			
			MMDebug.DebugLogInfo( additions + "You're all set.");
		}
		#endif
	}
}

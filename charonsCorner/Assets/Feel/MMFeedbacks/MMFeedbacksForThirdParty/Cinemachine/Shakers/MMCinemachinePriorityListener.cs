using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this to a Cinemachine virtual camera and it'll be able to listen to MMCinemachinePriorityEvent, usually triggered by a MMFeedbackCinemachineTransition
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Priority Listener")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineVirtualCameraBase))]
	#endif
	public class MMCinemachinePriorityListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		[Header("Priority Listener")]
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		protected CinemachineVirtualCameraBase _camera;
		protected int _initialPriority;
		#endif
        
		/// <summary>
		/// On Awake we store our virtual camera
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_camera = this.gameObject.GetComponent<CinemachineVirtualCameraBase>();
			#endif
			#if MM_CINEMACHINE 
			_initialPriority = _camera.Priority;
			#elif MM_CINEMACHINE3
			_initialPriority = _camera.Priority.Value; 
			#endif
		}

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		/// <summary>
		/// When we get an event we change our priorities if needed
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="forceMaxPriority"></param>
		/// <param name="newPriority"></param>
		/// <param name="forceTransition"></param>
		/// <param name="blendDefinition"></param>
		/// <param name="resetValuesAfterTransition"></param>
		public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			StartCoroutine(OnMMCinemachinePriorityEventCo(channelData, forceMaxPriority, newPriority, forceTransition,
				blendDefinition, resetValuesAfterTransition, timescaleMode, restore));
		}

		protected virtual IEnumerator OnMMCinemachinePriorityEventCo(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			yield return null;
			TimescaleMode = timescaleMode;
			if (MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				if (restore)
				{
					SetPriority(_initialPriority);	
					yield break;
				}
				SetPriority(newPriority);
			}
			else
			{
				if (forceMaxPriority)
				{
					if (restore)
					{
						SetPriority(_initialPriority);	
						yield break;;
					}
					SetPriority(0);
				}
			}
		}
		#endif

		protected virtual void SetPriority(int newPriority)
		{
			#if MM_CINEMACHINE 
			_camera.Priority = newPriority;
			#elif MM_CINEMACHINE3
			PrioritySettings prioritySettings = _camera.Priority;
			prioritySettings.Value = newPriority;
			_camera.Priority = prioritySettings;
			#endif
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
			#endif
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
			#endif
		}
	}

	/// <summary>
	/// An event used to pilot priorities on cinemachine virtual cameras and brain transitions
	/// </summary>
	public struct MMCinemachinePriorityEvent
	{
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false);
		static public void Trigger(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			OnEvent?.Invoke(channelData, forceMaxPriority, newPriority, forceTransition, blendDefinition, resetValuesAfterTransition, timescaleMode, restore);
		}
		#endif
	}
}
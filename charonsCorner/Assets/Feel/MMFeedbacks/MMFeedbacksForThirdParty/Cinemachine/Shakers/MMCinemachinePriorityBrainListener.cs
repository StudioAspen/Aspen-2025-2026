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
	/// Add this to a Cinemachine brain and it'll be able to accept custom blend transitions (used with MMFeedbackCinemachineTransition)
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Priority Brain Listener")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineBrain))]
	#endif
	public class MMCinemachinePriorityBrainListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
    
		#if MM_CINEMACHINE || MM_CINEMACHINE3
		protected CinemachineBrain _brain;
		protected CinemachineBlendDefinition _initialDefinition;
		#endif
		protected Coroutine _coroutine;

		/// <summary>
		/// On Awake we grab our brain
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_brain = this.gameObject.GetComponent<CinemachineBrain>();
			#endif
		}

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		/// <summary>
		/// When getting an event we change our default transition if needed
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="forceMaxPriority"></param>
		/// <param name="newPriority"></param>
		/// <param name="forceTransition"></param>
		/// <param name="blendDefinition"></param>
		/// <param name="resetValuesAfterTransition"></param>
		public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			if (forceTransition)
			{
				if (_coroutine != null)
				{
					StopCoroutine(_coroutine);
				}
				else
				{
					#if MM_CINEMACHINE
					_initialDefinition = _brain.m_DefaultBlend;
					#elif MM_CINEMACHINE3
					_initialDefinition = _brain.DefaultBlend;
					#endif
				}
				#if MM_CINEMACHINE
					_brain.m_DefaultBlend = blendDefinition;
				#elif MM_CINEMACHINE3
					_brain.DefaultBlend = blendDefinition;
				#endif
				TimescaleMode = timescaleMode;
				#if MM_CINEMACHINE
				_coroutine = StartCoroutine(ResetBlendDefinition(blendDefinition.m_Time));    
				#elif MM_CINEMACHINE3
				_coroutine = StartCoroutine(ResetBlendDefinition(blendDefinition.Time));    
				#endif            
			}
		}
		#endif

		/// <summary>
		/// a coroutine used to reset the default transition to its initial value
		/// </summary>
		/// <param name="delay"></param>
		/// <returns></returns>
		protected virtual IEnumerator ResetBlendDefinition(float delay)
		{
			yield return null;
			yield return null;
			for (float timer = 0; timer < delay; timer += GetDeltaTime())
			{
				yield return null;
			}
			#if MM_CINEMACHINE
			_brain.m_DefaultBlend = _initialDefinition;
			#elif MM_CINEMACHINE3
			_brain.DefaultBlend = _initialDefinition;
			#endif
			_coroutine = null;
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			_coroutine = null;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
			#endif
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
			_coroutine = null;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
			#endif
		}
	}
}
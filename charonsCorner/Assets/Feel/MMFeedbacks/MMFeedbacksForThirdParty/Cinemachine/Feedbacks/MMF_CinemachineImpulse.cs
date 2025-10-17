using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	[AddComponentMenu("")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[FeedbackPath("Camera/Cinemachine Impulse")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.Cinemachine")]
	[FeedbackHelp("This feedback lets you trigger a Cinemachine Impulse event. You'll need a Cinemachine Impulse Listener on your camera for this to work.")]
	public class MMF_CinemachineImpulse : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif
		public override bool HasRandomness => true;

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		[MMFInspectorGroup("Cinemachine Impulse", true, 28)]
		/// the impulse definition to broadcast
		[Tooltip("the impulse definition to broadcast")]
		public CinemachineImpulseDefinition m_ImpulseDefinition = new CinemachineImpulseDefinition();
		/// the velocity to apply to the impulse shake
		[Tooltip("the velocity to apply to the impulse shake")]
		public Vector3 Velocity;
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback")]
		public bool ClearImpulseOnStop = false;
		#endif
		
		[Header("Gizmos")]
		/// whether or not to draw gizmos to showcase the various distance properties of this feedback, when applicable. Dissipation distance in blue, impact radius in yellow.
		[Tooltip("whether or not to draw gizmos to showcase the various distance properties of this feedback, when applicable. Dissipation distance in blue, impact radius in yellow.")]
		public bool DrawGizmos = false;
		
		#if MM_CINEMACHINE
		/// the duration of this feedback is the duration of the impulse
		public override float FeedbackDuration { get { return m_ImpulseDefinition != null ? m_ImpulseDefinition.m_TimeEnvelope.Duration : 0f; } }
		#elif MM_CINEMACHINE3
		/// the duration of this feedback is the duration of the impulse
		public override float FeedbackDuration { get { return m_ImpulseDefinition != null ? m_ImpulseDefinition.TimeEnvelope.Duration : 0f; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			CinemachineImpulseManager.Instance.IgnoreTimeScale = !InScaledTimescaleMode;
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			m_ImpulseDefinition.CreateEvent(position, Velocity * intensityMultiplier);
			#endif
		}

		/// <summary>
		/// Stops the animation if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}

		/// <summary>
		/// When adding the feedback we initialize its cinemachine impulse definition
		/// </summary>
		public override void OnAddFeedback()
		{
			#if MM_CINEMACHINE 
			// sets the feedback properties
			if (this.m_ImpulseDefinition == null)
			{
				this.m_ImpulseDefinition = new CinemachineImpulseDefinition();
			}
			this.m_ImpulseDefinition.m_RawSignal = Resources.Load<NoiseSettings>("MM_6D_Shake");
			this.Velocity = new Vector3(5f, 5f, 5f);
			#elif MM_CINEMACHINE3
			// sets the feedback properties
			if (this.m_ImpulseDefinition == null)
			{
				this.m_ImpulseDefinition = new CinemachineImpulseDefinition();
			}
			this.m_ImpulseDefinition.RawSignal = Resources.Load<NoiseSettings>("MM_6D_Shake");
			this.Velocity = new Vector3(5f, 5f, 5f);
			#endif
		}

		/// <summary>
		/// Draws dissipation distance and impact distance gizmos if necessary
		/// </summary>
		public override void OnDrawGizmosSelectedHandler()
		{
			if (!DrawGizmos)
			{
				return;
			}
			#if MM_CINEMACHINE 
			{
				if ( (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Dissipating)
				     || (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Propagating)
				     || (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy) )
				{
					Gizmos.color = MMColors.Aqua;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.m_DissipationDistance);
				}
				if (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy)
				{
					Gizmos.color = MMColors.ReunoYellow;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.m_ImpactRadius);
				}
			}
			#elif MM_CINEMACHINE3
			if (this.m_ImpulseDefinition != null)
			{
				if ( (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Dissipating)
					 || (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Propagating)
					 || (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy) )
				{
					Gizmos.color = MMColors.Aqua;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.DissipationDistance);
				}
				if (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy)
				{
					Gizmos.color = MMColors.ReunoYellow;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.ImpactRadius);
				}
			}
			#endif
		}
		
		/// <summary>
		/// Automatically adds a Cinemachine Impulse Listener to the camera
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			MMCinemachineHelpers.AutomaticCinemachineShakersSetup(Owner, "CinemachineImpulse");
		}
	}
}
using MoreMountains.Tools;
using UnityEngine;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the character spacing of a target TMP over time
	/// </summary>
	[AddComponentMenu("")]
	[System.Serializable]
	[FeedbackHelp("This feedback lets you control the character spacing of a target TMP over time.")]
	#if MM_UGUI2
	[FeedbackPath("TextMesh Pro/TMP Character Spacing")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.TextMeshPro")]
	public class MMF_TMPCharacterSpacing : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTMPText be set to be able to work properly. You can set one below."; } }
		#endif
        
		#if UNITY_EDITOR && MM_UGUI2
		public override bool EvaluateRequiresSetup() { return (TargetTMPText == null); }
		public override string RequiredTargetText { get { return TargetTMPText != null ? TargetTMPText.name : "";  } }
		#endif
        
		#if MM_UGUI2
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => TargetTMPText = FindAutomatedTarget<TMP_Text>();
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the TMP_Text component to control
		[Tooltip("the TMP_Text component to control")]
		public TMP_Text TargetTMPText;
		#endif

		[MMFInspectorGroup("Character Spacing", true, 16)]
		/// the curve to tween on
		[Tooltip("the curve to tween on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType CharacterSpacingCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 1f;
		/// the value to move to in instant mode
		[Tooltip("the value to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantSpacing;
		/// the value to move to in destination mode
		[Tooltip("the value to move to in destination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationSpacing;
        
		protected override void FillTargets()
		{
			#if MM_UGUI2
			if (TargetTMPText == null)
			{
				return;
			}

			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetTMPText.gameObject;
			receiver.TargetComponent = TargetTMPText;
			receiver.TargetPropertyName = "characterSpacing";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = CharacterSpacingCurve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantSpacing;
			target.ToDestinationLevel = DestinationSpacing;

			_targets.Add(target);
			#endif
		}

	}
}
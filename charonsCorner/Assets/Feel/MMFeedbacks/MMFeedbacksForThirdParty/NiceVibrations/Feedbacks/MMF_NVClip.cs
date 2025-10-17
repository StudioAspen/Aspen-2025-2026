using UnityEngine;
using MoreMountains.Feedbacks;

#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
using Lofelt.NiceVibrations;
using MoreMountains.Tools;
#endif

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this feedback to play a .haptic clip, optionally randomizing its level and frequency
	/// </summary>
	[AddComponentMenu("")]
	#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
	[FeedbackPath("Haptics/Haptic Clip")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.NiceVibrations")]
	[FeedbackHelp("This feedback will let you play a haptic clip, and randomize its level and frequency.")]
	public class MMF_NVClip : MMF_Feedback
	{
		#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override bool HasCustomInspectors => true;
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HapticsColor; } }
		public override bool EvaluateRequiresSetup() { return (Clip == null); }
		public override string RequiredTargetText { get { return Clip != null ? Clip.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a Clip be set to be able to work properly. You can set one below."; } }
		#endif
        
		[MMFInspectorGroup("Haptic Clip", true, 13, true)]
		/// the haptic clip to play with this feedback
		[Tooltip("the haptic clip to play with this feedback")]
		public HapticClip Clip;
		/// a preset to play should the device you're running your game on doesn't support playing haptic clips
		[Tooltip("a preset to play should the device you're running your game on doesn't support playing haptic clips")]
		public HapticPatterns.PresetType FallbackPreset = HapticPatterns.PresetType.LightImpact;
		/// whether or not this clip should play on a loop, until stopped (won't work on gamepads)
		[Tooltip("whether or not this clip should play on a loop, until stopped (won't work on gamepads)")]
		public bool Loop = false;
		/// at what timestamp this clip should start playing
		[Tooltip("at what timestamp this clip should start playing")]
		public float SeekTime = 0f;
		/// a debug button that lets you test the haptic file from its inspector
		public MMF_Button TestHapticButton;

		[MMFInspectorGroup("Audio To Haptic", true, 14)]
		
		/// the label of the MMSM Sound feedback you want to convert the audio clip from. If left empty, will find the first on this MMF Player
		[Tooltip("the label of the MMSM Sound feedback you want to convert the audio clip from. If left empty, will find the first on this MMF Player")]
		[MMFInformation("While you can set a clip in the field above, this feedback also offers the option to automatically convert a MMSM Sound feedback's audio clip into a haptic clip. " +
		                "This is a great way to save time, while retaining fine control over amplitude and frequency.\n\n " +
		                "To do it, you'll need a MMSM Sound feedback with a clip on that same MMF Player. If you have more than one, you can specify the label of the feedback you're after in the field below. " +
		                "Then, press the convert button. You can then press the Test button below to try your haptic and audio together and see if you like them.\n\n" +
		                "You can then normalize amplitude and/or frequency for gamepad to your liking. The first curve shows the haptic file for iOS/Android, the second curve shows rumble data.", MMFInformationAttribute.InformationType.Info, false)]
		public string MMSMSoundFeedbackLabel;
		
		/// the sample count is the resolution at which the haptic clip will be computed
		[Tooltip("the sample count is the resolution at which the haptic clip will be computed")]
		public int SampleCount = 256;

		[Header("Amplitude")] 
		/// whether or not to normalize amplitude for the gamepad rumble
		[Tooltip("whether or not to normalize amplitude for the gamepad rumble")]
		public bool NormalizeAmplitude = true;
		/// the factor to use when normalizing amplitude
		[Tooltip("the factor to use when normalizing amplitude")]
		[MMFCondition("NormalizeAmplitude", true)]
		public float NormalizeAmplitudeFactor = 1f;
	
		[Header("Frequency")]
		/// whether or not to normalize frequency for the gamepad rumble
		[Tooltip("whether or not to normalize frequency for the gamepad rumble")]
		public bool NormalizeFrequency = true;
		/// the factor to use when normalizing frequency
		[Tooltip("the factor to use when normalizing frequency")]
		[MMFCondition("NormalizeFrequency", true)]
		public float NormalizeFrequencyFactor = 1f;
		
		/// a test button to convert the MMSM Sound feedback's audio clip into a haptic clip and assign it to this feedback
		public MMF_Button ConvertButton;
		/// a test button to play both the haptic and sound at once
		public MMF_Button TestHapticAudioButton;
		
		public NVHapticData HapticData;

		[MMFInspectorGroup("Level", true, 14)]
		/// the minimum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)
		[Tooltip("the minimum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)")]
		[Range(0f, 5f)]
		public float MinLevel = 1f;
		/// the maximum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)
		[Tooltip("the maximum level at which this clip should play (level will be randomized between MinLevel and MaxLevel)")]
		[Range(0f, 5f)]
		public float MaxLevel = 1f;
        
		[MMFInspectorGroup("Frequency Shift", true, 15)]
		/// the minimum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxLevel)
		[Tooltip("the minimum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxFrequencyShift)")]
		[Range(-1f, 1f)]
		public float MinFrequencyShift = 0f;
		/// the maximum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxLevel)
		[Tooltip("the maximum frequency shift at which this clip should play (frequency shift will be randomized between MinFrequencyShift and MaxFrequencyShift)")]
		[Range(-1f, 1f)]
		public float MaxFrequencyShift = 0f;

		[MMFInspectorGroup("Settings", true, 16)]
		/// a set of settings you can tweak to specify how and when exactly this haptic should play
		[Tooltip("a set of settings you can tweak to specify how and when exactly this haptic should play")]
		public MMFeedbackNVSettings HapticSettings;

		
		/// <summary>
		/// On play, we load our clip, set its settings and play it
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || HapticSettings == null || !HapticSettings.CanPlay() || (Clip == null))
			{
				return;
			}

			PlayHapticClip();
		}

		/// <summary>
		/// Plays the haptic clip
		/// </summary>
		protected virtual void PlayHapticClip()
		{
			if (Clip == null)
			{
				return;
			}
			HapticController.Load(Clip);
			HapticSettings.SetGamepad();
			HapticController.fallbackPreset = FallbackPreset;
			HapticController.Loop(Loop);
			HapticController.Seek(SeekTime);
			HapticController.clipLevel = Random.Range(MinLevel, MaxLevel);
			HapticController.clipFrequencyShift = Random.Range(MinFrequencyShift, MaxFrequencyShift);
			HapticController.Play();
		}
        
		/// <summary>
		/// On stop we stop haptics
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!FeedbackTypeAuthorized)
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			HapticController.Stop();
		}
		
		/// <summary>
		/// Initializes custom buttons
		/// </summary>
		public override void InitializeCustomAttributes()
		{
			base.InitializeCustomAttributes();
			ConvertButton = new MMF_Button("Convert MMSM Sound feedback Audio Clip to Haptic", Convert);
			TestHapticAudioButton = new MMF_Button("Test Haptic and Audio", TestHapticAndAudio);
			TestHapticButton = new MMF_Button("Test Haptic", PlayHapticClip);
		}
		
		/// <summary>
		/// A debug method used from the inspector to test both the haptic and audio files playing at once
		/// </summary>
		protected virtual void TestHapticAndAudio()
		{
			MMF_MMSoundManagerSound soundFeedback = Owner.GetFeedbackOfType<MMF_MMSoundManagerSound>(MMSMSoundFeedbackLabel);
			if (soundFeedback != null)
			{
				soundFeedback.TestPlaySound();	
			}
			PlayHapticClip();
		}

		/// <summary>
		/// Tries and converts the MMSM Sound feedback's audio clip on the same MMF Player into a haptic clip and sets it as this feedback's haptic clip 
		/// </summary>
		protected virtual void Convert()
		{
			#if UNITY_EDITOR
			MMF_MMSoundManagerSound soundFeedback;
			if ((MMSMSoundFeedbackLabel == null) || (MMSMSoundFeedbackLabel == ""))
			{
				soundFeedback = Owner.GetFeedbackOfType<MMF_MMSoundManagerSound>();
				if (soundFeedback != null)
				{
					MMSMSoundFeedbackLabel = soundFeedback.Label;
				}
				else
				{
					Debug.LogError(this.Owner.name + " - NV Clip feedback : there is no MM Sound Manager Sound feedback on this MMF Player, nothing to convert.");
					return;
				}
			}
			else
			{
				soundFeedback = Owner.GetFeedbackOfType<MMF_MMSoundManagerSound>(MMSMSoundFeedbackLabel);
				if (soundFeedback == null)
				{
					Debug.LogError(this.Owner.name + " - NV Clip feedback : couldn't find a MM Sound Manager Sound feedback with this label: " + MMSMSoundFeedbackLabel);
					return;
				}
			}
			
			AudioClip clip = soundFeedback.Sfx;

			if (clip == null)
			{
				if (soundFeedback.RandomSfx.Length > 0)
				{
					clip = soundFeedback.RandomSfx[0];
				}

				if (clip == null)
				{
					Debug.LogError(this.Owner.name + " - NV Clip feedback : thee MM Sound Manager Sound feedback on this MMF Player doesn't have a clip, nothing to convert.");
					return;	
				}
			}
			
			string filePath = AssetDatabase.GetAssetPath(clip);
			string folderPath = Path.GetDirectoryName(filePath);
			string newFileName = Path.GetFileNameWithoutExtension(filePath)+".haptic";


			HapticData = AudioToHapticConverter.GenerateHapticFile(clip, folderPath, newFileName, 
																		NormalizeAmplitude, NormalizeAmplitudeFactor, 
																		NormalizeFrequency, NormalizeFrequencyFactor, 
																		SampleCount);
			Clip = HapticData.Clip;
			CacheRequiresSetup();
			#endif
		}
		
		#else
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }
		#endif
	}    
}
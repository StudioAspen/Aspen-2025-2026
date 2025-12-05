using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.Scripting.APIUpdating;
using Object = UnityEngine.Object;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A scriptable object used to store data for MMSoundManager play
	/// </summary>
	[Serializable]
	[CreateAssetMenu(menuName = "MoreMountains/Audio/MMF_SoundData")]
	public class MMF_MMSoundManagerSoundData : ScriptableObject
	{
		[Header("Sound")]
		/// the sound clip to play
		[Tooltip("the sound clip to play")]
		public AudioClip Sfx;

		[Header("Random Sound")]
		/// an array to pick a random sfx from
		[Tooltip("an array to pick a random sfx from")]
		public AudioClip[] RandomSfx;
		/// if this is true, random sfx audio clips will be played in sequential order instead of at random
		[Tooltip("if this is true, random sfx audio clips will be played in sequential order instead of at random")]
		public bool SequentialOrder = false;
		/// if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called
		[Tooltip("if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called")]
		[MMFCondition("SequentialOrder", true)]
		public bool SequentialOrderHoldLast = false;
		/// if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored
		[Tooltip("if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored")]
		[MMFCondition("SequentialOrderHoldLast", true)]
		public float SequentialOrderHoldCooldownDuration = 2f;
		/// if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over
		[Tooltip("if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over")]
		public bool RandomUnique = false;
        
		[Header("Sound Properties")]
		[Header("Volume")]
		/// the minimum volume to play the sound at
		[Tooltip("the minimum volume to play the sound at")]
		[Range(0f,2f)]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("the maximum volume to play the sound at")]
		[Range(0f,2f)]
		public float MaxVolume = 1f;

		[Header("Pitch")]
		/// the minimum pitch to play the sound at
		[Tooltip("the minimum pitch to play the sound at")]
		[Range(-3f,3f)]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("the maximum pitch to play the sound at")]
		[Range(-3f,3f)]
		public float MaxPitch = 1f;

		[Header("Time")]
		/// a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time) 
		[Tooltip("a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time)")]
		[MMFVector("Min", "Max")]
		public Vector2 PlaybackTime = new Vector2(0f, 0f);
		/// a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.
		[Tooltip("a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackDuration = new Vector2(0f, 0f);
		
		[Header("Sound Manager Options")]
		/// the track on which to play the sound. Pick the one that matches the nature of your sound
		[Tooltip("the track on which to play the sound. Pick the one that matches the nature of your sound")]
		public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
		/// the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards. 
		[Tooltip("the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards.")]
		public int ID = 0;
		/// the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.
		[Tooltip("the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.")]
		public AudioMixerGroup AudioGroup = null;
		/// if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here 
		[Tooltip("if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here")]
		public AudioSource RecycleAudioSource = null;
		/// whether or not this sound should loop
		[Tooltip("whether or not this sound should loop")]
		public bool Loop = false;
		/// whether or not this sound should continue playing when transitioning to another scene
		[Tooltip("whether or not this sound should continue playing when transitioning to another scene")]
		public bool Persistent = false;
		/// whether or not this sound should play if the same sound clip is already playing
		[Tooltip("whether or not this sound should play if the same sound clip is already playing")]
		public bool DoNotPlayIfClipAlreadyPlaying = false;
		/// the maximum amount of instances of this sound allowed to play at once. use -1 for unlimited concurrent plays 
		[Tooltip("the maximum amount of instances of this sound allowed to play at once. use -1 for unlimited concurrent plays")]
		public int MaximumConcurrentInstances = -1;
		/// if this is true, this sound will stop playing when stopping the feedback
		[Tooltip("if this is true, this sound will stop playing when stopping the feedback")]
		public bool StopSoundOnFeedbackStop = false;
        
		[Header("Fade In")]
		/// whether or not to fade this sound in when playing it
		[Tooltip("whether or not to fade this sound in when playing it")]
		public bool FadeIn = false;
		/// if fading, the volume at which to start the fade
		[Tooltip("if fading, the volume at which to start the fade")]
		[MMCondition("FadeIn", true)]
		public float FadeInInitialVolume = 0f;
		/// if fading, the duration of the fade, in seconds
		[Tooltip("if fading, the duration of the fade, in seconds")]
		[MMCondition("FadeIn", true)]
		public float FadeInDuration = 1f;
		/// if fading, the tween over which to fade the sound 
		[Tooltip("if fading, the tween over which to fade the sound ")]
		public MMTweenType FadeInTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic, "FadeIn");
		
		[Header("Fade Out")]
		/// whether or not to fade this sound in when stopping the feedback
		[Tooltip("whether or not to fade this sound in when stopping the feedback")]
		public bool FadeOutOnStop = false;
		/// if fading out, the duration of the fade, in seconds
		[Tooltip("if fading out, the duration of the fade, in seconds")]
		[MMCondition("FadeOutOnStop", true)]
		public float FadeOutDuration = 1f;
		/// if fading out, the tween over which to fade the sound 
		[Tooltip("if fading out, the tween over which to fade the sound ")]
		public MMTweenType FadeOutTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic, "FadeOutOnStop");
        
		[Header("Solo")]
		/// whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing")]
		public bool SoloSingleTrack = false;
		/// whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing
		[Tooltip("whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing")]
		public bool SoloAllTracks = false;
		/// if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing
		[Tooltip("if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing")]
		public bool AutoUnSoloOnEnd = false;

		[Header("Spatial Settings")]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
		[Range(0f,1f)]
		public float SpatialBlend;
		/// a Transform this sound can 'attach' to and follow it along as it plays - when used on a feedback, will only apply if the feedback's AttachToTransform is empty
		[Tooltip("a Transform this sound can 'attach' to and follow it along as it plays - when used on a feedback, will only apply if the feedback's AttachToTransform is empty")]
		public Transform AttachToTransform;
		
		[Header("Effects")]
		/// Bypass effects (Applied from filter components or global listener filters).
		[Tooltip("Bypass effects (Applied from filter components or global listener filters).")]
		public bool BypassEffects = false;
		/// When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.
		[Tooltip("When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.")]
		public bool BypassListenerEffects = false;
		/// When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.
		[Tooltip("When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.")]
		public bool BypassReverbZones = false;
		/// Sets the priority of the AudioSource.
		[Tooltip("Sets the priority of the AudioSource.")]
		[Range(0, 256)]
		public int Priority = 128;
		/// The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.
		[Tooltip("The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.")]
		[Range(0f,1.1f)]
		public float ReverbZoneMix = 1f;
        
		[Header("3D Sound Settings")]
		/// Sets the Doppler scale for this AudioSource.
		[Tooltip("Sets the Doppler scale for this AudioSource.")]
		[Range(0f,5f)]
		public float DopplerLevel = 1f;
		/// Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.
		[Tooltip("Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.")]
		[Range(0,360)]
		public int Spread = 0;
		/// Sets/Gets how the AudioSource attenuates over distance.
		[Tooltip("Sets/Gets how the AudioSource attenuates over distance.")]
		public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
		/// Within the Min distance the AudioSource will cease to grow louder in volume.
		[Tooltip("Within the Min distance the AudioSource will cease to grow louder in volume.")]
		public float MinDistance = 1f;
		/// (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
		[Tooltip("(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.")]
		public float MaxDistance = 500f;
		/// whether or not to use a custom curve for custom volume rolloff
		[Tooltip("whether or not to use a custom curve for custom volume rolloff")]
		public bool UseCustomRolloffCurve = false;
		/// the curve to use for custom volume rolloff if UseCustomRolloffCurve is true
		[Tooltip("the curve to use for custom volume rolloff if UseCustomRolloffCurve is true")]
		[MMCondition("UseCustomRolloffCurve", true)]
		public AnimationCurve CustomRolloffCurve;
		/// whether or not to use a custom curve for spatial blend
		[Tooltip("whether or not to use a custom curve for spatial blend")]
		public bool UseSpatialBlendCurve = false;
		/// the curve to use for custom spatial blend if UseSpatialBlendCurve is true
		[Tooltip("the curve to use for custom spatial blend if UseSpatialBlendCurve is true")]
		[MMCondition("UseSpatialBlendCurve", true)]
		public AnimationCurve SpatialBlendCurve;
		/// whether or not to use a custom curve for reverb zone mix
		[Tooltip("whether or not to use a custom curve for reverb zone mix")]
		public bool UseReverbZoneMixCurve = false;
		/// the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true
		[Tooltip("the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true")]
		[MMCondition("UseReverbZoneMixCurve", true)]
		public AnimationCurve ReverbZoneMixCurve;
		/// whether or not to use a custom curve for spread
		[Tooltip("whether or not to use a custom curve for spread")]
		public bool UseSpreadCurve = false;
		/// the curve to use for custom spread if UseSpreadCurve is true
		[Tooltip("the curve to use for custom spread if UseSpreadCurve is true")]
		[MMCondition("UseSpreadCurve", true)]
		public AnimationCurve SpreadCurve;
		
		[MMInspectorButton("TestPlaySound")]
		public bool TestPlaySoundButton;
		
		protected AudioClip _randomClip;
		protected MMShufflebag<int> _randomUniqueShuffleBag;
		protected int _currentIndex;
		protected float _randomPlaybackTime;
		protected float _randomPlaybackDuration;
		protected MMSoundManagerPlayOptions _options;
		protected AudioSource _playedAudioSource;
		protected AudioClip _lastPlayedClip;
		protected bool _initialized = false;
		protected AudioSource _editorAudioSource;
		protected float _lastPlayTimestamp = -float.MaxValue;

		protected virtual void Initialization()
		{
			_lastPlayedClip = null;
			
			if (RandomSfx == null)
			{
				RandomSfx = Array.Empty<AudioClip>();
			}
			
			if (RandomUnique)
			{
				_randomUniqueShuffleBag = new MMShufflebag<int>(RandomSfx.Length);
				for (int i = 0; i < RandomSfx.Length; i++)
				{
					_randomUniqueShuffleBag.Add(i,1);
				}
			}
			
			_initialized  = true;
		}
		
		public virtual void Play(Vector3 position)
		{
			if (!_initialized || (RandomUnique && _randomUniqueShuffleBag == null))
			{
				Initialization();
			}
			
			if (RandomSfx.Length > 0)
			{
				_randomClip = PickRandomClip();

				if (_randomClip != null)
				{
					PlaySound(_randomClip, position);
					return;
				}
			}
			
			if ((Sfx != null))
			{
				PlaySound(Sfx, position);
				return;
			}
		}
		
		protected virtual AudioSource PlaySound(AudioClip sfx, Vector3 position)
		{
			if (DoNotPlayIfClipAlreadyPlaying) 
			{
				if ((MMSoundManager.Instance.FindByClip(sfx) != null) && (MMSoundManager.Instance.FindByClip(sfx).isPlaying))
				{
					return null;
				}
			}

			if (MaximumConcurrentInstances >= 0)
			{
				if (MMSoundManager.Instance.CurrentlyPlayingCount(sfx) >= MaximumConcurrentInstances)
				{
					return null;
				}
			}
			
			_lastPlayedClip = null;
            
			float volume = Random.Range(MinVolume, MaxVolume);
            
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();

			_options.MmSoundManagerTrack = MmSoundManagerTrack;
			_options.Location = position;
			_options.Loop = Loop;
			_options.Volume = volume;
			_options.ID = ID;
			_options.Fade = FadeIn;
			_options.FadeInitialVolume = FadeInInitialVolume;
			_options.FadeDuration = FadeInDuration;
			_options.FadeTween = FadeInTween;
			_options.Persistent = Persistent;
			_options.RecycleAudioSource = RecycleAudioSource;
			_options.AudioGroup = AudioGroup;
			_options.Pitch = pitch;
			_options.PlaybackTime = _randomPlaybackTime;
			_options.PlaybackDuration = _randomPlaybackDuration;
			_options.PanStereo = PanStereo;
			_options.SpatialBlend = SpatialBlend;
			_options.SoloSingleTrack = SoloSingleTrack;
			_options.SoloAllTracks = SoloAllTracks;
			_options.AutoUnSoloOnEnd = AutoUnSoloOnEnd;
			_options.BypassEffects = BypassEffects;
			_options.BypassListenerEffects = BypassListenerEffects;
			_options.BypassReverbZones = BypassReverbZones;
			_options.Priority = Priority;
			_options.ReverbZoneMix = ReverbZoneMix;
			_options.DopplerLevel = DopplerLevel;
			_options.Spread = Spread;
			_options.RolloffMode = RolloffMode;
			_options.MinDistance = MinDistance;
			_options.MaxDistance = MaxDistance;
			_options.AttachToTransform = AttachToTransform;
			_options.UseSpreadCurve = UseSpreadCurve;
			_options.SpreadCurve = SpreadCurve;
			_options.UseCustomRolloffCurve = UseCustomRolloffCurve;
			_options.CustomRolloffCurve = CustomRolloffCurve;
			_options.UseSpatialBlendCurve = UseSpatialBlendCurve;
			_options.SpatialBlendCurve = SpatialBlendCurve;
			_options.UseReverbZoneMixCurve = UseReverbZoneMixCurve;
			_options.ReverbZoneMixCurve = ReverbZoneMixCurve;
			_options.DoNotAutoRecycleIfNotDonePlaying = true;
			
			_playedAudioSource = MMSoundManagerSoundPlayEvent.Trigger(sfx, _options);
			_lastPlayedClip = sfx;

			return _playedAudioSource;
		}
		
		public virtual void RandomizeTimes()
		{
			_randomPlaybackTime = Random.Range(PlaybackTime.x, PlaybackTime.y);
			_randomPlaybackDuration = Random.Range(PlaybackDuration.x, PlaybackDuration.y);
		}
		
		protected virtual AudioClip PickRandomClip()
		{
			int newIndex = 0;
	        
			if (!SequentialOrder)
			{
				if (RandomUnique)
				{
					newIndex = _randomUniqueShuffleBag.Pick();
				}
				else
				{
					newIndex = Random.Range(0, RandomSfx.Length);	
				}
			}
			else
			{
				newIndex = _currentIndex;
		        
				if (newIndex >= RandomSfx.Length)
				{
					if (SequentialOrderHoldLast)
					{
						newIndex--;
						if (SequentialOrderHoldCooldownDuration > 0)
						{
							newIndex = 0;    
						}
					}
					else
					{
						newIndex = 0;
					}
				}
				_currentIndex = newIndex + 1;
			}
			return RandomSfx[newIndex];
		}
		
		public virtual async void TestPlaySound()
		{
			if (!_initialized || (RandomUnique && _randomUniqueShuffleBag == null))
			{
				Initialization();
			}
			
			AudioClip tmpAudioClip = null;
			
			if (Sfx != null)
			{
				tmpAudioClip = Sfx;
			}

			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				tmpAudioClip = PickRandomClip();
			}

			if (tmpAudioClip == null) 
			{
				Debug.LogError("This SoundData can't play in editor mode, you haven't set its Sfx.");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, SceneManager.GetActiveScene());
			temporaryAudioHost.transform.position = Vector3.zero;
			if (!Application.isPlaying)
			{
				temporaryAudioHost.AddComponent<MMForceDestroyInPlayMode>();
			}
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, _randomPlaybackTime, _randomPlaybackDuration);
			_lastPlayTimestamp = Time.time;
			_lastPlayedClip = tmpAudioClip;
			float length = 0f;
			if (tmpAudioClip != null)
			{
				length = (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : tmpAudioClip.length;
			}
			else
			{
				length = 10f;
			}
			length *= 1000;
			length = length / Mathf.Abs(pitch);
			await Task.Delay((int)length);
			Object.DestroyImmediate(temporaryAudioHost);
		}
		
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, float time, float playbackDuration)
		{
			audioSource.clip = sfx;
			audioSource.time = time;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.loop = false;
			audioSource.Play(); 
		}
	}
}

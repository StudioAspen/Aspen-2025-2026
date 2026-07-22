using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A ScriptableObject used to define a custom MMSoundManager track.
	/// Create one per custom track (e.g. Ambience, Voice, Foley) via Create > MoreMountains > Audio > MMSoundManagerTrack.
	/// Assign it to the CustomTracks list on your MMSoundManagerSettingsSO, and reference it in play options or feedbacks
	/// when using the "Other" track mode to route sounds through this custom track.
	/// </summary>
	[Serializable]
	[CreateAssetMenu(menuName = "MoreMountains/Audio/MMSoundManagerTrack")]
	public class MMSoundManagerCustomTrackSO : ScriptableObject
	{
		[Header("Mixer Group")]
		/// the AudioMixerGroup to route sounds on this track to
		[Tooltip("the AudioMixerGroup to route sounds on this track to")]
		public AudioMixerGroup AudioMixerGroup;
		
		[Header("Volume Control")]
		/// the name of the exposed volume parameter on the AudioMixer for this track
		[Tooltip("the name of the exposed volume parameter on the AudioMixer for this track")]
		public string VolumeParameter;
		/// the default volume of this track
		[Tooltip("the default volume of this track")]
		[Range(MMSoundManagerSettings._minimalVolume, MMSoundManagerSettings._maxVolume)]
		public float DefaultVolume = MMSoundManagerSettings._defaultVolume;
		/// whether this track is on by default
		[Tooltip("whether this track is on by default")]
		public bool DefaultOn = true;
	}
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This event will let you order the MMSoundManager to fade an entire track's sounds' volume towards the specified FinalVolume
	///
	/// Example : MMSoundManagerTrackFadeEvent.Trigger(MMSoundManager.MMSoundManagerTracks.Music, 2f, 0.5f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
	/// will fade the volume of the music track towards 0.5, over 2 seconds, using an ease in cubic tween 
	/// </summary>
	public struct MMSoundManagerTrackFadeEvent
	{
		public enum Modes { PlayFade, StopFade }

		/// whether we are fading a sound, or stopping an existing fade
		public Modes Mode;
		/// the track to fade the volume of
		public MMSoundManager.MMSoundManagerTracks Track;
		/// if Track is Other, the optional custom track SO to target
		public MMSoundManagerCustomTrackSO CustomTrack;
		/// the duration of the fade, in seconds
		public float FadeDuration;
		/// the final volume to fade towards
		public float FinalVolume;
		/// the tween to use when fading
		public MMTweenType FadeTween;
        
		public MMSoundManagerTrackFadeEvent(Modes mode, MMSoundManager.MMSoundManagerTracks track, float fadeDuration, float finalVolume, MMTweenType fadeTween, MMSoundManagerCustomTrackSO customTrack = null)
		{
			Mode = mode;
			Track = track;
			CustomTrack = customTrack;
			FadeDuration = fadeDuration;
			FinalVolume = finalVolume;
			FadeTween = fadeTween;
		}

		static MMSoundManagerTrackFadeEvent e;
		public static void Trigger(Modes mode, MMSoundManager.MMSoundManagerTracks track, float fadeDuration, float finalVolume, MMTweenType fadeTween, MMSoundManagerCustomTrackSO customTrack = null)
		{
			e.Mode = mode;
			e.Track = track;
			e.CustomTrack = customTrack;
			e.FadeDuration = fadeDuration;
			e.FinalVolume = finalVolume;
			e.FadeTween = fadeTween;
			MMEventManager.TriggerEvent(e);
		}
	}
}
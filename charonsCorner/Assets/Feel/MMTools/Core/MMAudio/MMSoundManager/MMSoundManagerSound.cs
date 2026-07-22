using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A simple struct used to store information about the sounds played by the MMSoundManager
	/// </summary>
	[Serializable]
	public struct MMSoundManagerSound
	{
		/// the ID of the sound 
		public int ID;
		/// the track the sound is being played on
		public MMSoundManager.MMSoundManagerTracks Track;
		/// if Track is Other, the optional custom track SO this sound is associated with
		public MMSoundManagerCustomTrackSO CustomTrack;
		/// the associated audiosource
		public AudioSource Source;
		/// whether or not this sound will play over multiple scenes
		public bool Persistent;

		public float PlaybackTime;
		public float PlaybackDuration;
	}
}
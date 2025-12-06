using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class manages an object pool of audiosources
	/// </summary>
	[Serializable]
	public class MMSoundManagerAudioPool 
	{
		protected List<AudioSource> _pool;
        
		/// <summary>
		/// Fills the pool with ready-to-use audiosources
		/// </summary>
		/// <param name="poolSize"></param>
		/// <param name="parent"></param>
		public virtual void FillAudioSourcePool(int poolSize, Transform parent)
		{
			if (_pool == null)
			{
				_pool = new List<AudioSource>();
			}
            
			if ((poolSize <= 0) || (_pool.Count >= poolSize))
			{
				return;
			}

			foreach (AudioSource source in _pool)
			{
				UnityEngine.Object.Destroy(source.gameObject);
			}

			for (int i = 0; i < poolSize; i++)
			{
				AddOneObjectToThePool(i, parent, false);
			}
		}

		/// <summary>
		/// Disables an audio source after it's done playing
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public virtual IEnumerator AutoDisableAudioSource(float duration, AudioSource source, AudioClip clip, bool doNotAutoRecycleIfNotDonePlaying, float playbackTime, float playbackDuration)
		{
			if (clip != null)
			{
				while (source.time == 0 && source.isPlaying)
				{
					yield return null;
				}
			}
			if (source.resource != null)
			{
				while (source.isPlaying)
				{
					yield return null;
				}
			}
			float initialWait = (playbackDuration > 0) ? playbackDuration : duration;
			yield return MMCoroutine.WaitForUnscaled(initialWait);
			if ((clip != null) && (source.clip != clip))
			{
				yield break;
			}
			if (doNotAutoRecycleIfNotDonePlaying)
			{
				float maxTime = 0;
				if (clip != null)
				{
					 maxTime = (playbackDuration > 0) ? playbackTime + playbackDuration : source.clip.length;	
				}
				else
				{
					maxTime = playbackTime + playbackDuration;
				}
				
				if (clip != null)
				{
					while ((source.time != 0) && (source.time <= maxTime))
					{
						yield return null;
					}
				}
				if (source.resource != null)
				{
					while (source.isPlaying)
					{
						yield return null;
					}
				}
			}
			source.gameObject.SetActive(false);
		}

		/// <summary>
		/// Pulls an available audio source from the pool
		/// </summary>
		/// <param name="poolCanExpand"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public virtual AudioSource GetAvailableAudioSource(bool poolCanExpand, Transform parent)
		{
			foreach (AudioSource source in _pool)
			{
				if (!source.gameObject.activeInHierarchy)
				{
					source.gameObject.SetActive(true);
					return source;
				}
			}

			if (poolCanExpand)
			{
				AudioSource tempSource = AddOneObjectToThePool(_pool.Count, parent, true);
				return tempSource;
			}

			return null;
		}

		protected virtual AudioSource AddOneObjectToThePool(int index, Transform parent, bool active)
		{
			GameObject temporaryAudioHost = new GameObject("MMAudioSourcePool_"+index);
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, parent.gameObject.scene);
			AudioSource tempSource = temporaryAudioHost.AddComponent<AudioSource>();
			MMFollowTarget followTarget = temporaryAudioHost.AddComponent<MMFollowTarget>();
			followTarget.enabled = false;
			followTarget.DisableSelfOnSetActiveFalse = true;
			temporaryAudioHost.transform.SetParent(parent);
			temporaryAudioHost.SetActive(active);
			_pool.Add(tempSource);
			return tempSource;
		}

		/// <summary>
		/// Stops an audiosource and returns it to the pool
		/// </summary>
		/// <param name="sourceToStop"></param>
		/// <returns></returns>
		public virtual bool FreeSound(AudioSource sourceToStop)
		{
			foreach (AudioSource source in _pool)
			{
				if (source == sourceToStop)
				{
					source.Stop();
					source.gameObject.SetActive(false);
					return true;
				}
			}
			return false;
		}
	}
}
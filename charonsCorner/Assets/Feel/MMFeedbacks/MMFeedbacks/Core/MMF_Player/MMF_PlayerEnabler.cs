using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A helper class added automatically by MMF_Player if they're in AutoPlayOnEnable mode
	/// This lets them play again should their parent game object be disabled/enabled
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_PlayerEnabler : MonoBehaviour
	{
		/// the MMF_Player to pilot
		public virtual MMF_Player TargetMmfPlayer { get; set; }
        
		/// <summary>
		/// On enable, we re-enable (and thus play) our MMF_Player if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((TargetMmfPlayer != null) && !TargetMmfPlayer.enabled && TargetMmfPlayer.AutoPlayOnEnable)
			{
				TargetMmfPlayer.enabled = true;
			}
		}
	}    
}
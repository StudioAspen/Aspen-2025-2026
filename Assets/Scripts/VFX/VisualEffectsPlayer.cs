using UnityEngine;
using UnityEngine.VFX;

namespace Aspen.VFX
{
	public class VisualEffectsPlayer : VFXPlayerBase
	{
		[SerializeField] private VisualEffect _visualEffect;
		
		/// <summary>
		/// Used to play visual effected created by Unity's Visual Effects Graph
		/// </summary>
		public override void Play()
		{
			_visualEffect.Play();
		}

		public override void Stop()
		{
			_visualEffect.Stop();
		}

		public override void Pause(bool val)
		{
			_visualEffect.pause = val;
		}
	}

}
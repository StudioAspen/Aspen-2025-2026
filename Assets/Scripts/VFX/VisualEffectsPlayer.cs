using UnityEngine;
using UnityEngine.VFX;

namespace Aspen
{
	public class VisualEffectsPlayer : VFXPlayerBase
	{
		[SerializeField] private VisualEffect _visualEffect;
		
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
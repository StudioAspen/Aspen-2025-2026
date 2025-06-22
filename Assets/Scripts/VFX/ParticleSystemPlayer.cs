using Aspen;
using UnityEngine;

namespace Aspen.VFX
{
	/// <summary>
	/// Used to play visual effected created by Unity's Particle System
	/// </summary>
	public class ParticleSystemPlayer : VFXPlayerBase
	{
		[SerializeField] private ParticleSystem _particleSystem;
    
		public override void Play()
		{
			_particleSystem.Play();
		}

		public override void Stop()
		{
			_particleSystem.Stop();
		}

		public override void Pause(bool val)
		{
			if (val)
			{
				_particleSystem.Pause();
			}
			else
			{
				Play();
			}
		}
	}

}
using UnityEngine;

namespace Aspen
{
	public abstract class VFXPlayerBase : MonoBehaviour
	{
		public abstract void Play();

		public abstract void Stop();

		public abstract void Pause(bool val);
	}

}
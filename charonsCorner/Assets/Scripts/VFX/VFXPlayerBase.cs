using UnityEngine;

namespace Aspen.VFX
{
	/// <summary>
	/// A base class that can be inherited. Extend behavior for visual effects.
	/// </summary>
	public abstract class VFXPlayerBase : MonoBehaviour
	{
		public abstract void Play();

		public abstract void Stop();

		public abstract void Pause(bool val);
	}

}
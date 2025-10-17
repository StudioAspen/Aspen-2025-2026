using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// An abstract class used to build spring components to pilot various properties (float, vector2, vector3, color, etc)
	/// </summary>
	[MMRequiresConstantRepaintOnlyWhenPlaying]
	public abstract class MMSpringComponentBase : MMMonoBehaviour
	{
		/// the different possible timescale modes for the spring
		public enum TimeScaleModes { Unscaled, Scaled }
		/// whether or not this spring has reached a low enough velocity to self disable
		public virtual bool LowVelocity => false;

		[MMInspectorGroup("Events", true, 16, true)] 
		public UnityEvent OnEquilibriumReached;
		
		protected float _velocityLowThreshold = 0.001f;
		
		/// <summary>
		/// Sets the threshold under which the spring will consider its velocity as too low and will self disable
		/// </summary>
		/// <param name="threshold"></param>
		public virtual void SetVelocityLowThreshold(float threshold)
		{
			_velocityLowThreshold = threshold;
		}
		
		/// <summary>
		/// On awake we self disable
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			this.enabled = false;
		}

		/// <summary>
		/// On update we update our spring value and self disable if needed
		/// </summary>
		protected virtual void Update()
		{
			UpdateSpringValue();
			SelfDisable();
		}

		/// <summary>
		/// Activates this component
		/// </summary>
		protected virtual void Activate()
		{
			this.enabled = true;
		}

		/// <summary>
		/// Disables this component
		/// </summary>
		protected virtual void SelfDisable()
		{
			if (LowVelocity)
			{
				if (OnEquilibriumReached != null)
				{
					OnEquilibriumReached.Invoke();
				}
				Finish();
				this.enabled = false;
			}
		}

		/// <summary>
		/// Stops all value movement on this spring
		/// </summary>
		public virtual void Stop() { }
		
		/// <summary>
		/// Moves this spring to its destination and disables it
		/// </summary>
		public virtual void Finish() { }
		
		/// <summary>
		/// Restores this spring's initial value 
		/// </summary>
		public virtual void RestoreInitialValue() { }
		
		/// <summary>
		/// Sets the current value of this spring as its new initial value, overriding the previous one
		/// </summary>
		public virtual void ResetInitialValue() { }
		
		/// <summary>
		/// Performs initialization for this spring
		/// </summary>
		protected virtual void Initialization() { }
		
		/// <summary>
		/// Grabs the current value on the spring's target
		/// </summary>
		protected virtual void GrabCurrentValue() { }
		
		/// <summary>
		/// Updates the spring's target value
		/// </summary>
		protected virtual void UpdateSpringValue() { }


		#region TEST_METHODS

		protected virtual void TestMoveTo() { }
		protected virtual void TestMoveToAdditive() { }
		protected virtual void TestMoveToSubtractive() { }
		protected virtual void TestMoveToRandom() { }
		protected virtual void TestMoveToInstant() { }
		protected virtual void TestBump() { }
		protected virtual void TestBumpRandom() { }

		#endregion
		
	}
}

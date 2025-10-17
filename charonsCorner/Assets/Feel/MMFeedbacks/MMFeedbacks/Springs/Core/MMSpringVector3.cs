using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringVector3 : MMSpringDefinition<Vector3>
	{
		public bool SeparateAxis = false;
		public MMSpringFloat UnifiedSpring;
		public MMSpringFloat SpringX;
		public MMSpringFloat SpringY;
		public MMSpringFloat SpringZ;

		protected Vector3 _returnCurrentValue;
		protected Vector3 _returnTargetValue;
		protected Vector3 _returnVelocity;
		
		public MMSpringVector3()
		{
			SpringX = new MMSpringFloat();
			SpringY = new MMSpringFloat();
			SpringZ = new MMSpringFloat();
			UnifiedSpring = new MMSpringFloat();
			UnifiedSpring.UnifiedSpring = true;
		}

		public virtual void SetDamping(Vector3 newDamping)
		{
			UnifiedSpring.Damping = newDamping.x;
			SpringX.Damping = newDamping.x;
			SpringY.Damping = newDamping.y;
			SpringZ.Damping = newDamping.z;
		}

		public virtual void SetFrequency(Vector3 newFrequency)
		{
			UnifiedSpring.Frequency = newFrequency.x;
			SpringX.Frequency = newFrequency.x;
			SpringY.Frequency = newFrequency.y;
			SpringZ.Frequency = newFrequency.z;
		}
		
		public override Vector3 CurrentValue
		{
			get
			{
				_returnCurrentValue.x = SpringX.CurrentValue;
				_returnCurrentValue.y = SpringY.CurrentValue;
				_returnCurrentValue.z = SpringZ.CurrentValue;
				return _returnCurrentValue;
			} 
			set
			{
				SpringX.CurrentValue = value.x;
				SpringY.CurrentValue = value.y;
				SpringZ.CurrentValue = value.z;
			}
		}

		public override Vector3 TargetValue
		{
			get
			{
				_returnTargetValue.x = SpringX.TargetValue;
				_returnTargetValue.y = SpringY.TargetValue;
				_returnTargetValue.z = SpringZ.TargetValue;
				return _returnTargetValue;
			} 
			set
			{
				SpringX.TargetValue = value.x;
				SpringY.TargetValue = value.y;
				SpringZ.TargetValue = value.z;
			}
		}

		public override Vector3 Velocity
		{
			get
			{
				_returnVelocity.x = SpringX.Velocity;
				_returnVelocity.y = SpringY.Velocity;
				_returnVelocity.z = SpringZ.Velocity;
				return _returnVelocity;
			} 
			set
			{
				SpringX.Velocity = value.x;
				SpringY.Velocity = value.y;
				SpringZ.Velocity = value.z;
			}
		}
		
		public override void UpdateSpringValue(float deltaTime)
		{
			if (!SeparateAxis)
			{
				SpringX.Damping = UnifiedSpring.Damping;
				SpringX.Frequency = UnifiedSpring.Frequency;
				SpringY.Damping = UnifiedSpring.Damping;
				SpringY.Frequency = UnifiedSpring.Frequency;
				SpringZ.Damping = UnifiedSpring.Damping;
				SpringZ.Frequency = UnifiedSpring.Frequency;
			}
			SpringX.UpdateSpringValue(deltaTime);
			SpringY.UpdateSpringValue(deltaTime);
			SpringZ.UpdateSpringValue(deltaTime);
		}
		
		public override void MoveToInstant(Vector3 newValue)
		{
			SpringX.MoveToInstant(newValue.x);
			SpringY.MoveToInstant(newValue.y);
			SpringZ.MoveToInstant(newValue.z);
		}

		public override void Stop()
		{
			SpringX.Stop();
			SpringY.Stop();
			SpringZ.Stop();
		}

		public override void SetInitialValue(Vector3 newInitialValue)
		{
			SpringX.SetInitialValue(newInitialValue.x);
			SpringY.SetInitialValue(newInitialValue.y);
			SpringZ.SetInitialValue(newInitialValue.z);
		}

		public override void RestoreInitialValue()
		{
			SpringX.RestoreInitialValue();
			SpringY.RestoreInitialValue();
			SpringZ.RestoreInitialValue();
		}

		public override void SetCurrentValueAsInitialValue()
		{
			SpringX.SetCurrentValueAsInitialValue();
			SpringY.SetCurrentValueAsInitialValue();
			SpringZ.SetCurrentValueAsInitialValue();
		}
		
		public override void MoveTo(Vector3 newValue)
		{
			SpringX.MoveTo(newValue.x);
			SpringY.MoveTo(newValue.y);
			SpringZ.MoveTo(newValue.z);
		}
		
		public override void MoveToAdditive(Vector3 newValue)
		{
			SpringX.MoveToAdditive(newValue.x);
			SpringY.MoveToAdditive(newValue.y);
			SpringZ.MoveToAdditive(newValue.z);
		}
		
		public override void MoveToSubtractive(Vector3 newValue)
		{
			SpringX.MoveToSubtractive(newValue.x);
			SpringY.MoveToSubtractive(newValue.y);
			SpringZ.MoveToSubtractive(newValue.z);
		}

		public override void MoveToRandom(Vector3 min, Vector3 max)
		{
			SpringX.MoveToRandom(min.x, max.x);
			SpringY.MoveToRandom(min.y, max.y);
			SpringZ.MoveToRandom(min.z, max.z);
		}

		public override void Bump(Vector3 bumpAmount)
		{
			SpringX.Bump(bumpAmount.x);
			SpringY.Bump(bumpAmount.y);
			SpringZ.Bump(bumpAmount.z);
		}

		public override void BumpRandom(Vector3 min, Vector3 max)
		{
			SpringX.BumpRandom(min.x, max.x);
			SpringY.BumpRandom(min.y, max.y);
			SpringZ.BumpRandom(min.z, max.z);
		}
		
		public override void Finish()
		{
			SpringX.Finish();
			SpringY.Finish();
			SpringZ.Finish();
		}
	}
}
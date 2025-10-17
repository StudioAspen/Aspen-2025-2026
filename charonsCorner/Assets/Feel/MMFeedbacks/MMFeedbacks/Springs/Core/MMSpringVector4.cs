using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringVector4 : MMSpringDefinition<Vector4>
	{
		public bool SeparateAxis = false;
		public MMSpringFloat UnifiedSpring;
		public MMSpringFloat SpringX;
		public MMSpringFloat SpringY;
		public MMSpringFloat SpringZ;
		public MMSpringFloat SpringW;

		protected Vector4 _returnCurrentValue;
		protected Vector4 _returnTargetValue;
		protected Vector4 _returnVelocity;
		
		public MMSpringVector4()
		{
			SpringX = new MMSpringFloat();
			SpringY = new MMSpringFloat();
			SpringZ = new MMSpringFloat();
			SpringW = new MMSpringFloat();
			UnifiedSpring = new MMSpringFloat();
			UnifiedSpring.UnifiedSpring = true;
		}

		public virtual void SetDamping(Vector4 newDamping)
		{
			UnifiedSpring.Damping = newDamping.x;
			SpringX.Damping = newDamping.x;
			SpringY.Damping = newDamping.y;
			SpringZ.Damping = newDamping.z;
			SpringW.Damping = newDamping.w;
		}

		public virtual void SetFrequency(Vector4 newFrequency)
		{
			UnifiedSpring.Frequency = newFrequency.x;
			SpringX.Frequency = newFrequency.x;
			SpringY.Frequency = newFrequency.y;
			SpringZ.Frequency = newFrequency.z;
			SpringW.Frequency = newFrequency.w;
		}
		
		public override Vector4 CurrentValue
		{
			get
			{
				_returnCurrentValue.x = SpringX.CurrentValue;
				_returnCurrentValue.y = SpringY.CurrentValue;
				_returnCurrentValue.z = SpringZ.CurrentValue;
				_returnCurrentValue.w = SpringW.CurrentValue;
				return _returnCurrentValue;
			} 
			set
			{
				SpringX.CurrentValue = value.x;
				SpringY.CurrentValue = value.y;
				SpringZ.CurrentValue = value.z;
				SpringW.CurrentValue = value.w;
			}
		}

		public override Vector4 TargetValue
		{
			get
			{
				_returnTargetValue.x = SpringX.TargetValue;
				_returnTargetValue.y = SpringY.TargetValue;
				_returnTargetValue.z = SpringZ.TargetValue;
				_returnTargetValue.w = SpringW.TargetValue;
				return _returnTargetValue;
			} 
			set
			{
				SpringX.TargetValue = value.x;
				SpringY.TargetValue = value.y;
				SpringZ.TargetValue = value.z;
				SpringW.TargetValue = value.w;
			}
		}

		public override Vector4 Velocity
		{
			get
			{
				_returnVelocity.x = SpringX.Velocity;
				_returnVelocity.y = SpringY.Velocity;
				_returnVelocity.z = SpringZ.Velocity;
				_returnVelocity.w = SpringW.Velocity;
				return _returnVelocity;
			} 
			set
			{
				SpringX.Velocity = value.x;
				SpringY.Velocity = value.y;
				SpringZ.Velocity = value.z;
				SpringW.Velocity = value.w;
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
				SpringW.Damping = UnifiedSpring.Damping;
				SpringW.Frequency = UnifiedSpring.Frequency;
			}
			SpringX.UpdateSpringValue(deltaTime);
			SpringY.UpdateSpringValue(deltaTime);
			SpringZ.UpdateSpringValue(deltaTime);
			SpringW.UpdateSpringValue(deltaTime);
		}
		
		public override void MoveToInstant(Vector4 newValue)
		{
			SpringX.MoveToInstant(newValue.x);
			SpringY.MoveToInstant(newValue.y);
			SpringZ.MoveToInstant(newValue.z);
			SpringW.MoveToInstant(newValue.w);
		}

		public override void Stop()
		{
			SpringX.Stop();
			SpringY.Stop();
			SpringZ.Stop();
			SpringW.Stop();
		}

		public override void SetInitialValue(Vector4 newInitialValue)
		{
			SpringX.SetInitialValue(newInitialValue.x);
			SpringY.SetInitialValue(newInitialValue.y);
			SpringZ.SetInitialValue(newInitialValue.z);
			SpringW.SetInitialValue(newInitialValue.w);
		}

		public override void RestoreInitialValue()
		{
			SpringX.RestoreInitialValue();
			SpringY.RestoreInitialValue();
			SpringZ.RestoreInitialValue();
			SpringW.RestoreInitialValue();
		}

		public override void SetCurrentValueAsInitialValue()
		{
			SpringX.SetCurrentValueAsInitialValue();
			SpringY.SetCurrentValueAsInitialValue();
			SpringZ.SetCurrentValueAsInitialValue();
			SpringW.SetCurrentValueAsInitialValue();
		}
		
		public override void MoveTo(Vector4 newValue)
		{
			SpringX.MoveTo(newValue.x);
			SpringY.MoveTo(newValue.y);
			SpringZ.MoveTo(newValue.z);
			SpringW.MoveTo(newValue.w);
		}
		
		public override void MoveToAdditive(Vector4 newValue)
		{
			SpringX.MoveToAdditive(newValue.x);
			SpringY.MoveToAdditive(newValue.y);
			SpringZ.MoveToAdditive(newValue.z);
			SpringW.MoveToAdditive(newValue.w);
		}
		
		public override void MoveToSubtractive(Vector4 newValue)
		{
			SpringX.MoveToSubtractive(newValue.x);
			SpringY.MoveToSubtractive(newValue.y);
			SpringZ.MoveToSubtractive(newValue.z);
			SpringW.MoveToSubtractive(newValue.w);
		}

		public override void MoveToRandom(Vector4 min, Vector4 max)
		{
			SpringX.MoveToRandom(min.x, max.x);
			SpringY.MoveToRandom(min.y, max.y);
			SpringZ.MoveToRandom(min.z, max.z);
			SpringW.MoveToRandom(min.w, max.w);
		}

		public override void Bump(Vector4 bumpAmount)
		{
			SpringX.Bump(bumpAmount.x);
			SpringY.Bump(bumpAmount.y);
			SpringZ.Bump(bumpAmount.z);
			SpringW.Bump(bumpAmount.w);
		}

		public override void BumpRandom(Vector4 min, Vector4 max)
		{
			SpringX.BumpRandom(min.x, max.x);
			SpringY.BumpRandom(min.y, max.y);
			SpringZ.BumpRandom(min.z, max.z);
			SpringW.BumpRandom(min.w, max.w);
		}
		
		public override void Finish()
		{
			SpringX.Finish();
			SpringY.Finish();
			SpringZ.Finish();
			SpringW.Finish();
		}
	}
}
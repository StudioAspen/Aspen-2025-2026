using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringVector2 : MMSpringDefinition<Vector2>
	{
		public bool SeparateAxis = false;
		public MMSpringFloat UnifiedSpring;
		public MMSpringFloat SpringX;
		public MMSpringFloat SpringY;

		protected Vector2 _returnCurrentValue;
		protected Vector2 _returnTargetValue;
		protected Vector2 _returnVelocity;
		
		public MMSpringVector2()
		{
			SpringX = new MMSpringFloat();
			SpringY = new MMSpringFloat();
			UnifiedSpring = new MMSpringFloat();
			UnifiedSpring.UnifiedSpring = true;
		}

		public virtual void SetDamping(Vector2 newDamping)
		{
			UnifiedSpring.Damping = newDamping.x;
			SpringX.Damping = newDamping.x;
			SpringY.Damping = newDamping.y;
		}

		public virtual void SetFrequency(Vector2 newFrequency)
		{
			UnifiedSpring.Frequency = newFrequency.x;
			SpringX.Frequency = newFrequency.x;
			SpringY.Frequency = newFrequency.y;
		}
		
		public override Vector2 CurrentValue
		{
			get
			{
				_returnCurrentValue.x = SpringX.CurrentValue;
				_returnCurrentValue.y = SpringY.CurrentValue;
				return _returnCurrentValue;
			} 
			set
			{
				SpringX.CurrentValue = value.x;
				SpringY.CurrentValue = value.y;
			}
		}

		public override Vector2 TargetValue
		{
			get
			{
				_returnTargetValue.x = SpringX.TargetValue;
				_returnTargetValue.y = SpringY.TargetValue;
				return _returnTargetValue;
			} 
			set
			{
				SpringX.TargetValue = value.x;
				SpringY.TargetValue = value.y;
			}
		}

		public override Vector2 Velocity
		{
			get
			{
				_returnVelocity.x = SpringX.Velocity;
				_returnVelocity.y = SpringY.Velocity;
				return _returnVelocity;
			} 
			set
			{
				SpringX.Velocity = value.x;
				SpringY.Velocity = value.y;
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
			}
			SpringX.UpdateSpringValue(deltaTime);
			SpringY.UpdateSpringValue(deltaTime);
		}
		
		public override void MoveToInstant(Vector2 newValue)
		{
			SpringX.MoveToInstant(newValue.x);
			SpringY.MoveToInstant(newValue.y);
		}

		public override void Stop()
		{
			SpringX.Stop();
			SpringY.Stop();
		}

		public override void SetInitialValue(Vector2 newInitialValue)
		{
			SpringX.SetInitialValue(newInitialValue.x);
			SpringY.SetInitialValue(newInitialValue.y);
		}

		public override void RestoreInitialValue()
		{
			SpringX.RestoreInitialValue();
			SpringY.RestoreInitialValue();
		}

		public override void SetCurrentValueAsInitialValue()
		{
			SpringX.SetCurrentValueAsInitialValue();
			SpringY.SetCurrentValueAsInitialValue();
		}
		
		public override void MoveTo(Vector2 newValue)
		{
			SpringX.MoveTo(newValue.x);
			SpringY.MoveTo(newValue.y);
		}
		
		public override void MoveToAdditive(Vector2 newValue)
		{
			SpringX.MoveToAdditive(newValue.x);
			SpringY.MoveToAdditive(newValue.y);
		}
		
		public override void MoveToSubtractive(Vector2 newValue)
		{
			SpringX.MoveToSubtractive(newValue.x);
			SpringY.MoveToSubtractive(newValue.y);
		}

		public override void MoveToRandom(Vector2 min, Vector2 max)
		{
			SpringX.MoveToRandom(min.x, max.x);
			SpringY.MoveToRandom(min.y, max.y);
		}

		public override void Bump(Vector2 bumpAmount)
		{
			SpringX.Bump(bumpAmount.x);
			SpringY.Bump(bumpAmount.y);
		}

		public override void BumpRandom(Vector2 min, Vector2 max)
		{
			SpringX.BumpRandom(min.x, max.x);
			SpringY.BumpRandom(min.y, max.y);
		}
		
		public override void Finish()
		{
			SpringX.Finish();
			SpringY.Finish();
		}
	}
}
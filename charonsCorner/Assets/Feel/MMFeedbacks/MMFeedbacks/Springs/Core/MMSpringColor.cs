using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringColor : MMSpringDefinition<Color>
	{
		public MMSpringFloat ColorSpring;
		
		public MMSpringFloat SpringR;
		public MMSpringFloat SpringG;
		public MMSpringFloat SpringB;
		public MMSpringFloat SpringA;

		protected Color _returnCurrentValue;
		protected Color _returnTargetValue;
		protected Color _returnVelocity;
		
		public MMSpringColor()
		{
			SpringR = new MMSpringFloat();
			SpringG = new MMSpringFloat();
			SpringB = new MMSpringFloat();
			SpringA = new MMSpringFloat();
			ColorSpring = new MMSpringFloat();
		}

		public virtual void SetDamping(float newDamping)
		{
			ColorSpring.Damping = newDamping;
			SpringR.Damping = newDamping;
			SpringG.Damping = newDamping;
			SpringB.Damping = newDamping;
			SpringA.Damping = newDamping;
		}

		public virtual void SetFrequency(float newFrequency)
		{
			ColorSpring.Frequency = newFrequency;
			SpringR.Frequency = newFrequency;
			SpringG.Frequency = newFrequency;
			SpringB.Frequency = newFrequency;
			SpringA.Frequency = newFrequency;
		}
		
		public override Color CurrentValue
		{
			get
			{
				_returnCurrentValue.r = SpringR.CurrentValue;
				_returnCurrentValue.g = SpringG.CurrentValue;
				_returnCurrentValue.b = SpringB.CurrentValue;
				_returnCurrentValue.a = SpringA.CurrentValue;
				return _returnCurrentValue;
			} 
			set
			{
				SpringR.CurrentValue = value.r;
				SpringG.CurrentValue = value.g;
				SpringB.CurrentValue = value.b;
				SpringA.CurrentValue = value.a;
			}
		}

		public override Color TargetValue
		{
			get
			{
				_returnTargetValue.r = SpringR.TargetValue;
				_returnTargetValue.g = SpringG.TargetValue;
				_returnTargetValue.b = SpringB.TargetValue;
				_returnTargetValue.a = SpringA.TargetValue;
				return _returnTargetValue;
			} 
			set
			{
				SpringR.TargetValue = value.r;
				SpringG.TargetValue = value.g;
				SpringB.TargetValue = value.b;
				SpringA.TargetValue = value.a;
			}
		}

		public override Color Velocity
		{
			get
			{
				_returnVelocity.r = SpringR.Velocity;
				_returnVelocity.g = SpringG.Velocity;
				_returnVelocity.b = SpringB.Velocity;
				_returnVelocity.a = SpringA.Velocity;
				return _returnVelocity;
			} 
			set
			{
				SpringR.Velocity = value.r;
				SpringG.Velocity = value.g;
				SpringB.Velocity = value.b;
				SpringA.Velocity = value.a;
			}
		}
		
		public override void UpdateSpringValue(float deltaTime)
		{
			SpringR.Damping = ColorSpring.Damping;
			SpringG.Damping = ColorSpring.Damping;
			SpringB.Damping = ColorSpring.Damping;
			SpringA.Damping = ColorSpring.Damping;
			SpringR.Frequency = ColorSpring.Frequency;
			SpringG.Frequency = ColorSpring.Frequency;
			SpringB.Frequency = ColorSpring.Frequency;
			SpringA.Frequency = ColorSpring.Frequency;
			
			SpringR.UpdateSpringValue(deltaTime);
			SpringG.UpdateSpringValue(deltaTime);
			SpringB.UpdateSpringValue(deltaTime);
			SpringA.UpdateSpringValue(deltaTime);
			ColorSpring.UpdateSpringValue(deltaTime);
		}
		
		public override void MoveToInstant(Color newValue)
		{
			SpringR.MoveToInstant(newValue.r);
			SpringG.MoveToInstant(newValue.g);
			SpringB.MoveToInstant(newValue.b);
			SpringA.MoveToInstant(newValue.a);
			ColorSpring.MoveToInstant(newValue.MMSum());
		}

		public override void Stop()
		{
			SpringR.Stop();
			SpringG.Stop();
			SpringB.Stop();
			SpringA.Stop();
			ColorSpring.Stop();
		}

		public override void SetInitialValue(Color newInitialValue)
		{
			SpringR.SetInitialValue(newInitialValue.r);
			SpringG.SetInitialValue(newInitialValue.g);
			SpringB.SetInitialValue(newInitialValue.b);
			SpringA.SetInitialValue(newInitialValue.a);
		}

		public override void RestoreInitialValue()
		{
			SpringR.RestoreInitialValue();
			SpringG.RestoreInitialValue();
			SpringB.RestoreInitialValue();
			SpringA.RestoreInitialValue();
		}

		public override void SetCurrentValueAsInitialValue()
		{
			SpringR.SetCurrentValueAsInitialValue();
			SpringG.SetCurrentValueAsInitialValue();
			SpringB.SetCurrentValueAsInitialValue();
			SpringA.SetCurrentValueAsInitialValue();
			ColorSpring.SetCurrentValueAsInitialValue();
		}
		
		public override void MoveTo(Color newValue)
		{
			SpringR.MoveTo(newValue.r);
			SpringG.MoveTo(newValue.g);
			SpringB.MoveTo(newValue.b);
			SpringA.MoveTo(newValue.a);
			ColorSpring.MoveTo(newValue.MMSum());
		}
		
		public override void MoveToAdditive(Color newValue)
		{
			SpringR.MoveToAdditive(newValue.r);
			SpringG.MoveToAdditive(newValue.g);
			SpringB.MoveToAdditive(newValue.b);
			SpringA.MoveToAdditive(newValue.a);
			ColorSpring.MoveToAdditive(newValue.MMSum());
		}
		
		public override void MoveToSubtractive(Color newValue)
		{
			SpringR.MoveToSubtractive(newValue.r);
			SpringG.MoveToSubtractive(newValue.g);
			SpringB.MoveToSubtractive(newValue.b);
			SpringA.MoveToSubtractive(newValue.a);
			ColorSpring.MoveToSubtractive(newValue.MMSum());
		}

		public override void MoveToRandom(Color min, Color max)
		{
			SpringR.MoveToRandom(min.r, max.r);
			SpringG.MoveToRandom(min.g, max.g);
			SpringB.MoveToRandom(min.b, max.b);
			SpringA.MoveToRandom(min.a, max.a);
			ColorSpring.MoveToRandom(min.MMSum(), max.MMSum());
		}

		public override void Bump(Color bumpAmount)
		{
			ColorSpring.Bump(bumpAmount.MMSum());
		}

		public override void BumpRandom(Color min, Color max)
		{
			ColorSpring.BumpRandom(min.MMSum(), max.MMSum());
		}
		
		public override void Finish()
		{
			SpringR.Finish();
			SpringG.Finish();
			SpringB.Finish();
			SpringA.Finish();
			ColorSpring.Finish();
		}
	}
}
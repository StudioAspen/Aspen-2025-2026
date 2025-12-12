using System;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public abstract class MMSpringDefinition<T>
	{
		public abstract T CurrentValue { get; set; }
		
		public abstract T TargetValue { get; set; }
		
		public abstract T Velocity { get; set; }
		
		public abstract void UpdateSpringValue(float deltaTime);

		public abstract void MoveToInstant(T newValue);

		public abstract void Stop();

		public abstract void SetInitialValue(T newInitialValue);

		public abstract void RestoreInitialValue();

		public abstract void SetCurrentValueAsInitialValue();

		public abstract void MoveTo(T newValue);

		public abstract void MoveToAdditive(T newValue);

		public abstract void MoveToSubtractive(T newValue);

		public abstract void MoveToRandom(T min, T max);

		public abstract void Bump(T bumpAmount);

		public abstract void BumpRandom(T min, T max);

		public abstract void Finish();
	}
}


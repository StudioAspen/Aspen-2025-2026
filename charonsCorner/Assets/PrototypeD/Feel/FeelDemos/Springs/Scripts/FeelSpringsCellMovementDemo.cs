using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Feel
{
	[AddComponentMenu("")]
	public class FeelSpringsCellMovementDemo : MonoBehaviour
	{
		[Header("Spring")] 
		public MMSpringPosition MovementSpring;
		public MMSpringRotation RotationSpring;
		public MMSpringScale ScaleSpring;

		[Header("Bindings")] 
		public FeelSpringsDemoSlider DampingSlider;
		public FeelSpringsDemoSlider FrequencySlider;
		public MMFeedbacks MoveFeedback;

		protected Vector3 _newPosition;
		protected float _cellWidth = 0.125f;
		
		protected enum Directions { Left, Right, Up, Down }
		
		protected Vector2 _currentPosition;
		protected Vector3 _movementPosition;
		
		protected virtual void Update()
		{
			UpdateSliderValues();
			HandleInput();
		}

		public virtual void MoveRandomly()
		{
			int randomDirection = Random.Range(0, 4);
			Move((Directions)randomDirection);
		}
		
		protected virtual void Move(Directions direction)
		{
			// we compute in _currentPosition the position we want to be in on the grid
			// _currentPosition is a Vector2, with x and y values ranging from -3 to 3, 
			// representing the 7x7 grid we're moving on
			// so at the start, _currentPosition is (0,0), meaning we're in the middle of the grid
			// and if we were to move left, the new _currentPosition would be (-1,0), meaning we'd be one cell to the left
			ComputeNewGridPosition(direction);
			
			// then we simply move the spring to the new position, by passing it the world position we want to move to
			// to get that world position, we multiply the current position by the cell width
			_movementPosition.x = _currentPosition.x * _cellWidth;
			_movementPosition.y = _currentPosition.y * _cellWidth;
			_movementPosition.z = MovementSpring.Target.transform.localPosition.z;
			// this is the only line required to have an object move using a spring in Feel :
			MovementSpring.MoveTo(_movementPosition);
			
			ScaleSpring.Bump(5f * Vector3.one);
			MoveFeedback?.PlayFeedbacks();
		}

		protected virtual void Bump(Directions direction)
		{
			switch (direction)
			{
				case Directions.Left:
					RotationSpring.Bump(new Vector3(0f,0f,-900f));
					MovementSpring.Bump(new Vector3(4f,0f,0f));
					break;
				case Directions.Right:
					RotationSpring.Bump(new Vector3(0f,0f,900f));
					MovementSpring.Bump(new Vector3(-4f,0f,0f));
					break;
				case Directions.Up:
					RotationSpring.Bump(new Vector3(0f,0f,450f));
					MovementSpring.Bump(new Vector3(0f,-4f,0f));
					break;
				case Directions.Down:
					RotationSpring.Bump(new Vector3(0f,0f,-450f));
					MovementSpring.Bump(new Vector3(0f,4f,0f));
					break;
			}
		}

		protected virtual void ComputeNewGridPosition(Directions direction)
		{
			switch (direction)
			{
				case Directions.Left:
					_currentPosition.x -= 1;
					break;
				case Directions.Right:
					_currentPosition.x += 1;
					break;
				case Directions.Up:
					_currentPosition.y += 1;
					break;
				case Directions.Down:
					_currentPosition.y -= 1;
					break;
			}
			if (_currentPosition.x < -3)
			{
				_currentPosition.x = -3;
				Bump(Directions.Left);
				return;
			}
			if (_currentPosition.x > 3)
			{
				_currentPosition.x = 3;
				Bump(Directions.Right);
				return;
			}
			if (_currentPosition.y < -3)
			{
				_currentPosition.y = -3;
				Bump(Directions.Down);
				return;
			}
			if (_currentPosition.y > 3)
			{
				_currentPosition.y = 3;
				Bump(Directions.Up);
				return;
			}
		}

		protected virtual void HandleInput()
		{
			bool input;

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.leftArrowKey.wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.LeftArrow);
			#endif
			if (input) { Move(Directions.Left); }

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.rightArrowKey.wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.RightArrow);
			#endif
			if (input) { Move(Directions.Right); }


			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.downArrowKey.wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.DownArrow);
			#endif
			if (input) { Move(Directions.Down); }

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current.upArrowKey.wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.UpArrow);
			#endif
			if (input) { Move(Directions.Up); }
		}

		protected virtual void UpdateSliderValues()
		{
			// bind the slider values to the springs
			MovementSpring.SpringVector3.SetDamping(DampingSlider.value * Vector3.one);
			MovementSpring.SpringVector3.SetFrequency(FrequencySlider.value * Vector3.one);
		}
	}
}

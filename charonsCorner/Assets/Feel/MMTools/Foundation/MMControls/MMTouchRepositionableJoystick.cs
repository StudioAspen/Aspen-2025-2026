using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if MM_UI
using UnityEngine.EventSystems;
namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to a UI rectangle and it'll act as a detection zone for a joystick.
	/// Note that this component extends the MMTouchJoystick class so you don't need to add another joystick to it. It's both the detection zone and the stick itself.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Controls/MM Touch Repositionable Joystick")]
	public class MMTouchRepositionableJoystick : MMTouchJoystick, IPointerDownHandler
	{
		[MMInspectorGroup("Repositionable Joystick", true, 22)]
		/// the canvas group to use as the joystick's knob
		[Tooltip("the canvas group to use as the joystick's knob")]
		public CanvasGroup KnobCanvasGroup;
		/// the canvas group to use as the joystick's background
		[Tooltip("the canvas group to use as the joystick's background")]
		public CanvasGroup BackgroundCanvasGroup;
		/// if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas
		[Tooltip("if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas")]
		public bool ConstrainToInitialRectangle = true;
		/// if this is true, the joystick will return back to its initial position when released
		[Tooltip("if this is true, the joystick will return back to its initial position when released")]
		public bool ResetPositionToInitialOnRelease = false;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;
		protected RectTransform _rectTransform;

		/// <summary>
		/// On Start, we instantiate our joystick's image if there's one
		/// </summary>
		protected override void Start()
		{
			base.Start();

			_rectTransform = GetComponent<RectTransform>();
			_initialPosition = BackgroundCanvasGroup.GetComponent<RectTransform>().position;
		}

		/// <summary>
		/// On init we set our knob transform
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			SetKnobTransform(KnobCanvasGroup.transform);
			_canvasGroup = KnobCanvasGroup;
			_initialOpacity = _canvasGroup.alpha;
		}

		/// <summary>
		/// When the zone is pressed, we move our joystick accordingly
		/// </summary>
		/// <param name="data">Data.</param>
		public override void OnPointerDown(PointerEventData data)
		{
			_targetOpacity = PressedOpacity;
			OnPointerDownEvent.Invoke();
			
			_newPosition = ConvertToWorld(data.position);
			
			if (!WithinBounds())
			{
				return;
			}

			BackgroundCanvasGroup.transform.position = _newPosition;
			SetNeutralPosition(_newPosition);
			_knobTransform.position = _newPosition;
			
			_initialZPosition = _newPosition.z;
		}

		/// <summary>
		/// Override OnDrag to handle repositionable joystick with rotated camera
		/// </summary>
		public override void OnDrag(PointerEventData eventData)
		{
			OnDragEvent.Invoke();

			_newTargetPosition = ConvertToWorld(eventData.position);

			Vector3 localDelta = TransformToLocalSpace(_newTargetPosition - _neutralPosition);

			localDelta = Vector2.ClampMagnitude(localDelta, ComputedMaxRange);

			if (!HorizontalAxisEnabled)
			{
				localDelta.x = 0;
			}
			if (!VerticalAxisEnabled)
			{
				localDelta.y = 0;
			}

			RawValue.x = EvaluateInputValue(localDelta.x);
			RawValue.y = EvaluateInputValue(localDelta.y);

			_newTargetPosition = _neutralPosition + TransformToWorldSpace(localDelta);
			_newJoystickPosition = _newTargetPosition;
			_newJoystickPosition.z = _initialZPosition;

			_knobTransform.position = _newJoystickPosition;
		}

		/// <summary>
		/// Returns true if the joystick's new position is within the bounds of the top level canvas
		/// </summary>
		/// <returns></returns>
		protected virtual bool WithinBounds()
		{
			if (!ConstrainToInitialRectangle)
			{
				return true;
			}

			Vector2 screenPoint = _newPosition;
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera && TargetCamera != null)
			{
				screenPoint = TargetCamera.WorldToScreenPoint(_newPosition);
			}
			
			return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPoint, TargetCamera);
		}

		/// <summary>
		/// When the player lets go of the stick, we restore our stick's position if needed
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			if (ResetPositionToInitialOnRelease)
			{
				BackgroundCanvasGroup.transform.position = _initialPosition;
				_knobTransform.position = _initialPosition;
			}
		}
		
		
		#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos if needed
		/// </summary>
		protected override void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}

			Handles.color = MMColors.Orange;
			if (!Application.isPlaying)
			{
				if (KnobCanvasGroup != null)
				{
					Handles.DrawWireDisc(KnobCanvasGroup.transform.position, this.transform.forward, ComputedMaxRange);	
				}
				else
				{
					Handles.DrawWireDisc(this.transform.position, this.transform.forward, ComputedMaxRange);	
				}
			}
			else
			{
				Handles.DrawWireDisc(_neutralPosition, this.transform.forward, ComputedMaxRange);
			}
		}
		#endif
	}
}
#endif
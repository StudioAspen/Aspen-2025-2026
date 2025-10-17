using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This class will allow you to trigger zooms on your cinemachine camera by sending MMCameraZoomEvents from any other class
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Zoom")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineZoom : MonoBehaviour
	{
		[Header("Channel")]
		[MMFInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// if this is true, triggering a new zoom event will interrupt any transition that may be in progress
		[Tooltip("if this is true, triggering a new zoom event will interrupt any transition that may be in progress")]
		public bool Interruptable = false;

		[Header("Transition Speed")]
		/// the animation curve to apply to the zoom transition
		[Tooltip("the animation curve to apply to the zoom transition")]
		public MMTweenType ZoomTween = new MMTweenType( new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));

		[Header("Test Zoom")]
		/// the mode to apply the zoom in when using the test button in the inspector
		[Tooltip("the mode to apply the zoom in when using the test button in the inspector")]
		public MMCameraZoomModes TestMode;
		/// the target field of view to apply the zoom in when using the test button in the inspector
		[Tooltip("the target field of view to apply the zoom in when using the test button in the inspector")]
		public float TestFieldOfView = 30f;
		/// the transition duration to apply the zoom in when using the test button in the inspector
		[Tooltip("the transition duration to apply the zoom in when using the test button in the inspector")]
		public float TestTransitionDuration = 0.1f;
		/// the duration to apply the zoom in when using the test button in the inspector
		[Tooltip("the duration to apply the zoom in when using the test button in the inspector")]
		public float TestDuration = 0.05f;

		[MMFInspectorButton("TestZoom")]
		/// an inspector button to test the zoom in play mode
		public bool TestZoomButton;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		public virtual TimescaleModes TimescaleMode { get; set; }
        
		#if MM_CINEMACHINE
		protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _initialFieldOfView;
		protected MMCameraZoomModes _mode;
		protected float _startFieldOfView;
		protected float _transitionDuration;
		protected float _duration;
		protected float _targetFieldOfView;
		protected float _elapsedTime = 0f;
		protected int _direction = 1;
		protected float _reachedDestinationTimestamp;
		protected bool _destinationReached = false;
		protected float _zoomStartedAt = 0f;

		/// <summary>
		/// On Awake we grab our virtual camera
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = this.gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
			_initialFieldOfView = _virtualCamera.m_Lens.FieldOfView;
			#elif MM_CINEMACHINE3
			_virtualCamera = this.gameObject.GetComponent<CinemachineCamera>();
			_initialFieldOfView = _virtualCamera.Lens.FieldOfView;
			#endif
			MMCameraZoomEvent.Register(OnCameraZoomEvent);
			this.enabled = false;
		}	
        
		/// <summary>
		/// On Update if we're zooming we modify our field of view accordingly
		/// </summary>
		protected virtual void Update()
		{
			_elapsedTime = GetTime() - _zoomStartedAt;
			if (_elapsedTime <= _transitionDuration)
			{
				float t = MMMaths.Remap(_elapsedTime, 0f, _transitionDuration, 0f, 1f);
				#if MM_CINEMACHINE
				_virtualCamera.m_Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
				#elif MM_CINEMACHINE3
				_virtualCamera.Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
				#endif
			}
			else
			{
				if (!_destinationReached)
				{
					_reachedDestinationTimestamp = GetTime();
					_destinationReached = true;
				}
				if ((_mode == MMCameraZoomModes.For) && (_direction == 1))
				{
					if (GetTime() - _reachedDestinationTimestamp > _duration)
					{
						_direction = -1;
						_zoomStartedAt = GetTime();
						_startFieldOfView = _targetFieldOfView;
						_targetFieldOfView = _initialFieldOfView;
					}                    
				}
				else
				{
					this.enabled = false;
				}   
			}
		}

		/// <summary>
		/// A method that triggers the zoom, ideally only to be called via an event, but public for convenience
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="newFieldOfView"></param>
		/// <param name="transitionDuration"></param>
		/// <param name="duration"></param>
		public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, bool useUnscaledTime, bool relative = false, MMTweenType tweenType = null)
		{
			if (this.enabled && !Interruptable)
			{
				return;
			}

			this.enabled = true;
			_elapsedTime = 0f;
			_mode = mode;

			TimescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			#if MM_CINEMACHINE
			_startFieldOfView = _virtualCamera.m_Lens.FieldOfView;
			#elif MM_CINEMACHINE3
			_startFieldOfView = _virtualCamera.Lens.FieldOfView;
			#endif
			_transitionDuration = transitionDuration;
			_duration = duration;
			_transitionDuration = transitionDuration;
			_direction = 1;
			_destinationReached = false;
			_zoomStartedAt = GetTime();
			
			if (tweenType != null)
			{
				ZoomTween = tweenType;
			}

			switch (mode)
			{
				case MMCameraZoomModes.For:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Set:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Reset:
					_targetFieldOfView = _initialFieldOfView;
					break;
			}

			if (relative)
			{
				_targetFieldOfView += _initialFieldOfView;
			}
		}

		/// <summary>
		/// The method used by the test button to trigger a test zoom
		/// </summary>
		protected virtual void TestZoom()
		{
			Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration, false);
		}

		/// <summary>
		/// When we get an MMCameraZoomEvent we call our zoom method 
		/// </summary>
		/// <param name="zoomEvent"></param>
		public virtual void OnCameraZoomEvent(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, 
			bool useUnscaledTime, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			if (stop)
			{
				this.enabled = false;
				return;
			}
			if (restore)
			{
				#if MM_CINEMACHINE
				_virtualCamera.m_Lens.FieldOfView = _initialFieldOfView;
				#elif MM_CINEMACHINE3
				_virtualCamera.Lens.FieldOfView = _initialFieldOfView;
				#endif
				return;
			}
			this.Zoom(mode, newFieldOfView, transitionDuration, duration, useUnscaledTime, relative, tweenType);
		}

		/// <summary>
		/// Stops listening for MMCameraZoomEvents
		/// </summary>
		protected virtual void OnDestroy()
		{
			MMCameraZoomEvent.Unregister(OnCameraZoomEvent);
		}
	}
}
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Turns an object active or inactive at the various stages of the feedback
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to set global properties on your shader, or enable/disable keywords. Supports optional interpolation over time for Color, Float, Int, and Vector properties.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Renderer/Shader Global")]
	public class MMF_ShaderGlobal : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		public override bool HasCustomInspectors => true;
		#endif

		public enum Modes { SetGlobalColor, SetGlobalFloat, SetGlobalInt, SetGlobalMatrix, SetGlobalTexture, SetGlobalVector, EnableKeyword, DisableKeyword, WarmupAllShaders }

		[MMFInspectorGroup("Shader Global", true, 24)]
		/// the selected mode for this feedback
		[Tooltip("the selected mode for this feedback")]
		public Modes Mode = Modes.SetGlobalFloat;
		/// the name of the global property
		[Tooltip("the name of the global property")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public string PropertyName = "";
		/// the name ID of the property retrieved by Shader.PropertyToID
		[Tooltip("the name ID of the property retrieved by Shader.PropertyToID")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public int PropertyNameID = 0;
		/// a global color property for all shaders
		[Tooltip("a global color property for all shaders")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor)]
		public Color GlobalColor = Color.yellow;
		/// a global float property for all shaders
		[Tooltip("a global float property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalFloat)]
		public float GlobalFloat = 1f;
		/// a global int property for all shaders
		[Tooltip("a global int property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalInt)]
		public int GlobalInt = 1;
		/// a global matrix property for all shaders
		[Tooltip("a global matrix property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalMatrix)]
		public Matrix4x4 GlobalMatrix = Matrix4x4.identity;
		/// a global texture property for all shaders
		[Tooltip("a global texture property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalTexture)]
		public RenderTexture GlobalTexture;
		/// a global vector property for all shaders
		[Tooltip("a global vector property for all shaders")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalVector)]
		public Vector4 GlobalVector;
		/// a global shader keyword
		[Tooltip("a global shader keyword")] 
		[MMFEnumCondition("Mode", (int)Modes.EnableKeyword, (int)Modes.DisableKeyword)]
		public string Keyword;

		[Header("Interpolation")]
		/// whether or not to interpolate the value over time. If set to false, the change will be instant. Only applies to Color, Float, Int, and Vector modes.
		[Tooltip("whether or not to interpolate the value over time. If set to false, the change will be instant. Only applies to Color, Float, Int, and Vector modes.")]
		public bool InterpolateValue = false;
		/// the duration of the interpolation
		[Tooltip("the duration of the interpolation")]
		[MMFCondition("InterpolateValue", true)]
		public float Duration = 2f;
		/// the curve over which to interpolate the value
		[Tooltip("the curve over which to interpolate the value")]
		public MMTweenType InterpolationCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)), "InterpolateValue");
		
		public override float FeedbackDuration { get { return (InterpolateValue && CanInterpolate()) ? ApplyTimeMultiplier(Duration) : 0f; } set { if (InterpolateValue) { Duration = value; } } }

		protected Color _initialColor;
		protected float _initialFloat;
		protected int _initialInt;
		protected Matrix4x4 _initialMatrix;
		protected RenderTexture _initialTexture;
		protected Vector4 _initialVector;
		protected Coroutine _coroutine;
		protected Color _newColor;
		protected Vector4 _newVector;

		/// <summary>
		/// Returns true if the current Mode supports interpolation
		/// </summary>
		protected virtual bool CanInterpolate()
		{
			return Mode == Modes.SetGlobalColor || Mode == Modes.SetGlobalFloat || 
			       Mode == Modes.SetGlobalInt || Mode == Modes.SetGlobalVector;
		}

		/// <summary>
		/// On init we capture the initial values of the global shader properties for interpolation and restore
		/// </summary>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			GetInitialValues();
		}

		/// <summary>
		/// Captures the current global shader property values for later use in interpolation and restore
		/// </summary>
		protected virtual void GetInitialValues()
		{
			switch (Mode)
			{
				case Modes.SetGlobalColor:
					_initialColor = (PropertyName == "") ? Shader.GetGlobalColor(PropertyNameID) : Shader.GetGlobalColor(PropertyName);
					break;
				case Modes.SetGlobalFloat:
					_initialFloat = (PropertyName == "") ? Shader.GetGlobalFloat(PropertyNameID) : Shader.GetGlobalFloat(PropertyName);
					break;
				case Modes.SetGlobalInt:
					_initialInt = (PropertyName == "") ? Shader.GetGlobalInt(PropertyNameID) : Shader.GetGlobalInt(PropertyName);
					break;
				case Modes.SetGlobalMatrix:
					_initialMatrix = (PropertyName == "") ? Shader.GetGlobalMatrix(PropertyNameID) : Shader.GetGlobalMatrix(PropertyName);
					break;
				case Modes.SetGlobalTexture:
					_initialTexture = (PropertyName == "") ? (RenderTexture)Shader.GetGlobalTexture(PropertyNameID) : (RenderTexture)Shader.GetGlobalTexture(PropertyName);
					break;
				case Modes.SetGlobalVector:
					_initialVector = (PropertyName == "") ? Shader.GetGlobalVector(PropertyNameID) : Shader.GetGlobalVector(PropertyName);
					break;
			}
		}
        
		/// <summary>
		/// On Play we set our global shader property, either instantly or interpolated over time
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (InterpolateValue && CanInterpolate())
			{
				_coroutine = Owner.StartCoroutine(InterpolationSequenceCo(feedbacksIntensity));
			}
			else
			{
				ApplyGlobalValue();
			}
		}

		/// <summary>
		/// Applies the global shader value instantly (no interpolation)
		/// </summary>
		protected virtual void ApplyGlobalValue()
		{
			switch (Mode)
			{
				case Modes.SetGlobalColor:
					SetGlobalColor(GlobalColor);
					break;
				case Modes.SetGlobalFloat:
					SetGlobalFloat(GlobalFloat);
					break;
				case Modes.SetGlobalInt:
					SetGlobalInt(GlobalInt);
					break;
				case Modes.SetGlobalMatrix:
					SetGlobalMatrix(GlobalMatrix);
					break;
				case Modes.SetGlobalTexture:
					SetGlobalTexture(GlobalTexture);
					break;
				case Modes.SetGlobalVector:
					SetGlobalVector(GlobalVector);
					break;
				case Modes.EnableKeyword:
					Shader.EnableKeyword(Keyword);
					break;
				case Modes.DisableKeyword:
					Shader.DisableKeyword(Keyword);
					break;
				case Modes.WarmupAllShaders:
					Shader.WarmupAllShaders();
					break;
			}
		}

		/// <summary>
		/// An internal coroutine used to interpolate the value over time
		/// </summary>
		protected virtual IEnumerator InterpolationSequenceCo(float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);
				SetValueAtTime(remappedTime, intensityMultiplier);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetValueAtTime(FinalNormalizedTime, intensityMultiplier);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the value of the global shader property at a given normalized time during interpolation
		/// </summary>
		protected virtual void SetValueAtTime(float t, float intensityMultiplier)
		{
			switch (Mode)
			{
				case Modes.SetGlobalColor:
					float colorEval = MMTween.Tween(t, 0f, 1f, 0f, 1f, InterpolationCurve);
					_newColor = Color.Lerp(_initialColor, GlobalColor, colorEval);
					SetGlobalColor(_newColor);
					break;
				case Modes.SetGlobalFloat:
					float newFloat = MMTween.Tween(t, 0f, 1f, _initialFloat, GlobalFloat, InterpolationCurve);
					SetGlobalFloat(newFloat);
					break;
				case Modes.SetGlobalInt:
					int newInt = (int)MMTween.Tween(t, 0f, 1f, _initialInt, GlobalInt, InterpolationCurve);
					SetGlobalInt(newInt);
					break;
				case Modes.SetGlobalVector:
					_newVector = MMTween.Tween(t, 0f, 1f, _initialVector, GlobalVector, InterpolationCurve);
					SetGlobalVector(_newVector);
					break;
			}
		}

		/// <summary>
		/// Stops this feedback
		/// </summary>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.SetGlobalColor:
					SetGlobalColor(_initialColor);
					break;
				case Modes.SetGlobalFloat:
					SetGlobalFloat(_initialFloat);
					break;
				case Modes.SetGlobalInt:
					SetGlobalInt(_initialInt);
					break;
				case Modes.SetGlobalMatrix:
					SetGlobalMatrix(_initialMatrix);
					break;
				case Modes.SetGlobalTexture:
					SetGlobalTexture(_initialTexture);
					break;
				case Modes.SetGlobalVector:
					SetGlobalVector(_initialVector);
					break;
			}
		}

		#region GlobalShaderHelpers

		/// <summary>
		/// Sets a global color on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalColor(Color color)
		{
			if (PropertyName == "") { Shader.SetGlobalColor(PropertyNameID, color); }
			else { Shader.SetGlobalColor(PropertyName, color); }
		}

		/// <summary>
		/// Sets a global float on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalFloat(float value)
		{
			if (PropertyName == "") { Shader.SetGlobalFloat(PropertyNameID, value); }
			else { Shader.SetGlobalFloat(PropertyName, value); }
		}

		/// <summary>
		/// Sets a global int on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalInt(int value)
		{
			if (PropertyName == "") { Shader.SetGlobalInt(PropertyNameID, value); }
			else { Shader.SetGlobalInt(PropertyName, value); }
		}

		/// <summary>
		/// Sets a global matrix on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalMatrix(Matrix4x4 matrix)
		{
			if (PropertyName == "") { Shader.SetGlobalMatrix(PropertyNameID, matrix); }
			else { Shader.SetGlobalMatrix(PropertyName, matrix); }
		}

		/// <summary>
		/// Sets a global texture on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalTexture(Texture texture)
		{
			if (PropertyName == "") { Shader.SetGlobalTexture(PropertyNameID, texture); }
			else { Shader.SetGlobalTexture(PropertyName, texture); }
		}

		/// <summary>
		/// Sets a global vector on the shader, using either PropertyName or PropertyNameID
		/// </summary>
		protected virtual void SetGlobalVector(Vector4 vector)
		{
			if (PropertyName == "") { Shader.SetGlobalVector(PropertyNameID, vector); }
			else { Shader.SetGlobalVector(PropertyName, vector); }
		}

		#endregion
	}
}


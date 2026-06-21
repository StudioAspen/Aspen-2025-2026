using System;
using UnityEngine;
#if MM_VISUALEFFECTGRAPH
using UnityEngine.VFX;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you set a property on a target VisualEffect
	/// </summary>
	[AddComponentMenu("")]
	[System.Serializable]
	[FeedbackHelp("This feedback will let you set a property on a target VisualEffect")]
	#if MM_VISUALEFFECTGRAPH
	[FeedbackPath("Particles/VisualEffectSetProperty")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.VisualEffectGraph")]
	public class MMF_VisualEffectSetProperty : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		
		[MMFInspectorGroup("Visual Effect Property", true, 41)]
		/// the duration for the player to consider. This won't impact your visual effect, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your visual effect, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;
		
		#if MM_VISUALEFFECTGRAPH
		
		public enum PropertyTypes { AnimationCurve, Bool, Float, Gradient, Int, Mesh, Texture, UInt, Vector2, Vector3, Vector4, }
		
		/// the visual effect on which to set a property
		[Tooltip("the visual effect on which to set a property")]
		public VisualEffect TargetVisualEffect;
		/// the ID of the property to set, as exposed by the Visual Effect Graph
		[Tooltip("the ID of the property to set, as exposed by the Visual Effect Graph")] 
		public string PropertyID;
		/// the type of the property to set
		[Tooltip("the type of the property to set")]
		public PropertyTypes PropertyType = PropertyTypes.Float;
		/// if the property is an animation curve, the new animation curve to set
		[Tooltip("if the property is an animation curve, the new animation curve to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.AnimationCurve)]
		public AnimationCurve NewAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// if the property is a bool, the new bool to set
		[Tooltip("if the property is a bool, the new bool to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Bool)]
		public bool NewBool = true;
		/// if the property is a float, the new float to set
		[Tooltip("if the property is a float, the new float to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Float)]
		public float NewFloat = 1f;
		/// if the property is a gradient, the new gradient to set
		[Tooltip("if the property is a gradient, the new gradient to set")] 
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Gradient)]
		[GradientUsage(true)]
		public Gradient NewGradient = new Gradient();
		/// if the property is an int, the new int to set
		[Tooltip("if the property is an int, the new int to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Int)]
		public int NewInt;
		/// if the property is a mesh, the new mesh to set
		[Tooltip("if the property is a mesh, the new mesh to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Mesh)]
		public Mesh NewMesh;
		/// if the property is a texture, the new texture to set
		[Tooltip("if the property is a texture, the new texture to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Texture)]
		public Texture NewTexture;
		/// if the property is an unsigned int, the new unsigned int to set
		[Tooltip("if the property is an unsigned int, the new unsigned int to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.UInt)]
		public uint NewUInt;
		/// if the property is a vector2, the new vector2 to set
		[Tooltip("if the property is a vector2, the new vector2 to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Vector2)]
		public Vector2 NewVector2;
		/// if the property is a vector3, the new vector3 to set
		[Tooltip("if the property is a vector3, the new vector3 to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Vector3)]
		public Vector3 NewVector3;
		/// if the property is a vector4, the new vector4 to set
		[Tooltip("if the property is a vector4, the new vector4 to set")]
		[MMFEnumCondition("PropertyType", (int)PropertyTypes.Vector4)]
		public Vector4 NewVector4;

		protected int _propertyID;

		protected AnimationCurve _initialAnimationCurve;
		protected bool _initialBool;
		protected float _initialFloat;
		protected Gradient _initialGradient;
		protected int _initialInt;
		protected Mesh _initialMesh;
		protected Texture _initialTexture;
		protected uint _initialUInt;
		protected Vector2 _initialVector2;
		protected Vector3 _initialVector3;
		protected Vector4 _initialVector4;
		
		/// <summary>
		/// On init we cache our property ID
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_propertyID = Shader.PropertyToID(PropertyID);
			GetInitialValue();
		}

		/// <summary>
		/// Grabs and stores the initial value of the target property
		/// </summary>
		protected virtual void GetInitialValue()
		{
			if (TargetVisualEffect == null)
			{
				return;
			}
			
			switch (PropertyType)
			{
				case PropertyTypes.AnimationCurve:
					_initialAnimationCurve = TargetVisualEffect.GetAnimationCurve(_propertyID);
					break;
				case PropertyTypes.Bool:
					_initialBool = TargetVisualEffect.GetBool(_propertyID);
					break;
				case PropertyTypes.Float:
					_initialFloat = TargetVisualEffect.GetFloat(_propertyID);
					break;
				case PropertyTypes.Gradient:
					_initialGradient = TargetVisualEffect.GetGradient(_propertyID);
					break;
				case PropertyTypes.Int:
					_initialInt = TargetVisualEffect.GetInt(_propertyID);
					break;
				case PropertyTypes.Mesh:
					_initialMesh = TargetVisualEffect.GetMesh(_propertyID);
					break;
				case PropertyTypes.Texture:
					_initialTexture = TargetVisualEffect.GetTexture(_propertyID);
					break;
				case PropertyTypes.UInt:
					_initialUInt = TargetVisualEffect.GetUInt(_propertyID);
					break;
				case PropertyTypes.Vector2:
					_initialVector2 = TargetVisualEffect.GetVector2(_propertyID);
					break;
				case PropertyTypes.Vector3:
					_initialVector3 = TargetVisualEffect.GetVector3(_propertyID);
					break;
				case PropertyTypes.Vector4:
					_initialVector4 = TargetVisualEffect.GetVector4(_propertyID);
					break;
			}
		}

		/// <summary>
		/// Sets the target property on the target VisualEffect to the specified new value
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetVisualEffect == null))
			{
				return;
			}

			switch (PropertyType)
			{
				case PropertyTypes.AnimationCurve:
					TargetVisualEffect.SetAnimationCurve(_propertyID, NewAnimationCurve);
					break;
				case PropertyTypes.Bool:
					TargetVisualEffect.SetBool(_propertyID, NewBool);
					break;
				case PropertyTypes.Float:
					TargetVisualEffect.SetFloat(_propertyID, NewFloat);
					break;
				case PropertyTypes.Gradient:
					TargetVisualEffect.SetGradient(_propertyID, NewGradient);
					break;
				case PropertyTypes.Int:
					TargetVisualEffect.SetInt(_propertyID, NewInt);
					break;
				case PropertyTypes.Mesh:
					TargetVisualEffect.SetMesh(_propertyID, NewMesh);
					break;
				case PropertyTypes.Texture:
					TargetVisualEffect.SetTexture(_propertyID, NewTexture);
					break;
				case PropertyTypes.UInt:
					TargetVisualEffect.SetUInt(_propertyID, NewUInt);
					break;
				case PropertyTypes.Vector2:
					TargetVisualEffect.SetVector2(_propertyID, NewVector2);
					break;
				case PropertyTypes.Vector3:
					TargetVisualEffect.SetVector3(_propertyID, NewVector3);
					break;
				case PropertyTypes.Vector4:
					TargetVisualEffect.SetVector4(_propertyID, NewVector4);
					break;
			}
		}
		
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			switch (PropertyType)
			{
				case PropertyTypes.AnimationCurve:
					TargetVisualEffect.SetAnimationCurve(_propertyID, _initialAnimationCurve);
					break;
				case PropertyTypes.Bool:
					TargetVisualEffect.SetBool(_propertyID, _initialBool);
					break;
				case PropertyTypes.Float:
					TargetVisualEffect.SetFloat(_propertyID, _initialFloat);
					break;
				case PropertyTypes.Gradient:
					TargetVisualEffect.SetGradient(_propertyID, _initialGradient);
					break;
				case PropertyTypes.Int:
					TargetVisualEffect.SetInt(_propertyID, _initialInt);
					break;
				case PropertyTypes.Mesh:
					TargetVisualEffect.SetMesh(_propertyID, _initialMesh);
					break;
				case PropertyTypes.Texture:
					TargetVisualEffect.SetTexture(_propertyID, _initialTexture);
					break;
				case PropertyTypes.UInt:
					TargetVisualEffect.SetUInt(_propertyID, _initialUInt);
					break;
				case PropertyTypes.Vector2:
					TargetVisualEffect.SetVector2(_propertyID, _initialVector2);
					break;
				case PropertyTypes.Vector3:
					TargetVisualEffect.SetVector3(_propertyID, _initialVector3);
					break;
				case PropertyTypes.Vector4:
					TargetVisualEffect.SetVector4(_propertyID, _initialVector4);
					break;
			}
		}
		
		#else
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f) { }
		#endif
	}
}
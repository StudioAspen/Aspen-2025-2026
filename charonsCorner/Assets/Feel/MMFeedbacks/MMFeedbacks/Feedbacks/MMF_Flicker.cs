using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will make the bound renderer flicker for the set duration when played (and restore its initial color when stopped)
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you flicker the color of a specified renderer (sprite, mesh, etc) for a certain duration, at the specified octave, and with the specified color. Useful when a character gets hit, for example (but so much more!).")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Renderer/Flicker")]
	public class MMF_Flicker : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundRenderer == null);
		public override string RequiredTargetText => BoundRenderer != null ? BoundRenderer.name : "";
		public override string RequiresSetupText => "This feedback requires that a BoundRenderer be set to be able to work properly. You can set one below.";
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundRenderer = FindAutomatedTarget<Renderer>();

		/// the possible modes
		/// Color : will control material.color
		/// PropertyName : will target a specific shader property by name
		public enum Modes { Color, PropertyName }

		[MMFInspectorGroup("Flicker", true, 61, true)]
		/// the renderer to flicker when played
		[Tooltip("the renderer to flicker when played")]
		public Renderer BoundRenderer;
		/// more renderers to flicker when played
		[Tooltip("more renderers to flicker when played")]
		public List<Renderer> ExtraBoundRenderers;
		/// the selected mode to flicker the renderer 
		[Tooltip("the selected mode to flicker the renderer")]
		public Modes Mode = Modes.Color;
		/// the name of the property to target
		[MMFEnumCondition("Mode", (int)Modes.PropertyName)]
		[Tooltip("the name of the property to target")]
		public string PropertyName = "_Tint";
		/// the duration of the flicker when getting damage
		[Tooltip("the duration of the flicker when getting damage")]
		public float FlickerDuration = 0.2f;
		/// the duration of the period for the flicker
		[Tooltip("the duration of the period for the flicker")]
		[FormerlySerializedAs("FlickerOctave")]
		public float FlickerPeriod = 0.04f;
		/// the color we should flicker the sprite to 
		[Tooltip("the color we should flicker the sprite to")]
		[ColorUsage(true, true)]
		public Color FlickerColor = new Color32(255, 20, 20, 255);
		/// the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0 
		[Tooltip("the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0")]
		public int[] MaterialIndexes;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
		public bool UseMaterialPropertyBlocks = false;
		/// if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.
		[Tooltip("if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.")]
		[MMCondition("UseMaterialPropertyBlocks", true)]
		public string SpriteRendererTextureProperty = "_MainTex";

		/// the duration of this feedback is the duration of the flicker
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

		protected const string _colorPropertyName = "_Color";
        
		protected int[] _propertyIDs;
		protected bool[] _propertiesFound;
		protected bool _spriteRendererIsNull;
		
		protected Coroutine[] _coroutines;
		protected List<Coroutine[]> _extraCoroutines;
		
		protected Color[] _initialFlickerColors;
		protected List<Color[]> _extraInitialFlickerColors;
		
		protected MaterialPropertyBlock _propertyBlock;
		protected List<MaterialPropertyBlock> _extraPropertyBlocks;
		
		protected SpriteRenderer _spriteRenderer;
		protected List<SpriteRenderer> _spriteRenderers;
		
		protected Texture2D _spriteRendererTexture;
		protected List<Texture2D> _spriteRendererTextures;

		/// <summary>
		/// On init we grab our initial color and components
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			if (MaterialIndexes == null)
			{
				MaterialIndexes = Array.Empty<int>();
			}
			if (ExtraBoundRenderers == null)
			{
				ExtraBoundRenderers = new List<Renderer>();
			}
			
			// init material indexes
			if (MaterialIndexes.Length == 0)
			{
				MaterialIndexes = new int[1];
				MaterialIndexes[0] = 0;
			}

			_coroutines = new Coroutine[MaterialIndexes.Length];
			_initialFlickerColors = new Color[MaterialIndexes.Length];
			
			_extraCoroutines = new List<Coroutine[]>();
			_extraInitialFlickerColors = new List<Color[]>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				_extraCoroutines.Add(new Coroutine[MaterialIndexes.Length]);
				_extraInitialFlickerColors.Add(new Color[MaterialIndexes.Length]);
			}
			
			_propertyIDs = new int[MaterialIndexes.Length];
			_propertiesFound = new bool[MaterialIndexes.Length];
			_propertyBlock = new MaterialPropertyBlock();

			AcquireRenderers(owner);
			StoreSpriteRendererTexture();

			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_propertiesFound[i] = false;
				int index = MaterialIndexes[i];

				if (Active && (BoundRenderer != null))
				{
					if (Mode == Modes.Color)
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(_colorPropertyName) : BoundRenderer.materials[index].HasProperty(_colorPropertyName);
						if (_propertiesFound[i])
						{
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].color : BoundRenderer.materials[index].color;
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].color : renderer.materials[index].color;
							}
						}
					}
					else
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(PropertyName) : BoundRenderer.materials[index].HasProperty(PropertyName); 
						if (_propertiesFound[i])
						{
							_propertyIDs[i] = Shader.PropertyToID(PropertyName);
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : BoundRenderer.materials[index].GetColor(_propertyIDs[i]);
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : renderer.materials[index].GetColor(_propertyIDs[i]);
							}
						}
					}
				}
			}
		}

		protected virtual void AcquireRenderers(MMF_Player owner)
		{
			if (Active && (BoundRenderer == null) && (owner != null))
			{
				if (Owner.gameObject.MMFGetComponentNoAlloc<Renderer>() != null)
				{
					BoundRenderer = owner.GetComponent<Renderer>();
				}
				if (BoundRenderer == null)
				{
					BoundRenderer = owner.GetComponentInChildren<Renderer>();
				}
			}
			if (BoundRenderer == null)
			{
				Debug.LogWarning("[Flicker Feedback] The flicker feedback on "+Owner.name+" doesn't have a bound renderer, it won't work. You need to specify a renderer to flicker in its inspector.");
			}

			if (BoundRenderer != null)
			{
				_spriteRenderer = BoundRenderer.GetComponent<SpriteRenderer>();	
			}
			
			_spriteRenderers = new List<SpriteRenderer>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				if (renderer.GetComponent<SpriteRenderer>() != null)
				{
					_spriteRenderers.Add(renderer.GetComponent<SpriteRenderer>());
				}
			}
			_spriteRendererIsNull = _spriteRenderer == null;
		}

		protected virtual void StoreSpriteRendererTexture()
		{
			if (_spriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
			_spriteRendererTextures = new List<Texture2D>();
			for (var index = 0; index < ExtraBoundRenderers.Count; index++)
			{
				_spriteRendererTextures.Add(_spriteRenderers[index].sprite.texture);
			}
		}

		/// <summary>
		/// On play we make our renderer flicker
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (BoundRenderer == null))
			{
				return;
			}
			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				if (_coroutines[i] != null) { Owner.StopCoroutine(_coroutines[i]); }
				_coroutines[i] = Owner.StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerPeriod, FeedbackDuration));
				for (var index = 0; index < ExtraBoundRenderers.Count; index++)
				{
					_extraCoroutines[index][i] = Owner.StartCoroutine(Flicker(ExtraBoundRenderers[index], i, _extraInitialFlickerColors[index][i], FlickerColor, FlickerPeriod, FeedbackDuration));
				}
			}
		}

		/// <summary>
		/// On reset we make our renderer stop flickering
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (BoundRenderer != null))
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(BoundRenderer, i, _initialFlickerColors[i]);
				}
			}
			
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(renderer, i, _extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i]);
				}
			}
		}
		
		protected virtual void SetStoredSpriteRendererTexture(Renderer renderer, MaterialPropertyBlock block)
		{
			if (_spriteRendererIsNull)
			{
				return;
			}

			if (renderer == BoundRenderer)
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);	
			}
			else
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTextures[ExtraBoundRenderers.IndexOf(renderer)]);
			}
		}

		public virtual IEnumerator Flicker(Renderer renderer, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
		{
			if (renderer == null)
			{
				yield break;
			}

			if (!_propertiesFound[materialIndex])
			{
				yield break;
			}

			if (initialColor == flickerColor)
			{
				yield break;
			}

			float flickerStop = FeedbackTime + flickerDuration;
			IsPlaying = true;
			
			StoreSpriteRendererTexture();
            
			while (FeedbackTime < flickerStop)
			{
				SetColor(renderer, materialIndex, flickerColor);
				yield return WaitFor(flickerSpeed);
				SetColor(renderer, materialIndex, initialColor);
				yield return WaitFor(flickerSpeed);
			}

			SetColor(renderer, materialIndex, initialColor);
			IsPlaying = false;
		}


		protected virtual void SetColor(Renderer renderer, int materialIndex, Color color)
		{
			if (!_propertiesFound[materialIndex])
			{
				return;
			}
            
			if (Mode == Modes.Color)
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_colorPropertyName, color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].color = color;
				}
			}
			else
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_propertyIDs[materialIndex], color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].SetColor(_propertyIDs[materialIndex], color);
				}
			}            
		}
        
		/// <summary>
		/// Stops this feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			IsPlaying = false;
			for (int i = 0; i < _coroutines.Length; i++)
			{
				if (_coroutines[i] != null)
				{
					Owner.StopCoroutine(_coroutines[i]);    
				}
				_coroutines[i] = null;  
			}
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					if (_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] != null)
					{
						Owner.StopCoroutine(_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i]);
					}
					_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] = null;
				}
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

			CustomReset();
		}
	}
}
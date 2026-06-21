using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to quickly animate a sprite renderer or an image using a list of sprites, looping or not, at a specified frame rate, with an optional random offset.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("A feedback used to quickly animate a sprite renderer or an image using a list of sprites, looping or not, at a specified frame rate, with an optional random offset.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Animation/Sprite Sheet Animation")]
	public class MMF_SpriteSheetAnimation : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		#endif
		
		/// the duration of this feedback is the declared duration 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DetermineDuration()); } set {   } } 

		[MMFInspectorGroup("Targets", true, 13, true)]
		/// the list of SpriteRenderers to animate
		[Tooltip("the list of SpriteRenderers to animate")]
		public List<SpriteRenderer> TargetSpriteRenderers;
		/// the list of Images to animate
		[Tooltip("the the list of Images to animate")]
		public List<Image> TargetImages;
		
		[MMFInspectorGroup("Animation", true, 12, true)]
		/// a list of sprites to use as the sequential animation
		[Tooltip("a list of sprites to use as the sequential animation")]
		public List<Sprite> AnimationSprites;
		/// the number of frames per second to use for the animation
		[Tooltip("the number of frames per second to use for the animation")]
		public int FrameRate = 12;
		/// the minimum and maximum random offset to apply to the animation, useful to create a bit of variety in the animation
		[Tooltip("the minimum and maximum random offset to apply to the animation, useful to create a bit of variety in the animation")]
		[MMVector("Min", "Max")]
		public Vector2Int RandomOffset = Vector2Int.zero;
		/// whether the animation should loop or not once it reaches the last sprite in the AnimationSprites list
		[Tooltip("whether the animation should loop or not once it reaches the last sprite in the AnimationSprites list")] 
		public bool Loop = false;

		protected Coroutine _animationCoroutine;
		protected List<int> _spriteRendererOffsets;
		protected List<int> _imageOffsets;
		protected List<Sprite> _spriteRendererInitialSprites;
		protected List<Sprite> _imageInitialSprites;
		
		/// <summary>
		/// Determines the duration of the animation based on the number of sprites and the frame rate.
		/// </summary>
		/// <returns></returns>
		protected virtual float DetermineDuration()
		{
			if (AnimationSprites == null)
			{
				return 0f;
			}
			if (AnimationSprites.Count > 0)
			{
				return (float) AnimationSprites.Count / FrameRate;
			}
			else
			{
				return 0f;
			}
		}
		
		/// <summary>
		/// Custom Init
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			// initializes the lists of initial sprites 

			_spriteRendererInitialSprites = new List<Sprite>();
			if (TargetSpriteRenderers == null)
			{
				TargetSpriteRenderers = new List<SpriteRenderer>();
			}
			if (TargetImages == null)
			{
				TargetImages = new List<Image>();
			}
			foreach (SpriteRenderer spriteRenderer in TargetSpriteRenderers)
			{
				if (spriteRenderer == null) { _spriteRendererInitialSprites.Add(null); continue; }
				_spriteRendererInitialSprites.Add(spriteRenderer.sprite);
			}
			_imageInitialSprites = new List<Sprite>();
			foreach (Image image in TargetImages)
			{
				if (image == null) { _imageInitialSprites.Add(null); continue; }
				_imageInitialSprites.Add(image.sprite);
			}
			
			// initializes the lists of random offsets
			
			_spriteRendererOffsets = new List<int>();
			foreach (SpriteRenderer spriteRenderer in TargetSpriteRenderers)
			{
				int offset = Random.Range(RandomOffset.x, RandomOffset.y+1);
				_spriteRendererOffsets.Add(offset);
			}
			_imageOffsets = new List<int>();
			foreach (Image image in TargetImages)
			{
				int offset = Random.Range(RandomOffset.x, RandomOffset.y+1);
				_imageOffsets.Add(offset);
			}
		}
		
		/// <summary>
		/// On RestoreInitialValues, we restore the initial sprites of our sprite renderers and images
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			base.CustomRestoreInitialValues();
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			StopAnimationCoroutine();

			for (var i = 0; i < TargetSpriteRenderers.Count; i++)
			{
				if (TargetSpriteRenderers[i] == null) { continue; }
				TargetSpriteRenderers[i].sprite = _spriteRendererInitialSprites[i];
			}
			
			for (var i = 0; i < TargetImages.Count; i++)
			{
				if (TargetImages[i] == null) { continue; }
				TargetImages[i].sprite = _imageInitialSprites[i];
			}
		}

		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			StopAnimationCoroutine();
			_animationCoroutine = Owner.StartCoroutine(AnimateSprites());
		}

		/// <summary>
		/// Animates the sprites of the target sprite renderers and images at the specified frame rate.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AnimateSprites()
		{
			if (AnimationSprites == null || AnimationSprites.Count == 0)
			{
				yield break;
			}
			
			float delay = 1f / FrameRate;
			int index = 0;

			while (true)
			{
				SetSprite(index);
				index = (index + 1) % AnimationSprites.Count;
				if (!Loop && index == 0)
				{
					yield break;
				}
				yield return WaitFor(delay);
			}
		}

		/// <summary>
		/// Sets the sprite of the target sprite renderers and images based on the current index and optional offset
		/// </summary>
		/// <param name="index"></param>
		protected virtual void SetSprite(int index)
		{
			int newIndex = index;

			for (var i = 0; i < TargetSpriteRenderers.Count; i++)
			{
				newIndex = index + _spriteRendererOffsets[i];
				if (!Loop && newIndex >= AnimationSprites.Count)
				{
					continue;
				}
				if (Loop && newIndex >= AnimationSprites.Count)
				{
					newIndex = newIndex % AnimationSprites.Count;
				}
				SpriteRenderer spriteRenderer = TargetSpriteRenderers[i];
				if (spriteRenderer == null) { continue; }
				spriteRenderer.sprite = AnimationSprites[newIndex];
			}

			for (var i = 0; i < TargetImages.Count; i++)
			{
				newIndex = index + _imageOffsets[i];
				if (!Loop && newIndex >= AnimationSprites.Count)
				{
					continue; 
				}
				if (Loop && newIndex >= AnimationSprites.Count)
				{
					newIndex = newIndex % AnimationSprites.Count;
				}
				Image image = TargetImages[i];
				if (image == null) { continue; }
				image.sprite = AnimationSprites[newIndex];
			}
		}
        
		/// <summary>
		/// On stop, we stop the animation coroutine if it's running
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			StopAnimationCoroutine();
		}

		/// <summary>
		/// Stops the animation coroutine if it's running.
		/// </summary>
		protected virtual void StopAnimationCoroutine()
		{
			if (_animationCoroutine != null)
			{
				Owner.StopCoroutine(_animationCoroutine);
				_animationCoroutine = null;
			}
			
		}
	}
}
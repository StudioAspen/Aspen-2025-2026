using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
#if MM_UGUI2
using MoreMountains.Tools;
using TMPro;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you reveal words, lines, or characters in a target TMP, one at a time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you reveal words, lines, or characters in a target TMP, one at a time")]
	#if MM_UGUI2
	[FeedbackPath("TextMesh Pro/TMP Text Reveal")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.TextMeshPro")]
	public class MMF_TMPTextReveal : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTMPText be set to be able to work properly. You can set one below."; } }
		#endif
		#if UNITY_EDITOR && MM_UGUI2
		public override bool EvaluateRequiresSetup() { return (TargetTMPText == null); }
		public override string RequiredTargetText { get { return TargetTMPText != null ? TargetTMPText.name : "";  } }
		#endif

		protected string _originalText;
		
		#if MM_UGUI2
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTMPText = FindAutomatedTarget<TMP_Text>();

		protected TMP_TextInfo _textInfo;

		/// the duration of this feedback 
		public override float FeedbackDuration
		{
			get
			{
				if (DurationMode == DurationModes.TotalDuration)
				{
					return RevealDuration;
				}
				else
				{
					if (TargetTMPText == null)
					{
						return 0f;
					}
					
					if (TargetTMPText.textInfo == null)
					{
						bool initiallyActive = TargetTMPText.gameObject.activeSelf;
						TargetTMPText.gameObject.SetActive(true);
						TargetTMPText.ForceMeshUpdate(true);
						TargetTMPText.gameObject.SetActive(initiallyActive);
					}

					if (AllowHierarchyActivationForDurationComputation)
					{
						List<Transform> disabledParents = TargetTMPText.transform.MMEnumerateAllParents(true).Where(p => !p.gameObject.activeSelf).ToList();
						disabledParents.ForEach(p => p.gameObject.SetActive(true));
						TargetTMPText.ForceMeshUpdate(true);
						disabledParents.ForEach(p => p.gameObject.SetActive(false));
					}

					if (TargetTMPText.textInfo == null)
					{
						return 0f;
					}

					float foundLength = 0f;

					if (ReplaceText)
					{
						_originalText = TargetTMPText.text;
						TargetTMPText.text = NewText;
					}
					
					switch (RevealMode)
					{
						case RevealModes.Character:
							foundLength = RichTextLength(TargetTMPText.text) * IntervalBetweenReveals;
							break;
						case RevealModes.Lines:
							foundLength = TargetTMPText.textInfo.lineCount * IntervalBetweenReveals;
							break;
						case RevealModes.Words:
							foundLength = TargetTMPText.textInfo.wordCount * IntervalBetweenReveals;
							break;
					}

					if (ReplaceText)
					{
						TargetTMPText.text = _originalText;
					}

					return foundLength;
				}                
			}
			set
			{
				if (DurationMode == DurationModes.TotalDuration)
				{
					RevealDuration = value;
					
					if (TargetTMPText != null)
					{
						if (ReplaceText)
						{
							_originalText = TargetTMPText.text;
							TargetTMPText.text = NewText;
						}
						switch (RevealMode)
						{
							case RevealModes.Character:
								IntervalBetweenReveals = value / RichTextLength(TargetTMPText.text);
								break;
							case RevealModes.Lines:
								IntervalBetweenReveals = value / TargetTMPText.textInfo.lineCount;
								break;
							case RevealModes.Words:
								IntervalBetweenReveals = value / TargetTMPText.textInfo.wordCount;
								break;
						}
						if (ReplaceText)
						{
							TargetTMPText.text = _originalText;
						}
					}
				}
				else
				{
					if (TargetTMPText != null)
					{
						if (ReplaceText)
						{
							_originalText = TargetTMPText.text;
							TargetTMPText.text = NewText;
						}
						switch (RevealMode)
						{
							case RevealModes.Character:
								IntervalBetweenReveals = value / RichTextLength(TargetTMPText.text);
								break;
							case RevealModes.Lines:
								IntervalBetweenReveals = value / TargetTMPText.textInfo.lineCount;
								break;
							case RevealModes.Words:
								IntervalBetweenReveals = value / TargetTMPText.textInfo.wordCount;
								break;
						}
						if (ReplaceText)
						{
							TargetTMPText.text = _originalText;
						}
					}
				}
			}
		}
		
		#endif

		/// the possible ways to reveal the text
		public enum RevealModes { Character, Lines, Words }
		/// whether to define duration by the time interval between two unit reveals, or by the total duration the reveal should take
		public enum DurationModes { Interval, TotalDuration }

		#if MM_UGUI2
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("the target TMP_Text component we want to change the text on")]
		public TMP_Text TargetTMPText;
		#endif

		[MMFInspectorGroup("Change Text", true, 13)]

		/// whether or not to replace the current TMP target's text on play
		[Tooltip("whether or not to replace the current TMP target's text on play")]
		public bool ReplaceText = false;
		/// if this is true, the maxVisible Characters/Lines/Words will be set to 0 on initialization
		[Tooltip("if this is true, the maxVisible Characters/Lines/Words will be set to 0 on initialization")]
		public bool HideTextOnInitialization = false;
		/// the new text to replace the old one with
		[Tooltip("the new text to replace the old one with")]
		[TextArea]
		public string NewText = "Hello World";

		[MMFInspectorGroup("Reveal", true, 14)]
		/// the selected way to reveal the text (character by character, word by word, or line by line)
		[Tooltip("the selected way to reveal the text (character by character, word by word, or line by line)")]
		public RevealModes RevealMode = RevealModes.Character;
		/// whether to define duration by the time interval between two unit reveals, or by the total duration the reveal should take
		[Tooltip("whether to define duration by the time interval between two unit reveals, or by the total duration the reveal should take")]
		public DurationModes DurationMode = DurationModes.Interval;
		/// the interval (in seconds) between two reveals
		[Tooltip("the interval (in seconds) between two reveals")]
		[MMFEnumCondition("DurationMode", (int)DurationModes.Interval)]
		public float IntervalBetweenReveals = 0.05f;
		/// the total duration of the text reveal, in seconds
		[Tooltip("the total duration of the text reveal, in seconds")]
		[MMFEnumCondition("DurationMode", (int)DurationModes.TotalDuration)]
		public float RevealDuration = 1f;
		/// a UnityEvent to invoke every time a reveal happens (word, line or character)
		[Tooltip("a UnityEvent to invoke every time a reveal happens (word, line or character)")]
		public UnityEvent OnReveal;
		/// alright so that one will be weird : for reasons, TextMeshPro won't let you read the length of a disabled text, so to do so, we need to enable it, even if it's just to disable it again right after. If you're targeting a disabled text, or a text that is part of a disabled hierarchy, you'll probably want to set this to true so that the system can proceed with accurate duration computation. If you don't, and your target transform is disabled, duration won't be computed correctly.
		[Tooltip("alright so that one will be weird : for reasons, TextMeshPro won't let you read the length of a disabled text, so to do so, we need to enable it, even if it's just to disable it again right after. If you're targeting a disabled text, or a text that is part of a disabled hierarchy, you'll probably want to set this to true so that the system can proceed with accurate duration computation. If you don't, and your target transform is disabled, duration won't be computed correctly.")]
		public bool AllowHierarchyActivationForDurationComputation = false;

		protected float _delay;
		protected Coroutine _coroutine;
		protected int _richTextLength;

		protected int _totalCharacters;
		protected int _totalLines;
		protected int _totalWords;
		protected string _initialText;
		protected int _indexLastTime = -1;

		/// <summary>
		/// Sets the maximum amount of visible characters/words/lines to 0 if needed 
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_UGUI2
            
			if (TargetTMPText == null)
			{
				return;
			}
			
			if (HideTextOnInitialization)
			{
				switch (RevealMode)
				{
					case RevealModes.Character:
						TargetTMPText.maxVisibleCharacters = 0;
						break;
					case RevealModes.Lines:
						TargetTMPText.maxVisibleLines = 0;
						break;
					case RevealModes.Words:
						TargetTMPText.maxVisibleWords = 0;
						break;
				}
			}
			
			#endif
		}

		/// <summary>
		/// On play we change the text of our target TMPText
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_UGUI2
            
			if (TargetTMPText == null)
			{
				return;
			}

			if (DurationMode == DurationModes.TotalDuration)
			{
				FeedbackDuration = RevealDuration;
			}

			_initialText = TargetTMPText.text;
			_textInfo = TargetTMPText.textInfo;

			if (ReplaceText)
			{
				TargetTMPText.text = NewText;
				TargetTMPText.ForceMeshUpdate();
			}
			_richTextLength = RichTextLength(TargetTMPText.text);
			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
			switch (RevealMode)
			{
				case RevealModes.Character:
					_delay = (DurationMode == DurationModes.Interval) ? IntervalBetweenReveals : RevealDuration / _richTextLength;
					TargetTMPText.maxVisibleCharacters = 0;
					_coroutine = Owner.StartCoroutine(RevealCharacters());
					break;
				case RevealModes.Lines:
					_delay = (DurationMode == DurationModes.Interval) ? IntervalBetweenReveals : RevealDuration / TargetTMPText.textInfo.lineCount;
					TargetTMPText.maxVisibleLines = 0;
					_coroutine = Owner.StartCoroutine(RevealLines());
					break;
				case RevealModes.Words:
					_delay = (DurationMode == DurationModes.Interval) ? IntervalBetweenReveals : RevealDuration / TargetTMPText.textInfo.wordCount;
					TargetTMPText.maxVisibleWords = 0;
					_coroutine = Owner.StartCoroutine(RevealWords());
					break;
			}
			#endif
		}

		#if MM_UGUI2

		/// <summary>
		/// Reveals characters one at a time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RevealCharacters()
		{
			float startTime = FeedbackTime;
			_totalCharacters = _richTextLength;
			int visibleCharacters = 0;

			IsPlaying = true;
			TargetTMPText.maxVisibleCharacters = 0;

			while ((visibleCharacters < _totalCharacters) && !Owner.SkippingToTheEnd)
			{
				float currentTime = FeedbackTime;
				float elapsed = currentTime - startTime;

				int expectedVisibleCharacters = 0;

				if (DurationMode == DurationModes.Interval)
				{
					expectedVisibleCharacters = Mathf.FloorToInt(elapsed / IntervalBetweenReveals);
				}
				else 
				{
					expectedVisibleCharacters = Mathf.FloorToInt((_totalCharacters * elapsed) / RevealDuration);
				}

				expectedVisibleCharacters = Mathf.Clamp(expectedVisibleCharacters, 0, _totalCharacters);

				if (expectedVisibleCharacters > visibleCharacters)
				{
					visibleCharacters = expectedVisibleCharacters;
					TargetTMPText.maxVisibleCharacters = visibleCharacters;
					InvokeRevealEvents();
				}

				yield return null;
			}

			TargetTMPText.maxVisibleCharacters = _richTextLength;
			IsPlaying = false;
		}

		/// <summary>
		/// Reveals lines one at a time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RevealLines()
		{
			_totalLines = TargetTMPText.textInfo.lineCount;
			int visibleLines = 0;

			IsPlaying = true;
			while ((visibleLines <= _totalLines) && !Owner.SkippingToTheEnd)
			{
				TargetTMPText.maxVisibleLines = visibleLines;
				InvokeRevealEvents();
				visibleLines++;

				yield return WaitFor(_delay);
			}
			TargetTMPText.maxVisibleLines = _totalLines;
			IsPlaying = false;
		}
	        
		/// <summary>
		/// Reveals words one at a time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RevealWords()
		{
			_totalWords = TargetTMPText.textInfo.wordCount;
			int visibleWords = 0;

			IsPlaying = true;
			while ((visibleWords <= _totalWords) && !Owner.SkippingToTheEnd)
			{
				TargetTMPText.maxVisibleWords = visibleWords;
				InvokeRevealEvents();
				visibleWords++;
				yield return WaitFor(_delay);
			}
			TargetTMPText.maxVisibleWords = _totalWords;
			IsPlaying = false;
		}

		/// <summary>
		/// Invokes on reveal events
		/// </summary>
		protected virtual void InvokeRevealEvents()
		{
			if ( ((RevealMode == RevealModes.Character) && (TargetTMPText.maxVisibleCharacters == 0))
			    || ((RevealMode == RevealModes.Character) && !IsNewVisibleCharacter())
				|| ((RevealMode == RevealModes.Lines) && (TargetTMPText.maxVisibleLines == 0))
				|| ((RevealMode == RevealModes.Words) && (TargetTMPText.maxVisibleWords == 0)) )
			{
				return;
			}
			
			OnReveal?.Invoke();
		}

		/// <summary>
		/// Stops the animation if needed
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
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

		/// <summary>
		/// On skip, we display our entire text
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!IsPlaying)
			{
				return;
			}
		        
			switch (RevealMode)
			{
				case RevealModes.Character:
					TargetTMPText.maxVisibleCharacters = _totalCharacters;
					break;
				case RevealModes.Lines:
					TargetTMPText.maxVisibleLines = _totalLines;
					break;
				case RevealModes.Words:
					TargetTMPText.maxVisibleWords = _totalWords;
					break;
			}
		}
	        
		/// <summary>
		/// Returns the length of a rich text, excluding its tags
		/// </summary>
		/// <param name="richText"></param>
		/// <returns></returns>
		protected int RichTextLength(string richText)
		{
			int richTextLength = 0;
			bool insideTag = false;

			richText = richText.Replace("<br>", "-");
			var tagName = new StringBuilder();
			foreach (char character in richText)
			{
				if (character == '<')
				{
					insideTag = true;
					tagName.Clear();
					continue;
				}
				else if (character == '>')
				{
					if(tagName.ToString().StartsWith("sprite")) richTextLength++;
					insideTag = false;
				}
				else if (!insideTag)
				{
					richTextLength++;
				}
				else
				{
					tagName.Append(character);
				}
			}

			return richTextLength;
		}

		/// <summary>
		/// Returns true if the last visible letter of the TMP text is new and visible and a letter or digit
		/// </summary>
		/// <returns></returns>
		protected virtual bool IsNewVisibleCharacter()
		{
			int lastVisibleCharIndex = -1;
			_textInfo = TargetTMPText.GetTextInfo(TargetTMPText.text);

			for (int i = 0; i < _textInfo.characterCount; i++)
			{
				if (_textInfo.characterInfo[i].isVisible)
				{
					lastVisibleCharIndex = i;
				}
			}

			if ((lastVisibleCharIndex < 0) 
			    || (lastVisibleCharIndex > TargetTMPText.text.Length)
			    || (lastVisibleCharIndex == _indexLastTime))
			{
				return false;
			}
			
			_indexLastTime = lastVisibleCharIndex;
			return Char.IsLetterOrDigit(_textInfo.characterInfo[lastVisibleCharIndex].character);
		}
		
		#endif
		
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_UGUI2
			TargetTMPText.text = _initialText;
			#endif
		}
	}
}
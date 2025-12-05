using System;
using System.Collections.Generic;
using MoreMountains.Tools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace MoreMountains.Feedbacks
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MMF_Player), true)]
	public class MMF_PlayerEditorUITK : Editor
	{
		public struct FeedbackTypePair
		{
			public System.Type FeedbackType;
			public string FeedbackName;
		}

		public class FeedbackHeaderContainersData
		{
			public int Index;
			public MMF_Feedback Feedback;
			public VisualElement HeaderContainer;
			public VisualElement SetupRequiredWarningBox;
			public VisualElement ProgressBar;
			public Type FeedbackType;
		}

		public class MMFFeedbackGroupExtrasContainerData
		{
			public VisualElement HeaderExtrasContainer;
			public MMFInspectorGroupData GroupData;
			public MMFInspectorDrawData DrawData;
		}

		// Bindings 
		public StyleSheet StyleSheetBase;
		public StyleSheet StyleSheetControls;
		public StyleSheet StyleSheetFeedbacksList;
		public StyleSheet StyleSheetFoldouts;
		public StyleSheet StyleSheetSettings;
		public StyleSheet StyleSheetLightSkin;

		// Icons
		public Sprite GearIcon;
		public Sprite TopToBottomIcon;
		public Sprite BottomToTopIcon;
		public Sprite ContextlMenuIcon;
		public Sprite SetupRequiredIcon;
		public Sprite EmptyListImage;
		public Sprite ScriptIcon;
		public Sprite SearchFieldCloseIcon;

		// Properties
		public MMF_Player TargetMmfPlayer;
		public static List<string> _typeDisplays;
		public static List<string> _typeNamesDisplays;
		public static List<FeedbackTypePair> _typesAndNames = new List<FeedbackTypePair>();

		// Protected
		protected bool _settingsFoldoutIsOpen = false;
		protected SerializedProperty _inScriptDrivenPause;
		protected SerializedProperty _mmfeedbacksList;
		protected VisualElement _root;
		protected VisualElement _bottomBar;
		protected Foldout _settingsFoldout;
		protected Label _isPlayingLabel;
		protected VisualElement _settingsInfo;
		protected ListView _feedbacksListView;
		protected bool _initialized = false;
		protected bool _isPlayingLastFrame;
		protected string _feedbackLabel;
		protected IVisualElementScheduledItem _blinkingTask;
		protected Label _feedbacksListLabel;
		protected Length _progressBarLength;
		protected VisualElement _emptyFeedbackListContainer;
		protected VisualElement _automaticShakerSetupButtonContainer;
		protected StyleBackground _styleBackgroundGearIcon;
		protected StyleColor _transparentColor = new StyleColor(new Color(0, 0, 0, 0));

		protected Dictionary<MMF_Feedback, FeedbackHeaderContainersData> FeedbackHeaderContainersDictionary;
		protected Dictionary<MMFInspectorGroupData, MMFFeedbackGroupExtrasContainerData> FeedbackGroupsDictionary;

		// text constants
		protected const string _copyAllText = "Copy all";
		protected const string _pasteAsNewText = "Paste as new";
		protected const string _pasteUndoText = "Paste Feedback";
		protected const string _replaceAllText = "Replace all";
		protected const string _replaceAllUndoText = "Replace all feedbacks";
		protected const string _addUndoText = "Add new feedback";
		protected const string _removeUndoText = "Remove feedback";
		protected const string _reorderUndoText = "Reorder feedback";
		protected const string _pasteAllAsNewText = "Paste all feedbacks as new";

		protected const string _inactiveMessage =
			"All MMFeedbacks, including this one, are currently disabled. This is done via script, by changing the value of the MMFeedbacks.GlobalMMFeedbacksActive boolean. Right now this value has been set to false. Setting it back to true will allow MMFeedbacks to play again.";

		protected const string _initializationSectionText = "Initialization";
		protected const string _directionSectionText = "Direction";
		protected const string _intensitySectionText = "Intensity";
		protected const string _timingSectionText = "Timing";
		protected const string _rangeSectionText = "Range";
		protected const string _playSettingsSectionText = "Play Settings";
		protected const string _eventsSectionText = "Events";
		protected const string _settingsText = "MMF PLAYER SETTINGS";
		protected const string _isPlayingText = "PLAYING ";
		protected const string _infiniteLoopText = "[Infinite Loop] ";
		protected const string _initializeText = "Initialization";
		protected const string _playText = "Play";
		protected const string _removeText = "Remove";
		protected const string _pauseText = "Pause";
		protected const string _stopText = "Stop";
		protected const string _resetText = "Reset";
		protected const string _resetUndoText = "Reset Feedback";
		protected const string _changeDirectionText = "ChangeDirection";
		protected const string _duplicateText = "Duplicate";
		protected const string _copyText = "Copy";
		protected const string _pasteText = "Paste";
		protected const string _editScriptText = "Edit Script";
		protected const string _skipText = "SkipToTheEnd";
		protected const string _restoreText = "RestoreInitialValues";
		protected const string _keepPlaymodeChangesText = "Keep Play Mode Changes";
		protected const string _scriptEditLabelText = "Script";
		protected const string _searchFeedbackPlaceholderText = "Search...";

		protected const string _scriptDrivenInProgressText =
			"Script driven pause in progress, call ResumeFeedbacks() or press the button below to exit pause";

		protected const string _resumeText = "ResumeFeedbacks";
		protected const string _feedbacksSectionTitle = "FEEDBACKS";
		protected const string _originalLabelColor = "#666";

		protected const string _emptyFeedbackListMessage =
			"This MMF Player doesn't contain any feedbacks for the moment.\n\n Pick a feedback from the \"Add a new feedback\" dropdown below to get started.";

		protected const string _addNewFeedbackText = "Add new feedback...";
		protected const string _automaticShakerSetupText = "Automatic Shaker Setup";

		protected const string _undoText = "Modified Feedback Manager";
		//protected const string _debugControlsText = "Debug Controls";

		// property names
		protected const string _inScriptDrivenPausePropertyName = "InScriptDrivenPause";
		protected const string _feedbacksListPropertyName = "FeedbacksList";
		protected const string _feedbackBaseName = "MMF_FeedbackBase";
		protected const string _scriptPropertyName = "m_Script";
		protected const string _initializationModePropertyName = "InitializationMode";
		protected const string _autoPlayOnStartPropertyName = "AutoPlayOnStart";
		protected const string _autoPlayOnEnablePropertyName = "AutoPlayOnEnable";
		protected const string _autoInitializationPropertyName = "AutoInitialization";
		protected const string _directionPropertyName = "Direction";
		protected const string _autoChangeDirectionOnEndPropertyName = "AutoChangeDirectionOnEnd";
		protected const string _feedbacksIntensityPropertyName = "FeedbacksIntensity";
		protected const string _durationMultiplierPropertyName = "DurationMultiplier";
		protected const string _randomizeDurationPropertyName = "RandomizeDuration";
		protected const string _randomDurationMultiplierPropertyName = "RandomDurationMultiplier";
		protected const string _displayFullDurationDetailsPropertyName = "DisplayFullDurationDetails";
		protected const string _cooldownDurationPropertyName = "CooldownDuration";
		protected const string _initialDelayPropertyName = "InitialDelay";
		protected const string _chanceToPlayPropertyName = "ChanceToPlay";
		protected const string _playerTimescaleModePropertyName = "PlayerTimescaleMode";
		protected const string _forceTimescaleModePropertyName = "ForceTimescaleMode";
		protected const string _forcedTimescaleModePropertyName = "ForcedTimescaleMode";
		protected const string _timescaleMultiplierPropertyName = "TimescaleMultiplier";
		protected const string _rangeCenterPropertyName = "RangeCenter";
		protected const string _rangeDistancePropertyName = "RangeDistance";
		protected const string _useRangeFalloffPropertyName = "UseRangeFalloff";
		protected const string _onlyPlayIfWithinRangePropertyName = "OnlyPlayIfWithinRange";
		protected const string _rangeFalloffPropertyName = "RangeFalloff";
		protected const string _remapRangeFalloffPropertyName = "RemapRangeFalloff";
		protected const string _ignoreRangeEventsPropertyName = "IgnoreRangeEvents";
		protected const string _canPlayPropertyName = "CanPlay";
		protected const string _canPlayWhileAlreadyPlayingPropertyName = "CanPlayWhileAlreadyPlaying";
		protected const string _performanceModePropertyName = "PerformanceMode";
		protected const string _stopFeedbacksOnDisablePropertyName = "StopFeedbacksOnDisable";
		protected const string _restoreInitialValuesOnDisablePropertyName = "RestoreInitialValuesOnDisable";
		protected const string _playCountPropertyName = "PlayCount";
		protected const string _labelPropertyName = "Label";
		protected const string _infiniteLoopPropertyName = "InfiniteLoop";
		
		protected const string _channelModePropertyName = "MMF_ChannelMode";
		protected const string _channelPropertyName = "MMF_Channel";
		protected const string _mmChannelDefinitionPropertyName = "MMF_ChannelDefinition";

		// class names
		public const string _foldoutClassName = "mm-foldout";
		public const string _iconClassName = "mm-icon";
		public const string _setupRequiredIconClassName = "mm-setup-required-icon";
		public const string _foldoutToggleClassName = "mm-foldout-toggle";
		protected const string _objectSelectorClassName = "unity-object-field__selector";
		protected const string _mmfEditorClassName = "mmf-editor";
		protected const string _settingsFoldoutSuffix = "- SettingsFoldout";
		protected const string _settingsFoldoutSubClassName = "mm-settings-foldout-sub";
		protected const string _settingsFoldoutSubToggleClassName = "mm-settings-foldout-sub-toggle"; 
		protected const string _settingsFoldoutToggleClassName = "mm-settings-foldout-toggle";
		protected const string _settingsFoldoutClassName = "mm-settings-foldout";
		protected const string _iconSettingsClassName = "mm-settings-icon";
		protected const string _scriptClassName = "mm-script-field";
		protected const string _settingsInfoClassName = "mm-settings-info";
		protected const string _settingsInfoSuffix = "- SettingsInfo";
		protected const string _unityFoldoutTextClassName = "unity-foldout__text";
		protected const string _settingsIsPlayingClassName = "mm-settings-is-playing";
		protected const string _settingsDurationClassName = "mm-settings-duration";
		protected const string _directionIconClassName = "mm-direction-icon";
		protected const string _inactiveWarningClassName = "mm-feedbacks-inactive-warning";
		protected const string _feedbackFoldoutClassName = "mm-feedback-foldout";
		protected const string _bottomBarClassName = "mm-bottom-bar";
		protected const string _controlsClassName = "mm-controls";
		protected const string _searchFieldClassName = "mm-add-feedback-search-field";
		protected const string _searchFieldPlaceholderClassName = "mm-add-feedback-search-field-placeholder";
		protected const string _playModeButtonActiveClassName = "mm-playmode-button-active";
		protected const string _searchResultsRowClassName = "mm-search-results-row";
		protected const string _searchAddFeedbackButtonClassName = "mm-search-add-feedback-button";
		protected const string _scriptDrivenPauseLabelClassName = "mm-script-driven-pause-label";
		protected const string _scriptDrivenPauseLabelBlinkClassName = "mm-script-driven-pause-label-blink";
		protected const string _feedbackControlButtonsClassName = "mm-feedback-control-buttons";
		protected const string _feedbackHeaderContainerClassName = "mm-feedback-header-container";
		protected const string _feedbackActiveCheckboxClassName = "mm-feedback-active-checkbox";
		protected const string _automaticShakerSetupButtonClassName = "automatic-shaker-setup-button";
		protected const string _settingsInitClassNameSuffix = "- SettingsFoldoutInitialization";
		protected const string _settingsDirectionClassNameSuffix = "- SettingsFoldoutDirection";
		protected const string _settingsIntensityClassNameSuffix = "- SettingsFoldoutIntensity";
		protected const string _settingsTimingClassNameSuffix = "- SettingsFoldoutTiming";
		protected const string _settingsRangeClassNameSuffix = "- SettingsFoldoutRange";
		protected const string _settingsPlaySettingsClassNameSuffix = "- SettingsFoldoutPlaySettings";
		protected const string _addNewFeedbackPopupFieldLabel = "addNewFeedbackPopupField";
		protected const string _emptyListImageClassName = "mm-empty-list-image";
		protected const string _feedbacksListEmptyClassName = "mm-feedbacks-list-empty";
		protected const string _feedbacksListViewClassNameSuffix = "- feedbacksListView";
		protected const string _feedbacksListClassName = "mm-feedbacks-list";
		protected const string _feedbacksListContainerClassNameSuffix = "- feedbacksListContainer";
		protected const string _feedbacksListTitleClassName = "mm-feedbacks-list-title";
		protected const string _settingsFoldoutClassNameSuffix = "- SettingsFoldout - ";
		protected const string _feedbackHelpBoxClassName = "mm-feedback-help-box";
		protected const string _feedbackHelpLabelClassName = "mm-feedback-help-label";
		protected const string _feedbackEditScriptButtonContainerClassName = "mm-feedback-edit-script-button-container";
		protected const string _feedbackEditScriptButtonLabelClassName = "mm-feedback-edit-script-button-label";
		protected const string _feedbackEditScriptButtonBoxClassName = "mm-feedback-edit-script-button-box";
		protected const string _feedbackProgressLineContainerClassName = "mm-feedback-progress-line-container";
		protected const string _feedbackProgressLineClassName = "mm-feedback-progress-line";
		protected const string _feedbackSetupRequiredBoxClassName = "mm-feedback-setup-required-box";
		protected const string _feedbackSetupRequiredLabelClassName = "mm-feedback-setup-required-label";
		protected const string _feedbackInspectorContainerClassName = "mm-feedback-inspector-container";
		protected const string _feedbackReorderableItemClassName = "mm-feedback-reorderable-item";
		protected const string _feedbackLeftBorderClassName = "mm-feedback-left-border";
		protected const string _feedbackBackgroundColorClassName = "mm-feedback-background-color";
		protected const string _feedbackFoldoutLabelClassName = "mm-feedback-foldout-label";
		protected const string _feedbackTimingLabelClassName = "mm-feedback-timing-label";
		protected const string _feedbackDirectionIconClassName = "mm-feedback-direction-icon";
		protected const string _feedbackContextualMenuButtonClassName = "mm-feedback-contextual-menu-button";
		protected const string _contextMenuIconClassName = "mm-context-menu-icon";
		protected const string _feedbackRequiredTargetLabelClassName = "mm-feedback-required-target-label";

		#region LIFE CYCLE

		private void OnEnable()
		{
			ClearDictionaries();
			Undo.undoRedoPerformed += OnUndoRedo;
			EditorApplication.update += OnEditorUpdate;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
			EditorApplication.update -= OnEditorUpdate;
		}

		public override bool RequiresConstantRepaint() => false;

		#endregion

		#region UPDATE EVENTS

		private void OnEditorUpdate()
		{
			if (!_initialized)
			{
				return;
			}

			if (Application.isPlaying)
			{
				if (TargetMmfPlayer.IsPlaying)
				{
					foreach (var headerData in FeedbackHeaderContainersDictionary)
					{
						UpdateProgressBar(headerData.Value);
					}
				}

				if (TargetMmfPlayer.IsPlaying != _isPlayingLastFrame)
				{
					// display IsPlaying label in the top bar
					_isPlayingLabel.style.display = TargetMmfPlayer.IsPlaying ? DisplayStyle.Flex : DisplayStyle.None;
					if (!TargetMmfPlayer.IsPlaying)
					{
						foreach (var headerData in FeedbackHeaderContainersDictionary)
						{
							headerData.Value.ProgressBar.style.width = 0f;
						}
					}
				}
			}

			_isPlayingLastFrame = TargetMmfPlayer.IsPlaying;
		}

		protected virtual void UpdateProgressBar(FeedbackHeaderContainersData data)
		{
			if (!TargetMmfPlayer.IsPlaying)
			{
				data.ProgressBar.style.width = 0f;
				return;
			}

			float totalDuration = data.Feedback.TotalDuration - data.Feedback.Timing.InitialDelay;
			float startedAt = data.Feedback.FeedbackStartedAt;
			float thisTime = data.Feedback.Timing.TimescaleMode == TimescaleModes.Scaled
				? Time.time
				: Time.unscaledTime;

			if (totalDuration == 0f)
			{
				totalDuration = 0.1f;
			}

			if (startedAt == 0f)
			{
				startedAt = 0.001f;
			}

			if ((startedAt > 0f) && (thisTime - startedAt < totalDuration + 0.05f))
			{
				if (totalDuration == 0f)
				{
					totalDuration = 0.1f;
				}

				float percent = ((thisTime - startedAt) / totalDuration) * 100f;

				_progressBarLength.value = percent;
				data.ProgressBar.style.width = _progressBarLength;
			}
			else
			{
				data.ProgressBar.style.width = 0f;
			}
		}

		protected virtual void OnAnyValueChanged(SerializedProperty property)
		{
			// triggers for any value changed anywhere on the feedback
			DrawDurationAndDirectionContents();
			RedrawAllFeedbackHeaders();
		}

		protected virtual void OnFeedbackFieldValueChanged(SerializedProperty property, MMFInspectorGroupData groupData,
			MMF_Feedback feedback) // triggers for any value changed anywhere on the feedback
		{
			// we update only that feedback's header 
			FeedbackHeaderContainersDictionary[feedback].HeaderContainer =
				DrawFeedbackHeaderContainer(FeedbackHeaderContainersDictionary[feedback]);
			// we update that field's group header
			MMF_FeedbackPropertyDrawerUITK.DrawGroupExtrasContainer(FeedbackGroupsDictionary[groupData]);
		}

		protected virtual void OnFeedbackListReorder()
		{
			Undo.RecordObject(target, _reorderUndoText);
			RedrawFeedbacksList();
			SavePlayerChanges();
		}

		protected virtual void OnUndoRedo()
		{
			if (serializedObject == null)
			{
				return;
			}

			if (serializedObject.targetObject == null)
			{
				return;
			}

			serializedObject.Update();
			CacheFeedbacksListProperty();
			RedrawFeedbacksList();
		}

		#endregion

		public override VisualElement CreateInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, _undoText);

			Initialization();
			DrawScriptField(_root);
			DrawFeedbacksInactiveWarning(_root);
			DrawSettingsDropDown(_root);
			DrawDurationAndDirection();
			DrawFeedbacksList(_root);
			DrawBottomBar(_root);
			DrawDebugControls(_root);
			SavePlayerChanges();
			return _root;
		}

		protected virtual void Initialization()
		{
			TargetMmfPlayer = (MMF_Player)target;

			CacheFeedbacksListProperty();
			_inScriptDrivenPause = serializedObject.FindProperty(_inScriptDrivenPausePropertyName);
			PrepareFeedbackTypeList();

			FeedbackHeaderContainersDictionary = new Dictionary<MMF_Feedback, FeedbackHeaderContainersData>();
			FeedbackGroupsDictionary = new Dictionary<MMFInspectorGroupData, MMFFeedbackGroupExtrasContainerData>();

			_styleBackgroundGearIcon = new StyleBackground(GearIcon);

			_progressBarLength = new Length(0f, LengthUnit.Percent);

			// draw the root
			_root = new VisualElement();
			_root.AddToClassList(_mmfEditorClassName);
			_root.styleSheets.Add(StyleSheetBase);
			_root.styleSheets.Add(StyleSheetFoldouts);
			_root.styleSheets.Add(StyleSheetControls);
			_root.styleSheets.Add(StyleSheetSettings);
			_root.styleSheets.Add(StyleSheetFeedbacksList);
			if (!EditorGUIUtility.isProSkin)
			{
				_root.styleSheets.Add(StyleSheetLightSkin);
			}

			// listen for changes on all properties in the MMF Player
			SerializedProperty propertyIterator = serializedObject.GetIterator();
			if (propertyIterator.NextVisible(true))
			{
				do
				{
					_root.TrackPropertyValue(propertyIterator, this.OnAnyValueChanged);
				} while (propertyIterator.NextVisible(false));
			}

			_initialized = true;
		}

		protected virtual void CacheFeedbacksListProperty()
		{
			_mmfeedbacksList = serializedObject.FindProperty(_feedbacksListPropertyName);
		}

		protected virtual void PrepareFeedbackTypeList()
		{
			if (_typeDisplays == null)
			{
				_typeDisplays = new List<string>();
			}
			
			if (_typeNamesDisplays == null)
			{
				_typeNamesDisplays = new List<string>();
			}

			if (_typeDisplays.Count > 0)
			{
				return;
			}

			// Retrieve available feedbacks
			List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where assemblyType.IsSubclassOf(typeof(MMF_Feedback))
				select assemblyType).ToList();

			// Create display list from types
			_typeDisplays.Clear();
			_typeNamesDisplays.Clear();
			for (int i = 0; i < types.Count; i++)
			{
				FeedbackTypePair _newType = new FeedbackTypePair();
				_newType.FeedbackType = types[i];
				_newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
				if ((_newType.FeedbackName == _feedbackBaseName) || (_newType.FeedbackName == null))
				{
					continue;
				}

				_typesAndNames.Add(_newType);
			}

			_typesAndNames = _typesAndNames.OrderBy(t => t.FeedbackName).ToList();

			_typeDisplays.Add(_addNewFeedbackText);
			for (int i = 0; i < _typesAndNames.Count; i++)
			{
				_typeDisplays.Add(_typesAndNames[i].FeedbackName);
				_typeNamesDisplays.Add(_typesAndNames[i].FeedbackName.Split('/').Last());
			}
		}

		protected virtual void DrawScriptField(VisualElement root)
		{
			SerializedProperty scriptProperty = serializedObject.FindProperty(_scriptPropertyName);
			PropertyField scriptField = new PropertyField(scriptProperty);
			scriptField.AddToClassList(_scriptClassName);
			scriptField.SetEnabled(false);
			root.Add(scriptField);
		}

		protected virtual void DrawDurationAndDirection()
		{
			_settingsInfo = new VisualElement();
			_settingsInfo.AddToClassList(_settingsInfoClassName);
			_settingsInfo.viewDataKey = target.name + _settingsInfoSuffix;
			DrawDurationAndDirectionContents();
		}

		public virtual void DrawDurationAndDirectionContents()
		{
			_settingsInfo.Clear();
			VisualElement foldoutLabel = _settingsFoldout.Q<VisualElement>(className: _unityFoldoutTextClassName);
			foldoutLabel.parent.Add(_settingsInfo);

			// IS PLAYING label
			_isPlayingLabel = new Label(_isPlayingText);
			_isPlayingLabel.AddToClassList(_settingsIsPlayingClassName);
			_isPlayingLabel.style.display = TargetMmfPlayer.IsPlaying ? DisplayStyle.Flex : DisplayStyle.None;
			_settingsInfo.Add(_isPlayingLabel);

			// feedback duration label
			Label durationLabel = new Label("[" + TargetMmfPlayer.TotalDuration.ToString("F2") + "s]");
			durationLabel.AddToClassList(_settingsDurationClassName);
			_settingsInfo.Add(durationLabel);

			// direction icon
			VisualElement directionIcon = new VisualElement();
			directionIcon.AddToClassList(_iconClassName);
			directionIcon.AddToClassList(_directionIconClassName);
			directionIcon.style.backgroundImage = (TargetMmfPlayer.Direction == MMF_Player.Directions.BottomToTop)
				? new StyleBackground(BottomToTopIcon)
				: new StyleBackground(TopToBottomIcon);
			_settingsInfo.Add(directionIcon);
		}

		protected virtual void DrawFeedbacksInactiveWarning(VisualElement root)
		{
			// displays a label if feedbacks have been turned off via a global variable
			if (!MMF_Player.GlobalMMFeedbacksActive)
			{
				VisualElement feedbacksInactiveWarning = new VisualElement();
				feedbacksInactiveWarning.AddToClassList(_inactiveWarningClassName);
				Label feedbacksInactiveLabel = new Label(_inactiveMessage);
				feedbacksInactiveWarning.Add(feedbacksInactiveLabel);
				root.Add(feedbacksInactiveWarning);
			}
		}

		protected virtual void DrawSettingsDropDown(VisualElement root)
		{
			// settings foldout
			_settingsFoldout = new Foldout();
			_settingsFoldout.text = _settingsText;
			_settingsFoldout.value = _settingsFoldoutIsOpen;
			_settingsFoldout.AddToClassList(_foldoutClassName);
			_settingsFoldout.AddToClassList(_settingsFoldoutClassName);
			_settingsFoldout.viewDataKey = target.name + _settingsFoldoutSuffix;
			_settingsFoldout.Q<Toggle>().AddToClassList(_foldoutToggleClassName);
			_settingsFoldout.Q<Toggle>().AddToClassList(_settingsFoldoutToggleClassName);
			root.Add(_settingsFoldout);

			// icon on the left
			VisualElement settingsIcon = new VisualElement();
			settingsIcon.AddToClassList(_iconClassName);
			settingsIcon.AddToClassList(_iconSettingsClassName);
			settingsIcon.style.backgroundImage = _styleBackgroundGearIcon;
			VisualElement foldoutLabel = _settingsFoldout.Q<VisualElement>(className: _unityFoldoutTextClassName);
			foldoutLabel.style.flexGrow = 1;
			foldoutLabel.parent.Insert(0, settingsIcon);

			// initialization foldout 
			Foldout settingsInitializationFoldout =
				CreateSettingsSubFoldout(_initializationSectionText, _settingsFoldout);
			settingsInitializationFoldout.value = false;
			settingsInitializationFoldout.viewDataKey = target.name + _settingsInitClassNameSuffix;
			settingsInitializationFoldout.style.borderLeftColor =
				new StyleColor(MMColors.CreateColor(249, 246, 216, 255));
			HandleFoldoutChange(settingsInitializationFoldout, BuildSettingsInitializationFoldout);

			void BuildSettingsInitializationFoldout()
			{
				MMUIToolkit.CreateAndBindPropertyField(_initializationModePropertyName, serializedObject,
					settingsInitializationFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_autoPlayOnStartPropertyName, serializedObject,
					settingsInitializationFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_autoPlayOnEnablePropertyName, serializedObject,
					settingsInitializationFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_autoInitializationPropertyName, serializedObject,
					settingsInitializationFoldout);
			}

			// direction foldout
			Foldout settingsDirectionFoldout = CreateSettingsSubFoldout(_directionSectionText, _settingsFoldout);
			settingsDirectionFoldout.value = false;
			settingsDirectionFoldout.viewDataKey = target.name + _settingsDirectionClassNameSuffix;
			settingsDirectionFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(249, 244, 205, 255));
			HandleFoldoutChange(settingsDirectionFoldout, BuildSettingsDirectionFoldout);

			void BuildSettingsDirectionFoldout()
			{
				MMUIToolkit.CreateAndBindPropertyField(_directionPropertyName, serializedObject,
					settingsDirectionFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_autoChangeDirectionOnEndPropertyName, serializedObject,
					settingsDirectionFoldout);
			}

			// intensity foldout
			Foldout settingsIntensityFoldout = CreateSettingsSubFoldout(_intensitySectionText, _settingsFoldout);
			settingsIntensityFoldout.value = false;
			settingsIntensityFoldout.viewDataKey = target.name + _settingsIntensityClassNameSuffix;
			settingsIntensityFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(250, 238, 179, 255));
			HandleFoldoutChange(settingsIntensityFoldout, BuildIntensitySettingsFoldout);

			void BuildIntensitySettingsFoldout()
			{
				MMUIToolkit.CreateAndBindPropertyField(_feedbacksIntensityPropertyName, serializedObject,
					settingsIntensityFoldout);
			}

			// timing foldout
			Foldout settingsTimingFoldout = CreateSettingsSubFoldout(_timingSectionText, _settingsFoldout);
			settingsTimingFoldout.value = false;
			settingsTimingFoldout.viewDataKey = target.name + _settingsTimingClassNameSuffix;
			settingsTimingFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(251, 226, 128, 255));
			HandleFoldoutChange(settingsTimingFoldout, BuildSettingsTimingFoldout);

			void BuildSettingsTimingFoldout()
			{
				MMUIToolkit.CreateAndBindPropertyField(_durationMultiplierPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_randomizeDurationPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_randomDurationMultiplierPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_displayFullDurationDetailsPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_cooldownDurationPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_initialDelayPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_chanceToPlayPropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_playerTimescaleModePropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_forceTimescaleModePropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_forcedTimescaleModePropertyName, serializedObject,
					settingsTimingFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_timescaleMultiplierPropertyName, serializedObject,
					settingsTimingFoldout);
			}

			// range foldout
			Foldout settingsRangeFoldout = CreateSettingsSubFoldout(_rangeSectionText, _settingsFoldout);
			settingsRangeFoldout.value = false;
			settingsRangeFoldout.viewDataKey = target.name + _settingsRangeClassNameSuffix;
			settingsRangeFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(253, 216, 88, 255));
			PropertyField onlyPlayIfWithinRangeField =
				new PropertyField(serializedObject.FindProperty(_onlyPlayIfWithinRangePropertyName));
			settingsRangeFoldout.Add(onlyPlayIfWithinRangeField);

			// range container
			VisualElement rangeContainer = new VisualElement();
			rangeContainer.Add(new PropertyField(serializedObject.FindProperty(_rangeCenterPropertyName)));
			rangeContainer.Add(new PropertyField(serializedObject.FindProperty(_rangeDistancePropertyName)));
			PropertyField useRangeFalloffField =
				new PropertyField(serializedObject.FindProperty(_useRangeFalloffPropertyName));
			rangeContainer.Add(useRangeFalloffField);
			settingsRangeFoldout.Add(rangeContainer);

			// range falloff container
			VisualElement rangeFalloffContainer = new VisualElement();
			rangeFalloffContainer.Add(new PropertyField(serializedObject.FindProperty(_rangeFalloffPropertyName)));
			rangeFalloffContainer.Add(new PropertyField(serializedObject.FindProperty(_remapRangeFalloffPropertyName)));
			rangeFalloffContainer.Add(new PropertyField(serializedObject.FindProperty(_ignoreRangeEventsPropertyName)));
			rangeContainer.Add(rangeFalloffContainer);
			onlyPlayIfWithinRangeField.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				rangeContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
			});
			useRangeFalloffField.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				rangeFalloffContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
			});

			// play settings foldout
			Foldout settingsPlaySettingsFoldout = CreateSettingsSubFoldout(_playSettingsSectionText, _settingsFoldout);
			settingsPlaySettingsFoldout.value = false;
			settingsPlaySettingsFoldout.viewDataKey = target.name + _settingsPlaySettingsClassNameSuffix;
			settingsPlaySettingsFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(254, 209, 57, 255));
			HandleFoldoutChange(settingsPlaySettingsFoldout, BuildPlaySettingsFoldout);

			void BuildPlaySettingsFoldout()
			{
				MMUIToolkit.CreateAndBindPropertyField(_canPlayPropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_canPlayWhileAlreadyPlayingPropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_performanceModePropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_stopFeedbacksOnDisablePropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_restoreInitialValuesOnDisablePropertyName, serializedObject, settingsPlaySettingsFoldout);
				
				MMUIToolkit.CreateAndBindPropertyField(_channelModePropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_channelPropertyName, serializedObject, settingsPlaySettingsFoldout);
				MMUIToolkit.CreateAndBindPropertyField(_mmChannelDefinitionPropertyName, serializedObject, settingsPlaySettingsFoldout);

				if (Application.isPlaying)
				{
					MMUIToolkit.CreateAndBindPropertyField(_playCountPropertyName, serializedObject,
						settingsPlaySettingsFoldout);
				}
			}

			// events foldout
			PropertyField eventsField = new PropertyField(serializedObject.FindProperty(_eventsSectionText));
			_settingsFoldout.Add(eventsField);
			eventsField.RegisterCallback<GeometryChangedEvent>(evt =>
			{
				Foldout eventsFieldFoldout = eventsField.Q<Foldout>();
				if (eventsFieldFoldout != null)
				{
					eventsFieldFoldout.AddToClassList(_foldoutClassName);
					eventsFieldFoldout.AddToClassList(_settingsFoldoutSubClassName);
					eventsFieldFoldout.style.borderLeftColor = new StyleColor(MMColors.CreateColor(255, 197, 8, 255));
				}

				eventsField.Q<Toggle>()?.AddToClassList(_foldoutToggleClassName);
				eventsField.Q<Toggle>()?.AddToClassList(_settingsFoldoutSubToggleClassName);
			});

			// automatic setup button
			_automaticShakerSetupButtonContainer = new VisualElement();
			_settingsFoldout.Add(_automaticShakerSetupButtonContainer);
			DrawAutomaticShakerSetupButton();

			// when a foldout gets opened/closed, we build the foldout contents if needed
			void HandleFoldoutChange(Foldout foldout, Action buildMethod)
			{
				foldout.schedule.Execute(() =>
				{
					if (foldout.value)
					{
						buildMethod();
					}
				}).ExecuteLater(1);

				EventCallback<ChangeEvent<bool>> callback = null;
				callback = evt =>
				{
					if (evt.newValue) // foldout opened
					{
						if (foldout.childCount == 0)
						{
							buildMethod();
							foldout.UnregisterValueChangedCallback(callback);
						}
					}
				};
				foldout.RegisterValueChangedCallback(callback);
			}

			// creates a new settings sub foldout
			Foldout CreateSettingsSubFoldout(string name, VisualElement parent)
			{
				Foldout newFoldout = new Foldout();
				newFoldout.text = name;
				newFoldout.AddToClassList(_foldoutClassName);
				newFoldout.AddToClassList(_settingsFoldoutSubClassName);
				newFoldout.viewDataKey = target.name + _settingsFoldoutClassNameSuffix + name;
				parent.Add(newFoldout);
				newFoldout.Q<Toggle>().AddToClassList(_foldoutToggleClassName);
				newFoldout.Q<Toggle>().AddToClassList(_settingsFoldoutSubToggleClassName);
				return newFoldout;
			}
		}

		protected virtual void DrawAutomaticShakerSetupButton()
		{
			_automaticShakerSetupButtonContainer.Clear();
			if (!Application.isPlaying && TargetMmfPlayer.HasAutomaticShakerSetup)
			{
				Button automaticShakerSetupButton = new Button(() => TargetMmfPlayer.AutomaticShakerSetup());
				automaticShakerSetupButton.text = _automaticShakerSetupText;
				automaticShakerSetupButton.AddToClassList(_automaticShakerSetupButtonClassName);
				_automaticShakerSetupButtonContainer.Add(automaticShakerSetupButton);
			}
		}

		protected virtual void DrawFeedbacksList(VisualElement root)
		{
			// draw feedbacks list container
			VisualElement feedbacksListContainer = new VisualElement();
			feedbacksListContainer.AddToClassList(_feedbacksListClassName);
			feedbacksListContainer.viewDataKey = target.name + _feedbacksListContainerClassNameSuffix;
			root.Add(feedbacksListContainer);

			// draw top label
			_feedbacksListLabel = new Label();
			_feedbacksListLabel.AddToClassList(_feedbacksListTitleClassName);
			UpdateFeedbacksListLabel();
			feedbacksListContainer.Add(_feedbacksListLabel);
			feedbacksListContainer.style.flexDirection = FlexDirection.Column;

			// empty state
			_emptyFeedbackListContainer = new VisualElement();
			DrawEmptyListState();
			feedbacksListContainer.Add(_emptyFeedbackListContainer);

			// draw feedback list
			AssembleFeedbacksList();
			feedbacksListContainer.Add(_feedbacksListView);
		}

		protected virtual void UpdateFeedbacksListLabel()
		{
			int count = 0;
			if ((TargetMmfPlayer != null) && (TargetMmfPlayer.FeedbacksList != null))
			{
				count = TargetMmfPlayer.FeedbacksList.Count;
			}

			_feedbacksListLabel.text = count + " " + _feedbacksSectionTitle;
		}

		protected virtual void DrawEmptyListState()
		{
			if (_emptyFeedbackListContainer == null)
			{
				return;
			}
			_emptyFeedbackListContainer.Clear();
			if ((TargetMmfPlayer.FeedbacksList == null) || (TargetMmfPlayer.FeedbacksList.Count == 0))
			{
				VisualElement emptyListImage = new VisualElement();
				emptyListImage.AddToClassList(_emptyListImageClassName);
				emptyListImage.style.backgroundImage = new StyleBackground(EmptyListImage);
				_emptyFeedbackListContainer.Add(emptyListImage);
				_emptyFeedbackListContainer.AddToClassList(_feedbacksListEmptyClassName);
				_emptyFeedbackListContainer.Add(new Label(_emptyFeedbackListMessage));
				_emptyFeedbackListContainer.style.display = DisplayStyle.Flex;
			}
			else
			{
				_emptyFeedbackListContainer.style.display = DisplayStyle.None;
			}
		}

		protected virtual void AssembleFeedbacksList()
		{
			_feedbacksListView = new ListView();
			BindListViewToData();
			_feedbacksListView.viewDataKey = target.name + _feedbacksListViewClassNameSuffix;
			_feedbacksListView.name = target.name + _feedbacksListViewClassNameSuffix;
			_feedbacksListView.fixedItemHeight = 25;
			_feedbacksListView.reorderable = true;
			_feedbacksListView.reorderMode = ListViewReorderMode.Animated;
			_feedbacksListView.selectionType = SelectionType.Single;
			_feedbacksListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
			_feedbacksListView.itemIndexChanged += (oldIndex, newIndex) => OnFeedbackListReorder();
			_feedbacksListView.RegisterCallback<PointerDownEvent>(evt => { evt.StopPropagation(); });

			ClearDictionaries();

			_feedbacksListView.makeItem = () => { return new Foldout(); };

			_feedbacksListView.bindItem = (element, index) =>
			{
				Type feedbackType = TargetMmfPlayer.FeedbacksList[index].GetType();
				
				// to prevent double bindings during list reorders, we return if we've already bound this feedback
				if (FeedbackHeaderContainersDictionary.ContainsKey(TargetMmfPlayer.FeedbacksList[index]))
				{
					return;
				}

				// feedback foldout
				Foldout foldout = (element as Foldout);
				foldout.AddToClassList(_feedbackFoldoutClassName);
				foldout.text = DetermineFeedbackLabel(index, feedbackType);
				foldout.Clear();
				foldout.value = TargetMmfPlayer.FeedbacksList[index].IsExpanded;
				foldout.viewDataKey = TargetMmfPlayer.name + "-" + TargetMmfPlayer.FeedbacksList[index].UniqueID;
				
				// help box 
				if (MMMenuHelp.HelpEnabled)
				{
					string helpText =
						FeedbackHelpAttribute.GetFeedbackHelpText(feedbackType);
					if (!string.IsNullOrEmpty(helpText))
					{
						VisualElement helpBox = new VisualElement();
						helpBox.AddToClassList(_feedbackHelpBoxClassName);
						Label helpLabel = new Label(helpText);
						helpLabel.AddToClassList(_feedbackHelpLabelClassName);
						helpBox.Add(helpLabel);
						helpBox.style.borderLeftColor = TargetMmfPlayer.FeedbacksList[index].FeedbackColor;
						foldout.Add(helpBox);
					}
				}
				
				// script edit button 
				PropertyField scriptEditContainer = new PropertyField();
				scriptEditContainer.AddToClassList(_feedbackEditScriptButtonContainerClassName);
				Label scriptEditLabel = new Label(_scriptEditLabelText); 
				scriptEditLabel.AddToClassList(_feedbackEditScriptButtonLabelClassName);
				scriptEditContainer.Add(scriptEditLabel);
				VisualElement scriptEditButtonBox = new VisualElement();
				scriptEditButtonBox.AddToClassList(_feedbackEditScriptButtonBoxClassName);
				scriptEditContainer.Add(scriptEditButtonBox);
				VisualElement scriptIcon = new VisualElement();
				scriptIcon.AddToClassList(_iconClassName);
				scriptIcon.style.backgroundImage = new StyleBackground(ScriptIcon);
				scriptEditButtonBox.Add(scriptIcon);
				Button scriptEditButton = new Button(() => EditScript(feedbackType));
				scriptEditButton.text = feedbackType.Name; 
				scriptEditButtonBox.Add(scriptEditButton);
				VisualElement scriptEditSelectorIcon = new VisualElement();
				scriptEditSelectorIcon.AddToClassList(_objectSelectorClassName); 
				scriptEditButtonBox.Add(scriptEditSelectorIcon);
				foldout.Add(scriptEditContainer);

				// progress line
				VisualElement feedbackProgressBarContainer = new VisualElement();
				feedbackProgressBarContainer.AddToClassList(_feedbackProgressLineContainerClassName);
				foldout.parent.Add(feedbackProgressBarContainer);
				VisualElement feedbackProgressBar = new VisualElement();
				feedbackProgressBar.AddToClassList(_feedbackProgressLineClassName);
				feedbackProgressBar.style.backgroundColor =
					TargetMmfPlayer.FeedbacksList[index].FeedbackColor.MMLighten(0.3f);
				feedbackProgressBarContainer.Add(feedbackProgressBar);

				// setup required
				VisualElement setupRequiredWarningBox = new VisualElement();
				setupRequiredWarningBox.AddToClassList(_feedbackSetupRequiredBoxClassName);
				setupRequiredWarningBox.focusable = false;
				Label setupRequiredLabel = new Label(TargetMmfPlayer.FeedbacksList[index].RequiresSetupText);
				setupRequiredLabel.AddToClassList(_feedbackSetupRequiredLabelClassName);
				setupRequiredWarningBox.Add(setupRequiredLabel);
				foldout.Add(setupRequiredWarningBox);

				// feedback inspector
				SerializedProperty feedbackProperty = _mmfeedbacksList.GetArrayElementAtIndex(index);
				MMFInspectorDrawData drawData = new MMFInspectorDrawData();
				drawData.Index = index;
				drawData.FeedbackFoldout = foldout;
				drawData.CurrentProperty = feedbackProperty;
				drawData.Feedback = TargetMmfPlayer.FeedbacksList[index];
				drawData.OnAnyValueChanged = OnAnyValueChanged;
				drawData.PlayerEditor = this;
				drawData.OnFeedbackFieldValueChanged = OnFeedbackFieldValueChanged;
				drawData.FeedbackGroupsDictionary = FeedbackGroupsDictionary;
				drawData.SetupRequiredIcon = SetupRequiredIcon;
				VisualElement feedbackInspectorContainer = new VisualElement();
				feedbackInspectorContainer.AddToClassList(_feedbackInspectorContainerClassName);

				// we wait for one frame before checking if the foldout is open, because Unity can't let you know if a foldout is open instantly after drawing it for the first time, but it can after one frame
				foldout.schedule.Execute(() => DrawInspectorIfFoldoutIsOpen()).ExecuteLater(1);
				// believe it or not we do it again after 100ms, because Unity can't let you know if a foldout is open or not instantly after a reorder in a list view.
				// you'd think you could get a callback when the object is ready, think again
				foldout.schedule.Execute(() => DrawInspectorIfFoldoutIsOpen()).ExecuteLater(100);
				// we listen for foldout events, if the foldout opens, we draw its contents if not done already
				foldout.RegisterValueChangedCallback(evt => DrawInspectorIfFoldoutIsOpen());

				void DrawInspectorIfFoldoutIsOpen()
				{
					if (foldout.value)
					{
						if (feedbackInspectorContainer.childCount == 0)
						{
							feedbackInspectorContainer.Add(MMF_FeedbackPropertyDrawerUITK.DrawInspector(drawData));
						}
					}
				}

				foldout.Add(feedbackInspectorContainer);

				// bind label and infinite loop property to header refresh
				PropertyField labelField = feedbackInspectorContainer.Q<PropertyField>(_labelPropertyName);
				labelField?.RegisterCallback<ChangeEvent<string>>(evt =>
				{
					foldout.text = DetermineFeedbackLabel(index, feedbackType);
				});

				PropertyField infiniteLoopField =
					feedbackInspectorContainer.Q<PropertyField>(_infiniteLoopPropertyName);
				infiniteLoopField?.RegisterCallback<ChangeEvent<bool>>(evt =>
				{
					foldout.text = DetermineFeedbackLabel(index, feedbackType);
				});

				// setting the left bar border color
				VisualElement feedbackReorderableItem = foldout.parent.parent;
				feedbackReorderableItem.AddToClassList(_feedbackReorderableItemClassName);

				// feedback left border color
				VisualElement feedbackLeftBorder = new VisualElement();
				feedbackLeftBorder.AddToClassList(_feedbackLeftBorderClassName);
				feedbackReorderableItem.Insert(0, feedbackLeftBorder);
				feedbackLeftBorder.style.backgroundColor = TargetMmfPlayer.FeedbacksList[index].FeedbackColor;

				// handling background color
				if (TargetMmfPlayer.FeedbacksList[index].DisplayFullHeaderColor)
				{
					VisualElement feedbackBackgroundColor = new VisualElement();
					feedbackBackgroundColor.AddToClassList(_feedbackBackgroundColorClassName);
					feedbackBackgroundColor.style.backgroundColor = TargetMmfPlayer.FeedbacksList[index].DisplayColor;
					feedbackReorderableItem.Insert(0, feedbackBackgroundColor);
				}

				// toggle label, add class 
				Toggle toggle = foldout.Q<Toggle>();
				Label toggleLabel = foldout.Q<Label>();
				toggleLabel.focusable = false;
				toggleLabel.AddToClassList(_feedbackFoldoutLabelClassName);

				// active checkbox
				Toggle activeCheckbox = new Toggle();
				activeCheckbox.AddToClassList(_feedbackActiveCheckboxClassName);
				SerializedProperty activeProperty =
					_mmfeedbacksList.GetArrayElementAtIndex(index).FindPropertyRelative("Active");
				activeCheckbox.BindProperty(activeProperty);
				foldout.parent.Insert(1, activeCheckbox);
				feedbackInspectorContainer.SetEnabled(TargetMmfPlayer.FeedbacksList[index].Active);

				// active checkbox changes listener
				foldout.schedule.Execute(() =>
				{
					activeCheckbox.RegisterValueChangedCallback(evt =>
					{
						feedbackInspectorContainer.SetEnabled(evt.newValue);
					});
				}).ExecuteLater(100);

				// feedback header container (target, required setup, direction, context menu...)
				VisualElement feedbackHeaderContainer = new VisualElement();
				feedbackHeaderContainer.AddToClassList(_feedbackHeaderContainerClassName);
				feedbackHeaderContainer.pickingMode = PickingMode.Ignore;
				foldout.parent.Insert(1, feedbackHeaderContainer);

				// header container data
				FeedbackHeaderContainersData feedbackHeaderContainersData = new FeedbackHeaderContainersData();
				feedbackHeaderContainersData.Index = index;
				feedbackHeaderContainersData.Feedback = TargetMmfPlayer.FeedbacksList[index];
				feedbackHeaderContainersData.HeaderContainer = feedbackHeaderContainer;
				feedbackHeaderContainersData.SetupRequiredWarningBox = setupRequiredWarningBox;
				feedbackHeaderContainersData.ProgressBar = feedbackProgressBar;
				feedbackHeaderContainersData.FeedbackType = feedbackType;
				FeedbackHeaderContainersDictionary.Add(TargetMmfPlayer.FeedbacksList[index],
					feedbackHeaderContainersData);
				DrawFeedbackHeaderContainer(feedbackHeaderContainersData);

				// context menu - right click
				ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator((evt) =>
				{
					FillContextualMenu(evt.menu, index, feedbackType);
				});
				toggle.AddManipulator(contextualMenuManipulator);

				// feedback control buttons
				VisualElement feedbackControlButtons = new VisualElement();
				feedbackControlButtons.AddToClassList(_feedbackControlButtonsClassName);
				foldout.Add(feedbackControlButtons);
				Button playButton = new Button(() => PlayFeedback(index));
				playButton.text = _playText;
				feedbackControlButtons.Add(playButton);
				Button stopButton = new Button(() => StopFeedback(index));
				stopButton.text = _stopText;
				feedbackControlButtons.Add(stopButton);
				if (!Application.isPlaying)
				{
					playButton.SetEnabled(false);
					stopButton.SetEnabled(false);
				}

				feedbackControlButtons.RegisterCallback<PointerDownEvent>(evt => { evt.StopPropagation(); });
			};
		}

		protected virtual void EditScript(Type feedbackType)
		{
			string[] guids = AssetDatabase.FindAssets(feedbackType.Name + " t:script");
			if (guids.Length == 0)
			{
				Debug.LogError("Script not found for type: " + feedbackType.Name); 
				return;
			}
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
				if (string.Equals(fileName, feedbackType.Name, System.StringComparison.Ordinal))
				{
					MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
					AssetDatabase.OpenAsset(script);
				}
			}
		}

		protected virtual void ClearDictionaries()
		{
			FeedbackHeaderContainersDictionary?.Clear();
			FeedbackGroupsDictionary?.Clear();
		}

		public virtual void RedrawAllFeedbackHeaders()
		{
			foreach (var feedback in FeedbackHeaderContainersDictionary.Keys)
			{
				FeedbackHeaderContainersDictionary[feedback].HeaderContainer =
					DrawFeedbackHeaderContainer(FeedbackHeaderContainersDictionary[feedback]);
			}
		}

		protected virtual VisualElement DrawFeedbackHeaderContainer(
			FeedbackHeaderContainersData feedbackHeaderContainersData)
		{
			int index = feedbackHeaderContainersData.Index;

			feedbackHeaderContainersData.HeaderContainer.Clear();

			// setup required
			feedbackHeaderContainersData.SetupRequiredWarningBox.style.display =
				(TargetMmfPlayer.FeedbacksList[index].RequiresSetup) ? DisplayStyle.Flex : DisplayStyle.None;

			// direction arrows 
			VisualElement directionIcon = new VisualElement();
			directionIcon.AddToClassList(_iconClassName);
			directionIcon.AddToClassList(_feedbackDirectionIconClassName);
			directionIcon.style.backgroundImage = DetermineFeedbackDirectionIcon(index);
			feedbackHeaderContainersData.HeaderContainer.Add(directionIcon);

			// context menu button 
			var toolbar = new ToolbarMenu();
			toolbar.AddToClassList(_feedbackContextualMenuButtonClassName);
			FillContextualMenu(toolbar.menu, index, feedbackHeaderContainersData.FeedbackType);
			toolbar.RegisterCallback<PointerDownEvent>(evt => { FillContextualMenu(toolbar.menu, index, feedbackHeaderContainersData.FeedbackType); },
				TrickleDown.TrickleDown);
			feedbackHeaderContainersData.HeaderContainer.Add(toolbar);

			// context menu button icon
			VisualElement contextMenuIcon = new VisualElement();
			contextMenuIcon.AddToClassList(_iconClassName);
			contextMenuIcon.AddToClassList(_contextMenuIconClassName);
			contextMenuIcon.style.backgroundImage = new StyleBackground(ContextlMenuIcon);
			toolbar.Clear();
			toolbar.Add(contextMenuIcon);

			// target 
			Label targetLabel = new Label(TargetMmfPlayer.FeedbacksList[index].RequiredTarget);
			targetLabel.AddToClassList(_feedbackRequiredTargetLabelClassName);
			feedbackHeaderContainersData.HeaderContainer.Add(targetLabel);
			targetLabel.style.display =
				TargetMmfPlayer.DisplayFullDurationDetails ? DisplayStyle.None : DisplayStyle.Flex;

			// timing info 
			string timingInfo = "";
			bool displayTotal = false;
			if (TargetMmfPlayer.DisplayFullDurationDetails)
			{
				if (TargetMmfPlayer.FeedbacksList[index].Timing.InitialDelay != 0)
				{
					timingInfo +=
						TargetMmfPlayer.ApplyTimeMultiplier(TargetMmfPlayer.FeedbacksList[index].Timing.InitialDelay)
							.ToString() + "s + ";
					displayTotal = true;
				}

				timingInfo += TargetMmfPlayer.FeedbacksList[index].FeedbackDuration.ToString("F2") + "s";

				if (TargetMmfPlayer.FeedbacksList[index].Timing.NumberOfRepeats != 0)
				{
					float delayBetweenRepeats =
						TargetMmfPlayer.ApplyTimeMultiplier(TargetMmfPlayer.FeedbacksList[index].Timing
							.DelayBetweenRepeats);

					timingInfo += " + " + TargetMmfPlayer.FeedbacksList[index].Timing.NumberOfRepeats.ToString() +
					              " x ";
					
					timingInfo += "(" + TargetMmfPlayer.FeedbacksList[index].FeedbackDuration.ToString() +
					              "s + ";
					
					timingInfo +=
						TargetMmfPlayer.ApplyTimeMultiplier(TargetMmfPlayer.FeedbacksList[index].Timing
							.DelayBetweenRepeats) + "s)";
					displayTotal = true;
				}

				if (displayTotal)
				{
					timingInfo += " = " + TargetMmfPlayer.FeedbacksList[index].TotalDuration.ToString("F2") + "s";
				}
			}
			else
			{
				timingInfo = TargetMmfPlayer.FeedbacksList[index].TotalDuration.ToString("F2") + "s";
			}

			Label timingLabel = new Label(timingInfo);
			timingLabel.AddToClassList(_feedbackTimingLabelClassName);
			feedbackHeaderContainersData.HeaderContainer.Add(timingLabel);

			// setup required icon
			if (TargetMmfPlayer.FeedbacksList[index].RequiresSetup)
			{
				VisualElement setupRequiredIcon = new VisualElement();
				setupRequiredIcon.AddToClassList(_iconClassName);
				setupRequiredIcon.AddToClassList(_setupRequiredIconClassName);
				setupRequiredIcon.style.backgroundImage = new StyleBackground(SetupRequiredIcon);
				feedbackHeaderContainersData.HeaderContainer.Add(setupRequiredIcon);
			}

			return feedbackHeaderContainersData.HeaderContainer;
		}

		protected virtual StyleBackground DetermineFeedbackDirectionIcon(int index)
		{
			if ((TargetMmfPlayer.FeedbacksList[index].Timing.MMFeedbacksDirectionCondition ==
			     MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards)
			    || (TargetMmfPlayer.FeedbacksList[index].Timing.MMFeedbacksDirectionCondition ==
			        MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards))
			{
				return (TargetMmfPlayer.FeedbacksList[index].Timing.MMFeedbacksDirectionCondition ==
				        MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)
					? new StyleBackground(BottomToTopIcon)
					: new StyleBackground(TopToBottomIcon);
			}

			return null;
		}

		public virtual string DetermineFeedbackLabel(int index, Type feedbackType)
		{
			_feedbackLabel = TargetMmfPlayer.FeedbacksList[index].GetLabel();
			if (TargetMmfPlayer.FeedbacksList[index].Label != TargetMmfPlayer.FeedbacksList[index].OriginalLabel)
			{
				if (TargetMmfPlayer.FeedbacksList[index].OriginalLabel == "")
				{
					TargetMmfPlayer.FeedbacksList[index].OriginalLabel =
						FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);
				}

				if (TargetMmfPlayer.FeedbacksList[index].OriginalLabel != "")
				{
					_feedbackLabel += " <color=" + _originalLabelColor + ">(" +
					                  TargetMmfPlayer.FeedbacksList[index].OriginalLabel + ")</color>";
				}
			}

			if ((TargetMmfPlayer.FeedbacksList[index].LooperPause == true) && (Application.isPlaying))
			{
				if ((TargetMmfPlayer.FeedbacksList[index] as MMF_Looper).InfiniteLoop)
				{
					_feedbackLabel = _feedbackLabel + _infiniteLoopText;
				}
				else
				{
					_feedbackLabel = _feedbackLabel + "[" +
					                 (TargetMmfPlayer.FeedbacksList[index] as MMF_Looper).NumberOfLoopsLeft +
					                 " loops left] ";
				}
			}

			return _feedbackLabel;
		}

		protected virtual void FillContextualMenu(DropdownMenu menu, int index, Type feedbackType)
		{
			menu.ClearItems();
			DropdownMenuAction.Status playStatus = Application.isPlaying
				? DropdownMenuAction.Status.Normal
				: DropdownMenuAction.Status.Disabled;
			menu.AppendAction(_playText, action => PlayFeedback(index), playStatus);
			menu.AppendSeparator();
			menu.AppendAction(_removeText, action => RemoveFeedback(index), DropdownMenuAction.Status.Normal);
			menu.AppendAction(_resetText, action => ResetContextMenuFeedback(index), DropdownMenuAction.Status.Normal);
			menu.AppendSeparator();
			menu.AppendAction(_duplicateText, action => DuplicateFeedback(index), DropdownMenuAction.Status.Normal);
			menu.AppendAction(_copyText, action => CopyFeedback(index), DropdownMenuAction.Status.Normal);
			DropdownMenuAction.Status copyStatus = MMF_PlayerCopy.HasCopy()
				? DropdownMenuAction.Status.Normal
				: DropdownMenuAction.Status.Disabled;
			menu.AppendAction(_pasteText, action => PasteAsNew(), copyStatus);
			menu.AppendSeparator();
			menu.AppendAction(_editScriptText, action => EditScript(feedbackType));
		}

		protected virtual void RedrawFeedbacksList() 
		{
			if (serializedObject == null)
			{
				return;
			}
			serializedObject.Update();
			ClearDictionaries();
			DrawEmptyListState();
			BindListViewToData();
			DrawAutomaticShakerSetupButton(); 
			_feedbacksListView.Rebuild();
			UpdateFeedbacksListLabel();
		}

		protected virtual void DrawBottomBar(VisualElement root)
		{
			_bottomBar = new VisualElement();
			_bottomBar.AddToClassList(_bottomBarClassName);
			RedrawBottomBar();
			root.Add(_bottomBar);
		}

		protected virtual void RedrawBottomBar()
		{
			_bottomBar.Clear();
			_bottomBar.Add(DrawBottomBarContents());
		}

		protected virtual VisualElement DrawBottomBarContents()
		{
			VisualElement bottomBarContents = new VisualElement();
			bottomBarContents.style.flexDirection = FlexDirection.Column;
			
			// setup bar row and results row
			VisualElement controlsRow = new VisualElement();
			VisualElement searchResultsRow = new VisualElement();
			controlsRow.style.flexDirection = FlexDirection.Row;
			searchResultsRow.AddToClassList(_searchResultsRowClassName);
			
			// add new feedback popup
			PopupField<string> addNewFeedbackPopupField =
				new PopupField<string>(_addNewFeedbackPopupFieldLabel, _typeDisplays, 0);
			addNewFeedbackPopupField.Q<Label>().style.display = DisplayStyle.None;
			addNewFeedbackPopupField.style.flexGrow = 1;
			controlsRow.Add(addNewFeedbackPopupField);
			addNewFeedbackPopupField.RegisterValueChangedCallback(evt =>
			{
				int newItem = _typeDisplays.IndexOf(evt.newValue);
				if (newItem >= 0)
				{
					AddFeedback(newItem, addNewFeedbackPopupField);
				}
			});
			
			// Search Field
			BuildSearchField(controlsRow, searchResultsRow);

			// copy all button
			if (!MMF_PlayerCopy.HasMultipleCopies())
			{
				Button copyAllButton = new Button(CopyAll) { text = _copyAllText };
				controlsRow.Add(copyAllButton);
			}

			// paste as new button
			if (MMF_PlayerCopy.HasCopy())
			{
				Button pasteAsNewButton = new Button(PasteAsNew) { text = _pasteAsNewText };
				controlsRow.Add(pasteAsNewButton);
			}

			// replace all button
			if (MMF_PlayerCopy.HasMultipleCopies())
			{
				Button replaceAllButton = new Button(ReplaceAll) { text = _replaceAllText };
				Button pasteAllAsNewButton = new Button(PasteAllAsNew) { text = _pasteAllAsNewText };

				controlsRow.Add(replaceAllButton);
				controlsRow.Add(pasteAllAsNewButton);
			}

			bottomBarContents.Add(controlsRow);
			bottomBarContents.Add(searchResultsRow);
			
			return bottomBarContents;
		}

		protected virtual void AddFeedback(int feedbackIndex, PopupField<string> addNewFeedbackPopupField)
		{
			serializedObject.Update();
			Undo.RecordObject(target, _addUndoText);
			int newFeedbackIndex = feedbackIndex - 1;
			AddFeedback(_typesAndNames[newFeedbackIndex].FeedbackType);
			SavePlayerChanges();
			if (addNewFeedbackPopupField != null)
			{
				addNewFeedbackPopupField.SetValueWithoutNotify(_typeDisplays[0]);
			}
			serializedObject.Update();
			CacheFeedbacksListProperty();
			BindListViewToData();
			RedrawFeedbacksList();
		}

		protected virtual void BuildSearchField(VisualElement anchor, VisualElement resultsVisualElement)
		{
			TextField searchField = new TextField("");
			searchField.AddToClassList(_searchFieldClassName);
			anchor.Add(searchField);

			// callbacks
			searchField.RegisterCallback<FocusInEvent>(OnSearchFieldFocusIn);
			searchField.RegisterCallback<FocusOutEvent>(OnSearchFieldFocusOut);
			searchField.RegisterValueChangedCallback<string>(OnSearchFieldValueChanged);
			SetSearchFieldToPlaceholderMode();
			
			searchField.RegisterCallback<KeyDownEvent>(evt =>
			{
				if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
				{
					int selectionIndex = (evt.keyCode == KeyCode.DownArrow) ? -1 : 0;
					Navigate(selectionIndex, evt.keyCode);
				}
			}, TrickleDown.TrickleDown);
			
			// clear button
			Button clearSearchFieldButton = new Button(ClearSearchField);
			clearSearchFieldButton.AddToClassList(_iconClassName);
			clearSearchFieldButton.style.backgroundImage = new StyleBackground(SearchFieldCloseIcon);
			clearSearchFieldButton.style.backgroundColor = _transparentColor;
			searchField.Q("unity-text-input").Add(clearSearchFieldButton);
			
			void Navigate(int currentIndex, KeyCode directionKey)
			{
				if (directionKey == KeyCode.Escape)
				{
					ClearSearchField();
					return;
				}
				
				if (directionKey != KeyCode.UpArrow && directionKey != KeyCode.DownArrow)
				{
					return;
				}

				if (resultsVisualElement.childCount == 0)
				{
					return;
				}

				int direction = (directionKey == KeyCode.DownArrow) ? 1 : -1;
				
				int newIndex = currentIndex + direction;
				if (newIndex < 0)
				{
					newIndex = resultsVisualElement.childCount - 1;
				}
				else if (newIndex >= resultsVisualElement.childCount)
				{
					newIndex = 0;
				}
				if (resultsVisualElement.ElementAt(newIndex) != null)
				{
					resultsVisualElement.ElementAt(newIndex).Focus();
				}
			}

			void ClearSearchField()
			{
				anchor.Focus();
				SetSearchFieldToPlaceholderMode();
			}

			void OnSearchFieldFocusIn(FocusInEvent evt)
			{
				if (searchField.value == _searchFeedbackPlaceholderText)
				{
					SetSearchFieldToNonPlaceholderMode();
				}
			}

			void OnSearchFieldFocusOut(FocusOutEvent evt)
			{
				bool buttonClicked = false;
				Focusable focusedElement = evt.relatedTarget;
				if (focusedElement is VisualElement && (focusedElement as VisualElement).ClassListContains(_searchAddFeedbackButtonClassName))
				{
					buttonClicked = true;
				}
				
				if (string.IsNullOrEmpty(searchField.value))
				{
					SetSearchFieldToPlaceholderMode();
				}

				if (!buttonClicked)
				{
					SetSearchFieldToPlaceholderMode();
				}
			}

			void SetSearchFieldToPlaceholderMode()
			{
				searchField.value = _searchFeedbackPlaceholderText;
				searchField.AddToClassList(_searchFieldPlaceholderClassName);
			}
			void SetSearchFieldToNonPlaceholderMode()
			{
				searchField.value = "";
				searchField.RemoveFromClassList(_searchFieldPlaceholderClassName);
			}

			void OnSearchFieldValueChanged(ChangeEvent<string> evt)
			{
				string searchedText = evt.newValue.ToLower();

				resultsVisualElement.Clear();
				
				if (searchedText.Length == 0)
				{
					return;
				}
				
				List<string> filteredFeedbackNames = new List<string>(_typeNamesDisplays);
				filteredFeedbackNames.Clear();
				filteredFeedbackNames.AddRange(_typeNamesDisplays.Where(option => option.ToLower().Contains(searchedText)));

				foreach (string feedbackName in filteredFeedbackNames)
				{
					Button addFeedbackButton = new Button(() => AddFeedbackByName(feedbackName, searchField)) { text = feedbackName };
					addFeedbackButton.AddToClassList(_searchAddFeedbackButtonClassName);
					addFeedbackButton.RegisterCallback<KeyDownEvent>(keyEvt =>
					{
						Navigate(resultsVisualElement.IndexOf(addFeedbackButton), keyEvt.keyCode);
					}, TrickleDown.TrickleDown);
					resultsVisualElement.Add(addFeedbackButton);
				}
			}
		}
		
		protected virtual void AddFeedbackByName(string name, TextField searchField)
		{
			for (int i=0; i<_typeNamesDisplays.Count; i++)
			{
				if (_typeNamesDisplays[i] == name)
				{
					AddFeedback(i+1, null);
					searchField.Focus();
					return;
				}
			}
		}
		
		protected virtual void BindListViewToData()
		{
			_feedbacksListView.itemsSource = TargetMmfPlayer.FeedbacksList;
		}

		protected virtual void DrawDebugControls(VisualElement root)
		{
			VisualElement controlsContainer = new VisualElement();
			controlsContainer.AddToClassList(_controlsClassName);
			root.Add(controlsContainer);

			// first row
			VisualElement firstRow = new VisualElement();
			firstRow.style.flexDirection = FlexDirection.Row;

			Button initializeButton = new Button(() => TargetMmfPlayer.Initialization()) { text = _initializeText };
			Button playButton = new Button(() => TargetMmfPlayer.PlayFeedbacks()) { text = _playText };
			Button pauseButton = new Button(() => TargetMmfPlayer.PauseFeedbacks()) { text = _pauseText };
			Button stopButton = new Button(() => TargetMmfPlayer.StopFeedbacks()) { text = _stopText };
			Button resetButton = new Button(() => TargetMmfPlayer.ResetFeedbacks()) { text = _resetText };

			playButton.style.backgroundColor = new StyleColor(new Color32(100, 123, 37, 255));

			firstRow.Add(initializeButton);
			firstRow.Add(playButton);
			if (Application.isPlaying && TargetMmfPlayer.ContainsLoop)
			{
				firstRow.Add(pauseButton);
			}

			firstRow.Add(stopButton);
			firstRow.Add(resetButton);

			// second row
			VisualElement secondRow = new VisualElement();
			secondRow.style.flexDirection = FlexDirection.Row;

			Button skipButton = new Button(() => TargetMmfPlayer.SkipToTheEnd()) { text = _skipText };
			Button restoreButton = new Button(() => TargetMmfPlayer.RestoreInitialValues()) { text = _restoreText };
			Button changeDirectionButton = new Button(() => TargetMmfPlayer.ChangeDirection())
				{ text = _changeDirectionText };

			secondRow.Add(skipButton);
			secondRow.Add(restoreButton);
			secondRow.Add(changeDirectionButton);

			controlsContainer.Add(firstRow);
			controlsContainer.Add(secondRow);

			// disable buttons if not in play mode
			if (!Application.isPlaying)
			{
				initializeButton.SetEnabled(false);
				playButton.SetEnabled(false);
				stopButton.SetEnabled(false);
				resetButton.SetEnabled(false);
				skipButton.SetEnabled(false);
				restoreButton.SetEnabled(false);
			}

			// keep playmode changes button
			Button keepPlaymodeChangesButton = new Button() { text = _keepPlaymodeChangesText };
			controlsContainer.Add(keepPlaymodeChangesButton);
			if (TargetMmfPlayer.KeepPlayModeChanges)
			{
				keepPlaymodeChangesButton.AddToClassList(_playModeButtonActiveClassName);
			}

			keepPlaymodeChangesButton.RegisterCallback<ClickEvent>(evt =>
			{
				TargetMmfPlayer.KeepPlayModeChanges = !TargetMmfPlayer.KeepPlayModeChanges;
				if (TargetMmfPlayer.KeepPlayModeChanges)
				{
					keepPlaymodeChangesButton.AddToClassList(_playModeButtonActiveClassName);
				}
				else
				{
					keepPlaymodeChangesButton.RemoveFromClassList(_playModeButtonActiveClassName);
				}

				EditorUtility.SetDirty(TargetMmfPlayer);
			});

			// script driven pause blinking label
			VisualElement scriptDrivenPauseContainer = new VisualElement();
			scriptDrivenPauseContainer.style.display = DisplayStyle.None;
			controlsContainer.Add(scriptDrivenPauseContainer);

			Label scriptDrivenPauseLabel = new Label();
			scriptDrivenPauseLabel.AddToClassList(_scriptDrivenPauseLabelClassName);
			scriptDrivenPauseContainer.Add(scriptDrivenPauseLabel);
			scriptDrivenPauseLabel.text = _scriptDrivenInProgressText;
			scriptDrivenPauseLabel.RegisterCallback<TransitionEndEvent>(evt =>
				scriptDrivenPauseLabel.ToggleInClassList(_scriptDrivenPauseLabelBlinkClassName));

			// resume button
			Button resumeButton = new Button(() => TargetMmfPlayer.ResumeFeedbacks()) { text = _resumeText };
			scriptDrivenPauseContainer.Add(resumeButton);
			DrawScriptDrivenPauseContainer();

			scriptDrivenPauseContainer.TrackPropertyValue(_inScriptDrivenPause, _ =>
			{
				DrawScriptDrivenPauseContainer();
			});

			void DrawScriptDrivenPauseContainer()
			{
				if (_inScriptDrivenPause.boolValue)
				{
					if (_blinkingTask == null)
					{
						_blinkingTask = root.schedule.Execute(() =>
								scriptDrivenPauseLabel.ToggleInClassList(_scriptDrivenPauseLabelBlinkClassName))
							.StartingIn(100);
					}
					else
					{
						_blinkingTask.Resume();
					}
				}
				else
				{
					if (_blinkingTask != null)
					{
						_blinkingTask.Pause();
					}
				}

				scriptDrivenPauseContainer.style.display =
					_inScriptDrivenPause.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		#region FeedbacksControls

		/// <summary>
		/// Add a feedback to the list
		/// </summary>
		protected virtual MMF_Feedback AddFeedback(System.Type type)
		{
			return (target as MMF_Player).AddFeedback(type);
		}

		/// <summary>
		/// Remove the selected feedback
		/// </summary>
		protected virtual void RemoveFeedback(int id)
		{
			Undo.RecordObject(target, _removeUndoText);
			(target as MMF_Player).RemoveFeedback(id);
			SavePlayerChanges();
			RedrawFeedbacksList();
			SavePlayerChanges();
		}

		protected virtual void ResetContextMenuFeedback(int id)
		{
			Undo.RecordObject(target, _resetUndoText);
			Type feedbackType = (target as MMF_Player).FeedbacksList[id].GetType();
			MMF_Feedback newFeedback = (target as MMF_Player).AddFeedback(feedbackType, false);
			(target as MMF_Player).FeedbacksList[id] = newFeedback;
			serializedObject.ApplyModifiedProperties();
			RedrawFeedbacksList();
			SavePlayerChanges();
		}

		protected virtual void SavePlayerChanges()
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(TargetMmfPlayer);
			PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMmfPlayer);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void InitializeFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Initialization(TargetMmfPlayer, id);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void PlayFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Play(TargetMmfPlayer.transform.position, TargetMmfPlayer.FeedbacksIntensity);
		}

		/// <summary>
		/// Play the selected feedback
		/// </summary>
		protected virtual void StopFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.Stop(TargetMmfPlayer.transform.position);
		}

		/// <summary>
		/// Resets the selected feedback
		/// </summary>
		/// <param name="id"></param>
		protected virtual void ResetFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];
			feedback.ResetFeedback();
		}

		#endregion

		#region FeedbacksCopy

		/// <summary>
		/// Copy the selected feedback
		/// </summary>
		protected virtual void CopyFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];

			MMF_PlayerCopy.Copy(feedback);
		}

		/// <summary>
		/// Copies and instantly pastes the selected feedback
		/// </summary>
		protected virtual void DuplicateFeedback(int id)
		{
			MMF_Feedback feedback = TargetMmfPlayer.FeedbacksList[id];

			MMF_PlayerCopy.Copy(feedback);
			PasteAsNew();
		}

		/// <summary>
		/// Asks for a full copy of the source
		/// </summary>
		protected virtual void CopyAll()
		{
			MMF_PlayerCopy.CopyAll(target as MMF_Player);
			RedrawBottomBar();
		}

		/// <summary>
		/// Creates a new feedback and applies the previoulsy copied feedback values
		/// </summary>
		protected virtual void PasteAsNew()
		{
			serializedObject.Update();
			Undo.RecordObject(target, _pasteUndoText);
			MMF_PlayerCopy.PasteAll(this);
			SavePlayerChanges();
			RedrawBottomBar();
			RedrawFeedbacksList();
		}

		/// <summary>
		/// Asks for a paste of all feedbacks in the source
		/// </summary>
		protected virtual void PasteAllAsNew()
		{
			serializedObject.Update();
			Undo.RecordObject(target, _pasteAllAsNewText);
			MMF_PlayerCopy.PasteAll(this);
			SavePlayerChanges();
			RedrawBottomBar();
			RedrawFeedbacksList();
		}

		protected virtual void ReplaceAll()
		{
			serializedObject.Update();
			Undo.RecordObject(target, _replaceAllUndoText);
			TargetMmfPlayer.FeedbacksList.Clear();
			MMF_PlayerCopy.PasteAll(this);
			SavePlayerChanges();
			RedrawBottomBar();
			RedrawFeedbacksList();
		}

		#endregion
	}
}
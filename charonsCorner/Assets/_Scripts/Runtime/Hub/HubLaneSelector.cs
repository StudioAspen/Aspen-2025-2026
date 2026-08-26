using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    public class HubLaneSelector : MonoBehaviour
    {
        [field: SerializeField] public List<LevelDataSO> LaneData { get; private set; } = new List<LevelDataSO>();
        [ShowInInspector, ReadOnly] public int CurrentLaneIndex { get; private set; }

        [Header("Navigation Repeat Settings")]
        [SerializeField] private float _initialDelay = 0.4f;
        [SerializeField] private float _repeatDelay = 0.1f;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player _spinRight;
        [SerializeField] private MMF_Player _spinLeft;
        [SerializeField] private LevelSelectUI _levelSelectUI;

        [Header("Skybox Rotation")]
        [SerializeField] private SkyboxRotator _skyboxRotator;
        [SerializeField] private Material _baseSkybox;

        [Header("Input Prompt")]
        [SerializeField] private InputInteraction _inputInteraction;

        private InputManager _input;
        private InputAction _moveAction;
        private Coroutine _moveCoroutine;

        public UnityEvent OnEnter = new();
        public UnityEvent OnLeave = new();
        public UnityEvent<int> OnLaneSelected = new();
        public UnityEvent<LevelDataSO> OnLaneInteracted = new();
        public UnityEvent<int> OnLaneInteractedIndex = new();

        private void Awake()
        {
            _input = InputManager.Instance;
            _moveAction = _input.InputActions.Player.Move;
        }

        private void OnEnable()
        {
            _moveAction.started += OnMoveStarted;
            _moveAction.canceled += OnMoveCanceled;

            if (_input)
            {
                _input.Exit += OnExit;
                _input.Interact += OnInteract;
            }

            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            
            // Find the last unlocked lane
            int lastUnlockedIndex = 0;
            for (int i = LaneData.Count - 1; i >= 0; i--)
            {
                if (LaneData[i].ChapterInWhichUnlocked <= currentChapterIndex)
                {
                    lastUnlockedIndex = i;
                    break;
                }
            }
            CurrentLaneIndex = lastUnlockedIndex;

            if (_spinRight != null)
                _spinRight.PlayFeedbacks();

            if (_skyboxRotator != null)
                _skyboxRotator.Rotate(1f);

            if (_inputInteraction != null)
                _inputInteraction.Appear();

            UpdateArrowStates();
            
            OnEnter.Invoke();
            OnLaneSelected.Invoke(CurrentLaneIndex);
        }

        private void OnDisable()
        {
            _moveAction.started -= OnMoveStarted;
            _moveAction.canceled -= OnMoveCanceled;

            if (_input)
            {
                _input.Exit -= OnExit;
                _input.Interact -= OnInteract;
            }

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            StopMoveCoroutine();
        }

        private void OnMoveStarted(InputAction.CallbackContext ctx)
        {
            Vector2 direction = ctx.ReadValue<Vector2>();

            StopMoveCoroutine();
            _moveCoroutine = StartCoroutine(MoveRoutine(direction));
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            StopMoveCoroutine();
        }

        private IEnumerator MoveRoutine(Vector2 direction)
        {
            FireMove(direction);
            yield return new WaitForSeconds(_initialDelay);

            while (true)
            {
                FireMove(direction);
                yield return new WaitForSeconds(_repeatDelay);
            }
        }

        private void FireMove(Vector2 direction)
        {
            if (direction.x < 0)
                SelectPreviousLane();
            else if (direction.x > 0)
                SelectNextLane();
        }

        private void StopMoveCoroutine()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        private void OnExit()
        {
            if (_baseSkybox != null)
            {
                RenderSettings.skybox = _baseSkybox;
            }
            
            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            if (_levelSelectUI != null)
                _levelSelectUI.PlayScaleDownFeedback();
            
            OnLeave.Invoke();
        }

        private void OnInteract()
        {
            LevelDataSO selectedLaneData = LaneData[CurrentLaneIndex];

            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            
            // if ChapterInWhichUnlocked is greater than the current chapter index, it will not be selectable at all
            // (Selection logic should already prevent this, but added as a safety check)
            if (selectedLaneData.ChapterInWhichUnlocked > currentChapterIndex)
            {
                Debug.LogWarning($"Level {selectedLaneData.LevelTitle} is locked until chapter {selectedLaneData.ChapterInWhichUnlocked}. Current chapter: {currentChapterIndex}");
                return;
            }


            int world1CurrentChapterFlagIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            if (selectedLaneData.WorldFlagIndex > world1CurrentChapterFlagIndex)
            {
                Debug.LogWarning($"Current progression is at {world1CurrentChapterFlagIndex}, cannot open level select for {selectedLaneData.LevelTitle} (Flag Index: {selectedLaneData.WorldFlagIndex})");
                return;
            }

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            if (_levelSelectUI != null)
                _levelSelectUI.PlayScaleDownFeedback();

            OnLaneInteracted?.Invoke(selectedLaneData);
            OnLaneInteractedIndex?.Invoke(CurrentLaneIndex);
        }

        public void SelectLane(int index)
        {
            if (index < 0 || index >= LaneData.Count)
                return;

            CurrentLaneIndex = index;
            OnLaneSelected.Invoke(CurrentLaneIndex);
            
            if (_inputInteraction != null)
                _inputInteraction.Appear();
            
            UpdateArrowStates();
        }

        private void UpdateArrowStates()
        {
            if (_levelSelectUI == null) return;

            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);

            bool canGoPrev = false;
            for (int i = CurrentLaneIndex - 1; i >= 0; i--)
            {
                if (LaneData[i].ChapterInWhichUnlocked <= currentChapterIndex)
                {
                    canGoPrev = true;
                    break;
                }
            }

            bool canGoNext = false;
            // The current lane is considered the "rightmost unlocked lane" (showing "???") 
            // if its ChapterInWhichUnlocked is exactly the currentChapterIndex.
            // In this case, the player should not be able to go further right.
            if (LaneData[CurrentLaneIndex].ChapterInWhichUnlocked < currentChapterIndex)
            {
                for (int i = CurrentLaneIndex + 1; i < LaneData.Count; i++)
                {
                    if (LaneData[i].ChapterInWhichUnlocked <= currentChapterIndex)
                    {
                        canGoNext = true;
                        break;
                    }
                }
            }

            _levelSelectUI.SetLeftArrowState(canGoPrev);
            _levelSelectUI.SetRightArrowState(canGoNext);
        }

        public void SelectNextLane()
        {
            int nextIndex = CurrentLaneIndex + 1;
            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);

            int targetIndex = -1;
            while (nextIndex < LaneData.Count)
            {
                if (LaneData[nextIndex].ChapterInWhichUnlocked <= currentChapterIndex)
                {
                    targetIndex = nextIndex;
                    break;
                }
                nextIndex++;
            }

            if (targetIndex == -1) return;

            if (_levelSelectUI != null)
                _levelSelectUI.OnRightArrowPressed();

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            SelectLane(targetIndex);
            if (_spinRight != null)
                _spinRight.PlayFeedbacks();

            if (_skyboxRotator != null)
                _skyboxRotator.Rotate(1f);
        }

        public void SelectPreviousLane()
        {
            int prevIndex = CurrentLaneIndex - 1;
            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);

            int targetIndex = -1;
            while (prevIndex >= 0)
            {
                if (LaneData[prevIndex].ChapterInWhichUnlocked <= currentChapterIndex)
                {
                    targetIndex = prevIndex;
                    break;
                }
                prevIndex--;
            }

            if (targetIndex == -1) return;

            if (_levelSelectUI != null)
                _levelSelectUI.OnLeftArrowPressed();

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            SelectLane(targetIndex);
            if (_spinLeft != null)
                _spinLeft.PlayFeedbacks();

            if (_skyboxRotator != null)
                _skyboxRotator.Rotate(-1f);
        }

        public LevelDataSO GetCurrentLevelData() => LaneData[CurrentLaneIndex];
    }
}
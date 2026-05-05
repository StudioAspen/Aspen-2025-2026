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

        [Header("Skybox Rotation")]
        [SerializeField] private SkyboxRotator _skyboxRotator;
        [SerializeField] private Material _baseSkybox;

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

            OnEnter.Invoke();
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
            OnLeave.Invoke();
        }

        private void OnInteract()
        {
            LevelDataSO selectedLaneData = LaneData[CurrentLaneIndex];

            int world1CurrentChapterFlagIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            if (selectedLaneData.WorldFlagIndex > world1CurrentChapterFlagIndex)
            {
                Debug.LogWarning($"Current progression is at {world1CurrentChapterFlagIndex}, cannot open level select for {selectedLaneData.LevelTitle} (Flag Index: {selectedLaneData.WorldFlagIndex})");
                return;
            }

            OnLaneInteracted?.Invoke(selectedLaneData);
            OnLaneInteractedIndex?.Invoke(CurrentLaneIndex);
        }

        public void SelectLane(int index)
        {
            if (index < 0 || index >= LaneData.Count)
                return;

            CurrentLaneIndex = index;
            OnLaneSelected.Invoke(CurrentLaneIndex);
        }

        public void SelectNextLane()
        {
            if (CurrentLaneIndex == LaneData.Count - 1)
                return;

            SelectLane(CurrentLaneIndex + 1);
            if (_spinRight != null)
                _spinRight.PlayFeedbacks();

            if (_skyboxRotator != null)
                _skyboxRotator.Rotate(1f);
        }

        public void SelectPreviousLane()
        {
            if (CurrentLaneIndex == 0)
                return;

            SelectLane(CurrentLaneIndex - 1);
            if (_spinLeft != null)
                _spinLeft.PlayFeedbacks();

            if (_skyboxRotator != null)
                _skyboxRotator.Rotate(-1f);
        }

        public LevelDataSO GetCurrentLevelData() => LaneData[CurrentLaneIndex];
    }
}
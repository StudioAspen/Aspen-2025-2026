using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Manages the tutorial popup UI. Listens to TutorialCheckpoint events
    /// and displays/hides the panel with the appropriate text.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [field: SerializeField] public GameObject TutorialPanel { get; private set; }
        [SerializeField] private TextMeshProUGUI tutorialTitleText;
        [SerializeField] private TextMeshProUGUI tutorialText;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player enterFeedback;
        [SerializeField] private MMF_Player exitFeedback;


        [ShowInInspector] public TutorialCheckpoint CurrentTutorial => _currentTutorial;
        private TutorialCheckpoint _currentTutorial;

        private void Awake()
        {
            if (TutorialPanel == null)
                TutorialPanel = transform.Find("TutorialPanel").gameObject;
        }

        private void OnEnable()
        {
            TutorialCheckpoint.OnTutorialCheckpointHit += HandleTutorialCheckpoint;
            if (InputManager.Instance != null)
                InputManager.Instance.OnControlSchemeChanged += HandleControlSchemeChanged;
        }

        private void OnDisable()
        {
            TutorialCheckpoint.OnTutorialCheckpointHit -= HandleTutorialCheckpoint;
            if (InputManager.Instance != null)
                InputManager.Instance.OnControlSchemeChanged -= HandleControlSchemeChanged;
        }

        private void HandleControlSchemeChanged(InputManager.ControlScheme scheme)
        {
            if (_currentTutorial != null)
            {
                UpdateUI(_currentTutorial);
            }
        }

        // When a checkpoint is hit, it sends itself as a parameter. If the parameter is null, it means the player left the checkpoint.
        private void HandleTutorialCheckpoint(TutorialCheckpoint checkpoint)
        {
            if (checkpoint == null)
            {
                HideTutorial();
                return;
            }

            _currentTutorial = checkpoint;
            UpdateUI(checkpoint);
        }

        private void UpdateUI(TutorialCheckpoint checkpoint)
        {
            InputManager.ControlScheme scheme = InputManager.Instance != null 
                ? InputManager.Instance.CurrentControlScheme 
                : InputManager.ControlScheme.KeyboardMouse;
                
            ShowTutorial(checkpoint.GetTitle(scheme), checkpoint.GetText(scheme));
        }

        private void ShowTutorial(string title, string message)
        {
            if (tutorialTitleText != null)
                tutorialTitleText.text = title;

            if (tutorialText != null)
                tutorialText.text = message;

            if (enterFeedback != null)
                enterFeedback.PlayFeedbacks();
        }

        private void HideTutorial()
        {
            _currentTutorial = null;

            if (exitFeedback != null)
                exitFeedback.PlayFeedbacks();
        }
    }
}

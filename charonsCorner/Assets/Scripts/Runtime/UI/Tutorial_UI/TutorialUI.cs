using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Manages the tutorial popup UI. Listens to TutorialCheckpoint events
    /// and displays/hides the panel with the appropriate text.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [field: SerializeField] public GameObject TutorialPanel { get; private set; }
        [SerializeField] private TextMeshProUGUI tutorialText;


        [ShowInInspector] public TutorialCheckpoint CurrentTutorial => _currentTutorial;
        private TutorialCheckpoint _currentTutorial;

        private void Awake()
        {
            if (TutorialPanel == null)
                TutorialPanel = transform.Find("TutorialPanel").gameObject;

            TutorialPanel.SetActive(false);
        }

        private void OnEnable() => TutorialCheckpoint.OnTutorialCheckpointHit += HandleTutorialCheckpoint;
        private void OnDisable() => TutorialCheckpoint.OnTutorialCheckpointHit -= HandleTutorialCheckpoint;

        // When a checkpoint is hit, it sends itself as a parameter. If the parameter is null, it means the player left the checkpoint.
        private void HandleTutorialCheckpoint(TutorialCheckpoint checkpoint)
        {
            if (checkpoint == null)
            {
                HideTutorial();
                return;
            }

            _currentTutorial = checkpoint;
            ShowTutorial(checkpoint.TutorialText);
        }

        private void ShowTutorial(string message)
        {
            if (tutorialText != null)
                tutorialText.text = message;

            TutorialPanel.SetActive(true);
        }

        private void HideTutorial()
        {
            TutorialPanel.SetActive(false);
            _currentTutorial = null;
        }
    }
}

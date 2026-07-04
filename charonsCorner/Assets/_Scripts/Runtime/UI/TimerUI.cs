using TMPro;
using UnityEngine;
using CharonsCorner.Runtime;

namespace CharonsCorner.Runtime.UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private RankingSystem _rankingSystem;

        private void Start()
        {
            if (_rankingSystem == null)
            {
                _rankingSystem = FindFirstObjectByType<RankingSystem>();
            }

            if (GameManager.Instance != null)
            {
                UpdateVisibility(GameManager.Instance.CurrentGameState);
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void Update()
        {
            if (_rankingSystem != null && _timerText != null)
            {
                UpdateTimerText(_rankingSystem.LevelTimeSeconds);
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            UpdateVisibility(newState);
        }

        private void UpdateVisibility(GameState state)
        {
            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(state != GameState.Cutscene);
            }
        }

        private void UpdateTimerText(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            _timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        }
    }
}

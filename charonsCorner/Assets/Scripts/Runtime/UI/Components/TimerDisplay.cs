using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Displays a timer UI as Minutes : Seconds : Milliseconds.  
    /// </summary>
    public class TimerDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private ScoringSystem _scoringSystem;

        private void Awake()
        {
            // Auto-assign text if not set in Inspector
            if (_timerText == null)
            {
                _timerText = GetComponentInChildren<TMP_Text>();
                if (_timerText == null)
                {
                    Debug.LogError("TimerDisplay: No TMP_Text assigned or found in children.", this);
                }
            }

            if (_scoringSystem == null)
            {
                _scoringSystem = FindFirstObjectByType<ScoringSystem>();
                if (_scoringSystem == null)
                {
                    Debug.Log("TimerDisplay: No ScoringSystem found in scene", this);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_timerText) return;

            float time;
            if (_scoringSystem != null)
            {
                time = _scoringSystem.LevelTimeSeconds;
            }
            else 
            {
                return;
            }

            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time - Mathf.FloorToInt(time)) * 1000f);

            _timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
        }
    }
}

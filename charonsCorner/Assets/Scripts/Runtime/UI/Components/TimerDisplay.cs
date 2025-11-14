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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

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

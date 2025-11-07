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

        /// <summary>
        /// Called by Unity before the first Update to perform component initialization.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Updates the on-screen timer text each frame by reading the current level time from the scoring system and formatting it as MM:SS:MMM.
        /// </summary>
        /// <remarks>
        /// If the text component reference or the scoring system is missing, the method exits without changing the display.
        /// </remarks>
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
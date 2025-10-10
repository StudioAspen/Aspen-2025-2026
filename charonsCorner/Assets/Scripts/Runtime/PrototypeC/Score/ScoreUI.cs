using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Updates the score UI when the player hits pins.
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += HandleScoreChanged; // subscribe to event
            HandleScoreChanged(ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0);
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= HandleScoreChanged; // unsubscribe to event
        }

        /// <summary>
        /// Updates the score UI to match the player's score.
        /// </summary>
        /// <param name="newScore"></param>
        private void HandleScoreChanged(int newScore)
        {
            if (_scoreText != null) _scoreText.text = newScore.ToString();
        }
    }
}

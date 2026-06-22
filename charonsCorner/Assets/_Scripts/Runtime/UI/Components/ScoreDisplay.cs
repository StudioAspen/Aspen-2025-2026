using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Displays a the amount of points that the player has.  
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private ScoringSystem _scoringSystem;

        private void Awake()
        {
            if (_scoreText == null)
            {
                _scoreText = GetComponentInChildren<TMP_Text>();
                if (_scoreText == null)
                {
                    Debug.LogError("ScoreDisplay: No TMP_Text assigned or found in children.", this);
                }
            }

            if (_scoringSystem == null)
            {
                _scoringSystem = FindFirstObjectByType<ScoringSystem>();
                if (_scoringSystem == null)
                {
                    Debug.LogError("ScoreDisplay: No ScoringSystem found in scene.", this);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_scoreText == null || _scoringSystem == null) return;

            _scoreText.text = _scoringSystem.GetPoints().ToString();
        }
    }
}

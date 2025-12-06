using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helps handle the player's score when pins are hit.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PinScoring : MonoBehaviour
    {
        [Header("Scoring")]
        [SerializeField] private int _pointsPerPin = 100;

        [Header("Knockdown Detection")]
        [Tooltip("Angle (deg) from upright that counts a pin as knocked down.")]
        [SerializeField] private float _knockdownAngle = 60f;

        [SerializeField] private ScoringSystem _scoringSystem;

        private bool _scored = false;

        private void Awake()
        {
            if (_scoringSystem == null)
            {
                _scoringSystem = FindFirstObjectByType<ScoringSystem>();
                if (_scoringSystem == null)
                {
                    Debug.Log("No ScoringSystem found in scene");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_scored) return;

            float angleFromUpright = Vector3.Angle(transform.up, Vector3.up); // angle btw pin's up and world's up
            bool isTilted = angleFromUpright >= _knockdownAngle;

            if (isTilted) ScoreOnce();
        }

        /// <summary>
        /// Add the score to the player's score, and count this pin as scored.
        /// </summary>
        private void ScoreOnce()
        {
            _scored = true;

            if (_scoringSystem != null)
            {
                _scoringSystem.AddPoints(_pointsPerPin);
            }

            Destroy(gameObject, 5f); // Destroy pin after 5 seconds
        }
    }
}
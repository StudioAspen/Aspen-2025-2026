using System.Collections.Specialized;
using UnityEngine;

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
        [SerializeField] private float _knockdownAngle = 45f;

        private bool _scored = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
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
        /// Count this pin as scored.
        /// </summary>
        private void ScoreOnce()
        {
            _scored = true;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(_pointsPerPin);
            }
            enabled = false;
        }
    }
}

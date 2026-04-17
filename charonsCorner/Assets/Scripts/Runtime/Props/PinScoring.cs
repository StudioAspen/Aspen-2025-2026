using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helps handle the player's score when pins are hit.
    /// </summary>
    public class PinScoring : MonoBehaviour
    {
        [Header("Scoring")]
        [SerializeField] private float _secondsToSubtract = 1f;

        public float SecondsToSubtract => _secondsToSubtract;

        public static System.Action<float> OnPinScored;
    }
}
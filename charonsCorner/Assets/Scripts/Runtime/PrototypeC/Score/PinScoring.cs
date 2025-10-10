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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A simple script to toggle between a Pin and a Humanoid game object.
    /// </summary>
    public class PinOrHumanoid : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _pinObject;
        [SerializeField] private GameObject _humanoidObject;

        [Header("Settings")]
        [SerializeField] private bool _startAsPin = true;

        private void Start()
        {
            if (_startAsPin)
            {
                Pin();
            }
            else
            {
                Humanoid();
            }
        }

        /// <summary>
        /// Activates the Pin object and deactivates the Humanoid object.
        /// </summary>
        public void Pin()
        {
            if (_pinObject != null) _pinObject.SetActive(true);
            if (_humanoidObject != null) _humanoidObject.SetActive(false);
        }

        /// <summary>
        /// Activates the Humanoid object and deactivates the Pin object.
        /// </summary>
        public void Humanoid()
        {
            if (_humanoidObject != null) _humanoidObject.SetActive(true);
            if (_pinObject != null) _pinObject.SetActive(false);
        }
    }
}

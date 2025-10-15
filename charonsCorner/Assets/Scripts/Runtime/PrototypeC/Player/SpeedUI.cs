using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SpeedUI : MonoBehaviour
    {
        [field: SerializeField] public TextMeshProUGUI SpeedText { get; private set; }
        private Rigidbody _playerRb;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            SpeedText.text = "Speed: " + _playerRb.linearVelocity.magnitude.ToString("0.00");
        }
    }
}

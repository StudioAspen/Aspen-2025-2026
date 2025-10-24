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
            Color color;
            if (_playerRb.linearVelocity.magnitude > 30f)
            {
                color = Color.red;
            }
            else if (_playerRb.linearVelocity.magnitude > 10f)
            {
                color = Color.yellow;
            }
            else
            {
                color = Color.green;
            }
            SpeedText.color = color;
            SpeedText.text = "Speed: " + _playerRb.linearVelocity.magnitude.ToString("0");
        }
    }
}

using UnityEngine;
using TMPro;

namespace CharonsCorner.Runtime
{
    public class PinUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI uiText;
        private string preText = "Pins ";
        private string postText = "/10";
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            uiText.text = preText + ScoreManager.Instance.numPinsKnocked + postText;
        }
    }
}

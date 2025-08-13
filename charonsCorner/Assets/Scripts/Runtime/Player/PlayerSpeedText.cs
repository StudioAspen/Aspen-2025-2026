using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerSpeedText : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private TMP_Text text;

        private void LateUpdate()
        {
            text.text = $"<b>Speed:</b> {Utilities.FloatToString(playerController.CurrentSpeed, 2)}";
        }
    }
}

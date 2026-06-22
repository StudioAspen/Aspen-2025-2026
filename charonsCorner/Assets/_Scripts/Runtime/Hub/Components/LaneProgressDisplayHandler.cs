using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class LaneProgressDisplayHandler : MonoBehaviour
    {
        [SerializeField] private Light _spotlight;
        [SerializeField] private Canvas _inputDisplayCanvas;
        [SerializeField] private Canvas _lockedDisplayCanvas;

        [SerializeField] private Color _lockedColor = Color.red;
        [SerializeField] private Color _unlockedColor = Color.white;

        public void ShowUnlocked()
        {
            _spotlight.color = _unlockedColor;
            _inputDisplayCanvas.gameObject.SetActive(true);
            _lockedDisplayCanvas.gameObject.SetActive(false);
        }
        
        public void ShowLocked()
        {
            _spotlight.color = _lockedColor;
            _inputDisplayCanvas.gameObject.SetActive(false);
            _lockedDisplayCanvas.gameObject.SetActive(true);
        }
    }
}
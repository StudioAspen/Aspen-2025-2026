using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    
    public class SoulCountText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI soulText;

        private void OnEnable()
        {
            if (SoulSingleton.Instance != null)
                SoulSingleton.Instance.OnSoulCountChanged += UpdateSoulText;

            // Initialize UI
            if (SoulSingleton.Instance != null)
                UpdateSoulText(SoulSingleton.Instance.SoulCount);
        }

        private void OnDisable()
        {
            if (SoulSingleton.Instance != null)
                SoulSingleton.Instance.OnSoulCountChanged -= UpdateSoulText;
        }

        private void UpdateSoulText(int count)
        {
            soulText.text = $"Souls: {count}";
        }
    }
}

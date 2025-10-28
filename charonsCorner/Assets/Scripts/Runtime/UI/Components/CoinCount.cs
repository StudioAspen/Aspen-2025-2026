using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    
    public class CoinCountText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;

        private void OnEnable()
        {
            if (CoinSingleton.Instance != null)
                CoinSingleton.Instance.OnCoinCountChanged += UpdateCoinText;

            // Initialize UI
            if (CoinSingleton.Instance != null)
                UpdateCoinText(CoinSingleton.Instance.CoinCount);
        }

        private void OnDisable()
        {
            if (CoinSingleton.Instance != null)
                CoinSingleton.Instance.OnCoinCountChanged -= UpdateCoinText;
        }

        private void UpdateCoinText(int count)
        {
            coinText.text = $"Coins: {count}";
        }
    }
}

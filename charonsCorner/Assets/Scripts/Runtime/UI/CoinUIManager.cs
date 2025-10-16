using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class CoinUIManager : MonoBehaviour
    {
        public static CoinUIManager Instance { get; private set; }

        [Header("UI References")]
       [SerializeField] private Transform coinContainer;
       [SerializeField] private GameObject CoinImagePrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddCoin(Sprite coinSprite)
        {
            if (coinContainer == null || CoinImagePrefab == null || coinSprite == null)
                return;

            GameObject coinGo = Instantiate(CoinImagePrefab, coinContainer);
            Image img = coinGo.GetComponent<Image>();
            if (img != null)
                img.sprite = coinSprite;
            {
                
            }
        }
    }
}

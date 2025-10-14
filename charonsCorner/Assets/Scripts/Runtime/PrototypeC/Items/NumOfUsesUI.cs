using CharonsCorner.ItemPowers;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class NumOfUsesUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _numOfUsesText;

        private string _preText = "Uses: ";
        private string _noItemText = "No Item";

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            ItemManager.OnItemChanged += HandleItemChanged; // subscribe to event
            //HandleItemChanged(ItemManager.Instance != null ? ItemManager.Instance.currentItem : _numOfUsesText);
        }

        private void OnDisable()
        {
            ItemManager.OnItemChanged -= HandleItemChanged; // unsubscribe to event
        }

        /// <summary>
        /// Updates the item UI to match the player's item.
        /// </summary>
        /// <param name="newItemName"></param>
        private void HandleItemChanged(ItemPower newPower)
        {
            if (_numOfUsesText != null) _numOfUsesText.text = _preText + newPower.itemUses;
        }
    }
}

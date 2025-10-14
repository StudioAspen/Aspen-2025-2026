using CharonsCorner.ItemPowers;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Handles changing the UI of what item the player is holding.
    /// </summary>
    public class HeldItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _heldItemNameText;

        private string _preText = "Held Item: ";
        private string _noItemText = "None";

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
            //HandleItemChanged(ItemManager.Instance != null ? ItemManager.Instance.currentItem : _noItemText);
        }

        private void OnDisable()
        {
            ItemManager.OnItemChanged -= HandleItemChanged; // unsubscribe to event
        }

        /// <summary>
        /// Updates the item UI to match the player's item.
        /// </summary>
        /// <param name="newItemName"></param>
        private void HandleItemChanged(ItemPower newItem)
        {
            if (newItem != null) _heldItemNameText.text = _preText + newItem.itemName;
            else _heldItemNameText.text = _noItemText;
        }


    }
}

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
        private GameObject player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Update is called once per frame
        void Update()
        {
            _numOfUsesText.text = _preText + player.GetComponent<PlayerAbility>().currentUses;
        }

        /*private void OnEnable()
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
            if (newPower != null) _numOfUsesText.text = _preText + newPower.itemUses;
        }*/
    }
}

using System;
using System.Collections.Specialized;
using UnityEngine;

namespace CharonsCorner.Runtime
{

    /// <summary>
    /// Handles changing the player's held item name.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; } // Singleton
        public static event Action<string> OnItemChanged;

        public string ItemName { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Awake()
        {
            if (Instance != null && Instance == this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Notify();
        }

        /// <summary>
        /// Changes the player's held item name.
        /// </summary>
        /// <param name="itemName"></param>
        public void ChangeItem(string newItemName)
        {
            ItemName = newItemName;
            Notify();
        }

        /// <summary>
        /// Removes the player's held item name.
        /// </summary>
        /// <param name="itemName"></param>
        public void RemoveItem()
        {
            ItemName = "None";
            Notify();
        }


        // Invoke the action when the item is changed
        private void Notify() => OnItemChanged?.Invoke(ItemName);
    }
}
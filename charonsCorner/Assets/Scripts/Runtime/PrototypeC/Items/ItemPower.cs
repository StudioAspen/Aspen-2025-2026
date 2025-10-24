using UnityEngine;

namespace CharonsCorner.ItemPowers
{
    public abstract class ItemPower : MonoBehaviour
    {
        public string itemName;
        public int itemUses;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public abstract void itemPower(Rigidbody rigidbody);
    }
}

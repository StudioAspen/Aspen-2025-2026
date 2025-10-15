using UnityEngine;
using CharonsCorner.ItemPowers;
using TMPro;

namespace CharonsCorner.Runtime
{
    public class ItemCollectable : MonoBehaviour
    {
        public int itemIndex;
        [SerializeField]
        private  TextMeshProUGUI itemUI;
        private ItemPower itemPower;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            itemPower = GetComponent<ItemPower>();
        }

        // Update is called once per frame
        void Update()
        {
            itemUI.text = itemPower.itemName;
            itemUI.gameObject.transform.LookAt(Camera.main.transform.position);
            itemUI.gameObject.transform.Rotate(Vector3.up, 180f);
        }
    }
}

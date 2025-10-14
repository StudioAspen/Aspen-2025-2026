using UnityEngine;
using CharonsCorner.Runtime;
using CharonsCorner.ItemPowers;

public class PlayerCollector : MonoBehaviour
{
    [SerializeField]
    private PlayerAbility playerAbility;
    [SerializeField]
    private ItemPowerSO itemPowerSO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            playerAbility.currentItemIndex = other.GetComponent<ItemCollectable>().itemIndex;
            playerAbility.currentUses = itemPowerSO.itemList[playerAbility.currentItemIndex].GetComponent<ItemPower>().itemUses;
            if (other.GetComponent<ItemCollectable>().itemIndex == 3)
            {
                playerAbility.enableBulletShooter();
            }

            ItemManager.Instance.ChangeItem(other.GetComponent<ItemPower>());
            Destroy(other.gameObject);
        }
    }
}

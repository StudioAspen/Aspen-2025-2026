using CharonsCorner.Runtime;
using UnityEngine;
using CharonsCorner.ItemPowers;

public class PlayerAbility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemPowerSO itemPowerSO;
    public int currentItemIndex = -1;
    public int currentUses;
    public Rigidbody playerRb;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentItemIndex > -1 && currentUses > 0) 
        {
            itemPowerSO.itemList[currentItemIndex].GetComponent<ItemPower>().itemPower(playerRb);
            currentUses--;
        }
    }
}

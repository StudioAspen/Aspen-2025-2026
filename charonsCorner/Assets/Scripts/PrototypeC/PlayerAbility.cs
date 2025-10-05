using UnityEngine;

public class PlayerAbility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemPowerSO itemPowerSO;
    public int currentItemIndex = 0;
    public int currentUses;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentItemIndex != 0 && currentUses > 0) 
        {
            itemPowerSO.itemList[currentItemIndex].GetComponent<ItemPower>().itemPower();
            currentUses--;
        }
    }
}

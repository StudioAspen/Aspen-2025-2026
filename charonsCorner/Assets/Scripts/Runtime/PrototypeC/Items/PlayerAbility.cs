using CharonsCorner.Runtime;
using UnityEngine;
using CharonsCorner.ItemPowers;
using System.Collections;

public class PlayerAbility : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemPowerSO itemPowerSO;
    public string abilityName;
    public int currentItemIndex = -1;
    public int currentUses;
    public Rigidbody playerRb;
    public GameObject bulletShooter;

    private IEnumerator changeGravity(float gravity, float duration)
    {
        GetComponent<PrototypePlayerController>().setGravity(gravity);
        yield return new WaitForSeconds(duration);
        GetComponent<PrototypePlayerController>().setGravity(30);
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentItemIndex > -1 && currentUses == 0)
        {
            ItemManager.Instance.RemoveItem();
            currentItemIndex = -1;
        }
        if (Input.GetKeyDown(KeyCode.Space) && currentItemIndex > -1 && currentUses > 0) 
        {
            ItemPower item = itemPowerSO.itemList[currentItemIndex].GetComponent<ItemPower>();
            item.itemPower(playerRb);
            abilityName = item.itemName;
        }
    }

    public void modGravity(float gravity, float duration)
    {
        StartCoroutine(changeGravity(gravity, duration));
    }

    public void enableBulletShooter()
    {
        bulletShooter.SetActive(true);
    }
}

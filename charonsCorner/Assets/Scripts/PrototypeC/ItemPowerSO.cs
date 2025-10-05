using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemPowerSO", menuName = "Scriptable Objects/ItemPowerSO")]
public class ItemPowerSO : ScriptableObject
{
    public List<GameObject> itemList = new List<GameObject>();
}

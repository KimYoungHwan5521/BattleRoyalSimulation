using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class Item
{
    public ItemManager.Items itemType;
    public LocalizedString itemName;
    [SerializeField] public int amount;
    [SerializeField] protected float weight;

    public Item(ItemManager.Items itemType, LocalizedString itemName, float weight, int amount = 1)
    {
        this.itemType = itemType;
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
    }

}

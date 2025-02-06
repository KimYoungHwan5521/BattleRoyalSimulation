using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public ItemManager.Items itemType;
    public string itemName;
    [SerializeField] public int amount;
    [SerializeField] protected float weight;

    public Item(ItemManager.Items itemType, string itemName, float weight, int amount = 1)
    {
        this.itemType = itemType;
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
    }

}

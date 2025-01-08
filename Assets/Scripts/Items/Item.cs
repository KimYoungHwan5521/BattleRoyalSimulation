using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public string itemName;
    [SerializeField] public int amount;
    [SerializeField] protected float weight;

    public Item(string itemName, float weight, int amount = 1)
    {
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
    }

    public bool IsValid()
    {
        if(itemName == null || itemName == "") return false;
        return true;
    }
}

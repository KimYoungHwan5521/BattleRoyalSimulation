using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public string itemName;
    [SerializeField] protected float weight;

    public Item(string itemName, float weight)
    {
        this.itemName = itemName;
        this.weight = weight;
    }
}

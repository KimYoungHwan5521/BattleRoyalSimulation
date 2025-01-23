using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BulletproofVest : Item
{
    float armor;
    public float Armor => armor;
    public BulletproofVest(string itemName, float weight, float armor, int amount = 1) : base(itemName, weight, amount)
    {
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
        this.armor = armor;
    }
}

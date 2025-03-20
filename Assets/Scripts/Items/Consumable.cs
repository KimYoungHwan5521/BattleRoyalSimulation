using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Item
{
    public Consumable(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {

    }
}

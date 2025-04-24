using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buriable : Consumable
{
    public Buriable(ItemManager.Items itemType, string itemName, float weight, int amount = 1) : base(itemType, itemName, weight, amount)
    {
    }
}

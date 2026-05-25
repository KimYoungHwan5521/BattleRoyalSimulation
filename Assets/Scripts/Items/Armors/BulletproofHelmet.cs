using System;
using UnityEngine.Localization;

[Serializable]
public class BulletproofHelmet : Armor
{
    public BulletproofHelmet(ItemManager.Items itemType, LocalizedString itemName, float weight, float defense, float maxDurability, int amount = 1) 
        : base(itemType, itemName, weight, defense, maxDurability, amount)
    {

    }
}

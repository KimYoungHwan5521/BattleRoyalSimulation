using System;
using UnityEngine.Localization;

[Serializable]
public class BulletproofHelmet : Armor
{
    public BulletproofHelmet(ItemManager.Items itemType, LocalizedString itemName, float weight, float defense, float maxDurability, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, defense, maxDurability, quality, amount)
    {

    }
}

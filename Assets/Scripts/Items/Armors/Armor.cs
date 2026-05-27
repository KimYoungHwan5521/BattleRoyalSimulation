using System;
using UnityEngine.Localization;

[Serializable]
public class Armor : Item
{
    float defense;
    public float Defense => defense;
    float maxDurability;
    public float MaxDurability => maxDurability;
    float curDurability;
    public float CurDurability
    {
        get { return curDurability; }
        set
        {
            curDurability = Math.Clamp(value, 0, maxDurability);
        }
    }
    public float DurabilityPercent => curDurability / maxDurability;

    public Armor(ItemManager.Items itemType, LocalizedString itemName, float weight, float defense, float maxDurability, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1)
        : base(itemType, itemName, weight, quality, amount)
    {
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
        this.defense = defense;
        CurDurability = this.maxDurability = maxDurability;
    }

    public void SetDurabilityPercent(float percent)
    {
        CurDurability = maxDurability * percent;
    }
}

using System;
using UnityEngine.Localization;

[Serializable]
public class BulletproofHelmet : Item
{
    float armor;
    public float Armor => armor;
    float durability = 1f;
    public float Durability
    {
        get { return durability; }
        set
        {
            durability = Math.Clamp(value, 0f, 1f);
        }
    }
    public BulletproofHelmet(ItemManager.Items itemType, LocalizedString itemName, float weight, float armor, int amount = 1) 
        : base(itemType, itemName, weight, amount)
    {
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
        this.armor = armor;
        durability = 1f;
    }
}

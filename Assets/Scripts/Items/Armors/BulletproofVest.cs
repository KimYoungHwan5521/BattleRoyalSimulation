using System;

[Serializable]
public class BulletproofVest : Item
{
    float armor;
    public float Armor => armor;
    public BulletproofVest(ItemManager.Items itemType, string itemName, float weight, float armor, int amount = 1) 
        : base(itemType, itemName, weight, amount)
    {
        this.itemName = itemName;
        this.weight = weight;
        this.amount = amount;
        this.armor = armor;
    }
}

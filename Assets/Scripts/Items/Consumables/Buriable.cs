using UnityEngine.Localization;

public class Buriable : Consumable
{
    bool isEnchanted;
    public bool IsEnchanted => isEnchanted;
    float damage;
    public float Damage => damage;

    public void Enchant()
    {
        isEnchanted = true;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage; 
    }

    public Buriable(ItemManager.Items itemType, LocalizedString itemName, float weight, float damage, CraftingQuality quality = CraftingQuality.NotCrafted, int amount = 1) 
        : base(itemType, itemName, weight, quality, amount)
    {
        this.damage = damage;
    }
}

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

    public Buriable(ItemManager.Items itemType, LocalizedString itemName, float weight, float damage, int amount = 1) : base(itemType, itemName, weight, amount)
    {
        this.damage = damage;
    }
}

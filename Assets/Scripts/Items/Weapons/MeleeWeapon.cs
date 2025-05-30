using System;
using System.Diagnostics;

public enum DamageType { Strike, Slash, GunShot, Explosion, Chemical }

[Serializable]
public class MeleeWeapon : Weapon
{
    DamageType damageType;
    public DamageType DamageType => damageType;
    bool isEnchanted;
    public bool IsEnchanted => isEnchanted;

    public void Enchant()
    {
        itemName = $"{itemType}(Enchanted)";
        if (Enum.TryParse(itemType.ToString() + "_Enchanted", out ItemManager.Items result))
        {
            itemType = result;
        }
        else UnityEngine.Debug.LogWarning($"Item type not found : {itemType.ToString() + "_Enchanted"}");
        isEnchanted = true;
    }

    public MeleeWeapon(ItemManager.Items itemType, string itemName, float weight, NeedHand needHand, DamageType damageType, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, needHand, attackDamage, attackRange, attackAnimNumber, amount)
    { 
        this.damageType = damageType;
    }
}

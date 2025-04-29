using System;

public enum DamageType { Strike, Cut, GunShot, Explosion, Chemical }

[Serializable]
public class MeleeWeapon : Weapon
{
    DamageType damageType;
    public DamageType DamageType => damageType;
    bool isEnchanted;
    public bool IsEnchanted => isEnchanted;

    public void Enchant()
    {
        isEnchanted = true;
    }

    public MeleeWeapon(ItemManager.Items itemType, string itemName, float weight, NeedHand needHand, DamageType damageType, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, needHand, attackDamage, attackRange, attackAnimNumber, amount)
    { 
        this.damageType = damageType;
    }
}

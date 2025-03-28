using System;

public enum DamageType { Strike, Cut, GunShot }

[Serializable]
public class MeleeWeapon : Weapon
{
    DamageType damageType;
    public DamageType DamageType => damageType;

    public MeleeWeapon(ItemManager.Items itemType, string itemName, float weight, NeedHand needHand, DamageType damageType, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, needHand, attackDamage, attackRange, attackAnimNumber, amount)
    { 
        this.damageType = damageType;
    }
}

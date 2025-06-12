using System;
using UnityEngine;
using UnityEngine.Localization;

public enum NeedHand { OneHand, TwoHand, OneOrTwoHand }

[Serializable]
public class Weapon : Item
{
    [SerializeField] NeedHand needHand;
    [SerializeField] float attackDamage;
    [SerializeField] float attackRange;
    [SerializeField] protected int attackAnimNumber;

    public NeedHand NeedHand => needHand;
    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public int AttackAnimNumber => attackAnimNumber;

    public Weapon(ItemManager.Items itemType, LocalizedString itemName, float weight, NeedHand needHand, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, amount)
    {
        this.needHand = needHand;
        this.attackDamage = attackDamage;
        this.attackRange = attackRange;
        this.attackAnimNumber = attackAnimNumber;
    }
}

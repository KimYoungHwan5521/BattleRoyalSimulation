using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weapon : Item
{
    [SerializeField] public float attackDamage;
    [SerializeField] public float attackRange;
    [SerializeField] protected int attackAnimNumber;
    public int AttackAnimNumber => attackAnimNumber;

    public Weapon(ItemManager.Items itemType, string itemName, float weight, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, amount)
    {
        this.attackDamage = attackDamage;
        this.attackRange = attackRange;
        this.attackAnimNumber = attackAnimNumber;
    }
}

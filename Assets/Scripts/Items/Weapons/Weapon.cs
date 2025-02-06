using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weapon : Item
{
    [SerializeField] public float attakDamage;
    [SerializeField] public float attackRange;
    [SerializeField] protected int attackAnimNumber;
    public int AttackAnimNumber => attackAnimNumber;

    public Weapon(ItemManager.Items itemType, string itemName, float weight, float attakDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemType, itemName, weight, amount)
    {
        this.attakDamage = attakDamage;
        this.attackRange = attackRange;
        this.attackAnimNumber = attackAnimNumber;
    }
}

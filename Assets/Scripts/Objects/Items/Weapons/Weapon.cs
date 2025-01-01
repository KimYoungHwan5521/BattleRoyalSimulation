using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weapon : Item
{
    [SerializeField] protected float attakDamage;
    [SerializeField] protected float attackRange;

    public Weapon(string itemName, float weight, float attakDamage, float attackRange) : base(itemName, weight)
    {
        this.attakDamage = attakDamage;
        this.attackRange = attackRange;
    }
}

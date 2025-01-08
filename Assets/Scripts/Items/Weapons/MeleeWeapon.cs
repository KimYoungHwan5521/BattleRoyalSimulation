using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeleeWeapon : Weapon
{
    public MeleeWeapon(string itemName, float weight, float attackDamage, float attackRange, int attackAnimNumber, int amount = 1) 
        : base(itemName, weight, attackDamage, attackRange, attackAnimNumber, amount)
    { 

    }
}

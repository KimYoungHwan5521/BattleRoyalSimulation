using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeleeWeapon : Weapon
{
    public MeleeWeapon(string itemName, float weight, float attackDamage, float attackRange, int attackAnimNumber) 
        : base(itemName, weight, attackDamage, attackRange, attackAnimNumber)
    { 

    }
}

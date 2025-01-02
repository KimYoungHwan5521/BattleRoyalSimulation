using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MeleeWeapon : Weapon
{
    public MeleeWeapon(string itemName, float weight, float attackDamage, float attackRange) 
        : base(itemName, weight, attackDamage, attackRange)
    { 

    }
}

public class Knife : MeleeWeapon
{
    public Knife(string itemName, float weight, float attackDamage, float attackRange)
        : base(itemName, weight, attackDamage, attackRange)
    {

    }
}
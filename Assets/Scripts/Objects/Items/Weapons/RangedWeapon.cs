using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RangedWeapon : Weapon
{
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected float shotCoolTime;
    [SerializeField] protected float magazineCapacity;
    [SerializeField] protected float reloadCoolTime;
    public RangedWeapon(string itemName, float weight, float attackDamage, float attackRange, 
        float projectileSpeed, float shotCoolTime, float magazineCapacity, float reloadCoolTime)
        : base(itemName, weight, attackDamage, attackRange)
    {
        this.projectileSpeed = projectileSpeed;
        this.shotCoolTime = shotCoolTime;
        this.magazineCapacity = magazineCapacity;
        this.reloadCoolTime = reloadCoolTime;
    }
}

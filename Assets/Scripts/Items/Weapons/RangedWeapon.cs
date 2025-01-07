using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RangedWeapon : Weapon
{
    [SerializeField] protected float minimumRange;
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected float shotCoolTime;
    [SerializeField] protected int magazineCapacity;
    [SerializeField] protected float reloadCoolTime;

    public float MinimumRange => minimumRange;
    public float ProjectileSpeed => projectileSpeed;
    public float ShotCoolTime => shotCoolTime;
    public int MagazineCapacity => magazineCapacity;
    public float ReloadCoolTime => reloadCoolTime;
    public RangedWeapon(string itemName, float weight, float attackDamage, float attackRange, 
        float minimumRange, float projectileSpeed, float shotCoolTime, int magazineCapacity, float reloadCoolTime, int attackAnimNumber)
        : base(itemName, weight, attackDamage, attackRange, attackAnimNumber)
    {
        this.minimumRange = minimumRange;
        this.projectileSpeed = projectileSpeed;
        this.shotCoolTime = shotCoolTime;
        this.magazineCapacity = magazineCapacity;
        this.reloadCoolTime = reloadCoolTime;
    }

}

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
    [SerializeField] protected int currentMagazine;
    [SerializeField] protected int shotAnimNumber;

    public float MinimumRange => minimumRange;
    public float ProjectileSpeed => projectileSpeed;
    public float ShotCoolTime => shotCoolTime;
    public int MagazineCapacity => magazineCapacity;
    public float ReloadCoolTime => reloadCoolTime;
    public int CurrentMagazine => currentMagazine;
    public int ShotAnimNumber => shotAnimNumber;
    public RangedWeapon(ItemManager.Items itemType, string itemName, float weight, float attackDamage, float attackRange, 
        float minimumRange, float projectileSpeed, float shotCoolTime, int magazineCapacity, 
        float reloadCoolTime, int attackAnimNumber, int shotAnimNumber, int amount = 1)
        : base(itemType, itemName, weight, attackDamage, attackRange, attackAnimNumber, amount)
    {
        this.minimumRange = minimumRange;
        this.projectileSpeed = projectileSpeed;
        this.shotCoolTime = shotCoolTime;
        this.magazineCapacity = magazineCapacity;
        this.reloadCoolTime = reloadCoolTime;
        this.shotAnimNumber = shotAnimNumber;
    }

    public void Fire()
    {
        currentMagazine--;
    }

    public void Reload(int amount)
    {
        currentMagazine += amount;
    }

}

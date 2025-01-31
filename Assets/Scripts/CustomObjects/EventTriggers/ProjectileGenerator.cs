using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGenerator : CustomObject
{
    Survivor owner;
    public Transform muzzleTF;

    protected override void Start()
    {
        base.Start();
        owner = GetComponent<Survivor>();
    }
    public void SpawnProjectile()
    {
        RangedWeapon weapon = owner.CurrentWeapon as RangedWeapon;
        weapon.Fire();

        if(weapon.itemName == "ShotGun")
        {
            SpawnProjectile_ShotGun(weapon);
            return;
        }

        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
        Bullet bullet = prefab.GetComponent<Bullet>();
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.LookRotation;
        bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.attakDamage, muzzleTF.position, destination, weapon.attackRange);
    }
    
    void SpawnProjectile_ShotGun(RangedWeapon weapon)
    {
        for(int i = 0; i < 12; i++)
        {
            GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
            Bullet bullet = prefab.GetComponent<Bullet>();
            Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.LookRotation;
            bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.attakDamage, muzzleTF.position, destination, weapon.attackRange);
        }

    }

    public void ResetMuzzleTF()
    {
        if(transform.Find("Right Hand")?.Find($"{owner.CurrentWeapon.itemName}")?.Find("Muzzle") != null)
            muzzleTF = transform.Find("Right Hand").Find($"{owner.CurrentWeapon.itemName}").Find("Muzzle");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGenerator : CustomObject
{
    Survivor owner;
    public Transform muzzleTF;
    float err = 3f;

    protected override void Start()
    {
        base.Start();
        owner = GetComponent<Survivor>();
    }
    public void SpawnProjectile()
    {
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
        Bullet bullet = prefab.GetComponent<Bullet>();
        RangedWeapon weapon = owner.CurrentWeapon as RangedWeapon;

        int num = weapon.itemName == "ShotGun" ? 12 : 1;

        for(int i = 0; i < num; i++)
        {
            float rand = Random.Range(-err, err);
            Vector2 destination = ((Vector2)owner.targetEnemy.transform.position).Rotate(rand);
            bullet.Initiate(weapon.ProjectileSpeed, weapon.attakDamage, muzzleTF.position, destination, weapon.attackRange);
        }
    }

    public void ResetMuzzleTF()
    {
        if(transform.Find("Right Hand")?.Find($"{owner.CurrentWeapon.itemName}")?.Find("Muzzle") != null)
            muzzleTF = transform.Find("Right Hand").Find($"{owner.CurrentWeapon.itemName}").Find("Muzzle");
    }

}

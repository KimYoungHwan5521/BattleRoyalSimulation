using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGenerator : CustomObject
{
    [SerializeField] Survivor owner;
    [SerializeField] Transform muzzleTF;
    float err = 3f;
    public void SpawnProjectile()
    {
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
        Bullet bullet = prefab.GetComponent<Bullet>();

        RangedWeapon weapon = owner.CurrentWeapon as RangedWeapon;
        float rand = Random.Range(-err, err);
        Vector2 destination = ((Vector2)owner.targetEnemy.transform.position).Rotate(rand);
        bullet.Initiate(weapon.ProjectileSpeed, weapon.attakDamage, muzzleTF.position, destination, weapon.attackRange);
    }
}

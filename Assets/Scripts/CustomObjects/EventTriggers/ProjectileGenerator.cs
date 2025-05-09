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
        owner.InGameUIManager.UpdateSelectedObjectInventory(owner);

        if (weapon.itemType == ItemManager.Items.ShotGun)
        {
            SpawnProjectile_ShotGun(weapon);
            return;
        }
        else if(weapon.itemType == ItemManager.Items.Bazooka)
        {
            SpawnProjectile_Bazooka(weapon);
            return;
        }
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
        Bullet bullet = prefab.GetComponent<Bullet>();
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.LookRotation;
        bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
    }
    
    void SpawnProjectile_ShotGun(RangedWeapon weapon)
    {
        for(int i = 0; i < 12; i++)
        {
            GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, muzzleTF.transform.position);
            Bullet bullet = prefab.GetComponent<Bullet>();
            Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.LookRotation;
            bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
        }
    }

    void SpawnProjectile_Bazooka(RangedWeapon weapon)
    {
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Rocket, muzzleTF.transform.position);
        Rocket rocket = prefab.GetComponent<Rocket>();
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.LookRotation;
        rocket.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
    }

    public void ResetMuzzleTF(Transform hand)
    {
        if(hand.Find($"{owner.CurrentWeapon.itemName}")?.Find("Muzzle") != null)
        {
            muzzleTF = hand.Find($"{owner.CurrentWeapon.itemName}").Find("Muzzle");
        }
        else
        {
            Debug.LogWarning("Muzzle not found!");
        }
    }

}

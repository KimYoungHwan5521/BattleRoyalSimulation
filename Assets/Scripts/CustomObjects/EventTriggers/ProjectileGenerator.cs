using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGenerator : CustomObject
{
    Survivor owner;
    public Transform muzzleTF;
    Vector2 spawnPos;

    protected override void Start()
    {
        base.Start();
        owner = GetComponent<Survivor>();
    }

    public void SpawnProjectile()
    {
        if (owner.CurrentWeaponAsRangedWeapon == null) return;
        RangedWeapon weapon = owner.CurrentWeapon as RangedWeapon;
        weapon.Fire();
        owner.InGameUIManager.UpdateSelectedObjectInventory(owner);

        if (muzzleTF == null) ResetMuzzleTF(owner.RightHandDisabled ? owner.leftHandTF : owner.rightHandTF);
        spawnPos = muzzleTF != null ? muzzleTF.position : owner.transform.position + owner.transform.up * 2;
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
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, spawnPos);
        Bullet bullet = prefab.GetComponent<Bullet>();
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.transform.up;
        bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
    }
    
    void SpawnProjectile_ShotGun(RangedWeapon weapon)
    {
        for(int i = 0; i < 12; i++)
        {
            GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Bullet, spawnPos);
            Bullet bullet = prefab.GetComponent<Bullet>();
            Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.transform.up;
            bullet.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
        }
    }

    void SpawnProjectile_Bazooka(RangedWeapon weapon)
    {
        GameObject prefab = PoolManager.Spawn(ResourceEnum.Prefab.Rocket, spawnPos);
        Rocket rocket = prefab.GetComponent<Rocket>();
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.transform.up;
        rocket.Initiate(owner, weapon.ProjectileSpeed, weapon.AttackDamage, muzzleTF.position, destination, weapon.AttackRange);
    }

    public void DrawBeam()
    {
        PlaySFX("laser,2", owner, muzzleTF.position);
        Vector2 destination = owner.TargetEnemy != null ? ((Vector2)owner.TargetEnemy.transform.position) : owner.transform.up;

        float err = owner.AimErrorRange;
        float rand = Random.Range(-err, err);
        
        Vector2 direction = ((destination - (Vector2)muzzleTF.position).normalized).Rotate(rand);

        RaycastHit2D[] hits = new RaycastHit2D[20];
        Physics2D.RaycastNonAlloc(muzzleTF.position, direction * owner.CurrentWeaponAsRangedWeapon.AttackRange, hits);
        Vector2 arrive = Vector2.zero;
        foreach(var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Wall"))
            {
                arrive = hit.point;
                break;
            }
            if(hit.collider.CompareTag("Survivor"))
            {
                if (hit.collider.TryGetComponent(out Survivor survivor)) survivor.TakeGunshotDamamge(owner, owner.CurrentWeaponAsRangedWeapon.AttackDamage);
                arrive = hit.point;
                break;
            }
            if(hit.collider.TryGetComponent(out Obstacle obstacle))
            {
                float randObs = Random.Range(0, 1f);
                if(randObs < obstacle.ObstructionRate)
                {
                    arrive = hit.point;
                    break;
                }
            }
        }
        if (arrive == Vector2.zero) arrive = (Vector2)muzzleTF.position + direction * owner.CurrentWeaponAsRangedWeapon.AttackRange;

        beam = PoolManager.Spawn(ResourceEnum.Prefab.Beam);
        beam.GetComponent<LineRenderer>().SetPositions(new Vector3[] { muzzleTF.position, arrive });
        Invoke(nameof(DespawnBeam), 0.3f);
    }

    GameObject beam;
    void DespawnBeam()
    {
        if (beam == null) Debug.LogWarning("There are no beam");
        else PoolManager.Despawn(beam);
    }

    public void ResetMuzzleTF(Transform hand)
    {
        if(hand.Find($"{owner.CurrentWeapon.itemType}")?.Find("Muzzle") != null)
        {
            muzzleTF = hand.Find($"{owner.CurrentWeapon.itemType}").Find("Muzzle");
        }
        else
        {
            Debug.LogWarning("Muzzle not found!");
        }
    }

}

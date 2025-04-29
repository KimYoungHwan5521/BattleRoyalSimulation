using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : CustomObject
{
    Survivor launcher;
    public Survivor Launcher => launcher;
    SpriteRenderer spriteRenderer;
    Collider2D col;
    TrailRenderer trailRenderer;
    float projectileSpeed;
    float damage;
    public float Damage => damage;
    Vector2 direction;
    Vector2 spawnedPosition;
    Vector2 lastPosition;
    float maxRange;
    public float MaxRange => maxRange;
    public float TraveledDistance { get { return Vector2.Distance(transform.position, spawnedPosition); } }

    float err;

    bool initiated;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Initiate(Survivor launcher, float projectileSpeed, float damage, Vector2 spawnedPosition, Vector2 targetPosition, float maxRange)
    {
        this.launcher = launcher;
        this.projectileSpeed = projectileSpeed;
        this.damage = damage;
        this.spawnedPosition = spawnedPosition;
        lastPosition = spawnedPosition;

        direction = targetPosition - spawnedPosition;
        direction.Normalize();
        err = launcher.CurrentWeapon.itemName == "ShotGun"?  7.5f : launcher.AimErrorRange;
        float rand = Random.Range(-err, err);
        if (launcher.CurrentWeapon.itemName == "SniperRifle") rand *= 0.67f;
        direction = direction.Rotate(rand);

        this.maxRange = maxRange;

        initiated = true;
    }

    // Shrapnel Trap
    public void Initiate(Survivor setter, Vector2 direction)
    {
        launcher = setter;
        projectileSpeed = 20f;
        maxRange = 5f;
        damage = 10f;
        lastPosition = transform.position;
        this.direction = direction;
        direction.Normalize();
        initiated = true;
    }

    void DelayedDespawn()
    {
        initiated = false;
        spriteRenderer.enabled = false;
        col.enabled = false;

        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(0.2f);

        PoolManager.Despawn(gameObject);
        spriteRenderer.enabled = true;
        col.enabled = true;
        trailRenderer.Clear();
        
    }

    bool hitSurvivor;
    Survivor beHitSurvivor;
    private void FixedUpdate()
    {
        if (!initiated) return;
        if (Vector2.Distance(transform.position, spawnedPosition) > maxRange)
        {
            DelayedDespawn();
        }
        transform.position += Time.fixedDeltaTime * projectileSpeed * (Vector3)direction;

        // 총알이 너무 빠르면 Collision Detection Mode가 Continious여도 충돌이 검출되지 않는 경우가 있어서 보조 계산
        //RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, lastPosition);
        //Debug.DrawLine(transform.position, lastPosition, Color.red);
        //foreach (RaycastHit2D hit in hits)
        //{
        //    //if (hit.collider.CompareTag("Wall"))
        //    //{
        //    //    Debug.Log(hit.collider.transform.position);
        //    //    if (Random.Range(0, 1f) < 0.5f)
        //    //    {
        //    //        PlaySFX("ricochet,10", launcher);
        //    //    }
        //    //    else
        //    //    {
        //    //        PlaySFX("ricochet2,10", launcher);
        //    //    }
        //    //    DelayedDespawn();
        //    //    return;
        //    //}
        //    if(hit.collider.CompareTag("Survivor") && !hit.collider.isTrigger)
        //    {
        //        Survivor victim = hit.collider.GetComponent<Survivor>();
        //        if (victim != launcher)
        //        {
        //            hitSurvivor = true;
        //            beHitSurvivor = victim;
        //        }
        //    }
        //}
        //if(hitSurvivor)
        //{
        //    beHitSurvivor.TakeDamage(this);
        //    DelayedDespawn();
        //}
        //lastPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!initiated) return;
        if (!collision.isTrigger && collision.CompareTag("Survivor") || collision.CompareTag("Wall"))
        {
            if (collision.CompareTag("Survivor"))
            {
                Survivor victim = collision.GetComponent<Survivor>();
                if (victim != launcher) victim.TakeDamage(this);
                else Debug.LogWarning("Launcer shot himself");
            }
            else
            {
                if(Random.Range(0, 1f) < 0.5f)
                {
                    PlaySFX("ricochet,5", launcher);
                }
                else
                {
                    PlaySFX("ricochet2,5", launcher);
                }
            }
            DelayedDespawn();
        }
        else if(collision.TryGetComponent(out Obstacle obstacle))
        {
            float rand = Random.Range(0, 1f);
            if(rand < obstacle.OpstructionRate)
            {
                if (Random.Range(0, 1f) < 0.5f)
                {
                    PlaySFX("ricochet,5", launcher);
                }
                else
                {
                    PlaySFX("ricochet2,5", launcher);
                }
            }
        }
    }
}

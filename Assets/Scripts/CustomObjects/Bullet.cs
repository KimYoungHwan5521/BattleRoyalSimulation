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
    Vector2 spawnedPosition;
    Vector2 targetPosition;
    Vector2 direction;
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
        this.targetPosition = targetPosition;

        direction = targetPosition - this.spawnedPosition;
        direction.Normalize();
        err = launcher.CurrentWeapon.itemName == "ShotGun"?  7.5f : launcher.AimErrorRange;
        float rand = Random.Range(-err, err);
        if (launcher.CurrentWeapon.itemName == "SniperRifle") rand *= 0.67f;
        direction = direction.Rotate(rand);

        this.maxRange = maxRange;

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

    private void FixedUpdate()
    {
        if (!initiated) return;
        if (Vector2.Distance(transform.position, spawnedPosition) > maxRange)
        {
            DelayedDespawn();
        }
        transform.position += Time.fixedDeltaTime * projectileSpeed * (Vector3)direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
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
                    SoundManager.Play(ResourceEnum.SFX.ricochet, transform.position);
                }
                else
                {
                    SoundManager.Play(ResourceEnum.SFX.ricochet2, transform.position);
                }
            }
            DelayedDespawn();
        }
    }
}

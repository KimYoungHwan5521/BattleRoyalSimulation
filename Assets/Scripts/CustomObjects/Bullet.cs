using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : CustomObject
{
    protected Survivor launcher;
    public Survivor Launcher => launcher;
    SpriteRenderer spriteRenderer;
    Collider2D col;
    TrailRenderer trailRenderer;
    protected float projectileSpeed;
    protected float damage;
    public float Damage => damage;
    protected Vector2 direction;
    protected Vector2 spawnedPosition;
    protected Vector2 lastPosition;
    protected float maxRange;
    public float MaxRange => maxRange;
    public float TraveledDistance { get { return Vector2.Distance(transform.position, spawnedPosition); } }

    float err;

    protected bool initiated;

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
        err = launcher.CurrentWeapon.itemType == ItemManager.Items.ShotGun? 7.5f : launcher.AimErrorRange;
        float rand = Random.Range(-err, err);
        if (launcher.CurrentWeapon.itemType == ItemManager.Items.SniperRifle) rand *= 0.67f;
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
        damage = 20f;
        lastPosition = transform.position;
        this.direction = direction;
        direction.Normalize();
        initiated = true;
    }

    protected virtual void DelayedDespawn()
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
            if(rand < obstacle.ObstructionRate)
            {
                if (Random.Range(0, 1f) < 0.5f)
                {
                    PlaySFX("ricochet,5", launcher);
                }
                else
                {
                    PlaySFX("ricochet2,5", launcher);
                }
                DelayedDespawn();
            }
        }
    }
}

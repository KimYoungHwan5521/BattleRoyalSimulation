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
    Vector3 direction;
    float maxRange;
    public float MaxRange => maxRange;
    public float TraveledDistance { get { return Vector2.Distance(transform.position, spawnedPosition); } }
    bool initiated;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Initiate(Survivor launcher, float projectileSpeed, float damage, Vector2 spawndPosition, Vector2 targetPosition, float maxRange)
    {
        this.launcher = launcher;
        this.projectileSpeed = projectileSpeed;
        this.damage = damage;
        this.spawnedPosition = spawndPosition;
        this.targetPosition = targetPosition;
        direction = targetPosition - spawnedPosition;
        direction.Normalize();
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
        transform.position += Time.fixedDeltaTime * projectileSpeed * direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            if (collision.CompareTag("Survivor"))
            {
                Survivor victim = collision.GetComponent<Survivor>();
                victim.TakeDamage(this);
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

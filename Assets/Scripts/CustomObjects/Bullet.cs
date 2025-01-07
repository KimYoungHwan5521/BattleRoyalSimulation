using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : CustomObject
{
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

    public void Initiate(float projectileSpeed, float damage, Vector2 spawndPosition, Vector2 targetPosition, float maxRange)
    {
        this.projectileSpeed = projectileSpeed;
        this.damage = damage;
        this.spawnedPosition = spawndPosition;
        this.targetPosition = targetPosition;
        direction = targetPosition - spawnedPosition;
        direction.Normalize();
        this.maxRange = maxRange;

        initiated = true;
    }

    private void FixedUpdate()
    {
        if (!initiated) return;
        if (Vector2.Distance(transform.position, spawnedPosition) > maxRange)
        {
            PoolManager.Despawn(gameObject);
        }
        transform.position += Time.fixedDeltaTime * projectileSpeed * direction;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Survivor") && !collision.isTrigger)
        {
            Survivor victim = collision.GetComponent<Survivor>();
            victim.TakeDamage(this);
            PoolManager.Despawn(gameObject);
        }
    }
}

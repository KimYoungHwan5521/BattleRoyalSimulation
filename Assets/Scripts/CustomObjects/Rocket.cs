using UnityEngine;

public class Rocket : Bullet
{
    [SerializeField] float explosionRange = 3f;

    void Explosion()
    {
        PoolManager.Spawn(ResourceEnum.Prefab.Explosion, transform.position);
        var hits = Physics2D.CircleCastAll(transform.position, explosionRange, Vector2.up);
        foreach (var hit in hits)
        {
            if (!hit.collider.isTrigger && hit.collider.TryGetComponent(out Survivor splashedSurvivor))
            {
                float distance = Vector2.Distance(transform.position, hit.point);
                bool critical = distance < 1f;
                float damage = critical ? launcher.CurrentWeapon.AttackDamage : launcher.CurrentWeapon.AttackDamage / (distance * distance);
                splashedSurvivor.TakeDamage(this, damage, critical);
            }
        }
        PoolManager.Despawn(gameObject);
        initiated = false;
    }

    private void FixedUpdate()
    {
        if (!initiated) return;
        transform.position += Time.fixedDeltaTime * projectileSpeed * (Vector3)direction;
        transform.right = direction;
        if (Vector2.Distance(transform.position, spawnedPosition) > maxRange)
        {
            Explosion();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initiated) return;
        if (!collision.isTrigger && (collision.CompareTag("Survivor") || collision.CompareTag("Wall")))
        {
            Explosion();
        }
    }
}

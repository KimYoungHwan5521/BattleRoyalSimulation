using UnityEngine;

public class Rocket : Bullet
{
    [SerializeField] float explosionRange = 3f;

    void Explosion()
    {
        PoolManager.Spawn(ResourceEnum.Prefab.Explosion, transform.position);
        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRange, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if (!hit.isTrigger && hit.TryGetComponent(out Survivor splashedSurvivor))
            {
                Vector2 closestPoint = hit.ClosestPoint(transform.position);
                float distance = Vector2.Distance(transform.position, closestPoint);
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

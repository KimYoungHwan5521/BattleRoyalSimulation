using UnityEngine;

public class LandMine : Trap
{
    [SerializeField] float explosionRange = 2;

    public void SetExplosionRange(float explosionRange)
    {
        this.explosionRange = explosionRange;
    }

    protected override bool Trigger(bool rightLeg)
    {
        if(!base.Trigger(rightLeg)) return false;
        GameObject explosion = PoolManager.Spawn(ResourceEnum.Prefab.Explosion, transform.position);
        explosion.transform.localScale = new(explosionRange / 2f, explosionRange / 2f);
        victim.TakeDamage(this, damage, rightLeg ? 1 : 2);
        var hits = Physics2D.OverlapCircleAll(transform.position, explosionRange, LayerMask.GetMask("Survivor"));
        foreach (var hit in hits)
        {
            if(!hit.isTrigger && hit.TryGetComponent(out Survivor splashedSurvivor))
            {
                if (splashedSurvivor == victim) continue;
                Vector2 closestPoint = hit.ClosestPoint(transform.position);
                float distance = Mathf.Max(Vector2.Distance(transform.position, closestPoint), 1);
                splashedSurvivor.TakeDamage(this, damage / (distance * distance), rightLeg ? 1 : 2);
            }
        }
        return true;
    }
}

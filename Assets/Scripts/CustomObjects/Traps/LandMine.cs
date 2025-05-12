using UnityEngine;

public class LandMine : Trap
{
    [SerializeField] float explosionRange = 2;
    protected override void Trigger(bool rightLeg)
    {
        base.Trigger(rightLeg);
        victim.TakeDamage(this, rightLeg ? InjurySite.RightAncle : InjurySite.LeftAncle);
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
    }
}

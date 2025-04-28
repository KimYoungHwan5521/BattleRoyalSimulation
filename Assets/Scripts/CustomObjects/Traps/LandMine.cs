using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : Trap
{
    [SerializeField] float explosionRange = 2;
    protected override void Trigger()
    {
        base.Trigger();
        victim.TakeDamage(this);
        var hits = Physics2D.CircleCastAll(transform.position, explosionRange, Vector2.up);
        foreach (var hit in hits)
        {
            if(hit.rigidbody.TryGetComponent(out Survivor splashedSurvivor))
            {
                if (splashedSurvivor == victim) continue;
                float distance = Mathf.Max(Vector2.Distance(transform.position, hit.point), 1);
                splashedSurvivor.TakeDamage(this, damage / (distance * distance));
            }
        }
    }
}

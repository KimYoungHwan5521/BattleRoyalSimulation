using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : Trap
{
    protected override void Trigger()
    {
        base.Trigger();
        victim.TakeDamage(this);
    }
}

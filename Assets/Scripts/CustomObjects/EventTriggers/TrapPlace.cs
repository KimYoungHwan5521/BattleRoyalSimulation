using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPlace : CustomObject
{
    [SerializeField] Trap buriedTrap;
    public Trap BuriedTrap => buriedTrap;

    public void SetTrap(Trap trap)
    {
        buriedTrap = trap;
        if(trap != null) trap.ownerPlace = this;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Survivor survivor) && !collision.isTrigger)
        {
            survivor.trapPlace = this;
        }
    }
}

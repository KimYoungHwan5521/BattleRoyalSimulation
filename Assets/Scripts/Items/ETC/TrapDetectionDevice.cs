using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDetectionDevice : MonoBehaviour
{
    [SerializeField] Survivor owner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Box box))
        {
            if(box.items.Find(x => x is BoobyTrap) != null) owner.DetectTrap(box);
        }
        else if(collision.TryGetComponent(out Trap trap))
        {
            owner.DetectTrap(trap);
        }
    }
}

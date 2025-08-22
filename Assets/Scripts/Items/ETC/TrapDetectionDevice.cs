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
            Item item = box.items.Find(x => x is BoobyTrap);
            if (item != null)
            {
                if(((BoobyTrap)item).Setter != owner) owner.DetectTrap(box);
            }
        }
        else if(collision.TryGetComponent(out Trap trap))
        {
            if(trap.setter != owner) owner.DetectTrap(trap);
        }
    }
}

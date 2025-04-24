using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : CustomObject
{
    Animator animator;
    public Survivor setter;
    protected Survivor victim;
    [SerializeField] public TrapPlace ownerPlace;
    [SerializeField] protected float damage;
    [SerializeField] protected DamageType damageType;
    [SerializeField] protected bool isBuriedType;

    public float Damage => damage;
    public DamageType DamageType => damageType;
    public bool IsBuriedType => isBuriedType;

    override protected void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected virtual void Trigger()
    {
        if(animator != null) animator.SetTrigger("Triggered");
        ownerPlace.SetTrap(null);
        ownerPlace = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent(out Survivor victim) && !collision.isTrigger)
        {
            if (victim != setter)
            {
                this.victim = victim;
                Trigger();
            }
        }
    }
}

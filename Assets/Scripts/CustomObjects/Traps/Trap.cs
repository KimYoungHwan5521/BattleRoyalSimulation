using UnityEngine;

public class Trap : CustomObject
{
    Animator animator;
    public Survivor setter;
    protected Survivor victim;
    public TrapPlace ownerPlace;
    [SerializeField] ItemManager.Items itemType;
    public ItemManager.Items ItemType => itemType;
    [SerializeField] protected float damage;
    [SerializeField] protected DamageType damageType;
    [SerializeField] protected bool isBuriedType;
    [SerializeField] float disarmTime;
    public float DisarmTime => disarmTime;

    public float Damage => damage;
    public DamageType DamageType => damageType;
    public bool IsBuriedType => isBuriedType;

    bool isEnchanted;
    public bool IsEnchanted => isEnchanted;

    public void Enchant()
    {
        isEnchanted = true;
    }

    override protected void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected virtual void Trigger(bool rightLeg)
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
                Vector2 foward = collision.transform.up;
                Vector2 toContact = (Vector2)transform.position - (Vector2)collision.transform.position;
                // 외적을 통해 왼발로 밟았는지 오른발로 밟았는지 판별
                float cross = foward.x * toContact.y - foward.y * toContact.x;
                Trigger(cross < 0);
            }
        }
    }
}

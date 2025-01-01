using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor : CustomObject
{
    [SerializeField] CircleCollider2D recognizeCollider;
    Animator animator;

    [SerializeField] bool debug;
    bool isDead;
    public bool IsDead 
    { 
        get { return isDead; }
        set 
        { 
            isDead = value;
            if(isDead) animator.SetBool("Attack", false);
        }
    }
    [SerializeField] float maxHP;
    [SerializeField] float curHP;
    public float CurHP => curHP;
    [SerializeField] float attakDamage;
    [SerializeField] float attackSpeed;
    [SerializeField] float attackRange;
    [SerializeField] float moveSpeed;
    [SerializeField] float detectionRange;
    [SerializeField] Vector2 moveDirection = Vector2.up;
    [SerializeField] Vector2 lookRotation = Vector2.up;
    [SerializeField] Vector2 rememberOriginalMoveDirection = Vector2.up;

    [SerializeField] List<Survivor> enemies = new();
    [SerializeField] List<Item> items = new();
    [SerializeField] Weapon currentWeapon;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        recognizeCollider.radius = detectionRange;
        curHP = maxHP;
    }
    private void Update()
    {
        if(isDead) return;
        AI();
    }

    private void FixedUpdate()
    {
        if(isDead) return;
        Move(moveDirection, Time.fixedDeltaTime);
        Look(lookRotation);
    }

    void Move(Vector2 preferDirection, float deltaTime)
    {
        preferDirection.Normalize();
        transform.position += deltaTime * moveSpeed * (Vector3)preferDirection;
    }

    void Look(Vector2 preferDirection)
    {
        float angle = Mathf.Atan2(preferDirection.y, preferDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    void AI()
    {
        if (enemies.Count == 0)
        {

            Explore();
        }
        else Combat(enemies[0]);

        //if (debug) Debug.Log(ObstaclesCheck());
        if(ObstaclesCheck())
        {
            AvoidObstacles();
        }
        else
        {
            moveDirection = rememberOriginalMoveDirection;
        }
    }

    bool ObstaclesCheck()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(transform.position, rememberOriginalMoveDirection.normalized, 2f);
        Debug.DrawRay(transform.position, rememberOriginalMoveDirection.normalized * 2f, Color.red);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag;
            if (hitTag == "Wall")
            {
                return true;
            }
        }
        return false;
    }

    void AvoidObstacles()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(transform.position, moveDirection.normalized, 2f);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag; 
            if (hitTag == "Wall")
            {
                if (Vector2.Angle(moveDirection.Rotate(-15), hit.normal) < Vector2.Angle(moveDirection.Rotate(15), hit.normal))
                {
                    moveDirection = moveDirection.Rotate(-15);
                }
                else
                {
                    moveDirection = moveDirection.Rotate(15);
                }
                lookRotation = moveDirection;
                return;
            }
        }
    }

    void Explore()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(transform.position, rememberOriginalMoveDirection.normalized, detectionRange);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag;
            if (hitTag == "Ground")
            {
                if (Random.Range(0, 3) < 2)
                {
                    rememberOriginalMoveDirection = rememberOriginalMoveDirection.Rotate(90);
                }
                else
                {
                    rememberOriginalMoveDirection = rememberOriginalMoveDirection.Rotate(135);
                }
                lookRotation = rememberOriginalMoveDirection;
                return;
            }
        }
    }

    void Combat(Survivor target)
    {
        lookRotation = target.transform.position - transform.position;
        if (Vector2.Distance(transform.position, target.transform.position) > attackRange)
        {
            animator.SetBool("Attack", false);
            rememberOriginalMoveDirection = lookRotation;
        }
        else
        {
            Attack(target);
        }
    }

    void Attack(Survivor target)
    {
        moveDirection = Vector2.zero;
        rememberOriginalMoveDirection = Vector2.zero;
        animator.SetBool("Attack", true);
    }

    public void TakeDamage(Survivor attacker, float damage)
    {
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            IsDead = true;
        }
    }

    void AttackAE()
    {
        if (enemies.Count == 0) return;
        if (Vector2.Distance(transform.position, enemies[0].transform.position) < attackRange)
        {
            enemies[0].TakeDamage(this, attakDamage);
            if (enemies[0].isDead) enemies.Remove(enemies[0]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.TryGetComponent(out Survivor survivor) && !survivor.isDead)
        {
            enemies.Add(survivor);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.TryGetComponent(out Survivor survivor))
        {
            if(enemies.Contains(survivor)) enemies.Remove(survivor);
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

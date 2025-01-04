using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survivor : CustomObject
{
    enum Status{ Farming, InCombat }

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
    [SerializeField] Status currentStatus;
    [SerializeField] float maxHP = 100;
    [SerializeField] float curHP;
    public float CurHP => curHP;
    [SerializeField] float attakDamage = 10f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] float farmingSpeed = 1f;

    [SerializeField] Vector2 moveDirection = Vector2.up;
    [SerializeField] Vector2 lookRotation = Vector2.up;
    [SerializeField] Vector2 rememberOriginalMoveDirection = Vector2.up;

    [SerializeField] Weapon currentWeapon;
    [SerializeField] List<Survivor> enemies = new();
    [SerializeField] List<Item> inventory = new();

    // value : Had finished farming?
    [SerializeField] Dictionary<FarmingSection, bool> farmingSections = new();
    [SerializeField] FarmingSection targetFarmingSection;
    [SerializeField] Dictionary<Box, bool> farmingBoxes;
    [SerializeField] Box targetFarmingBox;
    [SerializeField] float farmingTime = 3f;
    [SerializeField] float curFarmingTime;

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
        Move(moveDirection);
        Look(lookRotation);
    }

    void Move(Vector2 preferDirection)
    {
        preferDirection.Normalize();
        transform.position += Time.fixedDeltaTime * moveSpeed * (Vector3)preferDirection;
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
            currentStatus = Status.Farming;
            if(targetFarmingSection != null)
            {
                FarmingSection();
            }
            else
            {
                Explore();
                CheckFarmingSection();
            }
        }
        else
        {
            currentStatus = Status.InCombat;
            Combat(enemies[0]);
        }

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

    void CheckFarmingSection()
    {
        FarmingSection nearestFarmingSection = null;
        float minDistance = float.MaxValue;
        float distance;
        foreach (KeyValuePair<FarmingSection, bool> farmingCandidate in farmingSections)
        {
            if (!farmingCandidate.Value)
            {
                distance = Vector2.Distance(transform.position, farmingCandidate.Key.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestFarmingSection = farmingCandidate.Key;
                }
            }
        }
        if (nearestFarmingSection != null)
        {
            farmingBoxes = new();
            foreach(Box box in nearestFarmingSection.boxes)
            {
                farmingBoxes.Add(box, false);
            }
        }
        targetFarmingSection = nearestFarmingSection;
    }

    void FarmingSection()
    {
        if(targetFarmingBox == null)
        {
            Box nearestBox = null;
            float minDistance = float.MaxValue;
            float distance;
            foreach (KeyValuePair<Box, bool> box in farmingBoxes)
            {
                if(!box.Value)
                {
                    distance = Vector2.Distance(transform.position, box.Key.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestBox = box.Key;
                    }
                }
            }
            if(nearestBox == null)
            {
                farmingSections[targetFarmingSection] = true;
                targetFarmingSection = null;
                return;
            }
            targetFarmingBox = nearestBox;
        }
        else
        {
            FarmingBox();
        }
    }

    void FarmingBox()
    {
        if(Vector2.Distance(transform.position, targetFarmingBox.transform.position) < 1.5f)
        {
            moveDirection = Vector2.zero;
            rememberOriginalMoveDirection = Vector2.zero;

            curFarmingTime += Time.deltaTime * farmingSpeed;
            if(curFarmingTime > farmingTime)
            {
                foreach (Item item in targetFarmingBox.Items)
                    AcqireItem(item);
                targetFarmingBox.Items.Clear();
                farmingBoxes[targetFarmingBox] = true;
                targetFarmingBox = null;
                curFarmingTime = 0;
            }
        }
        else
        {
            rememberOriginalMoveDirection = targetFarmingBox.transform.position - transform.position;
            lookRotation = rememberOriginalMoveDirection;
        }
    }

    void AcqireItem(Item item)
    {
        Debug.Log(item.itemName);
        if(item is Weapon)
        {
            Weapon weapon = item as Weapon;
            if (currentWeapon != null)
            {
                inventory.Add(currentWeapon);
            }
            currentWeapon = weapon;
        }
        else inventory.Add(item);
    }

    bool ObstaclesCheck()
    {
        RaycastHit2D[] hits;
        hits = Physics2D.RaycastAll(transform.position, rememberOriginalMoveDirection.normalized, 10f);
        Debug.DrawRay(transform.position, rememberOriginalMoveDirection.normalized * 2f, Color.red);
        foreach (RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.gameObject.tag;
            if (hitTag == "Wall" || hitTag == "Box")
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
        foreach(RaycastHit2D hit in hits)
        {
            string hitTag = hit.collider.tag;
            if(hitTag == "Wall" || hitTag == "Ground" || hitTag == "Box")
            {
                //if (Vector2.Angle(moveDirection.Rotate(-15), hit.normal) < Vector2.Angle(moveDirection.Rotate(15), hit.normal))
                //{
                //    moveDirection = moveDirection.Rotate(-15);
                //}
                //else
                //{
                //    moveDirection = moveDirection.Rotate(15);
                //}
                moveDirection = moveDirection.Rotate(15);
            }

        }
        lookRotation = moveDirection;
    }

    void Explore()
    {
        if (rememberOriginalMoveDirection == Vector2.zero)
        {
            rememberOriginalMoveDirection = Vector2.up;
            lookRotation = rememberOriginalMoveDirection;
        }
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
        animator.SetFloat("AttackSpeed", attackSpeed);
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
        if (!collision.isTrigger)
        {
            if (collision.TryGetComponent(out Survivor survivor) && !survivor.isDead)
            {
                RaycastHit2D hit;
                hit = Physics2D.Linecast(transform.position, survivor.transform.position, LayerMask.GetMask("Wall"));
                if(hit.collider == null)
                    enemies.Add(survivor);
            }
        }
        else
        {
            if(collision.TryGetComponent(out FarmingSection farmingSection))
            {
                if(!farmingSections.ContainsKey(farmingSection))
                {
                    farmingSections.Add(farmingSection, false);
                }
            }
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

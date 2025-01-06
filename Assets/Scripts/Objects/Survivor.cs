using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Survivor : CustomObject
{
    public enum Status{ Farming, InCombat }

    [SerializeField] CircleCollider2D recognizeCollider;
    Animator animator;
    NavMeshAgent agent;

    [SerializeField] bool debug;
    [SerializeField] bool isDead;
    public bool IsDead 
    { 
        get { return isDead; }
        set 
        { 
            isDead = value;
            if(isDead) animator.SetBool("Attack", false);
        }
    }
    [SerializeField] public Status currentStatus;
    [SerializeField] float maxHP = 100;
    [SerializeField] float curHP;
    public float CurHP => curHP;
    [SerializeField] float attakDamage = 10f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] float farmingSpeed = 1f;

    [SerializeField] Vector2 lookRotation = Vector2.zero;

    [SerializeField] Weapon currentWeapon = null;
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
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        recognizeCollider.radius = detectionRange;
        curHP = maxHP;
        agent.speed = moveSpeed;
    }
    private void Update()
    {
        if(isDead) return;
        AI();
    }

    Vector2 lastPos;
    private void FixedUpdate()
    {
        if(isDead) return;
        if(lookRotation != Vector2.zero)
        {
            Look(lookRotation);
        }
        else
        {
            Look((Vector2)transform.position - lastPos);
        }
        lastPos = transform.position;
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
            lookRotation = Vector2.zero;
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
            agent.SetDestination(transform.position);

            curFarmingTime += Time.deltaTime * farmingSpeed;
            if (curFarmingTime > farmingTime)
            {
                foreach (Item item in targetFarmingBox.Items)
                    AcqireItem(item);
                targetFarmingBox.Items.Clear();
                farmingBoxes[targetFarmingBox] = true;
                targetFarmingBox = null;
                lookRotation = Vector2.zero;
                curFarmingTime = 0;
            }
            else lookRotation = targetFarmingBox.transform.position - transform.position;
        }
        else
        {
            agent.SetDestination(targetFarmingBox.transform.position);
        }
    }

    void AcqireItem(Item item)
    {
        if (item is Weapon)
        {
            Weapon weapon = item as Weapon;
            Equip(weapon);
        }
        else inventory.Add(item);
    }

    void Equip(Weapon wantWeapon)
    {
        if (currentWeapon.IsValid())
        {
            inventory.Add(currentWeapon);
            Transform curWeaponTF = transform.Find("Right Hand").Find($"{currentWeapon.itemName}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Can't find weapon : {currentWeapon.itemName}");
            }
        }
        if(wantWeapon.IsValid())
        {
            Transform weaponTF = null;
            // Active가 꺼져있는 오브젝트는 Find로 찾을 수 없다.
            foreach (Transform child in transform.Find("Right Hand"))
            {
                if (child.name == $"{wantWeapon.itemName}")
                {
                    weaponTF = child; // 이름이 일치하는 자식 반환
                }
            }
            if (weaponTF != null)
            {
                weaponTF.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Can't find weapon : {wantWeapon.itemName}");
            }
            currentWeapon = wantWeapon;
        }
    }

    void Explore()
    {
        if(Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            agent.SetDestination(new Vector2(Random.Range(-20, 20), Random.Range(-20, 20)));
        }
    }

    void Combat(Survivor target)
    {
        lookRotation = target.transform.position - transform.position;
        if (Vector2.Distance(transform.position, target.transform.position) > attackRange)
        {
            animator.SetBool("Attack", false);
            agent.SetDestination(target.transform.position);
        }
        else
        {
            Attack(target);
        }
    }

    void Attack(Survivor target)
    {
        agent.SetDestination(transform.position);
        animator.SetBool("Attack", true);
        animator.SetFloat("AttackSpeed", attackSpeed);
    }

    public void TakeDamage(Survivor attacker, float damage)
    {
        float probability = Random.Range(0, 1f);
        if(probability < 0.2f)
        {
            // 회피
            return;
        }
        else if(probability < 0.5f)
        {
            // 방어
            damage *= 0.5f;
        }
        else if(probability > 0.9f)
        {
            // 치명타
            damage *= 2;
        }
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
        if(currentWeapon.IsValid())
        {
            if (Vector2.Distance(transform.position, enemies[0].transform.position) < currentWeapon.attackRange)
            {
                enemies[0].TakeDamage(this, currentWeapon.attakDamage);
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, enemies[0].transform.position) < attackRange)
            {
                enemies[0].TakeDamage(this, attakDamage);
            }   
        }
        if (enemies[0].isDead)
        {
            enemies.Remove(enemies[0]);
            animator.SetBool("Attack", false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            if (collision.TryGetComponent(out Survivor survivor) && !survivor.isDead)
            {
                RaycastHit2D hit;
                hit = Physics2D.Linecast(transform.position, survivor.transform.position, LayerMask.GetMask("Wall"));
                if(hit.collider == null && !enemies.Contains(survivor))
                {
                    enemies.Add(survivor);
                }
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

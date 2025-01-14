using System;
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
            agent.SetDestination(transform.position);
            if(isDead) animator.SetBool("Attack", false);
            if(isDead) animator.SetBool("Aim", false);
            if(isDead) animator.SetBool("Reload", false);
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
    public Weapon CurrentWeapon => currentWeapon;
    [SerializeField] RangedWeapon CurrentWeaponAsRangedWeapon
    {
        get
        {
            if (currentWeapon is RangedWeapon)
            {
                return currentWeapon as RangedWeapon;
            }
            return null;
        }
    }
    ProjectileGenerator projectileGenerator;

    [SerializeField] List<Survivor> enemies = new();
    public Survivor targetEnemy => enemies[0];
    [SerializeField] List<Item> inventory = new();
    Item ValidBullet
    {
        get
        {
            int index = inventory.FindIndex(x => x.itemName == $"Bullet({currentWeapon.itemName})");
            if(index > -1)
            {
                if (inventory[index].amount > 0)
                {
                    return inventory[index];
                }
            }
            return null;
        }
    }

    // value : Had finished farming?
    [SerializeField] Dictionary<FarmingSection, bool> farmingSections = new();
    [SerializeField] FarmingSection targetFarmingSection;
    [SerializeField] Dictionary<Box, bool> farmingBoxes;
    [SerializeField] Box targetFarmingBox;
    [SerializeField] Dictionary<Survivor, bool> farmingCorpses = new();
    [SerializeField] Survivor targetFarmingCorpse;
    [SerializeField] float farmingTime = 3f;
    [SerializeField] float curFarmingTime;
    [SerializeField] float curShotTime;
    //[SerializeField] float curReloadTime;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        projectileGenerator = GetComponent<ProjectileGenerator>();

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
            animator.SetBool("Attack", false);
            animator.SetBool("Aim", false);
            if (CurrentWeaponAsRangedWeapon != null)
            {
                if(projectileGenerator.muzzleTF == null) projectileGenerator.ResetMuzzleTF();
                if (CurrentWeaponAsRangedWeapon.CurrentMagazine < CurrentWeaponAsRangedWeapon.MagazineCapacity && ValidBullet != null)
                {
                    Reload();
                    return;
                }
            }
            animator.SetBool("Reload", false);

            lookRotation = Vector2.zero;
            currentStatus = Status.Farming;
            if(targetFarmingCorpse != null)
            {
                FarmingCorpse();
            }
            else if(targetFarmingSection != null)
            {
                FarmingSection();
            }
            else
            {
                Explore();
                CheckFarmingTarget();
            }
        }
        else
        {
            currentStatus = Status.InCombat;
            if (enemies[0].isDead)
            {
                if (!farmingCorpses.ContainsKey(enemies[0]))
                {
                    farmingCorpses.Add(enemies[0], false);
                    targetFarmingCorpse = enemies[0];
                }
                enemies.Remove(enemies[0]);
            }
            else
            {
                Combat(enemies[0]);
            }
        }
    }

    void CheckFarmingTarget()
    {
        if(!CheckFarmingCorpse()) CheckFarmingSection();
    }

    bool CheckFarmingCorpse()
    {
        foreach(KeyValuePair<Survivor, bool> corpse in farmingCorpses)
        {
            if(!corpse.Value)
            {
                targetFarmingCorpse = corpse.Key;
                return true;
            }
        }
        return false;
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

    void FarmingCorpse()
    {
        if (Vector2.Distance(transform.position, targetFarmingCorpse.transform.position) < 1.5f)
        {
            agent.SetDestination(transform.position);

            curFarmingTime += Time.deltaTime * farmingSpeed;
            if (curFarmingTime > farmingTime)
            {
                if(targetFarmingCorpse.currentWeapon.IsValid())
                {
                    AcqireItem(targetFarmingCorpse.currentWeapon);
                    targetFarmingCorpse.Unequip();
                }
                foreach (Item item in targetFarmingCorpse.inventory)
                    AcqireItem(item);
                targetFarmingCorpse.inventory.Clear();
                farmingCorpses[targetFarmingCorpse] = true;
                targetFarmingCorpse = null;
                lookRotation = Vector2.zero;
                curFarmingTime = 0;
            }
            else lookRotation = targetFarmingCorpse.transform.position - transform.position;
        }
        else
        {
            agent.SetDestination(targetFarmingCorpse.transform.position);
        }
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
            Weapon newWeapon = item as Weapon;
            if(CompareWeaponValue(newWeapon))
            {
                Equip(newWeapon);
            }
            else
            {
                inventory.Add(item);
            }
        }
        else if(item.itemName.Contains("Bullet"))
        {
            string wantWeapon = item.itemName.Split('(')[0].Split(')')[0];
            RangedWeapon weapon = inventory.Find(x => x.itemName == wantWeapon) as RangedWeapon;
            if(weapon != null && CompareWeaponValue(weapon))
            {
                Equip(weapon);
            }
            else
            {
                inventory.Add(item);
            }
        }
        else
        {
            inventory.Add(item);
        }
    }

    void ConsumptionItem(Item item, int amount)
    {
        int index = inventory.IndexOf(item);
        if(index > -1)
        {
            inventory[index].amount -= amount;
            if(inventory[index].amount == 0)
            {
                inventory.Remove(item);
            }
        }
    }

    bool CompareWeaponValue(Weapon newWeapon)
    {
        if(!currentWeapon.IsValid()) return true;
        else if (currentWeapon is MeleeWeapon)
        {
            if (newWeapon is RangedWeapon)
            {
                // �� vs ��
                RangedWeapon newWeaponAsRangedWeapon = newWeapon as RangedWeapon;
                if (newWeaponAsRangedWeapon.MagazineCapacity > 0) return true;
                else
                {
                    Item bullet = inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})");
                    if (bullet != null) return true;
                    else return false;
                }
            }
            else
            {
                // �� vs ��
                if (newWeapon.attakDamage > currentWeapon.attakDamage) return true;
                else return false;
            }
        }
        else
        {
            if(newWeapon is MeleeWeapon)
            {
                // �� vs ��
                if (ValidBullet != null || CurrentWeaponAsRangedWeapon.MagazineCapacity > 0) return true;
                else return false;
            }
            else
            {
                // �� vs ��
                RangedWeapon newWeaponAsRangedWeapon = newWeapon as RangedWeapon;
                Item bullet = inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})");
                if (newWeaponAsRangedWeapon.MagazineCapacity > 0 || bullet != null)
                {
                    // �� �� �Ѿ��� �ִ� ���
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.MagazineCapacity > 0)
                    {
                        if (newWeapon.attackRange > currentWeapon.attackRange) return true;
                        else return false;
                    }
                    else return true;
                }
                else
                {
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.MagazineCapacity > 0) return false;
                    else
                    {
                        // �� �� �Ѿ��� ���� ���
                        if (newWeapon.attackRange > currentWeapon.attackRange) return true;
                        else return false;
                    }
                }
            }
        }
    }

    void Equip(Weapon wantWeapon)
    {
        // ���� �ִ� ���Ⱑ ������ ����
        Unequip();

        // �� ���� ����
        if(wantWeapon.IsValid())
        {
            Transform weaponTF = null;
            // Active�� �����ִ� ������Ʈ�� Find�� ã�� �� ����.
            foreach (Transform child in transform.Find("Right Hand"))
            {
                if (child.name == $"{wantWeapon.itemName}")
                {
                    weaponTF = child; // �̸��� ��ġ�ϴ� �ڽ� ��ȯ
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
            animator.SetInteger("AnimNumber", currentWeapon.AttackAnimNumber);
            if(currentWeapon is RangedWeapon) animator.SetInteger("ShotAnimNumber", CurrentWeaponAsRangedWeapon.ShotAnimNumber);
        }
    }

    void Unequip()
    {
        if (currentWeapon.IsValid())
        {
            inventory.Add(currentWeapon);
            Transform curWeaponTF = transform.Find("Right Hand").Find($"{currentWeapon.itemName}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
                projectileGenerator.muzzleTF = null;
            }
            else
            {
                Debug.LogWarning($"Can't find weapon : {currentWeapon.itemName}");
            }
        }
    }

    void Explore()
    {
        if(Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            agent.SetDestination(new Vector2(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20)));
        }
    }

    void Combat(Survivor target)
    {
        lookRotation = target.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, enemies[0].transform.position);
        if (currentWeapon.IsValid())
        {
            if(CurrentWeaponAsRangedWeapon != null)
            {
                if (CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
                {
                    if (distance < CurrentWeaponAsRangedWeapon.attackRange)
                    {
                        Aim();
                        return;
                    }
                }
                else if(ValidBullet != null && distance > CurrentWeaponAsRangedWeapon.MinimumRange)
                {
                    Reload();
                    return;
                }
                else
                {

                    if(distance < attackRange)
                    {
                        Attack();
                        return;
                    }
                }
            }
            else
            {
                if (distance < currentWeapon.attackRange)
                {
                    Attack();
                    return;
                }
            }
        }
        else if (distance < attackRange)
        {
            Attack();
            return;
        }
        animator.SetBool("Attack", false);
        animator.SetBool("Aim", false);
        animator.SetBool("Reload", false);
        agent.SetDestination(target.transform.position);
    }

    void Attack()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("Aim", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Attack", true);
        if(currentWeapon.IsValid())
        {
            animator.SetInteger("AnimNumber", currentWeapon.AttackAnimNumber);
        }
        else
        {
            animator.SetInteger("AnimNumber", 0);
        }
        animator.SetFloat("AttackSpeed", attackSpeed);
    }

    void Aim()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("Attack", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Aim", true);

        curShotTime += Time.deltaTime;
        if(curShotTime > CurrentWeaponAsRangedWeapon.ShotCoolTime)
        {
            animator.SetInteger("ShotAnimNumber", CurrentWeaponAsRangedWeapon.ShotAnimNumber);
            animator.SetTrigger("Fire");
            curShotTime = 0;
        }
    }

    public void TakeDamage(Survivor attacker, float damage)
    {
        float probability = UnityEngine.Random.Range(0, 1f);
        if(probability < 0.2f)
        {
            // ȸ��
            return;
        }
        else if(probability < 0.5f)
        {
            // ���
            damage *= 0.5f;
        }
        else if(probability > 0.9f)
        {
            // ġ��Ÿ
            damage *= 2;
        }

        if (damage < 0) damage = 0;
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            IsDead = true;
        }
    }

    public void TakeDamage(Bullet bullet)
    {
        float damage;
        float probability = UnityEngine.Random.Range(0, 1f);
        // ��弦
        if(probability > 0.99f)
        {
            damage = 1000;
        }
        // �ٵ�
        else if(probability > 0.3f)
        {
            damage = bullet.Damage * 3;
        }
        else
        {
            damage = bullet.Damage;
        }
        // ��ȿ ��Ÿ� ��
        if(bullet.TraveledDistance > bullet.MaxRange * 0.5f)
        {
            damage *= (bullet.MaxRange * 1.5f - bullet.TraveledDistance) / bullet.MaxRange;
        }

        if(damage < 0) damage = 0;
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            IsDead = true;
        }
    }

    void Reload()
    {
        animator.SetBool("Attack", false);
        agent.SetDestination(transform.position);
        animator.SetBool("Reload", true);
    }

    void AE_Attack()
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
    }

    void AE_Reload()
    {
        int amount;
        if (currentWeapon.itemName == "ShotGun") amount = 1;
        else amount = Math.Clamp(ValidBullet.amount, 1, CurrentWeaponAsRangedWeapon.MagazineCapacity - CurrentWeaponAsRangedWeapon.CurrentMagazine);
        ConsumptionItem(ValidBullet, amount);
        CurrentWeaponAsRangedWeapon.Reload(amount);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(isDead) return;
        if (!collision.isTrigger)
        {
            if (collision.TryGetComponent(out Survivor survivor))
            {
                RaycastHit2D hit;
                hit = Physics2D.Linecast(transform.position, survivor.transform.position, LayerMask.GetMask("Wall"));
                if(hit.collider == null)
                {
                    if(survivor.isDead)
                    {
                        if(!farmingCorpses.ContainsKey(survivor))
                        {
                            farmingCorpses.Add(survivor, false);
                        }
                    }
                    else
                    {
                        if(!enemies.Contains(survivor))
                        {
                            enemies.Add(survivor);
                        }
                    }
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

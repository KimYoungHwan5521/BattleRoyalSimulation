using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Survivor : CustomObject
{
    public enum Status { Farming, InCombat }

    [SerializeField] CircleCollider2D recognizeCollider;
    [SerializeField] CircleCollider2D bodyCollider;
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
            agent.enabled = false;
            if (isDead) animator.SetTrigger("Dead");
            bodyCollider.enabled = false;
        }
    }
    public string survivorName;
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
    public Survivor TargetEnemy 
    { 
        get
        {
            if (enemies.Count == 0) return null;
            else return enemies[0];
        }
    }

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
    [SerializeField] public Dictionary<Area, bool> farmingAreas = new();
    [SerializeField] Area currentFarmingArea;
    [SerializeField] public Area CurrentFarmingArea
    {
        get => currentFarmingArea;
        set
        {
            currentFarmingArea = value;
            foreach(FarmingSection farmingSection in value.farmingSections)
            {
                if(!farmingSections.ContainsKey(farmingSection)) farmingSections.Add(farmingSection, false);
            }
        }
    }
    [SerializeField] Dictionary<FarmingSection, bool> farmingSections = new();
    [SerializeField] FarmingSection targetFarmingSection;
    [SerializeField] Dictionary<Box, bool> farmingBoxes = new();
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

    override protected void MyUpdate()
    {
        if(isDead) return;
        AI();
    }

    private void FixedUpdate()
    {
        if(isDead) return;
        if(lookRotation != Vector2.zero)
        {
            Look(lookRotation);
        }
        else
        {
            Look((Vector2)agent.velocity);
        }
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
                CheckFarmingTarget();
                Explore();
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
        FarmingSection nearestFarmingSection = FindNearest(farmingSections);
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

    TKey FindNearest<TKey>(Dictionary<TKey, bool> candidates) where TKey : MonoBehaviour
    {
        TKey nearest = default;
        float minDistance = float.MaxValue;
        float distance;
        foreach (KeyValuePair<TKey, bool> candidate in candidates)
        {
            if (typeof(TKey) == typeof(Area))
            {
                Area area = candidate.Key as Area;
                if (area.IsProhibited) continue;
            }
            if (!candidate.Value)
            {
                distance = Vector2.Distance(transform.position, candidate.Key.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = candidate.Key;
                }
            }
        }
        return nearest;
    }

    void CheckAreaClear()
    {
        bool farmingSectionLeft = false;
        foreach (FarmingSection farminSection in currentFarmingArea.farmingSections)
        {
            if (!farmingSections[farminSection])
            {
                farmingSectionLeft = true;
                return;
            }
        }
        if (!farmingSectionLeft) farmingAreas[currentFarmingArea] = true;
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
            Box nearestBox = FindNearest(farmingBoxes);
            if(nearestBox == null)
            {
                farmingSections[targetFarmingSection] = true;
                targetFarmingSection = null;
                CheckAreaClear();
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

    bool noMoreFarmingArea;
    void Explore()
    {
        if (!farmingAreas[currentFarmingArea]) return;
        if (Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            if (!noMoreFarmingArea)
            {
                foreach (Area farmingArea in currentFarmingArea.adjacentAreas)
                {
                    if (!farmingArea.IsProhibited && !farmingAreas[farmingArea])
                    {
                        CurrentFarmingArea = farmingArea;
                        return;
                    }
                }
                Area area = FindNearest(farmingAreas);
                if (area != null)
                {
                    CurrentFarmingArea = area;
                    return;
                }
                else noMoreFarmingArea = true;
            }

            Vector2 wantPosition = transform.position;
            wantPosition = new(
                currentFarmingArea.transform.position.x + UnityEngine.Random.Range(-currentFarmingArea.transform.localScale.x * 0.5f, currentFarmingArea.transform.localScale.x * 0.5f),
                currentFarmingArea.transform.position.y + UnityEngine.Random.Range(-currentFarmingArea.transform.localScale.y * 0.5f, currentFarmingArea.transform.localScale.y * 0.5f)
                );

            agent.SetDestination(wantPosition);
        }
    }

    public void LeaveCurrentArea()
    {
        farmingAreas[currentFarmingArea] = true;
        foreach (FarmingSection farmingSection in currentFarmingArea.farmingSections)
        {
            if (farmingSections.ContainsKey(farmingSection)) farmingSections[farmingSection] = true;
            foreach (Box box in farmingSection.boxes)
            {
                if(farmingBoxes.ContainsKey(box)) farmingBoxes[box] = true;
            }
        }
        targetFarmingBox = null;
        targetFarmingSection = null;
        CurrentFarmingArea = FindNearest(farmingAreas);
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
        if(newWeapon.itemName == currentWeapon.itemName) return false;
        
        if (currentWeapon is MeleeWeapon)
        {
            if (newWeapon is RangedWeapon)
            {
                // 근 vs 원
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
                // 근 vs 근
                if (newWeapon.attakDamage > currentWeapon.attakDamage) return true;
                else return false;
            }
        }
        else
        {
            if(newWeapon is MeleeWeapon)
            {
                // 원 vs 근
                if (ValidBullet != null || CurrentWeaponAsRangedWeapon.MagazineCapacity > 0) return true;
                else return false;
            }
            else
            {
                // 원 vs 원
                RangedWeapon newWeaponAsRangedWeapon = newWeapon as RangedWeapon;
                Item bullet = inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})");
                if (newWeaponAsRangedWeapon.MagazineCapacity > 0 || bullet != null)
                {
                    // 둘 다 총알이 있는 경우
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
                        // 둘 다 총알이 없는 경우
                        if (newWeapon.attackRange > currentWeapon.attackRange) return true;
                        else return false;
                    }
                }
            }
        }
    }

    void Equip(Weapon wantWeapon)
    {
        // 차고 있는 무기가 있으면 놓고
        Unequip();

        // 새 무기 차기
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

        if (damage < 0) damage = 0;
        curHP -= damage;
        if (curHP <= 0)
        {
            curHP = 0;
            IsDead = true;
        }

        if(enemies.Contains(attacker))
        {
            if(attacker != enemies[0])
            {
                enemies.Remove(attacker);
                enemies.Insert(0, attacker);
            }
        }
        else
        {
            enemies.Insert(0, attacker);
        }
    }

    public void TakeDamage(Bullet bullet)
    {
        if (isDead) return;
        float damage;
        float probability = UnityEngine.Random.Range(0, 1f);
        // 헤드샷
        if(probability > 0.99f)
        {
            damage = 1000;
        }
        // 바디샷
        else if(probability > 0.3f)
        {
            damage = bullet.Damage * 3;
        }
        else
        {
            damage = bullet.Damage;
        }
        // 실효 사거리 밖
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
        //else
        //{
        //    if(collision.TryGetComponent(out FarmingSection farmingSection))
        //    {
        //        if(!farmingSections.ContainsKey(farmingSection))
        //        {
        //            farmingSections.Add(farmingSection, false);
        //        }
        //    }
        //}
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
        if(isDead) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

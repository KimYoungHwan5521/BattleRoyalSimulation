using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Survivor : CustomObject
{
    #region Variables and Properties
    public enum Status { Farming, InCombat, TraceEnemy, InvestigateThreateningSound, Maintain }

    [Header("Components")]
    [SerializeField] PolygonCollider2D sightCollider;
    [SerializeField] CircleCollider2D bodyCollider;
    [SerializeField] SpriteRenderer[] bodySprites;
    Animator animator;
    NavMeshAgent agent;

    [SerializeField] MeshFilter sightMeshFilter;
    Mesh sightMesh;
    MeshRenderer sightMeshRenderer;
    Vector3[] sightVertices;
    int[] sightTriangles;
    Vector2[] sightColliderPoints;
    Material m_SightNormal;
    Material m_SightSuspicious;
    Material m_SightAlert;

    [SerializeField] GameObject emotion;
    Animator emotionAnimator;

    ProjectileGenerator projectileGenerator;

    [Header("Status")]
    [SerializeField] bool isDead;
    public bool IsDead
    {
        get { return isDead; }
        set
        {
            isDead = value;
            if (isDead)
            {
                agent.enabled = false;
                animator.SetTrigger("Dead");
                sightCollider.enabled = false;
                bodyCollider.isTrigger = true;
                sightMeshFilter.gameObject.SetActive(false);
                emotion.SetActive(false);

                currentFarmingArea = GetCorpseArea();
                BattleRoyalManager.SurvivorDead(this);
            }
        }
    }
    public int survivorID;
    public string survivorName;
    public Status currentStatus;
    [SerializeField] float maxHP = 100;
    public float MaxHP => maxHP;
    [SerializeField] float curHP;
    public float CurHP => curHP;
    [SerializeField] float attakDamage = 10f;
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float sightRange = 45f;
    float sightAngle = 120;
    public LayerMask sightObstacleMask;
    [SerializeField] int sightEdgeCount = 24;
    [SerializeField] float hearingAbility = 10f;
    [SerializeField] float farmingSpeed = 1f;
    [SerializeField] float aimErrorRange = 7.5f;
    public float AimErrorRange => aimErrorRange;

    [SerializeField] Vector2 lookRotation = Vector2.zero;
    public Vector2 LookRotation => lookRotation;

    [Header("Item")]
    [SerializeField] Weapon currentWeapon = null;
    public Weapon CurrentWeapon => currentWeapon;
    public RangedWeapon CurrentWeaponAsRangedWeapon
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
    [SerializeField] BulletproofHelmet currentHelmet;
    public BulletproofHelmet CurrentHelmet => currentHelmet;
    [SerializeField] BulletproofVest currentVest;
    public BulletproofVest CurrentVest => currentVest;

    [SerializeField] List<Item> inventory = new();
    public List<Item> Inventory => inventory;
    public Item ValidBullet
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
    bool currentWeaponisBestWeapon;

    [Header("Enemies")]
    [SerializeField] List<Survivor> inSightEnemies = new();
    public Survivor TargetEnemy 
    { 
        get
        {
            if (inSightEnemies.Count == 0) return null;
            else return inSightEnemies[0];
        }
    }
    Survivor lastTargetEnemy;
    [SerializeField] Vector2 targetEnemiesLastPosition;
    [SerializeField] Vector2 threateningSoundPosition;

    [Header("Farming")]
    // value : Had finished farming?
    public Dictionary<Area, bool> farmingAreas = new();
    [SerializeField] Area currentFarmingArea;
    public Area CurrentFarmingArea
    {
        get => currentFarmingArea;
        set
        {
            currentFarmingArea = value;
            if (value == null) return;
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

    [Header("Look")]
    [SerializeField] float lookAroundTime = 0.3f;
    [SerializeField] float curLookAroundTime;
    [SerializeField] int lookAroundCount;
    Vector2 keepAnEyeOnPosition;
    [SerializeField] float keepAnEyeOnTime = 3f;
    [SerializeField] float curKeepAnEyeOnTime;

    [Header("GameResult")]
    int killCount;
    public int KillCount => killCount;
    float totalDamage;
    public float TotalDamage => totalDamage;
    #endregion

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        projectileGenerator = GetComponent<ProjectileGenerator>();
        emotion.transform.parent = null;
        emotionAnimator = emotion.GetComponent<Animator>();
        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;
        sightMeshRenderer = sightMeshFilter.GetComponent<MeshRenderer>();

        sightVertices = new Vector3[sightEdgeCount + 1 + 1];  // +1은 원점을 포함
        sightTriangles = new int[(sightEdgeCount + 1) * 3];     // 삼각형 그리기
        sightColliderPoints = new Vector2[sightVertices.Length];
        m_SightNormal = ResourceManager.Get(ResourceEnum.Material.Sight_Normal);
        m_SightSuspicious = ResourceManager.Get(ResourceEnum.Material.Sight_Suspicious);
        m_SightAlert = ResourceManager.Get(ResourceEnum.Material.Sight_Alert);

        curHP = maxHP;
        agent.speed = moveSpeed;
    }


    override protected void MyUpdate()
    {
        if(!BattleRoyalManager.isBattleRoyalStart || isDead) return;
        emotion.transform.position = new(transform.position.x, transform.position.y + 1);
        AI();
        DrawSightMesh();
    }

    #region FixedUpdate, Look
    private void FixedUpdate()
    {
        if(!BattleRoyalManager.isBattleRoyalStart || isDead) return;
        if(keepAnEyeOnPosition != Vector2.zero)
        {
            curKeepAnEyeOnTime += Time.fixedDeltaTime;
            Look(keepAnEyeOnPosition);
            if(curKeepAnEyeOnTime > keepAnEyeOnTime)
            {
                keepAnEyeOnPosition = Vector2.zero;
                curKeepAnEyeOnTime = 0;
            }
        }
        else if (lookRotation != Vector2.zero)
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
        Vector2 currentLookVector = new(Mathf.Cos((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad));
        if(Vector2.Angle(currentLookVector, preferDirection) > 3)
        {
            float direction = Vector2.SignedAngle(currentLookVector, preferDirection) > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, transform.localEulerAngles.z + direction * 300 * Time.fixedDeltaTime);
        }
    }

    void LookAround()
    {
        curLookAroundTime += Time.deltaTime;
        sightMeshRenderer.material = m_SightSuspicious;
        emotionAnimator.SetTrigger("Suspicious");
        if (curLookAroundTime > lookAroundTime)
        {
            curLookAroundTime = 0;
            if (lookAroundCount > 3)
            {
                targetEnemiesLastPosition = Vector2.zero;
                threateningSoundPosition = Vector2.zero;
                lastTargetEnemy = null;
                lookAroundCount = 0;
                return;
            }
            lookRotation = new Vector2(Mathf.Cos(transform.eulerAngles.z), Mathf.Sin(transform.eulerAngles.z)).Rotate(120);
            lookAroundCount++;
        }
    }
    #endregion

    void AI()
    {
        if (inSightEnemies.Count == 0)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Aim", false);
            if(keepAnEyeOnPosition != Vector2.zero)
            {
                agent.SetDestination(transform.position);
                return;
            }

            if (CurrentWeaponAsRangedWeapon != null)
            {
                if(projectileGenerator.muzzleTF == null) projectileGenerator.ResetMuzzleTF();
                if (CurrentWeaponAsRangedWeapon.CurrentMagazine < CurrentWeaponAsRangedWeapon.MagazineCapacity && ValidBullet != null)
                {
                    currentStatus = Status.Maintain;
                    sightMeshRenderer.material = m_SightNormal;
                    Reload();
                    return;
                }
            }
            animator.SetBool("Reload", false);

            if(threateningSoundPosition != Vector2.zero)
            {
                InvestigateThreateningSound();
            }
            else if(targetEnemiesLastPosition != Vector2.zero)
            {
                TraceEnemy();
            }
            else
            {
                Farming();
            }
        }
        else
        {
            currentStatus = Status.InCombat;
            if (inSightEnemies[0].isDead)
            {
                if (!farmingCorpses.ContainsKey(inSightEnemies[0]))
                {
                    farmingCorpses.Add(inSightEnemies[0], false);
                    targetFarmingCorpse = inSightEnemies[0];
                }
                else if (!farmingCorpses[inSightEnemies[0]])
                {
                    targetFarmingCorpse = inSightEnemies[0];
                }
                inSightEnemies.Remove(inSightEnemies[0]);
                targetEnemiesLastPosition = Vector2.zero;
                lastTargetEnemy = null;
            }
            else
            {
                Combat(inSightEnemies[0]);
            }
        }
    }

    #region Farming
    void Farming()
    {
        lookRotation = Vector2.zero;
        currentStatus = Status.Farming;
        sightMeshRenderer.material = m_SightNormal;
        if (targetFarmingCorpse != null)
        {
            FarmingCorpse();
        }
        else if (targetFarmingSection != null)
        {
            FarmingSection();
        }
        else
        {
            CheckFarmingTarget();
            Explore();
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
                if (area.IsProhibited || area.IsProhibited_Plan) continue;
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
        if (targetFarmingCorpse.CurrentFarmingArea == null || targetFarmingCorpse.currentFarmingArea.IsProhibited || targetFarmingCorpse.currentFarmingArea.IsProhibited_Plan)
        {
            farmingCorpses[targetFarmingCorpse] = true;
            targetFarmingCorpse = null;
            return;
        }
        if (Vector2.Distance(transform.position, targetFarmingCorpse.transform.position) < 1.5f)
        {
            agent.SetDestination(transform.position);

            curFarmingTime += Time.deltaTime * farmingSpeed;
            if (curFarmingTime > farmingTime)
            {
                foreach (Item item in targetFarmingCorpse.inventory)
                    AcqireItem(item);
                if(IsValid(targetFarmingCorpse.currentWeapon))
                {
                    AcqireItem(targetFarmingCorpse.currentWeapon);
                    targetFarmingCorpse.UnequipWeapon();
                }
                if (IsValid(targetFarmingCorpse.currentHelmet))
                {
                    AcqireItem(targetFarmingCorpse.currentHelmet);
                    targetFarmingCorpse.UnequipBulletproofHelmet();
                }
                if (IsValid(targetFarmingCorpse.currentVest))
                {
                    AcqireItem(targetFarmingCorpse.currentVest);
                    targetFarmingCorpse.UnequipBulletproofVest();
                }
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

    bool farmingSFXPlayed;
    void FarmingBox()
    {
        if(Vector2.Distance(transform.position, targetFarmingBox.transform.position) < 1.5f)
        {
            if (!farmingSFXPlayed) PlayFarmingNoise();
            farmingSFXPlayed = true;
            agent.SetDestination(transform.position);

            curFarmingTime += Time.deltaTime * farmingSpeed;
            if (curFarmingTime > farmingTime)
            {
                farmingSFXPlayed = false;
                foreach (Item item in targetFarmingBox.items)
                    AcqireItem(item);
                targetFarmingBox.items.Clear();
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

    void PlayFarmingNoise()
    {
        float rand = UnityEngine.Random.Range(0, 1f);
        if(rand > 0.75f)
        {
            targetFarmingBox.PlaySFX("farmingNoise01,10");
        }
        else if (rand > 0.5f)
        {
            targetFarmingBox.PlaySFX("farmingNoise02,10");
        }
        else if (rand > 0.25f)
        {
            targetFarmingBox.PlaySFX("farmingNoise03,10");
        }
        else
        {
            targetFarmingBox.PlaySFX("farmingNoise04,10");
        }
    }

    bool noMoreFarmingArea;
    void Explore()
    {
        if (currentFarmingArea != null && !farmingAreas[currentFarmingArea]) return;
        if (Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            if (!noMoreFarmingArea)
            {
                if(currentFarmingArea != null)
                {
                    foreach (Area farmingArea in currentFarmingArea.adjacentAreas)
                    {
                        if (!farmingArea.IsProhibited && !farmingArea.IsProhibited_Plan && !farmingAreas[farmingArea])
                        {
                            CurrentFarmingArea = farmingArea;
                            return;
                        }
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
    #endregion

    #region Item
    public bool IsValid(Item item)
    {
        if (item == null || item.itemName == null ||  item.itemName == "") return false;
        return true;
    }

    void AcqireItem(Item item)
    {
        if (item is Weapon)
        {
            currentWeaponisBestWeapon = false;
            Weapon newWeapon = item as Weapon;
            if(CompareWeaponValue(newWeapon))
            {
                Equip(newWeapon);
            }
            else
            {
                GetItem(item);
            }
        }
        else if(item is BulletproofHelmet)
        {
            BulletproofHelmet newBulletproofHelmet = item as BulletproofHelmet;
            if (CompareBulletproofHelmetValue(newBulletproofHelmet))
            {
                Equip(newBulletproofHelmet);
            }
            else
            {
                GetItem(item);
            }
        }
        else if(item is BulletproofVest)
        {
            BulletproofVest newBulletproofVest = item as BulletproofVest;
            if (CompareBulletproofVestValue(newBulletproofVest))
            {
                Equip(newBulletproofVest);
            }
            else
            {
                GetItem(item);
            }
        }
        else if(item.itemName.Contains("Bullet"))
        {
            currentWeaponisBestWeapon = false;
            GetItem(item);
            string wantWeapon = item.itemName.Split('(')[0].Split(')')[0];
            RangedWeapon weapon = inventory.Find(x => x.itemName == wantWeapon) as RangedWeapon;
            if(weapon != null && CompareWeaponValue(weapon))
            {
                Equip(weapon);
            }
        }
        else
        {
            GetItem(item);
        }
    }

    void GetItem(Item item)
    {
        Item sameItem = inventory.Find(x => x.itemName == item.itemName);
        if (sameItem != null)
        {
            sameItem.amount += item.amount;
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

    // if newWeapon value > current weapon : return true
    bool CompareWeaponValue(Weapon newWeapon)
    {
        if(!IsValid(currentWeapon)) return true;
        if(newWeapon.itemName == currentWeapon.itemName) return false;
        
        if (currentWeapon is MeleeWeapon)
        {
            if (newWeapon is RangedWeapon)
            {
                // 근 vs 원
                RangedWeapon newWeaponAsRangedWeapon = newWeapon as RangedWeapon;
                if (newWeaponAsRangedWeapon.CurrentMagazine > 0) return true;
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
                if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0) return false;
                else return true;
            }
            else
            {
                // 원 vs 원
                RangedWeapon newWeaponAsRangedWeapon = newWeapon as RangedWeapon;
                Item bullet = inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})");
                if (newWeaponAsRangedWeapon.CurrentMagazine > 0 || bullet != null)
                {
                    // 둘 다 총알이 있는 경우
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
                    {
                        if (newWeapon.attackRange > currentWeapon.attackRange) return true;
                        else return false;
                    }
                    else return true;
                }
                else
                {
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0) return false;
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

    bool CompareBulletproofHelmetValue(BulletproofHelmet newBulletproofHelmet)
    {
        if (!IsValid(currentHelmet)) return true;
        return newBulletproofHelmet.Armor > currentHelmet.Armor;
    }

    bool CompareBulletproofVestValue(BulletproofVest newBulletproofVest)
    {
        if (!IsValid(currentVest)) return true;
        return newBulletproofVest.Armor > currentVest.Armor;
    }

    void Equip(Weapon wantWeapon)
    {
        // 차고 있는 무기가 있으면 놓고
        UnequipWeapon();

        // 새 무기 차기
        if(IsValid(wantWeapon))
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

    void UnequipWeapon()
    {
        if (IsValid(currentWeapon))
        {
            GetItem(currentWeapon);
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
            currentWeapon = null;
        }
    }

    void Equip(BulletproofHelmet wantBulletproofHelmet)
    {
        UnequipBulletproofHelmet();

        if (IsValid(wantBulletproofHelmet))
        {
            Transform weaponTF = null;
            foreach (Transform child in transform.Find("Head"))
            {
                if (child.name == $"{wantBulletproofHelmet.itemName}")
                {
                    weaponTF = child;
                }
            }
            if (weaponTF != null)
            {
                weaponTF.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Can't find helmet : {wantBulletproofHelmet.itemName}");
            }
            currentHelmet = wantBulletproofHelmet;
        }
    }

    void UnequipBulletproofHelmet()
    {
        if (IsValid(currentHelmet))
        {
            inventory.Add(currentHelmet);
            Transform curWeaponTF = transform.Find("Head").Find($"{currentHelmet.itemName}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Can't find helmet : {currentHelmet.itemName}");
            }
            currentHelmet = null;
        }
    }

    void Equip(BulletproofVest wantBulletproofVest)
    {
        UnequipBulletproofVest();

        if (IsValid(wantBulletproofVest))
        {
            currentVest = wantBulletproofVest;
        }
    }

    void UnequipBulletproofVest()
    {
        if (IsValid(currentVest))
        {
            inventory.Add(currentVest);
            currentVest = null;
        }
    }
    #endregion

    #region Combat
    void InvestigateThreateningSound()
    {
        currentStatus = Status.TraceEnemy;
        if (Vector2.Distance(transform.position, threateningSoundPosition) < 0.1f)
        {
            LookAround();
        }
        else
        {
            agent.SetDestination(threateningSoundPosition);
            lookRotation = Vector2.zero;
        }
    }

    void TraceEnemy()
    {
        currentStatus = Status.TraceEnemy;
        if (Vector2.Distance(transform.position, targetEnemiesLastPosition) < 0.1f)
        {
            LookAround();
        }
        else
        {
            agent.SetDestination(targetEnemiesLastPosition);
            lookRotation = Vector2.zero;
        }
    }

    void Combat(Survivor target)
    {
        lookRotation = target.transform.position - transform.position;
        float distance = Vector2.Distance(transform.position, inSightEnemies[0].transform.position);
        if (IsValid(currentWeapon))
        {
            if(CurrentWeaponAsRangedWeapon != null)
            {
                if(distance < CurrentWeaponAsRangedWeapon.MinimumRange)
                {
                    if(distance < attackRange)
                    {
                        Attack();
                        return;
                    }
                }
                else if (CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
                {
                    if (distance < CurrentWeaponAsRangedWeapon.attackRange)
                    {
                        Aim();
                        return;
                    }
                }
                else if(ValidBullet != null)
                {
                    Reload();
                    return;
                }
                else if(!currentWeaponisBestWeapon)
                {
                    List<Item> candidates = inventory.FindAll(x => x is Weapon);
                    foreach(Item candidate in candidates)
                    {
                        if (CompareWeaponValue(candidate as Weapon)) Equip(candidate as Weapon);
                    }
                    currentWeaponisBestWeapon = true;
                    return;
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
        if(IsValid(currentWeapon))
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

    void Reload()
    {
        animator.SetBool("Attack", false);
        agent.SetDestination(transform.position);
        animator.SetBool("Reload", true);
    }
    #endregion

    #region Sight
    void DrawSightMesh()
    {
        sightVertices[0] = Vector3.zero;  // 시야의 중심

        for (int i = 0; i <= sightEdgeCount; i++)
        {
            float angle = -sightAngle / 2 + i * (sightAngle / sightEdgeCount);  // 시작 각도부터 각도 간격만큼 더해가기
            Vector2 direction = DirFromAngle(angle);  // Ray를 쏠 때는 월드 기준
            Vector2 meshDirection = DirFromAngle(angle, true);  // 메쉬는 Survivor의 Head의 Sight가 들고있어서 로컬 기준
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, sightRange, sightObstacleMask);
            if(hits.Length > 0)
            {
                sightVertices[i + 1] = meshDirection.normalized * hits[0].distance;
                Debug.DrawRay(transform.position, direction.normalized * hits[0].distance, Color.red);
            }
            else
            {
                sightVertices[i + 1] = meshDirection.normalized * sightRange;  // 해당 방향으로 끝 점을 그리기
                Debug.DrawRay(transform.position, direction.normalized * sightRange, Color.red);
            }
        }

        for (int i = 0; i < sightEdgeCount; i++)
        {
            sightTriangles[i * 3] = 0;  // 중심 점
            sightTriangles[i * 3 + 1] = i + 1;  // 시작 점
            sightTriangles[i * 3 + 2] = i + 2;  // 끝 점
        }

        sightMesh.Clear();
        sightMesh.vertices = sightVertices;
        sightMesh.triangles = sightTriangles;
        sightMesh.RecalculateNormals();  // 법선 벡터 계산

        for (int i=0; i < sightVertices.Length; i++)
        {
            sightColliderPoints[i] = sightVertices[i];
        }
        sightCollider.pathCount = 1;
        sightCollider.SetPath(0, sightColliderPoints);
    }

    // 각도에서 방향 벡터를 계산하는 함수
    Vector2 DirFromAngle(float angleInDegrees, bool isLocal = false)
    {
        float rad = isLocal ? (angleInDegrees + 90) * Mathf.Deg2Rad : (angleInDegrees + transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));  // 2D에서는 Z축은 0
    }
    #endregion

    #region Hearing
    public void HearSound(float volume, Vector2 soundOrigin, CustomObject noiseMaker)
    {
        if (noiseMaker == this || inSightEnemies.Contains(noiseMaker as Survivor)) return;
        float distance = Vector2.Distance(transform.position, soundOrigin);
        float heardVolume = volume * hearingAbility / (distance * distance);
        Debug.Log($"{survivorName}, {(noiseMaker as Survivor).survivorName}, {heardVolume}");

        if(heardVolume > 10f)
        {
            // 어떤 소리인지 명확한 인지
            threateningSoundPosition = soundOrigin;
            sightMeshRenderer.material = m_SightAlert;
            emotionAnimator.SetTrigger("Alert");
        }
        else if( heardVolume > 1f)
        {
            // 불분명한 인지
            keepAnEyeOnPosition = soundOrigin;
            sightMeshRenderer.material = m_SightSuspicious;
            emotionAnimator.SetTrigger("Suspicious");
        }
    }
    #endregion

    #region Take Damage
    void ApplyDamage(Survivor attacker, float damage)
    {
        if (damage < 0) damage = 0;
        curHP -= damage;
        attacker.totalDamage += damage;
        if (curHP <= 0)
        {
            curHP = 0;
            Debug.Log($"{survivorName} is eliminated by {attacker.survivorName}");
            attacker.killCount++;
            IsDead = true;
        }

        if (inSightEnemies.Contains(attacker))
        {
            if (attacker != inSightEnemies[0])
            {
                inSightEnemies.Remove(attacker);
                inSightEnemies.Insert(0, attacker);
            }
        }
        else
        {
            inSightEnemies.Insert(0, attacker);
            sightMeshRenderer.material = m_SightAlert;
            emotionAnimator.SetTrigger("Alert");
        }

    }

    public void TakeDamage(Survivor attacker, float damage)
    {
        string hitSound;
        if(!inSightEnemies.Contains(attacker))
        {
            // 시야 밖에서 맞으면 무조건 치명타
            damage *= 2;
            hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,10" : "hit01,10";
        }
        else
        {
            float probability = UnityEngine.Random.Range(0, 1f);
            if (probability < 0.2f)
            {
                // 회피
                damage = 0;
                hitSound = "avoid, 1";
            }
            else if (probability < 0.5f)
            {
                // 방어
                damage *= 0.5f;
                hitSound = "guard, 5";
            }
            else if (probability > 0.9f)
            {
                // 치명타
                damage *= 2;
                hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,10" : "hit01,10";
            }
            else hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,10" : "hit01,10";
        }

        PlaySFX(hitSound, this);
        ApplyDamage(attacker, damage);
    }

    public void TakeDamage(Bullet bullet)
    {
        if (isDead) return;
        float damage;
        float probability = UnityEngine.Random.Range(0, 1f);
        int damagePart; // 0 : 헤드, 1 : 바디, 2 : 기타
        // 헤드샷
        if(probability > 0.99f)
        {
            damage = bullet.Damage * 4;
            damagePart = 0;
        }
        // 바디샷
        else if(probability > 0.3f)
        {
            damage = bullet.Damage * 2;
            damagePart = 1;
        }
        else
        {
            damage = bullet.Damage;
            damagePart = 2;
        }
        // 실효 사거리 밖
        if(bullet.TraveledDistance > bullet.MaxRange * 0.5f)
        {
            damage *= (bullet.MaxRange * 1.5f - bullet.TraveledDistance) / bullet.MaxRange;
        }

        if(damagePart == 0)
        {
            if (currentHelmet != null)
            {
                damage -= currentHelmet.Armor;
                if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                {
                    PlaySFX("ricochet,10");
                }
                else
                {
                    PlaySFX("ricochet2,10");
                }
            }
        }
        else if(damagePart == 1)
        {
            if(currentVest != null) damage -= currentVest.Armor;
        }

        ApplyDamage(bullet.Launcher, damage);
    }
    #endregion

    #region Animation Events
    void AE_Attack()
    {
        if (inSightEnemies.Count == 0) return;
        if(IsValid(currentWeapon) && currentWeapon is MeleeWeapon)
        {
            if (Vector2.Distance(transform.position, inSightEnemies[0].transform.position) < currentWeapon.attackRange)
            {
                inSightEnemies[0].TakeDamage(this, currentWeapon.attakDamage);
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, inSightEnemies[0].transform.position) < attackRange)
            {
                inSightEnemies[0].TakeDamage(this, attakDamage);
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
    #endregion

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!BattleRoyalManager.isBattleRoyalStart || isDead) return;
        if (!collision.isTrigger)
        {
            if (collision.TryGetComponent(out Survivor survivor))
            {
                if (!inSightEnemies.Contains(survivor))
                {
                    inSightEnemies.Add(survivor);
                    sightMeshRenderer.material = m_SightAlert;
                    if(survivor != lastTargetEnemy) emotionAnimator.SetTrigger("Alert");
                }
            }
        }
        else
        {
            if (collision.TryGetComponent(out Survivor survivor))
            {
                if (survivor.isDead)
                {
                    if (!farmingCorpses.ContainsKey(survivor))
                    {
                        farmingCorpses.Add(survivor, false);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Survivor survivor))
        {
            if (!collision.isTrigger)
            {
                if(survivor == TargetEnemy)
                {
                    targetEnemiesLastPosition = survivor.transform.position;
                    lastTargetEnemy = survivor;
                    inSightEnemies.Remove(survivor);
                }
            }
        }
    }

    public void SetColor(float r, float g, float b, float a = 1.0f)
    {
        foreach(var sprite in bodySprites)
        {
            sprite.color = new Color(r, g, b, a);
        }
    }

    public void SetColor(Vector3 rgbVector)
    {
        SetColor(rgbVector.x, rgbVector.y, rgbVector.z);
    }

    Area GetCorpseArea()
    {
        foreach(var area in farmingAreas)
        {
            Transform areaTransform = area.Key.transform;
            if (transform.position.x > areaTransform.position.x - areaTransform.localScale.x * 0.5f 
                && transform.position.x < areaTransform.position.x + areaTransform.localScale.x * 0.5f
                && transform.position.y > areaTransform.position.y - areaTransform.localScale.y * 0.5f
                && transform.position.y < areaTransform.position.y + areaTransform.localScale.y * 0.5f)
            {
                return area.Key;
            }
        }
        return null;
    }

    public void SetSurvivorInfo(SurvivorInfo survivorInfo)
    {
        survivorName = survivorInfo.survivorName;
        curHP = maxHP = survivorInfo.hp;
        attakDamage = survivorInfo.attackDamage;
        attackSpeed = survivorInfo.attackSpeed;
        moveSpeed = survivorInfo.moveSpeed;
        farmingSpeed = survivorInfo.farmingSpeed;

    }

    private void OnDrawGizmos()
    {
        if(isDead) return;
    }
}

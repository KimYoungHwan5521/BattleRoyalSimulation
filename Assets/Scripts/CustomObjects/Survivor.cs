using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Survivor : CustomObject
{
    #region Variables and Properties
    public enum Status { Farming, InCombat, TraceEnemy, InvestigateThreateningSound, Maintain, RunAway }
    #region Components
    [Header("Components")]
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject leftHand;
    [SerializeField] PolygonCollider2D sightCollider;
    [SerializeField] CircleCollider2D bodyCollider;
    [SerializeField] SpriteRenderer[] bodySprites;
    Animator animator => GetComponent<Animator>();
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

    [SerializeField] GameObject canvas;
    [SerializeField] GameObject nameTag;
    [SerializeField] GameObject prohibitTimer;
    public InGameUIManager InGameUIManager => GameManager.Instance.GetComponent<InGameUIManager>();
    ProjectileGenerator projectileGenerator;
    #endregion
    #region Status
    [Header("Status")]
    [SerializeField] SurvivorData linkedSurvivorData;
    public SurvivorData LinkedSurvivorData => linkedSurvivorData;
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
                nameTag.SetActive(false);
                prohibitTimer.SetActive(false);

                currentFarmingArea = GetCurrentArea();
                BattleRoyaleManager.SurvivorDead(this);
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
    [SerializeField] float attackDamage = 10f;
    public float CurHPPercent => curHP / maxHP * 100;
    public float AttackDamage => attackDamage;
    [SerializeField] float attackSpeed = 1f;
    public float AttackSpeed => attackSpeed;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float moveSpeed = 3f;
    public float MoveSpeed => moveSpeed;
    [SerializeField] float leftSightRange = 45f;
    [SerializeField] float rightSightRange = 45f;
    float sightAngle = 120;
    public LayerMask sightObstacleMask;
    [SerializeField] int sightEdgeCount = 24;
    [SerializeField] float hearingAbility = 10f;
    [SerializeField] string heardSound;
    class RememberSound
    {
        public string soundName;
        public float soundTime;

        public RememberSound(string soundName)
        {
            this.soundName = soundName;
            soundTime = 0;
        }
    }
    List<RememberSound> rememberSounds = new();
    [SerializeField] float farmingSpeed = 1f;
    public float FarmingSpeed => farmingSpeed;
    float shooting = 1;
    public float Shooting => shooting;
    [SerializeField] float aimErrorRange = 7.5f;
    public float AimErrorRange => aimErrorRange;
    float luck = 50;
    public float Luck => luck;

    float meleeHitRate = 1;
    float meleeAvoidRate = 1;
    float meleeGuardRate = 1;
    float meleeCriticalRate = 1;

    float bloodRegeneration = 1;
    float hpRegeneration = 1;
    #endregion
    #region Characteristic / Injury
    [Header("Characteristic / Injury")]
    [SerializeField] List<Characteristic> charicteristics;
    public List<Characteristic> Characteristics => charicteristics;

    [SerializeField] Vector2 lookRotation = Vector2.zero;
    public Vector2 LookRotation => lookRotation;

    public List<Injury> injuries = new();
    public List<InjurySite> rememberAlreadyHaveInjury;

    [SerializeField] bool rightHandDisabled;
    bool RightHandDisabled
    {
        get { return rightHandDisabled; }
        set 
        { 
            rightHandDisabled = value;
            animator.SetBool("RightHandDisabled", value);
            rightHand.SetActive(!value);

            currentWeapon = null;
            currentWeaponisBestWeapon = false;
            List<Item> candidates = inventory.FindAll(x => x is Weapon);
            foreach (Item candidate in candidates)
            {
                if (CompareWeaponValue(candidate as Weapon)) Equip(candidate as Weapon);
            }
            currentWeaponisBestWeapon = true;
        }
    }
    [SerializeField] bool leftHandDisabled;
    bool LeftHandDisabled
    {
        get { return leftHandDisabled; }
        set
        {
            leftHandDisabled = value;
            animator.SetBool("LeftHandDisabled", value);
            leftHand.SetActive(!value);

            currentWeapon = null;
            currentWeaponisBestWeapon = false;
            List<Item> candidates = inventory.FindAll(x => x is Weapon);
            foreach (Item candidate in candidates)
            {
                if (CompareWeaponValue(candidate as Weapon)) Equip(candidate as Weapon);
            }
            currentWeaponisBestWeapon = true;
        }
    }
    bool dizzy;
    bool Dizzy
    {
        get { return dizzy; }
        set
        {
            dizzy = value;
            emotionAnimator.SetTrigger("Dizzy");
        }
    }
    [SerializeField] float dizzyRateByConcussion = 0;
    float DizzyRateByBleeding
    {
        get
        {
            return Mathf.Min(0, (0.8f - (curBlood / maxBlood)) / 0.3f);
        }
    }
    float DizzyRate
    {
        get
        {
            return 1 - (1 - dizzyRateByConcussion) * (1 - DizzyRateByBleeding);
        }
    }
    float dizzyCoolTime = 10f;
    float curDizzyCool;
    float dizzyDuration = 3f;
    float curDizzyDuration;

    bool inProhibitedArea;
    public bool InProhibitedArea
    {
        get { return InProhibitedArea; }
        set
        {
            inProhibitedArea = value;
            prohibitTimer.SetActive(value);
        }
    }
    [SerializeField] float prohibitedAreaTime = 3.1f;
    int timerSound = 3;

    public float maxBlood;
    public float curBlood;
    [SerializeField] float bleedingAmount = 0;
    public float BleedingAmount
    {
        get { return bleedingAmount; }
        set
        {
            bleedingAmount = Mathf.Max(value, 0);
            if (linkedSurvivorData.characteristics.FindIndex(x => x.type == CharacteristicType.Avenger) > -1)
            {
                ApplyCorrectionStats();
                if(bleedingAmount > 0)
                {
                    attackDamage *= 1.5f;
                    attackSpeed *= 1.3f;
                }
            }
        }
    }
    public float naturalHemostasis = 1f;
    float bleedingSprite;
    public List<GameObject> bloods = new();
    #endregion
    #region Item
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
    #endregion
    #region Enemies
    [Header("Enemies")]
    [SerializeField] List<Survivor> inSightEnemies = new();
    public Survivor TargetEnemy 
    { 
        get
        {
            if (inSightEnemies.Count == 0) return null;
            else
            {
                if (linkedSurvivorData.strategyDictionary[StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget].action == 0)
                    return inSightEnemies[0];
                else if (linkedSurvivorData.strategyDictionary[StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget].action == 1)
                    return FindNearest(inSightEnemies);
                else if (linkedSurvivorData.strategyDictionary[StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget].action == 2)
                    return FindWhoseWeaponRangeIsLongest(inSightEnemies);
                else return inSightEnemies[0];
            }
        }
    }
    Survivor lastTargetEnemy;
    [SerializeField] Vector2 targetEnemiesLastPosition;
    [SerializeField] Vector2 threateningSoundPosition;
    float curSetDestinationCool = 1f;
    [SerializeField] Vector2 runAwayDestination;
    [SerializeField] Survivor runAwayFrom;

    // value : Had finished farming?
    public Dictionary<Area, bool> farmingAreas = new();
    #endregion
    #region Farming
    [Header("Farming")]
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
    [SerializeField] float aimDelay = 1.5f;
    [SerializeField] float curAimDelay;
    [SerializeField] float curShotTime;
    #endregion
    #region Look
    [Header("Look")]
    [SerializeField] float lookAroundTime = 0.3f;
    [SerializeField] float curLookAroundTime;
    [SerializeField] int lookAroundCount;
    Vector2 keepEyesOnPosition;
    [SerializeField] float keepAnEyeOnTime = 3f;
    [SerializeField] float curKeepAnEyeOnTime;

    //[Header("Strategy")]
    delegate bool Condition();
    struct Conditions
    {
        public Condition[] conditions;
        public Condition TotalCondition;
    }
    Dictionary<StrategyCase, Conditions> strategyConditions = new();
    #endregion
    #region Game Result
    [Header("Game Result")]
    float survivedTime;
    public float SurvivedTime => survivedTime;
    int killCount;
    public int KillCount => killCount;
    float totalDamage;
    public float TotalDamage => totalDamage;
    #endregion
    #endregion
    protected override void Start()
    {
        base.Start();
        //animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        projectileGenerator = GetComponent<ProjectileGenerator>();
        emotion.transform.parent = null;
        emotionAnimator = emotion.GetComponent<Animator>();
        canvas.transform.SetParent(null);
        prohibitTimer.SetActive(false);
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


    override public void MyUpdate()
    {
        if(!BattleRoyaleManager.isBattleRoyaleStart || isDead)
        {
            animator.SetBool("Attack", false);
            return;
        }
        emotion.transform.position = new(transform.position.x, transform.position.y + 1);
        canvas.transform.position = new(transform.position.x, transform.position.y);
        
        survivedTime += Time.deltaTime;
        CheckProhibitArea();
        Bleeding();
        if(isDead) return;
        Regenerate();

        if (dizzy) return;
        AI();
        DrawSightMesh();
    }

    public override void MyDestroy()
    {
        Destroy(emotion);
        Destroy(canvas);
        base.MyDestroy();
    }

    #region FixedUpdate, Look
    private void FixedUpdate()
    {
        if(!BattleRoyaleManager.isBattleRoyaleStart || isDead) return;

        if(DizzyRate > 0)
        {
            if(dizzy)
            {
                curDizzyDuration += Time.fixedDeltaTime;
                if (curDizzyDuration > dizzyDuration)
                {
                    Dizzy = false;
                }
                else return;
            }
            else
            {
                curDizzyCool += Time.fixedDeltaTime;
                if(curDizzyCool > dizzyCoolTime)
                {
                    if(UnityEngine.Random.Range(0, 1f) < dizzyRateByConcussion) Dizzy = true;
                    curDizzyCool = 0;
                    return;
                }
            }
        }

        List<RememberSound> reserveRemove = new();
        foreach (var sound in rememberSounds)
        {
            sound.soundTime += Time.fixedDeltaTime;
            if(sound.soundTime > 10) reserveRemove.Add(sound);
        }
        foreach (var sound in reserveRemove)
        {
            rememberSounds.Remove(sound);
        }
        
        if(keepEyesOnPosition != Vector2.zero)
        {
            curKeepAnEyeOnTime += Time.fixedDeltaTime;
            Look(keepEyesOnPosition);
            if(curKeepAnEyeOnTime > keepAnEyeOnTime)
            {
                keepEyesOnPosition = Vector2.zero;
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

                CurrentFarmingArea = FindNearest(farmingAreas);
                targetFarmingSection = FindNearest(farmingSections);
                targetFarmingBox = FindNearest(farmingBoxes);
                targetFarmingCorpse = null;
                return;
            }
            lookRotation = new Vector2(Mathf.Cos(transform.eulerAngles.z), Mathf.Sin(transform.eulerAngles.z)).Rotate(120);
            lookAroundCount++;
        }
    }
    #endregion

    void CheckProhibitArea()
    {
        if(inProhibitedArea)
        {
            prohibitedAreaTime -= Time.deltaTime;
            if(prohibitedAreaTime < timerSound)
            {
                prohibitTimer.GetComponent<TextMeshProUGUI>().text = timerSound.ToString();
                SoundManager.Play(ResourceEnum.SFX.piep, transform.position);
                timerSound--;
                Debug.Log(survivorName);
            }
            if(prohibitedAreaTime < 0)
            {
                IsDead = true;
                InGameUIManager.ShowKillLog(survivorName, "prohibited area");
            }
        }
    }

    void Bleeding()
    {
        curBlood -= BleedingAmount * Time.deltaTime;
        BleedingAmount -= naturalHemostasis * Time.deltaTime;
        if(curBlood < bleedingSprite)
        {
            bloods.Add(PoolManager.Spawn(ResourceEnum.Prefab.Blood, transform.position));
            bleedingSprite -= 100;
        }
        if(curBlood / maxBlood < 0.5f)
        {
            IsDead = true;
            InGameUIManager.ShowKillLog(survivorName, "hemorrhage");
        }
    }

    void StopBleeding()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("StopBleeding", true);
    }

    void Regenerate()
    {
        curBlood += bloodRegeneration * Time.deltaTime;
        curHP = Mathf.Min(curHP + 0.17f * hpRegeneration * Time.deltaTime, maxHP);
    }

    void AI()
    {
        if (inSightEnemies.Count == 0)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Aim", false);
            curAimDelay = 0;

            if(runAwayDestination != Vector2.zero)
            {
                if (Vector2.Distance(transform.position, runAwayDestination) < 1f)
                {
                    runAwayDestination = Vector2.zero;
                    runAwayFrom = null;
                    sightMeshRenderer.material = m_SightNormal;
                }
                else return;
            }

            if(keepEyesOnPosition != Vector2.zero)
            {
                agent.SetDestination(transform.position);
                return;
            }

            if(!(rightHandDisabled && leftHandDisabled))
            {
                if(BleedingAmount >= 10)
                {
                    int bandageIndex = inventory.FindIndex(x => x.itemType == ItemManager.Items.BandageRoll);
                    if(bandageIndex != -1)
                    {
                        currentStatus = Status.Maintain;
                        StopBleeding();
                        return;
                    }
                }
                animator.SetBool("StopBleeding", false);

                if (CurrentWeaponAsRangedWeapon != null)
                {
                    if(projectileGenerator.muzzleTF == null) projectileGenerator.ResetMuzzleTF(rightHandDisabled ? leftHand.transform : rightHand.transform);
                    if (CurrentWeaponAsRangedWeapon.CurrentMagazine < CurrentWeaponAsRangedWeapon.MagazineCapacity && ValidBullet != null)
                    {
                        currentStatus = Status.Maintain;
                        sightMeshRenderer.material = m_SightNormal;
                        Reload();
                        return;
                    }
                }
                animator.SetBool("Reload", false);
            }

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
            // 보고 있던 대상이 죽어버릴 경우
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

                CurrentFarmingArea = FindNearest(farmingAreas);
                targetFarmingSection = FindNearest(farmingSections);
                targetFarmingBox = FindNearest(farmingBoxes);
            }
            else
            {
                lookRotation = TargetEnemy.transform.position - transform.position;
                float distance = Vector2.Distance(transform.position, TargetEnemy.transform.position);
                bool enemyInAttackRange = false;
                if (IsValid(currentWeapon))
                {
                    if (CurrentWeaponAsRangedWeapon != null)
                    {
                        if (distance < CurrentWeaponAsRangedWeapon.AttackRange) enemyInAttackRange = true;
                    }
                    else if (distance < currentWeapon.AttackRange) enemyInAttackRange = true;
                }
                else if (distance < attackRange) enemyInAttackRange = true;

                if (enemyInAttackRange)
                {
                    if(strategyConditions[StrategyCase.SawAnEnemyAndItIsInAttackRange].TotalCondition.Invoke())
                    {
                        if(linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 2)
                        {
                            if (TargetEnemy != runAwayFrom && CanRunAway(out runAwayDestination))
                            {
                                currentStatus = Status.RunAway;
                                agent.SetDestination(runAwayDestination);
                            }
                            else Combat(distance);
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 0)
                        {
                            Combat(distance);
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 1)
                        {
                            Farming();
                        }
                    }
                    else
                    {
                        if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 0)
                        {
                            Combat(distance);
                        }
                        else if(linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 1)
                        {
                            Farming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 2)
                        {
                            TryRunAway(distance);
                        }
                    }
                }
                else
                {
                    if (strategyConditions[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].TotalCondition.Invoke())
                    {
                        if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 0)
                        {
                            ApproachEnemy(TargetEnemy);
                        }
                        else if(linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 1)
                        {
                            Farming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 2)
                        {
                            TryRunAway(distance);
                        }
                    }
                    else
                    {
                        if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction == 0)
                        {
                            ApproachEnemy(TargetEnemy);
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction == 1)
                        {
                            Farming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction == 2)
                        {
                            TryRunAway(distance);
                        }
                    }
                }
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
                if(corpse.Key.CurrentFarmingArea.IsProhibited || corpse.Key.CurrentFarmingArea.IsProhibited_Plan)
                {
                    farmingCorpses[corpse.Key] = true;
                    return false;
                }
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

    public TKey FindNearest<TKey>(Dictionary<TKey, bool> candidates) where TKey : MonoBehaviour
    {
        TKey nearest = default;
        float minDistance = float.MaxValue;
        float distance;
        List<Area> reserveRemoves = new();
        foreach (KeyValuePair<TKey, bool> candidate in candidates)
        {
            if (typeof(TKey) == typeof(Area))
            {
                Area area = candidate.Key as Area;
                if (area.IsProhibited || area.IsProhibited_Plan)
                {
                    reserveRemoves.Add(area);
                    continue;
                }
            }
            if (!candidate.Value)
            {
                if(typeof(TKey) == typeof(FarmingSection))
                {
                    FarmingSection farmingSection = candidate.Key as FarmingSection;
                    if (farmingSection.ownerArea.IsProhibited || farmingSection.ownerArea.IsProhibited_Plan)
                    {
                        farmingSections[farmingSection] = true;
                        return null;
                    }
                }
                else if (typeof(TKey) == typeof(Box))
                {
                    Box farmingBox = candidate.Key as Box;
                    if (farmingBox.ownerArea.IsProhibited || farmingBox.ownerArea.IsProhibited_Plan)
                    {
                        farmingBoxes[farmingBox] = true;
                        return null;
                    }
                }
                distance = Vector2.Distance(transform.position, candidate.Key.transform.position);
                if (typeof(TKey) == typeof(Area)) Debug.Log($"{candidate}, distance : {distance}");
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = candidate.Key;
                }
            }
        }
        if(typeof(TKey) == typeof(Area))
        {
            foreach (Area reserveRemove in reserveRemoves)
            {
                farmingAreas[reserveRemove] = true;
            }
        }
        return nearest;
    }

    Survivor FindNearest(List<Survivor> survivors)
    {
        float distance;
        float minDistance = float.MaxValue;
        Survivor nearest = null;
        foreach(Survivor survivor in survivors)
        {
            distance = Vector2.Distance(transform.position, survivor.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = survivor;
            }
        }
        return nearest;
    }

    Survivor FindWhoseWeaponRangeIsLongest(List<Survivor> survivors)
    {
        float weaponRange;
        float longestRange = 0;
        Survivor longest = null;
        foreach (Survivor survivor in survivors)
        {
            weaponRange = survivor.CurrentWeapon != null ? survivor.CurrentWeapon.AttackRange : 1.5f;
            if (longestRange < weaponRange)
            {
                longestRange = weaponRange;
                longest = survivor;
            }
        }
        return longest;
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
                CurrentFarmingArea = FindNearest(farmingAreas);
                targetFarmingSection = FindNearest(farmingSections);
                targetFarmingBox = FindNearest(farmingBoxes);
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
        if(Vector2.Distance(bodyCollider.ClosestPoint(targetFarmingBox.transform.position), targetFarmingBox.Collider.ClosestPoint(transform.position)) < 1f)
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
                InGameUIManager.UpdateSelectedObjectInventory(targetFarmingBox);
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
            targetFarmingBox.PlaySFX("farmingNoise01,2", this);
        }
        else if (rand > 0.5f)
        {
            targetFarmingBox.PlaySFX("farmingNoise02,2", this);
        }
        else if (rand > 0.25f)
        {
            targetFarmingBox.PlaySFX("farmingNoise03,2", this);
        }
        else
        {
            targetFarmingBox.PlaySFX("farmingNoise04,2", this);
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
                foreach (Area farmingArea in currentFarmingArea.adjacentAreas)
                {
                    if (!farmingArea.IsProhibited && !farmingArea.IsProhibited_Plan && !farmingAreas[farmingArea])
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
                else
                {
                    noMoreFarmingArea = true;
                    CurrentFarmingArea = farmingAreas.FirstOrDefault(x => !x.Key.IsProhibited && !x.Key.IsProhibited_Plan).Key;
                }
            }
            else
            {
                Vector2 wantPosition = transform.position;
                wantPosition = new(
                    currentFarmingArea.transform.position.x + UnityEngine.Random.Range(-currentFarmingArea.transform.localScale.x * 0.5f, currentFarmingArea.transform.localScale.x * 0.5f),
                    currentFarmingArea.transform.position.y + UnityEngine.Random.Range(-currentFarmingArea.transform.localScale.y * 0.5f, currentFarmingArea.transform.localScale.y * 0.5f)
                    );

                agent.SetDestination(wantPosition);
            }
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

    public bool CanUse(Weapon weapon)
    {
        if (rightHandDisabled && leftHandDisabled) return false;
        if(weapon.NeedHand == NeedHand.TwoHand)
        {
            if (rightHandDisabled || leftHandDisabled) return false;
        }
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
        InGameUIManager.UpdateSelectedObjectInventory(this);
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
            InGameUIManager.UpdateSelectedObjectInventory(this);
        }
    }

    // if newWeapon value > current weapon : return true
    bool CompareWeaponValue(Weapon newWeapon)
    {
        if (!CanUse(newWeapon)) return false;
        if(!IsValid(currentWeapon)) return true;
        if(newWeapon.itemName == currentWeapon.itemName) return false;

        if (newWeapon.itemType == linkedSurvivorData.priority1Weapon) return true;
        else if (currentWeapon.itemType == linkedSurvivorData.priority1Weapon) return false;

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
                if (newWeapon.AttackDamage > currentWeapon.AttackDamage) return true;
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
                        if (newWeapon.AttackDamage > currentWeapon.AttackDamage) return true;
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
                        if (newWeapon.AttackDamage > currentWeapon.AttackDamage) return true;
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
            Transform hand = rightHand.transform;
            if (rightHandDisabled) hand = leftHand.transform;
            // Active가 꺼져있는 오브젝트는 Find로 찾을 수 없다.
            foreach (Transform child in hand)
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
        InGameUIManager.UpdateSelectedObjectInventory(this);
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
        InGameUIManager.UpdateSelectedObjectInventory(this);
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
        InGameUIManager.UpdateSelectedObjectInventory(this);
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
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void Equip(BulletproofVest wantBulletproofVest)
    {
        UnequipBulletproofVest();

        if (IsValid(wantBulletproofVest))
        {
            currentVest = wantBulletproofVest;
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void UnequipBulletproofVest()
    {
        if (IsValid(currentVest))
        {
            inventory.Add(currentVest);
            currentVest = null;
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }
    #endregion

    #region Combat
    void InvestigateThreateningSound()
    {
        currentStatus = Status.InvestigateThreateningSound;
        if (Vector2.Distance(transform.position, threateningSoundPosition) < 1f)
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
        if (Vector2.Distance(transform.position, targetEnemiesLastPosition) < 0.3f)
        {
            LookAround();
        }
        else
        {
            agent.SetDestination(targetEnemiesLastPosition);
            lookRotation = Vector2.zero;
        }
    }

    void Combat(float distance)
    {
        if (CurrentWeaponAsRangedWeapon != null)
        {
            if (distance < CurrentWeaponAsRangedWeapon.MinimumRange) Attack();
            else if (CurrentWeaponAsRangedWeapon.CurrentMagazine > 0) Aim();
            else if (ValidBullet != null) Reload();
            else if (!currentWeaponisBestWeapon)
            {
                List<Item> candidates = inventory.FindAll(x => x is Weapon);
                foreach (Item candidate in candidates)
                {
                    if (CompareWeaponValue(candidate as Weapon)) Equip(candidate as Weapon);
                }
                currentWeaponisBestWeapon = true;
            }
        }
        else Attack();
    }

    void ApproachEnemy(Survivor target)
    {
        animator.SetBool("Attack", false);
        animator.SetBool("Aim", false);
        curAimDelay = 0;
        animator.SetBool("Reload", false);
        animator.SetBool("StopBleeding", false);
        if (Vector2.Distance(agent.destination, target.transform.position) > attackRange)
        {
            curSetDestinationCool += Time.deltaTime;
            if (curSetDestinationCool > 1)
            {
                agent.SetDestination(target.transform.position);
                curSetDestinationCool = 0;
            }
        }
    }

    void Attack()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("Aim", false);
        curAimDelay = 0;
        animator.SetBool("Reload", false);
        animator.SetBool("StopBleeding", false);
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
        animator.SetBool("StopBleeding", false);
        animator.SetBool("Aim", true);

        curAimDelay += Time.deltaTime;
        if(curAimDelay > aimDelay)
        {
            curShotTime -= Time.deltaTime;
            if(curShotTime < CurrentWeaponAsRangedWeapon.ShotCoolTime)
            {
                animator.SetInteger("ShotAnimNumber", CurrentWeaponAsRangedWeapon.ShotAnimNumber);
                animator.SetTrigger("Fire");
                curShotTime = 0;
            }
        }
    }

    void Reload()
    {
        animator.SetBool("Attack", false);
        animator.SetBool("StopBleeding", false);
        agent.SetDestination(transform.position);
        animator.SetBool("Reload", true);
        curAimDelay = 0;
    }
    #endregion

    #region Run Away
    void TryRunAway(float distance)
    {
        if(TargetEnemy != runAwayFrom) Combat(distance);
        else if (CanRunAway(out runAwayDestination))
        {
            currentStatus = Status.RunAway;
            agent.SetDestination(runAwayDestination);
        }
    }

    bool CanRunAway(out Vector2 destination)
    {
        destination = Vector2.zero;
        List<Vector2> enemyBlockeds = new();
        List<Vector2> imNotBlockeds = new();
        for(int j = 1; j<6; j++)
        {
            // 적의 360도 방향으로 Ray를 쏴서 상대로 부터 시야가 막힌 공간을 찾는다.
            for (int i = 0; i < 24; i++)
            {
                float angle = i * 15;
                Vector2 direction = DirFromAngle(angle);
                RaycastHit2D[] hits = Physics2D.RaycastAll(TargetEnemy.transform.position, direction, 10f * j, LayerMask.GetMask("Wall", "Edge"));
                if (hits.Length > 0) enemyBlockeds.Add((Vector2)TargetEnemy.transform.position + direction);
            }
        }
        // 그 공간들 중 내위치에서 Ray를 쏴서 내가 갈 수 있을 만한 길인지 판별한다.
        foreach(Vector2 enemyBlocked in enemyBlockeds)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, enemyBlocked, LayerMask.GetMask("Wall", "Edge"));
            if (hits.Length == 0) imNotBlockeds.Add(enemyBlocked);
        }
        // 그중 가장 가까운 곳을 반환
        if (imNotBlockeds.Count > 0)
        {
            float minDistance = float.MaxValue;
            foreach (Vector2 candidate in imNotBlockeds)
            {
                float distance = Vector2.Distance(transform.position, candidate);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    destination = candidate;
                }
            }
            runAwayFrom = TargetEnemy;
            return true;
        }
        else return false;
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
            float sightRange;
            if (i <= sightEdgeCount / 3) sightRange = leftSightRange;
            else if(i <= sightEdgeCount * 2 / 3) sightRange = Mathf.Max(rightSightRange, leftSightRange);
            else sightRange = rightSightRange;
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
        if (noiseMaker == this || inSightEnemies.Contains(noiseMaker as Survivor) || noiseMaker == lastTargetEnemy) return;
        float distance = Vector2.Distance(transform.position, soundOrigin);
        float heardVolume = volume * hearingAbility / (distance * distance);
        //Debug.Log($"{survivorName}, {(noiseMaker as Survivor).survivorName}, {heardVolume}");

        if(heardVolume > 1f)
        {
            // 어떤 소리인지 명확한 인지
            HeardDistinguishableSound(soundOrigin);
        }
        else if( heardVolume > 0.1f)
        {
            // 불분명한 인지
            HeardIndistinguishableSound((noiseMaker as Survivor).survivorName, soundOrigin);
        }
    }

    void HeardDistinguishableSound(Vector2 soundOrigin)
    {
        sightMeshRenderer.material = m_SightAlert;
        emotionAnimator.SetTrigger("Alert");
        if (strategyConditions[StrategyCase.HeardDistinguishableSound].TotalCondition.Invoke())
        {
            if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardDistinguishableSound].action == 0) threateningSoundPosition = soundOrigin;
            else if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardDistinguishableSound].action == 1) keepEyesOnPosition = soundOrigin;
        }
        else
        {
            if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardDistinguishableSound].elseAction == 0) threateningSoundPosition = soundOrigin;
            else if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardDistinguishableSound].elseAction == 1) keepEyesOnPosition = soundOrigin;
        }

    }

    void HeardIndistinguishableSound(string noiseMaker, Vector2 soundOrigin)
    {
        heardSound = noiseMaker;
        RememberSound sound = rememberSounds.Find(x => x.soundName == noiseMaker);
        if (sound != null)
        {
            if(sound.soundTime > 1)
            {
                sound.soundTime = 1;
                HeardDistinguishableSound(soundOrigin);
                return;
            }
        }
        else rememberSounds.Add(new(heardSound));

        sightMeshRenderer.material = m_SightSuspicious;
        emotionAnimator.SetTrigger("Suspicious");
        if (strategyConditions[StrategyCase.HeardIndistinguishableSound].TotalCondition.Invoke())
        {
            if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardIndistinguishableSound].action == 0) threateningSoundPosition = soundOrigin;
            else if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardIndistinguishableSound].action == 1) keepEyesOnPosition = soundOrigin;
        }
        else
        {
            if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardIndistinguishableSound].elseAction == 0) threateningSoundPosition = soundOrigin;
            else if (linkedSurvivorData.strategyDictionary[StrategyCase.HeardIndistinguishableSound].elseAction == 1) keepEyesOnPosition = soundOrigin;
        }

    }
    #endregion

    #region Take Damage
    void ApplyDamage(Survivor attacker, float damage, InjurySiteMajor damagePart, DamageType damageType)
    {
        if (isDead) return;
        if (damage > 0)
        {
            InjurySite specificDamagePart = GetSpecificDamagePart(damagePart, damageType);
            Injury alreadyHaveInjury = injuries.Find(x => x.site == specificDamagePart);
            bool damagedPartIsArtifical = false;
            bool noPain = false;
            if (alreadyHaveInjury != null)
            {
                if(alreadyHaveInjury.type == InjuryType.ArtificalPartsTransplanted)
                {
                    damagedPartIsArtifical = true;
                    // 재밌는 switch 용법
                    noPain = alreadyHaveInjury.site switch
                    {
                        InjurySite.Organ or InjurySite.RightEye or InjurySite.LeftEye or InjurySite.RightEar or InjurySite.LeftEar => false,
                        _ => true,
                    };
                }
                if(!damagedPartIsArtifical && (damagePart == InjurySiteMajor.Head || damagePart == InjurySiteMajor.Torso || alreadyHaveInjury.degree < 1)) damage *= 1 + alreadyHaveInjury.degree;
            }
            if(!(damagedPartIsArtifical && noPain))curHP -= damage;
            attacker.totalDamage += damage;
            if (curHP <= 0)
            {
                curHP = 0;
                attacker.killCount++;
                IsDead = true;
                if (damagePart == InjurySiteMajor.Head && damageType == DamageType.GunShot)
                {
                    GameObject headshot = PoolManager.Spawn(ResourceEnum.Prefab.Headshot, transform.position);
                    headshot.transform.SetParent(canvas.transform);
                }
                InGameUIManager.ShowKillLog(survivorName, attacker.survivorName);
            }

            if(damagedPartIsArtifical) GetDamageArtificalPart(alreadyHaveInjury, damage);
            else GetInjury(specificDamagePart, damageType, damage);
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
        DamageType damageType = DamageType.Strike;
        if (currentWeapon != null && IsValid(currentWeapon))
        {
            if(currentWeapon is MeleeWeapon) damageType = (currentWeapon as MeleeWeapon).DamageType;
        }

        InjurySiteMajor damagePart;
        // 타격 무기면 머리를 주로 노릴 것이고, 날붙이면 몸을 노릴 것이라 가정
        if ((UnityEngine.Random.Range(0, 1f) < 0.8f) ^ damageType == DamageType.Strike) damagePart = InjurySiteMajor.Torso;
        else damagePart = InjurySiteMajor.Head;

        if(!inSightEnemies.Contains(attacker))
        {
            // 시야 밖에서 맞으면 무조건 치명타
            damage *= 2;
            damagePart = InjurySiteMajor.Head;
            hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,10" : "hit01,10";
        }
        else
        {
            float probability = UnityEngine.Random.Range(0, 1f);
            float avoidRate = 0.2f * (moveSpeed / 3 + attackSpeed) / (attacker.moveSpeed / 3 + attacker.moveSpeed) * (meleeAvoidRate / attacker.meleeHitRate);
            float defendRate = 0.3f * meleeGuardRate;
            if (rightHandDisabled) defendRate -= defendRate * 0.5f;
            if (leftHandDisabled) defendRate -= defendRate * 0.5f;
            float criticalRate = 0.1f * attacker.meleeCriticalRate * 0.02f * attacker.luck / luck;

            if (probability < avoidRate)
            {
                // 회피
                damage = 0;
                hitSound = "avoid, 5";
            }
            else if (probability < avoidRate + defendRate)
            {
                // 방어
                damage *= 0.5f;
                damagePart = InjurySiteMajor.Arms;
                hitSound = "guard, 10";
            }
            else if (probability > 1 - criticalRate)
            {
                // 치명타
                damage *= 2;
                if (damageType == DamageType.Strike) damagePart = InjurySiteMajor.Head;
                else damagePart = InjurySiteMajor.Torso;
                hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,20" : "hit01,20";
            }
            else hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,20" : "hit01,20";
        }

        PlaySFX(hitSound, this);
        ApplyDamage(attacker, damage, damagePart, damageType);
    }

    public void TakeDamage(Bullet bullet)
    {
        if (isDead) return;
        float damage;
        float probability = UnityEngine.Random.Range(0, 1f);
        InjurySiteMajor damagePart;
        float headShotProbability = 0.01f * 0.02f * bullet.Launcher.luck / luck;
        float bodyShotProbability = 0.7f * 0.02f * bullet.Launcher.luck / luck;
        // 헤드샷
        if(probability < headShotProbability)
        {
            damage = bullet.Damage * 4;
            damagePart = InjurySiteMajor.Head;
        }
        // 바디샷
        else if(probability < bodyShotProbability)
        {
            damage = bullet.Damage * 2;
            damagePart = InjurySiteMajor.Torso;
        }
        else
        {
            damage = bullet.Damage;
            if(UnityEngine.Random.Range(0, 1f) < 0.5f) damagePart = InjurySiteMajor.Arms;
            else damagePart = InjurySiteMajor.Legs;
        }
        // 실효 사거리 밖
        if(bullet.TraveledDistance > bullet.MaxRange * 0.5f)
        {
            damage *= (bullet.MaxRange * 1.5f - bullet.TraveledDistance) / bullet.MaxRange;
        }

        if(damagePart == InjurySiteMajor.Head)
        {
            if (currentHelmet != null)
            {
                damage -= currentHelmet.Armor;
                if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                {
                    PlaySFX("ricochet,10", this);
                }
                else
                {
                    PlaySFX("ricochet2,10", this);
                }
            }
        }
        else if(damagePart == InjurySiteMajor.Torso)
        {
            if(currentVest != null) damage -= currentVest.Armor;
        }

        ApplyDamage(bullet.Launcher, damage, damagePart, DamageType.GunShot);
    }
    #endregion

    #region Injury
    InjurySite GetSpecificDamagePart(InjurySiteMajor damagePart, DamageType damageType)
    {
        InjurySite injurySite = InjurySite.None;
        float rand;
        switch (damagePart)
        {
            case InjurySiteMajor.Head:
                if (damageType == DamageType.Strike)
                {
                    if (linkedSurvivorData.characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) rand = UnityEngine.Random.Range(0, 2f);
                    else if (linkedSurvivorData.characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) rand = UnityEngine.Random.Range(0, 0.75f);
                    rand = UnityEngine.Random.Range(0, 1f);
                }
                else rand = UnityEngine.Random.Range(0, 0.5f);

                if (rand < 0.5f && rand > 0.45f) injurySite = InjurySite.RightEye;
                else if (rand > 0.4f) injurySite = InjurySite.LeftEye;
                else if (rand > 0.3f) injurySite = InjurySite.Head;
                else if (rand > 0.25f) injurySite = InjurySite.RightEar;
                else if (rand > 0.2f) injurySite = InjurySite.LeftEar;
                else if (rand > 0.1f) injurySite = InjurySite.Nose;
                else if (rand < 0.1f) injurySite = InjurySite.Jaw;
                break;
            case InjurySiteMajor.Torso:
                if (damageType == DamageType.Strike) rand = UnityEngine.Random.Range(0, 1f);
                else rand = UnityEngine.Random.Range(0, 0.5f);

                if (rand < 0.5f && rand > 0.25f) injurySite = InjurySite.Chest;
                else if (rand < 0.25f) injurySite = InjurySite.Abdomen;
                break;
            case InjurySiteMajor.Arms:
                rand = UnityEngine.Random.Range(0, 1f);
                if (rand > 0.7f) injurySite = InjurySite.RightArm;
                else if (rand > 0.4f) injurySite = InjurySite.LeftArm;
                else if (rand > 0.25f) injurySite = InjurySite.RightHand;
                else if (rand > 0.1f) injurySite = InjurySite.LeftHand;
                else if (rand > 0.09f) injurySite = InjurySite.RightThumb;
                else if (rand > 0.08f) injurySite = InjurySite.LeftThumb;
                else if (rand > 0.07f) injurySite = InjurySite.RightIndexFinger;
                else if (rand > 0.06f) injurySite = InjurySite.LeftIndexFinger;
                else if (rand > 0.05f) injurySite = InjurySite.RightMiddleFinger;
                else if (rand > 0.04f) injurySite = InjurySite.LeftMiddleFinger;
                else if (rand > 0.03f) injurySite = InjurySite.RightRingFinger;
                else if (rand > 0.02f) injurySite = InjurySite.LeftRingFinger;
                else if (rand > 0.01f) injurySite = InjurySite.RightLittleFinger;
                else injurySite = InjurySite.LeftLittleFinger;
                break;
            case InjurySiteMajor.Legs:
                rand = UnityEngine.Random.Range(0, 1f);
                if (rand > 0.75f) injurySite = InjurySite.RightLeg;
                else if (rand > 0.5f) injurySite = InjurySite.LeftLeg;
                else if (rand > 0.4f) injurySite = InjurySite.RightKnee;
                else if (rand > 0.3f) injurySite = InjurySite.LeftKnee;
                else if (rand > 0.2f) injurySite = InjurySite.RightAncle;
                else if (rand > 0.1f) injurySite = InjurySite.LeftAncle;
                else if (rand > 0.5f) injurySite = InjurySite.RightBigToe;
                else injurySite = InjurySite.LeftBigToe;
                break;
        }

        return injurySite;
    }

    void GetInjury(InjurySite injurySite, DamageType damageType, float damage)
    {
        InjuryType injuryType = InjuryType.Fracture;
        float injuryDegree = 0;
        switch (injurySite)
        {
            case InjurySite.Head:
                if(damageType == DamageType.Strike)
                {
                    injurySite = InjurySite.Skull;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    if (injuryDegree > 0.3f) injuryType = InjuryType.Fracture;
                    else injuryType = InjuryType.Contusion;
                    if(injuryDegree > 0.7f)
                    {
                        AddInjury(InjurySite.Brain, InjuryType.Concussion, Mathf.Clamp((injuryDegree - 0.7f) / 0.3f, 0, 0.99f));
                    }
                }
                else if(damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else
                {
                    injurySite = InjurySite.Skull;
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                break;
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 1);
                if(damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Contusion;
                    if (injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                }
                else if(damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                    AddInjury(injurySite, InjuryType.Loss, 1);
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                    AddInjury(injurySite, InjuryType.Loss, 1);
                }
                break;
            case InjurySite.RightEar:
            case InjurySite.LeftEar:
                if(damageType == DamageType.Strike)
                {
                    injuryDegree = 0;
                }
                else if (damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1);
                    if(injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                }
                break;
            case InjurySite.Nose:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if(damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Fracture;
                }
                else if(damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.Jaw:
                if (damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Fracture;
                    injuryDegree = Mathf.Clamp((damage - 20) / 80, 0, 0.99f);
                }
                else if (damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                break;
            case InjurySite.Chest:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if (damageType == DamageType.Strike)
                {
                    if (injuryDegree > 0.3f)
                    {
                        injurySite = InjurySite.Libs;
                        injuryType = InjuryType.Fracture;
                    }
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Cut)
                {
                    injuryType = InjuryType.Cutting;
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.Abdomen:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                if (damageType == DamageType.Strike)
                {
                    if (injuryDegree > 0.3f)
                    {
                        injurySite = InjurySite.Organ;
                        if (injuryDegree >= 1)
                        {
                            injuryType = InjuryType.Rupture;
                            AddInjury(InjurySite.Organ, InjuryType.Rupture, 1);
                        }
                        else injuryType = InjuryType.Damage;
                    }
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Cut)
                {
                    if (injuryDegree > 0.3f)
                    {
                        injurySite = InjurySite.Organ;
                        if (injuryDegree >= 1)
                        {
                            injuryType = InjuryType.Rupture;
                            AddInjury(InjurySite.Organ, InjuryType.Rupture, 1);
                        }
                        else injuryType = InjuryType.Cutting;
                    }
                    else injuryType = InjuryType.Cutting;
                }
                else
                {
                    if (injuryDegree > 0.3f)
                    {
                        injurySite = InjurySite.Organ;
                        if (injuryDegree >= 1)
                        {
                            injuryType = InjuryType.Rupture;
                            AddInjury(InjurySite.Organ, InjuryType.Rupture, 1);
                        }
                        else injuryType = InjuryType.GunshotWound;
                    }
                    else injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.RightArm:
            case InjurySite.LeftArm:
                if (damageType == DamageType.Strike)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    if (injuryDegree > 0.3f) injuryType = InjuryType.Fracture;
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Cut)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
                if (damageType == DamageType.Strike)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    if (injuryDegree > 0.3f) injuryType = InjuryType.Fracture;
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Cut)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.RightThumb:
            case InjurySite.LeftThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.LeftIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.RightLittleFinger:
            case InjurySite.LeftLittleFinger:
                injuryDegree = Mathf.Clamp(damage / 40, 0, 1f);
                if (injuryDegree >= 1f) AddInjury(injurySite, InjuryType.Loss, 1);
                else if (damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Fracture;
                }
                else if (damageType == DamageType.Cut)
                {
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else
                {
                    injuryType = InjuryType.GunshotWound;
                }
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
            case InjurySite.RightAncle:
            case InjurySite.LeftAncle:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                injuryType = InjuryType.GunshotWound;
                break;
            case InjurySite.RightBigToe:
            case InjurySite.LeftBigToe:
                injuryDegree = Mathf.Clamp(damage / 40, 0, 1f);
                if (injuryDegree >= 1) injuryType = InjuryType.Loss;
                else injuryType = InjuryType.GunshotWound;
                break;
            case InjurySite.None:
                injuryDegree = 0;
                break;
            default:
                Debug.LogWarning($"Unknown injurySite : {injurySite}");
                injuryDegree = 0;
                break;
        }
        if(injuryDegree > 0.1f)
        {
            AddInjury(injurySite, injuryType, injuryDegree);
            AddBleeding(injurySite, injuryType, injuryDegree);
        }
    }

    void GetDamageArtificalPart(Injury artificalPart, float damage)
    {
        float degree;
        switch(artificalPart.site)
        {
            case InjurySite.RightThumb:
            case InjurySite.LeftThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.LeftIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.RightLittleFinger:
            case InjurySite.LeftLittleFinger:
            case InjurySite.RightBigToe:
            case InjurySite.LeftBigToe:
                degree = Mathf.Clamp(damage / 40, 0, 1f);
                break;
            default:
                degree = Mathf.Clamp(damage / 100, 0, 1f);
                break;
        }
        AddInjury(artificalPart.site, InjuryType.ArtificalPartsTransplanted, degree);
    }

    void AddInjury(InjurySite injurySite, InjuryType injuryType, float injuryDegree)
    {
        if (injuryDegree == 0) return;
        if(injuryType == InjuryType.Loss || injuryType == InjuryType.Amputation || injuryType == InjuryType.Rupture)
        {
            int index = injuries.FindIndex(x => x.site == injurySite);
            if (index == -1)
            {
                injuries.Add(new(injurySite, injuryType, 1));
            }
            else
            {
                injuries[index].type = injuryType;
                injuries[index].degree = 1;
            }
            // 팔이 절단 됐으면 손, 손가락 부상 다 빼줘야함
            // 출혈도 빼줘야함? 보류
            List<InjurySite> subparts = GetSubparts(injurySite);
            List<Injury> toRemove = new();
            foreach(var injury in injuries)
            {
                if(subparts.Contains(injury.site) && injury.degree >= 1) toRemove.Add(injury);
            }
            foreach(var injury in toRemove)
            {
                injuries.Remove(injury);
            }
            ApplyInjuryPenalty();
        }
        else
        {
            // 상위 부위가 이미 절단된 상태면 return
            if (UpperPartAlreadyLoss(injurySite)) return;

            int index = injuries.FindIndex(x => x.site == injurySite);
            if (index != -1)
            {
                injuries[index].degree += injuryDegree;
                // dgree가 1이 되면 loss
                if (injuries[index].degree >= 1)
                {
                    switch(injuries[index].site)
                    {
                        // Unlosable
                        case InjurySite.Head:
                        case InjurySite.Skull:
                        case InjurySite.Brain:
                        case InjurySite.RightEar:
                        case InjurySite.LeftEar:
                        case InjurySite.Nose:
                        case InjurySite.Jaw:
                        case InjurySite.Chest:
                        case InjurySite.Libs:
                        case InjurySite.Abdomen:
                            injuries[index].degree = 0.99f;
                            break;
                        // Losable
                        default:
                            AddInjury(injurySite, InjuryType.Rupture, 1);
                            break;
                    }
                }
            }
            else
            {
                injuries.Add(new(injurySite, injuryType, injuryDegree));
                ApplyInjuryPenalty();
            }
        }
    }

    void AddBleeding(InjurySite injurySite, InjuryType injuryType, float injuryDegree)
    {
        // upper part 체크
        if (UpperPartAlreadyLoss(injurySite)) return;

        float amount = 0;
        switch (injurySite)
        {
            case InjurySite.RightThumb:
            case InjurySite.LeftThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.LeftIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.RightLittleFinger:
            case InjurySite.LeftLittleFinger:
            case InjurySite.RightBigToe:
            case InjurySite.LeftBigToe:
                if(injuryDegree >= 1) curBlood -= 10;
                amount = 20 * injuryDegree;
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
                if (injuryDegree >= 1) curBlood -= 100;
                amount = 100 * injuryDegree;
                break;
            case InjurySite.RightAncle:
            case InjurySite.LeftAncle:
                if (injuryDegree >= 1) curBlood -= 100;
                amount = 200 * injuryDegree;
                break;
            case InjurySite.RightArm:
            case InjurySite.LeftArm:
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
                if (injuryDegree >= 1) curBlood -= 250;
                amount = 400 * injuryDegree;
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
                if (injuryDegree >= 1) curBlood -= 500;
                amount = 600 * injuryDegree;
                break;
            case InjurySite.Head:
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
            case InjurySite.RightEar:
            case InjurySite.LeftEar:
            case InjurySite.Nose:
            case InjurySite.Jaw:
                amount = 100 * injuryDegree;
                break;
            case InjurySite.Chest:
                amount = 300 * injuryDegree;
                break;
            case InjurySite.Abdomen:
            case InjurySite.Organ:
                amount = 600 * injuryDegree;
                break;
        }
        if (injuryType == InjuryType.Damage || injuryType == InjuryType.GunshotWound) amount *= 0.5f;
        BleedingAmount += amount;
    }

    List<InjurySite> GetSubparts(InjurySite upperPart)
    {
        List<InjurySite> result = new();
        if(upperPart == InjurySite.RightArm || upperPart == InjurySite.RightHand)
        {
            result.Add(InjurySite.RightThumb);
            result.Add(InjurySite.RightIndexFinger);
            result.Add(InjurySite.RightMiddleFinger);
            result.Add(InjurySite.RightRingFinger);
            result.Add(InjurySite.RightLittleFinger);
            if(upperPart == InjurySite.RightArm)
            {
                result.Add(InjurySite.RightHand);
            }
        }
        else if (upperPart == InjurySite.LeftArm || upperPart == InjurySite.LeftHand)
        {
            result.Add(InjurySite.LeftThumb);
            result.Add(InjurySite.LeftIndexFinger);
            result.Add(InjurySite.LeftMiddleFinger);
            result.Add(InjurySite.LeftRingFinger);
            result.Add(InjurySite.LeftLittleFinger);
            if (upperPart == InjurySite.LeftArm)
            {
                result.Add(InjurySite.LeftHand);
            }
        }
        else if (upperPart == InjurySite.RightLeg || upperPart == InjurySite.RightKnee || upperPart == InjurySite.RightAncle)
        {
            result.Add(InjurySite.RightBigToe);
            if (upperPart == InjurySite.RightLeg || upperPart == InjurySite.RightKnee)
            {
                result.Add(InjurySite.RightAncle);
                if (upperPart == InjurySite.RightLeg) result.Add(InjurySite.RightKnee);
            }
        }
        else if (upperPart == InjurySite.LeftLeg || upperPart == InjurySite.LeftKnee || upperPart == InjurySite.LeftAncle)
        {
            result.Add(InjurySite.LeftBigToe);
            if (upperPart == InjurySite.LeftLeg || upperPart == InjurySite.LeftKnee)
            {
                result.Add(InjurySite.LeftAncle);
                if (upperPart == InjurySite.LeftLeg) result.Add(InjurySite.LeftKnee);
            }
        }

        return result;
    }

    List<InjurySite> GetUpperParts(InjurySite subpart)
    {
        List<InjurySite> result = new();
        switch(subpart)
        {
            case InjurySite.RightThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.RightLittleFinger:
                result.Add(InjurySite.RightHand);
                result.Add(InjurySite.RightArm);
                break;
            case InjurySite.LeftThumb:
            case InjurySite.LeftIndexFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.LeftLittleFinger:
                result.Add(InjurySite.LeftHand);
                result.Add(InjurySite.LeftArm);
                break;
            case InjurySite.RightHand:
                result.Add(InjurySite.RightArm);
                break;
            case InjurySite.LeftHand:
                result.Add(InjurySite.LeftArm);
                break;
            case InjurySite.RightBigToe:
                result.Add(InjurySite.RightAncle);
                result.Add(InjurySite.RightKnee);
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftBigToe:
                result.Add(InjurySite.LeftAncle);
                result.Add(InjurySite.LeftKnee);
                result.Add(InjurySite.LeftLeg);
                break;
            case InjurySite.RightAncle:
                result.Add(InjurySite.RightKnee);
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftAncle:
                result.Add(InjurySite.LeftKnee);
                result.Add(InjurySite.LeftLeg);
                break;
            case InjurySite.RightKnee:
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftKnee:
                result.Add(InjurySite.LeftLeg);
                break;
            default:
                break;
        }
        return result;
    }

    bool UpperPartAlreadyLoss(InjurySite injurySite)
    {
        List<InjurySite> upperParts = GetUpperParts(injurySite);
        foreach (var injury in injuries)
        {
            if (upperParts.Contains(injury.site) && injury.degree >= 1) return true;
        }
        return false;

    }

    float injuryCorrection_HearingAbility = 1;
    float injuryCorrection_FarmingSpeed = 1;
    float injuryCorrection_AttackSpeed = 1;
    float injuryCorrection_MoveSpeed = 1;
    float injuryCorrection_AttackDamage = 1;
    void ApplyInjuryPenalty()
    {
        InGameUIManager.UpdateSelectedObjectInjury(this);
        float ear1Penalty = 0;
        float ear2Penalty = 0;
        bool eyeInjured = false;
        float penaltiedFarmingSpeedByEyes = 0;
        float penaltiedFarmingSpeedByOrgan = 1;
        float penaltiedAttackSpeedByOrgan = 1;
        float penaltiedMoveSpeedByOrgan = 1;
        float penaltiedAttackDamageByRightArm = 1;
        float penaltiedAttackDamageByLeftArm = 1;
        float penaltiedMoveSpeedByRightLeg = 1;
        float penaltiedMoveSpeedByLeftLeg = 1;
        foreach(Injury injury in injuries)
        {
            switch(injury.site)
            {
                case InjurySite.RightEar:
                    ear1Penalty = injury.degree;
                    break;
                case InjurySite.LeftEar:
                    ear2Penalty = injury.degree;
                    break;
                case InjurySite.RightEye:
                    rightSightRange = 45 * (1 - injury.degree);
                    penaltiedFarmingSpeedByEyes = Mathf.Max(penaltiedFarmingSpeedByEyes, 1 - injury.degree);
                    affectionList_FarmingSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    eyeInjured = true;
                    break;
                case InjurySite.LeftEye:
                    leftSightRange = 45 * (1 - injury.degree);
                    penaltiedFarmingSpeedByEyes = Mathf.Max(penaltiedFarmingSpeedByEyes, 1 - injury.degree);
                    affectionList_FarmingSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    eyeInjured = true;
                    break;
                case InjurySite.Organ:
                    penaltiedFarmingSpeedByOrgan = 1 - injury.degree;
                    penaltiedAttackSpeedByOrgan = 1 - injury.degree;
                    penaltiedMoveSpeedByOrgan = 1 - injury.degree;
                    affectionList_FarmingSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    affectionList_AttackSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.RightArm:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightHand:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.5f);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightThumb:
                case InjurySite.RightIndexFinger:
                case InjurySite.RightMiddleFinger:
                case InjurySite.RightRingFinger:
                case InjurySite.RightLittleFinger:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.1f);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.LeftArm:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftHand:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.5f);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftThumb:
                case InjurySite.LeftIndexFinger:
                case InjurySite.LeftMiddleFinger:
                case InjurySite.LeftRingFinger:
                case InjurySite.LeftLittleFinger:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.1f);
                    affectionList_AttackDamage.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.RightLeg:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.RightKnee:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.RightAncle:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.RightBigToe:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, injury.degree * 0.1f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.LeftLeg:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.LeftKnee:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.LeftAncle:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, injury.degree * 0.5f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.LeftBigToe:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, injury.degree * 0.1f);
                    affectionList_MoveSpeed.Add($"<color=red>{injury.site} {injury.type}</color>");
                    break;
                case InjurySite.Brain:
                    dizzyRateByConcussion = injury.degree;
                    break;
            }
        }

        if(ear1Penalty < ear2Penalty)
        {
            (ear2Penalty, ear1Penalty) = (ear1Penalty, ear2Penalty);
        }
        injuryCorrection_HearingAbility = (1 - ear1Penalty) * (1 - 0.3f * ear2Penalty);

        if (!eyeInjured) penaltiedFarmingSpeedByEyes = 1;
        float penaltiedFarmingSpeedByHands = 1;
        if (rightHandDisabled && leftHandDisabled) penaltiedFarmingSpeedByHands = 0.1f;
        else if (rightHandDisabled || leftHandDisabled) penaltiedFarmingSpeedByHands = 0.7f;
        injuryCorrection_FarmingSpeed = penaltiedFarmingSpeedByEyes * penaltiedFarmingSpeedByOrgan * penaltiedFarmingSpeedByHands;

        injuryCorrection_AttackSpeed = penaltiedAttackSpeedByOrgan;

        injuryCorrection_MoveSpeed = penaltiedMoveSpeedByOrgan * penaltiedMoveSpeedByRightLeg * penaltiedMoveSpeedByLeftLeg;

        injuryCorrection_AttackDamage = Mathf.Max(penaltiedAttackDamageByLeftArm, penaltiedAttackDamageByRightArm);
        ApplyCorrectionStats();
    }
    #endregion

    #region Characteristic
    float characteristicCorrection_SightRange;
    float characteristicCorrection_HearingAbility;
    float characteristicCorrection_AttackDamage;
    float characteristicCorrection_AttackSpeed;
    float characteristicCorrection_MoveSpeed;
    float characteristicCorrection_FarmingSpeed;
    float characteristicCorrection_Shooting;
    float characteristicCorrection_MeleeHitRate;
    float characteristicCorrection_MeleeAvoidRate;
    float characteristicCorrection_MeleeGuardRate;
    float characteristicCorrection_MeleeCriticalRate;
    float characteristicCorrection_AimTime;
    float characteristicCorrection_NatualHemostasis;
    float characteristicCorrection_BloodRegeneration;
    float characteristicCorrection_HpRegeneration;
    float characteristicCorrection_Luck;

    public List<string> affectionList_AttackDamage = new();
    public List<string> affectionList_AttackSpeed = new();
    public List<string> affectionList_MoveSpeed = new();
    public List<string> affectionList_FarmingSpeed = new();
    public List<string> affectionList_Shooting = new();

    void ApplyCharacteristics()
    {
        characteristicCorrection_SightRange = 1;
        characteristicCorrection_HearingAbility = 1;
        characteristicCorrection_AttackDamage = 1;
        characteristicCorrection_AttackSpeed = 1;
        characteristicCorrection_MoveSpeed = 1;
        characteristicCorrection_FarmingSpeed = 1;
        characteristicCorrection_Shooting = 1;
        characteristicCorrection_MeleeHitRate = 1;
        characteristicCorrection_MeleeAvoidRate = 1;
        characteristicCorrection_MeleeGuardRate = 1;
        characteristicCorrection_MeleeCriticalRate = 1;
        characteristicCorrection_AimTime = 1;
        characteristicCorrection_NatualHemostasis = 1;
        characteristicCorrection_BloodRegeneration = 1;
        characteristicCorrection_HpRegeneration = 1;
        characteristicCorrection_Luck = 1;

        Calendar calender = GameManager.Instance.GetComponent<Calendar>();

        foreach (var characteristic in Characteristics)
        {
            switch(characteristic.type)
            {
                case CharacteristicType.EagleEye:
                    characteristicCorrection_SightRange *= 1.3f;
                    break;
                case CharacteristicType.BadEye:
                    characteristicCorrection_SightRange *= 0.7f;
                    break;
                case CharacteristicType.KeenHearing:
                    characteristicCorrection_HearingAbility *= 1.3f;
                    break;
                case CharacteristicType.BadHearing:
                    characteristicCorrection_HearingAbility *= 0.7f;
                    break;
                case CharacteristicType.ClutchPerformance:
                    if (calender.LeagueReserveInfo[calender.Today].league == League.SeasonChampionship || calender.LeagueReserveInfo[calender.Today].league == League.WorldChampionship)
                    {
                        characteristicCorrection_AttackDamage *= 1.1f;
                        characteristicCorrection_AttackSpeed *= 1.1f;
                        characteristicCorrection_MoveSpeed *= 1.1f;
                        characteristicCorrection_FarmingSpeed *= 1.1f;
                        characteristicCorrection_Shooting *= 1.1f;
                        affectionList_AttackDamage.Add($"<#009900>{characteristic.characteristicName}</color>");
                        affectionList_AttackSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                        affectionList_MoveSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                        affectionList_FarmingSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                        affectionList_Shooting.Add($"<#009900>{characteristic.characteristicName}</color>");
                    }
                    break;
                case CharacteristicType.ChokingUnderPressure:
                    if (calender.LeagueReserveInfo[calender.Today].league == League.SeasonChampionship || calender.LeagueReserveInfo[calender.Today].league == League.WorldChampionship)
                    {
                        characteristicCorrection_AttackDamage *= 0.9f;
                        characteristicCorrection_AttackSpeed *= 0.9f;
                        characteristicCorrection_MoveSpeed *= 0.9f;
                        characteristicCorrection_FarmingSpeed *= 0.9f;
                        characteristicCorrection_Shooting *= 0.9f;
                        affectionList_AttackDamage.Add($"<color=red>{characteristic.characteristicName}</color>");
                        affectionList_AttackSpeed.Add($"<color=red>{characteristic.characteristicName}</color>");
                        affectionList_MoveSpeed.Add($"<color=red>{characteristic.characteristicName}</color>");
                        affectionList_FarmingSpeed.Add($"<color=red>{characteristic.characteristicName}</color>");
                        affectionList_Shooting.Add($"<color=red>{characteristic.characteristicName}</color>");
                    }
                    break;
                case CharacteristicType.Boxer:
                    characteristicCorrection_AttackDamage *= 1.2f;
                    characteristicCorrection_AttackSpeed *= 1.2f;
                    characteristicCorrection_MeleeHitRate *= 1.5f;
                    characteristicCorrection_MeleeAvoidRate *= 1.5f;
                    characteristicCorrection_MeleeGuardRate *= 1.5f;
                    characteristicCorrection_MeleeCriticalRate *= 1.5f;
                    affectionList_AttackDamage.Add($"<#009900>{characteristic.characteristicName}</color>");
                    affectionList_AttackSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                    break;
                case CharacteristicType.Giant:
                    maxHP *= 1.3f;
                    curHP = maxHP;
                    transform.localScale = new(1.3f, 1.3f);
                    foreach(Transform child in rightHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in leftHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in transform.Find("Head")) child.localScale = new(0.77f, 0.77f);
                    characteristicCorrection_AttackDamage *= 1.3f;
                    characteristicCorrection_AttackSpeed *= 0.7f;
                    characteristicCorrection_MoveSpeed *= 0.7f;
                    affectionList_AttackDamage.Add($"<#009900>{characteristic.characteristicName}</color>");
                    affectionList_AttackSpeed.Add($"<color=red>{characteristic.characteristicName}</color>");
                    affectionList_MoveSpeed.Add($"<color=red>{characteristic.characteristicName}</color>");
                    break;
                case CharacteristicType.Dwarf:
                    maxHP *= 0.7f;
                    curHP = maxHP;
                    transform.localScale = new(0.7f, 0.7f);
                    foreach (Transform child in rightHand.transform) child.localScale = new(1.43f, 1.43f);
                    foreach (Transform child in leftHand.transform) child.localScale = new(1.43f, 1.43f);
                    foreach (Transform child in transform.Find("Head")) child.localScale = new(1.43f, 1.43f);
                    characteristicCorrection_AttackDamage *= 0.7f;
                    characteristicCorrection_AttackSpeed *= 1.3f;
                    characteristicCorrection_MoveSpeed *= 1.3f;
                    affectionList_AttackDamage.Add($"<color=red>{characteristic.characteristicName}</color>");
                    affectionList_AttackSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                    affectionList_MoveSpeed.Add($"<#009900>{characteristic.characteristicName}</color>");
                    break;
                case CharacteristicType.CarefulShooter:
                    characteristicCorrection_AimTime *= 2;
                    characteristicCorrection_Shooting *= 2;
                    affectionList_Shooting.Add($"<#009900>{characteristic.characteristicName}</color>");
                    break;
                case CharacteristicType.Fragile:
                    characteristicCorrection_NatualHemostasis *= 0.5f;
                    break;
                case CharacteristicType.Sturdy:
                    characteristicCorrection_NatualHemostasis *= 2;
                    break;
                case CharacteristicType.Avenger:
                    characteristicCorrection_NatualHemostasis *= 0.5f;
                    break;
                case CharacteristicType.Regenerator:
                    characteristicCorrection_NatualHemostasis *= 5f;
                    characteristicCorrection_BloodRegeneration *= 5f;
                    characteristicCorrection_HpRegeneration *= 5f;
                    break;
                case CharacteristicType.UpsAndDowns:
                    float condition = UnityEngine.Random.Range(0.7f, 1.3f);
                    characteristicCorrection_AttackDamage *= condition;
                    characteristicCorrection_AttackSpeed *= condition;
                    characteristicCorrection_MoveSpeed *= condition;
                    characteristicCorrection_FarmingSpeed *= condition;
                    characteristicCorrection_Shooting *= condition;
                    break;
                case CharacteristicType.LuckGuy:
                    characteristicCorrection_Luck *= 1.2f;
                    break;
                case CharacteristicType.TheCursed:
                    characteristicCorrection_Luck *= 0.8f;
                    break;
                default:
                    Debug.LogWarning($"Unknown CharacteristicType : {characteristic.type}");
                    break;
            }
        }
    }
    #endregion

    void ApplyCorrectionStats()
    {
        leftSightRange = 45 * characteristicCorrection_SightRange;
        rightSightRange = 45 * characteristicCorrection_SightRange;
        hearingAbility = 10 * injuryCorrection_HearingAbility * characteristicCorrection_HearingAbility;
        attackDamage = linkedSurvivorData.attackDamage * injuryCorrection_AttackDamage * characteristicCorrection_AttackDamage;
        attackSpeed = Mathf.Max(linkedSurvivorData.attackSpeed * injuryCorrection_AttackSpeed * characteristicCorrection_AttackSpeed, 0.1f);
        moveSpeed = Mathf.Max(linkedSurvivorData.moveSpeed * injuryCorrection_MoveSpeed * characteristicCorrection_MoveSpeed, 0.1f);
        farmingSpeed = Mathf.Max(linkedSurvivorData.attackSpeed * injuryCorrection_FarmingSpeed * characteristicCorrection_FarmingSpeed, 0.1f);
        shooting = linkedSurvivorData.shooting * characteristicCorrection_Shooting;
        aimErrorRange = 7.5f / shooting;
        luck = linkedSurvivorData.luck * characteristicCorrection_Luck;
        meleeHitRate = characteristicCorrection_MeleeHitRate;
        meleeAvoidRate = characteristicCorrection_MeleeAvoidRate;
        meleeGuardRate = characteristicCorrection_MeleeGuardRate;
        meleeCriticalRate = characteristicCorrection_MeleeCriticalRate;
        aimDelay = 1.5f * characteristicCorrection_AimTime;
        naturalHemostasis = characteristicCorrection_NatualHemostasis;
        bloodRegeneration = characteristicCorrection_BloodRegeneration;
        hpRegeneration = characteristicCorrection_HpRegeneration;
        InGameUIManager.UpdateSelectedObjectStat(this);
    }

    #region Animation Events
    void AE_Attack()
    {
        if (inSightEnemies.Count == 0) return;
        if(IsValid(currentWeapon) && currentWeapon is MeleeWeapon)
        {
            if (Vector2.Distance(transform.position, inSightEnemies[0].transform.position) < currentWeapon.AttackRange)
            {
                float damage = currentWeapon.AttackDamage + attackDamage;
                if (currentWeapon.NeedHand == NeedHand.OneOrTwoHand && (rightHandDisabled || leftHandDisabled)) damage *= 0.7f;
                inSightEnemies[0].TakeDamage(this, damage);
            }
            else PlaySFX("avoid, 5", this);
        }
        else
        {
            if (Vector2.Distance(transform.position, inSightEnemies[0].transform.position) < attackRange)
            {
                inSightEnemies[0].TakeDamage(this, attackDamage);
            }
            else PlaySFX("avoid, 5", this);
        }
    }

    void AE_Reload()
    {
        int amount;
        if (currentWeapon.itemName == "ShotGun") amount = 1;
        else amount = Math.Clamp(ValidBullet.amount, 1, CurrentWeaponAsRangedWeapon.MagazineCapacity - CurrentWeaponAsRangedWeapon.CurrentMagazine);
        ConsumptionItem(ValidBullet, amount);
        CurrentWeaponAsRangedWeapon.Reload(amount);
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void AE_Taping()
    {
        ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.BandageRoll), 1);
        BleedingAmount -= 100;
    }
    #endregion

    #region Trigger Events
    float curSeeEnemy = 0;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!BattleRoyaleManager.isBattleRoyaleStart || isDead) return;
        if (!collision.isTrigger)
        {
            if (collision.TryGetComponent(out Survivor survivor))
            {
                if (!inSightEnemies.Contains(survivor))
                {
                    curSeeEnemy += Time.deltaTime;
                    if(curSeeEnemy > 0.1f)
                    {
                        inSightEnemies.Add(survivor);
                        sightMeshRenderer.material = m_SightAlert;
                        if(survivor != lastTargetEnemy) emotionAnimator.SetTrigger("Alert");
                        curSeeEnemy = 0;
                    }
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
                if(survivor == TargetEnemy && survivor != isDead)
                {
                    targetEnemiesLastPosition = survivor.transform.position;
                    lastTargetEnemy = survivor;
                    inSightEnemies.Remove(survivor);
                    curSeeEnemy = 0;
                }
            }
        }
    }
    #endregion

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

    public Area GetCurrentArea()
    {
        float distance;
        float minDistance = float.MaxValue;
        Area nearest = null;
        foreach(var area in farmingAreas)
        {
            Transform areaTransform = area.Key.transform;
            distance = Vector2.Distance(transform.position, areaTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = area.Key;
            }
        }
        return nearest;
    }

    public void SetSurvivorInfo(SurvivorData survivorInfo)
    {
        linkedSurvivorData = survivorInfo;
        survivorName = survivorInfo.survivorName;
        nameTag.GetComponent<TextMeshProUGUI>().text = survivorInfo.survivorName;
        curHP = maxHP = survivorInfo.hp;
        curBlood = maxBlood = survivorInfo.hp * 80;
        bleedingSprite = curBlood - 100;
        attackDamage = survivorInfo.attackDamage;
        attackSpeed = survivorInfo.attackSpeed;
        moveSpeed = survivorInfo.moveSpeed;
        farmingSpeed = survivorInfo.farmingSpeed;
        shooting = survivorInfo.shooting;
        luck = survivorInfo.luck;
        aimErrorRange = 7.5f / survivorInfo.shooting;
        charicteristics = survivorInfo.characteristics;

        injuries = survivorInfo.injuries;
        foreach(Injury injury in injuries)
        {
            if (injury.type == InjuryType.ArtificalPartsTransplanted || injury.degree == 1) rememberAlreadyHaveInjury.Add(injury.site);
        }
        ApplyCharacteristics();
        ApplyInjuryPenalty();
        ApplyStrategies();
    }

    void ApplyStrategies()
    {
        foreach (var strategyDictionary in linkedSurvivorData.strategyDictionary)
        {
            Conditions condition = new()
            {
                conditions = new Condition[5]
            };
            for (int i = 0; i < 5; i++)
            {
                if (i < strategyDictionary.Value.conditionConut)
                {
                    switch(strategyDictionary.Value.conditions[i].variable1)
                    {
                        // My Weapon
                        case 0:
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    switch(strategyDictionary.Value.conditions[i].variable2)
                                    {
                                        case 0:
                                            condition.conditions[i] = () => currentWeapon is MeleeWeapon;
                                            break;
                                        case 1:
                                            condition.conditions[i] = () => currentWeapon is RangedWeapon;
                                            break;
                                        case 2:
                                            condition.conditions[i] = () => currentWeapon == null || (currentWeapon is RangedWeapon && ValidBullet == null);
                                            break;
                                    }
                                    break;
                                case 1:
                                    switch (strategyDictionary.Value.conditions[i].variable2)
                                    {
                                        case 0:
                                            condition.conditions[i] = () => currentWeapon is not MeleeWeapon;
                                            break;
                                        case 1:
                                            condition.conditions[i] = () => currentWeapon is not RangedWeapon;
                                            break;
                                        case 2:
                                            condition.conditions[i] = () => currentWeapon is MeleeWeapon || (currentWeapon is RangedWeapon && ValidBullet != null);
                                            break;
                                    }
                                    break;
                            }
                            break;
                        // The enemy's weapon
                        case 1:
                            switch (strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    switch (strategyDictionary.Value.conditions[i].variable2)
                                    {
                                        case 0:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon is MeleeWeapon;
                                            break;
                                        case 1:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon is RangedWeapon;
                                            break;
                                        case 2:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon == null;
                                            break;
                                    }
                                    break;
                                case 1:
                                    switch (strategyDictionary.Value.conditions[i].variable2)
                                    {
                                        case 0:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon is not MeleeWeapon;
                                            break;
                                        case 1:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon is not RangedWeapon;
                                            break;
                                        case 2:
                                            condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.currentWeapon != null;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        // My HP
                        case 2:
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    condition.conditions[i] = () => CurHPPercent > strategyDictionary.Value.conditions[i].inputInt;
                                    break;
                                case 1:
                                    condition.conditions[i] = () => CurHPPercent < strategyDictionary.Value.conditions[i].inputInt;
                                    break;
                            }
                            break;
                        // The enemy
                        case 3:
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    condition.conditions[i] = () => TargetEnemy != null && TargetEnemy.inSightEnemies.Contains(this);
                                    break;
                                case 1:
                                    condition.conditions[i] = () => TargetEnemy != null && !TargetEnemy.inSightEnemies.Contains(this);
                                    break;
                            }
                            break;
                        // Distance with the enemy
                        case 4:
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    condition.conditions[i] = () => TargetEnemy != null && Vector2.Distance(transform.position, TargetEnemy.transform.position) > strategyDictionary.Value.conditions[i].inputInt;
                                    break;
                                case 1:
                                    condition.conditions[i] = () => TargetEnemy != null && Vector2.Distance(transform.position, TargetEnemy.transform.position) < Mathf.Min(strategyDictionary.Value.conditions[i].inputInt, 1.5f);
                                    break;
                            }
                            break;
                    }
                }
                else condition.conditions[i] = () => true;

                if (strategyDictionary.Value.conditions[1].andOr == 0)
                {
                    if (strategyDictionary.Value.conditions[2].andOr == 0)
                    {
                        if (strategyDictionary.Value.conditions[3].andOr == 0)
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() && condition.conditions[2].Invoke() && condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() && condition.conditions[2].Invoke() && condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                        else
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() && condition.conditions[2].Invoke() || condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() && condition.conditions[2].Invoke() || condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                    }
                    else
                    {
                        if (strategyDictionary.Value.conditions[3].andOr == 0)
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() || condition.conditions[2].Invoke() && condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() || condition.conditions[2].Invoke() && condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                        else
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() || condition.conditions[2].Invoke() || condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() && condition.conditions[1].Invoke() || condition.conditions[2].Invoke() || condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                    }
                }
                else
                {
                    if (strategyDictionary.Value.conditions[2].andOr == 0)
                    {
                        if (strategyDictionary.Value.conditions[3].andOr == 0)
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() && condition.conditions[2].Invoke() && condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() && condition.conditions[2].Invoke() && condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                        else
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() && condition.conditions[2].Invoke() || condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() && condition.conditions[2].Invoke() || condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                    }
                    else
                    {
                        if (strategyDictionary.Value.conditions[3].andOr == 0)
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() || condition.conditions[2].Invoke() && condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() || condition.conditions[2].Invoke() && condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                        else
                        {
                            if (strategyDictionary.Value.conditions[4].andOr == 0) condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() || condition.conditions[2].Invoke() || condition.conditions[3].Invoke() && condition.conditions[4].Invoke();
                            else condition.TotalCondition = () => condition.conditions[0].Invoke() || condition.conditions[1].Invoke() || condition.conditions[2].Invoke() || condition.conditions[3].Invoke() || condition.conditions[4].Invoke();
                        }
                    }
                }
            }

            strategyConditions.Add(strategyDictionary.Key, condition);
        }

    }

    private void OnDrawGizmos()
    {
        if(isDead) return;
    }
}

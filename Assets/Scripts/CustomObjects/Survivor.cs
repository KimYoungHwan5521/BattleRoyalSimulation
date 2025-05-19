using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Survivor : CustomObject
{
    #region Variables and Properties
    public enum Status { Farming, FarmingBox, InCombat, TraceEnemy, InvestigateThreateningSound, Maintain, RunAway, Trapping, TrapDisarming, Crafting }
    #region Components
    [Header("Components")]
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject leftHand;
    [SerializeField] PolygonCollider2D sightCollider;
    [SerializeField] CircleCollider2D bodyCollider;
    [SerializeField] SpriteRenderer[] bodySprites;
    [SerializeField] GameObject trapDetectionDevice;
    [SerializeField] GameObject biometricRader;
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
    [SerializeField] Image progressBar;
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

                trapDetectionDevice.SetActive(false);
                biometricRader.SetActive(false);
                sightMeshFilter.gameObject.SetActive(false);
                emotion.SetActive(false);
                nameTag.SetActive(false);
                prohibitTimer.SetActive(false);
                progressBar.fillAmount = 0;

                currentFarmingArea = GetCurrentArea();
                GameManager.Instance.BattleRoyaleManager.SurvivorDead(this);
            }
        }
    }
    public int survivorID;
    public string survivorName;
    [SerializeField] Status currentStatus;
    public Status CurrentStatus
    {
        get => currentStatus;
        set
        {
            currentStatus = value;
            progressBar.fillAmount = 0;
            InGameUIManager.UpdateSelectedObjectStatus(this);
        }
    }
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

    [Serializable]
    public class SoundsMemory
    {
        public string soundName;
        public float soundTime;

        public SoundsMemory(string soundName)
        {
            this.soundName = soundName;
            soundTime = 0;
        }
    }
    [SerializeField]List<SoundsMemory> soundsMemories = new();
    [SerializeField] float farmingSpeed = 1f;
    public float FarmingSpeed => farmingSpeed;
    [SerializeField] float aimErrorRange = 7.5f;
    public float AimErrorRange => aimErrorRange;
    float luck = 50;
    public float Luck => luck;

    float bloodRegeneration = 1;
    float hpRegeneration = 1;
    #endregion
    #region Characteristic
    [Header("Characteristic")]
    [SerializeField] List<Characteristic> charicteristics;
    public List<Characteristic> Characteristics => charicteristics;
    bool isAssassin;
    #endregion
    #region Injury
    [Header("Injury")]
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
    bool isBlind;
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

    bool poisoned;
    Survivor curPoisonOriginator;
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
            if (CurrentWeaponAsRangedWeapon == null) return null;
            string bulletName;
            if (CurrentWeapon.itemType != ItemManager.Items.Bazooka) bulletName = $"Bullet_{currentWeapon.itemType}";
            else bulletName = "Rocket_Bazooka";
            if (!Enum.TryParse(bulletName, out ItemManager.Items bullet))
            {
                Debug.LogWarning($"Wrong bullet name : {bulletName}");
                return null;
            }

            Item validBullet = inventory.Find(x => x.itemType == bullet);
            return validBullet;
        }
    }
    bool currentWeaponisBestWeapon;

    [SerializeField]List<ItemManager.Craftable> craftables = new();
    ItemManager.Craftable currentCrafting;
    public ItemManager.Craftable CurrentCrafting => currentCrafting;
    int AdvancedComponentCount
    {
        get
        {
            Item item = inventory.Find(x => x.itemType == ItemManager.Items.AdvancedComponent);
            return item != null ? item.amount : 0;
        }
    }
    int ComponentsCount
    {
        get
        {
            Item item = inventory.Find(x => x.itemType == ItemManager.Items.Components);
            return item != null ? item.amount : 0;
        }
    }
    int ChemicalsCount
    {
        get
        {
            Item item = inventory.Find(x => x.itemType == ItemManager.Items.Chemicals);
            return item != null ? item.amount : 0;
        }
    }

    int GunpowderCount
    {
        get
        {
            Item item = inventory.Find(x => x.itemType == ItemManager.Items.Gunpowder);
            return item != null ? item.amount : 0;
        }
    }

    int SalvagesCount
    {
        get
        {
            Item item = inventory.Find(x => x.itemType == ItemManager.Items.Salvages);
            return item != null ? item.amount : 0;
        }
    }
    Item curEnchanting;
    Item curDrinking;
    float curCraftingTime;
    float craftingSpeed = 1f;
    #endregion
    #region Trap
    public TrapPlace trapPlace;
    [SerializeField] float trappingTime = 3f;
    [SerializeField] float curTrappingTime;
    Buriable curBurying;
    Item curSettingBoobyTrap;
    Box curSettingBoobyTrapBox;

    public List<GameObject> burieds = new();
    List<Trap> detectedTraps = new();
    List<Box> detectedTrapSetBoxes = new();
    Trap curDisarmTrap;
    float curDisarmTime;
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
                if(!farmingSections.ContainsKey(farmingSection))
                {
                    farmingSections.Add(farmingSection, false);
                    foreach(var box in farmingSection.boxes) if(!farmingBoxes.ContainsKey(box)) farmingBoxes.Add(box, false);
                }
            }
        }
    }
    [SerializeField] Area lastFarmingArea;
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
    [SerializeField] Vector2 lookPosition = Vector2.zero;
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
    #region Footstep
    [Header("Footstep")]
    float footstepDistance = 1f;
    float curMoveDistance = 0;
    Vector2 lastPosition;
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
    bool lastArea;
    #endregion
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        projectileGenerator = GetComponent<ProjectileGenerator>();
        emotionAnimator = emotion.GetComponent<Animator>();
        sightMeshRenderer = sightMeshFilter.GetComponent<MeshRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        emotion.transform.parent = null;
        canvas.transform.SetParent(null);
        prohibitTimer.SetActive(false);
        progressBar.fillAmount = 0;

        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;

        sightVertices = new Vector3[sightEdgeCount + 1 + 1];  // +1은 원점을 포함
        sightTriangles = new int[(sightEdgeCount + 1) * 3];     // 삼각형 그리기
        sightColliderPoints = new Vector2[sightVertices.Length];
        m_SightNormal = ResourceManager.Get(ResourceEnum.Material.Sight_Normal);
        m_SightSuspicious = ResourceManager.Get(ResourceEnum.Material.Sight_Suspicious);
        m_SightAlert = ResourceManager.Get(ResourceEnum.Material.Sight_Alert);
    }

    override public void MyUpdate()
    {
        if(!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart || isDead)
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

        if (FeelDizzy()) return;
        PlayFootstepNoise();

        RememberSound();
        AI();

        Look();
        if(!isBlind) DrawSightMesh();
    }

    public override void MyDestroy()
    {
        Destroy(emotion);
        Destroy(canvas);
        base.MyDestroy();
    }

    float curFixedUpdateCool;
    private void FixedUpdate()
    {
        if(!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart || isDead) return;
        if (isBlind) return;

        curFixedUpdateCool += Time.fixedDeltaTime;
        if(curFixedUpdateCool > GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count * 0.02f)
        {
            curFixedUpdateCool = 0;
            CalculateSightMesh();
        }
        if(runAwayFrom != null)
        {
            CanRunAway(out runAwayDestination);
        }
    }

    #region Look
    void Look()
    {
        if (keepEyesOnPosition != Vector2.zero)
        {
            curKeepAnEyeOnTime += Time.fixedDeltaTime;
            Look(keepEyesOnPosition);
            if (curKeepAnEyeOnTime > keepAnEyeOnTime)
            {
                keepEyesOnPosition = Vector2.zero;
                curKeepAnEyeOnTime = 0;
            }
        }
        else if (lookPosition != Vector2.zero)
        {
            Look(lookPosition);
        }
        else
        {
            if(agent.velocity.magnitude > 0) Look((Vector2)(transform.position + agent.velocity));
        }
    }

    void Look(Vector2 targetPosition)
    {
        // Vector2 currentLookVector = new(Mathf.Cos((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad));
        Vector2 currentLookVector = transform.up;
        Vector2 preferDirection = targetPosition - (Vector2)transform.position;
        if(Vector2.Angle(currentLookVector, preferDirection) > 5f)
        {
            float direction = Vector2.SignedAngle(currentLookVector, preferDirection) > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, transform.localEulerAngles.z + direction * 200 * Time.deltaTime * aiCool);
        }
    }

    void LookAround()
    {
        curLookAroundTime += Time.deltaTime * aiCool;
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
            //lookPosition = new Vector2(Mathf.Cos(transform.eulerAngles.z), Mathf.Sin(transform.eulerAngles.z)).Rotate(120);
            lookPosition = Vector2.up.Rotate(120) + (Vector2)transform.position;
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
        curBlood = Mathf.Min(curBlood + bloodRegeneration * Time.deltaTime, maxBlood);
        if(poisoned) ApplyPoisonDamage(curPoisonOriginator);
        else curHP = Mathf.Min(curHP + 0.17f * hpRegeneration * Time.deltaTime, maxHP);
    }

    bool FeelDizzy()
    {
        if (DizzyRate > 0)
        {
            if (dizzy)
            {
                curDizzyDuration += Time.deltaTime;
                if (curDizzyDuration > dizzyDuration)
                {
                    Dizzy = false;
                }
                else return true;
            }
            else
            {
                curDizzyCool += Time.deltaTime;
                if (curDizzyCool > dizzyCoolTime)
                {
                    if (UnityEngine.Random.Range(0, 1f) < dizzyRateByConcussion) Dizzy = true;
                    curDizzyCool = 0;
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    void RememberSound()
    {
        List<SoundsMemory> reserveRemove = new();
        foreach (var sound in soundsMemories)
        {
            sound.soundTime += Time.fixedDeltaTime;
            if (sound.soundTime > 10) reserveRemove.Add(sound);
        }
        foreach (var sound in reserveRemove)
        {
            soundsMemories.Remove(sound);
        }
    }

    int curAICool;
    int aiCool = 1;
    void AI()
    {
        //if (curAICool++ < GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count) return;
        //aiCool = GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count;
        //curAICool = 0;
        if(isBlind)
        {
            agent.SetDestination(transform.position);
            if(CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
            {
                if (threateningSoundPosition != Vector2.zero) lookPosition = threateningSoundPosition;
                else if (keepEyesOnPosition != Vector2.zero) lookPosition = keepEyesOnPosition;
                else return;

                Aim();
            }
        }
        else if (inSightEnemies.Count == 0)
        {
            animator.SetBool("Attack", false);
            animator.SetBool("Aim", false);
            curAimDelay = 0;

            if(CurrentFarmingArea != null && (CurrentFarmingArea.IsProhibited_Plan || CurrentFarmingArea.IsProhibited))
            {
                CurrentFarmingArea = FindNearest(farmingAreas);
            }

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
                if(Maintain())
                {
                    CurrentStatus = Status.Maintain;
                    return;
                }

                if(detectedTraps.Count > 0)
                {
                    DisarmTrap();
                }
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
                TryFarming();
            }
        }
        else
        {
            CurrentStatus = Status.InCombat;
            // 대상이 죽어버릴 경우
            if (TargetEnemy.isDead)
            {
                if (!farmingCorpses.ContainsKey(TargetEnemy))
                {
                    farmingCorpses.Add(TargetEnemy, false);
                    targetFarmingCorpse = TargetEnemy;
                }
                else if (!farmingCorpses[TargetEnemy])
                {
                    targetFarmingCorpse = TargetEnemy;
                }
                inSightEnemies.Remove(TargetEnemy);
                targetEnemiesLastPosition = Vector2.zero;
                lastTargetEnemy = null;

                CurrentFarmingArea = FindNearest(farmingAreas);
                targetFarmingSection = FindNearest(farmingSections);
                targetFarmingBox = FindNearest(farmingBoxes);
                targetFarmingCorpse = FindNearest(farmingCorpses);
            }
            else
            {
                lookPosition = TargetEnemy.transform.position;
                float distance = Vector2.Distance(transform.position, TargetEnemy.transform.position);
                bool enemyInAttackRange = false;
                if (IsValid(currentWeapon))
                {
                    if (CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.CurrentMagazine == 0)
                    {
                        if (ValidBullet != null)
                        {
                            Reload();
                            return;
                        }
                        // 원거린데 총알 없으면 attackRange
                        if (distance < attackRange) enemyInAttackRange = true;
                    }
                    // 근거리거나 원거리+총알 있으면 weapon.AttackRange
                    else if (distance < currentWeapon.AttackRange) enemyInAttackRange = true;
                }
                else if (distance < attackRange) enemyInAttackRange = true;

                if (enemyInAttackRange)
                {
                    if(strategyConditions[StrategyCase.SawAnEnemyAndItIsInAttackRange].TotalCondition.Invoke())
                    {
                        if(linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 2)
                        {
                            if (runAwayFrom != null && TargetEnemy != runAwayFrom) Combat(distance);
                            else if (!TryRunAway(TargetEnemy)) Combat(distance);
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 0)
                        {
                            Combat(distance);
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 1)
                        {
                            TryFarming();
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
                            TryFarming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 2)
                        {
                            if(!TryRunAway(TargetEnemy)) Combat(distance);
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
                            TryFarming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 2)
                        {
                            if (!TryRunAway(TargetEnemy)) ApproachEnemy(TargetEnemy);
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
                            TryFarming();
                        }
                        else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction == 2)
                        {
                            if(!TryRunAway(TargetEnemy)) ApproachEnemy(TargetEnemy);
                        }
                    }
                }
            }
        }
    }

    bool Maintain()
    {
        if (BleedingAmount >= 30 || (BleedingAmount > 0 && (BleedingAmount) * (BleedingAmount - 1) / 2 + curBlood > maxBlood / 2))
        {
            if (inventory.Find(x => x.itemType == ItemManager.Items.BandageRoll) != null || inventory.Find(x => x.itemType == ItemManager.Items.HemostaticBandageRoll) != null)
            {
                StopBleeding();
                return true;
            }
        }
        animator.SetBool("StopBleeding", false);

        if (poisoned)
        {
            if (curDrinking == null)
            {
                Item antidote = inventory.Find(x => x.itemType == ItemManager.Items.Antidote);
                if (antidote != null)
                {
                    curDrinking = antidote;
                    return true;
                }
            }
            else
            {
                agent.destination = transform.position;
                animator.SetBool("Drinking", true);
                return true;
            }
        }
        else if(maxHP - curHP > 50)
        {
            if (curDrinking == null)
            {
                Item potion = inventory.Find(x => x.itemType == ItemManager.Items.Potion);
                if (potion != null)
                {
                    curDrinking = potion;
                    return true;
                }
            }
            else
            {
                agent.destination = transform.position;
                animator.SetBool("Drinking", true);
                return true;
            }
        }
        animator.SetBool("Drinking", false);

        if (CurrentWeaponAsRangedWeapon != null)
        {
            if (projectileGenerator.muzzleTF == null) projectileGenerator.ResetMuzzleTF(rightHandDisabled ? leftHand.transform : rightHand.transform);
            if (CurrentWeaponAsRangedWeapon.CurrentMagazine < CurrentWeaponAsRangedWeapon.MagazineCapacity && ValidBullet != null)
            {
                sightMeshRenderer.material = m_SightNormal;
                Reload();
                return true;
            }
        }
        animator.SetBool("Reload", false);
        return false;
    }

    #region Farming
    void TryFarming()
    {
        if(!GetCurrentArea().IsProhibited_Plan && !GetCurrentArea().IsProhibited && !(RightHandDisabled && LeftHandDisabled))
        {
            if (SetBoobyTrap()) return;
            if (currentCrafting != null)
            {
                Craft();
                return;
            }
            else if (Crafting())
            {
                curCraftingTime = 0;
                return;
            }
            if (Enchanting()) return;
            animator.SetBool("Crafting", false);
            if (trapPlace != null && trapPlace.BuriedTrap == null)
            {
                if (BuryTrap()) return;
            }
        }
        animator.SetBool("Crafting", false);
        Farming();
    }

    void Farming()
    {
        lookPosition = Vector2.zero;
        CurrentStatus = Status.Farming;
        sightMeshRenderer.material = m_SightNormal;
        if (!(RightHandDisabled && leftHandDisabled))
        {
            // 파밍을 하는 경우:
            // 1. 막금구가 아님
            // 2. 막금구인 경우 : 무기가 priority1 무기가 아님 || 총알이 없음
            if (!lastArea || currentWeapon.itemType != linkedSurvivorData.priority1Weapon || (CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.CurrentMagazine == 0 && ValidBullet == null))
            {
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
            else ExploreLastFarmingArea();
        }
        else ExploreWithNoHand();

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
                if(corpse.Key.GetCurrentArea().IsProhibited || corpse.Key.GetCurrentArea().IsProhibited_Plan)
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
        else if(currentFarmingArea == null)
        {
            CurrentFarmingArea = FindNearest(farmingAreas);
        }
        else
        {
            farmingAreas[currentFarmingArea] = true;
            CurrentFarmingArea = FindNearest(farmingAreas);
            return;
        }
        targetFarmingSection = nearestFarmingSection;
    }

    public Area FindNearest(Dictionary<Area, bool> candidates)
    {
        Area nearest = null;
        float minDistance = float.MaxValue;
        float distance;
        List<Area> reserveRemoves = new();
        foreach (KeyValuePair<Area, bool> candidate in candidates)
        {
            Area area = candidate.Key;
            if (area.IsProhibited || area.IsProhibited_Plan)
            {
                reserveRemoves.Add(area);
                continue;
            }
            distance = Vector2.Distance(transform.position, candidate.Key.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = candidate.Key;
            }
        }
        foreach (Area reserveRemove in reserveRemoves)
        {
            farmingAreas[reserveRemove] = true;
        }
        if (nearest == null) return farmingAreas.FirstOrDefault(x => !x.Key.IsProhibited && !x.Key.IsProhibited_Plan).Key;
        return nearest;
    }

    public TKey FindNearest<TKey>(Dictionary<TKey, bool> candidates) where TKey : MonoBehaviour
    {
        TKey nearest = default;
        float minDistance = float.MaxValue;
        float distance;
        foreach (KeyValuePair<TKey, bool> candidate in candidates)
        {
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
                else if (typeof(TKey) == typeof(Survivor))
                {
                    Survivor corpse = candidate.Key as Survivor;
                    Area corpseArea = corpse.GetCurrentArea();
                    if (corpseArea.IsProhibited || corpseArea.IsProhibited_Plan)
                    {
                        farmingCorpses[corpse] = true;
                        return null;
                    }
                }
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

    public void FindNewNearestFarmingTarget()
    {
        CurrentFarmingArea = FindNearest(farmingAreas);
        targetFarmingSection = FindNearest(farmingSections);
        targetFarmingBox = FindNearest(farmingBoxes);
        targetFarmingCorpse = FindNearest(farmingCorpses);
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
        if (currentFarmingArea == null) return;
        bool farmingSectionLeft = false;
        foreach (FarmingSection farmingSection in currentFarmingArea.farmingSections)
        {
            if (!farmingSections[farmingSection])
            {
                farmingSectionLeft = true;
                targetFarmingSection = farmingSection;
                foreach (Box box in farmingSection.boxes) farmingBoxes.TryAdd(box, false);
                return;
            }
        }
        if (!farmingSectionLeft)
        {
            farmingAreas[currentFarmingArea] = true;
            CurrentFarmingArea = null;
        }
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

            curFarmingTime += Time.deltaTime * aiCool * farmingSpeed;
            progressBar.fillAmount = curFarmingTime / farmingTime;
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
                CheckCraftables();
                CurrentFarmingArea = FindNearest(farmingAreas);
                targetFarmingSection = FindNearest(farmingSections);
                targetFarmingBox = FindNearest(farmingBoxes);
                lookPosition = Vector2.zero;
                curFarmingTime = 0;
                progressBar.fillAmount = 0;
            }
            else lookPosition = targetFarmingCorpse.transform.position;
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
            CurrentStatus = Status.FarmingBox;
            if (!farmingSFXPlayed) PlayFarmingNoise();
            farmingSFXPlayed = true;
            agent.SetDestination(transform.position);

            // 상자에 설치된 부비트랩이 있으면 폭발
            BoobyTrap setBoobyTrap = targetFarmingBox.items.Find(x => x is BoobyTrap) as BoobyTrap;
            if (setBoobyTrap != null && setBoobyTrap.IsTriggered)
            {
                setBoobyTrap.Trigger(this);
                targetFarmingBox.items.Remove(setBoobyTrap);
                InGameUIManager.UpdateSelectedObjectInventory(targetFarmingBox);
                farmingBoxes[targetFarmingBox] = true;
                targetFarmingBox = null;
                return;
            }

            curFarmingTime += Time.deltaTime * aiCool * farmingSpeed;
            progressBar.fillAmount = curFarmingTime / farmingTime;
            if (curFarmingTime > farmingTime)
            {
                farmingSFXPlayed = false;
                foreach (Item item in targetFarmingBox.items)
                    AcqireItem(item);
                targetFarmingBox.items.Clear();
                InGameUIManager.UpdateSelectedObjectInventory(targetFarmingBox);
                farmingBoxes[targetFarmingBox] = true;

                // 파밍 후에 부비트랩이 있으면 상자에 설치
                var boobyTrapToSet = inventory.Find(x => x is BoobyTrap);
                if(boobyTrapToSet != null)
                {
                    curSettingBoobyTrap = boobyTrapToSet;
                    curSettingBoobyTrapBox = targetFarmingBox;
                }
                targetFarmingBox = null;

                CheckCraftables();
                lookPosition = Vector2.zero;
                curFarmingTime = 0;
                progressBar.fillAmount = 0;
            }
            else lookPosition = targetFarmingBox.transform.position;
        }
        else
        {
            agent.SetDestination(targetFarmingBox.transform.position);
        }
    }

    void PlayFarmingNoise()
    {
        float rand = UnityEngine.Random.Range(0, 1f);
        float volume = isAssassin ? 2 : 3;
        if(rand > 0.75f)
        {
            targetFarmingBox.PlaySFX($"farmingNoise01,{volume}", this);
        }
        else if (rand > 0.5f)
        {
            targetFarmingBox.PlaySFX($"farmingNoise02,{volume}", this);
        }
        else if (rand > 0.25f)
        {
            targetFarmingBox.PlaySFX($"farmingNoise03,{volume}", this);
        }
        else
        {
            targetFarmingBox.PlaySFX($"farmingNoise04,{volume}", this);
        }
    }

    bool leftFoot;
    void PlayFootstepNoise()
    {
        curMoveDistance += Vector2.Distance(lastPosition, transform.position);
        if(curMoveDistance > footstepDistance)
        {
            curMoveDistance = 0;
            Collider2D[] cols = Physics2D.OverlapPointAll(transform.position);
            string sfxName = "";
            foreach(var col in cols)
            {
                if(col.TryGetComponent(out Tilemap tile))
                {
                    sfxName = "footstep_concrete";
                    break;
                }
            }
            if (string.IsNullOrEmpty(sfxName)) sfxName = "footstep_grass";
            sfxName += leftFoot ? "1" : "2";
            sfxName += isAssassin ? ",2" : ",0.5";
            PlaySFX(sfxName);
            leftFoot = !leftFoot;
        }
        lastPosition = transform.position;
    }

    void Explore()
    {
        if (currentFarmingArea != null && !farmingAreas[currentFarmingArea]) return;
        if (lastFarmingArea == null)
        {
            Area area = FindNearest(farmingAreas);
            if (area != null)
            {
                CurrentFarmingArea = area;
                return;
            }
            else
            {
                lastFarmingArea = farmingAreas.FirstOrDefault(x => !x.Key.IsProhibited && !x.Key.IsProhibited_Plan).Key;
            }
        }
        else
        {
            ExploreLastFarmingArea();
        }
    }

    void ExploreLastFarmingArea()
    {
        if (Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            Vector2 wantPosition = transform.position;
            wantPosition = new(
                lastFarmingArea.transform.position.x + UnityEngine.Random.Range(-25f, 25f),
                lastFarmingArea.transform.position.y + UnityEngine.Random.Range(-25f, 25f)
                );

            agent.SetDestination(wantPosition);
        }
    }

    void ExploreWithNoHand()
    {
        if (Vector2.Distance(agent.destination, transform.position) < 1f)
        {
            Area area = farmingAreas.FirstOrDefault(x => !x.Key.IsProhibited && !x.Key.IsProhibited_Plan).Key;

            Vector2 wantPosition = transform.position;
            wantPosition = new(
                area.transform.position.x + UnityEngine.Random.Range(-25f, 25f),
                area.transform.position.y + UnityEngine.Random.Range(-25f, 25f)
                );

            agent.SetDestination(wantPosition);
        }
    }

    public void RemoveProhibitArea(Area area)
    {
        farmingAreas[area] = true;
        targetFarmingBox = null;
        targetFarmingSection = null;
        foreach (FarmingSection farmingSection in area.farmingSections)
        {
            foreach (Box box in farmingSection.boxes)
            {
                if(farmingBoxes.ContainsKey(box)) farmingBoxes.Remove(box);
            }
            if (farmingSections.ContainsKey(farmingSection)) farmingSections.Remove(farmingSection);
        }
        CurrentFarmingArea = FindNearest(farmingAreas);
    }
    #endregion

    #region Item
    public bool IsValid(Item item)
    {
        if (item == null || string.IsNullOrEmpty(item.itemName) || item.itemType == ItemManager.Items.NotValid) return false;
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
        else if(item.itemName.Contains("Bullet") || item.itemName.Contains("Rocket"))
        {
            currentWeaponisBestWeapon = false;
            GetItem(item);
            string wantWeapon = item.itemName.Split('(')[0].Split(')')[0];
            if (inventory.Find(x => x.itemName == wantWeapon) is RangedWeapon weapon && CompareWeaponValue(weapon))
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
        Item alreadyHave = inventory.Find(x => x.itemType == item.itemType);
        if(alreadyHave != null)
        {
            alreadyHave.amount += item.amount;
        }
        else
        {
            inventory.Add(item);
            if (item.itemType == ItemManager.Items.TrapDetectionDevice) trapDetectionDevice.SetActive(true);
            else if (item.itemType == ItemManager.Items.BiometricRader) biometricRader.SetActive(true);
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void ConsumptionItem(Item item, int amount)
    {
        item.amount -= amount;
        if(item.amount <= 0)
        {
            inventory.Remove(item);
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
                    Item bullet = newWeapon.itemType != ItemManager.Items.Bazooka ? inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})") : inventory.Find(x => x.itemName == $"Rocket(Bazooka)");
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
                Item bullet = newWeapon.itemType != ItemManager.Items.Bazooka ? inventory.Find(x => x.itemName == $"Bullet({newWeapon.itemName})") : inventory.Find(x => x.itemName == $"Rocket(Bazooka)");
                if (newWeaponAsRangedWeapon.CurrentMagazine > 0 || bullet != null)
                {
                    // 둘 다 총알이 있는 경우
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
                    {
                        if (newWeapon.AttackRange > currentWeapon.AttackRange) return true;
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
                        if (newWeapon.AttackRange > currentWeapon.AttackRange) return true;
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
                if (child.name == $"{wantWeapon.itemType}")
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
                Debug.LogWarning($"Can't find weapon : {wantWeapon.itemType}");
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
            Transform curWeaponTF = transform.Find("Right Hand").Find($"{currentWeapon.itemType}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
                projectileGenerator.muzzleTF = null;
            }
            else
            {
                Debug.LogWarning($"Can't find weapon : {currentWeapon.itemType}");
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
                if (child.name == $"{wantBulletproofHelmet.itemType}")
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
                Debug.LogWarning($"Can't find helmet : {wantBulletproofHelmet.itemType}");
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
            Transform curWeaponTF = transform.Find("Head").Find($"{currentHelmet.itemType}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Can't find helmet : {currentHelmet.itemType}");
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

    #region Crafting
    float craftingCool;
    bool Crafting()
    {
        craftingCool += Time.deltaTime * aiCool;
        if (craftingCool < 1f) return false;
        craftingCool = 0;
        if(craftables.Count > 0)
        {
            // 중독 중이면 우선적으로 해독제부터
            if(poisoned && linkedSurvivorData.craftingAllows[ItemManager.craftables.FindIndex(x => x.itemType == ItemManager.Items.Antidote)])
            {
                ItemManager.Craftable antidote = craftables.Find(x => x.itemType == ItemManager.Items.Antidote);
                if (antidote != null)
                {
                    currentCrafting = antidote;
                    return true;
                }
            }

            bool lockPriorityCraftingsMaterials = false;
            // 그 다음으로 Crafting Priority 체크
            if (linkedSurvivorData.priority1Crafting != null && linkedSurvivorData.priority1Crafting.itemType != ItemManager.Items.NotValid)
            {
                if (inventory.Find(x => x.itemType == linkedSurvivorData.priority1Crafting.itemType) == null)
                {
                    var priority1 = craftables.Find(x => x == linkedSurvivorData.priority1Crafting);
                    if (priority1 != null)
                    {
                        currentCrafting = priority1;
                        return true;
                    }
                    else lockPriorityCraftingsMaterials = true;
                }
                else lockPriorityCraftingsMaterials = false;
            }
            else lockPriorityCraftingsMaterials = false;

            for(int i=1; i <= craftables.Count; i++)
            {
                if (!linkedSurvivorData.craftingAllows[ItemManager.craftables.FindIndex(x => x.itemType == craftables[^i].itemType)]) continue;
                if(lockPriorityCraftingsMaterials)
                {
                    // 만약에 이 craftable이 craftingPriority1의 재료가 아니면
                    if (!linkedSurvivorData.priority1Crafting.etcNeedItems.ContainsKey(craftables[^i].itemType))
                    {
                        // 재료가 여분인지 체크
                        if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                        if (ComponentsCount - craftables[^i].needComponentsCount < linkedSurvivorData.priority1Crafting.needComponentsCount) continue;
                        if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                        if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                        if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                    }
                    else
                    {
                        // 재료가 맞으면
                        // 이미 재료가 충분히 만들어져 있는지 체크
                        Item alreadyHave = inventory.Find(x => x.itemType == craftables[^i].itemType);
                        if(alreadyHave != null && alreadyHave.amount >= linkedSurvivorData.priority1Crafting.etcNeedItems[craftables[^i].itemType])
                        {
                            // 재료가 충분하면 여분으로만 더 만들게
                            if(AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                            if(ComponentsCount - craftables[^i].needComponentsCount < linkedSurvivorData.priority1Crafting.needComponentsCount) continue;
                            if(AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                            if(AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                            if(AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < linkedSurvivorData.priority1Crafting.needAdvancedComponentCount) continue;
                        }
                        // 아니면(priority1의 재료면) 만듦
                        else
                        {
                            currentCrafting = craftables[^i];
                            return true;
                        }
                    }
                }
                // 이미 보유 중인 아이템은 만들지 않음
                if (inventory.Find(x => x.itemType == craftables[^i].itemType) != null) continue;
                // 총 필요성 검사
                bool gunNeeds = false;
                if(currentWeapon is not RangedWeapon)
                {
                    // 총이 없으면 무조건 만들고
                    gunNeeds = true;
                }
                else
                {
                    bool needCompare = false;
                    // 총이 있는데 총알이 없으면 총기 티어비교
                    if(CurrentWeaponAsRangedWeapon.CurrentMagazine == 0 && ValidBullet == null) needCompare = true;
                    else
                    {
                        // 총알이 있으면, 내가 만드려는 무기의 총알이 있을 때만 총기 티어 비교
                        string bulletName;
                        if (craftables[^i].itemType != ItemManager.Items.Bazooka) bulletName = $"Bullet_{craftables[^i].itemType}";
                        else bulletName = "Rocket_Bazooka";
                        if(Enum.TryParse(bulletName, out ItemManager.Items bullet))
                        {
                            Item validBullet = inventory.Find(x => x.itemType == bullet);
                            if (validBullet != null) needCompare = true;
                        }
                    }
                    if (needCompare)
                    {
                        int curWeaponTier = currentWeapon.itemType switch
                        {
                            ItemManager.Items.Pistol or ItemManager.Items.Revolver => 1,
                            ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun => 2,
                            ItemManager.Items.AssaultRifle or ItemManager.Items.SniperRifle => 3,
                            ItemManager.Items.Bazooka => 4,
                            _ => 5
                        };
                        int candidateTier = craftables[^i].itemType switch
                        {
                            ItemManager.Items.Pistol or ItemManager.Items.Revolver => 1,
                            ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun => 2,
                            ItemManager.Items.AssaultRifle or ItemManager.Items.SniperRifle => 3,
                            ItemManager.Items.Bazooka => 4,
                            _ => 5
                        };
                        gunNeeds = candidateTier > curWeaponTier;
                    }
                }
                // 총알 필요성 검사
                bool bulletNeeds = false;
                Item bestWeapon = null;
                if (linkedSurvivorData.priority1Weapon != ItemManager.Items.NotValid) bestWeapon = inventory.Find(x => x.itemType == linkedSurvivorData.priority1Weapon);
                if (bestWeapon == null)
                {
                    float maxRange = 0;
                    if (CurrentWeaponAsRangedWeapon != null)
                    {
                        bestWeapon = CurrentWeaponAsRangedWeapon;
                        maxRange = CurrentWeaponAsRangedWeapon.AttackRange;
                    }
                    foreach (var item in inventory)
                    {
                        if (item is RangedWeapon rangedWeapon && rangedWeapon.AttackRange > maxRange)
                        {
                            bestWeapon = item;
                            maxRange = rangedWeapon.AttackRange;
                        }
                    }
                }
                if(bestWeapon != null)
                {
                    if (CurrentWeaponAsRangedWeapon == bestWeapon)
                    {
                        if (CurrentWeaponAsRangedWeapon.CurrentMagazine == 0 && ValidBullet == null) bulletNeeds = true;
                    }
                    else bulletNeeds = true;
                }
                // 아이템 제작 필요성 검사
                switch (craftables[^i].itemType)
                {
                    case ItemManager.Items.Poison:
                        continue;
                    case ItemManager.Items.WalkingAid:
                        int howManyNeed = HowManyWalkingAidNeed();
                        Item walkingAid = inventory.Find(x => x.itemType == ItemManager.Items.WalkingAid);
                        int currentHave = walkingAid != null ? walkingAid.amount : 0;
                        if (currentHave < howManyNeed)
                        {
                            currentCrafting = craftables[^i];
                            return true;
                        }
                        else continue;
                    case ItemManager.Items.Bullet_Revolver:
                    case ItemManager.Items.Bullet_Pistol:
                    case ItemManager.Items.Bullet_SubMachineGun:
                    case ItemManager.Items.Bullet_ShotGun:
                    case ItemManager.Items.Bullet_AssaultRifle:
                    case ItemManager.Items.Bullet_SniperRifle:
                    case ItemManager.Items.Rocket_Bazooka:
                        if (bulletNeeds)
                        {
                            if (craftables[^i].itemType.ToString().Split("_")[1] == bestWeapon.itemType.ToString())
                            {
                                currentCrafting = craftables[^i];
                                return true;
                            }
                            else continue;
                        }
                        else continue;
                    default:
                        currentCrafting = craftables[^i];
                        return true;
                }
            }
        }
        return false;
    }

    bool Enchanting()
    {
        Item poison = inventory.Find(x => x.itemType == ItemManager.Items.Poison);
        if(poison != null)
        {
            if(currentWeapon is MeleeWeapon)
            {
                MeleeWeapon weapon = (MeleeWeapon)currentWeapon;
                if(!weapon.IsEnchanted)
                {
                    Enchant(weapon);
                    return true;
                }
            }
            Item notEnchantedBearTrap = inventory.Find(x => x.itemType == ItemManager.Items.BearTrap && !((Buriable)x).IsEnchanted);
            if(notEnchantedBearTrap != null)
            {
                Enchant(notEnchantedBearTrap);
                return true;
            }
        }
        return false;
    }

    void Craft()
    {
        CurrentStatus = Status.Crafting;
        agent.SetDestination(transform.position);
        animator.SetInteger("CraftingAnimNumber", currentCrafting.craftingAnimNumber);
        animator.SetBool("Crafting", true);

        curCraftingTime += Time.deltaTime * aiCool * craftingSpeed;
        progressBar.fillAmount = curCraftingTime / currentCrafting.craftingTime;
        if(curCraftingTime > currentCrafting.craftingTime)
        {
            curCraftingTime = 0;
            animator.SetBool("Crafting", false);
            if (currentCrafting.needAdvancedComponentCount > 0) ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.AdvancedComponent), currentCrafting.needAdvancedComponentCount);
            if (currentCrafting.needComponentsCount > 0) ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.Components), currentCrafting.needComponentsCount);
            if (currentCrafting.needChemicalsCount > 0) ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.Chemicals), currentCrafting.needChemicalsCount);
            if (currentCrafting.needSalvagesCount > 0) ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.Salvages), currentCrafting.needSalvagesCount);
            if (currentCrafting.needGunpowderCount > 0) ConsumptionItem(inventory.Find(x => x.itemType == ItemManager.Items.Gunpowder), currentCrafting.needGunpowderCount);
            foreach (var etcNeeds in currentCrafting.etcNeedItems) ConsumptionItem(inventory.Find(x => x.itemType == etcNeeds.Key), etcNeeds.Value);

            int amount = currentCrafting.outputAmount;
            ItemManager.AddItems(currentCrafting.itemType, amount);
            bool isWeapon = false;
            Item item = null;
            for (int i = 1; i <= amount; i++)
            {
                item = ItemManager.itemDictionary[currentCrafting.itemType][^i];
                isWeapon = item is Weapon;
                GetItem(item);
            }
            currentCrafting = null;
            craftables.Clear();
            if (isWeapon) Equip((Weapon)item);
            CheckCraftables();
        }
    }

    void Enchant(Item wantItem)
    {
        curEnchanting = wantItem;
        agent.SetDestination(transform.position);
        animator.SetInteger("CraftingAnimNumber", 2);
        animator.SetBool("Crafting", true);
    }

    void CheckCraftables()
    {
        craftables.Clear();

        int knowledge = linkedSurvivorData.Knowledge;
        foreach(var craftable in ItemManager.craftables)
        {
            if(knowledge >= craftable.requiredKnowledge && AdvancedComponentCount >= craftable.needAdvancedComponentCount && ComponentsCount >= craftable.needComponentsCount
                && ChemicalsCount >= craftable.needChemicalsCount && SalvagesCount >= craftable.needSalvagesCount && GunpowderCount >= craftable.needGunpowderCount)
            {
                bool haveETCNeeds = true;
                foreach(var etcNeed in craftable.etcNeedItems)
                {
                    Item _item = inventory.Find(x => x.itemType == etcNeed.Key);
                    if(_item == null || _item.amount < etcNeed.Value)
                    {
                        haveETCNeeds = false;
                        break;
                    }
                }
                if(haveETCNeeds) craftables.Add(craftable);
            }
        }
    }
    #endregion

    #region Traps
    bool BuryTrap()
    {
        if(curBurying == null)
        {
            Item item = inventory.Find(x => x is Buriable);
            if (item != null)
            {
                curBurying = item as Buriable;
                return true;
            }
            trapPlace = null;
            return false;
        }
        else
        {
            CurrentStatus = Status.Trapping;
            agent.SetDestination(transform.position);
            lookPosition = trapPlace.transform.position;

            curTrappingTime += Time.deltaTime * aiCool;
            progressBar.fillAmount = curTrappingTime / trappingTime;
            if(curTrappingTime > trappingTime)
            {
                curTrappingTime = 0;
                progressBar.fillAmount = 0;
                if (Enum.TryParse(curBurying.itemType.ToString(), out ResourceEnum.Prefab trap))
                {
                    Trap settedTrap = PoolManager.Spawn(trap, trapPlace.transform.position).GetComponent<Trap>();
                    if (curBurying.IsEnchanted) settedTrap.Enchant();
                    settedTrap.setter = this;
                    trapPlace.SetTrap(settedTrap);
                    burieds.Add(settedTrap.gameObject);
                }
                else Debug.LogWarning($"Failed to spawn trap : {curBurying.itemType}");
                trapPlace = null;
                return false;
            }
            return true;
        }
    }

    bool SetBoobyTrap()
    {
        if (curSettingBoobyTrap == null || curSettingBoobyTrapBox == null) return false;
        CurrentStatus = Status.Trapping;
        agent.SetDestination(transform.position);
        lookPosition = curSettingBoobyTrapBox.transform.position;
        
        curTrappingTime += Time.deltaTime * aiCool;
        progressBar.fillAmount = curTrappingTime / trappingTime;
        if (curTrappingTime > trappingTime)
        {
            curTrappingTime = 0;
            progressBar.fillAmount = 0;
            (curSettingBoobyTrap as BoobyTrap).SetSetter(this, targetFarmingBox);
            ItemManager.AddItems(curSettingBoobyTrap.itemType, 1);
            curSettingBoobyTrapBox.items.Add(ItemManager.itemDictionary[curSettingBoobyTrap.itemType][^1]);
            ConsumptionItem(curSettingBoobyTrap, 1);
            curSettingBoobyTrap = null;
            curSettingBoobyTrapBox = null;
            return false;
        }
        return true;
    }

    public void DetectTrap(Box box)
    {
        if(!detectedTrapSetBoxes.Contains(box))
        {
            detectedTrapSetBoxes.Add(box);
            if(farmingBoxes.ContainsKey(box)) farmingBoxes[box] = true;
        }
    }

    public void DetectTrap(Trap trap)
    {
        if (!detectedTraps.Contains(trap)) detectedTraps.Add(trap);
    }

    void DisarmTrap()
    {
        if(curDisarmTrap == null)
        {
            Trap nearestTrap = null;
            float minDistance = float.MaxValue;
            float distance;
            foreach(Trap trap in detectedTraps)
            {
                distance = Vector2.Distance(transform.position, trap.gameObject.transform.position);
                if(minDistance > distance)
                {
                    nearestTrap = trap;
                    minDistance = distance;
                }
            }
            curDisarmTrap = nearestTrap;
        }
        else if(Vector2.Distance(transform.position, curDisarmTrap.transform.position) > 1.5f)
        {
            agent.SetDestination(curDisarmTrap.transform.position);
        }
        else
        {
            CurrentStatus = Status.TrapDisarming;
            agent.SetDestination(transform.position);
            lookPosition = curDisarmTrap.transform.position;
            curDisarmTime += Time.deltaTime * aiCool;
            progressBar.fillAmount = curDisarmTime / curDisarmTrap.DisarmTime;
            if(curDisarmTime > curDisarmTrap.DisarmTime)
            {
                ItemManager.AddItems(curDisarmTrap.ItemType, 1);
                GetItem(ItemManager.itemDictionary[curDisarmTrap.ItemType][^1]);
                PoolManager.Despawn(curDisarmTrap.gameObject);
                detectedTraps.Remove(curDisarmTrap);
                curDisarmTrap = null;
            }
        }
    }
    #endregion

    public void DetectSurvivor(Vector2[] positions)
    {
        float minDistance = float.MaxValue;
        float distance;
        Vector2 nearest = Vector2.zero;
        foreach(Vector2 position in positions)
        {
            distance = Vector2.Distance(transform.position, position);
            if(distance < minDistance)
            {
                minDistance = distance;
                nearest = position;
            }
        }
        targetEnemiesLastPosition = nearest;
    }

    #region Combat
    void InvestigateThreateningSound()
    {
        CurrentStatus = Status.InvestigateThreateningSound;
        animator.SetBool("Crafting", false);
        if (Vector2.Distance(transform.position, threateningSoundPosition) < 1f)
        {
            LookAround();
        }
        else
        {
            agent.SetDestination(threateningSoundPosition);
            lookPosition = Vector2.zero;
        }
    }

    void TraceEnemy()
    {
        CurrentStatus = Status.TraceEnemy;
        if (Vector2.Distance(transform.position, targetEnemiesLastPosition) < 0.3f)
        {
            LookAround();
        }
        else
        {
            // 추격하다가 destination이 금구가 되면 추격 중지
            Area targetArea = GameManager.Instance.BattleRoyaleManager.GetArea(targetEnemiesLastPosition);
            if (targetArea.IsProhibited_Plan || targetArea.IsProhibited)
            {
                targetEnemiesLastPosition = Vector2.zero;
                return;
            }
            agent.SetDestination(targetEnemiesLastPosition);
            lookPosition = Vector2.zero;
        }
    }

    void Combat(float distance)
    {
        if (CurrentWeaponAsRangedWeapon != null)
        {
            if (distance < CurrentWeaponAsRangedWeapon.MinimumRange)
            {
                if (distance < attackRange) Attack();
                else ApproachEnemy(TargetEnemy);
            }
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
        animator.SetBool("Crafting", false);
        if (Vector2.Distance(agent.destination, target.transform.position) > attackRange)
        {
            // 상대는 금구에 있고 난 아닌경우 :
            // 내가 사거리가 더 길면 상대가 나올 곳에서 대기
            // 내가 사거리가 더 짧으면 Approach
            Area area = target.GetCurrentArea();
            Vector2 destination;
            float atkRange = CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.CurrentMagazine > 0 ? CurrentWeaponAsRangedWeapon.AttackRange : attackRange;
            if ((area.IsProhibited_Plan || area.IsProhibited) && target.currentWeapon.AttackRange <= atkRange)
            {
                float x, y;
                float myX = transform.position.x;
                float myY = transform.position.y;
                if(Mathf.Abs(myX - area.transform.position.x) > Mathf.Abs(myY - area.transform.position.y))
                {
                    x = myX > area.transform.position.x ? area.transform.position.x + 26 : area.transform.position.x - 26;
                    y = myY;
                }
                else
                {
                    x = myX;
                    y = myY > area.transform.position.y ? area.transform.position.y + 26 : area.transform.position.y - 26;
                }
                destination = new(x,y);
            }
            else destination = target.transform.position;

            curSetDestinationCool += Time.deltaTime * aiCool;
            if (curSetDestinationCool > 1)
            {
                agent.SetDestination(destination);
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
        animator.SetBool("Crafting", false);
        animator.SetBool("Drinking", false);
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
        animator.SetBool("Crafting", false);
        animator.SetBool("Drinking", false);
        animator.SetBool("Aim", true);

        curAimDelay += Time.deltaTime * aiCool;
        progressBar.fillAmount = curAimDelay / aimDelay;
        if(curAimDelay > aimDelay)
        {
            progressBar.fillAmount = 0;
            curShotTime -= Time.deltaTime * aiCool;
            if(curShotTime < 0)
            {
                animator.SetInteger("ShotAnimNumber", CurrentWeaponAsRangedWeapon.ShotAnimNumber);
                animator.SetTrigger("Fire");
                curShotTime = CurrentWeaponAsRangedWeapon.ShotCoolTime;
            }
        }
    }

    void Reload()
    {
        animator.SetBool("Attack", false);
        animator.SetBool("StopBleeding", false);
        animator.SetBool("Crafting", false);
        agent.SetDestination(transform.position);
        animator.SetBool("Reload", true);
        curAimDelay = 0;
    }
    #endregion

    #region Run Away
    bool TryRunAway(Survivor target)
    {
        runAwayFrom = target;
        if (runAwayDestination != Vector2.zero)
        {
            CurrentStatus = Status.RunAway;
            agent.SetDestination(runAwayDestination);
            return true;
        }
        return false;
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
        sightMesh.Clear();
        sightMesh.vertices = sightVertices;
        sightMesh.triangles = sightTriangles;
        sightMesh.RecalculateNormals();  // 법선 벡터 계산
    }

    void CalculateSightMesh()
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
            if(currentStatus == Status.FarmingBox || currentStatus == Status.Trapping || currentStatus == Status.Crafting) sightRange = Mathf.Min(sightRange, 5);
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
        else if( heardVolume > 0.5f)
        {
            // 불분명한 인지
            if (noiseMaker != null && noiseMaker is Survivor) HeardIndistinguishableSound((noiseMaker as Survivor).survivorName, soundOrigin);
            else Debug.LogWarning("There are no noiseMaker");
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
        Debug.Log(noiseMaker);
        heardSound = noiseMaker;
        SoundsMemory sound = soundsMemories.Find(x => x.soundName == noiseMaker);
        if (sound != null)
        {
            if(sound.soundTime > 1)
            {
                sound.soundTime = 1;
                HeardDistinguishableSound(soundOrigin);
                return;
            }
        }
        else soundsMemories.Add(new(heardSound));

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
    void ApplyDamage(Survivor attacker, float damage, InjurySiteMajor damagePart, InjurySite specificDamagePart, DamageType damageType)
    {
        if (isDead) return;
        Injury alreadyHaveInjury = injuries.Find(x => x.site == specificDamagePart);
        bool damagedPartIsArtifical = false;
        bool noPain = false;
        if (alreadyHaveInjury != null)
        {
            if (alreadyHaveInjury.type == InjuryType.ArtificalPartsTransplanted)
            {
                damagedPartIsArtifical = true;
                // 재밌는 switch 용법
                noPain = alreadyHaveInjury.site switch
                {
                    InjurySite.Organ or InjurySite.RightEye or InjurySite.LeftEye or InjurySite.RightEar or InjurySite.LeftEar => false,
                    _ => true,
                };
            }
            if (!damagedPartIsArtifical && (damagePart == InjurySiteMajor.Head || damagePart == InjurySiteMajor.Torso || alreadyHaveInjury.degree < 1)) damage *= 1 + alreadyHaveInjury.degree;
        }
        float maxDamage = specificDamagePart switch
        {
            InjurySite.RightArm or InjurySite.LeftArm or InjurySite.RightLeg or InjurySite.LeftLeg => Mathf.Min(damage, 50),
            InjurySite.RightKnee or InjurySite.LeftKnee => Mathf.Max(damage, 30),
            InjurySite.RightHand or InjurySite.LeftHand or InjurySite.RightAncle or InjurySite.LeftAncle => Mathf.Min(damage, 20),
            InjurySite.RightThumb or InjurySite.LeftThumb or InjurySite.RightIndexFinger or InjurySite.LeftIndexFinger or InjurySite.RightMiddleFinger or InjurySite.LeftMiddleFinger
            or InjurySite.RightRingFinger or InjurySite.LeftRingFinger or InjurySite.RightLittleFinger or InjurySite.LeftLittleFinger or InjurySite.RightBigToe or InjurySite.LeftBigToe => Mathf.Min(damage, 10),
            _ => damage
        };
        if (!(damagedPartIsArtifical && noPain)) curHP -= maxDamage;
        attacker.totalDamage += maxDamage;
        if (curHP <= 0)
        {
            curHP = 0;
            attacker.killCount++;
            InGameUIManager.UpdateSelectedObjectKillCount(attacker);
            attacker.FindNewNearestFarmingTarget();
            IsDead = true;
            if (damagePart == InjurySiteMajor.Head && damageType == DamageType.GunShot)
            {
                GameObject headshot = PoolManager.Spawn(ResourceEnum.Prefab.Headshot, transform.position);
                headshot.transform.SetParent(canvas.transform);
            }
            InGameUIManager.ShowKillLog(survivorName, attacker.survivorName);
        }

        if (damagedPartIsArtifical) GetDamageArtificalPart(alreadyHaveInjury, damage);
        else GetInjury(specificDamagePart, damageType, damage);
        if (attacker.currentWeapon is MeleeWeapon weapon && weapon.IsEnchanted) Poisoning(attacker);
    }

    void ApplyDamage(Survivor attacker, float damage, InjurySiteMajor damagePart, DamageType damageType)
    {
        if (isDead) return;
        if (damage > 0)
        {
            InjurySite specificDamagePart = GetSpecificDamagePart(damagePart, damageType);
            ApplyDamage(attacker, damage, damagePart, specificDamagePart, damageType);
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

    //                                                                                side - 0: don't know / 1: right / 2: left
    void ApplyExplosionDamage(Survivor attacker, float damage, InjurySiteMajor injurySiteMajor, int side = 0)
    {
        switch(injurySiteMajor)
        {
            case InjurySiteMajor.Head:
                int count = 1;
                // head = 1
                float randLEye = UnityEngine.Random.Range(0, 1f);
                float randREye = UnityEngine.Random.Range(0, 1f);
                float randLEar = UnityEngine.Random.Range(0, 1f);
                float randREar = UnityEngine.Random.Range(0, 1f);
                float randNose = UnityEngine.Random.Range(0, 1f);
                if (randLEye < 0.3f) count++;
                if (randREye < 0.3f) count++;
                if (randLEar < 0.5f) count++;
                if (randREar < 0.5f) count++;
                if (randNose < 0.7f) count++;
                float dividedDamage = damage / count;
                ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.Head, DamageType.Explosion);
                if(randLEye < 0.3f) ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.LeftEye, DamageType.Explosion);
                if(randREye < 0.3f) ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.RightEye, DamageType.Explosion);
                if(randREar < 0.5f) ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.LeftEar, DamageType.Explosion);
                if(randLEar < 0.5f) ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.RightEar, DamageType.Explosion);
                if(randNose < 0.7f) ApplyDamage(attacker, dividedDamage, InjurySiteMajor.Head, InjurySite.Nose, DamageType.Explosion);
                break;
            case InjurySiteMajor.Torso:
                ApplyDamage(attacker, damage / 2, InjurySiteMajor.Torso, InjurySite.Chest, DamageType.Explosion);
                ApplyDamage(attacker, damage / 2, InjurySiteMajor.Torso, InjurySite.Abdomen, DamageType.Explosion);
                break;
            case InjurySiteMajor.Arms:
                ApplyDamage(attacker, damage, InjurySiteMajor.Arms, DamageType.Explosion);
                break;
            case InjurySiteMajor.Legs:
                float rand = UnityEngine.Random.Range(0, 1f);
                if(rand > 0.3f)
                {
                    if(side == 1) ApplyDamage(attacker, damage, InjurySiteMajor.Legs, InjurySite.RightAncle, DamageType.Explosion);
                    else if(side == 2) ApplyDamage(attacker, damage, InjurySiteMajor.Legs, InjurySite.LeftAncle, DamageType.Explosion);
                    else ApplyDamage(attacker, damage, InjurySiteMajor.Legs, rand > 0.65f ? InjurySite.RightAncle : InjurySite.LeftAncle, DamageType.Explosion);
                }
                else
                {
                    if(side == 1) ApplyDamage(attacker, damage, InjurySiteMajor.Legs, InjurySite.RightKnee, DamageType.Explosion);
                    else if(side == 2) ApplyDamage(attacker, damage, InjurySiteMajor.Legs, InjurySite.LeftKnee, DamageType.Explosion);
                    else ApplyDamage(attacker, damage, InjurySiteMajor.Legs, rand > 0.15f ? InjurySite.RightKnee : InjurySite.LeftKnee, DamageType.Explosion);
                }
                break;
        }
    }

    void ApplyPoisonDamage(Survivor poisonOriginator)
    {
        float damage = 5 * Time.deltaTime;
        poisonOriginator.totalDamage += damage;
        if (curHP <= 0)
        {
            curHP = 0;
            poisonOriginator.killCount++;
            InGameUIManager.UpdateSelectedObjectKillCount(poisonOriginator);
            IsDead = true;
            InGameUIManager.ShowKillLog(survivorName, poisonOriginator.survivorName);
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
            if (attacker.currentWeapon is RangedWeapon attackerWeaponRange)
            {
                hitSound = attackerWeaponRange.AttackAnimNumber == 2 ? "hit02,10" : "hit01,10";
            }
            else if (attacker.currentWeapon is MeleeWeapon attackerWeaponMelee)
            {
                if (attackerWeaponMelee.DamageType == DamageType.Slash) hitSound = "hit_flesh,5";
                else if (attackerWeaponMelee.DamageType == DamageType.Strike) hitSound = "hit01,20";
                else hitSound = "hit01,10";
            }
            else hitSound = "hit01,10";
        }
        else
        {
            float probability = UnityEngine.Random.Range(0, 1f);
            float coefficient = (linkedSurvivorData.Fighting / attacker.linkedSurvivorData.Fighting) * (moveSpeed / attacker.moveSpeed);
            float avoidRate = 0.2f * coefficient;
            float defendRate = 0.3f * coefficient;
            if (rightHandDisabled) defendRate -= defendRate * 0.5f;
            if (leftHandDisabled) defendRate -= defendRate * 0.5f;
            float criticalRate = 0.1f * coefficient * attacker.luck / luck;

            if (probability < avoidRate)
            {
                // 회피
                damage = 0;
                hitSound = "avoid, 1";
            }
            else if (probability < avoidRate + defendRate)
            {
                // 방어
                damage *= 0.5f;
                damagePart = InjurySiteMajor.Arms;
                hitSound = "guard, 2";
            }
            else if (probability > 1 - criticalRate)
            {
                // 치명타
                damage *= 2;
                if (damageType == DamageType.Strike) damagePart = InjurySiteMajor.Head;
                else damagePart = InjurySiteMajor.Torso;
                hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,5" : "hit01,5";
            }
            else hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,5" : "hit01,5";
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
                    PlaySFX("ricochet,5", this);
                }
                else
                {
                    PlaySFX("ricochet2,5", this);
                }
            }
        }
        else if(damagePart == InjurySiteMajor.Torso)
        {
            if(currentVest != null) damage -= currentVest.Armor;
        }

        ApplyDamage(bullet.Launcher, damage, damagePart, DamageType.GunShot);
    }

    public void TakeDamage(Trap trap, InjurySite injurySite)
    {
        ApplyDamage(trap.setter, trap.Damage, InjurySiteMajor.Legs, injurySite, trap.DamageType);
    }

    public void TakeDamage(Trap trap, float damage, int side = 0)
    {
        if (trap.DamageType == DamageType.Explosion) ApplyExplosionDamage(trap.setter, damage, InjurySiteMajor.Legs, side);
        else ApplyDamage(trap.setter, damage, InjurySiteMajor.Legs, trap.DamageType);
    }

    public void TakeDamage(BoobyTrap boobyTrap, float damage)
    {
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.4f, InjurySiteMajor.Head);
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.4f, InjurySiteMajor.Torso);
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.2f, InjurySiteMajor.Arms);
    }

    public void TakeDamage(Rocket rocket, float damage, bool critical)
    {
        int site;
        if(critical)
        {
            site = UnityEngine.Random.Range(0, 4);
            switch(site)
            {
                case 0:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, InjurySiteMajor.Head);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, InjurySiteMajor.Torso);
                    break;
                case 1:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.2f, InjurySiteMajor.Head);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.5f, InjurySiteMajor.Torso);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.2f, InjurySiteMajor.Arms);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, InjurySiteMajor.Legs);
                    break;
                case 2:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, InjurySiteMajor.Arms);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, InjurySiteMajor.Torso);
                    break;
                case 3:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, InjurySiteMajor.Legs);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, InjurySiteMajor.Torso);
                    break;
            }
        }
        else
        {
            site = UnityEngine.Random.Range(0, 2);
            if(site == 0)
            {
                ApplyExplosionDamage(rocket.Launcher, damage * 0.9f, InjurySiteMajor.Arms);
                ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, InjurySiteMajor.Torso);
            }
            else
            {
                ApplyExplosionDamage(rocket.Launcher, damage * 0.9f, InjurySiteMajor.Arms);
                ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, InjurySiteMajor.Torso);
            }
        }
    }

    public void Poisoning(Survivor poisonOriginator)
    {
        curPoisonOriginator = poisonOriginator;
        poisoned = true;
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
                if(damageType == DamageType.GunShot)
                {
                    if (rand > 0.75f) injurySite = InjurySite.RightLeg;
                    else if (rand > 0.5f) injurySite = InjurySite.LeftLeg;
                    else if (rand > 0.4f) injurySite = InjurySite.RightKnee;
                    else if (rand > 0.3f) injurySite = InjurySite.LeftKnee;
                    else if (rand > 0.2f) injurySite = InjurySite.RightAncle;
                    else if (rand > 0.1f) injurySite = InjurySite.LeftAncle;
                    else if (rand > 0.5f) injurySite = InjurySite.RightBigToe;
                    else injurySite = InjurySite.LeftBigToe;
                }
                else if(damageType == DamageType.Slash)
                {
                    if (rand > 0.5f) injurySite = InjurySite.RightAncle;
                    else injurySite = InjurySite.LeftAncle;
                }
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
                else if(damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else if(damageType == DamageType.GunShot)
                {
                    injurySite = InjurySite.Skull;
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                    injuryDegree = damage / 100;
                }
                break;
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 1);
                if(damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Contusion;
                    if (injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                    else if(injuryDegree > 0.9) AddInjury(injurySite, InjuryType.PermanentVisualImpairment, 1);
                }
                else if(damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                    if (injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                    else if (injuryDegree > 0.9) AddInjury(injurySite, InjuryType.PermanentVisualImpairment, 1);
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                    AddInjury(injurySite, InjuryType.Loss, 1);
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                    injuryDegree = Mathf.Clamp(damage / 20, 0, 1);
                    if (injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                    else if (injuryDegree > 0.9) AddInjury(injurySite, InjuryType.PermanentVisualImpairment, 1);
                }
                break;
            case InjurySite.RightEar:
            case InjurySite.LeftEar:
                if(damageType == DamageType.Strike)
                {
                    injuryDegree = 0;
                }
                else if (damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1);
                    if(injuryDegree >= 1) AddInjury(injurySite, InjuryType.Loss, 1);
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                    injuryDegree = damage / 20;
                }
                break;
            case InjurySite.Nose:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if(damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Fracture;
                }
                else if(damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                    injuryDegree = Mathf.Clamp(damage / 20, 0, 0.99f);
                }
                break;
            case InjurySite.Jaw:
                if (damageType == DamageType.Strike)
                {
                    injuryType = InjuryType.Fracture;
                    injuryDegree = Mathf.Clamp((damage - 20) / 80, 0, 0.99f);
                }
                else if (damageType == DamageType.Slash)
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
                else if (damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                }
                else if (damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
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
                else if (damageType == DamageType.Slash)
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
                else if(damageType == DamageType.GunShot)
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
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
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
                else if (damageType == DamageType.Slash)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else if (damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Loss;
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
                else if (damageType == DamageType.Slash)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Explosion)
                {
                    injuryDegree = Mathf.Clamp(damage / 40, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Loss;
                    else injuryType = InjuryType.Burn;
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
                else if (damageType == DamageType.Slash)
                {
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                }
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
            case InjurySite.RightAncle:
            case InjurySite.LeftAncle:
                if(damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Slash)
                {
                    float rand = UnityEngine.Random.Range(0, 1f);
                    if (rand > 0.7f)
                    {
                        injuryDegree = 1;
                        injuryType = InjuryType.Amputation;
                    }
                    else
                    {
                        injuryDegree = rand / 0.7f;
                        injuryType = InjuryType.Cutting;
                    }
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryDegree = 1;
                    injuryType = InjuryType.Loss;
                }
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
            if (index != -1 && injuries[index].type != InjuryType.PermanentVisualImpairment)
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

    int HowManyWalkingAidNeed()
    {
        bool right = false;
        bool left = false;
        foreach(var injury in injuries)
        {
            switch(injury.site)
            {
                case InjurySite.RightLeg:
                case InjurySite.RightKnee:
                case InjurySite.RightAncle:
                case InjurySite.RightBigToe:
                    right = true;
                    break;
                case InjurySite.LeftLeg:
                case InjurySite.LeftKnee:
                case InjurySite.LeftAncle:
                case InjurySite.LeftBigToe:
                    left = true;
                    break;
                default:
                    continue;
            }
        }
        if (right)
        {
            if (left) return 2;
            else return 1;
        }
        else if (left) return 1;
        else return 0;
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
    bool isLeftEyeArtificial;
    bool isLeftEyePermanentVisualImpairment;
    bool isRightEyeArtificial;
    bool isRightEyePermanentVisualImpairment;
    float injuryCorrection_LeftSightRange = 1;
    float injuryCorrection_RightSightRange = 1;
    float injuryCorrection_CraftingSpeed = 1;
    void ApplyInjuryPenalty()
    {
        InGameUIManager.UpdateSelectedObjectInjury(this);
        float ear1Penalty = 0;
        float ear2Penalty = 0;
        bool eyeInjured = false;
        int blind = 0;
        float penaltiedFarmingSpeedByEyes = 0;
        float penaltiedFarmingSpeedByOrgan = 1;
        float penaltiedAttackSpeedByOrgan = 1;
        float penaltiedMoveSpeedByOrgan = 1;
        float penaltiedAttackDamageByRightArm = 1;
        float penaltiedAttackDamageByLeftArm = 1;
        float penaltiedMoveSpeedByRightLeg = 1;
        float penaltiedMoveSpeedByLeftLeg = 1;
        float penaltiedCraftingSpeedByRightArm = 1;
        float penaltiedCraftingSpeedByLeftArm = 1;
        foreach (Injury injury in injuries)
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
                    if (injury.type == InjuryType.ArtificalPartsTransplanted) isRightEyeArtificial = true;
                    else if (injury.type == InjuryType.PermanentVisualImpairment) isRightEyePermanentVisualImpairment = true;
                    else
                    {
                        injuryCorrection_RightSightRange = 1 - injury.degree;
                        penaltiedFarmingSpeedByEyes = Mathf.Max(penaltiedFarmingSpeedByEyes, 1 - injury.degree);
                        eyeInjured = true;
                        if (injury.degree >= 1) blind++;
                    }
                    break;
                case InjurySite.LeftEye:
                    if (injury.type == InjuryType.ArtificalPartsTransplanted) isLeftEyeArtificial = true;
                    else if (injury.type == InjuryType.PermanentVisualImpairment) isLeftEyePermanentVisualImpairment = true;
                    else
                    {
                        injuryCorrection_LeftSightRange = 1 - injury.degree;
                        penaltiedFarmingSpeedByEyes = Mathf.Max(penaltiedFarmingSpeedByEyes, 1 - injury.degree);
                        eyeInjured = true;
                        if (injury.degree >= 1) blind++;
                    }
                    break;
                case InjurySite.Organ:
                    penaltiedFarmingSpeedByOrgan = 1 - injury.degree;
                    penaltiedAttackSpeedByOrgan = 1 - injury.degree;
                    penaltiedMoveSpeedByOrgan = 1 - injury.degree;
                    break;
                case InjurySite.RightArm:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightHand:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.5f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree);
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightThumb:
                case InjurySite.RightIndexFinger:
                case InjurySite.RightMiddleFinger:
                case InjurySite.RightRingFinger:
                case InjurySite.RightLittleFinger:
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.1f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    break;
                case InjurySite.LeftArm:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftHand:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.5f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree);
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftThumb:
                case InjurySite.LeftIndexFinger:
                case InjurySite.LeftMiddleFinger:
                case InjurySite.LeftRingFinger:
                case InjurySite.LeftLittleFinger:
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.1f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    break;
                case InjurySite.RightLeg:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightKnee:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightAncle:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightBigToe:
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.1f));
                    break;
                case InjurySite.LeftLeg:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.LeftKnee:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.LeftAncle:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.LeftBigToe:
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.1f));
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
        isBlind = blind >= 2;

        Item walkingAid = inventory.Find(x => x.itemType == ItemManager.Items.WalkingAid);
        if (walkingAid != null)
        {
            if(walkingAid.amount >= 2)
            {
                penaltiedMoveSpeedByLeftLeg = 1 - (1 - penaltiedMoveSpeedByLeftLeg) * 0.5f;
                penaltiedMoveSpeedByRightLeg = 1 - (1 - penaltiedMoveSpeedByRightLeg) * 0.5f;
            }
            else if(penaltiedMoveSpeedByLeftLeg > penaltiedMoveSpeedByRightLeg) penaltiedMoveSpeedByLeftLeg = 1 - (1 - penaltiedMoveSpeedByLeftLeg) * 0.5f;
            else penaltiedMoveSpeedByRightLeg = 1 - (1 - penaltiedMoveSpeedByRightLeg) * 0.5f;
        }
        injuryCorrection_MoveSpeed = penaltiedMoveSpeedByOrgan * penaltiedMoveSpeedByRightLeg * penaltiedMoveSpeedByLeftLeg;

        injuryCorrection_AttackDamage = Mathf.Max(penaltiedAttackDamageByLeftArm, penaltiedAttackDamageByRightArm);
        if (RightHandDisabled) injuryCorrection_CraftingSpeed = 0.5f * penaltiedCraftingSpeedByLeftArm;
        else if (LeftHandDisabled) injuryCorrection_CraftingSpeed = 0.5f * penaltiedCraftingSpeedByRightArm;
        else injuryCorrection_CraftingSpeed = penaltiedCraftingSpeedByRightArm * penaltiedCraftingSpeedByLeftArm;
        ApplyCorrectionStats();
    }
    #endregion

    #region Characteristic
    float characteristicCorrection_SightRange = 1;
    float characteristicCorrection_HearingAbility = 1;
    int characteristicCorrection_Strength = 0;
    int characteristicCorrection_Agility = 0;
    int characteristicCorrection_Fighting = 0;
    int characteristicCorrection_Shooting = 0;
    int characteristicCorrection_Knowledge = 0;
    float characteristicCorrection_AimTime = 0;
    int characteristicCorrection_AimErrorRange = 0;
    float characteristicCorrection_NatualHemostasis = 1;
    float characteristicCorrection_BloodRegeneration = 1;
    float characteristicCorrection_HpRegeneration = 1;
    float characteristicCorrection_CraftingSpeed = 1;

    int correctedStrength = 0;
    int correctedAgility = 0;
    int correctedFighting = 0;
    int correctedShooting = 0;
    int correctedKnowledge = 0;
    public int CorrectedStrength => correctedStrength;
    public int CorrectedAgility => correctedAgility;
    public int CorrectedFighting => correctedFighting;
    public int CorrectedShooting => correctedShooting;
    public int CorrectedKnowledge => correctedKnowledge;

    void ApplyCharacteristics()
    {
        foreach (var characteristic in Characteristics)
        {
            switch(characteristic.type)
            {
                case CharacteristicType.HawkEye:
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
                case CharacteristicType.Giant:
                    transform.localScale = new(1.3f, 1.3f);
                    foreach(Transform child in rightHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in leftHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in transform.Find("Head")) child.localScale = new(0.77f, 0.77f);
                    break;
                case CharacteristicType.Dwarf:
                    transform.localScale = new(0.7f, 0.7f);
                    foreach (Transform child in rightHand.transform) child.localScale = new(1.43f, 1.43f);
                    foreach (Transform child in leftHand.transform) child.localScale = new(1.43f, 1.43f);
                    foreach (Transform child in transform.Find("Head")) child.localScale = new(1.43f, 1.43f);
                    break;
                case CharacteristicType.CarefulShooter:
                    characteristicCorrection_AimTime *= 2;
                    characteristicCorrection_AimErrorRange += 20;
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
                    int condition = UnityEngine.Random.Range(-10, 11);
                    characteristicCorrection_Strength += condition;
                    characteristicCorrection_Agility += condition;
                    characteristicCorrection_Fighting += condition;
                    characteristicCorrection_Shooting += condition;
                    characteristicCorrection_Knowledge += condition;
                    break;
                case CharacteristicType.ClumsyHand:
                    characteristicCorrection_CraftingSpeed *= 0.7f;
                    break;
                case CharacteristicType.Dexterous:
                    characteristicCorrection_CraftingSpeed *= 1.3f;
                    break;
                case CharacteristicType.Engineer:
                    characteristicCorrection_CraftingSpeed *= 1.6f;
                    break;
                case CharacteristicType.Assassin:
                    isAssassin = true;
                    break;
                default:
                    break;
            }
        }
        correctedStrength = Mathf.Max(linkedSurvivorData.Strength + characteristicCorrection_Strength, 0);
        correctedAgility = Mathf.Max(linkedSurvivorData.Agility + characteristicCorrection_Agility, 0);
        correctedFighting = Mathf.Max(linkedSurvivorData.Fighting + characteristicCorrection_Fighting, 0);
        correctedShooting = Mathf.Max(linkedSurvivorData.Shooting + characteristicCorrection_Shooting, 0);
        correctedKnowledge = Mathf.Max(linkedSurvivorData.Knowledge + characteristicCorrection_Knowledge, 0);
        aimErrorRange = 30f / Mathf.Pow(2, Mathf.Log(Mathf.Max(correctedShooting + characteristicCorrection_AimErrorRange, 1), 20));
        aimDelay = 1.5f * characteristicCorrection_AimTime;
        naturalHemostasis = characteristicCorrection_NatualHemostasis;
        bloodRegeneration = characteristicCorrection_BloodRegeneration;
        hpRegeneration = characteristicCorrection_HpRegeneration;
    }
    #endregion

    void ApplyCorrectionStats()
    {
        if (isLeftEyeArtificial) leftSightRange = 30 * injuryCorrection_LeftSightRange;
        else if(isLeftEyePermanentVisualImpairment) leftSightRange = 15 * injuryCorrection_LeftSightRange * characteristicCorrection_SightRange;
        else leftSightRange = 45 * characteristicCorrection_SightRange * injuryCorrection_LeftSightRange;
        if (isRightEyeArtificial) rightSightRange = 30 * injuryCorrection_RightSightRange;
        else if (isRightEyePermanentVisualImpairment) rightSightRange = 15 * injuryCorrection_RightSightRange * characteristicCorrection_SightRange;
        else rightSightRange = 45 * characteristicCorrection_SightRange * injuryCorrection_RightSightRange;
        hearingAbility = 10 * injuryCorrection_HearingAbility * characteristicCorrection_HearingAbility;

        attackDamage = (120f + correctedStrength + correctedFighting) / 16f * injuryCorrection_AttackDamage;
        attackSpeed = Mathf.Max((120f + correctedAgility + correctedFighting) / 160f * injuryCorrection_AttackSpeed, 0.1f);
        moveSpeed = Mathf.Max((60f + correctedAgility) * 3f / 80f * injuryCorrection_AttackSpeed, 0.1f);
        agent.speed = moveSpeed;
        farmingSpeed = Mathf.Max((60f + correctedAgility) / 80f * injuryCorrection_FarmingSpeed, 0.1f);
        craftingSpeed = injuryCorrection_CraftingSpeed * characteristicCorrection_CraftingSpeed;
        animator.SetFloat("CraftingSpeed", craftingSpeed);
        InGameUIManager.UpdateSelectedObjectStat(this);
    }

    #region Animation Events
    void AE_Attack()
    {
        if(isDead || TargetEnemy == null) return;
        if(IsValid(currentWeapon) && currentWeapon is MeleeWeapon)
        {
            if (Vector2.Distance(transform.position, TargetEnemy.transform.position) < currentWeapon.AttackRange)
            {
                float damage = currentWeapon.AttackDamage + attackDamage;
                if (currentWeapon.NeedHand == NeedHand.OneOrTwoHand && (rightHandDisabled || leftHandDisabled)) damage *= 0.7f;
                TargetEnemy.TakeDamage(this, damage);
            }
            else PlaySFX("avoid, 1", this);
        }
        else
        {
            if (Vector2.Distance(transform.position, TargetEnemy.transform.position) < attackRange)
            {
                TargetEnemy.TakeDamage(this, attackDamage);
            }
            else PlaySFX("avoid, 1", this);
        }
    }

    void AE_Reload()
    {
        int amount;
        if (currentWeapon.itemType == ItemManager.Items.ShotGun) amount = 1;
        else amount = Math.Clamp(ValidBullet.amount, 1, CurrentWeaponAsRangedWeapon.MagazineCapacity - CurrentWeaponAsRangedWeapon.CurrentMagazine);
        ConsumptionItem(ValidBullet, amount);
        CurrentWeaponAsRangedWeapon.Reload(amount);
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void AE_Taping()
    {
        Item bandage;
        int hemostaticAmount = 100;
        if(bleedingAmount > 100)
        {
            bandage = inventory.Find(x => x.itemType == ItemManager.Items.HemostaticBandageRoll);
            if (bandage == null)
            {
                bandage = inventory.Find(x => x.itemType == ItemManager.Items.BandageRoll);
                hemostaticAmount = 100;
            }
            else hemostaticAmount = 300;
        }
        else
        {
            bandage = inventory.Find(x => x.itemType == ItemManager.Items.BandageRoll);
            if (bandage == null)
            {
                bandage = inventory.Find(x => x.itemType == ItemManager.Items.HemostaticBandageRoll);
                hemostaticAmount = 300;
            }
            else hemostaticAmount = 100;
        }
        ConsumptionItem(bandage, 1);
        BleedingAmount -= hemostaticAmount;
    }

    void AE_Enchanting()
    {
        if (curEnchanting == null)
        {
            Debug.LogWarning("There is no curEnchanting.");
            return;
        }
        if (curEnchanting is MeleeWeapon weapon) weapon.Enchant();
        else if(curEnchanting is Buriable buriable) buriable.Enchant();
    }

    void AE_Drinking()
    {
        if(curDrinking == null)
        {
            Debug.LogWarning("There is no curDrinking.");
            return;
        }
        switch(curDrinking.itemType)
        {
            case ItemManager.Items.Antidote:
                poisoned = false;
                break;
            case ItemManager.Items.Potion:
                curHP = Mathf.Min(curHP + 100, maxHP);
                break;
            default:
                Debug.LogWarning($"Try to drink wrong item : {curDrinking.itemType}.");
                break;
        }
    }
    #endregion

    #region SightIn SightOut
    public void SightIn(Survivor survivor, bool corpse)
    {
        if(!corpse)
        {
            if (!inSightEnemies.Contains(survivor))
            {
                threateningSoundPosition = Vector2.zero;
                keepEyesOnPosition = Vector2.zero;
                inSightEnemies.Add(survivor);
                sightMeshRenderer.material = m_SightAlert;
                if (survivor != lastTargetEnemy) emotionAnimator.SetTrigger("Alert");
            }
        }
        else
        {
            if (!farmingCorpses.ContainsKey(survivor))
            {
                farmingCorpses.Add(survivor, false);
            }
        }
    }

    public void SightOut(Survivor survivor)
    {
        if (survivor == TargetEnemy && survivor != isDead)
        {
            targetEnemiesLastPosition = survivor.transform.position;
            lastTargetEnemy = survivor;
        }
        if(inSightEnemies.Contains(survivor)) inSightEnemies.Remove(survivor);
    }

    float curSeeEnemy = 0;
    float curTriggerStayCool;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart || isDead) return;
        //if (curFixedUpdateCool > GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count * 0.02f)
        //{

        //}
        if (collision.TryGetComponent(out Survivor survivor) && (!collision.isTrigger || survivor.IsDead))
        {
            curSeeEnemy += Time.fixedDeltaTime;
            if (curSeeEnemy > 0.1f)
            {
                curSeeEnemy = 0;
                SightIn(survivor, survivor.IsDead);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Survivor survivor))
        {
            if (!collision.isTrigger)
            {
                curSeeEnemy = 0;
                SightOut(survivor);
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
        return GameManager.Instance.BattleRoyaleManager.GetArea(transform.position);
    }

    public void SetSurvivorInfo(SurvivorData survivorInfo)
    {
        linkedSurvivorData = survivorInfo;
        survivorName = survivorInfo.survivorName;
        nameTag.GetComponent<TextMeshProUGUI>().text = survivorInfo.survivorName;
        curHP = maxHP = survivorInfo.Strength + 100;
        curBlood = maxBlood = maxHP * 80;
        bleedingSprite = curBlood - 100;
        luck = linkedSurvivorData.Luck;
        charicteristics = survivorInfo.characteristics;

        injuries = survivorInfo.injuries;
        foreach(Injury injury in injuries)
        {
            if (injury.type == InjuryType.ArtificalPartsTransplanted || injury.degree == 1) rememberAlreadyHaveInjury.Add(injury.site);
        }

        lastPosition = transform.position;
        ApplyCharacteristics();
        ApplyInjuryPenalty();
        ApplyStrategies();
    }

    public void LastArea()
    {
        lastArea = true;
        lastFarmingArea = farmingAreas.FirstOrDefault(x => !x.Key.IsProhibited && !x.Key.IsProhibited_Plan).Key;
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
                                            condition.conditions[i] = () => currentWeapon is RangedWeapon && (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0);
                                            break;
                                        case 2:
                                            condition.conditions[i] = () => currentWeapon == null || (currentWeapon is RangedWeapon && ValidBullet == null && CurrentWeaponAsRangedWeapon.CurrentMagazine == 0);
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
                                            condition.conditions[i] = () => currentWeapon is MeleeWeapon || (currentWeapon is RangedWeapon && (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0));
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

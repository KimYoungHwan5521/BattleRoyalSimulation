using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class Survivor : CustomObject
{
    #region Variables and Properties
    public enum Status { Farming, FarmingBox, InCombat, TraceEnemy, InvestigateThreateningSound, Maintain, RunAway, Trapping, 
        TrapDisarming, Crafting, Enchanting, FindingEnemy, Wearing, Reparing }
    #region Components
    [Header("Components")]
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject leftHand;
    public Transform rightHandTF;
    public Transform leftHandTF;
    [SerializeField] PolygonCollider2D sightCollider;
    [SerializeField] CircleCollider2D bodyCollider;
    [SerializeField] SpriteRenderer[] bodySprites;
    [SerializeField] GameObject trapDetectionDevice;
    [SerializeField] GameObject biometricRader;
    [SerializeField] GameObject energyBarrier;
    Animator animator => GetComponent<Animator>();
    NavMeshAgent agent;
    public Vector2 Velocity => ((Vector2)transform.position - lastPosition) / Time.deltaTime;

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
                energyBarrier.SetActive(false);
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
    public LocalizedString survivorName;
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
    int crafting;
    [SerializeField] float leftSightRange = 45f;
    [SerializeField] float rightSightRange = 45f;
    float sightAngle = 90;
    public LayerMask sightObstacleMask;
    [SerializeField] int sightEdgeCount = 12;
    [SerializeField] float hearingAbility = 10f;
    [SerializeField] string heardSound;

    [Serializable]
    public class SoundsMemory
    {
        public string soundName;
        public Vector2 soundOrigin;
        public float soundTime;

        public SoundsMemory(string soundName, Vector2 soundOrigin)
        {
            this.soundName = soundName;
            this.soundOrigin = soundOrigin;
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
    public float BloodRegeneration
    {
        get
        {
            if (curFuryTime > 0)
            {
                return bloodRegeneration * 2;
            }
            else
            {
                return bloodRegeneration + 0.2f * vampireStack;
            }
        }
    }
    float hpRegeneration = 1;
    public float HpRegeneration
    {
        get
        {
            if (curFuryTime > 0)
            {
                return hpRegeneration * 2;
            }
            else
            {
                return hpRegeneration + 0.2f * vampireStack;
            }
        }
    }

    int increaseFighting;
    public int IncreaseFighting => increaseFighting;
    int increaseShooting;
    public int IncreaseShooting => increaseShooting;
    int increaseCrafting;
    public int IncreaseCrafting => increaseCrafting;

    //Vector3 lastVelocity;
    bool temporaryAllowProhibitArea;
    #endregion
    #region Characteristic
    [Header("Characteristic")]
    [SerializeField] List<Characteristic> characteristics;
    public List<Characteristic> Characteristics => characteristics;
    bool isAssassin;
    float curFuryTime;
    int vampireStack;
    #endregion
    #region Injury
    [Header("Injury")]
    public List<Injury> injuries = new();
    public List<InjurySite> rememberAlreadyHaveInjury;

    [SerializeField] bool rightHandDisabled;
    public bool RightHandDisabled
    {
        get { return rightHandDisabled; }
        private set 
        {
            if (value) Debug.Log($"Right hand disabled : {linkedSurvivorData.localizedSurvivorName.GetLocalizedString()}");
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
    public bool LeftHandDisabled
    {
        get { return leftHandDisabled; }
        private set
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
    [SerializeField] bool dizzy;
    bool Dizzy
    {
        get { return dizzy; }
        set
        {
            dizzy = value;
            if(value) emotionAnimator.SetTrigger("Dizzy");
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

    //bool inProhibitedArea;
    public bool InProhibitedArea
    {
        get
        {
            return GetCurrentArea().IsProhibited;
        }
        //get { return InProhibitedArea; }
        //set
        //{
        //    inProhibitedArea = value;
        //    prohibitTimer.SetActive(value);
        //}
    }
    [SerializeField] float prohibitedAreaTime = 10f;
    int timerSound = 9;

    Survivor survivorWhoCausedBleeding;
    float rememberWhoCausedBleedingTime;
    public float maxBlood;
    public float curBlood;
    [SerializeField] float bleedingAmount = 0;
    public float BleedingAmount
    {
        get { return bleedingAmount; }
        set
        {
            bleedingAmount = Mathf.Max(value, 0);
            if (characteristics.FindIndex(x => x.type == CharacteristicType.Avenger) > -1)
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
    float naturalHemostasis = 1f;
    public float NaturalHemostasis => naturalHemostasis;
    float bleedingSprite;
    public List<GameObject> bloods = new();
    float stopBleedingSpeed = 1f;

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
            if (CurrentWeapon.itemType == ItemManager.Items.LASER) return null;
            string bulletName;
            if (CurrentWeapon.itemType == ItemManager.Items.Bazooka) bulletName = "Rocket_Bazooka";
            else if (CurrentWeapon.itemType == ItemManager.Items.Bow || CurrentWeapon.itemType == ItemManager.Items.AdvancedBow) bulletName = "Arrow";
            else bulletName = $"Bullet_{currentWeapon.itemType}";
            if (!Enum.TryParse(bulletName, out ItemManager.Items bullet))
            {
                Debug.LogWarning($"Wrong bullet name : {bulletName}");
                return null;
            }

            Item validBullet;
            if(bulletName == "Arrow")
            {
                validBullet = inventory.Find(x => x.itemType == ItemManager.Items.Arrow_Enchanted);
                if(validBullet == null) validBullet = inventory.Find(x => x.itemType == ItemManager.Items.Arrow);
            }
            else validBullet = inventory.Find(x => x.itemType == bullet);
            return validBullet;
        }
    }
    bool currentWeaponisBestWeapon;

    [SerializeField]List<ItemManager.Craftable> craftables = new();
    ItemManager.Craftable currentCrafting;
    public ItemManager.Craftable CurrentCrafting => currentCrafting;
    Item currentEnchanting;
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
    Item curDrinking;
    float curCraftingTime;
    float craftingSpeed = 1f;
    float enchantingTime = 1f;
    float curEnchantingTime;
    #endregion
    #region Trap
    public TrapPlace trapPlace;
    [SerializeField] float trappingTime = 3f;
    [SerializeField] float curTrappingTime;
    float trappingSpeed = 1f;
    Buriable curBurying;
    Item curSettingBoobyTrap;
    Box curSettingBoobyTrapBox;

    public List<GameObject> burieds = new();
    [SerializeField] List<Trap> detectedTraps = new();
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
    public Vector2 TargetEnemiesLastPosition => targetEnemiesLastPosition;
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
    [SerializeField] public Area lastCurrentArea;
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
    float farmingTime = 3f;
    [SerializeField] float curFarmingTime;
    float aimDelay = 1.5f;
    [SerializeField] float curAimDelay;
    [SerializeField] float curShotTime;
    [SerializeField] float reloadSpeed = 1;
    float wearingTime = 1f;
    float curWearingTime;
    float wearingSpeed = 1f;
    BulletproofHelmet currentWearingHelmet;
    public BulletproofHelmet CurrentWearingHelmet => currentWearingHelmet;
    BulletproofVest currentWearingVest;
    public BulletproofVest CurrentWearingVest => currentWearingVest;
    // Current Reparing - 0 : None, 1 : Helmet, 2 : Vest
    int currentRepairing;
    public int CurrentRepairing;
    float repairingTime;
    float curRepairingTime;
    int needComponentsToRepair;
    int needSalvagesToRepair;
    #endregion
    #region Look
    [Header("Look")]
    [SerializeField] Vector2 lookPosition = Vector2.zero;
    [SerializeField] float lookAroundTime = 1f;
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
    public bool playerSurvivor;
    float survivedTime;
    public float SurvivedTime => survivedTime;
    int killCount;
    public int KillCount
    {
        get { return killCount; }
        set
        {
            if (characteristics.FindIndex(x => x.type == CharacteristicType.TasteOfBlood) != -1)
            {
                curFuryTime = 10f;
                ApplyCorrectionStats();
            }
            if (characteristics.FindIndex(x => x.type == CharacteristicType.Vampire) != -1)
            {
                curBlood += 300;
            }

            if (playerSurvivor)
            {
                if(value > killCount)
                {
                    int curTotalKill = PlayerPrefs.GetInt("Total Kill");
                    PlayerPrefs.SetInt("Total Kill", curTotalKill + 1);
                    AchievementManager.SetStat("Total_Kill", curTotalKill + 1);
                    linkedSurvivorData.totalKill++;
                    if (linkedSurvivorData.totalKill >= 30) AchievementManager.UnlockAchievement("Notorious");
                    if (PlayerPrefs.GetInt("Total Kill") >= 100) AchievementManager.UnlockAchievement("Bloody Arms");
                    else if (PlayerPrefs.GetInt("Total Kill") >= 10) AchievementManager.UnlockAchievement("Bloody Hand");
                }

                if (value >= 5) AchievementManager.UnlockAchievement("Pentakill");
            }
            killCount = value;
        }
    }
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
        biometricRader.transform.parent = null;
        prohibitTimer.SetActive(false);
        progressBar.fillAmount = 0;

        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;

        m_SightNormal = ResourceManager.Get(ResourceEnum.Material.Sight_Normal);
        m_SightSuspicious = ResourceManager.Get(ResourceEnum.Material.Sight_Suspicious);
        m_SightAlert = ResourceManager.Get(ResourceEnum.Material.Sight_Alert);

        rightHandTF = rightHand.transform;
        leftHandTF = leftHand.transform;
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
        biometricRader.transform.position = new(transform.position.x, transform.position.y);
        
        survivedTime += Time.deltaTime;
        if (curFuryTime > 0)
        {
            curFuryTime -= Time.deltaTime;
            if(curFuryTime <= 0) ApplyCorrectionStats();
        }
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
        Destroy(biometricRader);
        base.MyDestroy();
    }

    float curFixedUpdateCool;
    private void FixedUpdate()
    {
        if(!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart || isDead) return;
        if (isBlind || dizzy) return;

        curFixedUpdateCool += Time.fixedDeltaTime;
        float fUpdateCool = 0.1f;
        if (curFixedUpdateCool > fUpdateCool)
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
        if (keepEyesOnPosition != Vector2.zero && inSightEnemies.Count == 0)
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
            //Look((Vector2)(transform.position + agent.velocity));
            if (agent.velocity.magnitude > 0)
            {
                Look((Vector2)(transform.position + agent.velocity));
                //lastVelocity = agent.velocity;
            }
            //else Look((Vector2)(transform.position + lastVelocity));
            else Look((Vector2)(transform.forward));
        }
    }

    void Look(Vector2 targetPosition)
    {
        // Vector2 currentLookVector = new(Mathf.Cos((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad), Mathf.Sin((transform.localEulerAngles.z + 90) * Mathf.Deg2Rad));
        Vector2 currentLookVector = transform.up;
        Vector2 preferDirection = targetPosition - (Vector2)transform.position;
        if (Vector2.Angle(currentLookVector, preferDirection) > 5f * Time.timeScale)
        {
            float direction = Vector2.SignedAngle(currentLookVector, preferDirection) > 0 ? 1 : -1;
            transform.rotation = Quaternion.Euler(0, 0, transform.localEulerAngles.z + direction * 200 * Time.deltaTime * aiCool);
        }
        else transform.up = preferDirection;
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
        prohibitTimer.SetActive(InProhibitedArea);
        if(InProhibitedArea)
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
                InGameUIManager.ShowKillLog(survivorName.GetLocalizedString(), new LocalizedString("Basic", "Restricted Area").GetLocalizedString());
            }
        }
    }

    void Bleeding()
    {
        curBlood -= BleedingAmount * Time.deltaTime;
        BleedingAmount -= NaturalHemostasis * Time.deltaTime;
        if(curBlood < bleedingSprite)
        {
            bloods.Add(PoolManager.Spawn(ResourceEnum.Prefab.Blood, transform.position));
            bleedingSprite -= 100;
        }
        if(curBlood / maxBlood < 0.5f)
        {
            IsDead = true;
            string cause;
            if (survivorWhoCausedBleeding != null)
            {
                survivorWhoCausedBleeding.KillCount++;
                cause = survivorWhoCausedBleeding.survivorName.GetLocalizedString();
                survivorWhoCausedBleeding = null;
            }
            else cause = new LocalizedString("Basic", "Severe Bleeding").GetLocalizedString();
            InGameUIManager.ShowKillLog(survivorName.GetLocalizedString(), cause);
            if(playerSurvivor) AchievementManager.UnlockAchievement("Severe Bleeding");
        }
        if(rememberWhoCausedBleedingTime > 0)
        {
            rememberWhoCausedBleedingTime -= Time.deltaTime * aiCool;
            if(rememberWhoCausedBleedingTime < 0)
            {
                survivorWhoCausedBleeding = null;
            }
        }
    }

    bool CheckStopBleeding()
    {
        if (BleedingAmount >= 30 || (BleedingAmount > 0 && (BleedingAmount) * (BleedingAmount - 1) / 2 + curBlood > maxBlood / 2))
        {
            if (inventory.Find(x => x.itemType == ItemManager.Items.BandageRoll) != null || inventory.Find(x => x.itemType == ItemManager.Items.HemostaticBandageRoll) != null)
            {
                StopBleeding();
                return true;
            }
        }
        return false;
    }

    void StopBleeding()
    {
        agent.SetDestination(transform.position);
        animator.SetBool("StopBleeding", true);
        animator.SetFloat("StopBleedingSpeed", stopBleedingSpeed);
    }

    bool CheckDrinking()
    {
        if (curDrinking == null)
        {
            if (poisoned)
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
                if (maxHP - curHP > 100)
                {
                    Item potion = inventory.Find(x => x.itemType == ItemManager.Items.AdvancedPotion);
                    if (potion != null)
                    {
                        curDrinking = potion;
                        return true;
                    }
                    else
                    {
                        potion = inventory.Find(x => x.itemType == ItemManager.Items.Potion);
                        if (potion != null)
                        {
                            curDrinking = potion;
                            return true;
                        }
                    }
                }
                else if (maxHP - curHP > 50)
                {
                    Item potion = inventory.Find(x => x.itemType == ItemManager.Items.Potion);
                    if (potion != null)
                    {
                        curDrinking = potion;
                        return true;
                    }
                    else
                    {
                        potion = inventory.Find(x => x.itemType == ItemManager.Items.AdvancedPotion);
                        if (potion != null)
                        {
                            curDrinking = potion;
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            agent.destination = transform.position;
            animator.SetBool("Drinking", true);
            return true;
        }
        return false;
    }

    bool CheckReload()
    {
        if (CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.NeedPreload)
        {
            if (CurrentWeaponAsRangedWeapon.CurrentMagazine < CurrentWeaponAsRangedWeapon.MagazineCapacity && ValidBullet != null)
            {
                sightMeshRenderer.material = m_SightNormal;
                Reload();
                return true;
            }
        }
        return false;
    }

    bool CheckRepair()
    {
        // 헬멧 갈아쓰기
        if (IsValid(CurrentHelmet) && CurrentHelmet.DurabilityPercent < 1f)
        {
            BulletproofHelmet theMostDurable = CurrentHelmet;
            foreach (var _ in inventory.FindAll(x => x.itemType == CurrentHelmet.itemType))
            {
                if (_ is BulletproofHelmet helmet)
                {
                    if (helmet.DurabilityPercent > theMostDurable.DurabilityPercent) theMostDurable = helmet;
                }
            }
            if (theMostDurable != CurrentHelmet) currentWearingHelmet = theMostDurable;
            else
            {
                // 수선
                if (CurrentHelmet.DurabilityPercent < linkedSurvivorData.strategyDictionary[StrategyCase.RepairCondition].action)
                {
                    //재료체크
                    int originalValue = CurrentHelmet.itemType switch
                    {
                        ItemManager.Items.LowLevelBulletproofHelmet => 0,
                        ItemManager.Items.MiddleLevelBulletproofHelmet => 3,
                        ItemManager.Items.HighLevelBulletproofHelmet => 6,
                        ItemManager.Items.LegendaryBulletproofHelmet => 12,
                        _ => 0
                    };
                    needComponentsToRepair = Mathf.CeilToInt(originalValue * CurrentHelmet.DurabilityPercent);
                    originalValue = CurrentHelmet.itemType switch
                    {
                        ItemManager.Items.LowLevelBulletproofHelmet => 7,
                        ItemManager.Items.MiddleLevelBulletproofHelmet => 8,
                        ItemManager.Items.HighLevelBulletproofHelmet => 9,
                        ItemManager.Items.LegendaryBulletproofHelmet => 18,
                        _ => 0
                    };
                    needSalvagesToRepair = Mathf.CeilToInt(originalValue * CurrentHelmet.DurabilityPercent);
                    if (ComponentsCount >= needComponentsToRepair && SalvagesCount >= needSalvagesToRepair)
                    {
                        currentRepairing = 1;
                        repairingTime = Math.Max(ItemManager.craftables.Find(x => x.itemType == CurrentHelmet.itemType).craftingTime * (1f - CurrentHelmet.DurabilityPercent), 4f);
                        return true;
                    }
                }
            }
        }

        // 조끼 갈아입기
        if (IsValid(CurrentVest) && CurrentVest.DurabilityPercent < 1f)
        {
            BulletproofVest theMostDurable = CurrentVest;
            foreach (var _ in inventory.FindAll(x => x.itemType == CurrentVest.itemType))
            {
                if (_ is BulletproofVest vest)
                {
                    if (vest.DurabilityPercent > theMostDurable.DurabilityPercent) theMostDurable = vest;
                }
            }
            if (theMostDurable != CurrentVest) currentWearingVest = theMostDurable;
            else
            {
                // 수선
                if (CurrentVest.DurabilityPercent < linkedSurvivorData.strategyDictionary[StrategyCase.RepairCondition].action)
                {
                    //재료체크
                    int originalValue = CurrentHelmet.itemType switch
                    {
                        ItemManager.Items.LowLevelBulletproofHelmet => 0,
                        ItemManager.Items.MiddleLevelBulletproofHelmet => 3,
                        ItemManager.Items.HighLevelBulletproofHelmet => 6,
                        ItemManager.Items.LegendaryBulletproofHelmet => 12,
                        _ => 0
                    };
                    needComponentsToRepair = Mathf.CeilToInt(originalValue * CurrentHelmet.DurabilityPercent);
                    originalValue = CurrentHelmet.itemType switch
                    {
                        ItemManager.Items.LowLevelBulletproofHelmet => 10,
                        ItemManager.Items.MiddleLevelBulletproofHelmet => 11,
                        ItemManager.Items.HighLevelBulletproofHelmet => 12,
                        ItemManager.Items.LegendaryBulletproofHelmet => 24,
                        _ => 0
                    };
                    needSalvagesToRepair = Mathf.CeilToInt(originalValue * CurrentHelmet.DurabilityPercent);
                    if (ComponentsCount >= needComponentsToRepair && SalvagesCount >= needSalvagesToRepair)
                    {
                        currentRepairing = 2;
                        repairingTime = Math.Max(ItemManager.craftables.Find(x => x.itemType == CurrentVest.itemType).craftingTime * (1f - CurrentVest.DurabilityPercent), 4f);
                        return true;
                    }
                }
            }
        }
        return false;
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
                agent.SetDestination(transform.position);
                lookPosition = Vector2.zero;
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
        if(temporaryAllowProhibitArea)
        {
            if (!GetCurrentArea().IsProhibited_Plan && !GetCurrentArea().IsProhibited)
            {
                temporaryAllowProhibitArea = false;
                agent.areaMask &= ~(1 << NavMesh.GetAreaFromName("Prohibited"));
                agent.areaMask &= ~(1 << NavMesh.GetAreaFromName("Prohibit_planned"));
            }
        }

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
        else
        {
            // 적이 있든 없든 일단 방어구 우선 착용
            if(!RightHandDisabled && !LeftHandDisabled)
            {
                if (currentWearingHelmet != null)
                {
                    WearHelmet(currentWearingHelmet);
                    return;
                }
                else if (currentWearingVest != null)
                {
                    WearVest(currentWearingVest);
                    return;
                }
            }

            if (inSightEnemies.Count == 0)
            {
                animator.SetBool("Attack", false);
                animator.SetBool("Aim", false);
                curAimDelay = 0;

                if (CurrentFarmingArea != null && (CurrentFarmingArea.IsProhibited_Plan || CurrentFarmingArea.IsProhibited))
                {
                    CurrentFarmingArea = FindNearest(farmingAreas);
                }

                if (runAwayDestination != Vector2.zero)
                {
                    if (Vector2.Distance(transform.position, runAwayDestination) < 1f)
                    {
                        runAwayDestination = Vector2.zero;
                        runAwayFrom = null;
                        agent.speed = moveSpeed;
                        sightMeshRenderer.material = m_SightNormal;
                    }
                    else
                    {
                        if(characteristics.FindIndex(x => x.type == CharacteristicType.Coward) != -1) agent.speed = moveSpeed * 1.3f;
                        return;
                    } 
                }

                if (keepEyesOnPosition != Vector2.zero)
                {
                    agent.SetDestination(transform.position);
                    return;
                }

                if (!(rightHandDisabled && leftHandDisabled))
                {
                    if(currentRepairing > 0)
                    {
                        Repair();
                        return;
                    }

                    if (Maintain())
                    {
                        CurrentStatus = Status.Maintain;
                        return;
                    }

                    if (detectedTraps.Count > 0)
                    {
                        DisarmTrap();
                        return;
                    }
                }

                if (threateningSoundPosition != Vector2.zero)
                {
                    InvestigateThreateningSound();
                }
                else if (targetEnemiesLastPosition != Vector2.zero)
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
                targetFarmingCorpse = null;
                targetFarmingSection = null;
                animator.SetBool("Crafting", false);
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
                            // 원거린데 총알 없으면 attackRange
                            if (distance < attackRange) enemyInAttackRange = true;
                            else if (ValidBullet != null)
                            {
                                Reload();
                                return;
                            }
                        }
                        // 근거리거나 원거리+총알 있으면 weapon.AttackRange
                        else if (distance < currentWeapon.AttackRange) enemyInAttackRange = true;
                    }
                    else if (distance < attackRange) enemyInAttackRange = true;

                    if (enemyInAttackRange)
                    {
                        if (strategyConditions[StrategyCase.SawAnEnemyAndItIsInAttackRange].TotalCondition.Invoke())
                        {
                            if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action == 2)
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
                            else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 1)
                            {
                                TryFarming();
                            }
                            else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction == 2)
                            {
                                if (!TryRunAway(TargetEnemy)) Combat(distance);
                            }
                        }
                    }
                    else
                    {
                        if (strategyConditions[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].TotalCondition.Invoke())
                        {
                            if (playerSurvivor) Debug.Log(linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action);
                            if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 0)
                            {
                                ApproachEnemy(TargetEnemy);
                            }
                            else if (linkedSurvivorData.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action == 1)
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
                                if (!TryRunAway(TargetEnemy)) ApproachEnemy(TargetEnemy);
                            }
                        }
                    }
                }
            }
        }
    }

    bool Maintain()
    {
        if (CheckStopBleeding()) return true;
        animator.SetBool("StopBleeding", false);

        if(CheckDrinking()) return true;
        animator.SetBool("Drinking", false);

        if (CheckReload()) return true;
        animator.SetBool("Reload", false);

        if (CheckRepair()) return true;
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

            if(currentEnchanting != null)
            {
                Enchant();
                return;
            }
            else if (CheckEnchantables())
            {
                curEnchantingTime = 0;
                return;
            }
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
        if (!(RightHandDisabled && LeftHandDisabled))
        {
            // 파밍을 하는 경우:
            // 1. 막금구가 아님
            // 2. 막금구인 경우 : 무기가 priority1 무기가 아님 || 총알이 없음
            if (!lastArea || !IsValid(currentWeapon) || currentWeapon.itemType != linkedSurvivorData.priority1Weapon || (CurrentWeaponAsRangedWeapon != null && CurrentWeaponAsRangedWeapon.CurrentMagazine == 0 && ValidBullet == null))
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
                if(corpse.Key.lastCurrentArea.IsProhibited || corpse.Key.lastCurrentArea.IsProhibited_Plan)
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
        if (!farmingAreas[GetCurrentArea()]) return GetCurrentArea();

        Area nearest = null;
        int minAreaDistance = 100;
        int areaDistance;
        float minDistance = float.MaxValue;
        float distance;
        List<Area> reserveRemoves = new();
        foreach (KeyValuePair<Area, bool> candidate in candidates)
        {
            Area area = candidate.Key;
            if (farmingAreas[area]) continue;
            if (area.IsProhibited || area.IsProhibited_Plan)
            {
                reserveRemoves.Add(area);
                continue;
            }

            Area curArea = GetCurrentArea();
            if (curArea.Distances.ContainsKey(candidate.Key)) areaDistance = curArea.Distances[candidate.Key];
            else continue;
            if(areaDistance < minAreaDistance)
            {
                minAreaDistance = areaDistance;
                nearest = candidate.Key;
                minDistance = float.MaxValue;
            }
            else if(areaDistance == minAreaDistance)
            {
                distance = Vector2.Distance(transform.position, candidate.Key.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = candidate.Key;
                }
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
                    Area corpseArea = corpse.lastCurrentArea;
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
        if (targetFarmingCorpse.lastCurrentArea == null || targetFarmingCorpse.lastCurrentArea.IsProhibited || targetFarmingCorpse.lastCurrentArea.IsProhibited_Plan)
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
                InGameUIManager.UpdateSelectedObjectInventory(targetFarmingCorpse);
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

            // 상자에 설치된 부비트랩이 있으면
            BoobyTrap setBoobyTrap = targetFarmingBox.items.Find(x => x is BoobyTrap) as BoobyTrap;
            if (setBoobyTrap != null && setBoobyTrap.IsTriggered)
            {
                // 감지장치가 있으면 파밍x
                if(inventory.Find(x => x.itemType == ItemManager.Items.TrapDetectionDevice) != null)
                {
                    farmingBoxes[targetFarmingBox] = true;
                    targetFarmingBox = null;
                    return;
                }
                // 없으면 폭발
                else
                {
                    setBoobyTrap.Trigger(this);
                    targetFarmingBox.items.Remove(setBoobyTrap);
                    InGameUIManager.UpdateSelectedObjectInventory(targetFarmingBox);
                    farmingBoxes[targetFarmingBox] = true;
                    targetFarmingBox = null;
                    return;
                }
            }

            float multiply = 1f;
            // 상자가 비었으면 파밍시간 절반
            if (targetFarmingBox.items.Count == 0) multiply = 2f;
            curFarmingTime += Time.deltaTime * aiCool * farmingSpeed * multiply;
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
        float volume = isAssassin ? 2 : 4;
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
                if(col.name == "Floor")
                {
                    sfxName = "footstep_concrete";
                    break;
                }
            }
            if (string.IsNullOrEmpty(sfxName)) sfxName = "footstep_grass";
            sfxName += leftFoot ? "1" : "2";
            sfxName += isAssassin ? ",3" : ",0.5";
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
        CurrentStatus = Status.FindingEnemy;
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
        if (item == null || item.itemType == ItemManager.Items.NotValid) return false;
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
        if (!IsValid(item)) return;
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
        else if(item is BulletproofHelmet newBulletproofHelmet)
        {
            GetItem(item);
            if (CompareBulletproofHelmetValue(newBulletproofHelmet))
            {
                //Equip(newBulletproofHelmet);
                currentWearingHelmet = newBulletproofHelmet;
            }
        }
        else if(item is BulletproofVest newBulletproofVest)
        {
            GetItem(item);
            if (CompareBulletproofVestValue(newBulletproofVest))
            {
                //Equip(newBulletproofVest);
                currentWearingVest = newBulletproofVest;
            }
        }
        else if(item.itemType.ToString().Contains("Bullet") || item.itemType.ToString().Contains("Rocket"))
        {
            currentWeaponisBestWeapon = false;
            GetItem(item);
            string wantWeapon = item.itemType.ToString().Split('_')[1];
            if (inventory.Find(x => x.itemType.ToString() == wantWeapon) is RangedWeapon weapon && CompareWeaponValue(weapon))
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
        bool alreadyHave = item.itemType switch
        {
            ItemManager.Items.LowLevelBulletproofHelmet or ItemManager.Items.MiddleLevelBulletproofHelmet or ItemManager.Items.HighLevelBulletproofHelmet
            or ItemManager.Items.LegendaryBulletproofHelmet or ItemManager.Items.LowLevelBulletproofVest or ItemManager.Items.MiddleLevelBulletproofVest
            or ItemManager.Items.HighLevelBulletproofVest or ItemManager.Items.LegendaryBulletproofVest => false,
            _ => true
        };
        Item sameItem = inventory.Find(x => x.itemType == item.itemType && x.quality == item.quality);
        if(sameItem == null) alreadyHave = false;
        if(alreadyHave)
        {
            sameItem.amount += item.amount;
        }
        else
        {
            inventory.Add(item);
            if (item.itemType == ItemManager.Items.TrapDetectionDevice)
            {
                trapDetectionDevice.SetActive(true);
                float detectionRange = item.quality switch
                {
                    CraftingQuality.Masterpiece => 2.5f,
                    CraftingQuality.Excellent => 2f,
                    CraftingQuality.Common => 1.25f,
                    CraftingQuality.Poor => 1f,
                    _ => 1.5f
                };
                trapDetectionDevice.GetComponent<CircleCollider2D>().radius = detectionRange;
            }
            else if (item.itemType == ItemManager.Items.BiometricRader)
            {
                biometricRader.SetActive(true);
                float detectionRange = item.quality switch
                {
                    CraftingQuality.Masterpiece => 20f,
                    CraftingQuality.Excellent => 17.5f,
                    CraftingQuality.Common => 12.5f,
                    CraftingQuality.Poor => 10f,
                    _ => 15f
                };
                biometricRader.GetComponent<CircleCollider2D>().radius = detectionRange;
                biometricRader.GetComponentInChildren<SpriteRenderer>().transform.localScale = new(detectionRange * 2, detectionRange * 2);
            }
            else if (item.itemType == ItemManager.Items.EnergyBarrier)
            {
                float obstructionRate = item.quality switch
                {
                    CraftingQuality.Masterpiece => 0.7f,
                    CraftingQuality.Excellent => 0.6f,
                    CraftingQuality.Common => 0.4f,
                    CraftingQuality.Poor => 0.3f,
                    _ => 0.5f
                };
                energyBarrier.SetActive(true);
                energyBarrier.GetComponent<Obstacle>().SetObstructionRate(obstructionRate);
            }
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void ConsumptionItem(Item item, int amount)
    {
        if (!IsValid(item)) return;
        item.amount -= amount;
        if(item.amount <= 0)
        {
            inventory.Remove(item);
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    // if newWeapon value > current weapon : return true
    bool CompareWeaponValue(Weapon newWeapon)
    {
        if (!CanUse(newWeapon)) return false;
        if(!IsValid(currentWeapon)) return true;
        if(newWeapon.itemName.TableEntryReference.Key == currentWeapon.itemName.TableEntryReference.Key) return false;

        // 새무기 = p1
        if (newWeapon.itemType == linkedSurvivorData.priority1Weapon)
        {
            if (newWeapon is RangedWeapon rangedWeapon) return HaveBullet(rangedWeapon);
            else return true;
        }
        // 새무기 = p2
        else if(newWeapon.itemType == linkedSurvivorData.priority2Weapon)
        {
            // 현무기 = p1
            if(currentWeapon.itemType == linkedSurvivorData.priority1Weapon)
            {
                if (currentWeapon is RangedWeapon rangedWeapon)
                {
                    if (HaveBullet(rangedWeapon)) return false;
                    else
                    {
                        if (newWeapon is RangedWeapon rangedNewWeapon) return HaveBullet(rangedNewWeapon);
                        else return true;
                    }
                }
                else return false;
            }
            // 새무기 p2, np
            else
            {
                if (newWeapon is RangedWeapon rangedNewWeapon) return HaveBullet(rangedNewWeapon);
                else return true;
            }
        }
        // 이하 새무기 np, 현무기 np
        if (currentWeapon is MeleeWeapon)
        {
            if (newWeapon is RangedWeapon rangedWeapon)
            {
                // 근 vs 원
                return HaveBullet(rangedWeapon);
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
                if (HaveBullet(newWeaponAsRangedWeapon))
                {
                    // 둘 다 총알이 있는 경우
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0)
                    {
                        return GetRangedWeaponTier(newWeaponAsRangedWeapon.itemType, newWeaponAsRangedWeapon.quality) > GetRangedWeaponTier(CurrentWeaponAsRangedWeapon.itemType, CurrentWeaponAsRangedWeapon.quality);
                    }
                    else return true;
                }
                else
                {
                    if (ValidBullet != null || CurrentWeaponAsRangedWeapon.CurrentMagazine > 0) return false;
                    else
                    {
                        // 둘 다 총알이 없는 경우
                        return GetRangedWeaponTier(newWeaponAsRangedWeapon.itemType, newWeaponAsRangedWeapon.quality) > GetRangedWeaponTier(CurrentWeaponAsRangedWeapon.itemType, CurrentWeaponAsRangedWeapon.quality);
                    }
                }
            }
        }
    }

    bool HaveBullet(RangedWeapon wantWeapon)
    {
        return wantWeapon.itemType switch
        {
            ItemManager.Items.LASER => true,
            ItemManager.Items.Bazooka => wantWeapon.CurrentMagazine > 0 || inventory.Find(x => x.itemType.ToString() == $"Rocket_Bazooka") != null,
            _ => wantWeapon.CurrentMagazine > 0 || inventory.Find(x => x.itemType.ToString() == $"Bullet_{wantWeapon.itemType}") != null,
        };
    }

    int GetRangedWeaponTier(ItemManager.Items wantWeapon, CraftingQuality quality)
    {
        int value = wantWeapon switch
        {
            ItemManager.Items.Pistol or ItemManager.Items.Revolver or ItemManager.Items.AdvancedBow => 1,
            ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun => 2,
            ItemManager.Items.Bazooka or ItemManager.Items.SniperRifle => 3,
            ItemManager.Items.AssaultRifle => 4,
            ItemManager.Items.LASER => 5,
            _ => 0
        };
        value += quality switch
        { 
            CraftingQuality.Masterpiece => 2,
            CraftingQuality.Excellent => 1,
            CraftingQuality.Common => -1,
            CraftingQuality.Poor => -2,
            _ => 0
        };
        return value;
    }

    bool CompareBulletproofHelmetValue(BulletproofHelmet newBulletproofHelmet)
    {
        if (!IsValid(currentHelmet)) return true;
        if (newBulletproofHelmet.Defense > currentHelmet.Defense) return true;
        else if (newBulletproofHelmet.Defense == currentHelmet.Defense && newBulletproofHelmet.CurDurability > currentHelmet.CurDurability) return true;
        else return false;
    }

    bool CompareBulletproofVestValue(BulletproofVest newBulletproofVest)
    {
        if (!IsValid(currentVest)) return true;
        if (newBulletproofVest.Defense > currentVest.Defense) return true;
        else if (newBulletproofVest.Defense == currentVest.Defense && newBulletproofVest.CurDurability > currentVest.CurDurability) return true;
        else return false;
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
            if (rightHandDisabled || wantWeapon.itemType == ItemManager.Items.Bow || wantWeapon.itemType == ItemManager.Items.AdvancedBow) hand = leftHand.transform;
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
            ApplyCorrectionStatByItem(wantWeapon, true);
        }
        InGameUIManager.UpdateSelectedObjectInventory(this);
    }

    void UnequipWeapon()
    {
        if (IsValid(currentWeapon))
        {
            GetItem(currentWeapon);
            Transform hand = rightHandDisabled || currentWeapon.itemType == ItemManager.Items.Bow || currentWeapon.itemType == ItemManager.Items.AdvancedBow ? leftHandTF : rightHandTF;
            Transform curWeaponTF = hand.Find($"{currentWeapon.itemType}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
                projectileGenerator.muzzleTF = null;
            }
            else
            {
                Debug.LogWarning($"Can't find weapon : {currentWeapon.itemType}");
            }
            curWeaponTF = transform.Find("Left Hand").Find($"{currentWeapon.itemType}");
            if (curWeaponTF != null)
            {
                curWeaponTF.gameObject.SetActive(false);
                projectileGenerator.muzzleTF = null;
            }
            ApplyCorrectionStatByItem(currentWeapon, false);
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
            ConsumptionItem(wantBulletproofHelmet, 1);
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
        ConsumptionItem(wantBulletproofVest, 1);
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

    void WearHelmet(BulletproofHelmet helmet)
    {
        CurrentStatus = Status.Wearing;
        lookPosition = Vector2.zero;
        agent.SetDestination(transform.position);
        curWearingTime += Time.deltaTime * aiCool * wearingSpeed;
        progressBar.fillAmount = curWearingTime / wearingTime;
        if (curWearingTime > wearingTime)
        {
            curWearingTime = 0;
            Equip(helmet);
            currentWearingHelmet = null;
        }
    }

    void WearVest(BulletproofVest vest)
    {
        CurrentStatus = Status.Wearing;
        lookPosition = Vector2.zero;
        agent.SetDestination(transform.position);
        curWearingTime += Time.deltaTime * aiCool * wearingSpeed;
        progressBar.fillAmount = curWearingTime / wearingTime;
        if(curWearingTime > wearingTime)
        {
            curWearingTime = 0;
            Equip(vest);
            currentWearingVest = null;
        }
    }

    void Repair()
    {
        CurrentStatus = Status.Reparing;
        lookPosition = Vector2.zero;
        agent.SetDestination(transform.position);
        animator.SetInteger("CraftingAnimNumber", 0);
        animator.SetBool("Crafting", true);
        curRepairingTime += Time.deltaTime * aiCool * craftingSpeed;
        progressBar.fillAmount = curRepairingTime / repairingTime;
        if (curRepairingTime > repairingTime)
        {
            curRepairingTime = 0;
            animator.SetBool("Crafting", false);
            if (currentRepairing == 1) CurrentHelmet.SetDurabilityPercent(1f);
            else CurrentVest.SetDurabilityPercent(1f);
            currentRepairing = 0;
        }
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

            int[] needLefts = new int[5];
            // 그 다음으로 Crafting Priority 체크
            if (linkedSurvivorData.priority1Crafting != null && linkedSurvivorData.priority1Crafting.itemType != ItemManager.Items.NotValid)
            {
                if (inventory.Find(x => x.itemType == linkedSurvivorData.priority1Crafting.itemType) == null)
                {
                    // 0: weapon, 1: helmet, 2: vest
                    int check = linkedSurvivorData.priority1Crafting.itemType switch
                    {
                        ItemManager.Items.Revolver or ItemManager.Items.Pistol or ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun
                        or ItemManager.Items.AssaultRifle or ItemManager.Items.SniperRifle or ItemManager.Items.Bazooka or ItemManager.Items.LASER => 0,
                        ItemManager.Items.LowLevelBulletproofHelmet or ItemManager.Items.MiddleLevelBulletproofHelmet or ItemManager.Items.HighLevelBulletproofHelmet
                        or ItemManager.Items.LegendaryBulletproofHelmet => 1,
                        ItemManager.Items.LowLevelBulletproofVest or ItemManager.Items.MiddleLevelBulletproofVest or ItemManager.Items.HighLevelBulletproofVest
                        or ItemManager.Items.LegendaryBulletproofVest => 2,
                        _ => -1
                    };
                    // 이미 장비하고 있으면 재료 보존x
                    if(!(check == 0 && currentWeapon != null && currentWeapon.itemType == linkedSurvivorData.priority1Crafting.itemType
                        || check == 1 && currentHelmet != null && currentHelmet.itemType == linkedSurvivorData.priority1Crafting.itemType
                        || check == 2 && currentVest != null && currentVest.itemType == linkedSurvivorData.priority1Crafting.itemType))
                    {
                        var priority1 = craftables.Find(x => x.itemType == linkedSurvivorData.priority1Crafting.itemType);
                        if (priority1 != null)
                        {
                            currentCrafting = priority1;
                            return true;
                        }
                        else
                        {
                            var original = ItemManager.craftables.Find(x => x.itemType == linkedSurvivorData.priority1Crafting.itemType);
                            needLefts = new int[] { original.needAdvancedComponentCount, original.needComponentsCount, original.needChemicalsCount, original.needGunpowderCount, original.needSalvagesCount };
                        }
                        
                    }
                }
                // cp2 체크
                if (linkedSurvivorData.priority2Crafting != null && linkedSurvivorData.priority2Crafting.itemType != ItemManager.Items.NotValid)
                {
                    if (inventory.Find(x => x.itemType == linkedSurvivorData.priority2Crafting.itemType) == null)
                    {
                        // 0: weapon, 1: helmet, 2: vest
                        int check = linkedSurvivorData.priority2Crafting.itemType switch
                        {
                            ItemManager.Items.Revolver or ItemManager.Items.Pistol or ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun
                            or ItemManager.Items.AssaultRifle or ItemManager.Items.SniperRifle or ItemManager.Items.Bazooka or ItemManager.Items.LASER => 0,
                            ItemManager.Items.LowLevelBulletproofHelmet or ItemManager.Items.MiddleLevelBulletproofHelmet or ItemManager.Items.HighLevelBulletproofHelmet
                            or ItemManager.Items.LegendaryBulletproofHelmet => 1,
                            ItemManager.Items.LowLevelBulletproofVest or ItemManager.Items.MiddleLevelBulletproofVest or ItemManager.Items.HighLevelBulletproofVest
                            or ItemManager.Items.LegendaryBulletproofVest => 2,
                            _ => -1
                        };
                        // 이미 장비하고 있으면 재료 보존x
                        if (!(check == 0 && currentWeapon != null && currentWeapon.itemType == linkedSurvivorData.priority2Crafting.itemType
                            || check == 1 && currentHelmet != null && currentHelmet.itemType == linkedSurvivorData.priority2Crafting.itemType
                            || check == 2 && currentVest != null && currentVest.itemType == linkedSurvivorData.priority2Crafting.itemType))
                        {
                            var priority2 = craftables.Find(x => x.itemType == linkedSurvivorData.priority2Crafting.itemType);
                            if (priority2 != null)
                            {
                                // cp1의 재료를 남기는 선에서 cp2 제작
                                if (AdvancedComponentCount - priority2.needAdvancedComponentCount >= needLefts[0]
                                    && ComponentsCount - priority2.needComponentsCount >= needLefts[1]
                                    && ChemicalsCount - priority2.needChemicalsCount >= needLefts[2]
                                    && GunpowderCount - priority2.needGunpowderCount >= needLefts[3]
                                    && SalvagesCount - priority2.needSalvagesCount >= needLefts[4])
                                currentCrafting = priority2;
                                return true;
                            }
                            else
                            {
                                var original = ItemManager.craftables.Find(x => x.itemType == linkedSurvivorData.priority2Crafting.itemType);
                                needLefts[0] += original.needAdvancedComponentCount;
                                needLefts[1] += original.needComponentsCount;
                                needLefts[2] += original.needChemicalsCount;
                                needLefts[3] += original.needGunpowderCount;
                                needLefts[4] += original.needSalvagesCount;
                            }
                        }
                    }
                }
            }

            for(int i=1; i <= craftables.Count; i++)
            {
                // craftingAllow = false면 continue
                if (!linkedSurvivorData.craftingAllows[ItemManager.craftables.FindIndex(x => x.itemType == craftables[^i].itemType)]) continue;
                if(needLefts.Sum() > 0)
                {
                    if (linkedSurvivorData.priority1Crafting != null && linkedSurvivorData.priority1Crafting.etcNeedItems != null && linkedSurvivorData.priority1Crafting.etcNeedItems.Count > 0)
                    {
                        // 만약에 이 craftable이 craftingPriority1의 하위재료가 아니면
                        if(!linkedSurvivorData.priority1Crafting.etcNeedItems.ContainsKey(craftables[^i].itemType))
                        {
                            // priority1 만들 재료를 남겨두고도 충분하면 만들고
                            // 그렇지 않으면 넘어가
                            if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < needLefts[0]) continue;
                            if (ComponentsCount - craftables[^i].needComponentsCount < needLefts[1]) continue;
                            if (ChemicalsCount - craftables[^i].needChemicalsCount < needLefts[2]) continue;
                            if (GunpowderCount - craftables[^i].needGunpowderCount < needLefts[3]) continue;
                            if (SalvagesCount - craftables[^i].needSalvagesCount < needLefts[4]) continue;
                        }
                        else
                        {
                            // 이 craftable이 craftingPriority1의 하위재료면
                            // 이미 재료가 충분히 만들어져 있는지 체크
                            Item alreadyHave = inventory.Find(x => x.itemType == craftables[^i].itemType);
                            if(alreadyHave != null && alreadyHave.amount >= linkedSurvivorData.priority1Crafting.etcNeedItems[craftables[^i].itemType])
                            {
                                // 재료가 충분하면 하위재료를 priority1 만들 수 있는 만큼만 남기고 충분하면 여분으로만 더 만들게
                                if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < needLefts[0]) continue;
                                if (ComponentsCount - craftables[^i].needComponentsCount < needLefts[1]) continue;
                                if (ChemicalsCount - craftables[^i].needChemicalsCount < needLefts[2]) continue;
                                if (GunpowderCount - craftables[^i].needGunpowderCount < needLefts[3]) continue;
                                if (SalvagesCount - craftables[^i].needSalvagesCount < needLefts[4]) continue;
                            }
                            // 아니면(priority1의 재료가 부족하면) 만듦
                            else
                            {
                                currentCrafting = craftables[^i];
                                return true;
                            }
                        }
                    }
                    else if(linkedSurvivorData.priority2Crafting != null && linkedSurvivorData.priority2Crafting.etcNeedItems != null && linkedSurvivorData.priority2Crafting.etcNeedItems.Count > 0)
                    {
                        // craftingPriority2의 하위재료인지도 체크
                        if (!linkedSurvivorData.priority2Crafting.etcNeedItems.ContainsKey(craftables[^i].itemType))
                        {
                            if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < needLefts[0]) continue;
                            if (ComponentsCount - craftables[^i].needComponentsCount < needLefts[1]) continue;
                            if (ChemicalsCount - craftables[^i].needChemicalsCount < needLefts[2]) continue;
                            if (GunpowderCount - craftables[^i].needGunpowderCount < needLefts[3]) continue;
                            if (SalvagesCount - craftables[^i].needSalvagesCount < needLefts[4]) continue;
                        }
                        else
                        {
                            Item alreadyHave = inventory.Find(x => x.itemType == craftables[^i].itemType);
                            if (alreadyHave != null && alreadyHave.amount >= linkedSurvivorData.priority2Crafting.etcNeedItems[craftables[^i].itemType])
                            {
                                if (AdvancedComponentCount - craftables[^i].needAdvancedComponentCount < needLefts[0]) continue;
                                if (ComponentsCount - craftables[^i].needComponentsCount < needLefts[1]) continue;
                                if (ChemicalsCount - craftables[^i].needChemicalsCount < needLefts[2]) continue;
                                if (GunpowderCount - craftables[^i].needGunpowderCount < needLefts[3]) continue;
                                if (SalvagesCount - craftables[^i].needSalvagesCount < needLefts[4]) continue;
                            }
                            else
                            {
                                currentCrafting = craftables[^i];
                                return true;
                            }
                        }
                    }
                }
                // 이미 보유 중인 아이템은 만들지 않음
                if (inventory.Find(x => x.itemType == craftables[^i].itemType) != null || (IsValid(currentWeapon) && currentWeapon.itemType == craftables[^i].itemType)
                    || (IsValid(currentHelmet) && currentHelmet.itemType == craftables[^i].itemType) || (IsValid(currentVest) && currentVest.itemType == craftables[^i].itemType)) continue;

                bool gunNeeds = false;
                // 총 필요성 검사
                if (IsValid(currentWeapon))
                {
                    if (currentWeapon.itemType == linkedSurvivorData.priority1Weapon || currentWeapon.itemType == linkedSurvivorData.priority2Weapon && craftables[^i].itemType != linkedSurvivorData.priority1Weapon)
                    {
                        // 지금 무기가 priority1 또는 p2 이면서 만드려는 무기가 p1이 아니면 패스
                        gunNeeds = false;
                    }
                    else if (currentWeapon is not RangedWeapon)
                    {
                        // 현재 무기가 총이 아니면 총부터 만들도록 craftables[^i]가 총이 아니면 패스
                        bool isRangedWeapon = craftables[^i].itemType switch
                        {
                            ItemManager.Items.LASER or ItemManager.Items.Bazooka or ItemManager.Items.SniperRifle or ItemManager.Items.AssaultRifle
                            or ItemManager.Items.SubMachineGun or ItemManager.Items.ShotGun or ItemManager.Items.Pistol or ItemManager.Items.Revolver
                            => true,
                            _ => false,
                        };
                        if (!isRangedWeapon) continue;
                        // 총이 없으면 무조건 만들고
                        gunNeeds = true;
                    }
                    else 
                    {
                        // 총이 있는 경우면
                        // craftables[^i]가 priority1 또는 p2 무기면 만들고
                        if (craftables[^i].itemType == linkedSurvivorData.priority1Weapon || craftables[^i].itemType == linkedSurvivorData.priority2Weapon) gunNeeds = true;
                        else
                        {
                            bool needCompare = false;
                            // 만드려는게 레이져면 만들고
                            if (craftables[^i].itemType == ItemManager.Items.LASER) needCompare = false;
                            // 아니면 현재 무기 총알 체크 => 총알이 없으면 총기 티어비교
                            else if (CurrentWeaponAsRangedWeapon.CurrentMagazine == 0 && ValidBullet == null) needCompare = true;
                            else
                            {
                                // 총알이 있으면, 내가 만드려는 무기의 총알이 있을 때만 총기 티어 비교
                                string bulletName;
                                if (craftables[^i].itemType != ItemManager.Items.Bazooka) bulletName = $"Bullet_{craftables[^i].itemType}";
                                else bulletName = "Rocket_Bazooka";
                                if (Enum.TryParse(bulletName, out ItemManager.Items bullet))
                                {
                                    Item validBullet = inventory.Find(x => x.itemType == bullet);
                                    if (validBullet != null) needCompare = true;
                                }
                            }
                            if (needCompare)
                            {
                                gunNeeds = GetRangedWeaponTier(craftables[^i].itemType, CraftingQuality.NotCrafted) > GetRangedWeaponTier(currentWeapon.itemType, CraftingQuality.NotCrafted);
                            }
                        }
                    }
                }
                else gunNeeds = true;

                bool bulletNeeds = false;
                Item bestWeapon = null;
                if (craftables[^i].itemType == ItemManager.Items.Bullet_Revolver || craftables[^i].itemType == ItemManager.Items.Bullet_Pistol
                    || craftables[^i].itemType == ItemManager.Items.Bullet_SubMachineGun || craftables[^i].itemType == ItemManager.Items.Bullet_ShotGun
                    || craftables[^i].itemType == ItemManager.Items.Bullet_AssaultRifle || craftables[^i].itemType == ItemManager.Items.Bullet_SniperRifle
                    || craftables[^i].itemType == ItemManager.Items.Rocket_Bazooka || craftables[^i].itemType == ItemManager.Items.Arrow)
                {
                    // 총알 필요성 검사
                    if (linkedSurvivorData.priority1Weapon != ItemManager.Items.NotValid)
                    {
                        if (IsValid(currentWeapon) && currentWeapon.itemType == linkedSurvivorData.priority1Weapon) bestWeapon = currentWeapon;
                        else bestWeapon = inventory.Find(x => x.itemType == linkedSurvivorData.priority1Weapon);
                    }
                    if (bestWeapon == null)
                    {
                        int bestWeaponValue = 0;
                        if (CurrentWeaponAsRangedWeapon != null)
                        {
                            bestWeapon = CurrentWeaponAsRangedWeapon;
                            bestWeaponValue = GetRangedWeaponTier(CurrentWeaponAsRangedWeapon.itemType, CurrentWeaponAsRangedWeapon.quality);
                        }
                        foreach (var item in inventory)
                        {
                            if (item is RangedWeapon rangedWeapon && GetRangedWeaponTier(rangedWeapon.itemType, CraftingQuality.NotCrafted) > bestWeaponValue)
                            {
                                bestWeapon = item;
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
                    case ItemManager.Items.Arrow:
                        if (bulletNeeds)
                        {
                            if(craftables[^i].itemType == ItemManager.Items.Arrow)
                            {
                                if (bestWeapon.itemType == ItemManager.Items.Bow || bestWeapon.itemType == ItemManager.Items.AdvancedBow) return true;
                                else return false;
                            }
                            else if (craftables[^i].itemType.ToString().Split("_")[1] == bestWeapon.itemType.ToString())
                            {
                                currentCrafting = craftables[^i];
                                return true;
                            }
                            else continue;
                        }
                        else continue;
                    case ItemManager.Items.Revolver:
                    case ItemManager.Items.Pistol:
                    case ItemManager.Items.SubMachineGun:
                    case ItemManager.Items.ShotGun:
                    case ItemManager.Items.SniperRifle:
                    case ItemManager.Items.AssaultRifle:
                    case ItemManager.Items.Bazooka:
                    case ItemManager.Items.LASER:
                    case ItemManager.Items.Bow:
                    case ItemManager.Items.AdvancedBow:
                        if (gunNeeds)
                        {
                            currentCrafting = craftables[^i];
                            return true;
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

    bool CheckEnchantables()
    {
        Item poison = inventory.Find(x => x.itemType == ItemManager.Items.Poison);
        if(poison != null)
        {
            if (currentWeapon is MeleeWeapon weapon && weapon.DamageType == DamageType.Slash)
            {
                if (!weapon.IsEnchanted)
                {
                    currentEnchanting = weapon;
                    return true;
                }
            }
            Item notEnchantedBearTrap = inventory.Find(x => x.itemType == ItemManager.Items.BearTrap && !((Buriable)x).IsEnchanted);
            if(notEnchantedBearTrap != null)
            {
                currentEnchanting = notEnchantedBearTrap;
                return true;
            }
            Item notEnchantedArrow = inventory.Find(x => x.itemType == ItemManager.Items.Arrow);
            if(notEnchantedArrow != null && (CurrentWeapon.itemType == ItemManager.Items.Bow || CurrentWeapon.itemType == ItemManager.Items.AdvancedBow))
            {
                currentEnchanting = notEnchantedArrow;
                return true;
            }
        }
        return false;
    }

    void Craft()
    {
        CurrentStatus = Status.Crafting;
        lookPosition = Vector2.zero;
        agent.SetDestination(transform.position);
        animator.SetInteger("CraftingAnimNumber", currentCrafting.craftingAnimNumber);
        animator.SetBool("Crafting", true);

        float craftingSpeedCorrection = 1f;
        if (characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (CurrentCrafting.itemType == ItemManager.Items.BearTrap || CurrentCrafting.itemType == ItemManager.Items.ShrapnelTrap || CurrentCrafting.itemType == ItemManager.Items.NoiseTrap || CurrentCrafting.itemType == ItemManager.Items.ChemicalTrap || CurrentCrafting.itemType == ItemManager.Items.ExplosiveTrap))
        {
            craftingSpeedCorrection = 1.3f;
        }
        animator.SetFloat("CraftingSpeed", craftingSpeed * craftingSpeedCorrection);
        curCraftingTime += Time.deltaTime * aiCool * craftingSpeed * craftingSpeedCorrection;
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
            int durabilityCount = 0;
            float durabilitySum = 0;
            foreach (var etcNeeds in currentCrafting.etcNeedItems)
            {
                var targetItem = inventory.Find(x => x.itemType == etcNeeds.Key);
                if(targetItem is BulletproofHelmet bH)
                {
                    durabilityCount++;
                    durabilitySum += bH.DurabilityPercent;
                }
                else if(targetItem is BulletproofVest bV)
                {
                    durabilityCount++;
                    durabilitySum += bV.DurabilityPercent;
                }
                ConsumptionItem(targetItem, etcNeeds.Value);
            }

            int amount = currentCrafting.outputAmount;
            CraftingQuality craftingQuality = CraftingQuality.NotCrafted;
            if(ItemManager.CheckUseQuality(currentCrafting.itemType))
            {
                float craftingQualityChance = UnityEngine.Random.Range(0, 100f);
                float pMasterPiece = (crafting - 40) * 1.25f;
                float pExcellent = (crafting - 20) * 1.25f;
                float pFine = crafting * 1.25f;
                float pCommon = (crafting + 20) * 1.25f;
                if (craftingQualityChance < pMasterPiece) craftingQuality = CraftingQuality.Masterpiece;
                else if (craftingQualityChance < pMasterPiece + pExcellent) craftingQuality = CraftingQuality.Excellent;
                else if (craftingQualityChance < pMasterPiece + pExcellent + pFine) craftingQuality = CraftingQuality.Fine;
                else if (craftingQualityChance < pMasterPiece + pExcellent + pFine + pCommon) craftingQuality = CraftingQuality.Common;
                else craftingQuality = CraftingQuality.Poor;
            }
            if(craftingQuality == CraftingQuality.Masterpiece)
            {
                if(playerSurvivor)
                {
                    AchievementManager.UnlockAchievement("Masterpiece");
                }
                var message = new LocalizedString("Basic", "Masterpiece Crafted");
                string crafter = linkedSurvivorData.localizedSurvivorName.GetLocalizedString();
                string crafted = new LocalizedString("Item", currentCrafting.itemType.ToString()).GetLocalizedString();
                message.Arguments = new[] { crafter, crafted };
                InGameUIManager.AddLog(message.GetLocalizedString());
            }
            ItemManager.AddItems(currentCrafting.itemType, amount, craftingQuality);
            int isEquipable = -1;
            Item item = null;
            for (int i = 1; i <= amount; i++)
            {
                item = ItemManager.itemDictionary[currentCrafting.itemType][^i];
                if (item is Weapon) isEquipable = 0;
                else if(item is BulletproofHelmet bH)
                {
                    isEquipable = 1;
                    bH.SetDurabilityPercent(durabilityCount == 0 ? 1 : durabilitySum / durabilityCount);
                }
                else if (item is BulletproofVest bV)
                {
                    isEquipable = 2;
                    bV.SetDurabilityPercent(durabilityCount == 0 ? 1 : durabilitySum / durabilityCount);
                }

                AcqireItem(item);
            }
            if(playerSurvivor)
            {
                linkedSurvivorData.craftingCount++;
                int totalCrafting = PlayerPrefs.GetInt("Total Crafting Count");
                PlayerPrefs.SetInt("Total Crafting Count", totalCrafting + 1);
                AchievementManager.SetStat("Total_Crafting", totalCrafting + 1);
                if (PlayerPrefs.GetInt("Total Crafting Count") >= 100) AchievementManager.UnlockAchievement("Foreman");
                if (linkedSurvivorData.craftingCount >= 10) AchievementManager.UnlockAchievement("Craftsman");
            }

            float chanceToIncreaseStat = UnityEngine.Random.Range(0, 1f);
            if (chanceToIncreaseStat < currentCrafting.requiredKnowledge * 0.01f)
            {
                linkedSurvivorData.IncreaseStats(0, 0, 0, 0, 1, 0);
                increaseCrafting++;
            }
            currentCrafting = null;
            craftables.Clear();
            if (isEquipable == 0 && CompareWeaponValue((Weapon)item)) Equip((Weapon)item);
            else if (isEquipable == 1) currentWearingHelmet = (BulletproofHelmet)item;
            else if (isEquipable == 2) currentWearingVest = (BulletproofVest)item;
            CheckCraftables();
        }
    }

    void Enchant()
    {
        CurrentStatus = Status.Enchanting;
        agent.SetDestination(transform.position);
        animator.SetInteger("CraftingAnimNumber", 2);
        animator.SetBool("Crafting", true);

        curEnchantingTime += Time.deltaTime * aiCool;
        progressBar.fillAmount = curEnchantingTime / enchantingTime;
        if(curEnchantingTime > enchantingTime)
        {
            if (currentEnchanting is MeleeWeapon weapon) weapon.Enchant();
            else if (currentEnchanting is Buriable buriable)
            {
                ConsumptionItem(buriable, 1);
                ItemManager.AddItems(ItemManager.Items.BearTrap_Enchanted, 1);
                Item item = ItemManager.itemDictionary[ItemManager.Items.BearTrap_Enchanted][^1];
                ((Buriable)item).SetDamage(buriable.Damage);
                GetItem(item);
            }
            else
            {
                var arrow = inventory.Find(x => x.itemType == ItemManager.Items.Arrow);
                if(arrow == null)
                {
                    currentEnchanting = null;
                    return;
                }
                int amount = Math.Min(arrow.amount, 5);
                ConsumptionItem(arrow, amount);
                ItemManager.AddItems(ItemManager.Items.Arrow_Enchanted, amount);
                for (int i = 0; i < amount; i++)
                {
                    GetItem(ItemManager.itemDictionary[ItemManager.Items.Arrow_Enchanted][^(i + 1)]);
                }
            }
            var poison = inventory.Find(x => x.itemType == ItemManager.Items.Poison);
            if (poison != null) ConsumptionItem(poison, 1);
        }
    }

    void CheckCraftables()
    {
        craftables.Clear();

        int knowledge = linkedSurvivorData.Knowledge;
        foreach(var craftable in ItemManager.craftables)
        {
            bool trapExpertAndTrap = characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (craftable.itemType == ItemManager.Items.BearTrap || craftable.itemType == ItemManager.Items.NoiseTrap || craftable.itemType == ItemManager.Items.ShrapnelTrap || craftable.itemType == ItemManager.Items.ChemicalTrap || craftable.itemType == ItemManager.Items.ExplosiveTrap || craftable.itemType == ItemManager.Items.TrapDetectionDevice);
            bool haveCraftingMaterials = AdvancedComponentCount >= craftable.needAdvancedComponentCount && ComponentsCount >= craftable.needComponentsCount && ChemicalsCount >= craftable.needChemicalsCount && SalvagesCount >= craftable.needSalvagesCount && GunpowderCount >= craftable.needGunpowderCount;
            if ((knowledge >= craftable.requiredKnowledge && haveCraftingMaterials) || trapExpertAndTrap)
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

            curTrappingTime += Time.deltaTime * aiCool * trappingSpeed;
            progressBar.fillAmount = curTrappingTime / trappingTime;
            if(curTrappingTime > trappingTime)
            {
                curTrappingTime = 0;
                progressBar.fillAmount = 0;
                if (Enum.TryParse(curBurying.itemType.ToString(), out ResourceEnum.Prefab trap))
                {
                    Trap settedTrap = PoolManager.Spawn(trap, trapPlace.transform.position).GetComponent<Trap>();
                    settedTrap.GetComponent<Animator>().SetTrigger("Reset");
                    if (curBurying.IsEnchanted) settedTrap.Enchant();
                    settedTrap.setter = this;
                    settedTrap.linkedItem = new Buriable(curBurying.itemType, curBurying.itemName, 10, curBurying.Damage);
                    trapPlace.SetTrap(settedTrap);
                    ConsumptionItem(Inventory.Find(x => x.itemType == curBurying.itemType), 1);
                    curBurying = null;
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
        if (curSettingBoobyTrap == null || curSettingBoobyTrapBox == null || targetFarmingBox == null) return false;
        CurrentStatus = Status.Trapping;
        agent.SetDestination(transform.position);
        lookPosition = curSettingBoobyTrapBox.transform.position;
        
        curTrappingTime += Time.deltaTime * aiCool * trappingSpeed;
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

    bool reached;
    void DisarmTrap()
    {
        CurrentStatus = Status.TrapDisarming;
        if (curDisarmTrap == null)
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
            reached = false;
        }
        else if(Vector2.Distance(transform.position, curDisarmTrap.transform.position) > 1.5f && !reached)
        {
            agent.SetDestination(curDisarmTrap.transform.position);
        }
        else
        {
            reached = true;
            agent.SetDestination(transform.position);
            lookPosition = curDisarmTrap.transform.position;
            curDisarmTime += Time.deltaTime * aiCool * trappingSpeed;
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

    void DisarmBoobyTrap()
    {
        // 추후 업데이트
    }
    #endregion

    public void DetectSurvivor(List<Vector2> positions)
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
        if (sightMeshRenderer.material != m_SightAlert) sightMeshRenderer.material = m_SightSuspicious;
        targetEnemiesLastPosition = nearest;
    }

    #region Combat
    void InvestigateThreateningSound()
    {
        CurrentStatus = Status.InvestigateThreateningSound;
        animator.SetBool("Crafting", false);
        Area soundArea = GameManager.Instance.BattleRoyaleManager.GetArea(threateningSoundPosition);
        if (soundArea.IsProhibited || soundArea.IsProhibited_Plan)
        {
            keepEyesOnPosition = threateningSoundPosition;
            threateningSoundPosition = Vector2.zero;
            return;
        }

        // 도달 가능하지 못한 곳에 좌표가 찍히면 벽박고 있는 문제 해결
        int destinationMargin = 1;
        while(destinationMargin < 10)
        {
            if (destinationMargin > 4)
            {
                threateningSoundPosition = Vector2.zero;
                return;
            }
            if(!NavMesh.SamplePosition(threateningSoundPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) destinationMargin++;
            else
            {
                threateningSoundPosition = hit.position;
                break;
            }
        }
        if (Vector2.Distance(transform.position, threateningSoundPosition) < destinationMargin)
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
            float targetAtkRange = target.CurrentWeaponAsRangedWeapon != null ? target.CurrentWeaponAsRangedWeapon.AttackRange : target.attackRange;
            if ((area.IsProhibited_Plan || area.IsProhibited) && targetAtkRange <= atkRange)
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
        // 적이 너무 가까우면 그냥 바로 사격
        if (Vector2.Distance(transform.position, TargetEnemy.transform.position) < 5f) curAimDelay = aimDelay + 1;
        progressBar.fillAmount = curAimDelay / aimDelay;
        if(curAimDelay > aimDelay)
        {
            progressBar.fillAmount = 0;
            curShotTime -= Time.deltaTime * aiCool;
            if(curShotTime < 0)
            {
                animator.SetInteger("ShotAnimNumber", CurrentWeaponAsRangedWeapon.ShotAnimNumber);
                animator.SetTrigger("Fire");
                float chanceToIncreaseStat = UnityEngine.Random.Range(0, 1f);
                if (chanceToIncreaseStat < CurrentWeaponAsRangedWeapon.ShotCoolTime * 0.1f)
                {
                    linkedSurvivorData.IncreaseStats(0, 0, 0, 1, 0, 0);
                    increaseShooting++;
                }
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
        animator.SetFloat("ReloadSpeed", reloadSpeed);
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
            int destinationMargin = 1;
            while (destinationMargin < 10)
            {
                if(destinationMargin > 4)
                {
                    runAwayDestination = Vector2.zero;
                    return false;
                }
                if (!NavMesh.SamplePosition(runAwayDestination, out NavMeshHit hit, 1f, NavMesh.AllAreas)) destinationMargin++;
                else
                {
                    runAwayDestination = hit.position;
                    break;
                }
            }
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
                RaycastHit2D[] hits = Physics2D.RaycastAll(runAwayFrom.transform.position, direction, 10f * j, LayerMask.GetMask("Wall", "Edge"));
                if (hits.Length > 0) enemyBlockeds.Add((Vector2)runAwayFrom.transform.position + direction);
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
        sightEdgeCount = (int)(sightAngle / 3);
        //sightEdgeCount = (int)((35 - GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count) * 1.5f);
        sightVertices = new Vector3[sightEdgeCount + 1 + 1];  // +1은 원점을 포함
        sightTriangles = new int[(sightEdgeCount + 1) * 3];     // 삼각형 그리기
        sightColliderPoints = new Vector2[sightVertices.Length];
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
        if (noiseMaker == this || inSightEnemies.Contains(noiseMaker as Survivor) || noiseMaker == lastTargetEnemy
            || noiseMaker is Trap trap && trap.Victim == this) return;
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
            if (noiseMaker != null && noiseMaker is Survivor) HeardIndistinguishableSound((noiseMaker as Survivor).survivorName.TableEntryReference.Key, soundOrigin);
            else Debug.LogWarning("There are no noiseMaker");
        }
    }

    void HeardDistinguishableSound(Vector2 soundOrigin)
    {
        if (currentStatus == Status.InCombat) return;
        if(sightMeshRenderer.material != m_SightAlert)emotionAnimator.SetTrigger("Alert");
        sightMeshRenderer.material = m_SightAlert;
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
        //Debug.Log(noiseMaker);
        heardSound = noiseMaker;
        SoundsMemory sound = soundsMemories.Find(x => Vector2.Angle(soundOrigin - (Vector2)transform.position, x.soundOrigin - (Vector2)transform.position) < 30f);
        if (sound != null)
        {
            if(sound.soundTime > 3)
            {
                sound.soundTime = 3;
                HeardDistinguishableSound(soundOrigin);
            }
            return;
        }
        else soundsMemories.Add(new(heardSound, soundOrigin));

        if (currentStatus == Status.InCombat) return;
        if(sightMeshRenderer.material != m_SightSuspicious && sightMeshRenderer.material != m_SightAlert) emotionAnimator.SetTrigger("Suspicious");
        if(sightMeshRenderer.material != m_SightAlert)sightMeshRenderer.material = m_SightSuspicious;
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
    void ApplyDamage(Survivor attacker, float damage, Item weapon, InjurySiteMajor damagePart, InjurySite specificDamagePart, DamageType damageType)
    {
        if (isDead) return;
        Injury alreadyHaveInjury = injuries.Find(x => x.site == specificDamagePart);
        // 0 : not artificial, 1 : artificial, 2 : augmented, 3 : transcendant
        int damagedPartIsArtifical = 0;
        bool noPain = false;
        if (alreadyHaveInjury != null)
        {
            if (alreadyHaveInjury.type == InjuryType.ArtificialPartsTransplanted || alreadyHaveInjury.type == InjuryType.ArtificialPartsDamaged
                || alreadyHaveInjury.type == InjuryType.AugmentedPartsTransplanted || alreadyHaveInjury.type == InjuryType.AugmentedPartsDamaged
                || alreadyHaveInjury.type == InjuryType.TranscendantPartsTransplanted || alreadyHaveInjury.type == InjuryType.TranscendantPartsDamaged)
            {
                if (alreadyHaveInjury.type == InjuryType.ArtificialPartsTransplanted || alreadyHaveInjury.type == InjuryType.ArtificialPartsDamaged) damagedPartIsArtifical = 1;
                else if (alreadyHaveInjury.type == InjuryType.AugmentedPartsTransplanted || alreadyHaveInjury.type == InjuryType.AugmentedPartsDamaged) damagedPartIsArtifical = 2;
                else if (alreadyHaveInjury.type == InjuryType.TranscendantPartsTransplanted || alreadyHaveInjury.type == InjuryType.TranscendantPartsDamaged) damagedPartIsArtifical = 3;
                // 재밌는 switch 용법
                noPain = alreadyHaveInjury.site switch
                {
                    InjurySite.Organ or InjurySite.RightEye or InjurySite.LeftEye or InjurySite.RightEar or InjurySite.LeftEar => false,
                    _ => true,
                };
            }
            if (damagedPartIsArtifical == 0 && (damagePart == InjurySiteMajor.Head || damagePart == InjurySiteMajor.Torso || alreadyHaveInjury.degree < 1)) damage *= 1 + alreadyHaveInjury.degree;
        }
        else
        {
            // Upperparts 체크
            foreach(var upperpart in Injury.GetUpperParts(specificDamagePart))
            {
                Injury upperpartAlreadyHaveInjury = injuries.Find(x => x.site == upperpart);
                if(upperpartAlreadyHaveInjury != null)
                {
                    if (upperpartAlreadyHaveInjury.type == InjuryType.ArtificialPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.ArtificialPartsDamaged
                        || upperpartAlreadyHaveInjury.type == InjuryType.AugmentedPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.AugmentedPartsDamaged
                        || upperpartAlreadyHaveInjury.type == InjuryType.TranscendantPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.TranscendantPartsDamaged)
                    {
                        if (upperpartAlreadyHaveInjury.type == InjuryType.ArtificialPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.ArtificialPartsDamaged) damagedPartIsArtifical = 1;
                        else if (upperpartAlreadyHaveInjury.type == InjuryType.AugmentedPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.AugmentedPartsDamaged) damagedPartIsArtifical = 2;
                        else if (upperpartAlreadyHaveInjury.type == InjuryType.TranscendantPartsTransplanted || upperpartAlreadyHaveInjury.type == InjuryType.TranscendantPartsDamaged) damagedPartIsArtifical = 3;
                        noPain = true;
                        injuries.Add(new(specificDamagePart, upperpartAlreadyHaveInjury.type, 0));
                        alreadyHaveInjury = injuries[^1];
                        break;
                    }
                }
            }
        }
        float maxDamage = specificDamagePart switch
        {
            InjurySite.RightArm or InjurySite.LeftArm or InjurySite.RightLeg or InjurySite.LeftLeg => Mathf.Min(damage, 80),
            InjurySite.RightKnee or InjurySite.LeftKnee => Mathf.Min(damage, 50),
            InjurySite.RightHand or InjurySite.LeftHand or InjurySite.RightFoot or InjurySite.LeftFoot => Mathf.Min(damage, 30),
            InjurySite.RightThumb or InjurySite.LeftThumb or InjurySite.RightIndexFinger or InjurySite.LeftIndexFinger or InjurySite.RightMiddleFinger or InjurySite.LeftMiddleFinger
            or InjurySite.RightRingFinger or InjurySite.LeftRingFinger or InjurySite.RightLittleFinger or InjurySite.LeftLittleFinger or InjurySite.RightBigToe or InjurySite.LeftBigToe
            or InjurySite.RightIndexToe or InjurySite.LeftIndexToe or InjurySite.RightMiddleToe or InjurySite.LeftMiddleToe
            or InjurySite.RightRingToe or InjurySite.LeftRingToe or InjurySite.RightLittleToe or InjurySite.LeftLittleToe => Mathf.Min(damage, 10),
            _ => damage
        };
        maxDamage = Mathf.Max(maxDamage, 0);
        if (!(damagedPartIsArtifical > 0 && noPain)) curHP -= maxDamage;
        attacker.totalDamage += maxDamage;
        if (curHP <= 0 && characteristics.FindIndex(x => x.type == CharacteristicType.Zombie) == -1)
        {
            curHP = 0;
            attacker.KillCount++;
            InGameUIManager.UpdateSelectedObjectKillCount(attacker);
            attacker.FindNewNearestFarmingTarget();
            IsDead = true;
            if (damagePart == InjurySiteMajor.Head && damageType == DamageType.GunShot)
            {
                GameObject headshot = PoolManager.Spawn(ResourceEnum.Prefab.Headshot, transform.position);
                headshot.transform.SetParent(canvas.transform);
            }
            if (attacker.playerSurvivor)
            {
                if (weapon == null || !IsValid(weapon))
                {
                    PlayerPrefs.SetInt($"Bare Knuckle Kill", PlayerPrefs.GetInt($"Bare Knuckle Kill") + 1);
                    AchievementManager.SetStat("Total_BareHandKill", PlayerPrefs.GetInt($"Bare Knuckle Kill"));
                    if (PlayerPrefs.GetInt($"Bare Knuckle Kill") >= 30) AchievementManager.UnlockAchievement("Bruce Lee");
                }
                else if (weapon is MeleeWeapon)
                {
                    PlayerPrefs.SetInt($"Melee Weapon Kill", PlayerPrefs.GetInt($"Melee Weapon Kill") + 1);
                    AchievementManager.SetStat("Total_MeleeKill", PlayerPrefs.GetInt($"Melee Weapon Kill"));
                    if (PlayerPrefs.GetInt($"Melee Weapon Kill") >= 30) AchievementManager.UnlockAchievement("Lethal Weapon");
                }
                else if (weapon is RangedWeapon)
                {
                    PlayerPrefs.SetInt($"Ranged Weapon Kill", PlayerPrefs.GetInt($"Ranged Weapon Kill") + 1);
                    AchievementManager.SetStat("Total_RangedKill", PlayerPrefs.GetInt($"Ranged Weapon Kill"));
                    if (PlayerPrefs.GetInt($"Ranged Weapon Kill") >= 30) AchievementManager.UnlockAchievement("Gunslinger");
                }
                if (IsValid(weapon))
                {
                    PlayerPrefs.SetInt($"{weapon.itemType} Kill", PlayerPrefs.GetInt($"{weapon.itemType} Kill") + 1);
                    int count = PlayerPrefs.GetInt($"{weapon.itemType} Kill");
                    if (weapon.itemType == ItemManager.Items.LongSword || weapon.itemType == ItemManager.Items.LongSword_Enchanted)
                    {
                        AchievementManager.SetStat("Total_SowrdKill", count);
                        if (count >= 10) AchievementManager.UnlockAchievement("Sword Master");
                    }
                    if (weapon.itemType == ItemManager.Items.SniperRifle)
                    {
                        AchievementManager.SetStat("Total_SniperKill", count);
                        if (count >= 10) AchievementManager.UnlockAchievement("Sniper");
                    }
                }
            }
            InGameUIManager.ShowKillLog(survivorName.GetLocalizedString(), attacker.survivorName.GetLocalizedString());
        }

        if (damagedPartIsArtifical > 0)
        {
            GetDamageArtificalPart(alreadyHaveInjury, damage, damagedPartIsArtifical);
        }
        else GetInjury(attacker, specificDamagePart, damageType, damage);
        if (weapon is MeleeWeapon meleeWeapon && meleeWeapon.IsEnchanted) Poisoning(attacker);
    }

    void ApplyDamage(Survivor attacker, float damage, Item weapon, InjurySiteMajor damagePart, DamageType damageType)
    {
        if (isDead) return;
        if (damage > 0)
        {
            InjurySite specificDamagePart = GetSpecificDamagePart(damagePart, damageType);
            if (specificDamagePart == InjurySite.Head)
            {
                if (currentHelmet != null && UnityEngine.Random.Range(0f, 1f) < currentHelmet.DurabilityPercent + 0.3f)
                {
                    if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
                        PlaySFX("ricochet,5", this);
                    }
                    else
                    {
                        PlaySFX("ricochet2,5", this);
                    }
                    if(damage > currentHelmet.Defense)
                    {
                        currentHelmet.CurDurability -= damage - currentHelmet.Defense + damage / 10;
                    }
                    else
                    {
                        currentHelmet.CurDurability -= damage / 10;
                    }
                    damage -= currentHelmet.Defense;
                    InGameUIManager.UpdateSelectedObjectInventory(this);
                }
            }
            ApplyDamage(attacker, damage, weapon, damagePart, specificDamagePart, damageType);
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
    void ApplyExplosionDamage(Survivor attacker, float damage, Item weapon, InjurySiteMajor injurySiteMajor, int side = 0)
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
                float randCheek = UnityEngine.Random.Range(0, 1f);
                float randNeck = UnityEngine.Random.Range(0, 1f);
                if (randLEye < 0.3f) count++;
                if (randREye < 0.3f) count++;
                if (randLEar < 0.5f) count++;
                if (randREar < 0.5f) count++;
                if (randNose < 0.7f) count++;
                if (randCheek < 0.7f) count++;
                if (randNeck < 0.3f) count++;
                float dividedDamage = damage / count;
                ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.Head, DamageType.Explosion);
                if(randLEye < 0.3f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.LeftEye, DamageType.Explosion);
                if(randREye < 0.3f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.RightEye, DamageType.Explosion);
                if(randREar < 0.5f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.LeftEar, DamageType.Explosion);
                if(randLEar < 0.5f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.RightEar, DamageType.Explosion);
                if(randNose < 0.7f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.Nose, DamageType.Explosion);
                if(randCheek < 0.7f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.Cheek, DamageType.Explosion);
                if(randNeck < 0.7f) ApplyDamage(attacker, dividedDamage, weapon, InjurySiteMajor.Head, InjurySite.Neck, DamageType.Explosion);
                break;
            case InjurySiteMajor.Torso:
                ApplyDamage(attacker, damage / 2, weapon, InjurySiteMajor.Torso, InjurySite.Chest, DamageType.Explosion);
                ApplyDamage(attacker, damage / 2, weapon, InjurySiteMajor.Torso, InjurySite.Abdomen, DamageType.Explosion);
                break;
            case InjurySiteMajor.Arms:
                ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Arms, DamageType.Explosion);
                break;
            case InjurySiteMajor.Legs:
                float rand = UnityEngine.Random.Range(0, 1f);
                if(rand > 0.3f)
                {
                    if(side == 1) ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, InjurySite.RightFoot, DamageType.Explosion);
                    else if(side == 2) ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, InjurySite.LeftFoot, DamageType.Explosion);
                    else ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, rand > 0.65f ? InjurySite.RightFoot : InjurySite.LeftFoot, DamageType.Explosion);
                }
                else
                {
                    if(side == 1) ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, InjurySite.RightKnee, DamageType.Explosion);
                    else if(side == 2) ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, InjurySite.LeftKnee, DamageType.Explosion);
                    else ApplyDamage(attacker, damage, weapon, InjurySiteMajor.Legs, rand > 0.15f ? InjurySite.RightKnee : InjurySite.LeftKnee, DamageType.Explosion);
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
            poisonOriginator.KillCount++;
            InGameUIManager.UpdateSelectedObjectKillCount(poisonOriginator);
            IsDead = true;
            if (poisonOriginator.playerSurvivor) AchievementManager.UnlockAchievement("Viper");
            InGameUIManager.ShowKillLog(survivorName.GetLocalizedString(), poisonOriginator.survivorName.GetLocalizedString());
        }
    }

    public void TakeDamage(Survivor attacker, float damage, Item weapon)
    {
        string hitSound;

        // 패링
        if(characteristics.FindIndex(x => x.type == CharacteristicType.SwordSaint) != -1 && IsValid(CurrentWeapon) && (CurrentWeapon.itemType == ItemManager.Items.LongSword || CurrentWeapon.itemType == ItemManager.Items.LongSword_Enchanted))
        {
            if(UnityEngine.Random.Range(0, 1f) < 0.5f)
            {
                animator.SetTrigger("Parrying");
                return;
            }
        }

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
            float coefficient = (linkedSurvivorData.Fighting / Mathf.Max(attacker.linkedSurvivorData.Fighting, 1)) * (moveSpeed / attacker.moveSpeed);
            float avoidRate = Mathf.Min(0.1f * coefficient, 0.3f);
            float defendRate = Mathf.Min(0.4f * coefficient, 0.7f);
            if (rightHandDisabled) defendRate -= defendRate * 0.5f;
            if (leftHandDisabled) defendRate -= defendRate * 0.5f;
            float criticalRate = 0.1f / coefficient * attacker.luck / luck;

            float chanceToIncreaseStat = UnityEngine.Random.Range(0, 1f);
            if (probability < avoidRate)
            {
                // 회피
                damage = 0;
                hitSound = "avoid, 1";

                if (chanceToIncreaseStat < 0.1f)
                {
                    linkedSurvivorData.IncreaseStats(0, 0, 1, 0, 0, 0);
                    increaseFighting++;
                }
            }
            else if (probability < avoidRate + defendRate)
            {
                // 방어
                damage *= 0.5f;
                damagePart = InjurySiteMajor.Arms;
                hitSound = "guard, 2";

				if (chanceToIncreaseStat < 0.02f)
                {
                    linkedSurvivorData.IncreaseStats(0, 0, 1, 0, 0, 0);
                    increaseFighting++;
                }
            }
            else if (probability > 1 - criticalRate || characteristics.FindIndex(x => x.type == CharacteristicType.KnifeFighter) != -1 && IsValid(CurrentWeapon) && CurrentWeapon.itemType == ItemManager.Items.Knife)
            {
                // 치명타
                damage *= 2;
                if (damageType == DamageType.Strike) damagePart = InjurySiteMajor.Head;
                else damagePart = InjurySiteMajor.Torso;
                hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,5" : "hit01,5";

				if (chanceToIncreaseStat < 0.1f)
                {
                    attacker.linkedSurvivorData.IncreaseStats(0, 0, 1, 0, 0, 0);
                    increaseFighting++;
                }
            }
            else
            {
                hitSound = currentWeapon is RangedWeapon && CurrentWeaponAsRangedWeapon.AttackAnimNumber == 2 ? "hit02,5" : "hit01,5";

				if (chanceToIncreaseStat < 0.02f)
                {
                    attacker.linkedSurvivorData.IncreaseStats(0, 0, 1, 0, 0, 0);
                    increaseFighting++;
                }
            }
        }

        if (damagePart == InjurySiteMajor.Torso && currentVest != null) damage -= currentVest.Defense;

        PlaySFX(hitSound, this);
        ApplyDamage(attacker, damage, weapon, damagePart, damageType);
    }

    public void TakeDamage(Bullet bullet)
    {
        if (isDead) return;
        float damage = bullet.Damage;
        // 실효 사거리 밖
        if (bullet.TraveledDistance > bullet.MaxRange * 0.5f)
        {
            damage *= (bullet.MaxRange * 1.5f - bullet.TraveledDistance) / bullet.MaxRange;
        }
        TakeGunshotDamamge(bullet.Launcher, damage);
    }

    public void TakeGunshotDamamge(Survivor launcher, float damage)
    {
        float probability = UnityEngine.Random.Range(0, 1f);
        InjurySiteMajor damagePart;
        float correctionProbability = Mathf.Pow(2, Mathf.Log(Mathf.Max(launcher.correctedShooting, 1), 20));
        float headShotProbability = 0.01f * launcher.luck / luck * correctionProbability;
        float bodyShotProbability = 1 - 0.01f * (128f / correctionProbability * luck / launcher.luck);
        // 헤드샷
        if (probability < headShotProbability)
        {
            damagePart = InjurySiteMajor.Head;
        }
        // 바디샷
        else if (probability < bodyShotProbability)
        {
            damagePart = InjurySiteMajor.Torso;
        }
        else
        {
            if (UnityEngine.Random.Range(0, 1f) < 0.5f) damagePart = InjurySiteMajor.Arms;
            else damagePart = InjurySiteMajor.Legs;
        }

        // 방탄조끼
        // 방탄 모자는 ApplyDamage에서 처리
        if (damagePart == InjurySiteMajor.Torso)
        {
            if (currentVest != null && UnityEngine.Random.Range(0f, 1f) < currentVest.DurabilityPercent + 0.3f)
            {
                if(damage > currentVest.Defense)
                {
                    currentVest.CurDurability -= damage - currentVest.Defense + damage / 10;
                }
                else
                {
                    currentVest.CurDurability -= damage / 10;
                }
                damage -= currentVest.Defense;
                InGameUIManager.UpdateSelectedObjectInventory(this);
            }
        }

        ApplyDamage(launcher, damage, launcher.CurrentWeapon, damagePart, DamageType.GunShot);
    }

    public void TakeDamage(Trap trap, InjurySite injurySite)
    {
        ApplyDamage(trap.setter, trap.Damage, trap.linkedItem, InjurySiteMajor.Legs, injurySite, trap.DamageType);
        if(IsDead && trap.setter.playerSurvivor)
        {
            PlayerPrefs.SetInt("Total Trap Kill", PlayerPrefs.GetInt("Total Trap Kill") + 1);
            //AchievementManager.SetStat("Total_TrapKill", PlayerPrefs.GetInt("Total Trap Kill"));
            //if (PlayerPrefs.GetInt("Total Trap Kill") >= 10) 
                AchievementManager.UnlockAchievement("Sun Tzu");
        }
    }

    public void TakeDamage(Trap trap, float damage, int side = 0)
    {
        if (trap.DamageType == DamageType.Explosion) ApplyExplosionDamage(trap.setter, damage, trap.linkedItem, InjurySiteMajor.Legs, side);
        else ApplyDamage(trap.setter, damage, trap.linkedItem, InjurySiteMajor.Legs, trap.DamageType);
    }

    public void TakeDamage(BoobyTrap boobyTrap, float damage)
    {
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.4f, boobyTrap, InjurySiteMajor.Head);
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.4f, boobyTrap, InjurySiteMajor.Torso);
        ApplyExplosionDamage(boobyTrap.Setter, damage * 0.2f, boobyTrap, InjurySiteMajor.Arms);
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
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Head);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
                    break;
                case 1:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.2f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Head);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.5f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.2f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Arms);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Legs);
                    break;
                case 2:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Arms);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
                    break;
                case 3:
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.75f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Legs);
                    ApplyExplosionDamage(rocket.Launcher, damage * 0.25f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
                    break;
            }
        }
        else
        {
            site = UnityEngine.Random.Range(0, 2);
            if(site == 0)
            {
                ApplyExplosionDamage(rocket.Launcher, damage * 0.9f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Arms);
                ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
            }
            else
            {
                ApplyExplosionDamage(rocket.Launcher, damage * 0.9f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Arms);
                ApplyExplosionDamage(rocket.Launcher, damage * 0.1f, rocket.Launcher.CurrentWeapon, InjurySiteMajor.Torso);
            }
        }
    }

    public void Poisoning(Survivor poisonOriginator)
    {
        if (characteristics.FindIndex(x => x.type == CharacteristicType.PoisonImmune) != -1) return;
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
                    if (characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) rand = UnityEngine.Random.Range(0, 4f);
                    else if (characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) rand = UnityEngine.Random.Range(0, 1f);
                    else rand = UnityEngine.Random.Range(0, 2f);
                }
                else rand = UnityEngine.Random.Range(0, 1f);

                if (rand < 1f)
                {
                    if(rand > 0.75f) injurySite = InjurySite.Head;
                    else if (rand > 0.4f) injurySite = InjurySite.Cheek;
                    else if (rand > 0.35f) injurySite = InjurySite.Neck;
                    else if (rand > 0.3f) injurySite = InjurySite.LeftEye;
                    else if (rand > 0.25f) injurySite = InjurySite.RightEye;
                    else if (rand > 0.2f) injurySite = InjurySite.RightEar;
                    else if (rand > 0.15f) injurySite = InjurySite.LeftEar;
                    else if (rand > 0.1f) injurySite = InjurySite.Nose;
                    else if (rand < 0.1f) injurySite = InjurySite.Jaw;
                }
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
                    else if (rand > 0.2f) injurySite = InjurySite.RightFoot;
                    else if (rand > 0.1f) injurySite = InjurySite.LeftFoot;
                    else if (rand > 0.09f) injurySite = InjurySite.RightBigToe;
                    else if (rand > 0.08f) injurySite = InjurySite.LeftBigToe;
                    else if (rand > 0.07f) injurySite = InjurySite.RightIndexToe;
                    else if (rand > 0.06f) injurySite = InjurySite.LeftIndexToe;
                    else if (rand > 0.05f) injurySite = InjurySite.RightMiddleToe;
                    else if (rand > 0.04f) injurySite = InjurySite.LeftMiddleToe;
                    else if (rand > 0.03f) injurySite = InjurySite.RightRingToe;
                    else if (rand > 0.02f) injurySite = InjurySite.LeftRingToe;
                    else if (rand > 0.01f) injurySite = InjurySite.RightLittleToe;
                    else injurySite = InjurySite.LeftLittleToe;
                }
                else if(damageType == DamageType.Slash)
                {
                    if (rand > 0.5f) injurySite = InjurySite.RightFoot;
                    else injurySite = InjurySite.LeftFoot;
                }
                break;
        }

        return injurySite;
    }

    void GetInjury(Survivor attacker, InjurySite injurySite, DamageType damageType, float damage)
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
                }
                break;
            case InjurySite.Cheek:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if (damageType == DamageType.Strike)
                {
                    injuryType = injuryDegree > 0.3f ? InjuryType.Fracture : InjuryType.Contusion;
                }
                else if (damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                }
                else if (damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
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
                else if (damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                else if (damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                    injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                }
                break;
            case InjurySite.Neck:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if (damageType == DamageType.Strike)
                {
                    injuryType = injuryDegree > 0.3f ? InjuryType.Fracture : InjuryType.Contusion;
                }
                else if (damageType == DamageType.Slash)
                {
                    injuryType = InjuryType.Cutting;
                }
                else if (damageType == DamageType.GunShot)
                {
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Explosion)
                {
                    injuryType = InjuryType.Burn;
                }
                break;
            case InjurySite.Chest:
                injuryDegree = Mathf.Clamp(damage / 100, 0, 0.99f);
                if (damageType == DamageType.Strike)
                {
                    if (injuryDegree > 0.3f)
                    {
                        injurySite = InjurySite.Ribs;
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
                    injuryDegree = Mathf.Clamp(damage / 80, 0, 0.99f);
                    if (injuryDegree > 0.3f) injuryType = InjuryType.Fracture;
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Slash)
                {
                    injuryDegree = Mathf.Clamp(damage / 80, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else if (damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 80, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if(damageType == DamageType.Explosion)
                {
                    injuryDegree = Mathf.Clamp(damage / 80, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Loss;
                }
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
                if (damageType == DamageType.Strike)
                {
                    injuryDegree = Mathf.Clamp(damage / 30, 0, 0.99f);
                    if (injuryDegree > 0.3f) injuryType = InjuryType.Fracture;
                    else injuryType = InjuryType.Contusion;
                }
                else if (damageType == DamageType.Slash)
                {
                    injuryDegree = Mathf.Clamp(damage / 30, 0, 1f);
                    if (injuryDegree >= 1f) injuryType = InjuryType.Amputation;
                    else injuryType = InjuryType.Cutting;
                }
                else if(damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 30, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Explosion)
                {
                    injuryDegree = Mathf.Clamp(damage / 30, 0, 1f);
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
                injuryDegree = Mathf.Clamp(damage / 10, 0, 1f);
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
                if (damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 80, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Slash)
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
                else if (damageType == DamageType.Explosion)
                {
                    injuryDegree = 1;
                    injuryType = InjuryType.Loss;
                }
                break;
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
                if (damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 50, 0, 0.99f);
                    injuryType = InjuryType.GunshotWound;
                }
                else if (damageType == DamageType.Slash)
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
                else if (damageType == DamageType.Explosion)
                {
                    injuryDegree = 1;
                    injuryType = InjuryType.Loss;
                }
                break;
            case InjurySite.RightFoot:
            case InjurySite.LeftFoot:
                if(damageType == DamageType.GunShot)
                {
                    injuryDegree = Mathf.Clamp(damage / 30, 0, 0.99f);
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
            case InjurySite.RightIndexToe:
            case InjurySite.LeftIndexToe:
            case InjurySite.RightMiddleToe:
            case InjurySite.LeftMiddleToe:
            case InjurySite.RightRingToe:
            case InjurySite.LeftRingToe:
            case InjurySite.RightLittleToe:
            case InjurySite.LeftLittleToe:
                injuryDegree = Mathf.Clamp(damage / 10, 0, 1f);
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
            AddBleeding(attacker, injurySite, injuryType, injuryDegree);
        }
    }

    void GetDamageArtificalPart(Injury artificalPart, float damage, int artificialType)
    {
        float degree;
        if (artificialType == 2) damage /= 2;
        else if (artificialType == 3) damage /= 4;
        switch (artificalPart.site)
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
            case InjurySite.RightIndexToe:
            case InjurySite.LeftIndexToe:
            case InjurySite.RightMiddleToe:
            case InjurySite.LeftMiddleToe:
            case InjurySite.RightRingToe:
            case InjurySite.LeftRingToe:
            case InjurySite.RightLittleToe:
            case InjurySite.LeftLittleToe:
                degree = Mathf.Clamp(damage / 40, 0, 1f);
                break;
            default:
                degree = Mathf.Clamp(damage / 100, 0, 1f);
                break;
        }
        if(artificialType == 1) AddInjury(artificalPart.site, InjuryType.ArtificialPartsDamaged, degree);
        else if(artificialType == 2) AddInjury(artificalPart.site, InjuryType.AugmentedPartsDamaged, degree);
        else if(artificialType == 3) AddInjury(artificalPart.site, InjuryType.TranscendantPartsDamaged, degree);
    }

    void AddInjury(InjurySite injurySite, InjuryType injuryType, float injuryDegree)
    {
        if (injuryDegree == 0) return;
        if(injuryType == InjuryType.Loss || injuryType == InjuryType.Amputation || injuryType == InjuryType.Rupture || injuryType == InjuryType.ArtificialPartsDamaged && injuryDegree >= 1)
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
            List<InjurySite> subparts = Injury.GetSubparts(injurySite);
            List<Injury> toRemove = new();
            foreach(var injury in injuries)
            {
                if(subparts.Contains(injury.site)) toRemove.Add(injury);
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
                if (injuries[index].type == InjuryType.ArtificialPartsTransplanted) injuries[index].type = InjuryType.ArtificialPartsDamaged;
                else if (injuries[index].type == InjuryType.AugmentedPartsTransplanted) injuries[index].type = InjuryType.AugmentedPartsDamaged;
                else if (injuries[index].type == InjuryType.TranscendantPartsTransplanted) injuries[index].type = InjuryType.TranscendantPartsDamaged;
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
                        case InjurySite.Ribs:
                        case InjurySite.Abdomen:
                        case InjurySite.Cheek:
                        case InjurySite.Neck:
                        case InjurySite.Organ:
                            injuries[index].degree = 0.99f;
                            break;
                        // Losable
                        default:
                            if (injuryType == InjuryType.ArtificialPartsDamaged) AddInjury(injurySite, InjuryType.ArtificialPartsDamaged, 1);
                            else if (injuryType == InjuryType.AugmentedPartsDamaged) AddInjury(injurySite, InjuryType.AugmentedPartsDamaged, 1);
                            else if (injuryType == InjuryType.TranscendantPartsDamaged) AddInjury(injurySite, InjuryType.TranscendantPartsDamaged, 1);
                            else AddInjury(injurySite, InjuryType.Rupture, 1);
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

    void AddBleeding(Survivor attacker, InjurySite injurySite, InjuryType injuryType, float injuryDegree)
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
            case InjurySite.RightIndexToe:
            case InjurySite.LeftIndexToe:
            case InjurySite.RightMiddleToe:
            case InjurySite.LeftMiddleToe:
            case InjurySite.RightRingToe:
            case InjurySite.LeftRingToe:
            case InjurySite.RightLittleToe:
            case InjurySite.LeftLittleToe:
                if (injuryDegree >= 1) curBlood -= 10;
                amount = 20 * injuryDegree;
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
                if (injuryDegree >= 1) curBlood -= 100;
                amount = 100 * injuryDegree;
                break;
            case InjurySite.RightFoot:
            case InjurySite.LeftFoot:
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
            case InjurySite.Cheek:
                amount = 100 * injuryDegree;
                break;
            case InjurySite.Neck:
                amount = 500 * injuryDegree;
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
        else if (injuryType == InjuryType.Burn || injuryType == InjuryType.Fracture) amount *= 0.3f;
        survivorWhoCausedBleeding = attacker;
        rememberWhoCausedBleedingTime = 10f;
        BleedingAmount += amount;
    }

    int HowManyWalkingAidNeed()
    {
        bool right = false;
        bool left = false;
        foreach(var injury in injuries)
        {
            if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged && injury.degree < 1) continue;
            if (injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged && injury.degree < 1) continue;
            if (injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged && injury.degree < 1) continue;
            switch(injury.site)
            {
                case InjurySite.RightLeg:
                case InjurySite.RightKnee:
                case InjurySite.RightFoot:
                    right = true;
                    break;
                case InjurySite.LeftLeg:
                case InjurySite.LeftKnee:
                case InjurySite.LeftFoot:
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
        List<InjurySite> upperParts = Injury.GetUpperParts(injurySite);
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
    int isLeftEyeArtificial = 0;
    bool isLeftEyePermanentVisualImpairment;
    int isRightEyeArtificial = 0;
    bool isRightEyePermanentVisualImpairment;
    float injuryCorrection_LeftSightRange = 1;
    float injuryCorrection_RightSightRange = 1;
    int injuryCorrection_Crafting = 0;
    float injuryCorrection_CraftingSpeed = 1;
    float injuryCorrection_WearingSpeed = 1;

    bool augmentedRightArm;
    bool transcendantRightArm;
    bool augmentedLeftArm;
    bool transcendantLeftArm;
    bool augmentedRightLeg;
    bool transcendantRightLeg;
    bool augmentedLeftLeg;
    bool transcendantLeftLeg;
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
        float penaltiedMoveSpeedByRightToe = 1;
        float penaltiedMoveSpeedByLeftLeg = 1;
        float penaltiedMoveSpeedByLeftToe = 1;
        float penaltiedCraftingSpeedByRightArm = 1;
        float penaltiedCraftingSpeedByLeftArm = 1;
        float penaltiedWearingSpeedByLeftArm = 1;
        float penaltiedWearingSpeedByRightArm = 1;

        foreach (Injury injury in injuries)
        {
            if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged && injury.degree < 1) continue;
            if (injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged && injury.degree < 1) continue;
            if (injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged && injury.degree < 1) continue;
            switch(injury.site)
            {
                case InjurySite.RightEar:
                    ear1Penalty = injury.degree;
                    break;
                case InjurySite.LeftEar:
                    ear2Penalty = injury.degree;
                    break;
                case InjurySite.RightEye:
                    if ((injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged) && injury.degree < 1) isRightEyeArtificial = 1;
                    else if ((injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged) && injury.degree < 1) isRightEyeArtificial = 2;
                    else if ((injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged) && injury.degree < 1) isRightEyeArtificial = 3;
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
                    if ((injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged) && injury.degree < 1) isLeftEyeArtificial = 1;
                    else if ((injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged) && injury.degree < 1) isLeftEyeArtificial = 2;
                    else if ((injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged) && injury.degree < 1) isLeftEyeArtificial = 3;
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
                    if ((augmentedRightArm || transcendantRightArm) && injury.degree < 1)
                    {
                        if (augmentedRightArm) penaltiedAttackDamageByRightArm *= 1.1f;
                        else if (transcendantRightArm) penaltiedAttackDamageByRightArm *= 1.2f;
                        break;
                    }
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightHand:
                    if ((augmentedRightArm || transcendantRightArm) && injury.degree < 1) break;
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.5f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree);
                    if (injury.degree >= 1) RightHandDisabled = true;
                    break;
                case InjurySite.RightThumb:
                case InjurySite.RightIndexFinger:
                case InjurySite.RightMiddleFinger:
                case InjurySite.RightRingFinger:
                case InjurySite.RightLittleFinger:
                    if ((augmentedRightArm || transcendantRightArm) && injury.degree < 1) break;
                    penaltiedAttackDamageByRightArm *= (1 - injury.degree * 0.1f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    break;
                case InjurySite.LeftArm:
                    if ((augmentedLeftArm || transcendantLeftArm) && injury.degree < 1)
                    {
                        if (augmentedLeftArm) penaltiedAttackDamageByLeftArm *= 1.1f;
                        else if (transcendantLeftArm) penaltiedAttackDamageByLeftArm *= 1.2f;
                        break;
                    }
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree * 0.5f);
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftHand:
                    if ((augmentedLeftArm || transcendantLeftArm) && injury.degree < 1) break;
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.5f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree);
                    if (injury.degree >= 1) LeftHandDisabled = true;
                    break;
                case InjurySite.LeftThumb:
                case InjurySite.LeftIndexFinger:
                case InjurySite.LeftMiddleFinger:
                case InjurySite.LeftRingFinger:
                case InjurySite.LeftLittleFinger:
                    if ((augmentedLeftArm || transcendantLeftArm) && injury.degree < 1) break;
                    penaltiedAttackDamageByLeftArm *= (1 - injury.degree * 0.1f);
                    penaltiedCraftingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    penaltiedWearingSpeedByRightArm *= (1 - injury.degree * 0.3f);
                    break;
                case InjurySite.RightLeg:
                    if ((augmentedRightLeg || transcendantRightLeg) && injury.degree < 1)
                    {
                        if (augmentedRightLeg) penaltiedMoveSpeedByRightLeg *= 1.1f;
                        else if (transcendantRightLeg) penaltiedMoveSpeedByRightLeg *= 1.2f;
                        break;
                    }
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightKnee:
                    if ((augmentedRightLeg || transcendantRightLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightFoot:
                    if ((augmentedRightLeg || transcendantRightLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.RightBigToe:
                case InjurySite.RightIndexToe:
                case InjurySite.RightMiddleToe:
                case InjurySite.RightRingToe:
                case InjurySite.RightLittleToe:
                    if ((augmentedRightLeg || transcendantRightLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByRightToe *= (1 - injury.degree * 0.1f);
                    penaltiedMoveSpeedByRightLeg = Mathf.Min(penaltiedMoveSpeedByRightLeg, penaltiedMoveSpeedByRightToe);
                    break;
                case InjurySite.LeftLeg:
                    if ((augmentedLeftLeg || transcendantLeftLeg) && injury.degree < 1)
                    {
                        if (augmentedRightLeg) penaltiedMoveSpeedByLeftLeg *= 1.1f;
                        else if (transcendantRightLeg) penaltiedMoveSpeedByLeftLeg *= 1.2f;
                        break;
                    }
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.1f));
                    break;
                case InjurySite.LeftKnee:
                    if ((augmentedLeftLeg || transcendantLeftLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.LeftFoot:
                    if ((augmentedLeftLeg || transcendantLeftLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, (1 - injury.degree * 0.5f));
                    break;
                case InjurySite.LeftBigToe:
                case InjurySite.LeftIndexToe:
                case InjurySite.LeftMiddleToe:
                case InjurySite.LeftRingToe:
                case InjurySite.LeftLittleToe:
                    if ((augmentedLeftLeg || transcendantLeftLeg) && injury.degree < 1) break;
                    penaltiedMoveSpeedByLeftToe *= (1 - injury.degree * 0.1f);
                    penaltiedMoveSpeedByLeftLeg = Mathf.Min(penaltiedMoveSpeedByLeftLeg, penaltiedMoveSpeedByLeftToe);
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
        if (RightHandDisabled)
        {
            injuryCorrection_Crafting -= (int)((1 - penaltiedCraftingSpeedByLeftArm) * 20) + 20;
            injuryCorrection_CraftingSpeed = 0.5f * penaltiedCraftingSpeedByLeftArm;
            injuryCorrection_WearingSpeed = 0.5f * penaltiedWearingSpeedByLeftArm;
        }
        else if (LeftHandDisabled)
        {
            injuryCorrection_Crafting -= (int)((1 - penaltiedCraftingSpeedByRightArm) * 20) + 20;
            injuryCorrection_CraftingSpeed = 0.5f * penaltiedCraftingSpeedByRightArm;
            injuryCorrection_WearingSpeed = 0.5f * penaltiedWearingSpeedByRightArm;
        }
        else
        {
            injuryCorrection_Crafting -= (int)((1 - penaltiedCraftingSpeedByRightArm * penaltiedCraftingSpeedByLeftArm) * 40);
            injuryCorrection_CraftingSpeed = penaltiedCraftingSpeedByRightArm * penaltiedCraftingSpeedByLeftArm;
            injuryCorrection_WearingSpeed = penaltiedWearingSpeedByRightArm * penaltiedWearingSpeedByLeftArm;
        }
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
    //int characteristicCorrection_Crafting = 0;
    int characteristicCorrection_Crafting = 0;
    float characteristicCorrection_AimTime = 0;
    int characteristicCorrection_AimErrorRange = 0;
    float characteristicCorrection_ReloadSpeed = 1;
    float characteristicCorrection_NatualHemostasis = 1;
    float characteristicCorrection_StopBleeding = 1;
    float characteristicCorrection_BloodRegeneration = 1;
    float characteristicCorrection_HpRegeneration = 1;
    float characteristicCorrection_TrappingSpeed = 1;

    int correctedStrength = 0;
    int correctedAgility = 0;
    int correctedFighting = 0;
    int correctedShooting = 0;
    int correctedCrafting = 0;
    public int CorrectedStrength => correctedStrength;
    public int CorrectedAgility => correctedAgility;
    public int CorrectedFighting => correctedFighting;
    public int CorrectedShooting => correctedShooting;
    public int CorrectedCrafting => correctedCrafting;

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
                case CharacteristicType.ChokingUnderPressure:
                    var todayLeague = GameManager.Instance.Calendar.LeagueReserveInfo[GameManager.Instance.Calendar.Today].league;
                    if (todayLeague == League.SeasonChampionship || todayLeague == League.WorldChampionship)
                    {
                        characteristicCorrection_Strength -= 10;
                        characteristicCorrection_Agility -= 10;
                        characteristicCorrection_Fighting -= 10;
                        characteristicCorrection_Shooting -= 10;
                        characteristicCorrection_Crafting -= 10;
                    }
                    break;
                case CharacteristicType.ClutchPerformance:
                    todayLeague = GameManager.Instance.Calendar.LeagueReserveInfo[GameManager.Instance.Calendar.Today].league;
                    if (todayLeague == League.SeasonChampionship || todayLeague == League.WorldChampionship)
                    {
                        characteristicCorrection_Strength += 20;
                        characteristicCorrection_Agility += 20;
                        characteristicCorrection_Fighting += 20;
                        characteristicCorrection_Shooting += 20;
                        characteristicCorrection_Crafting += 20;
                    }
                    break;
                case CharacteristicType.Giant:
                    transform.localScale = new(1.3f, 1.3f);
                    foreach(Transform child in rightHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in leftHand.transform) child.localScale = new(0.77f, 0.77f);
                    foreach(Transform child in transform.Find("Head")) child.localScale = new(0.77f, 0.77f);
                    break;
                case CharacteristicType.Dwarf:
                    transform.localScale = new(0.5f, 0.5f);
                    foreach (Transform child in rightHand.transform) child.localScale = new(2f, 2f);
                    foreach (Transform child in leftHand.transform) child.localScale = new(2f, 2f);
                    foreach (Transform child in transform.Find("Head")) child.localScale = new(2f, 2f);
                    break;
                case CharacteristicType.BigMan:
                    transform.localScale = new(1.15f, 1.15f);
                    foreach (Transform child in rightHand.transform) child.localScale = new(0.87f, 0.87f);
                    foreach (Transform child in leftHand.transform) child.localScale = new(0.87f, 0.87f);
                    foreach (Transform child in transform.Find("Head")) child.localScale = new(0.87f, 0.87f);
                    break;
                case CharacteristicType.CarefulShooter:
                    characteristicCorrection_AimTime *= 2;
                    characteristicCorrection_AimErrorRange += 20;
                    break;
                case CharacteristicType.QuickDrawer:
                    characteristicCorrection_AimTime *= 0.5f;
                    characteristicCorrection_ReloadSpeed *= 2;
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
                case CharacteristicType.Vampire:
                    characteristicCorrection_NatualHemostasis *= 100f;
                    break;
                case CharacteristicType.Regenerator:
                    characteristicCorrection_NatualHemostasis *= 5f;
                    characteristicCorrection_BloodRegeneration *= 5f;
                    characteristicCorrection_HpRegeneration *= 5f;
                    break;
                case CharacteristicType.UpsAndDowns:
                    int condition = UnityEngine.Random.Range(-20, 21);
                    characteristicCorrection_Strength += condition;
                    characteristicCorrection_Agility += condition;
                    characteristicCorrection_Fighting += condition;
                    characteristicCorrection_Shooting += condition;
                    characteristicCorrection_Crafting += condition;
                    break;
                case CharacteristicType.DiceMan:
                    condition = UnityEngine.Random.Range(-10, 21);
                    characteristicCorrection_Strength += condition;
                    characteristicCorrection_Agility += condition;
                    characteristicCorrection_Fighting += condition;
                    characteristicCorrection_Shooting += condition;
                    characteristicCorrection_Crafting += condition;
                    break;
                case CharacteristicType.Assassin:
                    isAssassin = true;
                    break;
                case CharacteristicType.FieldMedic:
                    characteristicCorrection_StopBleeding *= 1.3f;
                    break;
                case CharacteristicType.TrapExpert:
                    characteristicCorrection_TrappingSpeed *= 1.3f;
                    break;
                case CharacteristicType.StreetFighter:
                    itemCorrectionFighting = 20;
                    break;
                case CharacteristicType.BodyEnhancementAdvocate:
                    condition = 0;
                    condition += injuries.FindAll(x => x.type == InjuryType.AugmentedPartsTransplanted || x.type == InjuryType.AugmentedPartsDamaged).Count * 1;
                    condition += injuries.FindAll(x => x.type == InjuryType.TranscendantPartsTransplanted || x.type == InjuryType.TranscendantPartsDamaged).Count * 3;
                    characteristicCorrection_Strength += condition;
                    characteristicCorrection_Agility += condition;
                    characteristicCorrection_Fighting += condition;
                    characteristicCorrection_Shooting += condition;
                    characteristicCorrection_Crafting += condition;
                    break;
                case CharacteristicType.AugmentationFanatic:
                    condition = 0;
                    condition += injuries.FindAll(x => x.type == InjuryType.AugmentedPartsTransplanted || x.type == InjuryType.AugmentedPartsDamaged).Count * 2;
                    condition += injuries.FindAll(x => x.type == InjuryType.TranscendantPartsTransplanted || x.type == InjuryType.TranscendantPartsDamaged).Count * 4;
                    characteristicCorrection_Strength += condition;
                    characteristicCorrection_Agility += condition;
                    characteristicCorrection_Fighting += condition;
                    characteristicCorrection_Shooting += condition;
                    characteristicCorrection_Crafting += condition;
                    break;
                case CharacteristicType.WideVision:
                    sightAngle = 120;
                    break;
                case CharacteristicType.NarrowVision:
                    sightAngle = 60;
                    break;
                case CharacteristicType.Flounder:
                    sightAngle = 180;
                    break;
                default:
                    break;
            }
        }
        ApplyCorrectionStats();
    }
    #endregion

    int itemCorrectionFighting;
    int itemCorrectionShooting;
    void ApplyCorrectionStatByItem(Item item, bool equip)
    {
        if ((item.itemType == ItemManager.Items.Bow || item.itemType == ItemManager.Items.AdvancedBow) && characteristics.FindIndex(x => x.type == CharacteristicType.MasterArcher) != -1) itemCorrectionShooting = equip ? 20 : 0;
        if (item is MeleeWeapon && characteristics.FindIndex(x => x.type == CharacteristicType.LethalWeapon) != -1) itemCorrectionFighting = equip ? 20 : 0;
        if (characteristics.FindIndex(x => x.type == CharacteristicType.StreetFighter) != -1) itemCorrectionFighting = equip ? 0 : 20;
        ApplyCorrectionStats();
    }

    void ApplyCorrectionStats()
    {
        if (isLeftEyeArtificial > 2) leftSightRange = 60 * injuryCorrection_LeftSightRange;
        else if (isLeftEyeArtificial == 2) leftSightRange = 45 * injuryCorrection_LeftSightRange;
        else if (isLeftEyeArtificial == 1) leftSightRange = 35 * injuryCorrection_LeftSightRange;
        else if(isLeftEyePermanentVisualImpairment) leftSightRange = 15 * injuryCorrection_LeftSightRange * characteristicCorrection_SightRange;
        else leftSightRange = 45 * characteristicCorrection_SightRange * injuryCorrection_LeftSightRange;
        if (isRightEyeArtificial > 2) rightSightRange = 60 * injuryCorrection_RightSightRange;
        else if (isRightEyeArtificial == 2) rightSightRange = 45 * injuryCorrection_RightSightRange;
        else if (isRightEyeArtificial == 1) rightSightRange = 35 * injuryCorrection_RightSightRange;
        else if (isRightEyePermanentVisualImpairment) rightSightRange = 15 * injuryCorrection_RightSightRange * characteristicCorrection_SightRange;
        else rightSightRange = 45 * characteristicCorrection_SightRange * injuryCorrection_RightSightRange;
        hearingAbility = 10 * injuryCorrection_HearingAbility * characteristicCorrection_HearingAbility;

        int fury = curFuryTime > 0 ? 20 : 0;

        correctedStrength = Mathf.Max(linkedSurvivorData.Strength + characteristicCorrection_Strength, 0);
        correctedAgility = Mathf.Max(linkedSurvivorData.Agility + characteristicCorrection_Agility + fury, 0);
        correctedFighting = Mathf.Max(linkedSurvivorData.Fighting + characteristicCorrection_Fighting + itemCorrectionFighting, 0);
        correctedShooting = Mathf.Max(linkedSurvivorData.Shooting + characteristicCorrection_Shooting + itemCorrectionShooting, 0);
        correctedCrafting = Mathf.Max(linkedSurvivorData.Crafting + characteristicCorrection_Crafting, 0);
        aimDelay = 1.5f * characteristicCorrection_AimTime;
        aimErrorRange = 20f / Mathf.Pow(2, (correctedShooting + characteristicCorrection_AimErrorRange) / 20f);
        reloadSpeed = characteristicCorrection_ReloadSpeed;
        naturalHemostasis = characteristicCorrection_NatualHemostasis;
        bloodRegeneration = characteristicCorrection_BloodRegeneration;
        hpRegeneration = characteristicCorrection_HpRegeneration;
        stopBleedingSpeed = characteristicCorrection_StopBleeding;
        trappingSpeed = characteristicCorrection_TrappingSpeed;

        attackDamage = 5 * Mathf.Max(0.2f, (correctedStrength + correctedFighting) / 40f) * injuryCorrection_AttackDamage;
        attackSpeed = Mathf.Max((120f + correctedAgility + correctedFighting) / 160f * injuryCorrection_AttackSpeed, 0.1f);
        moveSpeed = Mathf.Max((60f + correctedAgility) * 3f / 80f * injuryCorrection_MoveSpeed, 0.1f);
        agent.speed = moveSpeed;
        farmingSpeed = Mathf.Max((60f + correctedAgility) / 80f * injuryCorrection_FarmingSpeed, 0.1f);
        crafting = Mathf.Max(correctedCrafting + injuryCorrection_Crafting, 0);
        craftingSpeed = Mathf.Max((1 + 0.01f * correctedCrafting) * injuryCorrection_CraftingSpeed, 0.1f);
        wearingSpeed = Mathf.Max(injuryCorrection_WearingSpeed, 0.1f);
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
                TargetEnemy.TakeDamage(this, damage, currentWeapon);
            }
            else
            {
                PlaySFX("avoid, 1", this);
                if (!TargetEnemy.inSightEnemies.Contains(this)) TargetEnemy.inSightEnemies.Add(this);
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, TargetEnemy.transform.position) < attackRange)
            {
                TargetEnemy.TakeDamage(this, attackDamage, null);
			}
            else
            {
                PlaySFX("avoid, 1", this);
                if (!TargetEnemy.inSightEnemies.Contains(this)) TargetEnemy.inSightEnemies.Add(this);
            }
		}
    }

    void AE_Reload()
    {
        int amount;
        if (currentWeapon.itemType == ItemManager.Items.ShotGun) amount = 1;
        else amount = Math.Max(1, CurrentWeaponAsRangedWeapon.MagazineCapacity - CurrentWeaponAsRangedWeapon.CurrentMagazine);
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
                float recoveryRate = curDrinking.quality switch
                {
                    CraftingQuality.Masterpiece => 70,
                    CraftingQuality.Excellent => 60,
                    CraftingQuality.Common => 40,
                    CraftingQuality.Poor => 30,
                    _ => 50
                };
                curHP = Mathf.Min(curHP + recoveryRate, maxHP);
                break;
            case ItemManager.Items.AdvancedPotion:
                recoveryRate = curDrinking.quality switch
                {
                    CraftingQuality.Masterpiece => 150,
                    CraftingQuality.Excellent => 125,
                    CraftingQuality.Common => 85,
                    CraftingQuality.Poor => 70,
                    _ => 100
                };
                curHP = Mathf.Min(curHP + recoveryRate, maxHP);
                break;
            default:
                Debug.LogWarning($"Try to drink wrong item : {curDrinking.itemType}.");
                break;
        }
        ConsumptionItem(curDrinking, 1);
        curDrinking = null;
    }
    #endregion

    #region SightIn SightOut
    public void SightIn(Survivor survivor, bool corpse, Vector2 collisionHitPoint)
    {
        if(!corpse)
        {
            if (!inSightEnemies.Contains(survivor))
            {
                threateningSoundPosition = Vector2.zero;
                keepEyesOnPosition = Vector2.zero;
                inSightEnemies.Add(survivor);
                sightMeshRenderer.material = m_SightAlert;
                if(projectileGenerator != null) projectileGenerator.collisionHitPoint = collisionHitPoint;
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
        if (survivor == TargetEnemy)
        {
            if (survivor != isDead)
            {
                targetEnemiesLastPosition = survivor.transform.position;
                lastTargetEnemy = survivor;
            }
            else if(!farmingCorpses.ContainsKey(survivor)) farmingCorpses.Add(survivor, false);
        }
        if(inSightEnemies.Contains(survivor)) inSightEnemies.Remove(survivor);
    }

    float curSeeEnemy = 0;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart || isDead) return;
        if (collision.TryGetComponent(out Survivor survivor) && (!collision.isTrigger || survivor.IsDead))
        {
            curSeeEnemy += Time.fixedDeltaTime;
            if (curSeeEnemy > 0.1f)
            {
                curSeeEnemy = 0;
                SightIn(survivor, survivor.IsDead, collision.ClosestPoint(transform.position));
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
        Area result = GameManager.Instance.BattleRoyaleManager.GetArea(transform.position, lastCurrentArea);
        if (!isDead)
        {
            temporaryAllowProhibitArea = true;
            if (result.IsProhibited) agent.areaMask |= 1 << NavMesh.GetAreaFromName("Prohibited");
            else if (result.IsProhibited_Plan) agent.areaMask |= 1 << NavMesh.GetAreaFromName("Prohibit_planned");
            else temporaryAllowProhibitArea = false;
        }
        lastCurrentArea = result;
        return result;
    }

    public void SetSurvivorInfo(SurvivorData survivorInfo)
    {
        linkedSurvivorData = survivorInfo;
        survivorName = new LocalizedString("Name", survivorInfo.SurvivorName);
        nameTag.GetComponent<LocalizeStringEvent>().StringReference = survivorName;
        curHP = maxHP = survivorInfo.Strength + 100;
        curBlood = maxBlood = maxHP * 80;
        bleedingSprite = curBlood - 100;
        luck = linkedSurvivorData.Luck;
        characteristics = survivorInfo.characteristics;

        injuries = survivorInfo.injuries;
        // game result 단계에서 돈을 걷지 않기 위해
        foreach (Injury injury in injuries)
        {
            if (injury.degree == 1 || injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged
                || injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged
                || injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged)
            {
                if (rememberAlreadyHaveInjury.Contains(injury.site)) continue;
                rememberAlreadyHaveInjury.Add(injury.site);
                foreach (var subpart in Injury.GetSubparts(injury.site)) 
                    if (!rememberAlreadyHaveInjury.Contains(subpart)) rememberAlreadyHaveInjury.Add(subpart);
            }
        }
        Injury tempInjury;
        tempInjury = injuries.Find(x => x.site == InjurySite.RightArm);
        augmentedRightArm = tempInjury != null && (tempInjury.type == InjuryType.AugmentedPartsTransplanted || tempInjury.type == InjuryType.AugmentedPartsDamaged);
        transcendantRightArm = tempInjury != null && (tempInjury.type == InjuryType.TranscendantPartsTransplanted || tempInjury.type == InjuryType.TranscendantPartsDamaged);
        tempInjury = injuries.Find(x => x.site == InjurySite.LeftArm);
        augmentedLeftArm = tempInjury != null && (tempInjury.type == InjuryType.AugmentedPartsTransplanted || tempInjury.type == InjuryType.AugmentedPartsDamaged);
        transcendantLeftArm = tempInjury != null && (tempInjury.type == InjuryType.TranscendantPartsTransplanted || tempInjury.type == InjuryType.TranscendantPartsDamaged);
        tempInjury = injuries.Find(x => x.site == InjurySite.RightLeg);
        augmentedRightLeg = tempInjury != null && (tempInjury.type == InjuryType.AugmentedPartsTransplanted || tempInjury.type == InjuryType.AugmentedPartsDamaged);
        transcendantRightLeg = tempInjury != null && (tempInjury.type == InjuryType.TranscendantPartsTransplanted || tempInjury.type == InjuryType.TranscendantPartsDamaged);
        tempInjury = injuries.Find(x => x.site == InjurySite.LeftLeg);
        augmentedLeftLeg = tempInjury != null && (tempInjury.type == InjuryType.AugmentedPartsTransplanted || tempInjury.type == InjuryType.AugmentedPartsDamaged);
        transcendantLeftLeg = tempInjury != null && (tempInjury.type == InjuryType.TranscendantPartsTransplanted || tempInjury.type == InjuryType.TranscendantPartsDamaged);


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
        if (linkedSurvivorData.strategyDictionary == null)
        {
            linkedSurvivorData.strategyDictionary = new();
            Strategy.ResetStrategyDictionary(linkedSurvivorData.strategyDictionary);
        }
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
                            int conditionInt= strategyDictionary.Value.conditions[i].inputInt;
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    condition.conditions[i] = () => CurHPPercent > conditionInt;
                                    break;
                                case 1:
                                    condition.conditions[i] = () => CurHPPercent < conditionInt;
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
                            float inputDistance = strategyDictionary.Value.conditions[i].inputInt;
                            switch(strategyDictionary.Value.conditions[i].operator_)
                            {
                                case 0:
                                    condition.conditions[i] = () => TargetEnemy != null && Vector2.Distance(transform.position, TargetEnemy.transform.position) > inputDistance;
                                    break;
                                case 1:
                                    condition.conditions[i] = () => TargetEnemy != null && Vector2.Distance(transform.position, TargetEnemy.transform.position) < Mathf.Min(inputDistance, 1.5f);
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

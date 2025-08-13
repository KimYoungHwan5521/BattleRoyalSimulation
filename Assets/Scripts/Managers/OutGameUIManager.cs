using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public enum Training { None, Weight, Running, Fighting, Shooting, Studying }

public enum SurgeryType { Transplant, ChronicDisorderTreatment, Alteration, RecoverySerumAdministeration }

public class OutGameUIManager : MonoBehaviour
{
    #region Variables and Properties
    [Header("Components")]
    [SerializeField] Canvas canvas;
    [SerializeField] GraphicRaycaster outCanvasRaycaster;
    EventSystem eventSystem;
    bool isClicked;

    [Header("Confirm / Alert")]
    [SerializeField] GameObject confirmCanvas;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmButton;
    [SerializeField] GameObject alertCanvas;

    [Header("Global")]
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] int money;
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            if(money < 0) moneyText.text = $"<color=red>{money:###,###,###,##0}</color>";
            else moneyText.text = $"{money:###,###,###,##0}";

            if (value >= 100000) AchievementManager.UnlockAchievement("Hundred-Thousandaire");
        }
    }

    [Header("Checklist")]
    [SerializeField] GameObject checkTrainingTrue;
    [SerializeField] GameObject checkTrainingFalse;
    [SerializeField] LocalizeStringEvent checkTrainingText;
    [SerializeField] GameObject checkBattleRoyaleTrue;
    [SerializeField] GameObject checkBattleRoyaleFalse;
    [SerializeField] LocalizeStringEvent checkBattleRoyaleText;

    [Header("Survivors / Hire Market")]
    int mySurvivorsId = 0;
    public int MySurvivorsId => mySurvivorsId;
    [SerializeField] GameObject hireSurvivor;
    [SerializeField] GameObject hireClose;
    public SurvivorInfo[] survivorsInHireMarket;
    [SerializeField] TMP_Dropdown survivorsDropdown;
    public TMP_Dropdown SurvivorsDropdown => survivorsDropdown;
    [SerializeField] SurvivorInfo selectedSurvivor;

    [SerializeField] List<SurvivorData> mySurvivorsData;
    public List<SurvivorData> MySurvivorsData => mySurvivorsData;
    int survivorHireLimit = 10;
    public int SurvivorHireLimit => survivorHireLimit;
    [SerializeField] TextMeshProUGUI survivorCountText;

    [Header("Training Room")]
    [SerializeField] GameObject trainingRoom;
    [SerializeField] GameObject trainingAssignForm;
    [SerializeField] GameObject scheduledTrainingByEachSurvivor;
    [SerializeField] GameObject[] scheduledTrainings;
    [SerializeField] TextMeshProUGUI weightTrainingNameText;
    [SerializeField] TextMeshProUGUI runningNameText;
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI studyingNameText;
    [SerializeField] TextMeshProUGUI weightTrainingExplain;
    [SerializeField] TextMeshProUGUI runningExplain;
    [SerializeField] TextMeshProUGUI fightingTrainingExplain;
    [SerializeField] TextMeshProUGUI shootingTrainingExplain;
    [SerializeField] TextMeshProUGUI studyExplain;
    [SerializeField] int fightTrainingLevel = 1;
    [SerializeField] int shootingTrainingLevel = 1;
    [SerializeField] int runningLevel = 1;
    [SerializeField] int weightTrainingLevel = 1;
    [SerializeField] int studyingLevel = 1;

    public int FightTrainingLevel => fightTrainingLevel;
    public int ShootingTrainingLevel => shootingTrainingLevel;
    public int AgilityTrainingLevel => runningLevel;
    public int WeightTrainingLevel => weightTrainingLevel;
    public int StudyLevel => studyingLevel;
    readonly int[] facilityUpgradeCost = { 3000, 10000, 30000 };
    [SerializeField] GameObject fightTrainingUpgradeButtion;
    [SerializeField] GameObject shootingTrainingUpgradeButtion;
    [SerializeField] GameObject runningUpgradeButtion;
    [SerializeField] GameObject weightTrainingUpgradeButtion;
    [SerializeField] GameObject studyingUpgradeButtion;
    [SerializeField] ScrollRect[] bookedTodayScrollRects;
    [SerializeField] TextMeshProUGUI weightTrainingBookers;
    [SerializeField] TextMeshProUGUI runningBookers;
    [SerializeField] TextMeshProUGUI fightTrainingBookers;
    [SerializeField] TextMeshProUGUI shootingTrainingBookers;
    [SerializeField] TextMeshProUGUI studyingBookers;

    [SerializeField] TextMeshProUGUI assignTrainingNameText;
    [SerializeField] Transform survivorsAssignedThis;
    [SerializeField] Transform survivorsWithoutSchedule; 
    [SerializeField] Transform survivorsWithOtherSchedule;
    List<SurvivorSchedule> survivorSchedules = new();
    bool autoAssign = true;
    [SerializeField] GameObject autoAssignCheckBox;

    [Header("Operating Room")]
    [SerializeField] GameObject operatingRoom;
    [SerializeField] TMP_Dropdown selectSurvivorGetSurgeryDropdown;
    [SerializeField] SurvivorInfo survivorInfoGetSurgery;
    [SerializeField] SurvivorData survivorWhoWantSurgery;
    [SerializeField] Toggle surgeryType_Transplantation;
    [SerializeField] Toggle surgeryType_ChronicDisorderTreatment;
    [SerializeField] Toggle surgeryType_Alteration;
    [SerializeField] Toggle surgeryType_Other_Treatments;

    [SerializeField] GameObject selectSurgery;
    [SerializeField] GameObject scheduledSurgery;
    [SerializeField] GameObject buttonScheduleSurgery;
    [SerializeField] GameObject buttonCancelSurgery;

    [SerializeField] GameObject[] surgeries;
    [SerializeField] ToggleGroup surgeriesToggleGroup;

    [Header("Strategy Room")]
    [SerializeField] GameObject strategyRoom;
    [SerializeField] TMP_Dropdown selectSurvivorEstablishStrategyDropdown;
    [SerializeField] SurvivorInfo survivorInfoEstablishStrategy;
    [SerializeField] SurvivorData survivorWhoWantEstablishStrategy;

    [SerializeField] TMP_InputField search;
    [SerializeField] GameObject deleteSearchText;

    [SerializeField] LocalizedDropdown weaponPriority1Dropdown;
    [SerializeField] Strategy[] strategies;
    
    [SerializeField] TMP_Dropdown sawAnEnemyAndItIsInAttackRangeDropdown;
    [SerializeField] TMP_Dropdown elseActionSawAnEnemyAndItIsInAttackRangeDropdown;

    [SerializeField] TMP_Dropdown sawAnEnemyAndItIsOutsideOfAttackRangeDropdown;
    [SerializeField] TMP_Dropdown elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown;

    [SerializeField] TMP_Dropdown heardDistinguishableSoundDropdown;
    [SerializeField] TMP_Dropdown elseActionHeardDistinguishableSoundDropdown;

    [SerializeField] TMP_Dropdown heardIndistinguishableSoundDropdown;
    [SerializeField] TMP_Dropdown elseActionHeardIndistinguishableSoundDropdown;

    [SerializeField] TMP_Dropdown whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown;

    [SerializeField] LocalizedDropdown craftingPriority1Dropdown;

    [SerializeField] Transform craftingAllow;
    public List<GameObject> craftableAllows = new();

    struct SurgeryInfo
    {
        public LocalizedString surgeryName;
        public int surgeryCost;
        public InjurySite surgerySite;
        public SurgeryType surgeryType;
        public CharacteristicType surgeryCharacteristic;

        public SurgeryInfo(LocalizedString surgeryName, int surgeryCost, InjurySite surgerySite, SurgeryType surgeryType, CharacteristicType surgeryCharacteristic = CharacteristicType.BadEye)
        {
            this.surgeryName = surgeryName;
            this.surgeryCost = surgeryCost;
            this.surgerySite = surgerySite;
            this.surgeryType = surgeryType;
            this.surgeryCharacteristic = surgeryCharacteristic;
        }
    }
    List<SurgeryInfo> surgeryList;

    [Header("Schedule")]
    public GameObject calendarObject;
    Calendar calendar;
    [SerializeField] SurvivorData mySurvivorDataInBattleRoyale;
    public SurvivorData MySurvivorDataInBattleRoyale
    {
        get
        {
            if (mySurvivorDataInBattleRoyale == null || mySurvivorDataInBattleRoyale.localizedSurvivorName == null) return null;
            else
            {
                Debug.Log(mySurvivorDataInBattleRoyale.localizedSurvivorName);
                return mySurvivorDataInBattleRoyale;
            }
        }
    }

    [Header("Daily Result")]
    [SerializeField] GameObject buttonEndTheWeek;
    [SerializeField] GameObject dailyResult;
    [SerializeField] GameObject[] survivorTrainingResults;
    TextMeshProUGUI[][] resultTexts;
    [SerializeField] GameObject surgeryResult;
    [SerializeField] TextMeshProUGUI surgeryResultText;
    bool hadSurgery;
    LocalizedString whoUnderwentSurgery;
    LocalizedString performedSurgery;

    [Header("Betting")]
    [SerializeField] GameObject bettingRoom;
    [SerializeField] GameObject[] contestants;
    [SerializeField] GameObject selectedContestant;
    [SerializeField] SurvivorData selectedContestantData;
    int needSurvivorNumber = 4;
    int needPredictionNumber = 2;
    public int PredictionNumber => needPredictionNumber;
    [SerializeField] GameObject[] predictRankings;
    [SerializeField] GameObject[] predictRankingContestants;
    public List<SurvivorData> contestantsData = null;
    [SerializeField] GameObject draggingContestant;
    LocalizedString[] predictions;
    public LocalizedString[] Predictions => predictions;
    [SerializeField] TMP_InputField bettingAmountInput;
    int bettingAmount;
    public int BettingAmount => bettingAmount;
    [SerializeField] LocalizedDropdown sortContestantsListDropdown;
    #endregion

    private void Start()
    {
        calendar = GetComponent<Calendar>();
        mySurvivorsData = new();
        SetHireMarketFirst();
        Money = 1000;

        resultTexts = new TextMeshProUGUI[survivorTrainingResults.Length][];
        for (int i = 0; i < survivorTrainingResults.Length; i++)
        {
            resultTexts[i] = survivorTrainingResults[i].GetComponentsInChildren<TextMeshProUGUI>(true);
        }
        eventSystem = FindAnyObjectByType<EventSystem>();
        bettingAmountInput.onValueChanged.AddListener((value) => { ValidateBettingAmount(value); });
        predictions = new LocalizedString[5];

        sortContestantsListDropdown.ClearOptions();
        sortContestantsListDropdown.AddLocalizedOptions(new List<LocalizedString>()
        {
            new("Basic", "Strength"),
            new("Basic", "Agility"),
            new("Basic", "Fighting"),
            new("Basic", "Shooting"),
            new("Basic", "Knowledge"),
            new("Basic", "Stat Total"),
        });

        GameManager.Instance.ObjectStart += () =>
        {
            RelocalizeTrainingRoom();
            InitializeStrategyRoom();
            sortContestantsListDropdown.RelocalizeOptions();
        };
        bool colorChanged = false;
        GameManager.Instance.ObjectUpdate += () =>
        {
            if (selectSurvivorGetSurgeryDropdown.IsExpanded)
            {
                if (!colorChanged)
                {
                    var dropdownSprites = selectSurvivorGetSurgeryDropdown.transform.Find("Dropdown List").Find("Viewport").Find("Content").GetComponentsInChildren<NullClass>();
                    for (int i = 0; i < mySurvivorsData.Count; i++)
                    {
                        bool exit = false;
                        foreach (var injury in mySurvivorsData[i].injuries)
                        {
                            if (injury.degree > 0)
                            {
                                dropdownSprites[i].GetComponent<Image>().color = new Color(1, 0.6467f, 0.6467f);
                                exit = true;
                                break;
                            }
                        }
                        if(!exit) dropdownSprites[i].GetComponent<Image>().color = Color.white;
                    }
                    colorChanged = true;
                }
            }
            else colorChanged = false;

        };
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void ResetData()
    {
        mySurvivorsData = new();
        SetHireMarketFirst();
        Money = 1000;
        survivorHireLimit = 10;
        fightTrainingLevel = 1;
        shootingTrainingLevel = 1;
        runningLevel = 1;
        weightTrainingLevel = 1;
        studyingLevel = 1;
    }

    void RelocalizeTrainingRoom()
    {
        var trainingType = new LocalizedString("Basic", "Training:Weight");
        weightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments 
            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = weightTrainingLevel } };
        trainingType = new LocalizedString("Basic", "Training:Running");
        runningNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = runningLevel } };
        trainingType = new LocalizedString("Basic", "Training:Fighting");
        fightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = fightTrainingLevel } };
        trainingType = new LocalizedString("Basic", "Training:Shooting");
        shootingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = shootingTrainingLevel } };
        trainingType = new LocalizedString("Basic", "Training:Studying");
        studyingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = studyingLevel } };

        weightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[weightTrainingLevel - 1] } };
        runningUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[runningLevel - 1] } };
        fightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[fightTrainingLevel - 1] } };
        shootingTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[shootingTrainingLevel - 1] } };
        studyingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[studyingLevel - 1] } };

        weightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        runningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        fightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        shootingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        studyingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        weightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        runningUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        fightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        shootingTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
        studyingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

        weightTrainingExplain.text = $"{new LocalizedString("Basic", "Strength").GetLocalizedString()}+";
        runningExplain.text = $"{new LocalizedString("Basic", "Agility").GetLocalizedString()}+";
        fightingTrainingExplain.text = $"{new LocalizedString("Basic", "Fighting").GetLocalizedString()}+";
        shootingTrainingExplain.text = $"{new LocalizedString("Basic", "Shooting").GetLocalizedString()}+";
        studyExplain.text = $"{new LocalizedString("Basic", "Knowledge").GetLocalizedString()}+";
    }

    private void Update()
    {
        if(isClicked)
        {
            if(selectedContestantData != null)
            {
                Vector2 localPoint;
                Vector2 mousePos = Input.mousePosition;

                // 마우스 위치를 캔버스의 로컬 좌표로 변환
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    mousePos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out localPoint
                );

                draggingContestant.transform.localPosition = localPoint;
            }
        }
    }

    #region Hire
    public void SetHireMarketFirst()
    {
        foreach(var hireMarket in survivorsInHireMarket) hireMarket.SoldOut = false;
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 25, 25, 20, 20, 20, 0, 100, Tier.Bronze);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 20, 20, 25, 25, 20, 0, 100, Tier.Bronze);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 20, 20, 20, 20, 30, 0, 100, Tier.Bronze);
        hireClose.SetActive(false);
    }

    public void ResetHireMarket()
    {
        float value = (fightTrainingLevel + shootingTrainingLevel + runningLevel + weightTrainingLevel + studyingLevel) / 5f;
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            int randStrength = UnityEngine.Random.Range(0, 101);
            int randAgility = UnityEngine.Random.Range(0, 101);
            int randFighting = UnityEngine.Random.Range(0, 101);
            int randShooting = UnityEngine.Random.Range(0, 101);
            int randKnowledge = UnityEngine.Random.Range(0, 101);
            int totalRand = randStrength + randAgility + randFighting + randShooting + randKnowledge;
            if ((totalRand < value * 70f || totalRand > value * 130f) && check < 1000)
            {
                i--;
                check++;
                continue;
            }
            if (check >= 1000) Debug.LogWarning("Infinite loop detected");
            check = 0;

            int characteristicCount;
            float randCharCount = UnityEngine.Random.Range(0, 1f);
            if (randCharCount < 0.33f) characteristicCount = 0;
            else if (randCharCount < 0.66f) characteristicCount = 1;
            else if (randCharCount < 0.9f) characteristicCount = 2;
            else characteristicCount = 3;
            
            survivorsInHireMarket[i].SetInfo(GetRandomName(),
                randStrength,
                randAgility,
                randFighting,
                randShooting,
                randKnowledge,
                characteristicCount,
                (int)(value * value * totalRand),
                Tier.Bronze);
            survivorsInHireMarket[i].SoldOut = false;
        }
    }

    public void OpenHireSurvivorMarket()
    {
        hireClose.SetActive(mySurvivorsData.Count > 0);
        hireSurvivor.SetActive(true);
        GameManager.Instance.openedWindows.Push(hireSurvivor);
        foreach (var survivorInfo in survivorsInHireMarket) survivorInfo.SetCharacteristic();
    }

    public void CloseAll()
    {
        hireSurvivor.SetActive(false);
        trainingRoom.SetActive(false);
        operatingRoom.SetActive(false);
        strategyRoom.SetActive(false);
        bettingRoom.SetActive(false);
    }

    public void HireSurvivor(int candidate)
    {
        OpenConfirmWindow($"Confirm:Purchase",
            () => {
                if(mySurvivorsData.Count >= survivorHireLimit)
                {
                    Alert("Alert:Survivor limit reached.");
                }
                else if(money < survivorsInHireMarket[candidate].survivorData.price)
                {
                    Alert("Alert:Not enough money.");
                }
                else
                {
                    Money -= survivorsInHireMarket[candidate].survivorData.price;
                    mySurvivorsData.Add(new(survivorsInHireMarket[candidate].survivorData));
                    ChecklistTraining();
                    mySurvivorsData[mySurvivorsData.Count - 1].id = mySurvivorsId++;
                    mySurvivorsData[mySurvivorsData.Count - 1].characteristics = survivorsInHireMarket[candidate].survivorData.characteristics;
                    //mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;
                    survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";

                    if(mySurvivorsData.Count == 1)
                    {
                        survivorsDropdown.ClearOptions();
                        selectedSurvivor.SetInfo(mySurvivorsData[0], true);
                    }
                    survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString() });
                    survivorsDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    selectSurvivorGetSurgeryDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    selectSurvivorEstablishStrategyDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    survivorsInHireMarket[candidate].SoldOut = true;

                    if (mySurvivorsData.Count == 1) ResetHireMarket();
                    hireSurvivor.SetActive(false);
                }
            },  $"{survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString()}", $"{survivorsInHireMarket[candidate].survivorData.price}");
        
    }

    LocalizedString GetRandomName(int depth = 0)
    {
        if (depth > 100)
        {
            Debug.LogWarning("Infinite recursion detected");
            return default;
        }

        string candidate = Names.SurvivorName[UnityEngine.Random.Range(0, Names.SurvivorName.Length)];
        for (int i = 0; i < 3; i++)
            if (survivorsInHireMarket[0].survivorData.SurvivorName == candidate) return GetRandomName(depth++);
        for (int i = 0; i < mySurvivorsData.Count; i++)
            if (mySurvivorsData[i].SurvivorName == candidate) return GetRandomName(depth++);
        if(contestantsData != null)
        {
            for(int i = 0; i < contestantsData.Count; i++)
                if(contestantsData[i].SurvivorName == candidate) return GetRandomName(depth++);
        }
        return new("Name", candidate);
    }

    public void ResetSurvivorsDropdown()
    {
        survivorsDropdown.ClearOptions();
        for(int i = 0; i<mySurvivorsData.Count; i++)
        {
            survivorsDropdown.AddOptions(new List<string>(new string[] { mySurvivorsData[i].localizedSurvivorName.GetLocalizedString() }));
        }
        if (mySurvivorsData.Count == 0) return;
        survivorsDropdown.captionText.text = survivorsDropdown.options[survivorsDropdown.value].text;
        ResetSelectedSurvivorInfo();
        survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";
    }

    public void DismissSurvivor()
    {
        if(mySurvivorsData.Count < 2)
        {
            Alert("Alert:You must have at least one survivor.");
        }
        else
        {
            SurvivorData wantDismiss = mySurvivorsData[survivorsDropdown.value];
            OpenConfirmWindow("Confirm:Release", () =>
                {
                    mySurvivorsData.Remove(wantDismiss);
                    ChecklistTraining();
                    ResetSurvivorsDropdown();
                    survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";
                    Alert("Alert:Survivor has been released.");
                }, $"{wantDismiss.localizedSurvivorName.GetLocalizedString()}");
        }
    }
    #endregion

    public void OnSurvivorSelected()
    {
        SurvivorData survivorInfo = mySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorsDropdown.options[survivorsDropdown.value].text);
        selectedSurvivor.SetInfo(survivorInfo, true);
    }

    public void ResetSelectedSurvivorInfo()
    {
        selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value], true);
    }

    #region Training
    public void OpenTrainingRoom()
    {
        if(calendar.Today % 7 > 4)
        {
            Alert("Alert:Training room is closed on weekends.");
        }
        else
        {
            trainingRoom.SetActive(true);
            AssignTraining();
            RelocalizeTrainingRoom();
            GameManager.Instance.openedWindows.Push(trainingRoom);
        }
    }

    public void OpenAssignTraining(int trainingIndex)
    {
        trainingAssignForm.SetActive(true);
        GameManager.Instance.openedWindows.Push(trainingAssignForm);
        Training training = (Training)trainingIndex;
        survivorSchedules = new();
        assignTrainingNameText.GetComponent<LocalizeStringEvent>().StringReference = new("Basic", $"Training:{training}");
        
        for (int i = survivorsAssignedThis.childCount - 1; i >= 0; i--)
        {
            PoolManager.Despawn(survivorsAssignedThis.GetChild(i).gameObject);
        }
        for (int i=survivorsWithoutSchedule.childCount - 1; i>=0; i--)
        {
            PoolManager.Despawn(survivorsWithoutSchedule.GetChild(i).gameObject);
        }
        for (int i = survivorsWithOtherSchedule.childCount - 1; i >= 0; i--)
        {
            PoolManager.Despawn(survivorsWithOtherSchedule.GetChild(i).gameObject);
        }

        foreach(SurvivorData survivor in mySurvivorsData)
        {
            Transform fitParent;
            Transform targetParent;
            SurvivorSchedule survivorSchedule;
            string description = "";
            string cause = "";
            LocalizedString targetTraining = new();
            
            bool assignable = true;
            bool alreadyAssigned = false;
            if (survivor.assignedTraining == training)
            {
                fitParent = survivorsAssignedThis;
                targetParent = survivorsWithoutSchedule;
            }
            else if (survivor.assignedTraining == Training.None)
            {
                fitParent = survivorsWithoutSchedule;
                targetParent = survivorsAssignedThis;
                if (!Trainable(survivor, training, out cause))
                {
                    assignable = false;
                    description = "Help:Cannot Assign Training";
                }
            }
            else
            {
                alreadyAssigned = true;
                fitParent = survivorsWithOtherSchedule;
                targetParent = survivorsAssignedThis;
                description = "Help:Training Already Assigned";
                targetTraining = new("Basic", $"Training:{survivor.assignedTraining}");
            }
            survivorSchedule = PoolManager.Spawn(ResourceEnum.Prefab.SurvivorSchedule, fitParent).GetComponent<SurvivorSchedule>();
            survivorSchedule.SetSurvivorData(survivor, training, assignable, fitParent, targetParent);
            if(!assignable) survivorSchedule.GetComponent<Help>().SetDescriptionWithKey(description, survivor.localizedSurvivorName.GetLocalizedString(), cause);
            else if(alreadyAssigned) survivorSchedule.GetComponent<Help>().SetDescriptionWithKey(description, survivor.localizedSurvivorName.GetLocalizedString(), targetTraining.GetLocalizedString());
            survivorSchedule.GetComponent<Button>().enabled = assignable;
            survivorSchedules.Add(survivorSchedule);
        }
    }

    public void OpenScheduledTrainingByEachSurvivor()
    {
        scheduledTrainingByEachSurvivor.SetActive(true);
        GameManager.Instance.openedWindows.Push(scheduledTrainingByEachSurvivor);

        for (int i = 0; i < scheduledTrainings.Length; i++)
        {
            if (i < mySurvivorsData.Count)
            {
                SurvivorData survivor = mySurvivorsData[i];
                var scheduleTexts = scheduledTrainings[i].GetComponentsInChildren<TextMeshProUGUI>();
                scheduleTexts[0].GetComponent<LocalizeStringEvent>().StringReference = survivor.localizedSurvivorName;
                string trainingName = mySurvivorsData[i].assignedTraining switch
                {
                    Training.None => "None",
                    Training.Weight => "Training:Weight",
                    Training.Running => "Training:Running",
                    Training.Fighting => "Training:Fighting",
                    Training.Shooting => "Training:Shooting",
                    Training.Studying => "Training:Studying",
                    _ => throw new NotImplementedException(),
                };
                scheduleTexts[1].GetComponent<LocalizeStringEvent>().StringReference = new("Basic", trainingName);

                scheduledTrainings[i].SetActive(true);
            }
            else scheduledTrainings[i].SetActive(false);
        }
    }

    public void ConfirmAssignTraining()
    {
        foreach(var survivorSchedule in survivorSchedules)
        {
            survivorSchedule.survivor.assignedTraining = survivorSchedule.whereAmI;
        }
        AssignTraining();
    }

    public void AssignTraining()
    {
        weightTrainingBookers.text = "";
        runningBookers.text = "";
        fightTrainingBookers.text = "";
        shootingTrainingBookers.text = "";
        studyingBookers.text = "";
        foreach (SurvivorData survivor in mySurvivorsData)
        {
            TextMeshProUGUI targetText;
            switch (survivor.assignedTraining)
            {
                case Training.Fighting:
                    targetText = fightTrainingBookers;
                    break;
                case Training.Shooting:
                    targetText = shootingTrainingBookers;
                    break;
                case Training.Running:
                    targetText = runningBookers;
                    break;
                case Training.Weight:
                    targetText = weightTrainingBookers;
                    break;
                case Training.Studying:
                    targetText = studyingBookers;
                    break;
                default:
                    targetText = null;
                    break;
            }

            if (targetText != null)
            {
                if (targetText.text != "") targetText.text += $"\n";
                targetText.text += $"{survivor.localizedSurvivorName.GetLocalizedString()}";
            }
        }
        foreach (var scrollRect in bookedTodayScrollRects)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.GetComponent<RectTransform>());
        }
        ChecklistTraining();
    }

    public void CheckTrainable(SurvivorData survivor)
    {
        if (Trainable(survivor, survivor.assignedTraining, out string cause)) return;
        else
        {
            survivor.assignedTraining = Training.None;
            AssignTraining();
            Alert("Alert:Training Assignment Cancelled", survivor.localizedSurvivorName.GetLocalizedString(), cause);
        }
    }

    bool TrainableAnything(SurvivorData survivor)
    {
        if (Trainable(survivor, Training.Fighting)) return true;
        if (Trainable(survivor, Training.Shooting)) return true;
        if (Trainable(survivor, Training.Running)) return true;
        if (Trainable(survivor, Training.Weight)) return true;
        return false;
    }

    bool Trainable(SurvivorData survivor, Training training)
    {
        return Trainable(survivor, training, out string cause);
    }

    bool Trainable(SurvivorData survivor, Training training, out string cause)
    {
        if(survivor.surgeryScheduled)
        {
            cause = new LocalizedString("Basic", "Surgery scheduled").GetLocalizedString();
            return false;
        }

        int eyeInjury = 0;
        foreach(Injury injury in survivor.injuries)
        {
            if (injury.type == InjuryType.ArtificialPartsTransplanted) continue;
            if (injury.degree < 0.1f) continue;
            switch(training)
            {
                case Training.Fighting:
                    switch(injury.site)
                    {
                        case InjurySite.Organ:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                        case InjurySite.RightLeg:
                        case InjurySite.LeftLeg:
                        case InjurySite.RightKnee:
                        case InjurySite.LeftKnee:
                        case InjurySite.RightFoot:
                        case InjurySite.LeftFoot:
                            cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                            return false;
                        default:
                            if (injury.degree < 1)
                            {
                                cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                                return false;
                            }
                            break;
                    }
                    break;
                case Training.Shooting:
                    switch(injury.site)
                    {
                        case InjurySite.Brain:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                            return false;
                        case InjurySite.Organ:
                            if(injury.degree >= 1)
                            {
                                cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                                return false;
                            }
                            break;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            eyeInjury++;
                            if (eyeInjury >= 2)
                            {
                                cause = new LocalizedString("Basic", "Both eye injuries").GetLocalizedString();
                                return false;
                            }
                            break;
                    }
                    break;
                case Training.Running:
                    switch (injury.site)
                    {
                        case InjurySite.Brain:
                        case InjurySite.Chest:
                        case InjurySite.Ribs:
                        case InjurySite.Abdomen:
                        case InjurySite.Organ:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                        case InjurySite.RightLeg:
                        case InjurySite.LeftLeg:
                        case InjurySite.RightKnee:
                        case InjurySite.LeftKnee:
                        case InjurySite.RightFoot:
                        case InjurySite.LeftFoot:
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
                            cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                            return false;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            eyeInjury++;
                            if (eyeInjury >= 2)
                            {
                                cause = new LocalizedString("Basic", "Both eye injuries").GetLocalizedString();
                                return false;
                            }
                            break;
                    }
                    break;
                case Training.Weight:
                    switch(injury.site)
                    {
                        case InjurySite.Brain:
                        case InjurySite.Chest:
                        case InjurySite.Ribs:
                        case InjurySite.Abdomen:
                        case InjurySite.Organ:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                            return false;
                    }
                    break;
                default:
                    break;

            }
        }
        cause = null;
        return true;
    }

    public void ToggleAutoAssign()
    {
        autoAssign = !autoAssign;
        autoAssignCheckBox.SetActive(autoAssign);
    }

    public void UpgradeTrainingRoom(int trainingRoomIndex)
    {
        switch (trainingRoomIndex)
        {
            case 0:
                OpenConfirmWindow("Confirm:Upgrade Facility", () =>
                {
                    if (money < facilityUpgradeCost[weightTrainingLevel - 1])
                    {
                        Alert("Alert:Not enough money.");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[weightTrainingLevel - 1];
                        weightTrainingLevel++;

                        var trainingType = new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}");
                        weightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
                            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = weightTrainingLevel } };
                        weightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

                        if (weightTrainingLevel > facilityUpgradeCost.Length) weightTrainingUpgradeButtion.SetActive(false);
                        else
                        {
                            weightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[weightTrainingLevel - 1] } };
                            weightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
                        }
                    }
                }, $"{new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}").GetLocalizedString()}");
                break;
            case 1:
                OpenConfirmWindow("Confirm:Upgrade Facility", () =>
                {
                    if (money < facilityUpgradeCost[runningLevel - 1])
                    {
                        Alert("Alert:Not enough money.");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[runningLevel - 1];
                        runningLevel++;

                        var trainingType = new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}");
                        runningNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
                            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = runningLevel } };
                        runningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

                        if (runningLevel > facilityUpgradeCost.Length) runningUpgradeButtion.SetActive(false);
                        else
                        {
                            runningUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[runningLevel - 1] } };
                            runningUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
                        }
                    }
                }, $"{new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}").GetLocalizedString()}");
                break;
            case 2:
                OpenConfirmWindow("Confirm:Upgrade Facility", ()=>
                {
                    if(money < facilityUpgradeCost[fightTrainingLevel - 1])
                    {
                        Alert("Alert:Not enough money.");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[fightTrainingLevel - 1];
                        fightTrainingLevel++;

                        var trainingType = new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}");
                        fightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
                            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = fightTrainingLevel } };
                        fightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

                        if (fightTrainingLevel > facilityUpgradeCost.Length) fightTrainingUpgradeButtion.SetActive(false);
                        else
                        {
                            fightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[fightTrainingLevel - 1] } };
                            fightTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
                        }
                    }
                }, $"{new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}").GetLocalizedString()}");
                break;
            case 3:
                OpenConfirmWindow("Confirm:Upgrade Facility", () =>
                {
                    if (money < facilityUpgradeCost[shootingTrainingLevel - 1])
                    {
                        Alert("Alert:Not enough money.");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[shootingTrainingLevel - 1];
                        shootingTrainingLevel++;

                        var trainingType = new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}");
                        shootingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
                            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = shootingTrainingLevel } };
                        shootingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

                        if (shootingTrainingLevel > facilityUpgradeCost.Length) shootingTrainingUpgradeButtion.SetActive(false);
                        else
                        {
                            shootingTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[shootingTrainingLevel - 1] } };
                            shootingTrainingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
                        }
                    }
                }, $"{new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}").GetLocalizedString()}");
                break;
            case 4:
                OpenConfirmWindow("Confirm:Upgrade Facility", () =>
                {
                    if (money < facilityUpgradeCost[studyingLevel - 1])
                    {
                        Alert("Alert:Not enough money.");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[studyingLevel - 1];
                        studyingLevel++;

                        var trainingType = new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}");
                        studyingNameText.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments
                            = new[] { new { trainingType = trainingType.GetLocalizedString(), level = studyingLevel } };
                        studyingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

                        if (studyingLevel > facilityUpgradeCost.Length) studyingUpgradeButtion.SetActive(false);
                        else
                        {
                            studyingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new[] { new { cost = facilityUpgradeCost[studyingLevel - 1] } };
                            studyingUpgradeButtion.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
                        }
                    }
                }, $"{new LocalizedString("Basic", $"Training:{(Training)(trainingRoomIndex + 1)}").GetLocalizedString()}");
                break;
        }
    }
    #endregion

    #region Operating Room
    public void OpenOperatingRoom()
    {
        if (calendar.Today % 7 > 4)
        {
            Alert("Alert:The operating room is closed on weekends.");
        }
        else
        {
            SetOperatingRoom();
            operatingRoom.SetActive(true);
            GameManager.Instance.openedWindows.Push(operatingRoom);
        }
    }

    public void SetOperatingRoom()
    {
        selectSurvivorGetSurgeryDropdown.ClearOptions();
        selectSurvivorGetSurgeryDropdown.AddOptions(survivorsDropdown.options);
        SelectSurvivorToSurgery();
    }

    public void SelectSurvivorToSurgery()
    {
        survivorInfoGetSurgery.SetInfo(MySurvivorsData[selectSurvivorGetSurgeryDropdown.value], false);
        survivorWhoWantSurgery = MySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == selectSurvivorGetSurgeryDropdown.options[selectSurvivorGetSurgeryDropdown.value].text);
        
        if(survivorWhoWantSurgery.surgeryScheduled)
        {
            scheduledSurgery.SetActive(true);
            buttonCancelSurgery.SetActive(true);
            selectSurgery.SetActive(false);
            buttonScheduleSurgery.SetActive(false);
            scheduledSurgery.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{survivorWhoWantSurgery.localizedScheduledSurgeryName.GetLocalizedString()}";
        }
        else
        {
            scheduledSurgery.SetActive(false);
            buttonCancelSurgery.SetActive(false);
            selectSurgery.SetActive(true);
            buttonScheduleSurgery.SetActive(true);
            GetListOfSurgeryCanUndergo();
        }
    }

    public void GetListOfSurgeryCanUndergo()
    {
        surgeryList = new();
        LocalizedString surgeryName;
        int cost = 0;
        if(surgeryType_Transplantation.isOn)
        {
            foreach(Injury injury in survivorWhoWantSurgery.injuries)
            {
                if(injury.degree >= 1)
                {
                    surgeryName = new LocalizedString("Injury", "Prosthetic Implant")
                    {
                        Arguments = new[] { new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString() }
                    };
                    switch(injury.site)
                    {
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
                            cost = 50;
                            break;
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cost = 500;
                            break;
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                            cost = 750;
                            break;
                        case InjurySite.RightFoot:
                        case InjurySite.LeftFoot:
                            cost = 500;
                            break;
                        case InjurySite.RightKnee:
                        case InjurySite.LeftKnee:
                            cost = 750;
                            break;
                        case InjurySite.RightLeg:
                        case InjurySite.LeftLeg:
                            cost = 1000;
                            break;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            cost = 2000;
                            break;
                        case InjurySite.RightEar:
                        case InjurySite.LeftEar:
                            cost = 500;
                            break;
                        case InjurySite.Organ:
                            cost = 3000;
                            break;
                        default:
                            Debug.LogWarning($"Can't transplant site : {injury.site}");
                            break;
                    }
                    surgeryList.Add(new(surgeryName, cost, injury.site, SurgeryType.Transplant));
                }
            }
        }
        else if(surgeryType_ChronicDisorderTreatment.isOn)
        {
            foreach(var characteristic in survivorWhoWantSurgery.characteristics)
            {
                var localizedSurveryName = new LocalizedString("Injury", "Treatment") { Arguments = new[] { characteristic.characteristicName.GetLocalizedString() } };
                switch(characteristic.type)
                {
                    case CharacteristicType.BadEye:
                        surgeryList.Add(new(localizedSurveryName, 1000, InjurySite.LeftEye, SurgeryType.ChronicDisorderTreatment, CharacteristicType.BadEye));
                        break;
                    case CharacteristicType.BadHearing:
                        surgeryList.Add(new(localizedSurveryName, 500, InjurySite.LeftEar, SurgeryType.ChronicDisorderTreatment, CharacteristicType.BadHearing));
                        break;
                    default:
                        break;
                }
            }
        }
        else if(surgeryType_Other_Treatments.isOn)
        {
            foreach (Injury injury in survivorWhoWantSurgery.injuries)
            {
                if(injury.degree > 0 && injury.degree < 1)
                {
                    surgeryList.Add(new(new("Injury", "Administer Recovery Serum"), 1000, InjurySite.None, SurgeryType.RecoverySerumAdministeration));
                    break;
                }
            }
        }

        for (int i = 0; i < surgeries.Length; i++)
        {
            if (i < surgeryList.Count)
            {
                surgeries[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = surgeryList[i].surgeryName.GetLocalizedString();
                surgeries[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"$ {surgeryList[i].surgeryCost}";
                surgeries[i].SetActive(true);
            }
            else
            {
                surgeries[i].SetActive(false);
            }
        }
        surgeries[0].GetComponentInChildren<Toggle>().isOn = true;
    }

    public void ScheduleSurgery()
    {
        if (surgeryList.Count == 0) return;
        int index = 0;
        for(int i=0; i< surgeries.Length; i++)
        {
            if (surgeries[i].GetComponentInChildren<Toggle>().isOn)
            {
                index = i;
                break;
            }
        }
        
        if(index > surgeryList.Count)
        {
            Debug.LogWarning("Wrong surgeryList index");
            return;
        }

        survivorWhoWantSurgery.localizedScheduledSurgeryName = surgeryList[index].surgeryName;
        survivorWhoWantSurgery.scheduledSurgeryCost = surgeryList[index].surgeryCost;
        survivorWhoWantSurgery.surgerySite = surgeryList[index].surgerySite;
        survivorWhoWantSurgery.surgeryType = surgeryList[index].surgeryType;
        survivorWhoWantSurgery.surgeryCharacteristic = surgeryList[index].surgeryCharacteristic;
        OpenConfirmWindow("Confirm:Schedule Surgery", ()=>
        {
            if(money < survivorWhoWantSurgery.scheduledSurgeryCost)
            {
                Alert("Alert:Not enough money.");
            }
            else if(surgeryList[index].surgeryType == SurgeryType.RecoverySerumAdministeration && survivorWhoWantSurgery.RecoverySerumAdministered)
            {
                Alert("Alert:Already administered");
            }
            else
            {
                if (survivorWhoWantSurgery.assignedTraining != Training.None)
                {
                    AssignTraining();
                    Alert("Alert:Training  Canceled", $"{survivorWhoWantSurgery.localizedSurvivorName.GetLocalizedString()}");
                }
                survivorWhoWantSurgery.assignedTraining = Training.None;
                survivorWhoWantSurgery.surgeryScheduled = true;
                Money -= survivorWhoWantSurgery.scheduledSurgeryCost;
                SelectSurvivorToSurgery();
                Alert("Alert:Surgery has been scheduled.");
            }
        }, $"{ survivorWhoWantSurgery.localizedSurvivorName.GetLocalizedString() }", $"{ survivorWhoWantSurgery.scheduledSurgeryName }", $"{ survivorWhoWantSurgery.scheduledSurgeryCost }");
    }

    public void CancelSurgery()
    {
        OpenConfirmWindow("Confirm:Cancel Surgery", () =>
        {
            survivorWhoWantSurgery.surgeryScheduled = false;
            Money += survivorWhoWantSurgery.scheduledSurgeryCost;
            survivorWhoWantSurgery.scheduledSurgeryCost = 0;
            SelectSurvivorToSurgery();
            Alert("Alert:Surgery has been canceled.");
        });
    }
    #endregion

    #region Strategy Room
    void InitializeStrategyRoom()
    {
        search.onValueChanged.AddListener((value) => Search(value));
        weaponPriority1Dropdown.ClearOptions();
        ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
        for(int i = (int)ItemManager.Items.Knife; i < (int)ItemManager.Items.Knife_Enchanted;  i++)
        {
            bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{items[i]}", out var itemSpriteEnum);
            weaponPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
            Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : null;
            weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        }
        GameManager.Instance.ObjectUpdate += () => 
        { 
            if(weaponPriority1Dropdown.dropdown.IsExpanded)
            {
                var dropdownSprites = weaponPriority1Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for(int i=0; i<weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
                    if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                }
            }
        };
        sawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        heardDistinguishableSoundDropdown.ClearOptions();
        heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        
        heardIndistinguishableSoundDropdown.ClearOptions();
        heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.ClearOptions();
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "First seen person").GetLocalizedString(), new LocalizedString("Basic", "Nearest person").GetLocalizedString(), new LocalizedString("Basic", "Person with the longest range").GetLocalizedString() }));
        
        craftingPriority1Dropdown.ClearOptions();
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        GameManager.Instance.ObjectUpdate += () =>
        {
            if (craftingPriority1Dropdown.dropdown.IsExpanded)
            {
                var dropdownSprites = craftingPriority1Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for (int i = 0; i < craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
                    if(image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                }
            }
        };
        for (int i = 0; i < ItemManager.craftables.Count; i++)
        {
            GameObject craftableAllow = PoolManager.Spawn(ResourceEnum.Prefab.CraftableAllow, craftingAllow);
            craftableAllow.GetComponentInChildren<TextMeshProUGUI>().text = new LocalizedString("Item", ItemManager.craftables[i].itemType.ToString()).GetLocalizedString();
            if (Enum.TryParse(ItemManager.craftables[i].itemType.ToString(), out ResourceEnum.Sprite sprite)) craftableAllow.GetComponentsInChildren<Image>()[1].sprite = ResourceManager.Get(sprite);
            int toggleIndex = i;
            craftableAllow.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { strategies.ToList().Find(x => x.strategyCase == StrategyCase.CraftingAllow).hasChanged = true; });
            craftableAllow.GetComponentInChildren<LocalizeStringEvent>().StringReference = new LocalizedString("Item", ItemManager.craftables[i].itemType.ToString());
            craftableAllows.Add(craftableAllow); 
        }
        foreach (Strategy strategy in strategies) strategy.Initialize();
        RelocalizeStrategyRoom();
        SetDefault();
    }

    void RelocalizeStrategyRoom()
    {
        ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
        for (int i = 0; i < weaponPriority1Dropdown.dropdown.options.Count; i++)
        {
            weaponPriority1Dropdown.RelocalizeOptions();
        }
        sawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        heardDistinguishableSoundDropdown.ClearOptions();
        heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));

        heardIndistinguishableSoundDropdown.ClearOptions();
        heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));

        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.ClearOptions();
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "First seen person").GetLocalizedString(), new LocalizedString("Basic", "Nearest person").GetLocalizedString(), new LocalizedString("Basic", "Person with the longest range").GetLocalizedString() }));

        for(int i=0; i< craftingPriority1Dropdown.keys.Count; i++)
        {
            craftingPriority1Dropdown.keys[i] = new LocalizedString("Item", ItemManager.craftables[i].itemType.ToString());
        }
    }

    public void SetDefault()
    {
        weaponPriority1Dropdown.dropdown.value = (int)ItemManager.Items.AssaultRifle - (int)ItemManager.Items.Knife;
        Image selectedWeaponPriority1Image = weaponPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        selectedWeaponPriority1Image.sprite = ResourceManager.Get(ResourceEnum.Sprite.AssaultRifle);
        selectedWeaponPriority1Image.GetComponent<AspectRatioFitter>().aspectRatio
            = selectedWeaponPriority1Image.sprite.textureRect.width / selectedWeaponPriority1Image.sprite.textureRect.height;
        foreach(Strategy strategy in strategies) strategy.SetDefault();
        sawAnEnemyAndItIsInAttackRangeDropdown.value = 0;
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = 0;
        heardDistinguishableSoundDropdown.value = 0;
        heardIndistinguishableSoundDropdown.value = 1;
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = 0;
        craftingPriority1Dropdown.dropdown.value = 0;
        foreach(var craftableAllow in craftableAllows) craftableAllow.GetComponentsInChildren<Toggle>()[0].isOn = true;
    }

    public void OpenStrategyRoom()
    {
        RelocalizeStrategyRoom();
        strategyRoom.SetActive(true);
        SetStrategyRoom();
        GameManager.Instance.openedWindows.Push(strategyRoom);
    }

    public void CloseStrategyRoom()
    {
        bool hasChanged = false;
        foreach(Strategy strategy in strategies) if(strategy.hasChanged) { hasChanged = true;  break; }
        if(hasChanged) OpenConfirmWindow("Confirm:Close Strategy Room", () => { strategyRoom.SetActive(false); });
        else strategyRoom.SetActive(false);
    }

    void SetStrategyRoom()
    {
        selectSurvivorEstablishStrategyDropdown.ClearOptions();
        selectSurvivorEstablishStrategyDropdown.AddOptions(survivorsDropdown.options);
        SelectSurvivorToEstablishStrategy();
        foreach (var strategy in strategies) strategy.hasChanged = false;
    }

    public void SelectSurvivorToEstablishStrategy()
    {
        survivorInfoEstablishStrategy.SetInfo(MySurvivorsData[selectSurvivorEstablishStrategyDropdown.value], false);
        survivorWhoWantEstablishStrategy = MySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == selectSurvivorEstablishStrategyDropdown.options[selectSurvivorEstablishStrategyDropdown.value].text);
        weaponPriority1Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
        craftingPriority1Dropdown.ClearOptions();
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        foreach (var craftable in ItemManager.craftables)
        {
            if (craftable.requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge)
            {
                bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftable.itemType}", out var itemSpriteEnum);
                craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString>{ new LocalizedString("Item", craftable.itemType.ToString()) });
                Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : null;
                craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
            }
        }
        craftingPriority1Dropdown.dropdown.value = survivorWhoWantEstablishStrategy.priority1CraftingToInt + 1;
        CraftingPriorityChanged();

        for(int i=0; i<survivorWhoWantEstablishStrategy.craftingAllows.Length; i++)
        {
            craftableAllows[i].SetActive(ItemManager.craftables[i].requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge);
            if (survivorWhoWantEstablishStrategy.craftingAllows[i]) craftableAllows[i].GetComponentsInChildren<Toggle>()[0].isOn = true;
            else craftableAllows[i].GetComponentsInChildren<Toggle>()[1].isOn = true;
        }

        // 조건 불러오기
        foreach (Strategy strategy in strategies) strategy.SetDefault();
        foreach (var strategyDictionary in survivorWhoWantEstablishStrategy.strategyDictionary)
        {
            Strategy strategy = strategies[0];
            foreach(Strategy st in strategies)
            {
                if(st.strategyCase == strategyDictionary.Key)
                {
                    strategy = st;
                    break;
                }
            }
            for(int j = 0;j < 5; j++)
            {
                if (j < strategyDictionary.Value.conditionConut)
                {
                    strategy.AddCondition();
                    strategy.andOrs[j].value = strategyDictionary.Value.conditions[j].andOr;
                    strategy.variable1s[j].value = strategyDictionary.Value.conditions[j].variable1;
                    strategy.operators[j].value = strategyDictionary.Value.conditions[j].operator_;
                    strategy.variable2s[j].value = strategyDictionary.Value.conditions[j].variable2;
                    strategy.inputFields[j].text = strategyDictionary.Value.conditions[j].inputInt.ToString();
                }
                else break;
            }
        }
    }

    void Search(string word)
    {
        deleteSearchText.SetActive(!string.IsNullOrEmpty(word));
        if(string.IsNullOrEmpty(word))
        {
            foreach(Strategy strategy in strategies) strategy.gameObject.SetActive(true);
        }
        else
        {
            foreach (Strategy strategy in strategies) strategy.gameObject.SetActive(strategy.CaseName.ToUpper().Contains(word.ToUpper()));
        }
    }

    public void DeleteSearchText()
    {
        search.text = string.Empty;
    }

    public void WeaponPriorityChanged()
    {
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}", out var itemSpriteEnum);
        if (spriteNotNull)
        {
            Image image = weaponPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
        else Debug.Log($"Sprite not found : {weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}");
    }

    public void CraftingPriorityChanged()
    {
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}", out var itemSpriteEnum);
        if (spriteNotNull)
        {
            Image image = craftingPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
        else if(craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key != "None") Debug.Log($"Sprite not found : {craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}");
    }

    public void SaveStrategy()
    {
        OpenConfirmWindow("Confirm:Save all changes?", () =>
        {
            foreach(var craftableAllow in craftableAllows) 
                if(craftableAllow.GetComponentInChildren<TextMeshProUGUI>().text == craftingPriority1Dropdown.dropdown.options[craftingPriority1Dropdown.dropdown.value].text)
                {
                    Alert("Alert:Crafting Priority Not Valid");
                    return;
                }

            bool itemNotNull = Enum.TryParse<ItemManager.Items>($"{weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}", out var itemEnum);
            if (itemNotNull) survivorWhoWantEstablishStrategy.priority1Weapon = itemEnum;
            else Debug.LogWarning($"Item enum not found : {weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}");

            if(craftingPriority1Dropdown.dropdown.value == 0)
            {
                survivorWhoWantEstablishStrategy.priority1Crafting = null;
                survivorWhoWantEstablishStrategy.priority1CraftingToInt = -1;
            }
            else
            {
                ItemManager.Craftable craftable = ItemManager.craftables.Find(x => x.itemType.ToString() == $"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value]}");
                itemNotNull = craftable != null;
                if (itemNotNull)
                {
                    survivorWhoWantEstablishStrategy.priority1Crafting = craftable;
                    survivorWhoWantEstablishStrategy.priority1CraftingToInt = craftingPriority1Dropdown.dropdown.value - 1;
                }
                else Debug.LogWarning($"Craftable not found : {craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value]}");
            }

            for(int i = 0; i < survivorWhoWantEstablishStrategy.craftingAllows.Length; i++)
            {
                survivorWhoWantEstablishStrategy.craftingAllows[i] = craftableAllows[i].GetComponentsInChildren<Toggle>()[0].isOn;
            }

            foreach(Strategy strategy in strategies)
            {
                if(strategy.NoCondition)
                {
                    survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] = new(0, 0, 0);
                }
                else
                {
                    ConditionData[] conditionData = new ConditionData[5];
                    for (int i = 0; i < 5; i++)
                    {
                        if(!int.TryParse(strategy.inputFields[i].text, out int num)) num = 0;
                        conditionData[i] = new(strategy.andOrs[i].value, strategy.variable1s[i].value, strategy.operators[i].value, strategy.variable2s[i].value, num);
                    }
                    survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] =
                        new(sawAnEnemyAndItIsInAttackRangeDropdown.value, elseActionSawAnEnemyAndItIsInAttackRangeDropdown.value, strategy.activeConditionCount, conditionData);
                }
                strategy.hasChanged = false;
            }

        });
    }

    public void CopyAllStrategies()
    {
        foreach(Strategy strategy in strategies) strategy.CopyStrategy();
    }

    public void PasteAllStrategies()
    {
        foreach (Strategy strategy in strategies) strategy.PasteStrategy();
    }
    #endregion

    public int MeasureTreatmentCost(Injury injury)
    {
        float cost = 0;

        switch(injury.site)
        {
            case InjurySite.Skull:
            case InjurySite.Brain:
                cost = injury.degree * 300;
                break;
            case InjurySite.Head:
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
            case InjurySite.RightEar:
            case InjurySite.LeftEar:
            case InjurySite.Nose:
            case InjurySite.Jaw:
                cost = injury.degree * 50;
                break;
            case InjurySite.Chest:
            case InjurySite.Ribs:
            case InjurySite.Abdomen:
                cost = injury.degree * 100;
                break;
            case InjurySite.Organ:
                cost = injury.degree * 300;
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
                cost = injury.degree * 100;
                break;
            case InjurySite.RightArm:
            case InjurySite.LeftArm:
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
                cost = injury.degree * 50; 
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
            case InjurySite.RightFoot:
            case InjurySite.LeftFoot:
                cost = injury.degree * 25;
                break;
            case InjurySite.RightThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.RightLittleFinger:
            case InjurySite.LeftThumb:
            case InjurySite.LeftIndexFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.LeftLittleFinger:
            case InjurySite.RightBigToe:
            case InjurySite.LeftBigToe:
                cost = injury.degree * 10;
                break;
        }

        return (int)cost;
    }

    public void StartBattleRoyale()
    {
        GameManager.Instance.Option.SetSaveButtonInteractable(false);
        mySurvivorDataInBattleRoyale = calendar.LeagueReserveInfo[calendar.Today].reserver;
        StartCoroutine(GameManager.Instance.BattleRoyaleStart());
    }

    #region Betting
    public void OpenBettingRoom()
    {
        if(contestantsData == null || contestantsData.Count == 0) SetContestants();
        bettingAmountInput.text = "0";

        mySurvivorDataInBattleRoyale = calendar.LeagueReserveInfo[calendar.Today].reserver;
        int thereIsPlayerSurvivor = mySurvivorDataInBattleRoyale == null ? 1 : 0;
        for (int i = 0; i < contestants.Length; i++)
        {
            if (i < contestantsData.Count)
            {
                contestants[i].SetActive(true);
                Vector3 colorVector = BattleRoyaleManager.colorInfo[i + thereIsPlayerSurvivor];
                contestants[i].GetComponentsInChildren<Image>()[1].color = new(colorVector.x, colorVector.y, colorVector.z);
                contestants[i].GetComponentInChildren<TextMeshProUGUI>().text = contestantsData[i].localizedSurvivorName.GetLocalizedString();
            }
            else contestants[i].SetActive(false);
        }
        SortContestantsList();

        needPredictionNumber = calendar.LeagueReserveInfo[calendar.Today].league switch
        {
            League.BronzeLeague => 2,
            League.SilverLeague => 3,
            League.GoldLeague => 4,
            _ => 5
        };
        for (int i = 0; i < predictRankings.Length; i++)
        {
            if (i < needPredictionNumber)
            {
                predictRankings[i].SetActive(true);
                predictRankingContestants[i].SetActive(false);
            }
            else predictRankings[i].SetActive(false);
        }
        selectedContestant.SetActive(false);

        bettingRoom.SetActive(true);
        sortContestantsListDropdown.RelocalizeOptions();
        GameManager.Instance.openedWindows.Push(bettingRoom);
    }

    public void SetContestants()
    {
        contestantsData = new();
        int index = 0;
        if (calendar.LeagueReserveInfo[calendar.Today].reserver != null)
        {
            contestantsData.Add(calendar.LeagueReserveInfo[calendar.Today].reserver);
            index++;
        }
        switch (calendar.LeagueReserveInfo[calendar.Today].league)
        {
            case League.BronzeLeague:
                needSurvivorNumber = 4;
                needPredictionNumber = 2;
                break;
            case League.SilverLeague:
                needSurvivorNumber = 9;
                needPredictionNumber = 3;
                break;
            case League.GoldLeague:
                needSurvivorNumber = 16;
                needPredictionNumber = 4;
                break;
            case League.SeasonChampionship:
            case League.WorldChampionship:
                needSurvivorNumber = 25;
                needPredictionNumber = 5;
                break;
            default:
                needSurvivorNumber = 25;
                needPredictionNumber = 5;
                break;
        }
        for (int i = index; i < needSurvivorNumber; i++)
        {
            contestantsData.Add(CreateRandomSurvivorData());
        }
    }

    public void SortContestantsList()
    {
        List<SurvivorData> orderedContestantsData = new();
        switch(sortContestantsListDropdown.keys[sortContestantsListDropdown.dropdown.value].TableEntryReference.Key)
        {
            case "Strength":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.Strength).ToList();
                break;
            case "Agility":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.Agility).ToList();
                break;
            case "Fighting":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.Fighting).ToList();
                break;
            case "Shooting":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.Shooting).ToList();
                break;
            case "Knowledge":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.Knowledge).ToList();
                break;
            case "Stat Total":
                orderedContestantsData = contestantsData.OrderByDescending(x => x.StatTotal).ToList();
                break;
            default:
                Debug.LogWarning($"Wrong sort key : {sortContestantsListDropdown.keys[sortContestantsListDropdown.dropdown.value].TableEntryReference.Key}");
                return;
        }

        int thereIsPlayerSurvivor = mySurvivorDataInBattleRoyale == null ? 1 : 0;
        for (int i = 0; i < orderedContestantsData.Count; i++)
        {
            int originalDataIndex = contestantsData.FindIndex(x => x.localizedSurvivorName.TableEntryReference.Key == orderedContestantsData[i].localizedSurvivorName.TableEntryReference.Key);
            Vector3 colorVector = BattleRoyaleManager.colorInfo[originalDataIndex + thereIsPlayerSurvivor];
            contestants[i].GetComponentsInChildren<Image>()[1].color = new(colorVector.x, colorVector.y, colorVector.z);
            contestants[i].GetComponentInChildren<TextMeshProUGUI>().text = contestantsData[originalDataIndex].localizedSurvivorName.GetLocalizedString();
        }

    }

    public void EasyBet(int amount)
    {
        int curBet = int.Parse(bettingAmountInput.text);
        bettingAmountInput.text = (curBet + amount).ToString();
    }

    public void MaxBet()
    {
        bettingAmountInput.text = money.ToString();
    }

    public void ValidateBettingAmount(string value)
    {
        if (string.IsNullOrEmpty(value)) // 빈 문자열 체크
        {
            bettingAmountInput.text = "0";
            return;
        }

        if (int.TryParse(value, out int number))
        {
            number = Mathf.Clamp(number, 0, Mathf.Max(money, 0));
            if (bettingAmountInput.text != number.ToString()) // 무한 루프 방지
                bettingAmountInput.text = number.ToString();
        }
        else
        {
            bettingAmountInput.text = "0"; // 숫자가 아닐 경우 0으로 설정
        }
    }

    public void Betting()
    {
        int _bettingAmount = int.Parse(bettingAmountInput.text);
        if (!IsValidPrediction(out string reason)) Alert($"{new LocalizedString("Basic", "Invalid Prediction").GetLocalizedString()} : {reason}");
        else if (_bettingAmount < 100) Alert("Alert:Minimum Bet");
        else
        {
            OpenConfirmWindow("Confirm:Bet", () =>
            {
                bettingAmount = _bettingAmount;
                for (int i = 0; i < needPredictionNumber; i++) predictions[i] = predictRankingContestants[i].GetComponentInChildren<LocalizeStringEvent>().StringReference;
                bettingRoom.SetActive(false);
                StartBattleRoyale();
            });
        }
    }

    public void SkipBetting()
    {
        OpenConfirmWindow("Confirm:Skip Bet", () => 
        {
            bettingAmount = 0;
            bettingRoom.SetActive(false);
            StartBattleRoyale();
        });
    }

    bool IsValidPrediction(out string reason)
    {
        for(int i = 0; i < needPredictionNumber; i++)
        {
            if (!predictRankingContestants[i].activeSelf)
            {
                reason = new LocalizedString("Basic", "Empty Prediction").GetLocalizedString();
                return false;
            }
            for(int j = 0; j < i; j++)
            {
                if(predictRankingContestants[j].GetComponentInChildren<TextMeshProUGUI>().text == predictRankingContestants[i].GetComponentInChildren<TextMeshProUGUI>().text)
                {
                    reason = new LocalizedString("Basic", "Duplicated Prediction").GetLocalizedString();
                    return false;
                }
            }
        }
        reason = "";
        return true;
    }

    public float GetOdds(int correctExactRanking, int correctOnlyRankedIn)
    {
        int totalCorrect = correctExactRanking + correctOnlyRankedIn;
        float odds = 0;
        switch(needPredictionNumber)
        {
            case 2:
                switch (totalCorrect)
                {
                    case 1:
                        odds = 1f;
                        break;
                    case 2:
                        odds = 3f;
                        break;
                }
                break;
            case 3:
                switch (totalCorrect)
                {
                    case 1:
                        odds = 1.31f;
                        break;
                    case 2:
                        odds = 4.42f;
                        break;
                    case 3:
                        odds = 84f;
                        break;
                }
                break;
            case 4:
                switch (totalCorrect)
                {
                    case 1:
                        odds = 1.37f;
                        break;
                    case 2:
                        odds = 4.09f;
                        break;
                    case 3:
                        odds = 37.14f;
                        break;
                    case 4:
                        odds = 1820f;
                        break;
                }
                break;
            case 5:
                switch(totalCorrect)
                {
                    case 1:
                        odds = 1.38f;
                        break;
                    case 2:
                        odds = 3.70f;
                        break;
                    case 3:
                        odds = 18f;
                        break;
                    case 4:
                        odds = 526.04f;
                        break;
                    case 5:
                        odds = 53130f;
                        break;
                }
                break;
        }
        if (correctExactRanking == 1) odds *= 1.5f;
        else odds *= Factorial(correctExactRanking);
        // 이론상 최대 배당 : x6375600
        return odds;
    }

    int Factorial(int n)
    {
        int result = 1;
        for (int i = n; i > 1; i--) result *= i;
        return result;
    }
    #endregion

    #region End The Day
    void DayEnd(int week = 0)
    {
        contestantsData = null;
        calendar.Today++;
        calendar.TurnPageCalendar(0);

        dailyResult.SetActive(true);
        int index = 0;
        foreach (GameObject survivorTrainingResult in survivorTrainingResults) survivorTrainingResult.SetActive(false);
        foreach (SurvivorData survivor in mySurvivorsData)
        {
            ApplyTraining(survivor, survivor.assignedTraining, week);
            if (survivor.assignedTraining != Training.None)
            {
                survivorTrainingResults[index].SetActive(true);
                resultTexts[index][0].text = survivor.localizedSurvivorName.GetLocalizedString();
                resultTexts[index][1].text = $"{new LocalizedString("Basic", "Strength").GetLocalizedString()} + {survivor.increaseComparedToPrevious_strength}";
                resultTexts[index][1].gameObject.SetActive(survivor.increaseComparedToPrevious_strength > -1);
                resultTexts[index][2].text = $"{new LocalizedString("Basic", "Agility").GetLocalizedString()} + {survivor.increaseComparedToPrevious_agility}";
                resultTexts[index][2].gameObject.SetActive(survivor.increaseComparedToPrevious_agility > -1);
                resultTexts[index][3].text = $"{new LocalizedString("Basic", "Fighting").GetLocalizedString()} + {survivor.increaseComparedToPrevious_fighting}";
                resultTexts[index][3].gameObject.SetActive(survivor.increaseComparedToPrevious_fighting > -1);
                resultTexts[index][4].text = $"{new LocalizedString("Basic", "Shooting").GetLocalizedString()} + {survivor.increaseComparedToPrevious_shooting}";
                resultTexts[index][4].gameObject.SetActive(survivor.increaseComparedToPrevious_shooting > -1);
                resultTexts[index][5].text = $"{new LocalizedString("Basic", "Knowledge").GetLocalizedString()} + {survivor.increaseComparedToPrevious_knowledge}";
                resultTexts[index][5].gameObject.SetActive(survivor.increaseComparedToPrevious_knowledge > -1);
                index++;
            }
            if (!autoAssign)
            {
                survivor.assignedTraining = Training.None;
                AssignTraining();
            }
            Surgery(survivor);
        }
        surgeryResult.SetActive(hadSurgery);
        if (hadSurgery)
        {
            surgeryResultText.text = $"{new LocalizedString("Basic", "Surgery Successful").GetLocalizedString()}\n({whoUnderwentSurgery.GetLocalizedString()} : {performedSurgery.GetLocalizedString()})";
        }
        selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value], true);

        GameManager.Instance.FixLayout(dailyResult.GetComponent<RectTransform>());
        GameManager.Instance.openedWindows.Push(dailyResult);
    }

    public void EndTheDay()
    {
        string key = "End The Day";
        string warning = "";
        if(calendar.Today % 7 < 5)
        {
            bool thereAreUnassignedSurvivors = false;
            warning = $"\n<color=red><i>{new LocalizedString("Basic", "There are unassigned survivors:").GetLocalizedString()} ";
            foreach (SurvivorData survivor in mySurvivorsData)
            {
                if (survivor.assignedTraining == Training.None && TrainableAnything(survivor))
                {
                    thereAreUnassignedSurvivors = true;
                    warning += $"{survivor.localizedSurvivorName.GetLocalizedString()}, ";
                }
            }
            warning = warning[..^2];
            warning += "</i></color>";
            if (!thereAreUnassignedSurvivors) warning = "";
        }
        else
        {
            if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
            {
                if(calendar.LeagueReserveInfo[calendar.Today].reserver != null)
                {
                    Alert("Alert:Scheduled Battle Royale Today", calendar.LeagueReserveInfo[calendar.Today].reserver.localizedSurvivorName.GetLocalizedString());
                    return;
                }
                else
                {
                    warning = new LocalizedString("Basic", "Today is a battle royale day. Skip it?").GetLocalizedString();
                }
            }
        }

        if(calendar.Today % 7 < 5)
        {
            OpenConfirmWindow(key, () =>
            {
                DayEnd();
            }, warning);
        }
        else if(calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        {
            OpenConfirmWindow(key, () =>
            {
                EndTheDayWeekend();
            }, warning);
        }
        else EndTheDayWeekend();
    }

    public void EndTheDayWeekend()
    {
        contestantsData = null;
        calendar.Today++;
        calendar.TurnPageCalendar(0);

        foreach(var survivor in mySurvivorsData)
        {
            survivor.increaseComparedToPrevious_strength = -1;
            survivor.increaseComparedToPrevious_agility = -1;
            survivor.increaseComparedToPrevious_fighting = -1;
            survivor.increaseComparedToPrevious_shooting = -1;
            survivor.increaseComparedToPrevious_knowledge = -1;
        }
        Alert("Alert:A day has passed.");
    }

    public void EndTheWeek()
    {
        string warning;
        bool thereAreUnassignedSurvivors = false;
        warning = $"\n<color=red><i>{new LocalizedString("Basic", "There are unassigned survivors:").GetLocalizedString()} ";
        foreach (SurvivorData survivor in mySurvivorsData)
        {
            if (survivor.assignedTraining == Training.None && TrainableAnything(survivor))
            {
                thereAreUnassignedSurvivors = true;
                warning += $"{survivor.localizedSurvivorName.GetLocalizedString()}, ";
            }
        }
        warning = warning[..^2];
        warning += "</i></color>";
        if (!thereAreUnassignedSurvivors) warning = "";
        OpenConfirmWindow("Confirm:End the Week", () =>
        {
            int week = 0;
            while(calendar.Today % 7 < 5)
            {
                DayEnd(week);
                if (week++ > 5)
                {
                    Debug.LogWarning("Loop error"); 
                    break;
                }
            }
        }, warning);
    }

    public void HideEndTheWeekend(bool hide)
    {
        buttonEndTheWeek.SetActive(!hide);
    }

    void ApplyTraining(SurvivorData survivor, Training training, int week = 0)
    {
        int survivorStrengthLv = survivor._strength / 20;
        int survivorAgilityLv = survivor._agility / 20;
        int survivorFightingLv = survivor._fighting / 20;
        int survivorShtLv = survivor._shooting / 20;
        int survivorKnowledgeLv = survivor._knowledge / 20;
        
        if(week == 0)
        {
            survivor.increaseComparedToPrevious_strength = -1;
            survivor.increaseComparedToPrevious_agility = -1;
            survivor.increaseComparedToPrevious_fighting = -1;
            survivor.increaseComparedToPrevious_shooting = -1;
            survivor.increaseComparedToPrevious_knowledge = -1;
        }

        switch (training)
        {
            case Training.Weight:
                int increaseStrength = Mathf.Max(weightTrainingLevel + 1 - survivorStrengthLv, 0);
                if(week == 0) survivor.increaseComparedToPrevious_strength = 0;
                survivor.IncreaseStats(increaseStrength, 0, 0, 0, 0);
                break;
            case Training.Running:
                int increseAgility = Mathf.Max(runningLevel + 1 - survivorAgilityLv, 0);
                if (week == 0) survivor.increaseComparedToPrevious_agility = 0;
                survivor.IncreaseStats(0, increseAgility, 0, 0, 0);
                break;
            case Training.Fighting:
                int increseFighting = Mathf.Max(fightTrainingLevel + 1 - survivorFightingLv, 0);
                if (week == 0) survivor.increaseComparedToPrevious_fighting = 0;
                survivor.IncreaseStats(0, 0, increseFighting, 0, 0);
                break;
            case Training.Shooting:
                int increseShooting = Mathf.Max(shootingTrainingLevel + 1 - survivorShtLv, 0);
                if (week == 0) survivor.increaseComparedToPrevious_shooting = 0;
                survivor.IncreaseStats(0, 0, 0, increseShooting, 0);
                break;
            case Training.Studying:
                int increseKnowledge = Mathf.Max(studyingLevel + 1 - survivorKnowledgeLv, 0);
                if (week == 0) survivor.increaseComparedToPrevious_knowledge = 0;
                survivor.IncreaseStats(0, 0, 0, 0, increseKnowledge);
                break;
            default:
                break;
        }
    }

    void Surgery(SurvivorData survivor)
    {
        if (!survivor.surgeryScheduled)
        {
            hadSurgery = false;
            return;
        }
        hadSurgery = true;
        whoUnderwentSurgery = survivor.localizedSurvivorName;
        performedSurgery = survivor.localizedScheduledSurgeryName;
        if(survivor.surgeryType == SurgeryType.Transplant)
        {
            Injury surgeryInjury = survivor.injuries.Find(x => x.site == survivor.surgerySite);
            surgeryInjury.type = InjuryType.ArtificialPartsTransplanted;
            surgeryInjury.degree = 0;
            survivor.injuries.Add(new(survivor.surgerySite, InjuryType.RecoveringFromSurgery, 0.5f));
        }
        else if (survivor.surgeryType == SurgeryType.ChronicDisorderTreatment)
        {
            switch(survivor.surgeryCharacteristic)
            {
                case CharacteristicType.BadEye:
                    survivor.injuries.Add(new(InjurySite.LeftEye, InjuryType.RecoveringFromSurgery, 0.4f));
                    survivor.injuries.Add(new(InjurySite.RightEye, InjuryType.RecoveringFromSurgery, 0.4f));
                    break;
                case CharacteristicType.BadHearing:
                    survivor.injuries.Add(new(InjurySite.LeftEar, InjuryType.RecoveringFromSurgery, 0.4f));
                    survivor.injuries.Add(new(InjurySite.RightEar, InjuryType.RecoveringFromSurgery, 0.4f));
                    break;
            }
            survivor.characteristics.Remove(survivor.characteristics.Find(x => x.type == survivor.surgeryCharacteristic));
        }
        else if(survivor.surgeryType == SurgeryType.RecoverySerumAdministeration)
        {
            survivor.RecoverySerumAdministered = true;
        }

        survivor.totalSurgeryFee += survivor.scheduledSurgeryCost;
        survivor.scheduledSurgeryCost = 0;
        survivor.surgeryScheduled = false;
    }

    public void SurvivorsRecovery()
    {
        foreach(SurvivorData survivor in mySurvivorsData)
        {
            List<Injury> fullyRecovered = new();
            foreach(Injury injury in survivor.injuries)
            {
                if(injury.degree < 1 && injury.type != InjuryType.ArtificialPartsTransplanted)
                {
                    float recovery = 0;
                    float recoveryRate = 1;
                    if (survivor.RecoverySerumAdministered)
                    {
                        recoveryRate *= 3;
                        survivor.recoverySerumMedicalEffectLeft--;
                        if (survivor.recoverySerumMedicalEffectLeft <= 0) survivor.RecoverySerumAdministered = false;
                    }
                    if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) recoveryRate *= 1.5f;
                    else if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) recoveryRate *= 0.7f;
                    
                    if (injury.degree > 0.9) recovery = (1 - injury.degree) * recoveryRate;
                    else recovery = (0.1f + (1 - injury.degree) * 0.1f) * recoveryRate;
                    injury.degree -= recovery;

                    if(injury.degree <= 0) fullyRecovered.Add(injury);
                }
            }
            foreach(Injury recovered in fullyRecovered)
            {
                survivor.injuries.Remove(recovered);
            }
        }
        ResetSelectedSurvivorInfo();
    }
    #endregion

    #region Checklist
    public void ChecklistTraining()
    {
        string unassigned = "";
        foreach(var survivor in mySurvivorsData)
        {
            if(survivor.assignedTraining == Training.None)
            {
                if(Trainable(survivor, Training.Weight) || Trainable(survivor, Training.Running) || Trainable(survivor, Training.Fighting) || Trainable(survivor, Training.Shooting) || Trainable(survivor, Training.Studying))
                {
                    unassigned += survivor.localizedSurvivorName.GetLocalizedString();
                    unassigned += ", ";
                }
            }
        }
        if (unassigned != "")
        {
            unassigned = unassigned[..^2];
            checkTrainingTrue.SetActive(false);
            checkTrainingFalse.SetActive(true);
            LocalizedString localizedString = new("Basic", "Survivors without training:");
            localizedString.Arguments = new[] { unassigned };
            checkTrainingText.StringReference = localizedString;
        }
        else
        {
            checkTrainingTrue.SetActive(true);
            checkTrainingFalse.SetActive(false);
            checkTrainingText.StringReference = new("Basic", "All survivors assigned to training.");
        }
        GameManager.Instance.FixLayout(checkTrainingText.GetComponent<RectTransform>());
    }

    public void ChecklistBattleRoyale()
    {
        bool thereAreNotReservedRoyale = false;
        int monday = calendar.Today - calendar.Today % 7;
        if (calendar.Today < monday + 5 && calendar.LeagueReserveInfo.ContainsKey(monday + 5))
        {
            if (calendar.LeagueReserveInfo[monday + 5].reserver == null)
            {
                thereAreNotReservedRoyale = true;
            }
        }
        if (calendar.Today < monday + 6 && calendar.LeagueReserveInfo.ContainsKey(monday + 6))
        {
            if (calendar.LeagueReserveInfo[monday + 6].reserver == null)
            {
                thereAreNotReservedRoyale = true;
            }
        }

        if(thereAreNotReservedRoyale)
        {
            checkBattleRoyaleTrue.SetActive(false);
            checkBattleRoyaleFalse.SetActive(true);
            checkBattleRoyaleText.StringReference = new("Basic", "Some battle royales this week are not reserved.");
        }
        else
        {
            checkBattleRoyaleTrue.SetActive(true);
            checkBattleRoyaleFalse.SetActive(false);
            checkBattleRoyaleText.StringReference = new("Basic", "All battle royales reserved for this week.");
        }
        checkBattleRoyaleText.RefreshString();
        GameManager.Instance.FixLayout(checkBattleRoyaleText.GetComponent<RectTransform>());
    }
    #endregion

    public SurvivorData CreateRandomSurvivorData()
    {
        League league = calendar.LeagueReserveInfo[calendar.Today].league;
        int value = league switch
        {
            League.BronzeLeague => 1,
            League.SilverLeague => 2,
            League.GoldLeague => 3,
            League.SeasonChampionship => 4,
            League.WorldChampionship => 5,
            _ => 4
        };
        int check = 0;
        while (check < 1000)
        {
            int randStrength = UnityEngine.Random.Range(0, 101);
            int randAgility = UnityEngine.Random.Range(0, 101);
            int randFighting = UnityEngine.Random.Range(0, 101);
            int randShooting = UnityEngine.Random.Range(0, 101);
            int randKnowledge = UnityEngine.Random.Range(0, 101);
            int totalRand = randStrength + randAgility + randShooting + randShooting + randKnowledge;
            if ((totalRand < value * 70 || totalRand > value * 130) && check < 1000)
            {
                check++;
                continue;
            }
            if (check >= 100) Debug.LogWarning("Infinite roof has detected");
            SurvivorData survivorData = new(
                GetRandomName(),
                randStrength,
                randAgility,
                randFighting,
                randShooting,
                randKnowledge,
                totalRand,
                calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league)
                );
            int characteristicCount;
            float randCharCount = UnityEngine.Random.Range(0, 1f);
            if (randCharCount < 0.33f) characteristicCount = 0;
            else if (randCharCount < 0.66f) characteristicCount = 1;
            else if (randCharCount < 0.9f) characteristicCount = 2;
            else characteristicCount = 3;
            CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicCount);
            return survivorData;
        }
        return new(GetRandomName(), 20, 20, 20, 20, 20, 100, calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league));
    }

    public void OpenConfirmWindow(string key, UnityAction wantAction, params string[] vars)
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(wantAction);
        confirmButton.onClick.AddListener(()=>confirmCanvas.SetActive(false));

        var localizedString = new LocalizedString("Basic", key);
        switch (vars.Length)
        {
            case 0:
                break;
            case 1:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0] } };
                break;
            case 2:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0], param1 = vars[1] } };
                break;
            case 3:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0], param1 = vars[1], param2 = vars[2] } };
                break;
            default:
                Debug.Log("To many params");
                break;
        }
        LocalizeStringEvent localizeStringEvent = confirmText.GetComponentInChildren<LocalizeStringEvent>();
        localizeStringEvent.StringReference = localizedString;
        localizeStringEvent.RefreshString();
        //confirmText.text = wantText;
        confirmCanvas.SetActive(true);
        GameManager.Instance.openedWindows.Push(confirmCanvas);
    }

    public void Alert(string key, params string[] vars)
    {
        GameObject alertWindow = PoolManager.Spawn(ResourceEnum.Prefab.Alert, alertCanvas.transform);
        //alertWindow.GetComponentInChildren<TextMeshProUGUI>().text = message;
        var localizedString = new LocalizedString("Basic", key);
        switch (vars.Length)
        {
            case 0:
                break;
            case 1:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0] } };
                break;
            case 2:
                localizedString.Arguments
                    = new[] { new { param0 = vars[0], param1 = vars[1] } };
                break;
            default:
                Debug.Log("To many params");
                break;
        }
        LocalizeStringEvent localizeStringEvent = alertWindow.GetComponentInChildren<LocalizeStringEvent>();
        localizeStringEvent.StringReference = localizedString;
        localizeStringEvent.RefreshString();
        GameManager.Instance.openedWindows.Push(alertWindow);
    }

    public void DebugLog(string log)
    {
        GameObject alertWindow = PoolManager.Spawn(ResourceEnum.Prefab.Alert, alertCanvas.transform);
        LocalizeStringEvent localizeStringEvent = alertWindow.GetComponentInChildren<LocalizeStringEvent>();
        localizeStringEvent.enabled = false;
        localizeStringEvent.GetComponent<TextMeshProUGUI>().text = log;
        GameManager.Instance.openedWindows.Push(alertWindow);
    }

    void OnClick(InputValue value)
    {
        List<RaycastResult> results = Raycast();
        if(value.Get<float>() > 0)
        {
            isClicked = true;

            int index = results.FindIndex(x => x.gameObject.CompareTag("ContestantUI"));
            if(index > -1)
            {
                for (int i = 0; i < contestants.Length; i++)
                {
                    int jndex = contestantsData.FindIndex(x => x.localizedSurvivorName.GetLocalizedString() == results[index].gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
                    if (jndex > -1) selectedContestantData = contestantsData[jndex];
                }
                if(selectedContestantData != null)
                {
                    selectedContestant.GetComponentInChildren<SurvivorInfo>().SetInfo(selectedContestantData, false);
                    selectedContestant.SetActive(true);
                    draggingContestant.SetActive(true);
                    draggingContestant.GetComponentsInChildren<Image>()[1].color = results[index].gameObject.GetComponentsInChildren<Image>()[1].color;
                    draggingContestant.GetComponentInChildren<LocalizeStringEvent>().StringReference = selectedContestantData.localizedSurvivorName;
                }
            }
        }
        else
        {
            isClicked = false;

            // 뗏을 때 Prediction 위면 prediction 저장
            int index = results.FindIndex(x => x.gameObject.CompareTag("PredictionContestantUI"));
            if(index > -1 && draggingContestant.activeSelf)
            {
                for(int i = 0; i < predictRankings.Length; i++)
                {
                    if (results[index].gameObject == predictRankings[i])
                    {
                        int alreadyPredicted = -1;
                        // prediction 할 때 이미 할당된 녀석이면 위치 바꿔주기
                        for(int j = 0; j < predictRankings.Length; j++)
                        {
                            if (predictRankingContestants[j].GetComponentInChildren<LocalizeStringEvent>().StringReference == selectedContestantData.localizedSurvivorName)
                            {
                                alreadyPredicted = j;
                                if (predictRankingContestants[i].activeSelf)
                                {
                                    predictRankingContestants[j].SetActive(true);
                                    predictRankingContestants[j].GetComponentsInChildren<Image>()[1].color = predictRankingContestants[i].GetComponentsInChildren<Image>()[1].color;
                                    predictRankingContestants[j].GetComponentInChildren<LocalizeStringEvent>().StringReference = predictRankingContestants[i].GetComponentInChildren<LocalizeStringEvent>().StringReference;
                                }
                                else predictRankingContestants[j].SetActive(false);
                            }
                        }

                        predictRankingContestants[i].SetActive(true);
                        predictRankingContestants[i].GetComponentsInChildren<Image>()[1].color = draggingContestant.GetComponentsInChildren<Image>()[1].color;
                        predictRankingContestants[i].GetComponentInChildren<LocalizeStringEvent>().StringReference = selectedContestantData.localizedSurvivorName;
                    }
                }
            }

            selectedContestantData = null;
            draggingContestant.SetActive(false);
        }
    }

    List<RaycastResult> Raycast()
    {
        PointerEventData pointerData = new(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        outCanvasRaycaster.Raycast(pointerData, results);
        return results;
    }

    public IEnumerator LoadMySurvivorData(List<SurvivorData> data)
    {
        GameManager.ClaimLoadInfo("Loading survivors...", 0, 3);
        mySurvivorsData.Clear();
        mySurvivorsData = data;
        AssignTraining();
        yield return null;
    }

    public void LoadData(int money, int mySurvivorsId, int survivorHireLimit, int fightTrainingLevel, int shootingTrainingLevel,
        int runningLevel, int weightTrainingLevel, int studyingLevel, List<SurvivorData> contestantsData)
    {
        Money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.survivorHireLimit = survivorHireLimit;
        this.fightTrainingLevel = fightTrainingLevel;
        this.shootingTrainingLevel = shootingTrainingLevel;
        this.runningLevel = runningLevel;
        this.weightTrainingLevel = weightTrainingLevel;
        this.studyingLevel = studyingLevel;
        this.contestantsData = contestantsData;
    }

    void OnLocaleChanged(Locale newLocale)
    {
        ChecklistTraining();
        if (survivorsDropdown.options.Count == 0) return;
        RelocalizeTrainingRoom();
        RelocalizeStrategyRoom();
        ResetSurvivorsDropdown();
        if (survivorsDropdown.options.Count == 0) return;
        AssignTraining();
        SetOperatingRoom();
        SetStrategyRoom();
        sortContestantsListDropdown.RelocalizeOptions();
    }
}

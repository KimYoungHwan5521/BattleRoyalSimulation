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

public enum GameMode
{
    SingleCareerRun,
    FreeManagement,
}

public enum SurgeryType 
{ 
    ArtificialPartTransplant,
    AugmentedPartTransplant,
    TrancendantPartTransplant,
    ChronicDisorderTreatment,
    RecoverySerumAdministeration,
}

public class OutGameUIManager : MonoBehaviour
{
    #region Variables and Properties
    [Header("Components")]
    [SerializeField] Canvas canvas;
    [SerializeField] GraphicRaycaster outCanvasRaycaster;
    [SerializeField] GraphicRaycaster championshipCanvasRaycaster;
    EventSystem eventSystem;
    bool isClicked;
    [SerializeField] GameObject survivorsList;
    [SerializeField] GameObject leaguePoint;
    [SerializeField] GameObject stamina;
    [SerializeField] GameObject dismissSurvivor;
    [SerializeField] GameObject hireSurvivorBtn;

    [Header("Confirm / Alert")]
    [SerializeField] GameObject confirmCanvas;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmButton;
    [SerializeField] GameObject alertCanvas;

    [Header("Global")]
    [SerializeField] GameMode gameMode;
    public GameMode GameMode => gameMode;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] int money;
    public int Money
    {
        get { return money; }
        set
        {
            if (value > 99999999) value = 99999999;
            money = value;
            if (money < 0) moneyText.text = $"<color=red>{money:###,###,###,##0}</color>";
            else moneyText.text = $"{money:###,###,###,##0}";

            if (value >= 100000) AchievementManager.UnlockAchievement("Hundred-Thousandaire");
        }
    }
    int difficulty;
    public int Difficulty
    {
        get => difficulty;
        set
        {
            difficulty = value;
            string text = value switch
            {
                0 => "Normal",
                1 => "Hard",
                2 => "Very Hard",
                3 => "Expert",
                4 => "Hardcore",
                5 => "Nightmare",
                6 or _ => "Hell",
            };
            difficultyText.StringReference = new("Basic", text);
            difficultyText.GetComponent<TextMeshProUGUI>().color = value switch
            {
                0 => new(0, 1, 0),
                1 => new(1, 1, 0),
                2 => new(1, 0.75f, 0),
                3 => new(1, 0.5f, 0),
                4 => new(1, 0.25f, 0),
                5 => new(1, 0, 0),
                6 or _ => new(0.75f, 0, 0)
            };
        }
    }
    [SerializeField] LocalizeStringEvent difficultyText;
    [SerializeField] GameObject objective;
    public TextMeshProUGUI objectiveText;
    bool championship;
    public bool Championship => championship;
    public int championshipHeldCount;
    
    [Serializable]
    public class ChampionshipData
    {
        public LocalizedString SurvivorName;
        public List<int> points = new();
        public List<int> killPoints = new();
        public int TotalPoint
        {
            get
            {
                int result = 0;
                foreach (int point in points) result += point;
                return result;
            }
        }
        public int TotalKillPoint
        {
            get
            {
                int result = 0;
                foreach(int point in killPoints) result += point;
                return result;
            }
        }
        public int beforeRank;
        public int currentRank;

        public ChampionshipData(SurvivorData survivor)
        {
            SurvivorName = survivor.localizedSurvivorName;
        }
    }
    public List<ChampionshipData> championshipDatas;

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
    public Sprite[] rankSprites;
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
    [Header("Single Career Run")]
    [SerializeField] TextMeshProUGUI trainingRoomLevelText;
    const float trainingGreatSuccessRate = 0.1f;
    public int trainingLevel = 1;
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] GameObject trainingResult;
    [SerializeField] TextMeshProUGUI trainingResultText;
    [SerializeField] TextMeshProUGUI trainingResultDetailText;
    public int Stamina
    {
        get
        {
            return mySurvivorsData.Count == 0 ? 0 : mySurvivorsData[0].Stamina;
        }
        set
        {
            mySurvivorsData[0].Stamina = value;
            selectedSurvivor.SetInfo(mySurvivorsData[0], true);
        }
    }
    [SerializeField] GameObject trainingRoomSingleCareerRun;
    [SerializeField] GameObject trainingRoomFreeManagement;
    public TrainingCard[] trainingCards;
    [SerializeField] TextMeshProUGUI currentFacilityLevelText;
    [SerializeField] Button upgradeFacilityButton;

    [Header("Free Management")]
    [SerializeField] GameObject trainingAssignForm;
    //[SerializeField] GameObject scheduledTrainingByEachSurvivor;
    [SerializeField] GameObject[] scheduledTrainings;
    [SerializeField] TextMeshProUGUI weightTrainingNameText;
    [SerializeField] TextMeshProUGUI runningNameText;
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI craftingTraningNameText;
    [SerializeField] TextMeshProUGUI studyingNameText;
    [SerializeField] TextMeshProUGUI weightTrainingExplain;
    [SerializeField] TextMeshProUGUI runningExplain;
    [SerializeField] TextMeshProUGUI fightingTrainingExplain;
    [SerializeField] TextMeshProUGUI shootingTrainingExplain;
    [SerializeField] TextMeshProUGUI craftingTrainingExplain;
    [SerializeField] TextMeshProUGUI studyExplain;
    int fightTrainingLevel = 1;
    int shootingTrainingLevel = 1;
    int craftingTrainingLevel = 1;
    int runningLevel = 1;
    int weightTrainingLevel = 1;
    int studyingLevel = 1;

    public int FightTrainingLevel => fightTrainingLevel;
    public int ShootingTrainingLevel => shootingTrainingLevel;
    public int CraftingTrainingLevel => craftingTrainingLevel;
    public int AgilityTrainingLevel => runningLevel;
    public int WeightTrainingLevel => weightTrainingLevel;
    public int StudyLevel => studyingLevel;
    readonly int[] facilityUpgradeCost = { 5000, 12000, 30000 };
    [SerializeField] GameObject weightTrainingUpgradeButton;
    [SerializeField] GameObject runningUpgradeButton;
    [SerializeField] GameObject fightTrainingUpgradeButton;
    [SerializeField] GameObject shootingTrainingUpgradeButton;
    [SerializeField] GameObject craftingTrainingUpgradeButton;
    [SerializeField] GameObject studyingUpgradeButton;
    [SerializeField] ScrollRect[] bookedTodayScrollRects;
    [SerializeField] TextMeshProUGUI weightTrainingBookers;
    [SerializeField] TextMeshProUGUI runningBookers;
    [SerializeField] TextMeshProUGUI fightTrainingBookers;
    [SerializeField] TextMeshProUGUI shootingTrainingBookers;
    [SerializeField] TextMeshProUGUI craftingTrainingBookers;
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

    [SerializeField] Strategy[] strategies;
    LocalizedDropdown weaponPriority1Dropdown;
    LocalizedDropdown weaponPriority2Dropdown;

    LocalizedDropdown craftingPriority1Dropdown;
    LocalizedDropdown craftingPriority2Dropdown;

    [SerializeField] Transform craftingAllow;
    public List<GameObject> craftableAllows = new();

    [SerializeField] TMP_InputField armorRepairConditionInputField;

    [Serializable]
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
    [SerializeField] List<SurgeryInfo> surgeryList;

    [Header("Schedule")]
    [SerializeField] GameObject scr_calanderObject;
    [SerializeField] GameObject fm_calanderObject;
    public GameObject CalendarObject => gameMode == GameMode.SingleCareerRun ? scr_calanderObject : fm_calanderObject;
    Calendar calendar;
    [SerializeField] SurvivorData mySurvivorDataInBattleRoyale;
    public SurvivorData MySurvivorDataInBattleRoyale
    {
        get
        {
            if (mySurvivorDataInBattleRoyale == null || mySurvivorDataInBattleRoyale.localizedSurvivorName == null) return null;
            else
            {
                return mySurvivorDataInBattleRoyale;
            }
        }
    }

    [Header("Daily Result")]
    [SerializeField] GameObject buttonEndTheDay;
    [SerializeField] GameObject buttonEndTheWeek;
    [SerializeField] GameObject dailyResult;
    [SerializeField] GameObject[] survivorTrainingResults;
    TextMeshProUGUI[][] resultTexts;
    [SerializeField] GameObject surgeryResult;
    [SerializeField] TextMeshProUGUI surgeryResultText;
    bool hadSurgery;
    List<LocalizedString> whoUnderwentSurgery = new();
    List<LocalizedString> performedSurgery = new();

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

    [SerializeField] GameObject mapInfo;
    [SerializeField] Image mapImage;
    [SerializeField] TextMeshProUGUI farmableItemsText;
    [SerializeField] ScrollRect farmableItemsScrollRect;

    [Header("Tutorial")]
    public Animator trainingRoomAnim;
    public Animator trainingRoomSurvivorAnim;
    public Animator scheduleAnim;
    public bool tutorial = false;

    [Header("Promote")]
    [SerializeField] GameObject promoteBG;
    [SerializeField] Image leaguePointBar;
    [SerializeField] TextMeshProUGUI leaguePointText;
    [SerializeField] TextMeshProUGUI leaguePointDetailText;
    [SerializeField] LocalizeStringEvent promotedText;
    [SerializeField] TextMeshProUGUI promoteDetailText;
    [SerializeField] Button promoteConfirmBtn;
    bool promoteAnimation;
    const float leaguePointIncreaseWait = 0.5f;
    float curLeaguePointIncreaseWait;
    const float leaguePointIncreaseTerm = 0.1f;
    float curLeaguePointIncreaseTerm;
    Tier beforeTier;

    [Header("Championship Rank")]
    [SerializeField] GameObject viewCurrentChampionshipStandings;
    public GameObject championshipRankBG;
    [SerializeField] LocalizeStringEvent championshipTitle;
    [SerializeField] GameObject championshipDescription;
    [SerializeField] GameObject[] championshipRanks;
    TextMeshProUGUI[] championshipRankRankChanges;
    TextMeshProUGUI[] championshipRankRanks;
    LocalizeStringEvent[] championshipRankNames;
    TextMeshProUGUI[] championshipRankDay1Points;
    TextMeshProUGUI[] championshipRankDay1PointDetails;
    TextMeshProUGUI[] championshipRankDay2Points;
    TextMeshProUGUI[] championshipRankDay2PointDetails;
    TextMeshProUGUI[] championshipRankDay3Points;
    TextMeshProUGUI[] championshipRankDay3PointDetails;
    TextMeshProUGUI[] championshipRankTotalPoints;
    TextMeshProUGUI[] championshipRankTotalPointDetails;

    [SerializeField] SurvivorInfo championshipSurvivorInfo;
    #endregion

    private void Awake()
    {
        weaponPriority1Dropdown = Array.Find(strategies, x => x.strategyCase == StrategyCase.WeaponPriority).ActionDropdown;
        weaponPriority2Dropdown = Array.Find(strategies, x => x.strategyCase == StrategyCase.WeaponPriority).ElseActionDropdown;
        craftingPriority1Dropdown = Array.Find(strategies, x => x.strategyCase == StrategyCase.CraftingPriority).ActionDropdown;
        craftingPriority2Dropdown = Array.Find(strategies, x => x.strategyCase == StrategyCase.CraftingPriority).ElseActionDropdown;
    }

    private void Start()
    {
        calendar = GetComponent<Calendar>();
        mySurvivorsData = new();
        //SetHireMarketFirst();
        hireClose.SetActive(false);
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
            new("Basic", "Crafting"),
            new("Basic", "Knowledge"),
            new("Basic", "Stat Total"),
        });


        championshipRankRankChanges = new TextMeshProUGUI[25];
        championshipRankRanks = new TextMeshProUGUI[25];
        championshipRankNames = new LocalizeStringEvent[25];
        championshipRankDay1Points = new TextMeshProUGUI[25];
        championshipRankDay1PointDetails = new TextMeshProUGUI[25];
        championshipRankDay2Points = new TextMeshProUGUI[25];
        championshipRankDay2PointDetails = new TextMeshProUGUI[25];
        championshipRankDay3Points = new TextMeshProUGUI[25];
        championshipRankDay3PointDetails = new TextMeshProUGUI[25];
        championshipRankTotalPoints = new TextMeshProUGUI[25];
        championshipRankTotalPointDetails = new TextMeshProUGUI[25];
        for (int i=0; i<championshipRanks.Length; i++)
        {
            TextMeshProUGUI[] tmps = championshipRanks[i].GetComponentsInChildren<TextMeshProUGUI>(true);
            championshipRankRankChanges[i] = tmps[0];
            championshipRankRanks[i] = tmps[1];
            championshipRankNames[i] = tmps[2].GetComponent<LocalizeStringEvent>();
            championshipRankDay1Points[i] = tmps[3];
            championshipRankDay1PointDetails[i] = tmps[4];
            championshipRankDay2Points[i] = tmps[5];
            championshipRankDay2PointDetails[i] = tmps[6];
            championshipRankDay3Points[i] = tmps[7];
            championshipRankDay3PointDetails[i] = tmps[8];
            championshipRankTotalPoints[i] = tmps[9];
            championshipRankTotalPointDetails[i] = tmps[10];
        }

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
                            if ((injury.degree > 0 && injury.type != InjuryType.ArtificialPartsDamaged && injury.type != InjuryType.AugmentedPartsDamaged && injury.type != InjuryType.TranscendantPartsDamaged) || injury.degree >= 1)
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

    private void Update()
    {
        if (isClicked)
        {
            if (selectedContestantData != null)
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

        if (promoteAnimation && MySurvivorsData != null && MySurvivorsData.Count > 0)
        {
            curLeaguePointIncreaseWait += Time.unscaledDeltaTime;
            if (curLeaguePointIncreaseWait > leaguePointIncreaseWait)
            {
                curLeaguePointIncreaseTerm += Time.unscaledDeltaTime;
                if (curLeaguePointIncreaseTerm > leaguePointIncreaseTerm)
                {
                    if (mySurvivorsData[0].increaseComparedToPrevious_promotePoint > 0) { mySurvivorsData[0].promotePoint++; mySurvivorsData[0].increaseComparedToPrevious_promotePoint--; }
                    leaguePointText.text = $"{mySurvivorsData[0].promotePoint} / 100";
                    leaguePointDetailText.text = $"{mySurvivorsData[0].promotePoint_Rank} + {mySurvivorsData[0].promotePoint_Kill} ({new LocalizedString("Basic", "Rank").GetLocalizedString()} + {new LocalizedString("Basic", "Kill").GetLocalizedString()})";
                    leaguePointBar.fillAmount = mySurvivorsData[0].promotePoint / 100f;
                    if (mySurvivorsData[0].increaseComparedToPrevious_promotePoint == 0 || mySurvivorsData[0].promotePoint >= 100)
                    {
                        promoteAnimation = false;
                        promoteConfirmBtn.interactable = true;
                        GameManager.Instance.openedWindows.Push(promoteBG);
                        if (mySurvivorsData[0].promotePoint >= 100)
                        {
                            mySurvivorsData[0].promotePoint = 0;
                            mySurvivorsData[0].increaseComparedToPrevious_promotePoint = 0;
                            promotedText.gameObject.SetActive(true);
                            promoteDetailText.gameObject.SetActive(true);
                            
                            if(beforeTier != Tier.Gold)
                            {
                                promotedText.StringReference = new LocalizedString("Basic", "Promoted!");
                                promoteDetailText.text = $"{new LocalizedString("Basic", beforeTier.ToString()).GetLocalizedString()} => {new LocalizedString("Basic", MySurvivorsData[0].tier.ToString()).GetLocalizedString()}";
                            }
                            else
                            {
                                mySurvivorsData[0].haveQualifyToParticipateInSeasonChampionship = true;
                                promotedText.StringReference = new LocalizedString("Basic", "Advanced to the Season Championship!");
                                promoteDetailText.text = $"";
                            }
                            SoundManager.PlayUISFX(ResourceEnum.SFX.Fanfare1);
                            ResetObjectiveText();
                            switch(beforeTier)
                            {
                                case Tier.Bronze:
                                    GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinBronzeLeague);
                                    break;
                                case Tier.Silver:
                                    GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinSilverLeague);
                                    break;
                                case Tier.Gold:
                                    GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinGoldLeague);
                                    break;
                            }
                        }
                        else
                        {
                            // 게임오버 체크 : 목표 달성 체크
                            if((calendar.Today > 21 && mySurvivorsData[0].tier == Tier.Bronze)
                                || (calendar.Today > 49 && mySurvivorsData[0].tier != Tier.Gold)
                                || (calendar.Today > 77 && !mySurvivorsData[0].haveQualifyToParticipateInSeasonChampionship))
                            {
                                GameResult gameResult = GameManager.Instance.GetComponent<GameResult>();
                                gameResult.gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
                                gameResult.GameOver();
                                return;
                            }
                        }
                        selectedSurvivor.SetInfo(mySurvivorsData[0], false);
                        GameManager.Instance.Save(0);
                    }
                }
            }
        }
    }

    public void ResetData(GameMode wantMode, int difficulty)
    {
        gameMode = wantMode;

        survivorsList.SetActive(gameMode == GameMode.FreeManagement);
        leaguePoint.SetActive(gameMode == GameMode.SingleCareerRun);
        stamina.SetActive(gameMode == GameMode.SingleCareerRun);
        dismissSurvivor.SetActive(gameMode == GameMode.FreeManagement);
        hireSurvivorBtn.SetActive(gameMode == GameMode.FreeManagement);
        buttonEndTheDay.SetActive(gameMode == GameMode.FreeManagement);
        buttonEndTheWeek.SetActive(gameMode == GameMode.FreeManagement);
        difficultyText.gameObject.SetActive(gameMode == GameMode.SingleCareerRun);

        //mySurvivorsData = new();
        hireSurvivor.SetActive(true);
        trainingLevel = 1;
        ResetHireMarket();
        Money = 1000;
        survivorHireLimit = 10;
        selectedSurvivor.ResetInfo();
        Difficulty = difficulty;
        objective.SetActive(false);
        objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective1").GetLocalizedString()}";

        tutorial = true;
        promoteAnimation = false;
        championship = false;
        championshipHeldCount = 0;
        championshipDatas = new();
        viewCurrentChampionshipStandings.SetActive(false);
    }

    public void PromoteAnimation(League league)
    {
        promoteAnimation = true;
        promoteBG.SetActive(true);
        promotedText.gameObject.SetActive(false);
        promoteDetailText.gameObject.SetActive(false);
        promoteConfirmBtn.interactable = false;
        beforeTier = league switch
        { 
            League.BronzeLeague => Tier.Bronze,
            League.SilverLeague => Tier.Silver,
            League.GoldLeague or _ => Tier.Gold,
        };
    }

    void RelocalizeTrainingRoom()
    {
        if(gameMode == GameMode.SingleCareerRun)
        {
            currentFacilityLevelText.text = new LocalizedString("Basic", "Current Facility Level")
            {
                Arguments = new[] { $"{trainingLevel}" }
            }.GetLocalizedString();
            upgradeFacilityButton.GetComponentInChildren<TextMeshProUGUI>().text = new LocalizedString("Basic", "Facility Upgrade")
            {
                Arguments = new[] { trainingLevel > facilityUpgradeCost.Length || trainingLevel <= 0 ? "" : $"{facilityUpgradeCost[trainingLevel - 1]}" }
            }.GetLocalizedString();
            trainingRoomLevelText.text = new LocalizedString("Basic", "Training Room Level") { Arguments = new[] { $"{trainingLevel}" } }.GetLocalizedString();
        }
        else
        {
            SetTrainingName(weightTrainingNameText, "Training:Weight", weightTrainingLevel);
            SetTrainingName(runningNameText, "Training:Running", runningLevel);
            SetTrainingName(fightTrainingNameText, "Training:Fighting", fightTrainingLevel);
            SetTrainingName(shootingTraningNameText, "Training:Shooting", shootingTrainingLevel);
            SetTrainingName(craftingTraningNameText, "Training:Crafting", craftingTrainingLevel);
            SetTrainingName(studyingNameText, "Training:Studying", studyingLevel);

            weightTrainingUpgradeButton.SetActive(weightTrainingLevel < facilityUpgradeCost.Length + 1);
            runningUpgradeButton.SetActive(runningLevel < facilityUpgradeCost.Length + 1);
            fightTrainingUpgradeButton.SetActive(fightTrainingLevel < facilityUpgradeCost.Length + 1);
            shootingTrainingUpgradeButton.SetActive(shootingTrainingLevel < facilityUpgradeCost.Length + 1);
            craftingTrainingUpgradeButton.SetActive(craftingTrainingLevel < facilityUpgradeCost.Length + 1);
            studyingUpgradeButton.SetActive(studyingLevel < facilityUpgradeCost.Length + 1);

            weightTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[weightTrainingLevel - 1] };
            runningUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[runningLevel - 1] };
            fightTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[fightTrainingLevel - 1] };
            shootingTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[shootingTrainingLevel - 1] };
            craftingTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[CraftingTrainingLevel - 1] };
            studyingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().StringReference.Arguments = new object[] { facilityUpgradeCost[studyingLevel - 1] };

            weightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            runningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            fightTrainingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            shootingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            craftingTraningNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            studyingNameText.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            weightTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            runningUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            fightTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            shootingTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            craftingTrainingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();
            studyingUpgradeButton.GetComponentInChildren<LocalizeStringEvent>().RefreshString();

            weightTrainingExplain.text = $"{new LocalizedString("Basic", "Strength").GetLocalizedString()}+";
            runningExplain.text = $"{new LocalizedString("Basic", "Agility").GetLocalizedString()}+";
            fightingTrainingExplain.text = $"{new LocalizedString("Basic", "Fighting").GetLocalizedString()}+";
            shootingTrainingExplain.text = $"{new LocalizedString("Basic", "Shooting").GetLocalizedString()}+";
            craftingTrainingExplain.text = $"{new LocalizedString("Basic", "Crafting").GetLocalizedString()}+";
            studyExplain.text = $"{new LocalizedString("Basic", "Knowledge").GetLocalizedString()}+";
        }
    }

    void SetTrainingName(TextMeshProUGUI target, string trainingKey, int level)
    {
        var trainingType = new LocalizedString("Basic", trainingKey);
        var stringEvent = target.GetComponentInChildren<LocalizeStringEvent>(true);

        stringEvent.StringReference.Arguments = new object[]
        {
            trainingType.GetLocalizedString(),
            level
        };

        stringEvent.RefreshString();
    }

    #region Hire
    public void ResetHireMarket()
    {
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            int randStrength = UnityEngine.Random.Range(0, 101);
            int randAgility = UnityEngine.Random.Range(0, 101);
            int randFighting = UnityEngine.Random.Range(0, 101);
            int randShooting = UnityEngine.Random.Range(0, 101);
            int randCrafting = UnityEngine.Random.Range(0, 101);
            int randKnowledge = UnityEngine.Random.Range(0, 101);
            int totalRand = randStrength + randAgility + randFighting + randShooting + randCrafting + randKnowledge;
            if ((totalRand < 120 || totalRand > 140) && check < 10000)
            {
                i--;
                check++;
                if (check >= 10000) Debug.LogWarning("Infinite loop detected");
                continue;
            }
            check = 0;

            int characteristicCount;
            float randCharCount = UnityEngine.Random.Range(0, 1f);
            if (randCharCount < 0.25f) characteristicCount = 1;
            else if (randCharCount < 0.75f) characteristicCount = 2;
            else characteristicCount = 3;

            survivorsInHireMarket[i].SetInfo(GetRandomName(),
                randStrength,
                randAgility,
                randFighting,
                randShooting,
                randCrafting,
                randKnowledge,
                characteristicCount,
                totalRand,
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
        trainingRoomSingleCareerRun.SetActive(false);
        trainingRoomFreeManagement.SetActive(false);
        operatingRoom.SetActive(false);
        strategyRoom.SetActive(false);
        bettingRoom.SetActive(false);
    }

    public void HireSurvivor(int candidate)
    {
        if(gameMode == GameMode.SingleCareerRun)
        {
            Purchase(candidate);
        }
        else
        {
            OpenConfirmWindow($"Confirm:Purchase", () => Purchase(candidate), $"{survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString()}", $"{survivorsInHireMarket[candidate].survivorData.price}");
        }
    }

    void Purchase(int candidate)
    {
        if (mySurvivorsData.Count >= survivorHireLimit)
        {
            Alert("Alert:Survivor limit reached.");
        }
        else if (gameMode == GameMode.FreeManagement && money < survivorsInHireMarket[candidate].survivorData.price)
        {
            Alert("Alert:Not enough money.");
        }
        else
        {
            if(gameMode == GameMode.FreeManagement) Money -= survivorsInHireMarket[candidate].survivorData.price;
            mySurvivorsData.Add(new(survivorsInHireMarket[candidate].survivorData));
            mySurvivorsData[^1].id = mySurvivorsId++;
            mySurvivorsData[^1].characteristics = survivorsInHireMarket[candidate].survivorData.characteristics;
            mySurvivorsData[0]._stamina = mySurvivorsData[0].MaxStamina;
            //mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;
            survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";

            if (mySurvivorsData.Count == 1)
            {
                survivorsDropdown.ClearOptions();
                selectedSurvivor.SetInfo(mySurvivorsData[0], true);
                GameManager.Instance.LoadStrategy(0);
                trainingRoomAnim.SetBool("Tutorial", true);
                Alert("Click the Training Room and assign survivors to training.");
                if(gameMode == GameMode.SingleCareerRun) objective.SetActive(true);
            }
            else if (mySurvivorsData.Count >= 10)
            {
                AchievementManager.UnlockAchievement("Full House");
            }
            survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString() });
            survivorsInHireMarket[candidate].SoldOut = true;

            //if (mySurvivorsData.Count == 1) ResetHireMarket();
            hireSurvivor.SetActive(false);
            if(gameMode == GameMode.SingleCareerRun)
            {
                ResetTrainingRoom();
                GameManager.Instance.Save(0);
            }
            GameManager.Instance.Option.SetSaveButtonInteractable(true, true);
        }
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
        if (mySurvivorsData == null || mySurvivorsData.Count == 0) return;
        selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value], true);
        selectedSurvivor.StatIncreaseAnimation();
    }

    public void Rest()
    {
        if(GameMode == GameMode.SingleCareerRun)
        {
            if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
            {
                Alert("Alert:Today is a Battle Royale match day.");
                return;
            }
            
            OpenConfirmWindow("Confirm:Rest", () =>
            {
                int value;
                float rand = UnityEngine.Random.Range(0, 1f);
                if (rand < 0.2f) value = 30;
                else if (rand < 0.8f) value = 50;
                else value = 70;

                if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.FastRecharge)) value = mySurvivorsData[0].MaxStamina;
                else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Tireless)) value = (int)(value * 1.1f);
                else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.IronMan)) value = (int)(value * 1.2f);

                bool overRest = value > mySurvivorsData[0].MaxStamina - mySurvivorsData[0].Stamina;
                value = Mathf.Min(value, mySurvivorsData[0].MaxStamina - mySurvivorsData[0].Stamina);
                mySurvivorsData[0].StaminaConsomtionReserve(value);
                trainingResult.SetActive(true);
                resultText.gameObject.SetActive(false);
                trainingResultText.text = new LocalizedString("Basic", "Rest").GetLocalizedString();

                if(overRest)
                {
                    trainingResultDetailText.text = $"{new LocalizedString("Basic", "Alert:OverRest").GetLocalizedString()}\n\n";

                    int[] randStat = new int[6];
                    randStat[UnityEngine.Random.Range(0, 6)]++;
                    mySurvivorsData[0].IncreaseStatsReserve(randStat[0], randStat[1], randStat[2], randStat[3], randStat[4], randStat[5]);
                    for (int i = 0; i < 6; i++)
                    {
                        if (randStat[i] > 0)
                        {
                            trainingResultDetailText.text += i switch
                            {
                                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                                _ => new LocalizedString("Basic", "Unknown").GetLocalizedString(),
                            };
                            trainingResultDetailText.text += $" + {randStat[i]}";
                        }
                    }
                    trainingResultDetailText.text += $"\n{new LocalizedString("Basic", "Stamina").GetLocalizedString()} <color=#367D38>+{value}</color>";
                }
                else if (value == 30)
                {
                    trainingResultDetailText.text = $"{new LocalizedString("Basic", "Alert:Rest30").GetLocalizedString()}\n\n";

                    int[] randStat = new int[6];
                    for (int i = 0; i < 2; i++)
                    {
                        randStat[UnityEngine.Random.Range(0, 6)]++;
                    }
                    mySurvivorsData[0].IncreaseStatsReserve(randStat[0], randStat[1], randStat[2], randStat[3], randStat[4], randStat[5]);
                    bool first = true;
                    for (int i = 0; i < 6; i++)
                    {
                        if (randStat[i] > 0)
                        {
                            if (!first) trainingResultDetailText.text += ", ";
                            trainingResultDetailText.text += i switch
                            {
                                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                                _ => new LocalizedString("Basic", "Unknown").GetLocalizedString(),
                            };
                            trainingResultDetailText.text += $" + {randStat[i]}";
                            first = false;
                        }
                    }
                }
                else
                {
                    trainingResultDetailText.text = $"{new LocalizedString("Basic", "Stamina").GetLocalizedString()} <color=#367D38>+{value}</color>";
                }
                selectedSurvivor.StatIncreaseAnimation();
                GameManager.Instance.openedWindows.Push(trainingResult);
                DayEnd();
            });

        }
    }

    #region Training : Single Career Run
    public void OpenTrainingRoom()
    {
        if(GameMode == GameMode.SingleCareerRun)
        {
            if(calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
            {
                Alert("Alert:Today is a Battle Royale match day.");
                return;
            }

            if (CheckHaveInjury(out int expectedDateOfFullyRecovery))
            {
                Alert("Alert:Can't Training", mySurvivorsData[0].localizedSurvivorName.GetLocalizedString(), $"{expectedDateOfFullyRecovery}");
            }
            else
            {
                trainingRoomSingleCareerRun.SetActive(true);
                RelocalizeTrainingRoom();
                foreach (var trainingCard in trainingCards) trainingCard.RecalculateFailRate();
                GameManager.Instance.openedWindows.Push(trainingRoomSingleCareerRun);
            }
        }
        else
        {
            RelocalizeTrainingRoom();
            trainingRoomFreeManagement.SetActive(true);
            GameManager.Instance.openedWindows.Push(trainingRoomFreeManagement);
            SetTrainingRoomSurvivorsInfo();
            //if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
            //{
            //    if ((calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship || calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            //    {
            //        Alert("Alert:Join Championship");
            //        return;
            //    }
            //    else if (calendar.Today > 77 && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            //    {
            //        Alert("Alert:Last Week Join League");
            //        return;
            //    }
            //    else if ((calendar.Today == 24 || calendar.Today == 53) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            //    {
            //        Alert("Alert:Last Chance For Objective");
            //        return;
            //    }
            //}
        }
    }

    public bool CheckHaveInjury(out int expectedDateOfFullyRecovery)
    {
        expectedDateOfFullyRecovery = 0;
        float mostDegree = 0;
        foreach(var injury in mySurvivorsData[0].injuries)
        {
            if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged || injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged
                || injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged)
                continue;
            if(injury.degree > mostDegree)
            {
                mostDegree = injury.degree;
            }
        }
        if (mostDegree == 0) return false;

        float recovery = 0;
        float recoveryRate = 1;
        if (mySurvivorsData[0].characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) recoveryRate *= 1.5f;
        else if (mySurvivorsData[0].characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) recoveryRate *= 0.7f;
        recovery = 0.2f * recoveryRate;
        expectedDateOfFullyRecovery = (int)(mostDegree / recovery) + 1;
        return true;
    }

    public void ResetTrainingRoom()
    {
        List<TrainingInfo> checkDup = new();
        foreach(var trainingCard in trainingCards)
        {
            TrainingInfo training = null;
            for (int i = 0; i < 1000; i++)
            {
                training = TrainingManager.GetRandomTraining(trainingLevel);
                if (checkDup.FindIndex(x => x.trainingName.TableEntryReference.Key == training.trainingName.TableEntryReference.Key) == -1) break;
            }
            // A ?? B: A가 null이면 B 아니면 A
            trainingCard.SetCard(training ?? TrainingManager.Trainings[0]);
            checkDup.Add(training);
        }
    }

    int focusedTraining;
    public void FocusTraining(int index)
    {
        focusedTraining = index;
        for (int i = 0; i < trainingCards.Length; i++)
        {
            trainingCards[i].Select(i == index);
        }
    }

    public void SelectTraining()
    {
        trainingRoomAnim.SetBool("Tutorial", false);
        TrainingInfo training = trainingCards[focusedTraining].LinkedTraining;
        float failRate = 0;
        if (Stamina < training.staminaConsumtion) failRate = 1f;
        else if (Stamina < training.trainingDifficulty) failRate = 1f - (float)Stamina / training.trainingDifficulty;
        //Debug.Log($"Stamina : {Stamina}, consumtion : {training.staminaConsumtion}, difficulty : {training.trainingDifficulty} | failRate : {failRate}");
        float rand = UnityEngine.Random.Range(0, 1f);
        trainingResult.SetActive(true);
        resultText.gameObject.SetActive(true);
        resultText.GetComponent<LocalizeStringEvent>().StringReference = new LocalizedString("Basic", "Training Results");
        if(rand < failRate)
        {
            // 실패
            trainingResultText.text = new LocalizedString("Basic", "Failed").GetLocalizedString();
            trainingResultDetailText.text = "";
            StaminaConsume(training);
            selectedSurvivor.StatIncreaseAnimation();
            SoundManager.PlayUISFX(ResourceEnum.SFX.Fail);
        }
        else if (rand > 1f - trainingGreatSuccessRate)
        {
            // 대성공
            trainingResultText.text = new LocalizedString("Basic", "Great Success").GetLocalizedString();
            trainingResultDetailText.text = training.GetTrainingExplain(true);
            ApplyTrainingResult(training, true);
            SoundManager.PlayUISFX(ResourceEnum.SFX.Fanfare2);
        }
        else
        {
            // 성공
            trainingResultText.text = new LocalizedString("Basic", "Success").GetLocalizedString();
            trainingResultDetailText.text = training.GetTrainingExplain(false);
            ApplyTrainingResult(training, false);
            SoundManager.PlayUISFX(ResourceEnum.SFX.Fanfare1);
        }
        mySurvivorsData[0].receivedTrainings++;
        //foreach (var card in trainingCards) card.SetCard(card.LinkedTraining);
        trainingRoomSingleCareerRun.SetActive(false);

        GameManager.Instance.openedWindows.Push(trainingResult);
        DayEnd();
    }

    void ApplyTrainingResult(TrainingInfo training, bool greatSuccess)
    {
        bool first = true;
        StaminaConsume(training);
        foreach(var value in training.increaseStats)
        {
            int total = value.value;
            if(first)
            {
                first = false;
                if (greatSuccess) total += training.rarity == TrainingRarity.Common ? 1 : training.rarity == TrainingRarity.Uncommon ? 2 : 4;
                if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Overzealous) || mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Gifted) 
                    || mySurvivorsData[0].HaveCharacteristic(CharacteristicType.FastLearner) && UnityEngine.Random.Range(0, 1f) < 0.5f) total += training.rarity == TrainingRarity.Common ? 1 : training.rarity == TrainingRarity.Uncommon ? 2 : 4;
            }
            switch(value.statType)
            {
                case 0:
                    mySurvivorsData[0].IncreaseStatsReserve(total, 0, 0, 0, 0, 0);
                    break;
                case 1:
                    mySurvivorsData[0].IncreaseStatsReserve(0, total, 0, 0, 0, 0);
                    break;
                case 2:
                    mySurvivorsData[0].IncreaseStatsReserve(0, 0, total, 0, 0, 0);
                    break;
                case 3:
                    mySurvivorsData[0].IncreaseStatsReserve(0, 0, 0, total, 0, 0);
                    break;
                case 4:
                    mySurvivorsData[0].IncreaseStatsReserve(0, 0, 0, 0, total, 0);
                    break;
                case 5:
                    mySurvivorsData[0].IncreaseStatsReserve(0, 0, 0, 0, 0, total);
                    break;
                case 6:
                    int[] randStat = new int[6];
                    for(int i = 0; i < total; i++)
                    {
                        randStat[UnityEngine.Random.Range(0, 6)]++;
                    }
                    mySurvivorsData[0].IncreaseStatsReserve(randStat[0], randStat[1], randStat[2], randStat[3], randStat[4], randStat[5]);
                    trainingResultDetailText.text = "";
                    for(int i = 0; i < 5; i++)
                    {
                        if (randStat[i] > 0)
                        {
                            if (!string.IsNullOrEmpty(trainingResultDetailText.text)) trainingResultDetailText.text += ", ";
                            trainingResultDetailText.text += i switch
                            {
                                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                                _ => new LocalizedString("Basic", "Unknown").GetLocalizedString(),
                            };
                            trainingResultDetailText.text += $" + {randStat[i]}";
                        }
                    }
                    break;
                default:
                    Debug.LogError("Wrong increase stat index!");
                    break;
            }
        }
        selectedSurvivor.StatIncreaseAnimation();
    }

    void StaminaConsume(TrainingInfo training)
    {
        int staminaConsumtion = training.staminaConsumtion;
        if (staminaConsumtion > 0)
        {
            if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.EasilyExhausted)) staminaConsumtion = (int)(staminaConsumtion * 1.05f);
            else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Tireless)) staminaConsumtion = (int)(staminaConsumtion * 0.9f);
            else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.IronMan)) staminaConsumtion = (int)(staminaConsumtion * 0.8f);
            else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Overzealous)) staminaConsumtion = (int)(staminaConsumtion * 1.2f);
        }
        else
        {
            if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.Tireless)) staminaConsumtion = (int)(staminaConsumtion * 1.1f);
            else if (mySurvivorsData[0].HaveCharacteristic(CharacteristicType.IronMan)) staminaConsumtion = (int)(staminaConsumtion * 1.2f);
        }
        mySurvivorsData[0].StaminaConsomtionReserve(-staminaConsumtion);
    }

    public void UpgradeFacility()
    {
        trainingLevel++;
    }
    #endregion
    #region Training Room : Free Management
    public void OpenAssignTraining(int trainingIndex)
    {
        trainingAssignForm.SetActive(true);
        GameManager.Instance.openedWindows.Push(trainingAssignForm);
        Training_FreeManagement training = (Training_FreeManagement)trainingIndex;
        survivorSchedules = new();
        assignTrainingNameText.GetComponent<LocalizeStringEvent>().StringReference = new("Basic", $"Training:{training}");

        for (int i = survivorsAssignedThis.childCount - 1; i >= 0; i--)
        {
            PoolManager.Despawn(survivorsAssignedThis.GetChild(i).gameObject);
        }
        for (int i = survivorsWithoutSchedule.childCount - 1; i >= 0; i--)
        {
            PoolManager.Despawn(survivorsWithoutSchedule.GetChild(i).gameObject);
        }
        for (int i = survivorsWithOtherSchedule.childCount - 1; i >= 0; i--)
        {
            PoolManager.Despawn(survivorsWithOtherSchedule.GetChild(i).gameObject);
        }

        foreach (SurvivorData survivor in mySurvivorsData)
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
            else if (survivor.assignedTraining == Training_FreeManagement.None)
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
            if (!assignable) survivorSchedule.GetComponent<Help>().SetDescriptionWithKey(description, survivor.localizedSurvivorName.GetLocalizedString(), cause);
            else if (alreadyAssigned) survivorSchedule.GetComponent<Help>().SetDescriptionWithKey(description, survivor.localizedSurvivorName.GetLocalizedString(), targetTraining.GetLocalizedString());
            survivorSchedule.GetComponent<Button>().enabled = assignable;
            survivorSchedules.Add(survivorSchedule);
        }
    }

    public void SetTrainingRoomSurvivorsInfo()
    {
        for (int i = 0; i < scheduledTrainings.Length; i++)
        {
            if (i < mySurvivorsData.Count)
            {
                scheduledTrainings[i].SetActive(true);
                scheduledTrainings[i].GetComponent<SurvivorInfo>().SetInfo(mySurvivorsData[i], false);
            }
            else scheduledTrainings[i].SetActive(false);
        }
    }

    public void ConfirmAssignTraining()
    {
        foreach (var survivorSchedule in survivorSchedules)
        {
            survivorSchedule.survivor.assignedTraining = survivorSchedule.whereAmI;
        }
        AssignTraining();
        SetTrainingRoomSurvivorsInfo();
    }

    public void AssignTraining()
    {
        weightTrainingBookers.text = "";
        runningBookers.text = "";
        fightTrainingBookers.text = "";
        shootingTrainingBookers.text = "";
        craftingTrainingBookers.text = "";
        studyingBookers.text = "";
        foreach (SurvivorData survivor in mySurvivorsData)
        {
            TextMeshProUGUI targetText;
            switch (survivor.assignedTraining)
            {
                case Training_FreeManagement.Fighting:
                    targetText = fightTrainingBookers;
                    break;
                case Training_FreeManagement.Shooting:
                    targetText = shootingTrainingBookers;
                    break;
                case Training_FreeManagement.Crafting:
                    targetText = craftingTrainingBookers;
                    break;
                case Training_FreeManagement.Running:
                    targetText = runningBookers;
                    break;
                case Training_FreeManagement.Weight:
                    targetText = weightTrainingBookers;
                    break;
                case Training_FreeManagement.Studying:
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
        //ChecklistTraining();
    }

    public void CheckTrainable(SurvivorData survivor)
    {
        if (Trainable(survivor, survivor.assignedTraining, out string cause)) return;
        else
        {
            survivor.assignedTraining = Training_FreeManagement.None;
            AssignTraining();
            Alert("Alert:Training Assignment Cancelled", survivor.localizedSurvivorName.GetLocalizedString(), cause);
        }
    }

    bool TrainableAnything(SurvivorData survivor)
    {
        if (Trainable(survivor, Training_FreeManagement.Fighting)) return true;
        if (Trainable(survivor, Training_FreeManagement.Shooting)) return true;
        if (Trainable(survivor, Training_FreeManagement.Crafting)) return true;
        if (Trainable(survivor, Training_FreeManagement.Running)) return true;
        if (Trainable(survivor, Training_FreeManagement.Weight)) return true;
        return false;
    }

    public bool Trainable(SurvivorData survivor, Training_FreeManagement training)
    {
        return Trainable(survivor, training, out string cause);
    }

    bool Trainable(SurvivorData survivor, Training_FreeManagement training, out string cause)
    {
        if (survivor.surgeryScheduled)
        {
            cause = new LocalizedString("Basic", "Surgery scheduled").GetLocalizedString();
            return false;
        }

        int eyeInjury = 0;
        foreach (Injury injury in survivor.injuries)
        {
            if (injury.type == InjuryType.ArtificialPartsTransplanted || (injury.type == InjuryType.ArtificialPartsDamaged && injury.degree < 1)) continue;
            if (injury.type == InjuryType.AugmentedPartsTransplanted || (injury.type == InjuryType.AugmentedPartsDamaged && injury.degree < 1)) continue;
            if (injury.type == InjuryType.TranscendantPartsTransplanted || (injury.type == InjuryType.TranscendantPartsDamaged && injury.degree < 1)) continue;
            if (injury.degree < 0.1f) continue;
            switch (training)
            {
                case Training_FreeManagement.Fighting:
                    switch (injury.site)
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
                case Training_FreeManagement.Shooting:
                    switch (injury.site)
                    {
                        case InjurySite.Brain:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cause = $"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}";
                            return false;
                        case InjurySite.Organ:
                            if (injury.degree >= 1)
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
                case Training_FreeManagement.Crafting:
                    switch (injury.site)
                    {
                        case InjurySite.Brain:
                        case InjurySite.Organ:
                        case InjurySite.RightArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftArm:
                        case InjurySite.LeftHand:
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
                case Training_FreeManagement.Running:
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
                case Training_FreeManagement.Weight:
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
        int level;
        Action<int> setLevel;
        TextMeshProUGUI nameText;
        GameObject upgradeButton;

        switch (trainingRoomIndex)
        {
            case 1:
                level = weightTrainingLevel;
                setLevel = value => weightTrainingLevel = value;
                nameText = weightTrainingNameText;
                upgradeButton = weightTrainingUpgradeButton;
                break;

            case 2:
                level = runningLevel;
                setLevel = value => runningLevel = value;
                nameText = runningNameText;
                upgradeButton = runningUpgradeButton;
                break;

            case 3:
                level = fightTrainingLevel;
                setLevel = value => fightTrainingLevel = value;
                nameText = fightTrainingNameText;
                upgradeButton = fightTrainingUpgradeButton;
                break;

            case 4:
                level = shootingTrainingLevel;
                setLevel = value => shootingTrainingLevel = value;
                nameText = shootingTraningNameText;
                upgradeButton = shootingTrainingUpgradeButton;
                break;

            case 5:
                level = craftingTrainingLevel;
                setLevel = value => craftingTrainingLevel = value;
                nameText = craftingTraningNameText;
                upgradeButton = craftingTrainingUpgradeButton;
                break;

            case 6:
                level = studyingLevel;
                setLevel = value => studyingLevel = value;
                nameText = studyingNameText;
                upgradeButton = studyingUpgradeButton;
                break;

            default:
                Debug.LogError($"Invalid training room index: {trainingRoomIndex}");
                return;
        }

        var trainingType = new LocalizedString(
            "Basic",
            $"Training:{(Training_FreeManagement)(trainingRoomIndex + 1)}"
        );

        string localizedTrainingName = trainingType.GetLocalizedString();

        OpenConfirmWindow(
            "Confirm:Upgrade Facility",
            () =>
            {
                // 이미 최종 레벨인 경우 배열 접근 방지
                if (level > facilityUpgradeCost.Length)
                {
                    upgradeButton.SetActive(false);
                    return;
                }

                int upgradeCost = facilityUpgradeCost[level - 1];

                if (money < upgradeCost)
                {
                    Alert("Alert:Not enough money.");
                    return;
                }

                Money -= upgradeCost;

                level++;
                setLevel(level);

                LocalizeStringEvent nameStringEvent =
                    nameText.GetComponentInChildren<LocalizeStringEvent>(true);

                // 로컬라이징 문자열: {0} (lv {1})
                nameStringEvent.StringReference.Arguments = new object[]
                {
                    localizedTrainingName,
                    level
                };
                nameStringEvent.RefreshString();

                if (level > facilityUpgradeCost.Length)
                {
                    upgradeButton.SetActive(false);
                    return;
                }

                LocalizeStringEvent buttonStringEvent = upgradeButton.GetComponentInChildren<LocalizeStringEvent>(true);

                buttonStringEvent.StringReference.Arguments = new object[]
                {
                    facilityUpgradeCost[level - 1]
                };
                buttonStringEvent.RefreshString();
            },
            localizedTrainingName
        );
    }
    #endregion

    #region Operating Room
    public void OpenOperatingRoom()
    {
        //if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        //{
        //    if ((calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship || calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
        //    {
        //        Alert("Alert:Join Championship");
        //        return;
        //    }
        //    else if (calendar.Today > 77 && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
        //    {
        //        Alert("Alert:Last Week Join League");
        //        return;
        //    }
        //    else if ((calendar.Today == 24 || calendar.Today == 53) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
        //    {
        //        Alert("Alert:Last Chance For Objective");
        //        return;
        //    }
        //}

        operatingRoom.SetActive(true);
        SetOperatingRoom();
        GameManager.Instance.openedWindows.Push(operatingRoom);
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
                if(injury.degree >= 1 || injury.degree > 0 && (injury.type == InjuryType.ArtificialPartsDamaged || injury.type == InjuryType.AugmentedPartsDamaged || injury.type == InjuryType.TranscendantPartsDamaged))
                {
                    if(injury.degree < 1)
                    {
                        surgeryName = new LocalizedString("Injury", "Replace Prosthetic")
                        {
                            Arguments = new[] { new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString() }
                        };
                    }
                    else
                    {
                        surgeryName = new LocalizedString("Injury", "Prosthetic Implant")
                        {
                            Arguments = new[] { new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString() }
                        };
                    }
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
                            cost = 10;
                            break;
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cost = 100;
                            break;
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                            cost = 250;
                            break;
                        case InjurySite.RightFoot:
                        case InjurySite.LeftFoot:
                            cost = 100;
                            break;
                        case InjurySite.RightKnee:
                        case InjurySite.LeftKnee:
                            cost = 250;
                            break;
                        case InjurySite.RightLeg:
                        case InjurySite.LeftLeg:
                            cost = 500;
                            break;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            cost = 400;
                            break;
                        case InjurySite.RightEar:
                        case InjurySite.LeftEar:
                            cost = 100;
                            break;
                        case InjurySite.Organ:
                            cost = 600;
                            break;
                        default:
                            Debug.LogWarning($"Can't transplant site : {injury.site}");
                            break;
                    }
                    surgeryList.Add(new(surgeryName, cost, injury.site, SurgeryType.ArtificialPartTransplant));
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
        else if(surgeryType_Alteration.isOn)
        {
            AddTransplantSurgeryToSurgeryList(InjurySite.RightEye, 4000, 20000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftEye, 4000, 20000);
            AddTransplantSurgeryToSurgeryList(InjurySite.RightArm, 5000, 25000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftArm, 5000, 25000);
            AddTransplantSurgeryToSurgeryList(InjurySite.RightLeg, 15000, 75000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftLeg, 15000, 75000);
        }
        else if (surgeryType_Other_Treatments.isOn)
        {
            foreach (Injury injury in survivorWhoWantSurgery.injuries)
            {
                if (injury.degree > 0 && injury.degree < 1 && injury.type != InjuryType.ArtificialPartsDamaged && injury.type != InjuryType.AugmentedPartsDamaged && injury.type != InjuryType.TranscendantPartsDamaged)
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
                surgeries[i].GetComponent<Help>().SetDescription(surgeryList[i].surgeryType, surgeryList[i].surgerySite);
                surgeries[i].SetActive(true);
            }
            else
            {
                surgeries[i].SetActive(false);
            }
        }
        surgeries[0].GetComponentInChildren<Toggle>().isOn = true;
    }

    void AddTransplantSurgeryToSurgeryList(InjurySite wantSite, int wantCostAugmentBody, int wantCostTranscendantBody)
    {
        LocalizedString localizedSurgeryName;
        Injury injury;
        injury = survivorWhoWantSurgery.injuries.Find(x => x.site == wantSite);
        if (injury == null)
        {
            // 부상이 없는 경우 : 무조건 개조 선택지가 뜬다.
            localizedSurgeryName = new LocalizedString("Injury", "Augmented Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
            surgeryList.Add(new(localizedSurgeryName, wantCostAugmentBody, wantSite, SurgeryType.AugmentedPartTransplant));
            localizedSurgeryName = new LocalizedString("Injury", "Transcendant Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
            surgeryList.Add(new(localizedSurgeryName, wantCostTranscendantBody, wantSite, SurgeryType.TrancendantPartTransplant));
        }
        else
        {
            // 부상이 있는 경우 : 그 부상이 이미 이식된 경우가 아닐 때만 뜬다.
            if (injury.type != InjuryType.TranscendantPartsTransplanted)
            {
                // 강화신체의 경우 초월신체와 강화신체가 모두 아닐 때에만 뜬다.
                if (injury.type != InjuryType.AugmentedPartsTransplanted || CheckSubpartDamaged(wantSite, 2))
                {
                    localizedSurgeryName = new LocalizedString("Injury", "Augmented Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
                    surgeryList.Add(new(localizedSurgeryName, wantCostAugmentBody, wantSite, SurgeryType.AugmentedPartTransplant));
                }
                localizedSurgeryName = new LocalizedString("Injury", "Transcendant Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
                surgeryList.Add(new(localizedSurgeryName, wantCostTranscendantBody, wantSite, SurgeryType.TrancendantPartTransplant));
            }
            else if (CheckSubpartDamaged(wantSite, 3))
            {
                // 그 부상이 이식된 경우에도 하위부위가 손상된 경우엔 뜬다.
                localizedSurgeryName = new LocalizedString("Injury", "Augmented Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
                surgeryList.Add(new(localizedSurgeryName, wantCostAugmentBody, wantSite, SurgeryType.AugmentedPartTransplant));
                localizedSurgeryName = new LocalizedString("Injury", "Transcendant Part Implant") { Arguments = new[] { (new LocalizedString("Injury", $"{wantSite}")).GetLocalizedString() } };
                surgeryList.Add(new(localizedSurgeryName, wantCostTranscendantBody, wantSite, SurgeryType.TrancendantPartTransplant));
            }
        }
    }

    bool CheckSubpartDamaged(InjurySite upperpart, int upperpartsTier)
    {
        foreach (var subpart in Injury.GetSubparts(upperpart))
        {
            Injury subpartInjury = survivorWhoWantSurgery.injuries.Find(x => x.site == subpart);
            if (subpartInjury != null)
            {
                if (subpartInjury.degree > 0) return true;
                else
                {
                    int subpartsTier = 0;
                    if (subpartInjury.type == InjuryType.ArtificialPartsTransplanted) subpartsTier = 1;
                    else if (subpartInjury.type == InjuryType.AugmentedPartsTransplanted) subpartsTier = 2;
                    else if (subpartInjury.type == InjuryType.TranscendantPartsTransplanted) subpartsTier = 3;
                    return upperpartsTier > subpartsTier;
                }
            }
        }
        return false;
    }

    void Surgery()
    {
        trainingResult.SetActive(true);
        GameManager.Instance.openedWindows.Push(trainingResult);
        resultText.gameObject.SetActive(true);
        resultText.GetComponent<LocalizeStringEvent>().StringReference = new LocalizedString("Basic", "Surgery Result");
        trainingResultText.text = new LocalizedString("Basic", "Surgery Successful").GetLocalizedString();
        trainingResultDetailText.text = new LocalizedString("Injury", mySurvivorsData[0].localizedScheduledSurgeryName.TableEntryReference.Key) { Arguments = new[] { new LocalizedString("Injury", mySurvivorsData[0].surgerySite.ToString()).GetLocalizedString() } }.GetLocalizedString();
        Surgery(mySurvivorsData[0]);
        selectedSurvivor.SetInfo(mySurvivorsData[0], false);
        operatingRoom.SetActive(false);

        GameManager.Instance.Save(0);
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
        survivorWhoWantSurgery.scheduledSurgeryName = survivorWhoWantSurgery.localizedScheduledSurgeryName.GetLocalizedString();
        survivorWhoWantSurgery.scheduledSurgeryCost = surgeryList[index].surgeryCost;
        survivorWhoWantSurgery.surgerySite = surgeryList[index].surgerySite;
        survivorWhoWantSurgery.surgeryType = surgeryList[index].surgeryType;
        survivorWhoWantSurgery.surgeryCharacteristic = surgeryList[index].surgeryCharacteristic;
        OpenConfirmWindow("Confirm:Surgery", ()=>
        {
            if(money < survivorWhoWantSurgery.scheduledSurgeryCost)
            {
                Alert("Alert:Not enough money.");
            }
            else
            {
                survivorWhoWantSurgery.surgeryScheduled = true;
                Money -= survivorWhoWantSurgery.scheduledSurgeryCost;
                SelectSurvivorToSurgery();
                //Alert("Alert:Surgery has been scheduled.");
                Surgery();
            }
        }, $"{ survivorWhoWantSurgery.localizedSurvivorName.GetLocalizedString() }", $"{ survivorWhoWantSurgery.localizedScheduledSurgeryName.GetLocalizedString() }", $"{ survivorWhoWantSurgery.scheduledSurgeryCost }");
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
        //weaponPriority1Dropdown.ClearOptions();
        //weaponPriority2Dropdown.ClearOptions();
        //ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
        //for(int i = (int)ItemManager.Items.Knife; i < (int)ItemManager.Items.Knife_Enchanted;  i++)
        //{
        //    bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{items[i]}", out var itemSpriteEnum);
        //    weaponPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
        //    weaponPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
        //    Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
        //    weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        //    weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        //}
        //GameManager.Instance.ObjectUpdate += () => 
        //{ 
        //    if(weaponPriority1Dropdown.dropdown.IsExpanded)
        //    {
        //        var dropdownSprites = weaponPriority1Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
        //        for(int i=0; i<weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
        //        {
        //            Image image = dropdownSprites[i].GetComponent<Image>();
        //            image.sprite = weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
        //            if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        //        }
        //    }

        //    if (weaponPriority2Dropdown.dropdown.IsExpanded)
        //    {
        //        var dropdownSprites = weaponPriority2Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
        //        for (int i = 0; i < weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
        //        {
        //            Image image = dropdownSprites[i].GetComponent<Image>();
        //            image.sprite = weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
        //            if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        //        }
        //    }
        //};

        foreach(var strategy in strategies)
        {
            switch(strategy.strategyCase)
            {
                case StrategyCase.WeaponPriority:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.ClearOptions();
                    ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
                    for (int i = (int)ItemManager.Items.Knife; i < (int)ItemManager.Items.Knife_Enchanted; i++)
                    {
                        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{items[i]}", out var itemSpriteEnum);
                        strategy.ActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
                        strategy.ElseActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
                        Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
                        strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
                        strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
                    }
                    GameManager.Instance.ObjectUpdate += () =>
                    {
                        if (strategy.ActionDropdown.dropdown.IsExpanded)
                        {
                            var dropdownSprites = strategy.ActionDropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                            for (int i = 0; i < strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                            {
                                Image image = dropdownSprites[i].GetComponent<Image>();
                                image.sprite = strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites[i];
                                if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                            }
                        }

                        if (strategy.ElseActionDropdown.dropdown.IsExpanded)
                        {
                            var dropdownSprites = strategy.ElseActionDropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                            for (int i = 0; i < strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                            {
                                Image image = dropdownSprites[i].GetComponent<Image>();
                                image.sprite = strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites[i];
                                if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                            }
                        }
                    };
                    break;
                case StrategyCase.SawAnEnemyAndItIsInAttackRange:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "Attacks."), new("Basic", "Ignores."), new("Basic", "Runs away."));
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new("Basic", "Attacks."), new("Basic", "Ignores."), new("Basic", "Runs away."));
                    break;
                case StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "Approaches."), new("Basic", "Ignores."), new("Basic", "Runs away."));
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new("Basic", "Approaches."), new("Basic", "Ignores."), new("Basic", "Runs away."));
                    break;
                case StrategyCase.WhenAnEnemyDisappearsFromSight:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "Tracks."), new("Basic", "Ignores."));
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new("Basic", "Tracks."), new("Basic", "Ignores."));
                    break;
                case StrategyCase.HeardDistinguishableSound:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "Goes to the source of the sound."), new("Basic", "Looks in the direction of the sound."), new("Basic", "Ignores the sound."));
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new("Basic", "Goes to the source of the sound."), new("Basic", "Looks in the direction of the sound."), new("Basic", "Ignores the sound."));
                    break;
                case StrategyCase.HeardIndistinguishableSound:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "Goes to the source of the sound."), new("Basic", "Looks in the direction of the sound."), new("Basic", "Ignores the sound."));
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new("Basic", "Goes to the source of the sound."), new("Basic", "Looks in the direction of the sound."), new("Basic", "Ignores the sound."));
                    break;
                case StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.AddLocalizedOptions(new("Basic", "First seen person"), new("Basic", "Nearest person"), new("Basic", "Person with the longest range"));
                    break;
                case StrategyCase.CraftingPriority:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
                    strategy.ActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
                    strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
                    strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
                    GameManager.Instance.ObjectUpdate += () =>
                    {
                        if (strategy.ActionDropdown.dropdown.IsExpanded)
                        {
                            var dropdownSprites = strategy.ActionDropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                            for (int i = 0; i < strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                            {
                                Image image = dropdownSprites[i].GetComponent<Image>();
                                image.sprite = strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites[i];
                                if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                            }
                        }
                        if (strategy.ElseActionDropdown.dropdown.IsExpanded)
                        {
                            var dropdownSprites = strategy.ElseActionDropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                            for (int i = 0; i < strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                            {
                                Image image = dropdownSprites[i].GetComponent<Image>();
                                image.sprite = strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites[i];
                                if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                            }
                        }
                    };
                    break;
            }
        }

        //sawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        //sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        //elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        //elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        
        //sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        //sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        //elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        //elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        //heardDistinguishableSoundDropdown.ClearOptions();
        //heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        //elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        //elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        
        //heardIndistinguishableSoundDropdown.ClearOptions();
        //heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        //elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        //elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        
        //whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.ClearOptions();
        //whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "First seen person").GetLocalizedString(), new LocalizedString("Basic", "Nearest person").GetLocalizedString(), new LocalizedString("Basic", "Person with the longest range").GetLocalizedString() }));
        
        //craftingPriority1Dropdown.ClearOptions();
        //craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        //craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        //craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        //craftingPriority2Dropdown.ClearOptions();
        //craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        //craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        //craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        //GameManager.Instance.ObjectUpdate += () =>
        //{
        //    if (craftingPriority1Dropdown.dropdown.IsExpanded)
        //    {
        //        var dropdownSprites = craftingPriority1Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
        //        for (int i = 0; i < craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
        //        {
        //            Image image = dropdownSprites[i].GetComponent<Image>();
        //            image.sprite = craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
        //            if(image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        //        }
        //    }
        //    if (craftingPriority2Dropdown.dropdown.IsExpanded)
        //    {
        //        var dropdownSprites = craftingPriority2Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
        //        for (int i = 0; i < craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
        //        {
        //            Image image = dropdownSprites[i].GetComponent<Image>();
        //            image.sprite = craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
        //            if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        //        }
        //    }
        //};
        for (int i = 0; i < ItemManager.craftables.Count; i++)
        {
            GameObject craftableAllow = PoolManager.Spawn(ResourceEnum.Prefab.CraftableAllow, craftingAllow);
            craftableAllow.GetComponentInChildren<TextMeshProUGUI>().text = new LocalizedString("Item", ItemManager.craftables[i].itemType.ToString()).GetLocalizedString();
            if (Enum.TryParse(ItemManager.craftables[i].itemType.ToString(), out ResourceEnum.Sprite sprite)) craftableAllow.GetComponentsInChildren<Image>()[1].sprite = ResourceManager.Get(sprite);
            int toggleIndex = i;
            craftableAllow.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => { Array.Find(strategies, x => x.strategyCase == StrategyCase.CraftingAllow).hasChanged = true; });
            craftableAllow.GetComponentInChildren<LocalizeStringEvent>().StringReference = new LocalizedString("Item", ItemManager.craftables[i].itemType.ToString());
            craftableAllow.AddComponent<Help>().SetDescription(ItemManager.craftables[i].itemType);
            craftableAllows.Add(craftableAllow); 
        }
        foreach (Strategy strategy in strategies) strategy.Initialize();
        RelocalizeStrategyRoom();
        SetDefault();
    }

    void RelocalizeStrategyRoom()
    {
        foreach(var strategy in strategies)
        {
            if (strategy.ActionDropdown != null) strategy.ActionDropdown.RelocalizeOptions();
            if (strategy.ElseActionDropdown != null) strategy.ElseActionDropdown.RelocalizeOptions();
        }
        //weaponPriority1Dropdown.RelocalizeOptions();
        //weaponPriority2Dropdown.RelocalizeOptions();

        //weaponPriority1Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
        //weaponPriority2Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority2Weapon - (int)ItemManager.Items.Knife;
        //sawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        //sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        //elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        //elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Attacks.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        //sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        //sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));
        //elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        //elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Approaches.").GetLocalizedString(), new LocalizedString("Basic", "Ignores.").GetLocalizedString(), new LocalizedString("Basic", "Runs away.").GetLocalizedString() }));

        //heardDistinguishableSoundDropdown.ClearOptions();
        //heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        //elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        //elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));

        //heardIndistinguishableSoundDropdown.ClearOptions();
        //heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));
        //elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        //elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Goes to the source of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Looks in the direction of the sound.").GetLocalizedString(), new LocalizedString("Basic", "Ignores the sound.").GetLocalizedString() }));

        //whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.ClearOptions();
        //whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "First seen person").GetLocalizedString(), new LocalizedString("Basic", "Nearest person").GetLocalizedString(), new LocalizedString("Basic", "Person with the longest range").GetLocalizedString() }));
        
        //if(survivorWhoWantEstablishStrategy != null && survivorWhoWantEstablishStrategy.strategyDictionary != null)
        //{
        //    sawAnEnemyAndItIsInAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action;
        //    elseActionSawAnEnemyAndItIsInAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction;
        //    sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action;
        //    elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction;
        //    heardDistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardDistinguishableSound].action;
        //    elseActionHeardDistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardDistinguishableSound].elseAction;
        //    heardIndistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardIndistinguishableSound].action;
        //    elseActionHeardIndistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardIndistinguishableSound].elseAction;
        //    whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget].action;
        //}

        //craftingPriority1Dropdown.RelocalizeOptions();
        //craftingPriority2Dropdown.RelocalizeOptions();
    }

    public void SetDefault()
    {
        foreach (Strategy strategy in strategies)
        {
            strategy.ResetConditions();
            switch(strategy.strategyCase)
            {
                case StrategyCase.WeaponPriority:
                    strategy.ActionDropdown.Value = (int)ItemManager.Items.LASER - (int)ItemManager.Items.Knife;
                    strategy.ElseActionDropdown.Value = (int)ItemManager.Items.AssaultRifle - (int)ItemManager.Items.Knife;
                    break;
                case StrategyCase.SawAnEnemyAndItIsInAttackRange:
                case StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange:
                case StrategyCase.HeardDistinguishableSound:
                    strategy.ActionDropdown.Value = 0;
                    strategy.ActionDropdown.Value = 0;
                    strategy.ActionDropdown.Value = 0;
                    break;
                case StrategyCase.HeardIndistinguishableSound:
                case StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget:
                    strategy.ActionDropdown.Value = 1;
                    strategy.ActionDropdown.Value = 1;
                    break;
                case StrategyCase.CraftingPriority:
                    strategy.ActionDropdown.Value = 0;
                    strategy.ElseActionDropdown.Value = 0;
                    break;
            }
        }
        
        //weaponPriority1Dropdown.dropdown.value = (int)ItemManager.Items.LASER - (int)ItemManager.Items.Knife;
        //weaponPriority2Dropdown.dropdown.value = (int)ItemManager.Items.AssaultRifle - (int)ItemManager.Items.Knife;
        //sawAnEnemyAndItIsInAttackRangeDropdown.value = 0;
        //sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = 0;
        //heardDistinguishableSoundDropdown.value = 0;
        //heardIndistinguishableSoundDropdown.value = 1;
        //whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = 1;
        //craftingPriority1Dropdown.dropdown.value = 0;
        //craftingPriority2Dropdown.dropdown.value = 0;
        foreach(var craftableAllow in craftableAllows) craftableAllow.GetComponentsInChildren<Toggle>()[0].isOn = true;
        armorRepairConditionInputField.text = "70";
    }

    public void OpenStrategyRoom()
    {
        strategyRoom.SetActive(true);
        SetStrategyRoom();
        RelocalizeStrategyRoom();
        GameManager.Instance.openedWindows.Push(strategyRoom);
    }

    public void CloseStrategyRoom()
    {
        bool hasChanged = false;
        foreach(Strategy strategy in strategies) if(strategy.hasChanged) { hasChanged = true;  break; }
        if(hasChanged) OpenConfirmWindow("Confirm:Close Strategy Room", () => { strategyRoom.SetActive(false); });
        else strategyRoom.SetActive(false);
    }

    bool strategyRoomJustOpend = true;
    void SetStrategyRoom()
    {
        selectSurvivorEstablishStrategyDropdown.ClearOptions();
        selectSurvivorEstablishStrategyDropdown.AddOptions(survivorsDropdown.options);
        survivorWhoWantEstablishStrategy = mySurvivorsData[0];
        strategyRoomJustOpend = true;
        SelectSurvivorToEstablishStrategy();
        foreach (var strategy in strategies) strategy.hasChanged = false;
    }

    public void SelectSurvivorToEstablishStrategy()
    {
        if (strategyRoomJustOpend) strategyRoomJustOpend = false;
        else
        {
            bool hasChanged = false;
            foreach (Strategy strategy in strategies) if (strategy.hasChanged) { hasChanged = true; break; }
            if(hasChanged) SaveStrategy();
        }
        survivorInfoEstablishStrategy.SetInfo(MySurvivorsData[selectSurvivorEstablishStrategyDropdown.value], false);
        survivorWhoWantEstablishStrategy = MySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == selectSurvivorEstablishStrategyDropdown.options[selectSurvivorEstablishStrategyDropdown.value].text);
        
        foreach(var strategy in strategies)
        {
            switch(strategy.strategyCase)
            {
                case StrategyCase.WeaponPriority:
                    strategy.ActionDropdown.Value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
                    strategy.ElseActionDropdown.Value = (int)survivorWhoWantEstablishStrategy.priority2Weapon - (int)ItemManager.Items.Knife;
                    break;
                case StrategyCase.CraftingPriority:
                    strategy.ActionDropdown.ClearOptions();
                    strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
                    strategy.ActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
                    strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
                    strategy.ElseActionDropdown.ClearOptions();
                    strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
                    strategy.ElseActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
                    strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
                    foreach (var craftable in ItemManager.craftables)
                    {
                        bool trapExpertAndTraps = survivorWhoWantEstablishStrategy.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (craftable.itemType == ItemManager.Items.BearTrap || craftable.itemType == ItemManager.Items.NoiseTrap || craftable.itemType == ItemManager.Items.ShrapnelTrap || craftable.itemType == ItemManager.Items.ChemicalTrap || craftable.itemType == ItemManager.Items.ExplosiveTrap || craftable.itemType == ItemManager.Items.TrapDetectionDevice);
                        if (craftable.requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge || trapExpertAndTraps)
                        {
                            bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftable.itemType}", out var itemSpriteEnum);
                            strategy.ActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new LocalizedString("Item", craftable.itemType.ToString()) });
                            strategy.ElseActionDropdown.AddLocalizedOptions(new List<LocalizedString> { new LocalizedString("Item", craftable.itemType.ToString()) });
                            Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
                            strategy.ActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
                            strategy.ElseActionDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
                        }
                    }
                    strategy.ActionDropdown.Value = strategy.ActionDropdown.keys.Count + 1 > survivorWhoWantEstablishStrategy.priority1CraftingToInt + 1 ? survivorWhoWantEstablishStrategy.priority1CraftingToInt + 1 : 0;
                    strategy.ElseActionDropdown.Value = strategy.ElseActionDropdown.keys.Count + 1 > survivorWhoWantEstablishStrategy.priority2CraftingToInt + 1 ? survivorWhoWantEstablishStrategy.priority2CraftingToInt + 1 : 0;
                    break;
                default:
                    if (strategy.ActionDropdown != null) strategy.ActionDropdown.Value = survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase].action;
                    if (strategy.ElseActionDropdown != null) strategy.ElseActionDropdown.Value = survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase].elseAction;
                    if (strategy.IntagerInput != null)
                    {
                        strategy.IntagerInput.text = survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase].action.ToString();
                    }
                    break;
            }
        }
        
        //weaponPriority1Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
        //craftingPriority1Dropdown.ClearOptions();
        //craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        //craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        //craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        //weaponPriority2Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority2Weapon - (int)ItemManager.Items.Knife;
        //craftingPriority2Dropdown.ClearOptions();
        //craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        //craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        //craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        //foreach (var craftable in ItemManager.craftables)
        //{
        //    bool trapExpertAndTraps = survivorWhoWantEstablishStrategy.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (craftable.itemType == ItemManager.Items.BearTrap || craftable.itemType == ItemManager.Items.NoiseTrap || craftable.itemType == ItemManager.Items.ShrapnelTrap || craftable.itemType == ItemManager.Items.ChemicalTrap || craftable.itemType == ItemManager.Items.ExplosiveTrap || craftable.itemType == ItemManager.Items.TrapDetectionDevice);
        //    if (craftable.requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge || trapExpertAndTraps)
        //    {
        //        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftable.itemType}", out var itemSpriteEnum);
        //        craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString>{ new LocalizedString("Item", craftable.itemType.ToString()) });
        //        craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString>{ new LocalizedString("Item", craftable.itemType.ToString()) });
        //        Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
        //        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        //        craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        //    }
        //}
        //craftingPriority1Dropdown.dropdown.value = survivorWhoWantEstablishStrategy.priority1CraftingToInt + 1;
        //craftingPriority2Dropdown.dropdown.value = survivorWhoWantEstablishStrategy.priority2CraftingToInt + 1;

        for(int i=0; i<survivorWhoWantEstablishStrategy.craftingAllows.Length; i++)
        {
            craftableAllows[i].SetActive(ItemManager.craftables[i].requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge);
            if (survivorWhoWantEstablishStrategy.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (ItemManager.craftables[i].itemType == ItemManager.Items.BearTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ShrapnelTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ChemicalTrap || ItemManager.craftables[i].itemType == ItemManager.Items.NoiseTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ExplosiveTrap || ItemManager.craftables[i].itemType == ItemManager.Items.TrapDetectionDevice))
                craftableAllows[i].SetActive(true);
            craftableAllows[i].GetComponentsInChildren<Toggle>(true)[0].isOn = survivorWhoWantEstablishStrategy.craftingAllows[i];
            craftableAllows[i].GetComponentsInChildren<Toggle>(true)[1].isOn = !survivorWhoWantEstablishStrategy.craftingAllows[i];
        }

        // 조건 불러오기
        foreach (Strategy strategy in strategies) strategy.ResetConditions();
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
            for (int j = 0; j < 5; j++)
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
        foreach (var strategy in strategies) strategy.hasChanged = false;
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

    public void WeaponPriority1Changed()
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

    public void WeaponPriority2Changed()
    {
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{weaponPriority2Dropdown.keys[weaponPriority2Dropdown.dropdown.value].TableEntryReference.Key}", out var itemSpriteEnum);
        if (spriteNotNull)
        {
            Image image = weaponPriority2Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
        else Debug.Log($"Sprite not found : {weaponPriority2Dropdown.keys[weaponPriority2Dropdown.dropdown.value].TableEntryReference.Key}");
    }

    public void CraftingPriority1Changed()
    {
        Image image = craftingPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}", out var itemSpriteEnum);
        if (spriteNotNull)
        {
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
        else if(craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key == "None") 
        {
            image.sprite = null;
            craftingPriority2Dropdown.dropdown.value = 0;
            CraftingPriority2Changed();
        }
        else
        {
            Debug.Log($"Sprite not found : {craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}");
        }
        craftingPriority2Dropdown.dropdown.interactable = craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key != "None";
    }

    public void CraftingPriority2Changed()
    {
        Image image = craftingPriority2Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value].TableEntryReference.Key}", out var itemSpriteEnum);
        if (spriteNotNull)
        {
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
        else if (craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value].TableEntryReference.Key == "None")
        {
            image.sprite = null;
        }
        else
        {
            Debug.Log($"Sprite not found : {craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value].TableEntryReference.Key}");
        }
    }

    public void SaveStrategy()
    {
        bool itemNotNull = Enum.TryParse<ItemManager.Items>($"{weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}", out var itemEnum);
        if (itemNotNull) survivorWhoWantEstablishStrategy.priority1Weapon = itemEnum;
        else Debug.LogWarning($"Item enum not found : {weaponPriority1Dropdown.keys[weaponPriority1Dropdown.dropdown.value].TableEntryReference.Key}");

        itemNotNull = Enum.TryParse<ItemManager.Items>($"{weaponPriority2Dropdown.keys[weaponPriority2Dropdown.dropdown.value].TableEntryReference.Key}", out var itemEnum2);
        if (itemNotNull) survivorWhoWantEstablishStrategy.priority2Weapon = itemEnum2;
        else Debug.LogWarning($"Item enum not found : {weaponPriority2Dropdown.keys[weaponPriority2Dropdown.dropdown.value].TableEntryReference.Key}");

        if (craftingPriority1Dropdown.dropdown.value == 0)
        {
            survivorWhoWantEstablishStrategy.priority1Crafting = null;
            survivorWhoWantEstablishStrategy.priority1CraftingToInt = -1;
            survivorWhoWantEstablishStrategy.priority2Crafting = null;
            survivorWhoWantEstablishStrategy.priority2CraftingToInt = -1;
        }
        else
        {
            foreach (var craftableAllow in craftableAllows)
            {
                if (craftableAllow.GetComponentInChildren<LocalizeStringEvent>().StringReference.TableEntryReference.Key == $"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}"
                && craftableAllow.GetComponentsInChildren<Toggle>()[1].isOn)
                {
                    Alert("Alert:Crafting Priority Not Valid");
                    return;
                }
            }

            ItemManager.Craftable craftable = ItemManager.craftables.Find(x => x.itemType.ToString() == $"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}");
            itemNotNull = craftable != null;
            if (itemNotNull)
            {
                survivorWhoWantEstablishStrategy.priority1Crafting = craftable;
                survivorWhoWantEstablishStrategy.priority1CraftingToInt = craftingPriority1Dropdown.dropdown.value - 1;
            }
            else Debug.LogWarning($"Craftable not found : {craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value].TableEntryReference.Key}");

            if(craftingPriority2Dropdown.dropdown.value == 0)
            {
                survivorWhoWantEstablishStrategy.priority2Crafting = null;
                survivorWhoWantEstablishStrategy.priority2CraftingToInt = -1;
            }
            else
            {
                foreach (var craftableAllow in craftableAllows)
                {
                    if (craftableAllow.GetComponentInChildren<LocalizeStringEvent>().StringReference.TableEntryReference.Key == $"{craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value - 1].TableEntryReference.Key}"
                    && craftableAllow.GetComponentsInChildren<Toggle>()[1].isOn)
                    {
                        Alert("Alert:Crafting Priority Not Valid");
                        return;
                    }
                }

                ItemManager.Craftable craftable2 = ItemManager.craftables.Find(x => x.itemType.ToString() == $"{craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value].TableEntryReference.Key}");
                itemNotNull = craftable2 != null;
                if (itemNotNull)
                {
                    survivorWhoWantEstablishStrategy.priority2Crafting = craftable2;
                    survivorWhoWantEstablishStrategy.priority2CraftingToInt = craftingPriority2Dropdown.dropdown.value - 1;
                }
                else Debug.LogWarning($"Craftable not found : {craftingPriority2Dropdown.keys[craftingPriority2Dropdown.dropdown.value].TableEntryReference.Key}");
            }
        }

        for(int i = 0; i < survivorWhoWantEstablishStrategy.craftingAllows.Length; i++)
        {
            survivorWhoWantEstablishStrategy.craftingAllows[i] = craftableAllows[i].GetComponentsInChildren<Toggle>()[0].isOn;
        }

        foreach(Strategy strategy in strategies)
        {
            if(strategy.NoCondition)
            {
                if(strategy.strategyCase == StrategyCase.RepairCondition)
                {
                    if (int.TryParse(strategy.IntagerInput.text, out int intInput))
                    {
                        survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.RepairCondition] = new(intInput, 0, 0);
                    }
                    else strategy.IntagerInput.text = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.RepairCondition].action.ToString();
                }
                else
                {
                    survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] = new(strategy.ActionDropdown != null ? strategy.ActionDropdown.Value : 0, strategy.ElseActionDropdown != null ? strategy.ElseActionDropdown.Value : 0, 0);
                }
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
                    new(strategy.ActionDropdown.Value, strategy.ElseActionDropdown.Value, strategy.activeConditionCount, conditionData);
            }
            strategy.hasChanged = false;
        }
        Alert("Alert:Strategy Saved");
    }

    public void CopyAllStrategies()
    {
        foreach(Strategy strategy in strategies) strategy.CopyStrategy();
    }

    public void PasteAllStrategies()
    {
        foreach (Strategy strategy in strategies) strategy.PasteStrategy();
    }

    public void PasteAllStrategiesOfThisSurvivor()
    {
        foreach (Strategy strategy in strategies) strategy.PasteThisStrategyToAllOtherSurvivor(true);
        Alert("Strategy pasted and saved.");
    }
    #endregion

    // alreadyHad : 0 = Not already had, 1 = prostetic, 2 = augmented, 3 = transcendant
    public int MeasureTreatmentCost(Injury injury, int alreadyHad)
    {
        float cost = 0;

        switch (injury.site)
        {
            case InjurySite.Skull:
            case InjurySite.Brain:
                cost = injury.degree * 300;
                break;
            case InjurySite.Head:
            case InjurySite.Cheek:
            case InjurySite.Neck:
            case InjurySite.Nose:
            case InjurySite.Jaw:
                cost = injury.degree * 50;
                break;
            case InjurySite.RightEar:
            case InjurySite.LeftEar:
                if(alreadyHad > 0) cost = injury.degree * 100;
                else if (injury.degree == 1) cost = 100;
                else cost = injury.degree * 50;
                break;
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
                if (alreadyHad == 1) cost = injury.degree * 400;
                else if (alreadyHad == 2) cost = injury.degree * 4000;
                else if (alreadyHad == 3) cost = injury.degree * 20000;
                else if (injury.degree == 1) cost = 400;
                else cost = injury.degree * 50;
                break;
            case InjurySite.Chest:
            case InjurySite.Ribs:
            case InjurySite.Abdomen:
                cost = injury.degree * 100;
                break;
            case InjurySite.Organ:
                if (alreadyHad > 0) cost = injury.degree * 600;
                else if (injury.degree == 1) cost = 600;
                else cost = injury.degree * 300;
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
                if (alreadyHad == 1) cost = injury.degree * 500;
                else if (alreadyHad == 2) cost = injury.degree * 15000 * 0.3f;
                else if (alreadyHad == 3) cost = injury.degree * 75000 * 0.3f;
                else if (injury.degree == 1) cost = 500;
                else cost = injury.degree * 100;
                break;
            case InjurySite.RightArm:
            case InjurySite.LeftArm:
                if (alreadyHad == 1) cost = injury.degree * 250;
                else if (alreadyHad == 2) cost = injury.degree * 5000 * 0.5f;
                else if (alreadyHad == 3) cost = injury.degree * 25000 * 0.5f;
                else if (injury.degree == 1) cost = 250;
                else cost = injury.degree * 50;
                break;
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
                if (alreadyHad == 1) cost = injury.degree * 250;
                else if (alreadyHad == 2) cost = injury.degree * 15000 * 0.3f;
                else if (alreadyHad == 3) cost = injury.degree * 75000 * 0.3f;
                else if (injury.degree == 1) cost = 250;
                else cost = injury.degree * 50;
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
                if (alreadyHad == 1) cost = injury.degree * 100;
                else if (alreadyHad == 2) cost = injury.degree * 5000 * 0.25f;
                else if (alreadyHad == 3) cost = injury.degree * 25000 * 0.25f;
                else if (injury.degree == 1) cost = 100;
                else cost = injury.degree * 25;
                break;
            case InjurySite.RightFoot:
            case InjurySite.LeftFoot:
                if (alreadyHad == 1) cost = injury.degree * 100;
                else if (alreadyHad == 2) cost = injury.degree * 15000 * 0.2f;
                else if (alreadyHad == 3) cost = injury.degree * 75000 * 0.2f;
                else if (injury.degree == 1) cost = 100;
                else cost = injury.degree * 25;
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
                if (alreadyHad == 1) cost = injury.degree * 10;
                else if (alreadyHad == 2) cost = injury.degree * 5000 * 0.05f;
                else if (alreadyHad == 3) cost = injury.degree * 25000 * 0.05f;
                else if (injury.degree == 1) cost = 10;
                else cost = injury.degree * 10;
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
                if (alreadyHad == 1) cost = injury.degree * 10;
                else if (alreadyHad == 2) cost = injury.degree * 15000 * 0.04f;
                else if (alreadyHad == 3) cost = injury.degree * 75000 * 0.04f;
                else if (injury.degree == 1) cost = 10;
                else cost = injury.degree * 10;
                break;
            case InjurySite.None:
                break;
            default:
                Debug.LogWarning($"Missing body parts : {injury.site}");
                break;
        }

        return (int)cost;
    }

    public void StartBattleRoyale()
    {
        tutorial = false;
        GameManager.Instance.Option.SetSaveButtonInteractable(false, true);
        if(gameMode == GameMode.SingleCareerRun)
        {
            mySurvivorDataInBattleRoyale = mySurvivorsData[0];
        }
        else
        {
            mySurvivorDataInBattleRoyale = calendar.LeagueReserveInfo[calendar.Today].reserver;
        }
        //if (mySurvivorDataInBattleRoyale == null) AchievementManager.UnlockAchievement("Spectator");
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
        if (championship && championshipDatas != null && championshipDatas.Count > 5) return;
        
        int index = 0;
        if(championship && calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship)
        {
            index = 5;
        }
        else
        {
            contestantsData = new();
        }

        //if (calendar.LeagueReserveInfo[calendar.Today].reserver != null)
        {
            calendar.LeagueReserveInfo[calendar.Today].reserver = mySurvivorsData[0];
            if(!championship || championship && calendar.LeagueReserveInfo[calendar.Today].league != League.WorldChampionship)
            {
                championshipDatas.Clear();
                contestantsData.Add(calendar.LeagueReserveInfo[calendar.Today].reserver);
                championshipDatas.Add(new(MySurvivorsData[0]));
                index++;
            }
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
            SurvivorData survivor = CreateRandomSurvivorData();
            contestantsData.Add(survivor);
            if(championship) championshipDatas.Add(new(survivor));
        }
    }

    [SerializeField] List<SurvivorData> sortedContestantsData = new();
    public void SortContestantsList()
    {
        sortedContestantsData = new();
        switch(sortContestantsListDropdown.keys[sortContestantsListDropdown.dropdown.value].TableEntryReference.Key)
        {
            case "Strength":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Strength).ToList();
                break;
            case "Agility":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Agility).ToList();
                break;
            case "Fighting":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Fighting).ToList();
                break;
            case "Shooting":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Shooting).ToList();
                break;
            case "Crafting":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Crafting).ToList();
                break;
            case "Knowledge":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.Knowledge).ToList();
                break;
            case "Stat Total":
                sortedContestantsData = contestantsData.OrderByDescending(x => x.StatTotal).ToList();
                break;
            default:
                Debug.LogWarning($"Wrong sort key : {sortContestantsListDropdown.keys[sortContestantsListDropdown.dropdown.value].TableEntryReference.Key}");
                return;
        }

        int thereIsPlayerSurvivor = mySurvivorDataInBattleRoyale == null ? 1 : 0;
        for (int i = 0; i < sortedContestantsData.Count; i++)
        {
            int originalDataIndex = contestantsData.FindIndex(x => x.localizedSurvivorName.TableEntryReference.Key == sortedContestantsData[i].localizedSurvivorName.TableEntryReference.Key);
            Vector3 colorVector = BattleRoyaleManager.colorInfo[originalDataIndex + thereIsPlayerSurvivor];
            contestants[i].GetComponentsInChildren<Image>()[1].color = new(colorVector.x, colorVector.y, colorVector.z);
            contestants[i].GetComponentInChildren<TextMeshProUGUI>().text = contestantsData[originalDataIndex].localizedSurvivorName.GetLocalizedString();
        }

    }

    public void OpenMapInfo()
    {
        mapInfo.SetActive(true);
        string minimapName = $"Minimap{calendar.LeagueReserveInfo[calendar.Today].map.ToString()[3..]}";
        if (Enum.TryParse(minimapName, out ResourceEnum.Sprite sprite))
        {
            mapImage.sprite = ResourceManager.Get(sprite);
        }
        else Debug.LogWarning($"Minimap not found : {minimapName}");

        farmableItemsText.text = string.Empty;
        foreach (var item in calendar.itemPool[calendar.LeagueReserveInfo[calendar.Today].itemPool])
        {
            farmableItemsText.text += $"{new LocalizedString("Item", item.Key.ToString()).GetLocalizedString()} x {item.Value},\n";
        }
        GameManager.Instance.FixLayout(farmableItemsText.GetComponent<RectTransform>());
        farmableItemsScrollRect.verticalNormalizedPosition = 1;
    }

    public void RandomBetting()
    {
        List<SurvivorData> shuffledContestants = contestantsData.Shuffle();
        for (int i=0; i<needPredictionNumber; i++)
        {
            predictRankingContestants[i].SetActive(true);
            int originalIndex = sortedContestantsData.FindIndex(x => x.SurvivorName == shuffledContestants[i].SurvivorName);
            predictRankingContestants[i].GetComponentsInChildren<Image>()[1].color = contestants[originalIndex].GetComponentsInChildren<Image>()[1].color;
            predictRankingContestants[i].GetComponentInChildren<LocalizeStringEvent>().StringReference = shuffledContestants[i].localizedSurvivorName;
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
        if (!IsValidPrediction(out string reason)) Alert("Invalid Prediction", reason);
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
        //OpenConfirmWindow("Confirm:Skip Bet", () => 
        //{
            bettingAmount = 0;
            bettingRoom.SetActive(false);

        // 2.0
        mySurvivorDataInBattleRoyale = mySurvivorsData[0];
        SetContestants();

            StartBattleRoyale();
        //});
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
        //contestantsData = null;
        ResetTrainingRoom();
        calendar.Today++;
        calendar.TurnPageCalendar(0);
        if (calendar.Today % 7 == 0)
        {
            Money += 1000;
            Alert("Alert:Money Recived");
        }

        GameManager.Instance.FixLayout(dailyResult.GetComponent<RectTransform>());
        GameManager.Instance.openedWindows.Push(dailyResult);
    }

    public void EndTheDay()
    {
        string key = "End The Day";
        string warning = "";
        if(calendar.Today % 7 < 5)
        {
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
                ShowSurgeryResult();
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
        //contestantsData = null;
        ResetTrainingRoom();
        calendar.Today++;
        calendar.TurnPageCalendar(0);
        if (calendar.Today % 7 == 0)
        {
            Money += 1000;
            Alert("Alert:Money Recived");
        }

        //foreach (var survivor in mySurvivorsData)
        //{
        //    survivor.increaseComparedToPrevious_strength = -1;
        //    survivor.increaseComparedToPrevious_agility = -1;
        //    survivor.increaseComparedToPrevious_fighting = -1;
        //    survivor.increaseComparedToPrevious_shooting = -1;
        //    survivor.increaseComparedToPrevious_crafting = -1;
        //    survivor.increaseComparedToPrevious_knowledge = -1;
        //}
        Alert("Alert:A day has passed.");
    }

    public void HideEndTheWeekend(bool hide)
    {
        buttonEndTheWeek.SetActive(!hide);
    }

    void Surgery(SurvivorData survivor)
    {
        if (!survivor.surgeryScheduled)
        {
            //hadSurgery = false;
            return;
        }
        hadSurgery = true;
        whoUnderwentSurgery.Add(survivor.localizedSurvivorName);
        performedSurgery.Add(survivor.localizedScheduledSurgeryName);
        if(survivor.surgeryType == SurgeryType.ArtificialPartTransplant)
        {
            Injury surgeryInjury = survivor.injuries.Find(x => x.site == survivor.surgerySite);
            surgeryInjury.type = InjuryType.ArtificialPartsTransplanted;
            surgeryInjury.degree = 0;
            foreach(var subpart in Injury.GetSubparts(survivor.surgerySite))
            {
                Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                if (subpartInjury != null) survivor.injuries.Remove(subpartInjury);
            }
        }
        else if(survivor.surgeryType == SurgeryType.AugmentedPartTransplant)
        {
            Injury surgeryInjury = survivor.injuries.Find(x => x.site == survivor.surgerySite);
            if(surgeryInjury != null)
            {
                surgeryInjury.type = InjuryType.AugmentedPartsTransplanted;
                surgeryInjury.degree = 0;
            }
            else survivor.injuries.Add(new(survivor.surgerySite, InjuryType.AugmentedPartsTransplanted, 0));
            foreach (var subpart in Injury.GetSubparts(survivor.surgerySite))
            {
                Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                if (subpartInjury != null) survivor.injuries.Remove(subpartInjury);
            }
            AchievementManager.UnlockAchievement("Augmented Prosthetic");
        }
        else if(survivor.surgeryType == SurgeryType.TrancendantPartTransplant)
        {
            Injury surgeryInjury = survivor.injuries.Find(x => x.site == survivor.surgerySite); 
            if (surgeryInjury != null)
            {
                surgeryInjury.type = InjuryType.TranscendantPartsTransplanted;
                surgeryInjury.degree = 0;
            }
            else survivor.injuries.Add(new(survivor.surgerySite, InjuryType.TranscendantPartsTransplanted, 0));
            foreach (var subpart in Injury.GetSubparts(survivor.surgerySite))
            {
                Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                if (subpartInjury != null) survivor.injuries.Remove(subpartInjury);
            }
            AchievementManager.UnlockAchievement("Transcendent Prosthetic");
        }
        else if (survivor.surgeryType == SurgeryType.ChronicDisorderTreatment)
        {
            switch (survivor.surgeryCharacteristic)
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
        else if (survivor.surgeryType == SurgeryType.RecoverySerumAdministeration)
        {
            survivor.injuries.RemoveAll(x => x.degree < 1 && x.degree > 0 && x.type != InjuryType.ArtificialPartsDamaged && x.type != InjuryType.AugmentedPartsDamaged && x.type != InjuryType.TranscendantPartsDamaged);
        }

        Characteristic? characteristic = survivor.characteristics.Find(x => x.type == CharacteristicType.BadEye || x.type == CharacteristicType.HawkEye);
        if(characteristic.HasValue && characteristic.Value.characteristicName != null)
        {
            Injury leftEyeCheck = survivor.injuries.Find(x => x.site == InjurySite.LeftEye);
            Injury rightEyeCheck = survivor.injuries.Find(x => x.site == InjurySite.RightEye);
            if (leftEyeCheck != null && (leftEyeCheck.type == InjuryType.ArtificialPartsTransplanted || leftEyeCheck.type == InjuryType.ArtificialPartsDamaged
                || leftEyeCheck.type == InjuryType.AugmentedPartsTransplanted || leftEyeCheck.type == InjuryType.AugmentedPartsDamaged
                || leftEyeCheck.type == InjuryType.TranscendantPartsTransplanted || leftEyeCheck.type == InjuryType.TranscendantPartsDamaged)
                && rightEyeCheck != null && (rightEyeCheck.type == InjuryType.ArtificialPartsTransplanted || rightEyeCheck.type == InjuryType.ArtificialPartsDamaged
                || rightEyeCheck.type == InjuryType.AugmentedPartsTransplanted || rightEyeCheck.type == InjuryType.AugmentedPartsDamaged
                || rightEyeCheck.type == InjuryType.TranscendantPartsTransplanted || rightEyeCheck.type == InjuryType.TranscendantPartsDamaged))
            {
                Alert("Alert:CharacteristicRemoved", new LocalizedString("Characteristic", characteristic.Value.type.ToString()).GetLocalizedString());
                survivor.characteristics.Remove(characteristic.Value);
            }
        }

        survivor.totalSurgeryFee += survivor.scheduledSurgeryCost;
        survivor.scheduledSurgeryCost = 0;
        survivor.surgeryScheduled = false;
    }

    public void SurvivorsRecovery()
    {
        foreach(SurvivorData survivor in mySurvivorsData)
        {
            bool checkFullRecover = survivor.injuries.FindIndex(injury => injury.degree < 1 && injury.type != InjuryType.ArtificialPartsTransplanted && injury.type != InjuryType.ArtificialPartsDamaged
                    && injury.type != InjuryType.AugmentedPartsTransplanted && injury.type != InjuryType.AugmentedPartsDamaged
                    && injury.type != InjuryType.TranscendantPartsTransplanted && injury.type != InjuryType.TranscendantPartsDamaged) > -1;
            List<Injury> fullyRecovered = new();
            foreach(Injury injury in survivor.injuries)
            {
                if(injury.degree < 1 && injury.type != InjuryType.ArtificialPartsTransplanted && injury.type != InjuryType.ArtificialPartsDamaged
                    && injury.type != InjuryType.AugmentedPartsTransplanted && injury.type != InjuryType.AugmentedPartsDamaged
                    && injury.type != InjuryType.TranscendantPartsTransplanted && injury.type != InjuryType.TranscendantPartsDamaged)
                {
                    float recovery = 0;
                    float recoveryRate = 1;
                    if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) recoveryRate *= 1.5f;
                    else if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) recoveryRate *= 0.7f;

                    //if (injury.degree > 0.9) recovery = (1 - injury.degree) * recoveryRate;
                    //else recovery = (0.1f + (1 - injury.degree) * 0.1f) * recoveryRate;
                    recovery = 0.2f * recoveryRate;
                    injury.degree -= recovery;

                    if (injury.degree <= 0) fullyRecovered.Add(injury);
                    else checkFullRecover = false;
                }
            }
            foreach(Injury recovered in fullyRecovered)
            {
                survivor.injuries.Remove(recovered);
            }
            if (checkFullRecover) Alert("Alert:Fully Recovery");
        }
        ResetSelectedSurvivorInfo();
    }

    void ShowSurgeryResult()
    {
        surgeryResult.SetActive(hadSurgery);
        if (hadSurgery)
        {
            surgeryResultText.text = "";
            for (int i = 0; i < whoUnderwentSurgery.Count; i++) surgeryResultText.text += $"{new LocalizedString("Basic", "Surgery Successful").GetLocalizedString()}\n({whoUnderwentSurgery[i].GetLocalizedString()} : {performedSurgery[i].GetLocalizedString()})\n";
            whoUnderwentSurgery.Clear();
            performedSurgery.Clear();
        }
    }

    public void CloseDailyResult()
    {
        hadSurgery = false;
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
        int[] distribute = new int[6];
        int min = value * 45 + difficulty * (value * value + 5) * 2;
        int max = min + 30;
        int totalValue = UnityEngine.Random.Range(min, max + 1);
        if(totalValue >= 600)
        {
            distribute = new int[]{ 100, 100, 100, 100, 100, 100 };
        }
        else
        {
            while (totalValue > 0)
            {
                int rand = UnityEngine.Random.Range(0, Mathf.Min(101, totalValue + 1));
                int target = UnityEngine.Random.Range(0, 6);
                if (distribute[target] + rand > 100)
                {
                    check++;
                    if (check >= 10000)
                    {
                        Debug.LogWarning("Infinite roof has detected");
                        break;
                    }
                    continue;
                }
                distribute[target] += rand;
                totalValue -= rand;
            }
        }
        int randStrength = distribute[0];
        int randAgility = distribute[1];
        int randFighting = distribute[2];
        int randShooting = distribute[3];
        int randCrafting = distribute[4];
        int randKnowledge = distribute[5];
        SurvivorData survivorData = new(
            GetRandomName(),
            randStrength,
            randAgility,
            randFighting,
            randShooting,
            randCrafting,
            randKnowledge,
            0,
            calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league)
            );
        int characteristicCount;
        float randCharCount = UnityEngine.Random.Range(0, 1f);
        if (randCharCount < 0.33f) characteristicCount = 0;
        else if (randCharCount < 0.66f) characteristicCount = 1;
        else if (randCharCount < 0.9f) characteristicCount = 2;
        else characteristicCount = 3;
        CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicCount, false);

        survivorData.priority1Weapon = ItemManager.Items.LASER;
        survivorData.priority2Weapon = ItemManager.Items.AssaultRifle;
        return survivorData;
        
    }

    public void OpenChampionshipProgress()
    {
        championshipRankBG.SetActive(true);
        if (championshipHeldCount >= 3) championshipSurvivorInfo.gameObject.SetActive(false);
        else
        {
            championshipSurvivorInfo.gameObject.SetActive(true);
            championshipSurvivorInfo.SetInfo(MySurvivorsData[0], false);
        }
        GameManager.Instance.openedWindows.Push(championshipRankBG);

        if (championshipDatas.Count < 25) SetContestants();

        bool season = calendar.LeagueReserveInfo.ContainsKey(calendar.Today) && calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship;
        championshipTitle.StringReference = season ? new("Basic", "SeasonChampionship") : new("Basic", "WorldChampionship");
        championshipDescription.SetActive(season);
        for(int i=0; i<25; i++)
        {
            int rankChange = championshipDatas[i].beforeRank - championshipDatas[i].currentRank;
            if(championshipDatas[i].points.Count <= 1 || rankChange == 0)championshipRankRankChanges[i].text = "";
            else if(rankChange > 0) championshipRankRankChanges[i].text = $"<color=red>(▲{rankChange})</color>";
            else championshipRankRankChanges[i].text = $"<color=blue>(▼{-rankChange})</color>";
            championshipRankRanks[i].text = $"{championshipDatas[i].currentRank + 1}";
            championshipRankNames[i].StringReference = championshipDatas[i].SurvivorName;
            championshipRanks[i].GetComponent<Image>().color = championshipDatas[i].SurvivorName.TableEntryReference.Key == mySurvivorsData[0].localizedSurvivorName.TableEntryReference.Key ? new(1, 1, 1, 0.5f) : new(1, 1, 1, 0);
            int point = championshipDatas[i].points.Count > 0 ? championshipDatas[i].points[0] : 0;
            championshipRankDay1Points[i].text = $"{point}";
            championshipRankDay1PointDetails[i].text = point > 0 ? $"( {championshipDatas[i].points[0] - championshipDatas[i].killPoints[0]} + {championshipDatas[i].killPoints[0]} )" : "";
            point = championshipDatas[i].points.Count > 1 ? championshipDatas[i].points[1] : 0;
            championshipRankDay2Points[i].text = $"{point}";
            championshipRankDay2PointDetails[i].text = point > 0 ? $"( {championshipDatas[i].points[1] - championshipDatas[i].killPoints[1]} + {championshipDatas[i].killPoints[1]} )" : "";
            point = championshipDatas[i].points.Count > 2 ? championshipDatas[i].points[2] : 0;
            championshipRankDay3Points[i].text = $"{point}";
            championshipRankDay3PointDetails[i].text = point > 0 ? $"( {championshipDatas[i].points[2] - championshipDatas[i].killPoints[2]} + {championshipDatas[i].killPoints[2]} )" : "";
            championshipRankTotalPoints[i].text = $"{championshipDatas[i].TotalPoint}";
            championshipRankTotalPointDetails[i].text = championshipDatas[i].TotalPoint > 0 ? $"( {championshipDatas[i].TotalPoint - championshipDatas[i].TotalKillPoint} + {championshipDatas[i].TotalKillPoint} )" : "";
        }
    }

    public void SetChampionship(bool boolean)
    {
        championship = boolean;
        viewCurrentChampionshipStandings.SetActive(boolean);
    }

    public void ResetObjectiveText()
    {
        if (mySurvivorsData[0].haveQualifyToParticipateInSeasonChampionship)
        {
            if(calendar.Today < 87)
            {
                objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective4").GetLocalizedString()}";
            }
            else
            {
                objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective5").GetLocalizedString()}";
            }
        }
        else
        {
            objectiveText.text = mySurvivorsData[0].tier switch
            {
                Tier.Bronze => $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective1").GetLocalizedString()}",
                Tier.Silver => $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective2").GetLocalizedString()}",
                _ => $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective3").GetLocalizedString()}",
            };
        }
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
        alertWindow.GetComponentsInChildren<RectTransform>()[1].sizeDelta = new(1280, 720);
        LocalizeStringEvent localizeStringEvent = alertWindow.GetComponentInChildren<LocalizeStringEvent>();
        localizeStringEvent.enabled = false;
        localizeStringEvent.GetComponent<TextMeshProUGUI>().text = log;
        GameManager.Instance.openedWindows.Push(alertWindow);
    }

    void OnClick(InputValue value)
    {
        List<RaycastResult> results = Raycast();
        List<RaycastResult> championshipR = ChampionshipRaycast();
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

            index = championshipR.FindIndex(x => x.gameObject.CompareTag("ContestantUI"));
            if(index > -1)
            {
                for (int i = 0; i < contestants.Length; i++)
                {
                    int jndex = contestantsData.FindIndex(x => x.localizedSurvivorName.GetLocalizedString() == championshipR[index].gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
                    if (jndex > -1) selectedContestantData = contestantsData[jndex];
                }
                if (selectedContestantData != null)
                {
                    if (gameMode == GameMode.SingleCareerRun)
                    {
                        if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
                        {
                            championshipSurvivorInfo.SetInfo(selectedContestantData, false);
                        }
                    }
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

    List<RaycastResult> ChampionshipRaycast()
    {
        PointerEventData pointerData = new(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        championshipCanvasRaycaster.Raycast(pointerData, results);
        return results;
    }

    public IEnumerator LoadMySurvivorData(List<SurvivorData> data)
    {
        GameManager.ClaimLoadInfo("Loading survivors...", 0, 3);
        mySurvivorsData.Clear();
        mySurvivorsData = data;
        yield return null;
    }

    public void LoadData(ETCData saveData)
    {
        gameMode = saveData.gameMode;
        Difficulty = saveData.difficulty;
        Money = saveData.money;
        mySurvivorsId = saveData.mySurvivorsId;
        trainingLevel = saveData.trainingLevel;

        fightTrainingLevel = saveData.fightTrainingLevel;
        shootingTrainingLevel = saveData.shootingTrainingLevel;
        craftingTrainingLevel = saveData.craftingTrainingLevel;
        runningLevel = saveData.runningLevel;
        weightTrainingLevel = saveData.weightTrainingLevel;
        studyingLevel = saveData.studyingLevel;
        survivorHireLimit = saveData.survivorHireLimit;
        contestantsData = saveData.contestantsData;
        if (contestantsData != null && contestantsData.Count > 0 && calendar.LeagueReserveInfo.ContainsKey(calendar.Today)) contestantsData[0] = calendar.LeagueReserveInfo[calendar.Today].reserver != null ? MySurvivorsData.Find(x => x.id == calendar.LeagueReserveInfo[calendar.Today].reserver.id) : MySurvivorsData[0];
        
        championship = saveData.championship;
        championshipHeldCount = saveData.championshipHeldCount;
        championshipDatas = saveData.championshipDatas;
        viewCurrentChampionshipStandings.SetActive(championship);

        ResetData(gameMode, difficulty);

        for (int i = 0; i < trainingCards.Length; i++) 
        {
            trainingCards[i].SetCard(saveData.trainings[i]);
        }
        tutorial = false;

        survivorsInHireMarket[0].SetInfo(saveData.hireMarketSurvivorData[0], false);
        survivorsInHireMarket[1].SetInfo(saveData.hireMarketSurvivorData[1], false);
        survivorsInHireMarket[2].SetInfo(saveData.hireMarketSurvivorData[2], false);
        survivorsInHireMarket[0].SoldOut = saveData.soldOut[0];
        survivorsInHireMarket[1].SoldOut = saveData.soldOut[1];
        survivorsInHireMarket[2].SoldOut = saveData.soldOut[2];

        ResetObjectiveText();
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (survivorsDropdown.options.Count == 0) return;
        RelocalizeTrainingRoom();
        RelocalizeStrategyRoom();
        ResetSurvivorsDropdown();
        if (survivorsDropdown.options.Count == 0) return;
        SetOperatingRoom();
        SetStrategyRoom();
        sortContestantsListDropdown.RelocalizeOptions();

        ResetObjectiveText();
    }
}

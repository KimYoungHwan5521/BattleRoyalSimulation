using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UnityEngine.XR;

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
            if (value > 99999999) value = 99999999;
            money = value;
            if(money < 0) moneyText.text = $"<color=red>{money:###,###,###,##0}</color>";
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
    [SerializeField] TextMeshProUGUI trainingRoomLevelText;
    const float trainingGreatSuccessRate = 0.1f;
    public int trainingLevel = 1;
    readonly int[] facilityUpgradeCost = new[]{ 5000, 10000, 15000, 20000, 25000, 30000 };
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
    [SerializeField] GameObject trainingRoom;
    public TrainingCard[] trainingCards;
    [SerializeField] TextMeshProUGUI currentFacilityLevelText;
    [SerializeField] Button upgradeFacilityButton;

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
    [SerializeField] LocalizedDropdown weaponPriority2Dropdown;
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
    [SerializeField] LocalizedDropdown craftingPriority2Dropdown;

    [SerializeField] Transform craftingAllow;
    public List<GameObject> craftableAllows = new();

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
                return mySurvivorDataInBattleRoyale;
            }
        }
    }

    [Header("Daily Result")]
    //[SerializeField] GameObject buttonEndTheWeek;
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
    #endregion

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

    public void ResetData(int difficulty)
    {
        mySurvivorsData = new();
        hireSurvivor.SetActive(true);
        trainingLevel = 1;
        ResetHireMarket();
        Money = 1000;
        survivorHireLimit = 10;
        selectedSurvivor.ResetInfo();
        Difficulty = difficulty;

        tutorial = true;
    }

    void RelocalizeTrainingRoom()
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
            if ((totalRand < 120 || totalRand > 140) && check < 1000)
            {
                i--;
                check++;
                continue;
            }
            if (check >= 1000) Debug.LogWarning("Infinite loop detected");
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
                //(int)(value * value * totalRand),
                0,
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
        //OpenConfirmWindow($"Confirm:Purchase",
        //    () => {
        //        if(mySurvivorsData.Count >= survivorHireLimit)
        //        {
        //            Alert("Alert:Survivor limit reached.");
        //        }
        //        else if(money < survivorsInHireMarket[candidate].survivorData.price)
        //        {
        //            Alert("Alert:Not enough money.");
        //        }
        //        else
        //        {
        //            Money -= survivorsInHireMarket[candidate].survivorData.price;
                    mySurvivorsData.Add(new(survivorsInHireMarket[candidate].survivorData));
                    mySurvivorsData[mySurvivorsData.Count - 1].id = mySurvivorsId++;
                    mySurvivorsData[mySurvivorsData.Count - 1].characteristics = survivorsInHireMarket[candidate].survivorData.characteristics;
                    //mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;
                    survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";

                    if(mySurvivorsData.Count == 1)
                    {
                        survivorsDropdown.ClearOptions();
                        selectedSurvivor.SetInfo(mySurvivorsData[0], true);
                        trainingRoomAnim.SetBool("Tutorial", true);
                        Alert("Click the Training Room and assign survivors to training.");
                    }
                    else if (mySurvivorsData.Count >= 10)
                    {
                        AchievementManager.UnlockAchievement("Full House");
                    }
                    survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString() });
                    survivorsInHireMarket[candidate].SoldOut = true;

                    if (mySurvivorsData.Count == 1) ResetHireMarket();
                    hireSurvivor.SetActive(false);
                    ResetTrainingRoom();
                    GameManager.Instance.Save(0);
        //    }
        //},  $"{survivorsInHireMarket[candidate].survivorData.localizedSurvivorName.GetLocalizedString()}", $"{survivorsInHireMarket[candidate].survivorData.price}");

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
        selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value], true);
    }

    public void Rest()
    {
        if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        {
            if((calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship || calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join Championship");
                return;
            }
            if(calendar.Today > 77 && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join League");
                return;
            }
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
            
            value = Mathf.Min(value, 100 - mySurvivorsData[0].Stamina);
            mySurvivorsData[0].StaminaConsomtionReserve(value);
            trainingResult.SetActive(true);
            GameManager.Instance.openedWindows.Push(trainingResult);
            resultText.gameObject.SetActive(false);
            trainingResultText.text = new LocalizedString("Basic", "Rest").GetLocalizedString();
            trainingResultDetailText.text = $"{new LocalizedString("Basic", "Stamina").GetLocalizedString()} <color=#367D38>+{value}</color>";
            selectedSurvivor.StatIncreaseProduction();
        });
    }

    #region Training
    public void OpenTrainingRoom()
    {
        if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        {
            if ((calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship || calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join Championship");
                return;
            }
            if (calendar.Today > 77 && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join League");
                return;
            }
        }

        if (CheckHaveInjury(out int expectedDateOfFullyRecovery))
        {
            Alert("Alert:Can't Training", mySurvivorsData[0].localizedSurvivorName.GetLocalizedString(), $"{expectedDateOfFullyRecovery}");
        }
        else
        {
            trainingRoom.SetActive(true);
            RelocalizeTrainingRoom();
            GameManager.Instance.openedWindows.Push(trainingRoom);
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
        TrainingInfo training = trainingCards[focusedTraining].LinkedTraining;
        float failRate = 0;
        if (Stamina < training.staminaConsumtion) failRate = 1f;
        else if (Stamina < training.trainingDifficulty) failRate = 1f - (float)Stamina / training.trainingDifficulty;
        float rand = UnityEngine.Random.Range(0, 1f);
        trainingResult.SetActive(true);
        GameManager.Instance.openedWindows.Push(trainingResult);
        resultText.gameObject.SetActive(true);
        resultText.GetComponent<LocalizeStringEvent>().StringReference = new LocalizedString("Basic", "Training Results");
        if(rand < failRate)
        {
            // 실패
            trainingResultText.text = new LocalizedString("Basic", "Failed").GetLocalizedString();
            trainingResultDetailText.text = "";
            StaminaConsume(training);
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
        //foreach (var card in trainingCards) card.SetCard(card.LinkedTraining);
        trainingRoom.SetActive(false);
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
        selectedSurvivor.StatIncreaseProduction();
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

    public void ConfirmTrainingResult()
    {
        trainingResult.SetActive(false);
        DayEnd();
    }

    public void RequestUpgradeFacility()
    {
        if(Money < facilityUpgradeCost[trainingLevel - 1])
        {
            Alert("Alert:Not enough money.");
        }
        else
        {
            OpenConfirmWindow("Confirm:Upgrade Facility", () =>
            {
                Money -= facilityUpgradeCost[trainingLevel - 1];
                UpgradeFacility();
            }, "");
        }
    }

    public void UpgradeFacility()
    {
        trainingLevel++;
        //upgradeFacilityButton.GetComponentInChildren<TextMeshProUGUI>().text = new LocalizedString("Basic", "Facility Upgrade")
        //{
        //    Arguments = new[] { trainingLevel > facilityUpgradeCost.Length ? "" : $"{facilityUpgradeCost[trainingLevel - 1]}" }
        //}.GetLocalizedString();
        //currentFacilityLevelText.text = new LocalizedString("Basic", "Current Facility Level")
        //{
        //    Arguments = new[] { $"{trainingLevel}" }
        //}.GetLocalizedString();
        //if (trainingLevel > facilityUpgradeCost.Length) upgradeFacilityButton.gameObject.SetActive(false);
        Alert("Alert:Facility upgraded.");
    }
    #endregion

    #region Operating Room
    public void OpenOperatingRoom()
    {
        if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        {
            if ((calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship || calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join Championship");
                return;
            }
            if (calendar.Today > 77 && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert("Alert:Last Week Join League");
                return;
            }
        }

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
            AddTransplantSurgeryToSurgeryList(InjurySite.RightEye, 4000, 40000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftEye, 4000, 40000);
            AddTransplantSurgeryToSurgeryList(InjurySite.RightArm, 2500, 25000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftArm, 2500, 25000);
            AddTransplantSurgeryToSurgeryList(InjurySite.RightLeg, 5000, 50000);
            AddTransplantSurgeryToSurgeryList(InjurySite.LeftLeg, 5000, 50000);
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
        resultText.gameObject.SetActive(true);
        resultText.GetComponent<LocalizeStringEvent>().StringReference = new LocalizedString("Basic", "Surgery Result");
        trainingResultText.text = new LocalizedString("Basic", "Surgery Successful").GetLocalizedString();
        trainingResultDetailText.text = new LocalizedString("Injury", mySurvivorsData[0].localizedScheduledSurgeryName.TableEntryReference.Key) { Arguments = new[] { new LocalizedString("Injury", mySurvivorsData[0].surgerySite.ToString()).GetLocalizedString() } }.GetLocalizedString();
        Surgery(mySurvivorsData[0]);
        operatingRoom.SetActive(false);
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
        weaponPriority1Dropdown.ClearOptions();
        weaponPriority2Dropdown.ClearOptions();
        ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
        for(int i = (int)ItemManager.Items.Knife; i < (int)ItemManager.Items.Knife_Enchanted;  i++)
        {
            bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{items[i]}", out var itemSpriteEnum);
            weaponPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
            weaponPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Item", items[i].ToString()) });
            Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
            weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
            weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
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

            if (weaponPriority2Dropdown.dropdown.IsExpanded)
            {
                var dropdownSprites = weaponPriority2Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for (int i = 0; i < weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = weaponPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
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
        craftingPriority2Dropdown.ClearOptions();
        craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
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
            if (craftingPriority2Dropdown.dropdown.IsExpanded)
            {
                var dropdownSprites = craftingPriority2Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for (int i = 0; i < craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
                    if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
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
            weaponPriority2Dropdown.RelocalizeOptions();
        }
        weaponPriority1Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
        weaponPriority2Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority2Weapon - (int)ItemManager.Items.Knife;
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
        
        if(survivorWhoWantEstablishStrategy != null && survivorWhoWantEstablishStrategy.strategyDictionary != null)
        {
            sawAnEnemyAndItIsInAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].action;
            elseActionSawAnEnemyAndItIsInAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsInAttackRange].elseAction;
            sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].action;
            elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange].elseAction;
            heardDistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardDistinguishableSound].action;
            elseActionHeardDistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardDistinguishableSound].elseAction;
            heardIndistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardIndistinguishableSound].action;
            elseActionHeardIndistinguishableSoundDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.HeardIndistinguishableSound].elseAction;
            whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = survivorWhoWantEstablishStrategy.strategyDictionary[StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget].action;
        }

        craftingPriority1Dropdown.RelocalizeOptions();
        craftingPriority2Dropdown.RelocalizeOptions();
    }

    public void SetDefault()
    {
        weaponPriority1Dropdown.dropdown.value = (int)ItemManager.Items.LASER - (int)ItemManager.Items.Knife;
        weaponPriority2Dropdown.dropdown.value = (int)ItemManager.Items.AssaultRifle - (int)ItemManager.Items.Knife;
        Image selectedWeaponPriority1Image = weaponPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        selectedWeaponPriority1Image.sprite = ResourceManager.Get(ResourceEnum.Sprite.AssaultRifle);
        selectedWeaponPriority1Image.GetComponent<AspectRatioFitter>().aspectRatio
            = selectedWeaponPriority1Image.sprite.textureRect.width / selectedWeaponPriority1Image.sprite.textureRect.height;
        selectedWeaponPriority1Image = weaponPriority2Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        selectedWeaponPriority1Image.sprite = ResourceManager.Get(ResourceEnum.Sprite.AssaultRifle);
        selectedWeaponPriority1Image.GetComponent<AspectRatioFitter>().aspectRatio
            = selectedWeaponPriority1Image.sprite.textureRect.width / selectedWeaponPriority1Image.sprite.textureRect.height;
        foreach (Strategy strategy in strategies) strategy.SetDefault();
        sawAnEnemyAndItIsInAttackRangeDropdown.value = 0;
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = 0;
        heardDistinguishableSoundDropdown.value = 0;
        heardIndistinguishableSoundDropdown.value = 1;
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = 1;
        craftingPriority1Dropdown.dropdown.value = 0;
        craftingPriority2Dropdown.dropdown.value = 0;
        foreach(var craftableAllow in craftableAllows) craftableAllow.GetComponentsInChildren<Toggle>()[0].isOn = true;
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
        weaponPriority1Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
        craftingPriority1Dropdown.ClearOptions();
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        weaponPriority2Dropdown.dropdown.value = (int)survivorWhoWantEstablishStrategy.priority2Weapon - (int)ItemManager.Items.Knife;
        craftingPriority2Dropdown.ClearOptions();
        craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Clear();
        craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString> { new("Basic", "None") });
        craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(null);
        foreach (var craftable in ItemManager.craftables)
        {
            bool trapExpertAndTraps = survivorWhoWantEstablishStrategy.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (craftable.itemType == ItemManager.Items.BearTrap || craftable.itemType == ItemManager.Items.NoiseTrap || craftable.itemType == ItemManager.Items.ShrapnelTrap || craftable.itemType == ItemManager.Items.ChemicalTrap || craftable.itemType == ItemManager.Items.ExplosiveTrap || craftable.itemType == ItemManager.Items.TrapDetectionDevice);
            if (craftable.requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge || trapExpertAndTraps)
            {
                bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{craftable.itemType}", out var itemSpriteEnum);
                craftingPriority1Dropdown.AddLocalizedOptions(new List<LocalizedString>{ new LocalizedString("Item", craftable.itemType.ToString()) });
                craftingPriority2Dropdown.AddLocalizedOptions(new List<LocalizedString>{ new LocalizedString("Item", craftable.itemType.ToString()) });
                Sprite sprite = spriteNotNull ? ResourceManager.Get(itemSpriteEnum) : ResourceManager.Get(ResourceEnum.Sprite.Unknown);
                craftingPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
                craftingPriority2Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
            }
        }
        craftingPriority1Dropdown.dropdown.value = survivorWhoWantEstablishStrategy.priority1CraftingToInt + 1;
        CraftingPriority1Changed();
        craftingPriority2Dropdown.dropdown.value = survivorWhoWantEstablishStrategy.priority2CraftingToInt + 1;
        CraftingPriority2Changed();

        for(int i=0; i<survivorWhoWantEstablishStrategy.craftingAllows.Length; i++)
        {
            craftableAllows[i].SetActive(ItemManager.craftables[i].requiredKnowledge <= survivorWhoWantEstablishStrategy.Knowledge);
            if (survivorWhoWantEstablishStrategy.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1 && (ItemManager.craftables[i].itemType == ItemManager.Items.BearTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ShrapnelTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ChemicalTrap || ItemManager.craftables[i].itemType == ItemManager.Items.NoiseTrap || ItemManager.craftables[i].itemType == ItemManager.Items.ExplosiveTrap || ItemManager.craftables[i].itemType == ItemManager.Items.TrapDetectionDevice))
                craftableAllows[i].SetActive(true);
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
            if (strategy.ActionDropdown != null) strategy.ActionDropdown.value = strategyDictionary.Value.action;
            if (strategy.ElseActionDropdown != null) strategy.ElseActionDropdown.value = strategyDictionary.Value.elseAction;
            if (strategy.IntagerInput != null)
            {
                strategy.IntagerInput.text = strategyDictionary.Value.action.ToString();
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
                if (craftableAllow.GetComponentInChildren<LocalizeStringEvent>().StringReference.TableEntryReference.Key == $"{craftingPriority1Dropdown.keys[craftingPriority1Dropdown.dropdown.value - 1].TableEntryReference.Key}"
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
                    survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] = new(strategy.ActionDropdown != null ? strategy.ActionDropdown.value : 0, strategy.ElseActionDropdown != null ? strategy.ElseActionDropdown.value : 0, 0);
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
                    new(strategy.ActionDropdown.value, strategy.ElseActionDropdown.value, strategy.activeConditionCount, conditionData);
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

    public int MeasureTreatmentCost(Injury injury)
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
                cost = injury.degree * 50;
                if (injury.degree == 1) cost += 100;
                break;
            case InjurySite.RightEye:
            case InjurySite.LeftEye:
                cost = injury.degree * 50;
                if (injury.degree == 1) cost += 400;
                break;
            case InjurySite.Chest:
            case InjurySite.Ribs:
            case InjurySite.Abdomen:
                cost = injury.degree * 100;
                break;
            case InjurySite.Organ:
                cost = injury.degree * 300;
                if (injury.degree == 1) cost += 600;
                break;
            case InjurySite.RightLeg:
            case InjurySite.LeftLeg:
                cost = injury.degree * 100;
                if (injury.degree == 1) cost += 500;
                break;
            case InjurySite.RightArm:
            case InjurySite.LeftArm:
            case InjurySite.RightKnee:
            case InjurySite.LeftKnee:
                cost = injury.degree * 50;
                if (injury.degree == 1) cost += 250;
                break;
            case InjurySite.RightHand:
            case InjurySite.LeftHand:
            case InjurySite.RightFoot:
            case InjurySite.LeftFoot:
                cost = injury.degree * 25;
                if (injury.degree == 1) cost += 100;
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
            case InjurySite.RightIndexToe:
            case InjurySite.LeftIndexToe:
            case InjurySite.RightMiddleToe:
            case InjurySite.LeftMiddleToe:
            case InjurySite.RightRingToe:
            case InjurySite.LeftRingToe:
            case InjurySite.RightLittleToe:
            case InjurySite.LeftLittleToe:
                cost = injury.degree * 10;
                if (injury.degree == 1) cost += 10;
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
        GameManager.Instance.Option.SetSaveButtonInteractable(false);
        mySurvivorDataInBattleRoyale = calendar.LeagueReserveInfo[calendar.Today].reserver;
        if (mySurvivorDataInBattleRoyale == null) AchievementManager.UnlockAchievement("Spectator");
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
        contestantsData = null;
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
        contestantsData = null;
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
        //buttonEndTheWeek.SetActive(!hide);
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
        while (check < 1000)
        {
            int randStrength = UnityEngine.Random.Range(0, 101);
            int randAgility = UnityEngine.Random.Range(0, 101);
            int randFighting = UnityEngine.Random.Range(0, 101);
            int randShooting = UnityEngine.Random.Range(0, 101);
            int randCrafting = UnityEngine.Random.Range(0, 101);
            int randKnowledge = UnityEngine.Random.Range(0, 101);
            int totalRand = randStrength + randAgility + randFighting + randShooting + randCrafting + randKnowledge;
            if ((totalRand < value * 60 + difficulty * (value * value + 10) || totalRand > (value + 1) * 60 + difficulty * (value * value + 10)) && check < 1000)
            {
                check++;
                continue;
            }
            if (check >= 1000) Debug.LogWarning("Infinite roof has detected");
            SurvivorData survivorData = new(
                GetRandomName(),
                randStrength,
                randAgility,
                randFighting,
                randShooting,
                randCrafting,
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
            CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicCount, false);

            survivorData.priority1Weapon = ItemManager.Items.LASER;
            survivorData.priority2Weapon = ItemManager.Items.AssaultRifle;
            return survivorData;
        }
        return new(GetRandomName(), 20 * value, 20 * value, 20 * value, 20 * value, 20 * value, 20 * value, 100, calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league));
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
        yield return null;
    }

    public void LoadData(int difficulty, int money, int mySurvivorsId, int trainingLevel, List<TrainingInfo> trainingInfos, int survivorHireLimit, List<SurvivorData> contestantsData)
    {
        Difficulty = difficulty;
        Money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.trainingLevel = trainingLevel;
        this.survivorHireLimit = survivorHireLimit;
        this.contestantsData = contestantsData;

        for (int i = 0; i < trainingCards.Length; i++) 
        {
            trainingCards[i].SetCard(trainingInfos[i]);
        }
        tutorial = false;
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
    }
}

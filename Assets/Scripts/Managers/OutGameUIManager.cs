using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public enum Training { None, Weight, Running, Fighting, Shooting, Studying }

public enum SurgeryType { Transplant, ChronicDisorderTreatment, Alteration }

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
        }
    }

    [Header("Survivors / Hire Market")]
    int mySurvivorsId = 0;
    public int MySurvivorsId => mySurvivorsId;
    [SerializeField] GameObject hireSurvivor;
    [SerializeField] GameObject hireClose;
    [SerializeField] SurvivorInfo[] survivorsInHireMarket;
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
    [SerializeField] TextMeshProUGUI weightTrainingNameText;
    [SerializeField] TextMeshProUGUI runningNameText;
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI studyingNameText;
    int fightTrainingLevel = 1;
    int shootingTrainingLevel = 1;
    int runningLevel = 1;
    int weightTrainingLevel = 1;
    int studyingLevel = 1;

    public int FightTrainingLevel => fightTrainingLevel;
    public int ShootingTrainingLevel => shootingTrainingLevel;
    public int AgilityTrainingLevel => runningLevel;
    public int WeightTrainingLevel => weightTrainingLevel;
    public int StudyLevel => studyingLevel;
    readonly int[] facilityUpgradeCost = { 1000, 3000, 10000, 30000 };
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
    List<SurvivorSchedule> survivorSchedules;
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

    [SerializeField] TMP_Dropdown weaponPriority1Dropdown;
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

    struct SurgeryInfo
    {
        public string surgeryName;
        public int surgeryCost;
        public InjurySite surgerySite;
        public SurgeryType surgeryType;
        public CharacteristicType surgeryCharacteristic;

        public SurgeryInfo(string surgeryName, int surgeryCost, InjurySite surgerySite, SurgeryType surgeryType, CharacteristicType surgeryCharacteristic = CharacteristicType.BadEye)
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
    SurvivorData mySurvivorDataInBattleRoyale;
    public SurvivorData MySurvivorDataInBattleRoyale => mySurvivorDataInBattleRoyale;

    [Header("Daily Result")]
    [SerializeField] GameObject dailyResult;
    [SerializeField] GameObject[] survivorTrainingResults;
    TextMeshProUGUI[][] resultTexts;

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
    public List<SurvivorData> contestantsData;
    [SerializeField] GameObject draggingContestant;
    string[] predictions;
    public string[] Predictions => predictions;
    [SerializeField] TMP_InputField bettingAmountInput;
    int bettingAmount;
    public int BettingAmount => bettingAmount;
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
        predictions = new string[5];
        GameManager.Instance.ObjectStart += () => InitializeStrategyRoom();
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
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 25, 25, 20, 20, 20, 0, 100, Tier.Bronze);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 20, 20, 25, 25, 20, 0, 100, Tier.Bronze);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 20, 20, 20, 20, 30, 0, 100, Tier.Bronze);
    }

    public void ResetHireMarket()
    {
        float value = (fightTrainingLevel + shootingTrainingLevel + runningLevel + weightTrainingLevel) / 4f;
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            int randStrength = UnityEngine.Random.Range(0, 100);
            int randAgility = UnityEngine.Random.Range(0, 100);
            int randFighting = UnityEngine.Random.Range(0, 100);
            int randShooting = UnityEngine.Random.Range(0, 100);
            int randKnowledge = UnityEngine.Random.Range(0, 100);
            int totalRand = randStrength + randAgility + randFighting + randShooting + randKnowledge;
            if ((totalRand < value * 70f || totalRand > value * 130f) && check < 1000)
            {
                i--;
                check++;
                continue;
            }
            if (check >= 1000) Debug.LogWarning("Infinite loop detected");
            check = 0;
            survivorsInHireMarket[i].SetInfo(GetRandomName(),
                100 + randStrength,
                randAgility,
                randFighting,
                randShooting,
                randKnowledge,
                UnityEngine.Random.Range(0, 4),
                (int)(value * value * value * 100 * totalRand / 120f),
                Tier.Bronze);
            survivorsInHireMarket[i].SoldOut = false;
        }
    }

    public void HireSurvivor(int candidate)
    {
        OpenConfirmWindow($"Are you sure to hire \"{survivorsInHireMarket[candidate].survivorData.survivorName}\" for $ {survivorsInHireMarket[candidate].survivorData.price} ?",
            () => {
                if(mySurvivorsData.Count >= survivorHireLimit)
                {
                    Alert("The survivor retention limit has been reached.");
                }
                else if(money < survivorsInHireMarket[candidate].survivorData.price)
                {
                    Alert("Not enough money.");
                }
                else
                {
                    Money -= survivorsInHireMarket[candidate].survivorData.price;
                    mySurvivorsData.Add(new(survivorsInHireMarket[candidate].survivorData));
                    mySurvivorsData[mySurvivorsData.Count - 1].id = mySurvivorsId++;
                    mySurvivorsData[mySurvivorsData.Count - 1].characteristics = survivorsInHireMarket[candidate].survivorData.characteristics;
                    mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;
                    survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";

                    if(mySurvivorsData.Count == 1)
                    {
                        survivorsDropdown.ClearOptions();
                        selectedSurvivor.SetInfo(mySurvivorsData[0], true);
                        hireClose.SetActive(true);
                    }
                    survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].survivorData.survivorName });
                    survivorsDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    selectSurvivorGetSurgeryDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    selectSurvivorEstablishStrategyDropdown.template.sizeDelta = new(0, Mathf.Min(50 * survivorsDropdown.options.Count, 600));
                    survivorsInHireMarket[candidate].SoldOut = true;

                    if (mySurvivorsData.Count == 1) ResetHireMarket();
                    hireSurvivor.SetActive(false);
                }
            });
        
    }

    string GetRandomName(int depth = 0)
    {
        if (depth > 100)
        {
            Debug.LogWarning("Infinite recursion detected");
            return "(Failed to load name)";
        }

        string candidate = Names.SurvivorName[UnityEngine.Random.Range(0, Names.SurvivorName.Length)];
        for (int i = 0; i < 3; i++)
            if (survivorsInHireMarket[0].survivorData.survivorName == candidate) return GetRandomName(depth++);
        for (int i = 0; i < mySurvivorsData.Count; i++)
            if (mySurvivorsData[i].survivorName == candidate) return GetRandomName(depth++);
        for(int i = 0; i < contestantsData.Count; i++)
            if(contestantsData[i].survivorName == candidate) return GetRandomName(depth++);
        return candidate;
    }

    public void ResetSurvivorsDropdown()
    {
        survivorsDropdown.ClearOptions();
        for(int i = 0; i<mySurvivorsData.Count; i++)
        {
            survivorsDropdown.AddOptions(new List<string>(new string[] { mySurvivorsData[i].survivorName }));
        }
        ResetSelectedSurvivorInfo();
        survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";
    }

    public void DismissSurvivor()
    {
        if(mySurvivorsData.Count < 2)
        {
            Alert("There must be at least one survivor left.");
        }
        else
        {
            SurvivorData wantDismiss = mySurvivorsData[survivorsDropdown.value];
            OpenConfirmWindow($"Are you sure to dismiss <i>{wantDismiss.survivorName}</i>?", () =>
                {
                    mySurvivorsData.Remove(wantDismiss);
                    ResetSurvivorsDropdown();
                    survivorCountText.text = $"( {mySurvivorsData.Count} / {survivorHireLimit} )";
                    Alert("The survivor has dismissed");
                });
        }
    }
    #endregion

    public void OnSurvivorSelected()
    {
        SurvivorData survivorInfo = mySurvivorsData.Find(x => x.survivorName == survivorsDropdown.options[survivorsDropdown.value].text);
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
            Alert("The training room is closed on weekendns");
        }
        else
        {
            trainingRoom.SetActive(true);
        }
    }

    public void OpenAssignTraining(int trainingIndex)
    {
        Training training = (Training)trainingIndex;
        survivorSchedules = new();
        assignTrainingNameText.text = trainingIndex switch
        {
            1 => "Weight Training",
            2 => "Running",
            3 => "Fight Training",
            4 => "Shooting Training",
            5 => "Studying",
            _ => "?",
        };
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
            bool assignable = true;
            if (survivor.assignedTraining == training)
            {
                fitParent = survivorsAssignedThis;
                targetParent = survivorsWithoutSchedule;
            }
            else if (survivor.assignedTraining == Training.None)
            {
                fitParent = survivorsWithoutSchedule;
                targetParent = survivorsAssignedThis;
                if (!Trainable(survivor, training, out string cause))
                {
                    assignable = false;
                    description = $"{survivor.survivorName} can't cannot be assigned to this training due to injury.\n<color=red><i>Cause : {cause}</i></color>";
                }
            }
            else
            {
                fitParent = survivorsWithOtherSchedule;
                targetParent = survivorsAssignedThis;
                description = $"{survivor.survivorName} is assigned to {survivor.assignedTraining} training.";
            }
            survivorSchedule = PoolManager.Spawn(ResourceEnum.Prefab.SurvivorSchedule, fitParent).GetComponent<SurvivorSchedule>();
            survivorSchedule.SetSurvivorData(survivor, training, assignable, fitParent, targetParent);
            survivorSchedule.GetComponent<Help>().SetDescription(description);
            survivorSchedule.GetComponent<Button>().enabled = assignable;
            survivorSchedules.Add(survivorSchedule);
        }
    }

    public void ConfirmAssignTraining()
    {
        foreach(var survivorSchedule in survivorSchedules)
        {
            survivorSchedule.survivor.assignedTraining = survivorSchedule.whereAmI;
        }

        weightTrainingBookers.text = "";
        runningBookers.text = "";
        fightTrainingBookers.text = "";
        shootingTrainingBookers.text = "";
        studyingBookers.text = "";
        foreach(SurvivorData survivor in mySurvivorsData)
        {
            TextMeshProUGUI targetText;
            switch(survivor.assignedTraining)
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
                targetText.text += $"{survivor.survivorName}";
            }
        }
        foreach (var scrollRect in bookedTodayScrollRects)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.GetComponent<RectTransform>());
        }
    }

    public void CheckTrainable(SurvivorData survivor)
    {
        if (Trainable(survivor, survivor.assignedTraining, out string cause)) return;
        else
        {
            survivor.assignedTraining = Training.None;
            ConfirmAssignTraining();
            Alert($"{survivor.survivorName} was released from training assignment due to injury.\n<color=red><i>Cause : {cause}</i></color>");
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
            cause = "Surgery scheduled";
            return false;
        }

        int eyeInjury = 0;
        foreach(Injury injury in survivor.injuries)
        {
            if (injury.type == InjuryType.ArtificalPartsTransplanted) continue;
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
                        case InjurySite.RightAncle:
                        case InjurySite.LeftAncle:
                            cause = $"{injury.site} {injury.type}";
                            return false;
                        default:
                            if (injury.degree < 1)
                            {
                                cause = $"{injury.site} {injury.type}";
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
                            cause = $"{injury.site} {injury.type}";
                            return false;
                        case InjurySite.Organ:
                            if(injury.degree >= 1)
                            {
                                cause = $"{injury.site} {injury.type}";
                                return false;
                            }
                            break;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            eyeInjury++;
                            if (eyeInjury >= 2)
                            {
                                cause = $"Both eyes injured";
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
                        case InjurySite.Libs:
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
                        case InjurySite.RightAncle:
                        case InjurySite.LeftAncle:
                        case InjurySite.RightBigToe:
                        case InjurySite.LeftBigToe:
                            cause = $"{injury.site} {injury.type}";
                            return false;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            eyeInjury++;
                            if (eyeInjury >= 2)
                            {
                                cause = $"Both eyes injured";
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
                        case InjurySite.Libs:
                        case InjurySite.Abdomen:
                        case InjurySite.Organ:
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cause = $"{injury.site} {injury.type}";
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
                OpenConfirmWindow("Are you sure to upgrade Weight Training Facilities", () =>
                {
                    if (money < facilityUpgradeCost[weightTrainingLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[weightTrainingLevel - 1];
                        weightTrainingLevel++;
                        fightTrainingNameText.text = $"Weight Training (lv {weightTrainingLevel})";
                        if (weightTrainingLevel > facilityUpgradeCost.Length) weightTrainingUpgradeButtion.SetActive(false);
                        else weightTrainingUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade Facilities\n$ {facilityUpgradeCost[weightTrainingLevel - 1]}";
                    }
                });
                break;
            case 1:
                OpenConfirmWindow("Are you sure to upgrade Agility Training Facilities?", () =>
                {
                    if (money < facilityUpgradeCost[runningLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[runningLevel - 1];
                        runningLevel++;
                        fightTrainingNameText.text = $"Running (lv {runningLevel})";
                        if (runningLevel > facilityUpgradeCost.Length) runningUpgradeButtion.SetActive(false);
                        else runningUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade Facilities\n$ {facilityUpgradeCost[runningLevel - 1]}";
                    }
                });
                break;
            case 2:
                OpenConfirmWindow("Are you sure to upgrade Fight Training Facilities?", ()=>
                {
                    if(money < facilityUpgradeCost[fightTrainingLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[fightTrainingLevel - 1];
                        fightTrainingLevel++;
                        fightTrainingNameText.text = $"Fight Training (lv {fightTrainingLevel})";
                        if (fightTrainingLevel > facilityUpgradeCost.Length) fightTrainingUpgradeButtion.SetActive(false);
                        else fightTrainingUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade Facilities\n$ {facilityUpgradeCost[fightTrainingLevel - 1]}";
                    }
                });
                break;
            case 3:
                OpenConfirmWindow("Are you sure to upgrade Shooting Training Facilities?", () =>
                {
                    if (money < facilityUpgradeCost[shootingTrainingLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[shootingTrainingLevel - 1];
                        shootingTrainingLevel++;
                        fightTrainingNameText.text = $"Shooting Training (lv {shootingTrainingLevel})";
                        if (shootingTrainingLevel > facilityUpgradeCost.Length) shootingTrainingUpgradeButtion.SetActive(false);
                        else shootingTrainingUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade Facilities\n$ {facilityUpgradeCost[shootingTrainingLevel - 1]}";
                    }
                });
                break;
            case 4:
                OpenConfirmWindow("Are you sure to upgrade Studying Facilities?", () =>
                {
                    if (money < facilityUpgradeCost[studyingLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[shootingTrainingLevel - 1];
                        shootingTrainingLevel++;
                        studyingNameText.text = $"Studying (lv {studyingLevel})";
                        if (studyingLevel > facilityUpgradeCost.Length) studyingUpgradeButtion.SetActive(false);
                        else studyingUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Upgrade Facilities\n$ {facilityUpgradeCost[studyingLevel - 1]}";
                    }
                });
                break;
        }
    }
    #endregion

    #region Operating Room
    public void OpenOperatingRoom()
    {
        if (calendar.Today % 7 > 4)
        {
            Alert("The operating room is closed on weekendns");
        }
        else
        {
            SetOperatingRoom();
            operatingRoom.SetActive(true);
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
        survivorWhoWantSurgery = MySurvivorsData.Find(x => x.survivorName == selectSurvivorGetSurgeryDropdown.options[selectSurvivorGetSurgeryDropdown.value].text);
        
        if(survivorWhoWantSurgery.surgeryScheduled)
        {
            scheduledSurgery.SetActive(true);
            buttonCancelSurgery.SetActive(true);
            selectSurgery.SetActive(false);
            buttonScheduleSurgery.SetActive(false);
            scheduledSurgery.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{survivorWhoWantSurgery.scheduledSurgeryName}";
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
        string surgeryName;
        int cost = 0;
        if(surgeryType_Transplantation.isOn)
        {
            foreach(Injury injury in survivorWhoWantSurgery.injuries)
            {
                if(injury.degree >= 1)
                {
                    surgeryName = $"Artifical {injury.site} transplant";
                    switch(injury.site)
                    {
                        case InjurySite.RightBigToe:
                        case InjurySite.LeftBigToe:
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
                            cost = 100;
                            break;
                        case InjurySite.RightHand:
                        case InjurySite.LeftHand:
                            cost = 500;
                            break;
                        case InjurySite.RightArm:
                        case InjurySite.LeftArm:
                            cost = 1000;
                            break;
                        case InjurySite.RightAncle:
                        case InjurySite.LeftAncle:
                            string side = injury.site == InjurySite.RightAncle ? "right" : "left";
                            surgeryName = $"Artifical foot({side}) transplant";
                            cost = 500;
                            break;
                        case InjurySite.RightKnee:
                        case InjurySite.LeftKnee:
                            side = injury.site == InjurySite.RightKnee ? "right" : "left";
                            surgeryName = $"Artifical leg({side}, under knee) transplant";
                            cost = 1000;
                            break;
                        case InjurySite.RightLeg:
                        case InjurySite.LeftLeg:
                            side = injury.site == InjurySite.RightLeg ? "right" : "left";
                            surgeryName = $"Artifical leg({side}) transplant";
                            cost = 2000;
                            break;
                        case InjurySite.RightEye:
                        case InjurySite.LeftEye:
                            side = injury.site == InjurySite.RightEye ? "right" : "left";
                            surgeryName = $"Artifical eye({side}) transplant";
                            cost = 3000;
                            break;
                        case InjurySite.RightEar:
                        case InjurySite.LeftEar:
                            side = injury.site == InjurySite.RightEar ? "right" : "left";
                            surgeryName = $"Artifical ear({side}) transplant";
                            cost = 1000;
                            break;
                        case InjurySite.Organ:
                            surgeryName = $"Artifical organ transplant";
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
                switch(characteristic.type)
                {
                    case CharacteristicType.BadEye:
                        surgeryList.Add(new("Bad eye treatment", 2000, InjurySite.LeftEye, SurgeryType.ChronicDisorderTreatment, CharacteristicType.BadEye));
                        break;
                    case CharacteristicType.BadHearing:
                        surgeryList.Add(new("Bad hearing treatment", 700, InjurySite.LeftEar, SurgeryType.ChronicDisorderTreatment, CharacteristicType.BadHearing));
                        break;
                    default:
                        break;
                }
            }
        }

        for(int i = 0; i < surgeries.Length; i++)
        {
            if(i < surgeryList.Count)
            {
                surgeries[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = surgeryList[i].surgeryName;
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

        survivorWhoWantSurgery.scheduledSurgeryName = surgeryList[index].surgeryName;
        survivorWhoWantSurgery.shceduledSurgeryCost = surgeryList[index].surgeryCost;
        survivorWhoWantSurgery.surgerySite = surgeryList[index].surgerySite;
        survivorWhoWantSurgery.surgeryType = surgeryList[index].surgeryType;
        survivorWhoWantSurgery.surgeryCharacteristic = surgeryList[index].surgeryCharacteristic;
        OpenConfirmWindow($"Do you confirm surgery?\n{survivorWhoWantSurgery.survivorName} : {survivorWhoWantSurgery.scheduledSurgeryName}($ {survivorWhoWantSurgery.shceduledSurgeryCost})", ()=>
        {
            if(money < survivorWhoWantSurgery.shceduledSurgeryCost)
            {
                Alert("Not enough money.");
            }
            else
            {
                if (survivorWhoWantSurgery.assignedTraining != Training.None)
                {
                    ConfirmAssignTraining();
                    Alert($"{survivorWhoWantSurgery.survivorName}'s training has canceled");
                }
                survivorWhoWantSurgery.assignedTraining = Training.None;
                survivorWhoWantSurgery.surgeryScheduled = true;
                Money -= survivorWhoWantSurgery.shceduledSurgeryCost;
                SelectSurvivorToSurgery();
                Alert("Surgery scheduled.");
            }
        });
    }

    public void CancelSurgery()
    {
        OpenConfirmWindow("Are you sure to cancel his surgery?", () =>
        {
            survivorWhoWantSurgery.surgeryScheduled = false;
            Money += survivorWhoWantSurgery.shceduledSurgeryCost;
            survivorWhoWantSurgery.shceduledSurgeryCost = 0;
            SelectSurvivorToSurgery();
            Alert("Surgery canceled.");
        });
    }
    #endregion

    #region Strategy Room
    void InitializeStrategyRoom()
    {
        search.onValueChanged.AddListener((value) => Search(value));
        weaponPriority1Dropdown.ClearOptions();
        ItemManager.Items[] items = (ItemManager.Items[])Enum.GetValues(typeof(ItemManager.Items));
        for(int i = (int)ItemManager.Items.Knife; i < (int)ItemManager.Items.Bullet_Revolver;  i++)
        {
            bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{items[i]}", out var itemSpriteEnum);
            TMP_Dropdown.OptionData optionData;
            optionData = new(items[i].ToString());
            weaponPriority1Dropdown.AddOptions(new List<TMP_Dropdown.OptionData>(new TMP_Dropdown.OptionData[] { optionData }));
            if (spriteNotNull) weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Add(ResourceManager.Get(itemSpriteEnum));
        }
        GameManager.Instance.ObjectUpdate += () => 
        { 
            if(weaponPriority1Dropdown.IsExpanded)
            {
                var dropdownSprites = weaponPriority1Dropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for(int i=0; i<weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = weaponPriority1Dropdown.GetComponent<DropdownSpritesData>().sprites[i];
                    image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                }
            }
        };
        sawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Attack", "Ignore", "Try run away" }));
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Attack", "Ignore", "Try run away" }));
        
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Approach", "Ignore", "Try run away" }));
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Approach", "Ignore", "Try run away" }));

        heardDistinguishableSoundDropdown.ClearOptions();
        heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        
        heardIndistinguishableSoundDropdown.ClearOptions();
        heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.ClearOptions();
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.AddOptions(new List<string>(new string[] { "Who first Seen.", "The closest one.", "Whose weapon's range is longest." }));
        SetDefault();
    }

    public void SetDefault()
    {
        weaponPriority1Dropdown.value = (int)ItemManager.Items.SniperRifle - (int)ItemManager.Items.Knife;
        Image selectedWeaponPriority1Image = weaponPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
        selectedWeaponPriority1Image.sprite = ResourceManager.Get(ResourceEnum.Sprite.SniperRifle);
        selectedWeaponPriority1Image.GetComponent<AspectRatioFitter>().aspectRatio
            = selectedWeaponPriority1Image.sprite.textureRect.width / selectedWeaponPriority1Image.sprite.textureRect.height;
        foreach(Strategy strategy in strategies) strategy.SetDefault();
        sawAnEnemyAndItIsInAttackRangeDropdown.value = 0;
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.value = 0;
        heardDistinguishableSoundDropdown.value = 0;
        heardIndistinguishableSoundDropdown.value = 1;
        whenThereAreMultipleEnemiesInSightWhoIsTheTargetDropdown.value = 0;
    }

    public void OpenStrategyRoom()
    {
        SetStrategyRoom();
        strategyRoom.SetActive(true);
    }

    public void CloseStrategyRoom()
    {
        bool hasChanged = false;
        foreach(Strategy strategy in strategies) if(strategy.hasChanged) { hasChanged = true;  break; }
        if(hasChanged) OpenConfirmWindow("Close the strategy room?\n(Unsaved content will be deleted)", () => { strategyRoom.SetActive(false); });
        else strategyRoom.SetActive(false);
    }

    void SetStrategyRoom()
    {
        selectSurvivorEstablishStrategyDropdown.ClearOptions();
        selectSurvivorEstablishStrategyDropdown.AddOptions(survivorsDropdown.options);
        SelectSurvivorToEstablishStrategy();
    }

    public void SelectSurvivorToEstablishStrategy()
    {
        survivorInfoEstablishStrategy.SetInfo(MySurvivorsData[selectSurvivorEstablishStrategyDropdown.value], false);
        survivorWhoWantEstablishStrategy = MySurvivorsData.Find(x => x.survivorName == selectSurvivorEstablishStrategyDropdown.options[selectSurvivorEstablishStrategyDropdown.value].text);
        weaponPriority1Dropdown.value = (int)survivorWhoWantEstablishStrategy.priority1Weapon - (int)ItemManager.Items.Knife;
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
        if(string.IsNullOrEmpty(word))
        {
            foreach(Strategy strategy in strategies) strategy.gameObject.SetActive(true);
        }
        else
        {
            foreach (Strategy strategy in strategies) strategy.gameObject.SetActive(strategy.CaseName.ToUpper().Contains(word.ToUpper()));
        }
    }

    public void WeaponPriorityChanged()
    {
        bool spriteNotNull = Enum.TryParse<ResourceEnum.Sprite>($"{weaponPriority1Dropdown.options[weaponPriority1Dropdown.value].text}", out var itemSpriteEnum);
        if(spriteNotNull)
        {
            Image image = weaponPriority1Dropdown.transform.Find("SizeBox").Find("Sprite").GetComponent<Image>();
            image.sprite = ResourceManager.Get(itemSpriteEnum);
            image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
        }
    }

    public void SaveStrategy()
    {
        OpenConfirmWindow("Save all changes?", () =>
        {
            bool itemNotNull = Enum.TryParse<ItemManager.Items>($"{weaponPriority1Dropdown.options[weaponPriority1Dropdown.value].text}", out var itemEnum);
            if (itemNotNull) survivorWhoWantEstablishStrategy.priority1Weapon = itemEnum;
            else Debug.LogWarning($"Item enum not found : {weaponPriority1Dropdown.options[weaponPriority1Dropdown.value].text}");

            foreach(Strategy strategy in strategies)
            {
                if(strategy.NoCondition)
                {
                    survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] = new(sawAnEnemyAndItIsInAttackRangeDropdown.value, 0, 0);
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
            case InjurySite.Libs:
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
            case InjurySite.RightAncle:
            case InjurySite.LeftAncle:
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
        mySurvivorDataInBattleRoyale = calendar.LeagueReserveInfo[calendar.Today].reserver;
        StartCoroutine(GameManager.Instance.BattleRoyaleStart());
    }

    #region Betting
    public void OpenBettingRoom()
    {
        contestantsData = new();
        bettingAmountInput.text = "0";
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
        }
        for (int i= index; i<needSurvivorNumber; i++)
        {
            contestantsData.Add(CreateRandomSurvivorData());
        }

        for(int i=0; i<contestants.Length; i++)
        {
            if(i < contestantsData.Count)
            {
                contestants[i].SetActive(true);
                Vector3 colorVector = BattleRoyaleManager.colorInfo[i];
                contestants[i].GetComponentsInChildren<Image>()[1].color = new(colorVector.x, colorVector.y, colorVector.z);
                contestants[i].GetComponentInChildren<TextMeshProUGUI>().text = contestantsData[i].survivorName;
            }
            else contestants[i].SetActive(false);
        }

        for(int i=0; i<predictRankings.Length; i++)
        {
            if(i < needPredictionNumber)
            {
                predictRankings[i].SetActive(true);
                predictRankingContestants[i].SetActive(false);
            }
            else predictRankings[i].SetActive(false);
        }
        selectedContestant.SetActive(false);

        bettingRoom.SetActive(true);
    }

    public void EasyBet(int amount)
    {
        int curBet = int.Parse(bettingAmountInput.text);
        bettingAmountInput.text = (curBet + amount).ToString();
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
        if (!IsValidPrediction(out string reason)) Alert($"Not valid prediction : {reason}");
        else if (_bettingAmount < 100) Alert("The minimum bet amount is 100$.");
        else
        {
            OpenConfirmWindow("Confirm betting?", () =>
            {
                bettingAmount = _bettingAmount;
                for (int i = 0; i < needPredictionNumber; i++) predictions[i] = predictRankingContestants[i].GetComponentInChildren<TextMeshProUGUI>().text;
                StartBattleRoyale();
                bettingRoom.SetActive(false);
            });
        }
    }

    public void SkipBetting()
    {
        OpenConfirmWindow("Skip betting?", () => 
        {
            bettingAmount = 0;
            StartBattleRoyale();
            bettingRoom.SetActive(false);
        });
    }

    bool IsValidPrediction(out string reason)
    {
        for(int i = 0; i < needPredictionNumber; i++)
        {
            if (!predictRankingContestants[i].activeSelf)
            {
                reason = "Empty predictions exist.";
                return false;
            }
            for(int j = 0; j < i; j++)
            {
                if(predictRankingContestants[j].GetComponentInChildren<TextMeshProUGUI>().text == predictRankingContestants[i].GetComponentInChildren<TextMeshProUGUI>().text)
                {
                    reason = "Duplicate predictions exist.";
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
                        odds = 1.2f;
                        break;
                    case 2:
                        odds = 6;
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
        Debug.Log(odds);
        odds *= Factorial(correctExactRanking);
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
    public void EndTheDay()
    {
        string message = "Are you done for the day?";
        string warning = "";
        if(calendar.Today % 7 < 5)
        {
            bool thereAreUnassignedSurvivors = false;
            warning = "\n<color=red><i>There are unassigned survivors : ";
            foreach (SurvivorData survivor in mySurvivorsData)
            {
                if (survivor.assignedTraining == Training.None && TrainableAnything(survivor))
                {
                    thereAreUnassignedSurvivors = true;
                    warning += $"{survivor.survivorName}, ";
                }
            }
            warning += "</i></color>";
            if (thereAreUnassignedSurvivors) message += warning;
        }
        else
        {
            if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
            {
                if(calendar.LeagueReserveInfo[calendar.Today].reserver != null)
                {
                    Alert($"There are survivors who have been reserved for Battle Royale today : <i>{calendar.LeagueReserveInfo[calendar.Today].reserver.survivorName}</i>");
                    return;
                }
                else
                {
                    message = "There's a Battle Royale match today. just skip it and end the day?";
                }
            }
        }

        if(calendar.Today % 7 < 5)
        {
            OpenConfirmWindow(message, () =>
            {
                int index = 0;
                foreach (GameObject survivorTrainingResult in survivorTrainingResults) survivorTrainingResult.SetActive(false);
                foreach (SurvivorData survivor in mySurvivorsData)
                {
                    ApplyTraining(survivor, survivor.assignedTraining);
                    if(survivor.assignedTraining != Training.None)
                    {
                        survivorTrainingResults[index].SetActive(true);
                        resultTexts[index][0].text = survivor.survivorName;
                        resultTexts[index][1].text = $"Strength + {survivor.increaseComparedToPrevious_strength}";
                        resultTexts[index][1].gameObject.SetActive(survivor.increaseComparedToPrevious_strength > 0);
                        resultTexts[index][2].text = $"Agility + {survivor.increaseComparedToPrevious_agility}";
                        resultTexts[index][2].gameObject.SetActive(survivor.increaseComparedToPrevious_agility > 0);
                        resultTexts[index][3].text = $"Fighting + {survivor.increaseComparedToPrevious_fighting}";
                        resultTexts[index][3].gameObject.SetActive(survivor.increaseComparedToPrevious_fighting > 0);
                        resultTexts[index][4].text = $"Shooting + {survivor.increaseComparedToPrevious_shooting}";
                        resultTexts[index][4].gameObject.SetActive(survivor.increaseComparedToPrevious_shooting > 0);
                        resultTexts[index][5].text = $"Knowledge + {survivor.increaseComparedToPrevious_knowledge}";
                        resultTexts[index][5].gameObject.SetActive(survivor.increaseComparedToPrevious_knowledge > 0);
                        index++;
                    }
                    if(!autoAssign)
                    {
                        survivor.assignedTraining = Training.None;
                        ConfirmAssignTraining();
                    }
                    Surgery(survivor);
                }
                selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value], true);
                calendar.Today++;
                calendar.TurnPageCalendar(0);

                dailyResult.SetActive(true);
            });
        }
        else if(calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
        {
            OpenConfirmWindow(message, () =>
            {
                EndTheDayWeekend();
            });
        }
        else EndTheDayWeekend();
    }

    public void EndTheDayWeekend()
    {
        calendar.Today++;
        calendar.TurnPageCalendar(0);
        Alert("Ended weekend");
    }

    void ApplyTraining(SurvivorData survivor, Training training)
    {
        int survivorStrengthLv = survivor._strength / 20;
        int survivorAgilityLv = survivor._agility / 20;
        int survivorFightingLv = survivor._fighting / 20;
        int survivorShtLv = survivor._shooting / 20;
        int survivorKnowledgeLv = survivor._knowledge / 20;

        survivor.increaseComparedToPrevious_strength = 0;
        survivor.increaseComparedToPrevious_agility = 0;
        survivor.increaseComparedToPrevious_fighting = 0;
        survivor.increaseComparedToPrevious_shooting = 0;
        survivor.increaseComparedToPrevious_knowledge = 0;

        switch (training)
        {
            case Training.Weight:
                int increaseStrength = Mathf.Max(fightTrainingLevel - survivorStrengthLv, 0);
                survivor.IncreaseStats(increaseStrength, 0, 0, 0, 0);
                break;
            case Training.Running:
                int increseAgility = Mathf.Max(runningLevel - survivorAgilityLv, 0);
                survivor.IncreaseStats(0, increseAgility, 0, 0, 0);
                break;
            case Training.Fighting:
                int increseFighting = Mathf.Max(weightTrainingLevel - survivorFightingLv, 0);
                survivor.IncreaseStats(0, 0, increseFighting, 0, 0);
                break;
            case Training.Shooting:
                int increseShooting = Mathf.Max(shootingTrainingLevel - survivorShtLv, 0);
                survivor.IncreaseStats(0, 0, 0, increseShooting, 0);
                break;
            case Training.Studying:
                int increseKnowledge = Mathf.Max( - survivorShtLv, 0);
                survivor.IncreaseStats(0, 0, 0, 0, increseKnowledge);
                break;
            default:
                break;
        }
    }

    void Surgery(SurvivorData survivor)
    {
        if (!survivor.surgeryScheduled) return;
        if(survivor.surgeryType == SurgeryType.Transplant)
        {
            Injury surgeryInjury = survivor.injuries.Find(x => x.site == survivor.surgerySite);
            surgeryInjury.type = InjuryType.ArtificalPartsTransplanted;
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

        survivor.shceduledSurgeryCost = 0;
        survivor.surgeryScheduled = false;
    }

    public void SurvivorsRecovery()
    {
        foreach(SurvivorData survivor in mySurvivorsData)
        {
            List<Injury> fullyRecovered = new();
            foreach(Injury injury in survivor.injuries)
            {
                if(injury.degree < 1 && injury.type != InjuryType.ArtificalPartsTransplanted)
                {
                    float recoveryRate = 1;
                    if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Sturdy) > -1) recoveryRate *= 1.5f;
                    else if (survivor.characteristics.FindIndex(x => x.type == CharacteristicType.Fragile) > -1) recoveryRate *= 0.7f;
                    
                    if (injury.type == InjuryType.Contusion) injury.degree -= 0.1f * recoveryRate;
                    else injury.degree -= 0.05f * recoveryRate;
                    if(injury.degree < 0.1) fullyRecovered.Add(injury);
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

    public SurvivorData CreateRandomSurvivorData()
    {
        int value = calendar.LeagueReserveInfo[calendar.Today].league switch
        {
            League.BronzeLeague => 1,
            League.SilverLeague => 1,
            League.GoldLeague => 2,
            League.SeasonChampionship => 3,
            League.WorldChampionship => 4,
            _ => 5
        };
        int check = 0;
        while (check < 1000)
        {
            int randStrength = UnityEngine.Random.Range(0, 100);
            int randAgility = UnityEngine.Random.Range(0, 100);
            int randFighting = UnityEngine.Random.Range(0, 100);
            int randShooting = UnityEngine.Random.Range(0, 100);
            int randKnowledge = UnityEngine.Random.Range(0, 100);
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
            CharacteristicManager.AddRandomCharacteristics(survivorData, UnityEngine.Random.Range(0, 4));
            return survivorData;
        }
        return new(GetRandomName(), 20, 20, 20, 20, 20, 100, calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league));
    }

    public void OpenConfirmWindow(string wantText, UnityAction wantAction)
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmText.text = wantText;
        confirmButton.onClick.AddListener(wantAction);
        confirmButton.onClick.AddListener(()=>confirmCanvas.SetActive(false));
        confirmCanvas.SetActive(true);
    }

    public void Alert(string message)
    {
        PoolManager.Spawn(ResourceEnum.Prefab.Alert, alertCanvas.transform).GetComponentInChildren<TextMeshProUGUI>().text = message;
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
                    int jndex = contestantsData.FindIndex(x => x.survivorName == results[index].gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
                    if (jndex > -1) selectedContestantData = contestantsData[jndex];
                }
                if(selectedContestantData != null)
                {
                    selectedContestant.GetComponentInChildren<SurvivorInfo>().SetInfo(selectedContestantData, false);
                    selectedContestant.SetActive(true);
                    draggingContestant.SetActive(true);
                    draggingContestant.GetComponentsInChildren<Image>()[1].color = results[index].gameObject.GetComponentsInChildren<Image>()[1].color;
                    draggingContestant.GetComponentInChildren<TextMeshProUGUI>().text = selectedContestantData.survivorName;
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
                            if (predictRankingContestants[j].GetComponentInChildren<TextMeshProUGUI>().text == selectedContestantData.survivorName)
                            {
                                alreadyPredicted = j;
                                if (predictRankingContestants[i].activeSelf)
                                {
                                    predictRankingContestants[j].SetActive(true);
                                    predictRankingContestants[j].GetComponentsInChildren<Image>()[1].color = predictRankingContestants[i].GetComponentsInChildren<Image>()[1].color;
                                    predictRankingContestants[j].GetComponentInChildren<TextMeshProUGUI>().text = predictRankingContestants[i].GetComponentInChildren<TextMeshProUGUI>().text;
                                }
                                else predictRankingContestants[j].SetActive(false);
                            }
                        }

                        predictRankingContestants[i].SetActive(true);
                        predictRankingContestants[i].GetComponentsInChildren<Image>()[1].color = draggingContestant.GetComponentsInChildren<Image>()[1].color;
                        predictRankingContestants[i].GetComponentInChildren<TextMeshProUGUI>().text = selectedContestantData.survivorName;
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

    public void LoadMySurvivorData(List<SurvivorData> data)
    {
        mySurvivorsData.Clear();
        mySurvivorsData = data;
    }

    public void LoadData(int money, int mySurvivorsId, int survivorHireLimit, int fightTrainingLevel, int shootingTrainingLevel,
        int agilityTrainingLevel, int weightTrainingLevel)
    {
        this.money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.survivorHireLimit = survivorHireLimit;
        this.fightTrainingLevel = fightTrainingLevel;
        this.shootingTrainingLevel = shootingTrainingLevel;
        this.runningLevel = agilityTrainingLevel;
        this.weightTrainingLevel = weightTrainingLevel;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;

public enum Training { None, Fighting, Shooting, Agility, Weight }

public enum SurgeryType { Transplant, ChronicDisorderTreatment, Alteration }

public class OutGameUIManager : MonoBehaviour
{
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
    [SerializeField] GameObject hireSurvivor;
    [SerializeField] GameObject hireClose;
    [SerializeField] SurvivorInfo[] survivorsInHireMarket;
    [SerializeField] TMP_Dropdown survivorsDropdown;
    public TMP_Dropdown SurvivorsDropdown => survivorsDropdown;
    [SerializeField] SurvivorInfo selectedSurvivor;

    [SerializeField] List<SurvivorData> mySurvivorsData;
    public List<SurvivorData> MySurvivorsData => mySurvivorsData;

    [Header("Training Room")]
    [SerializeField] GameObject trainingRoom;
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI agilityTrainingNameText;
    [SerializeField] TextMeshProUGUI weightTrainingNameText;
    int fightTrainingLevel = 1;
    int shootingTrainingLevel = 1;
    int agilityTrainingLevel = 1;
    int weightTrainingLevel = 1;
    int[] facilityUpgradeCost = { 1000, 3000, 10000, 30000 };
    [SerializeField] GameObject fightTrainingRoomUpgradeButtion;
    [SerializeField] GameObject shootingTrainingRoomUpgradeButtion;
    [SerializeField] GameObject agilityTrainingRoomUpgradeButtion;
    [SerializeField] GameObject weightTrainingRoomUpgradeButtion;
    [SerializeField] TextMeshProUGUI fightTrainingBookers;
    [SerializeField] TextMeshProUGUI shootingTrainingBookers;
    [SerializeField] TextMeshProUGUI agilityTrainingBookers;
    [SerializeField] TextMeshProUGUI weightTrainingBookers;

    [SerializeField] TextMeshProUGUI assignTrainingNameText;
    [SerializeField] Transform survivorsAssignedThis;
    [SerializeField] Transform survivorsWithoutSchedule; 
    [SerializeField] Transform survivorsWithOtherSchedule;
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

    [SerializeField] TMP_Dropdown weaponPriority1Dropdown;
    [SerializeField] Strategy[] strategies;
    
    [SerializeField] TMP_Dropdown sawAnEnemyAndItIsOutsideOfAttackRangeDropdown;
    [SerializeField] TMP_Dropdown elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown;

    [SerializeField] TMP_Dropdown sawAnEnemyAndItIsInAttackRangeDropdown;
    [SerializeField] TMP_Dropdown elseActionSawAnEnemyAndItIsInAttackRangeDropdown;

    [SerializeField] TMP_Dropdown heardDistinguishableSoundDropdown;
    [SerializeField] TMP_Dropdown elseActionHeardDistinguishableSoundDropdown;

    [SerializeField] TMP_Dropdown heardIndistinguishableSoundDropdown;
    [SerializeField] TMP_Dropdown elseActionHeardIndistinguishableSoundDropdown;

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
    Calendar calendar;
    SurvivorData mySurvivorDataInBattleRoyale;
    public SurvivorData MySurvivorDataInBattleRoyale => mySurvivorDataInBattleRoyale;

    private void Start()
    {
        calendar = GetComponent<Calendar>();
        mySurvivorsData = new();
        SetHireMarketFirst();
        Money = 1000;
        GameManager.Instance.ObjectStart += () => InitializeStrategyRoom();
    }

    #region Hire
    public void SetHireMarketFirst()
    {
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 110, 11, 1, 3, 1, 1f, 0, 100, Tier.Bronze);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 100, 10, 1.1f, 3.3f, 1.1f, 1f, 0, 100, Tier.Bronze);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 100, 10, 1, 3, 1, 1.1f, 0, 100, Tier.Bronze);
    }

    public void ResetHireMarket()
    {
        float value = 1;
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            float rand0 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand1 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand2 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand3 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand4 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand5 = UnityEngine.Random.Range(0.5f, 2.0f);
            float totalRand = rand0 * rand1 * rand2 * rand3 * rand4 * rand5;
            if ((totalRand < 0.7f || totalRand > 1.3) && check < 100)
            {
                i--;
                check++;
                continue;
            }
            if (check >= 100) Debug.LogWarning("Infinite loop detected");
            check = 0;
            survivorsInHireMarket[i].SetInfo(GetRandomName(),
                value * 100 * rand0,
                value * 10 * rand1,
                value * 1 * rand2,
                value * 3 * rand3,
                value * 1 * rand4,
                value * 1 * rand5,
                UnityEngine.Random.Range(0, 4),
                (int)(value * 100 * totalRand),
                Tier.Bronze);
            survivorsInHireMarket[i].SoldOut = false;
        }
    }

    public void HireSurvivor(int candidate)
    {
        OpenConfirmWindow($"Are you sure to hire \"{survivorsInHireMarket[candidate].survivorData.survivorName}\" for $ {survivorsInHireMarket[candidate].survivorData.price} ?",
            () => {
                if(money < survivorsInHireMarket[candidate].survivorData.price)
                {
                    Alert("Not enough money.");
                }
                else
                {
                    Money -= survivorsInHireMarket[candidate].survivorData.price;
                    mySurvivorsData.Add(new(survivorsInHireMarket[candidate].survivorData));
                    mySurvivorsData[mySurvivorsData.Count - 1].characteristics = survivorsInHireMarket[candidate].survivorData.characteristics;
                    mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;

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
        return candidate;
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
        assignTrainingNameText.text = $"{(Training)trainingIndex} Training";
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
            SurvivorSchedule survivorSchedule;
            string description = "";
            bool assignable = true;
            if (survivor.assignedTraining == (Training)trainingIndex) fitParent = survivorsAssignedThis;
            else if (survivor.assignedTraining == Training.None)
            {
                fitParent = survivorsWithoutSchedule;
                if(!Trainable(survivor, (Training)trainingIndex, out string cause))
                {
                    assignable = false;
                    description = $"{survivor.survivorName} can't cannot be assigned to this training due to injury.\n<color=red><i>Cause : {cause}</i></color>";
                }
            }
            else
            {
                fitParent = survivorsWithOtherSchedule;
                description = $"{survivor.survivorName} is assigned to {survivor.assignedTraining} training.";
            }
            survivorSchedule = PoolManager.Spawn(ResourceEnum.Prefab.SurvivorSchedule, fitParent).GetComponent<SurvivorSchedule>();
            survivorSchedule.SetSurvivorData(survivor, trainingIndex, assignable);
            survivorSchedule.GetComponent<Help>().SetDescription(description);
            survivorSchedule.GetComponent<Button>().enabled = assignable;
        }
    }

    public void ConfirmAssignTraining()
    {
        fightTrainingBookers.text = "";
        shootingTrainingBookers.text = "";
        agilityTrainingBookers.text = "";
        weightTrainingBookers.text = "";
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
                case Training.Agility:
                    targetText = agilityTrainingBookers;
                    break;
                case Training.Weight:
                    targetText = weightTrainingBookers;
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
        Invoke(nameof(RefreshUI), 0.1f);
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
        if (Trainable(survivor, Training.Agility)) return true;
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
                case Training.Agility:
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

    void RefreshUI()
    {
        fightTrainingBookers.text += " ";
        shootingTrainingBookers.text += " ";
        agilityTrainingBookers.text += " ";
        weightTrainingBookers.text += " ";
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
                OpenConfirmWindow("Are you sure to upgrade Fight Training Room?", ()=>
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
                        if (fightTrainingLevel > facilityUpgradeCost.Length) fightTrainingRoomUpgradeButtion.SetActive(false);
                        else fightTrainingRoomUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Facility Upgrades\n$ {facilityUpgradeCost[fightTrainingLevel - 1]}";
                    }
                });
                break;
            case 1:
                OpenConfirmWindow("Are you sure to upgrade Shooting Training Room?", () =>
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
                        if (shootingTrainingLevel > facilityUpgradeCost.Length) shootingTrainingRoomUpgradeButtion.SetActive(false);
                        else shootingTrainingRoomUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Facility Upgrades\n$ {facilityUpgradeCost[shootingTrainingLevel - 1]}";
                    }
                });
                break;
            case 2:
                OpenConfirmWindow("Are you sure to upgrade Agility Training Room?", () =>
                {
                    if (money < facilityUpgradeCost[agilityTrainingLevel - 1])
                    {
                        Alert("Not enough money");
                    }
                    else
                    {
                        Money -= facilityUpgradeCost[agilityTrainingLevel - 1];
                        agilityTrainingLevel++;
                        fightTrainingNameText.text = $"Agility Training (lv {agilityTrainingLevel})";
                        if (agilityTrainingLevel > facilityUpgradeCost.Length) agilityTrainingRoomUpgradeButtion.SetActive(false);
                        else agilityTrainingRoomUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Facility Upgrades\n$ {facilityUpgradeCost[agilityTrainingLevel - 1]}";
                    }
                });
                break;
            case 3:
                OpenConfirmWindow("Are you sure to upgrade Weight Training Room?", () =>
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
                        if (weightTrainingLevel > facilityUpgradeCost.Length) weightTrainingRoomUpgradeButtion.SetActive(false);
                        else weightTrainingRoomUpgradeButtion.GetComponentInChildren<TextMeshProUGUI>().text = $"Facility Upgrades\n$ {facilityUpgradeCost[weightTrainingLevel - 1]}";
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
        sawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Attack", "Ignore", "Run away" }));
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsInAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Attack", "Ignore", "Run away" }));
        
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        sawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Trace", "Ignore", "Run away" }));
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.ClearOptions();
        elseActionSawAnEnemyAndItIsOutsideOfAttackRangeDropdown.AddOptions(new List<string>(new string[] { "Trace", "Ignore", "Run away" }));

        heardDistinguishableSoundDropdown.ClearOptions();
        heardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        elseActionHeardDistinguishableSoundDropdown.ClearOptions();
        elseActionHeardDistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        
        heardIndistinguishableSoundDropdown.ClearOptions();
        heardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
        elseActionHeardIndistinguishableSoundDropdown.ClearOptions();
        elseActionHeardIndistinguishableSoundDropdown.AddOptions(new List<string>(new string[] { "Go where the sound is heard.", "Look in the direction in which the sound is heard.", "Ignore the sound" }));
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
    }

    public void OpenStrategyRoom()
    {
        SetStrategyRoom();
        strategyRoom.SetActive(true);
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
                ConditionData[] conditionData = new ConditionData[5];
                for (int i = 0; i < 5; i++)
                {
                    if(!int.TryParse(strategy.inputFields[i].text, out int num)) num = 0;
                    conditionData[i] = new(strategy.andOrs[i].value, strategy.variable1s[i].value, strategy.operators[i].value, strategy.variable2s[i].value, num);
                }
                survivorWhoWantEstablishStrategy.strategyDictionary[strategy.strategyCase] =
                    new(sawAnEnemyAndItIsInAttackRangeDropdown.value, elseActionSawAnEnemyAndItIsInAttackRangeDropdown.value, strategy.activeConditionCount, conditionData);
            }

        });
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

    public void StartBattleRoyale(SurvivorData participant)
    {
        mySurvivorDataInBattleRoyale = participant;
        StartCoroutine(GameManager.Instance.BattleRoyaleStart());
    }

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
            if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today) && calendar.LeagueReserveInfo[calendar.Today].reserver != null)
            {
                Alert($"There are survivors who have been reserved for Battle Royale today : <i>{calendar.LeagueReserveInfo[calendar.Today].reserver.survivorName}</i>");
                return;
            }
        }

        if(calendar.Today % 7 < 5)
        {
            OpenConfirmWindow(message, () =>
            {
                foreach (SurvivorData survivor in mySurvivorsData)
                {
                    ApplyTraining(survivor, survivor.assignedTraining);
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
        int survivorHpLv = (int)(Mathf.Max(survivor.hp - 100, 0) / 25);
        int survivorAtkDmgLv = (int)(Mathf.Max(survivor.attackDamage - 10, 0) / 10);
        int survivorAtkSpdLv = (int)(Mathf.Max(survivor.attackSpeed - 1, 0) / 0.3f);
        int survivorShtLv = (int)(Mathf.Max(survivor.shooting - 1, 0));
        int survivorMvSpdLv = (int)(Mathf.Max(survivor.moveSpeed - 3, 0) / 0.8f);
        int survivorFrmSpdLv = (int)(Mathf.Max(survivor.farmingSpeed - 1, 0) / 0.3f);

        survivor.increaseComparedToPrevious_hp = 0;
        survivor.increaseComparedToPrevious_attackDamage = 0;
        survivor.increaseComparedToPrevious_attackSpeed = 0;
        survivor.increaseComparedToPrevious_moveSpeed = 0;
        survivor.increaseComparedToPrevious_farmingSpeed = 0;
        survivor.increaseComparedToPrevious_shooting = 0;

        switch (training)
        {
            case Training.Fighting:
                float increaseAtkDmg = Mathf.Max(UnityEngine.Random.Range(0.1f, 0.3f) * (fightTrainingLevel - survivorAtkDmgLv), 0.001f);
                float increaseAtkSpeed = Mathf.Max(UnityEngine.Random.Range(0.03f, 0.09f) * (fightTrainingLevel - survivorAtkSpdLv), 0.0001f);
                survivor.IncreaseStats(0, increaseAtkDmg, increaseAtkSpeed, 0, 0, 0);
                break;
            case Training.Shooting:
                float increseShooting = Mathf.Max(UnityEngine.Random.Range(0.01f, 0.03f) * (shootingTrainingLevel - survivorShtLv), 0.001f);
                survivor.IncreaseStats(0, 0, 0, 0, 0, increseShooting);
                break;
            case Training.Agility:
                float increseMoveSpeed = Mathf.Max(UnityEngine.Random.Range(0.008f, 0.024f) * (agilityTrainingLevel - survivorMvSpdLv), 0.0001f);
                float increseFarmingSpeed = Mathf.Max(UnityEngine.Random.Range(0.03f, 0.09f) * (agilityTrainingLevel - survivorFrmSpdLv), 0.0001f);
                survivor.IncreaseStats(0, 0, 0, increseMoveSpeed, increseFarmingSpeed, 0);
                break;
            case Training.Weight:
                float increseHp = Mathf.Max(UnityEngine.Random.Range(0.25f, 0.75f) * (weightTrainingLevel - survivorHpLv), 0.001f);
                survivor.IncreaseStats(increseHp, 0, 0, 0, 0, 0);
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
        float value = 1;
        int check = 0;
        int i = 0;
        while (i < 1)
        {
            float randHp = UnityEngine.Random.Range(0.5f, 2.0f);
            float randAttackDamage = UnityEngine.Random.Range(0.5f, 2.0f);
            float randAttackSpeed = UnityEngine.Random.Range(0.7f, 1.3f);
            float randMoveSpeed = UnityEngine.Random.Range(0.7f, 1.3f);
            float randFarmingSpeed = UnityEngine.Random.Range(0.7f, 1.3f);
            float randShooting = UnityEngine.Random.Range(0.5f, 2.0f);
            float totalRand = randHp * randAttackDamage * randAttackSpeed * randMoveSpeed * randFarmingSpeed * randShooting;
            if ((totalRand < 0.7f || totalRand > 1.3f) && check < 100)
            {
                check++;
                continue;
            }
            if (check >= 100) Debug.LogWarning("Infinite roof has detected");
            SurvivorData survivorData = new(
                GetRandomName(),
                value * 100 * randHp,
                value * 10 * randAttackDamage,
                value * 1 * randAttackSpeed,
                value * 3 * randMoveSpeed,
                value * 1 * randFarmingSpeed,
                value * 1 * randShooting,
                (int)(value * 100 * totalRand),
                calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league)
                );
            CharacteristicManager.AddRandomCharacteristics(survivorData, UnityEngine.Random.Range(0, 4));
            return survivorData;
        }
        return new(GetRandomName(), 100, 10, 1, 3, 1, 1f, 100, calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league));
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
}

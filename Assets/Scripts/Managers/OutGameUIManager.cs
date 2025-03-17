using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;

#region SurvivorData
[Serializable]
public class SurvivorData
{
    public string survivorName;
    public float hp;
    public float attackDamage;
    public float attackSpeed;
    public float moveSpeed;
    public float farmingSpeed;
    public float shooting;
    public int price;
    public Tier tier;

    public bool isReserved;
    public Training assignedTraining;

    public float increaseComparedToPrevious_hp;
    public float increaseComparedToPrevious_attackDamage;
    public float increaseComparedToPrevious_attackSpeed;
    public float increaseComparedToPrevious_moveSpeed;
    public float increaseComparedToPrevious_farmingSpeed;
    public float increaseComparedToPrevious_shooting;

    public List<Injury> injuries = new();
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public int shceduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;


    public SurvivorData(string survivorName, float hp, float attackDamage, float attackSpeed, float moveSpeed,
        float farmingSpeed, float shooting, int price, Tier tier)
    {
        this.survivorName = survivorName;
        this.hp = hp;
        this.attackDamage = attackDamage;
        this.attackSpeed = attackSpeed;
        this.moveSpeed = moveSpeed;
        this.farmingSpeed = farmingSpeed;
        this.shooting = shooting;
        this.price = price;
        this.tier = tier;
    }

    public SurvivorData(SurvivorData survivorData)
    {
        this.survivorName = survivorData.survivorName;
        this.hp = survivorData.hp;
        this.attackDamage = survivorData.attackDamage;
        this.attackSpeed = survivorData.attackSpeed;
        this.moveSpeed = survivorData.moveSpeed;
        this.farmingSpeed = survivorData.farmingSpeed;
        this.shooting = survivorData.shooting;
        this.price = survivorData.price;
        this.tier = survivorData.tier;
    }

    public void IncreaseStats(float hp, float attackDamage, float attackSpeed, float moveSpeed, float farmingSpeed, float shooting)
    {
        this.hp += hp;
        this.attackDamage += attackDamage;
        this.attackSpeed += attackSpeed;
        this.moveSpeed += moveSpeed;
        this.farmingSpeed += farmingSpeed;
        this.shooting += shooting;

        increaseComparedToPrevious_hp += hp;
        increaseComparedToPrevious_attackDamage += attackDamage;
        increaseComparedToPrevious_attackSpeed += attackSpeed;
        increaseComparedToPrevious_moveSpeed += moveSpeed;
        increaseComparedToPrevious_farmingSpeed += farmingSpeed;
        increaseComparedToPrevious_shooting += shooting;
    }
}
public enum Tier { Bronze, Silver, Gold }
#endregion

public enum Training { None, Fighting, Shooting, Agility, Weight }

public enum SurgeryType { Transplant, Alteration }

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
    [SerializeField] Toggle surgeryType_Alteration;

    [SerializeField] GameObject selectSurgery;
    [SerializeField] GameObject scheduledSurgery;
    [SerializeField] GameObject buttonScheduleSurgery;
    [SerializeField] GameObject buttonCancelSurgery;

    [SerializeField] GameObject[] surgeries;
    [SerializeField] ToggleGroup surgeriesToggleGroup;
    struct SurgeryInfo
    {
        public string surgeryName;
        public int surgeryCost;
        public InjurySite surgerySite;
        public SurgeryType surgeryType;

        public SurgeryInfo(string surgeryName, int surgeryCost, InjurySite surgerySite, SurgeryType surgeryType)
        {
            this.surgeryName = surgeryName;
            this.surgeryCost = surgeryCost;
            this.surgerySite = surgerySite;
            this.surgeryType = surgeryType;
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
    }

    #region Hire
    public void SetHireMarketFirst()
    {
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 120, 10, 1, 3, 1, 1f, 100, Tier.Bronze);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 100, 12, 1.1f, 3, 1, 1f, 100, Tier.Bronze);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 100, 10, 1, 3.5f, 1.1f, 1f, 100, Tier.Bronze);
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
            if ((totalRand < 0.7f || totalRand > 2) && check < 100)
            {
                i--;
                check++;
                continue;
            }
            if (check >= 100) Debug.LogWarning("Infinite roof has detected");
            check = 0;
            survivorsInHireMarket[i].SetInfo(GetRandomName(),
                value * 100 * rand0,
                value * 10 * rand1,
                value * 1 * rand2,
                value * 3 * rand3,
                value * 1 * rand4,
                value * 1 * rand5,
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
                    mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].survivorData;

                    if(mySurvivorsData.Count == 1)
                    {
                        survivorsDropdown.ClearOptions();
                        selectedSurvivor.SetInfo(mySurvivorsData[0], true);
                        hireClose.SetActive(true);
                    }
                    survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].survivorData.survivorName });
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
        Invoke("RefreshUI", 0.1f);
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
                    }
                    surgeryList.Add(new(surgeryName, cost, injury.site, SurgeryType.Transplant));
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
                    if (injury.type == InjuryType.Contusion) injury.degree -= 0.1f;
                    else injury.degree -= 0.05f;
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

    public SurvivorData CreateRandomSurvivorData()
    {
        float value = 1;
        int check = 0;
        int i = 0;
        while (i < 1)
        {
            float rand0 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand1 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand2 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand3 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand4 = UnityEngine.Random.Range(0.5f, 2.0f);
            float rand5 = UnityEngine.Random.Range(0.5f, 2.0f);
            float totalRand = rand0 * rand1 * rand2 * rand3 * rand4 * rand5;
            if ((totalRand < 0.7f || totalRand > 1.3f) && check < 100)
            {
                check++;
                continue;
            }
            if (check >= 100) Debug.LogWarning("Infinite roof has detected");
            SurvivorData survivorData = new(
                GetRandomName(),
                value * 100 * rand0,
                value * 10 * rand1,
                value * 1 * rand2,
                value * 3 * rand3,
                value * 1 * rand4,
                value * 1 * rand5,
                (int)(value * 100 * totalRand),
                calendar.GetNeedTier(calendar.LeagueReserveInfo[calendar.Today].league)
                );
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

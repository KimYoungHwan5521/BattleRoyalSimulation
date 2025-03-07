using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

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

public enum Training { None, Fighting, Shooting, Agility }

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
            moneyText.text = $"{money:###,###,###,##0}";
        }
    }

    [Header("Survivors / Hire")]
    [SerializeField] GameObject hireSurvivor;
    [SerializeField] GameObject hireClose;
    [SerializeField] SurvivorInfo[] survivorsInHireMarket;
    [SerializeField] TMP_Dropdown survivorsDropdown;
    public TMP_Dropdown SurvivorsDropdown => survivorsDropdown;
    [SerializeField] SurvivorInfo selectedSurvivor;

    [SerializeField] List<SurvivorData> mySurvivorsData;
    public List<SurvivorData> MySurvivorsData => mySurvivorsData;

    [Header("Training")]
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI agilityTrainingNameText;
    int fightTrainingLevel = 1;
    int shootingTrainingLevel = 1;
    int agilityTrainingLevel = 1;
    [SerializeField] TextMeshProUGUI fightTrainingBookers;
    [SerializeField] TextMeshProUGUI shootingTrainingBookers;
    [SerializeField] TextMeshProUGUI agilityTrainingBookers;
    [SerializeField] TextMeshProUGUI assignTrainingNameText;
    [SerializeField] Transform survivorsAssignedThis;
    [SerializeField] Transform survivorsWithoutSchedule; 
    [SerializeField] Transform survivorsWithOtherSchedule;
    bool autoAssign = true;
    [SerializeField] GameObject autoAssignCheckBox;

    [Header("Schedule")]
    Calendar calendar;
    static SurvivorData mySurvivorDataInBattleRoyale;
    public static SurvivorData MySurvivorDataInBattleRoyale => mySurvivorDataInBattleRoyale;

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
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 200, 10, 1, 3, 1, 1f, 100, Tier.Bronze);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 100, 20, 1, 3, 1, 1f, 100, Tier.Bronze);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 100, 10, 1, 4.5f, 1.5f, 1f, 100, Tier.Bronze);
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
        OpenConfirmCanvas($"Are you sure to hire \"{survivorsInHireMarket[candidate].survivorData.survivorName}\" for $ {survivorsInHireMarket[candidate].survivorData.price} ?",
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
                        selectedSurvivor.SetInfo(mySurvivorsData[0]);
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
        selectedSurvivor.SetInfo(survivorInfo);
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
            if (survivor.assignedTraining == (Training)trainingIndex) fitParent = survivorsAssignedThis;
            else if (survivor.assignedTraining == Training.None) fitParent = survivorsWithoutSchedule;
            else fitParent = survivorsWithOtherSchedule;
            survivorSchedule = PoolManager.Spawn(ResourceEnum.Prefab.SurvivorSchedule, fitParent).GetComponent<SurvivorSchedule>();
            survivorSchedule.SetSurvivorData(survivor, trainingIndex);
        }
    }

    public void ConfirmAssignTraining()
    {
        fightTrainingBookers.text = "";
        shootingTrainingBookers.text = "";
        agilityTrainingBookers.text = "";
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

    void RefreshUI()
    {
        fightTrainingBookers.text += " ";
        shootingTrainingBookers.text += " ";
        agilityTrainingBookers.text += " ";
    }

    public void ToggleAutoAssign()
    {
        autoAssign = !autoAssign;
        autoAssignCheckBox.SetActive(autoAssign);
    }

    public void StartBattleRoyale(SurvivorData participant)
    {
        mySurvivorDataInBattleRoyale = new(participant);
        StartCoroutine(GameManager.Instance.BattleRoyaleStart());
    }

    public void EndTheDay()
    {
        string message = "Are you done for the day?";
        bool thereAreUnassignedSurvivors = false;
        string warning = "\n<color=red><i>There are unassigned survivors : ";
        foreach (SurvivorData survivor in mySurvivorsData)
        {
            if (survivor.assignedTraining == Training.None)
            {
                thereAreUnassignedSurvivors = true;
                warning += $"{survivor.survivorName}, ";
            }
        }
        warning += "</i></color>";
        if (thereAreUnassignedSurvivors) message += warning;
        OpenConfirmCanvas(message, () =>
        {
            calendar.Today++;
            calendar.TurnPageCalendar(0);
            foreach (SurvivorData survivor in mySurvivorsData)
            {
                ApplyTraining(survivor, survivor.assignedTraining);
                if(!autoAssign)
                {
                    survivor.assignedTraining = Training.None;
                    ConfirmAssignTraining();
                }
            }
            selectedSurvivor.SetInfo(mySurvivorsData[survivorsDropdown.value]);
        });
    }

    void ApplyTraining(SurvivorData survivor, Training training)
    {
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
                float increaseAtkSpeed = Mathf.Max(UnityEngine.Random.Range(0.003f, 0.009f) * (fightTrainingLevel - survivorAtkSpdLv), 0.0001f);
                survivor.IncreaseStats(0, increaseAtkDmg, increaseAtkSpeed, 0, 0, 0);
                break;
            case Training.Shooting:
                float increseShooting = Mathf.Max(UnityEngine.Random.Range(0.01f, 0.03f) * (shootingTrainingLevel - survivorShtLv), 0.001f);
                survivor.IncreaseStats(0, 0, 0, 0, 0, increseShooting);
                break;
            case Training.Agility:
                float increseMoveSpeed = Mathf.Max(UnityEngine.Random.Range(0.008f, 0.024f) * (agilityTrainingLevel - survivorMvSpdLv), 0.0001f);
                float increseFarmingSpeed = Mathf.Max(UnityEngine.Random.Range(0.01f, 0.02f) * (agilityTrainingLevel - survivorFrmSpdLv), 0.0001f);
                survivor.IncreaseStats(0, 0, 0, increseMoveSpeed, increseFarmingSpeed, 0);
                break;
            default:
                break;
        }
    }

    public void OpenConfirmCanvas(string wantText, UnityAction wantAction)
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

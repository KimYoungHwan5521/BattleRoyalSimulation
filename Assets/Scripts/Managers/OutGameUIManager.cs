using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public struct SurvivorData
{
    public string survivorName;
    public float hp;
    public float attackDamage;
    public float attackSpeed;
    public float moveSpeed;
    public float farmingSpeed;
    public float shooting;
    public int price;
}

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
    [SerializeField] SurvivorInfo selectedSurvivor;

    [SerializeField] TMP_Dropdown survivorWhoParticipateInBattleRoyaleDropdown;

    List<SurvivorData> mySurvivorsData;
    static SurvivorData mySurvivorDataInBattleRoyale;
    public static SurvivorData MySurvivorDataInBattleRoyale => mySurvivorDataInBattleRoyale;

    private void Start()
    {
        mySurvivorsData = new();
        SetHireMarketFirst();
        Money = 1000;
    }

    public void SetHireMarketFirst()
    {
        survivorsInHireMarket[0].SetInfo(GetRandomName(), 200, 10, 1, 3, 1, 7.5f, 100);
        survivorsInHireMarket[1].SetInfo(GetRandomName(), 100, 20, 1, 3, 1, 7.5f, 100);
        survivorsInHireMarket[2].SetInfo(GetRandomName(), 100, 10, 1, 4.5f, 1.5f, 7.5f, 100);
    }

    public void ResetHireMarket()
    {
        float value = 1;
        int check = 0;
        for (int i = 0; i < 3; i++)
        {
            float rand0 = Random.Range(0.5f, 2.0f);
            float rand1 = Random.Range(0.5f, 2.0f);
            float rand2 = Random.Range(0.5f, 2.0f);
            float rand3 = Random.Range(0.5f, 2.0f);
            float rand4 = Random.Range(0.5f, 2.0f);
            float rand5 = Random.Range(0.5f, 2.0f);
            float totalRand = rand0 * rand1 * rand2 * rand3 * rand4 * rand5;
            if ((totalRand < 0.4f || totalRand > 3) && check < 100)
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
                (int)(value * 100 * totalRand));
            survivorsInHireMarket[i].SoldOut = false;
        }
    }

    public void HireSurvivor(int candidate)
    {
        OpenConfirmCanvas($"Are you sure to hire \"{survivorsInHireMarket[candidate].suruvivorData.survivorName}\" for $ {survivorsInHireMarket[candidate].suruvivorData.price} ?",
            () => {
                if(money < survivorsInHireMarket[candidate].suruvivorData.price)
                {
                    Alert("Not enough money.");
                }
                else
                {
                    Money -= survivorsInHireMarket[candidate].suruvivorData.price;
                    mySurvivorsData.Add(survivorsInHireMarket[candidate].suruvivorData);
                    mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].suruvivorData;

                    if(mySurvivorsData.Count == 1)
                    {
                        survivorsDropdown.ClearOptions();
                        selectedSurvivor.SetInfo(mySurvivorsData[0]);
                        hireClose.SetActive(true);
                    }
                    survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].suruvivorData.survivorName });
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

        string candidate = Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)];
        for (int i = 0; i < 3; i++)
            if (survivorsInHireMarket[0].suruvivorData.survivorName == candidate) return GetRandomName(depth++);
        for (int i = 0; i < mySurvivorsData.Count; i++)
            if (mySurvivorsData[i].survivorName == candidate) return GetRandomName(depth++);
        return candidate;
    }

    public void OnSurvivorSelected()
    {
        SurvivorData survivorInfo = mySurvivorsData.Find(x => x.survivorName == survivorsDropdown.options[survivorsDropdown.value].text);
        selectedSurvivor.SetInfo(survivorInfo);
    }

    public void SetBattleRoyaleStartBox()
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(survivorsDropdown.options);
        mySurvivorDataInBattleRoyale = mySurvivorsData.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorsDropdown.value].text);
    }

    public void OnSurvivorParticipatingInBattleRoyaleSelected()
    {
        mySurvivorDataInBattleRoyale = mySurvivorsData.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorsDropdown.value].text);
    }

    public void StartBattleRoyale()
    {

        StartCoroutine(GameManager.Instance.BattleRoyaleStart());
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

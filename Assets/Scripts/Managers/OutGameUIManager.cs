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
    public float aimErrorRange;
    public int price;
}

public class OutGameUIManager : MonoBehaviour
{
    [Header("Confirm")]
    [SerializeField] GameObject confirmCanvas;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmButton;

    [Header("Survivors / Hire")]
    [SerializeField] GameObject hireSurvivor;
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
        SetCandidates();
    }

    public void SetCandidates()
    {
        survivorsInHireMarket[0].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 200, 10, 1, 3, 1, 7.5f);
        survivorsInHireMarket[1].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 100, 20, 1, 3, 1, 7.5f);
        survivorsInHireMarket[2].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 100, 10, 1, 4.5f, 1.5f, 7.5f);
    }

    public void HireSurvivor(int candidate)
    {
        OpenConfirmCanvas($"Are you sure to hire \"{survivorsInHireMarket[candidate].suruvivorData.survivorName}\" for $ {survivorsInHireMarket[candidate].suruvivorData.price} ?",
            () => {
                mySurvivorsData.Add(survivorsInHireMarket[candidate].suruvivorData);
                mySurvivorDataInBattleRoyale = survivorsInHireMarket[candidate].suruvivorData;

                if(mySurvivorsData.Count == 1)
                {
                    survivorsDropdown.ClearOptions();
                    selectedSurvivor.SetInfo(mySurvivorsData[0]);
                }
                survivorsDropdown.AddOptions(new List<string>() { survivorsInHireMarket[candidate].suruvivorData.survivorName });
                
                hireSurvivor.SetActive(false);
            });
        
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
}

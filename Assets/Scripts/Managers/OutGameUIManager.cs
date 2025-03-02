using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public struct SurvivorInfo
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
    [SerializeField] Candidate[] candidates;
    [SerializeField] GameObject confirmCanvas;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmButton;

    List<SurvivorInfo> mySurvivors;
    static SurvivorInfo mySurvivorInfo;
    public static SurvivorInfo MySurvivorInfo => mySurvivorInfo;
    private void Start()
    {
        mySurvivors = new();
        SetCandidates();
    }

    public void SetCandidates()
    {
        candidates[0].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 200, 10, 1, 3, 1, 7.5f);
        candidates[1].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 100, 20, 1, 3, 1, 7.5f);
        candidates[2].SetInfo(Names.SurvivorName[Random.Range(0, Names.SurvivorName.Length)], 100, 10, 1, 4.5f, 1.5f, 7.5f);
    }

    public void ChooseSurvivor(int candidate)
    {
        OpenConfirmCanvas($"Are you sure to hire {candidates[candidate].candidateInfo.survivorName} for $ {candidates[candidate].candidateInfo.price} ?",
            () => {
                mySurvivors.Add(candidates[candidate].candidateInfo);
                mySurvivorInfo = candidates[candidate].candidateInfo;
                StartCoroutine(GameManager.Instance.BattleRoyalStart());
            });
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

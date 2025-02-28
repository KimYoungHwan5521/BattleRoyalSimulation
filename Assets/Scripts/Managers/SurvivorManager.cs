using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SurvivorInfo
{
    public string survivorName;
    public float hp;
    public float attackDamage;
    public float attackSpeed;
    public float moveSpeed;
    public float farmingSpeed;
    public float aimErrorRange;
}

public class SurvivorManager : MonoBehaviour
{
    [SerializeField] Candidate[] candidates;

    static SurvivorInfo mySurvivorInfo;
    public static SurvivorInfo MySurvivorInfo => mySurvivorInfo;
    private void Start()
    {
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
        mySurvivorInfo = candidates[candidate].candidateInfo;
        StartCoroutine(GameManager.Instance.BattleRoyalStart());
    }
}

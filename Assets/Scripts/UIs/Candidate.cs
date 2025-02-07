using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Candidate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI survivorNameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI attackDamageText;
    [SerializeField] TextMeshProUGUI attackSpeedText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI farmingSpeedText;
    [SerializeField] TextMeshProUGUI aimErrorRangeText;

    public SurvivorInfo candidateInfo;

    public void SetInfo(string survivorName, float hp, float attackDamage, float attackSpeed, float moveSpeed, float farmingSpeed, float aimErrorRange)
    {
        candidateInfo.survivorName = survivorName;
        candidateInfo.hp = hp;
        candidateInfo.attackDamage = attackDamage;
        candidateInfo.attackSpeed = attackSpeed;
        candidateInfo.moveSpeed = moveSpeed;
        candidateInfo.farmingSpeed = farmingSpeed;
        candidateInfo.aimErrorRange = aimErrorRange;

        survivorNameText.text = survivorName;
        hpText.text = $"HP\t\t\t: {hp}";
        attackDamageText.text = $"Attack Damage\t: {attackDamage}";
        attackSpeedText.text = $"Attack Speed\t: {attackSpeed}";
        moveSpeedText.text = $"Move Speed\t: {moveSpeed}";
        farmingSpeedText.text = $"Farming Speed\t: {farmingSpeed}";
        aimErrorRangeText.text = $"Aim Error Range\t: {aimErrorRange}";
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivorInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI survivorNameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI attackDamageText;
    [SerializeField] TextMeshProUGUI attackSpeedText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI farmingSpeedText;
    [SerializeField] TextMeshProUGUI aimErrorRangeText;

    public SurvivorData suruvivorData;

    public void SetInfo(string survivorName, float hp, float attackDamage, float attackSpeed, float moveSpeed, float farmingSpeed, float aimErrorRange)
    {
        suruvivorData.survivorName = survivorName;
        suruvivorData.hp = hp;
        suruvivorData.attackDamage = attackDamage;
        suruvivorData.attackSpeed = attackSpeed;
        suruvivorData.moveSpeed = moveSpeed;
        suruvivorData.farmingSpeed = farmingSpeed;
        suruvivorData.aimErrorRange = aimErrorRange;

        survivorNameText.text = survivorName;
        hpText.text = $"HP\t\t\t: {hp}";
        attackDamageText.text = $"Attack Damage\t: {attackDamage}";
        attackSpeedText.text = $"Attack Speed\t: {attackSpeed}";
        moveSpeedText.text = $"Move Speed\t: {moveSpeed}";
        farmingSpeedText.text = $"Farming Speed\t: {farmingSpeed}";
        aimErrorRangeText.text = $"Aim Error Range\t: {aimErrorRange}";
    }

    public void SetInfo(SurvivorData wnatSurvivorData)
    {
        SetInfo(wnatSurvivorData.survivorName, wnatSurvivorData.hp, wnatSurvivorData.attackDamage, wnatSurvivorData.attackSpeed,
            wnatSurvivorData.moveSpeed, wnatSurvivorData.farmingSpeed, wnatSurvivorData.aimErrorRange);
    }
}

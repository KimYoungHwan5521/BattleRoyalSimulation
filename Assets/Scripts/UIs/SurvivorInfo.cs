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
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] TextMeshProUGUI priceText;

    [SerializeField] GameObject[] injuries;

    [SerializeField] GameObject soldOutImage;
    bool soldOut;
    public bool SoldOut
    {
        get => soldOut;
        set
        {
            soldOut = value;
            soldOutImage.SetActive(value);
        }
    }

    public SurvivorData survivorData;

    public void SetInfo(string survivorName, float hp, float attackDamage, float attackSpeed, float moveSpeed, 
        float farmingSpeed, float shooting, int price, Tier tier)
    {
        survivorData = new(survivorName, hp, attackDamage, attackSpeed, moveSpeed, farmingSpeed, shooting, price, tier);

        survivorNameText.text = survivorName;
        hpText.text = $"HP\t\t\t: {hp:0}";
        attackDamageText.text = $"Attack Damage\t: {attackDamage:0.##}";
        attackSpeedText.text = $"Attack Speed\t: {attackSpeed:0.###}";
        moveSpeedText.text = $"Move Speed\t: {moveSpeed:0.###}";
        farmingSpeedText.text = $"Farming Speed\t: {farmingSpeed:0.###}";
        shootingText.text = $"Shooting\t: {shooting:0.##}";
        priceText.text = $"$ {price}";
    }

    public void SetInfo(SurvivorData wantSurvivorData, bool showIncrease)
    {
        survivorNameText.text = wantSurvivorData.survivorName;
        hpText.text = $"HP\t\t\t: {wantSurvivorData.hp:0}";
        attackDamageText.text = $"Attack Damage\t: {wantSurvivorData.attackDamage:0.##}";
        attackSpeedText.text = $"Attack Speed\t: {wantSurvivorData.attackSpeed:0.###}";
        moveSpeedText.text = $"Move Speed\t: {wantSurvivorData.moveSpeed:0.###}";
        farmingSpeedText.text = $"Farming Speed\t: {wantSurvivorData.farmingSpeed:0.###}";
        shootingText.text = $"Shooting\t: {wantSurvivorData.shooting:0.##}";
        if(showIncrease)
        {
            if (wantSurvivorData.increaseComparedToPrevious_hp > 0) hpText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_hp})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_attackDamage > 0) attackDamageText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_attackDamage:0.##})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_attackSpeed > 0) attackSpeedText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_attackSpeed:0.###})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_moveSpeed > 0) moveSpeedText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_moveSpeed:0.###})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_farmingSpeed > 0) farmingSpeedText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_farmingSpeed:0.###})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_shooting > 0) shootingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_shooting:0.##})</color>";
        }

        for (int i = 0; i < injuries.Length; i++)
        {
            if (wantSurvivorData.injuries.Count > i)
            {
                injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{wantSurvivorData.injuries[i].site} {wantSurvivorData.injuries[i].type}";
                injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{wantSurvivorData.injuries[i].degree:0.##}";
                injuries[i].GetComponentInChildren<Help>().SetDescription(wantSurvivorData.injuries[i].site);
                injuries[i].SetActive(true);
            }
            else
            {
                injuries[i].SetActive(false);
            }
        }
    }
}

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
        attackDamageText.text = $"Attack Damage\t: {attackDamage:0}";
        attackSpeedText.text = $"Attack Speed\t: {attackSpeed:0.##}";
        moveSpeedText.text = $"Move Speed\t: {moveSpeed:0.##}";
        farmingSpeedText.text = $"Farming Speed\t: {farmingSpeed:0.##}";
        shootingText.text = $"Shooting\t: {shooting:0.##}";
        if(priceText != null) priceText.text = $"$ {price}";
    }

    public void SetInfo(SurvivorData wantSurvivorData)
    {
        SetInfo(wantSurvivorData.survivorName, wantSurvivorData.hp, wantSurvivorData.attackDamage, wantSurvivorData.attackSpeed,
            wantSurvivorData.moveSpeed, wantSurvivorData.farmingSpeed, wantSurvivorData.shooting, wantSurvivorData.price, wantSurvivorData.tier);
    }
}

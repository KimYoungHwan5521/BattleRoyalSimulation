using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurvivorInfo : MonoBehaviour
{
    [SerializeField] Image tierImage;
    [SerializeField] TextMeshProUGUI survivorNameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI attackSpeedText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI farmingSpeedText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] Image hpBar;
    [SerializeField] Image powerBar;
    [SerializeField] Image attackSpeedBar;
    [SerializeField] Image moveSpeedBar;
    [SerializeField] Image farmingSpeedBar;
    [SerializeField] Image shootingBar;
    [SerializeField] AutoNewLineLayoutGroup characteristicsLayout;
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

    public void SetInfo(string survivorName, float hp, int power, int attackSpeed, int moveSpeed,
        int farmingSpeed, int shooting, int characteristicsCount, int price, Tier tier)
    {
        survivorData = new(survivorName, hp, power, attackSpeed, moveSpeed, farmingSpeed, shooting, price, tier);

        Enum.TryParse(tier.ToString(), out ResourceEnum.Sprite tierSprite); 
        tierImage.sprite = ResourceManager.Get(tierSprite);
        
        survivorNameText.text = survivorName;
        hpText.text = $"{hp:0}";
        powerText.text = $"{power}";
        attackSpeedText.text = $"{attackSpeed}";
        moveSpeedText.text = $"{moveSpeed}";
        farmingSpeedText.text = $"{farmingSpeed}";
        shootingText.text = $"{shooting}";
        CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicsCount);
        priceText.text = $"$ {price}";

        hpBar.fillAmount = (hp - 100) / 100f;
        powerBar.fillAmount = power / 100f;
        attackSpeedBar.fillAmount = attackSpeed / 100f;
        moveSpeedBar.fillAmount = moveSpeed / 100f;
        farmingSpeedBar.fillAmount = farmingSpeed / 100f;
        shootingBar.fillAmount = shooting / 100f;

        SetCharacteristic();
    }

    public void SetInfo(SurvivorData wantSurvivorData, bool showIncrease)
    {
        Enum.TryParse(wantSurvivorData.tier.ToString(), out ResourceEnum.Sprite tierSprite);
        tierImage.sprite = ResourceManager.Get(tierSprite);

        survivorData = wantSurvivorData;
        survivorNameText.text = wantSurvivorData.survivorName;
        hpText.text = $"{wantSurvivorData.hp:0}";
        powerText.text = $"{wantSurvivorData._power}";
        attackSpeedText.text = $"{wantSurvivorData._attackSpeed}";
        moveSpeedText.text = $"{wantSurvivorData._moveSpeed}";
        farmingSpeedText.text = $"{wantSurvivorData._farmingSpeed}";
        shootingText.text = $"{wantSurvivorData._shooting}";

        hpBar.fillAmount = (wantSurvivorData.hp - 100) / 100f;
        powerBar.fillAmount = wantSurvivorData._power / 100f;
        attackSpeedBar.fillAmount = wantSurvivorData._attackSpeed / 100f;
        moveSpeedBar.fillAmount = wantSurvivorData._moveSpeed / 100f;
        farmingSpeedBar.fillAmount = wantSurvivorData._farmingSpeed / 100f;
        shootingBar.fillAmount = wantSurvivorData._shooting / 100f;

        if (showIncrease)
        {
            if (wantSurvivorData.increaseComparedToPrevious_hp > 0) hpText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_hp:0.##})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_power > 0) powerText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_power:0.##})</color>";
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
        SetCharacteristic();
    }

    public void SetCharacteristic()
    {
        characteristicsLayout.ArrangeCharacteristics(survivorData);
    }
}

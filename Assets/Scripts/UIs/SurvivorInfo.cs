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
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI agilityText;
    [SerializeField] TextMeshProUGUI fightingText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] TextMeshProUGUI knowledgeText;
    [SerializeField] Image strengthBar;
    [SerializeField] Image agilityBar;
    [SerializeField] Image fightingBar;
    [SerializeField] Image shootingBar;
    [SerializeField] Image knowledgeBar;
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

    public void SetInfo(string survivorName, int strength, int agility, int fighting, int shooting,
        int knowledge, int characteristicsCount, int price, Tier tier)
    {
        survivorData = new(survivorName, strength, agility, fighting, shooting, knowledge, price, tier);

        Enum.TryParse(tier.ToString(), out ResourceEnum.Sprite tierSprite); 
        tierImage.sprite = ResourceManager.Get(tierSprite);
        
        survivorNameText.text = survivorName;
        strengthText.text = $"{strength}";
        agilityText.text = $"{agility}";
        fightingText.text = $"{fighting}";
        shootingText.text = $"{shooting}";
        knowledgeText.text = $"{knowledge}";
        CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicsCount);
        priceText.text = $"$ {price}";

        strengthBar.fillAmount = strength / 100f;
        agilityBar.fillAmount = agility / 100f;
        fightingBar.fillAmount = fighting / 100f;
        shootingBar.fillAmount = shooting / 100f;
        knowledgeBar.fillAmount = knowledge / 100f;

        SetCharacteristic();
    }

    public void SetInfo(SurvivorData wantSurvivorData, bool showIncrease)
    {
        Enum.TryParse(wantSurvivorData.tier.ToString(), out ResourceEnum.Sprite tierSprite);
        tierImage.sprite = ResourceManager.Get(tierSprite);

        survivorData = wantSurvivorData;
        survivorNameText.text = wantSurvivorData.survivorName;
        strengthText.text = $"{wantSurvivorData._strength}";
        agilityText.text = $"{wantSurvivorData._agility}";
        fightingText.text = $"{wantSurvivorData._fighting}";
        shootingText.text = $"{wantSurvivorData._shooting}";
        knowledgeText.text = $"{wantSurvivorData._knowledge}";

        strengthBar.fillAmount = wantSurvivorData._strength / 100f;
        agilityBar.fillAmount = wantSurvivorData._agility / 100f;
        fightingBar.fillAmount = wantSurvivorData._fighting / 100f;
        shootingBar.fillAmount = wantSurvivorData._shooting / 100f;
        knowledgeBar.fillAmount = wantSurvivorData._knowledge / 100f;

        if (showIncrease)
        {
            if (wantSurvivorData.increaseComparedToPrevious_strength > 0) strengthText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_strength})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_agility > 0) agilityText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_agility})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_fighting > 0) fightingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_fighting})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_shooting > 0) shootingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_shooting})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_knowledge > 0) knowledgeText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_knowledge})</color>";
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

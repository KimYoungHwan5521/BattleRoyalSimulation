using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TrainingCard : MonoBehaviour
{
    [SerializeField] Image cardBody;
    [SerializeField] LocalizeStringEvent trainingName;
    [SerializeField] Image trainingImage;
    [SerializeField] TextMeshProUGUI trainingExplain;
    [SerializeField] TextMeshProUGUI failRateText;
    TrainingInfo linkedTraining;
    public TrainingInfo LinkedTraining => linkedTraining;

    public void SetCard(TrainingInfo training)
    {
        linkedTraining = training;
        cardBody.color = training.rarity switch
        {
            TrainingRarity.Common => new Color(0.2984f, 0.8483f, 0.9471f),
            TrainingRarity.Uncommon => new Color(0.8050f, 0.2980f, 0.9490f),
            TrainingRarity.Rare => new Color(0.9490f, 0.8036f, 0.2980f),
            _ => new(1, 1, 1)
        };
        trainingName.StringReference = training.trainingName;
        string spriteName = training.trainingName.TableEntryReference.Key.Replace(" ", "");
        if(Enum.TryParse(spriteName, out ResourceEnum.Sprite sprite))
        {
            trainingImage.sprite = ResourceManager.Get(sprite);
        }
        else trainingImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
        trainingImage.GetComponentInChildren<AspectRatioFitter>().aspectRatio = trainingImage.sprite.rect.width / trainingImage.sprite.rect.height;
        trainingExplain.text = training.GetTrainingExplain(false);
        int stamina = GameManager.Instance.OutGameUIManager.Stamina;
        float failRate = stamina < training.staminaConsumtion ? 1f : stamina < training.trainingDifficulty ? 1f - (float)stamina / training.trainingDifficulty : 0;
        failRateText.text = new LocalizedString("Basic", "FailRate")
        {
            Arguments = new[] { $"{failRate * 100:0}" }
        }.GetLocalizedString();
    }

    public void Select(bool selected)
    {
        Color c = linkedTraining.rarity switch
        {
            TrainingRarity.Common => new Color(0.2984f, 0.8483f, 0.9471f),
            TrainingRarity.Uncommon => new Color(0.8050f, 0.2980f, 0.9490f),
            TrainingRarity.Rare => new Color(0.9490f, 0.8036f, 0.2980f),
            _ => new(1, 1, 1)
        };
        c = selected ? c * new Color(0.78f, 0.78f, 0.78f, 1f) : c;
        cardBody.color = c;
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (linkedTraining == null) return;
        trainingExplain.text = linkedTraining.GetTrainingExplain(false);
        int stamina = GameManager.Instance.OutGameUIManager.Stamina;
        float failRate = stamina < linkedTraining.staminaConsumtion ? 1f : stamina < linkedTraining.trainingDifficulty ? stamina / linkedTraining.trainingDifficulty : 0;
        failRateText.text = new LocalizedString("Basic", "FailRate")
        {
            Arguments = new[] { $"{failRate * 100:0}" }
        }.GetLocalizedString();
    }
}

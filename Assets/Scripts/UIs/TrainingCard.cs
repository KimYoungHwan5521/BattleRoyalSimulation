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
    TrainingInfo linkedTraining;

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
        // trainingImage
        trainingExplain.text = "";
        foreach(var value in training.increaseStats)
        {
            if (!string.IsNullOrEmpty(trainingExplain.text)) trainingExplain.text += ", ";
            trainingExplain.text += value.Item1 switch
            {
                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                6 => new LocalizedString("Basic", "Random").GetLocalizedString(),
                _ => new LocalizedString("Basic", "Strength").GetLocalizedString()
            };
            trainingExplain.text += $" + {value.Item2}";
        }
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (linkedTraining == null) return;
        foreach (var value in linkedTraining.increaseStats)
        {
            if (!string.IsNullOrEmpty(trainingExplain.text)) trainingExplain.text += ", ";
            trainingExplain.text += value.Item1 switch
            {
                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                6 => new LocalizedString("Basic", "Random").GetLocalizedString(),
                _ => new LocalizedString("Basic", "Strength").GetLocalizedString()
            };
            trainingExplain.text += $" + {value.Item2}";
        }
    }
}

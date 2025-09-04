using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SurvivorInfo : MonoBehaviour
{
    [SerializeField] Image tierImage;
    [SerializeField] LocalizeStringEvent survivorNameText;
    [SerializeField] LocalizeStringEvent scheduledTrainingText;
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

    [Header("Injury Head")]
    [SerializeField] GameObject[] injuries;
    [SerializeField] Image head;
    [SerializeField] Image rightEye;
    [SerializeField] Image leftEye;
    [SerializeField] Image rightEar;
    [SerializeField] Image leftEar;
    [SerializeField] Image cheek;
    [SerializeField] Image nose;
    [SerializeField] Image jaw;
    [SerializeField] Image neck;
    [Header("Torso")]
    [SerializeField] Image chest;
    [SerializeField] Image ribs;
    [SerializeField] Image abdomen;
    [SerializeField] Image organs;
    [Header("Arm")]
    [SerializeField] Image rightArm;
    [SerializeField] Image leftArm;
    [SerializeField] Image rightHand;
    [SerializeField] Image leftHand;
    [SerializeField] Image rightThumb;
    [SerializeField] Image leftThumb;
    [SerializeField] Image rightIndexFinger;
    [SerializeField] Image leftIndexFinger;
    [SerializeField] Image rightMiddleFinger;
    [SerializeField] Image leftMiddleFinger;
    [SerializeField] Image rightRingFinger;
    [SerializeField] Image leftRingFinger;
    [SerializeField] Image rightLittleFinger;
    [SerializeField] Image leftLittleFinger;
    [Header("Leg")]
    [SerializeField] Image rightLeg;
    [SerializeField] Image leftLeg;
    [SerializeField] Image rightKnee;
    [SerializeField] Image leftKnee;
    [SerializeField] Image rightFoot;
    [SerializeField] Image leftFoot;
    [SerializeField] Image rightBigToe;
    [SerializeField] Image leftBigToe;
    [SerializeField] Image rightIndexToe;
    [SerializeField] Image leftIndexToe;
    [SerializeField] Image rightMiddleToe;
    [SerializeField] Image leftMiddleToe;
    [SerializeField] Image rightRingToe;
    [SerializeField] Image leftRingToe;
    [SerializeField] Image rightLittleToe;
    [SerializeField] Image leftLittleToe;

    [Header("Stastics")]
    [SerializeField] GameObject trophyBronze;
    [SerializeField] GameObject trophySilver;
    [SerializeField] GameObject trophyGold;
    [SerializeField] GameObject trophySeason;
    [SerializeField] GameObject trophyMelee;
    [SerializeField] GameObject trophyRanged;
    [SerializeField] GameObject trophyCrafting;
    [SerializeField] GameObject trophyWorld;
    [SerializeField] TextMeshProUGUI totalRecord;
    [SerializeField] TextMeshProUGUI totalRecordGoldPlus;
    [SerializeField] TextMeshProUGUI totalKill;
    [SerializeField] TextMeshProUGUI totalSurvivalTime;
    [SerializeField] TextMeshProUGUI totalPrize;
    [SerializeField] TextMeshProUGUI totalFee;
    [SerializeField] TextMeshProUGUI totalGiveDamage;
    [SerializeField] TextMeshProUGUI totalTakeDamage;

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

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void SetInfo(LocalizedString survivorName, int strength, int agility, int fighting, int shooting,
        int knowledge, int characteristicsCount, int price, Tier tier)
    {
        survivorData = new(survivorName, strength, agility, fighting, shooting, knowledge, price, tier);

        if (tierImage != null)
        {
            Enum.TryParse(tier.ToString(), out ResourceEnum.Sprite tierSprite); 
            tierImage.sprite = ResourceManager.Get(tierSprite);
            tierImage.GetComponent<Help>().SetDescriptionWithKey(tier.ToString());
        }
        
        survivorNameText.StringReference = survivorName;
        if (scheduledTrainingText != null) scheduledTrainingText.StringReference = new("Basic", $"{survivorData.assignedTraining}");
        strengthText.text = $"{survivorData.Strength}";
        agilityText.text = $"{survivorData.Agility}";
        fightingText.text = $"{survivorData.Fighting}";
        shootingText.text = $"{survivorData.Shooting}";
        knowledgeText.text = $"{survivorData.Knowledge}";
        CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicsCount);
        priceText.text = $"$ {price}";

        strengthBar.fillAmount = survivorData.Strength / 100f;
        agilityBar.fillAmount = survivorData.Agility / 100f;
        fightingBar.fillAmount = survivorData.Fighting / 100f;
        shootingBar.fillAmount = survivorData.Shooting / 100f;
        knowledgeBar.fillAmount = survivorData.Knowledge / 100f;

        SetCharacteristic();
    }

    public void SetInfo(SurvivorData wantSurvivorData, bool showIncrease)
    {
        if(tierImage != null)
        {
            Enum.TryParse(wantSurvivorData.tier.ToString(), out ResourceEnum.Sprite tierSprite);
            tierImage.sprite = ResourceManager.Get(tierSprite);
            tierImage.GetComponent<Help>().SetDescriptionWithKey(wantSurvivorData.tier.ToString());
        }

        survivorData = wantSurvivorData;
        if (scheduledTrainingText != null)
        {
            if(survivorData.assignedTraining == Training.None) scheduledTrainingText.StringReference = new("Basic", $"None");
            else scheduledTrainingText.StringReference = new("Basic", $"Training:{survivorData.assignedTraining}");

        }
        survivorNameText.StringReference = wantSurvivorData.localizedSurvivorName;
        strengthText.text = $"{wantSurvivorData.Strength}";
        agilityText.text = $"{wantSurvivorData.Agility}";
        fightingText.text = $"{wantSurvivorData.Fighting}";
        shootingText.text = $"{wantSurvivorData.Shooting}";
        knowledgeText.text = $"{wantSurvivorData.Knowledge}";

        strengthBar.fillAmount = wantSurvivorData.Strength / 100f;
        agilityBar.fillAmount = wantSurvivorData.Agility / 100f;
        fightingBar.fillAmount = wantSurvivorData.Fighting / 100f;
        shootingBar.fillAmount = wantSurvivorData.Shooting / 100f;
        knowledgeBar.fillAmount = wantSurvivorData.Knowledge / 100f;

        if (showIncrease)
        {
            if (wantSurvivorData.increaseComparedToPrevious_strength > -1) strengthText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_strength})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_agility > -1) agilityText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_agility})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_fighting > -1) fightingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_fighting})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_shooting > -1) shootingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_shooting})</color>";
            if (wantSurvivorData.increaseComparedToPrevious_knowledge > -1) knowledgeText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_knowledge})</color>";
        }
        SetInjury(wantSurvivorData.injuries);
        SetCharacteristic();
        SetStastics();
    }

    Image GetTargetImage(InjurySite site)
    {
        Image targetPart = null;
        targetPart = site switch
        {
            InjurySite.None => throw new NotImplementedException(),
            InjurySite.Head => head,
            InjurySite.RightEye => rightEye,
            InjurySite.LeftEye => leftEye,
            InjurySite.RightEar => rightEar,
            InjurySite.LeftEar => leftEar,
            InjurySite.Cheek => cheek,
            InjurySite.Nose => nose,
            InjurySite.Jaw => jaw,
            InjurySite.Skull => head,
            InjurySite.Brain => head,
            InjurySite.Neck => neck,
            InjurySite.Chest => chest,
            InjurySite.Ribs => ribs,
            InjurySite.Abdomen => abdomen,
            InjurySite.Organ => organs,
            InjurySite.RightArm => rightArm,
            InjurySite.LeftArm => leftArm,
            InjurySite.RightHand => rightHand,
            InjurySite.LeftHand => leftHand,
            InjurySite.RightThumb => rightThumb,
            InjurySite.RightIndexFinger => rightIndexFinger,
            InjurySite.RightMiddleFinger => rightMiddleFinger,
            InjurySite.RightRingFinger => rightRingFinger,
            InjurySite.RightLittleFinger => rightLittleFinger,
            InjurySite.LeftThumb => leftThumb,
            InjurySite.LeftIndexFinger => leftIndexFinger,
            InjurySite.LeftMiddleFinger => leftMiddleFinger,
            InjurySite.LeftRingFinger => leftRingFinger,
            InjurySite.LeftLittleFinger => leftLittleFinger,
            InjurySite.RightLeg => rightLeg,
            InjurySite.LeftLeg => leftLeg,
            InjurySite.RightKnee => rightKnee,
            InjurySite.LeftKnee => leftKnee,
            InjurySite.RightFoot => rightFoot,
            InjurySite.LeftFoot => leftFoot,
            InjurySite.RightBigToe => rightBigToe,
            InjurySite.LeftBigToe => leftBigToe,
            InjurySite.RightIndexToe => rightIndexToe,
            InjurySite.LeftIndexToe => leftIndexToe,
            InjurySite.RightMiddleToe => rightMiddleToe,
            InjurySite.LeftMiddleToe => leftMiddleToe,
            InjurySite.RightRingToe => rightRingToe,
            InjurySite.LeftRingToe => leftRingToe,
            InjurySite.RightLittleToe => rightLittleToe,
            InjurySite.LeftLittleToe => leftLittleToe,
            _ => throw new NotImplementedException()
        };
        return targetPart;
    }

    void SetInjury(List<Injury> injuries)
    {
        if (head == null) return;
        ResetInjuryInfo();
        foreach(var injury in injuries)
        {
            Image targetPart = GetTargetImage(injury.site);
            if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged)
            {
                Color wantColor = new(0.6f, 0.6f, 0.6f);
                targetPart.color = wantColor;
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
                }
            }
            else if(injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged)
            {
                Color wantColor = new(1f, 0f, 1f);
                targetPart.color = wantColor;
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
                }
            }
            else if (injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged)
            {
                Color wantColor = new(0f, 1f, 1f);
                targetPart.color = wantColor;
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
                }
            }
            else if (injury.degree == 1)
            {
                Color wantColor = new(0.3f, 0.3f, 0.3f);
                targetPart.color = wantColor;
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>().SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
                }
            }
            else
            {
                targetPart.color = new Color(1f, (1 - injury.degree) * 0.7f, (1 - injury.degree) * 0.7f);
            }
            targetPart.GetComponentInChildren<Help>().SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
        }

    }

    void ResetInjuryInfo()
    {
        head.color = Color.white;
        rightEye.color = Color.white;
        leftEye.color = Color.white;
        rightEar.color = Color.white;
        leftEar.color = Color.white;
        cheek.color = Color.white;
        nose.color = Color.white;
        jaw.color = Color.white;
        chest.color = Color.white;
        ribs.color = Color.white;
        abdomen.color = Color.white;
        organs.color = Color.white;
        rightArm.color = Color.white;
        leftArm.color = Color.white;
        rightHand.color = Color.white;
        leftHand.color = Color.white;
        rightThumb.color = Color.white;
        leftThumb.color = Color.white;
        rightIndexFinger.color = Color.white;
        leftIndexFinger.color = Color.white;
        rightMiddleFinger.color = Color.white;
        leftMiddleFinger.color = Color.white;
        rightRingFinger.color = Color.white;
        leftRingFinger.color = Color.white;
        rightLittleFinger.color = Color.white;
        leftLittleFinger.color = Color.white;
        rightLeg.color = Color.white;
        leftLeg.color = Color.white;
        rightKnee.color = Color.white;
        leftKnee.color = Color.white;
        rightFoot.color = Color.white;
        leftFoot.color = Color.white;
        rightBigToe.color = Color.white;
        leftBigToe.color = Color.white;
        rightIndexToe.color = Color.white;
        leftIndexToe.color = Color.white;
        rightMiddleToe.color = Color.white;
        leftMiddleToe.color = Color.white;
        rightRingToe.color = Color.white;
        leftRingToe.color = Color.white;
        rightLittleToe.color = Color.white;
        leftLittleToe.color = Color.white;

        head.GetComponentInChildren<Help>().SetDescription("");
        rightEye.GetComponentInChildren<Help>().SetDescription("");
        leftEye.GetComponentInChildren<Help>().SetDescription("");
        rightEar.GetComponentInChildren<Help>().SetDescription("");
        leftEar.GetComponentInChildren<Help>().SetDescription("");
        cheek.GetComponentInChildren<Help>().SetDescription("");
        nose.GetComponentInChildren<Help>().SetDescription("");
        jaw.GetComponentInChildren<Help>().SetDescription("");
        chest.GetComponentInChildren<Help>().SetDescription("");
        ribs.GetComponentInChildren<Help>().SetDescription("");
        abdomen.GetComponentInChildren<Help>().SetDescription("");
        organs.GetComponentInChildren<Help>().SetDescription("");
        rightArm.GetComponentInChildren<Help>().SetDescription("");
        leftArm.GetComponentInChildren<Help>().SetDescription("");
        rightHand.GetComponentInChildren<Help>().SetDescription("");
        leftHand.GetComponentInChildren<Help>().SetDescription("");
        rightThumb.GetComponentInChildren<Help>().SetDescription("");
        leftThumb.GetComponentInChildren<Help>().SetDescription("");
        rightIndexFinger.GetComponentInChildren<Help>().SetDescription("");
        leftIndexFinger.GetComponentInChildren<Help>().SetDescription("");
        rightMiddleFinger.GetComponentInChildren<Help>().SetDescription("");
        leftMiddleFinger.GetComponentInChildren<Help>().SetDescription("");
        rightRingFinger.GetComponentInChildren<Help>().SetDescription("");
        leftRingFinger.GetComponentInChildren<Help>().SetDescription("");
        rightLittleFinger.GetComponentInChildren<Help>().SetDescription("");
        leftLittleFinger.GetComponentInChildren<Help>().SetDescription("");
        rightLeg.GetComponentInChildren<Help>().SetDescription("");
        leftLeg.GetComponentInChildren<Help>().SetDescription("");
        rightKnee.GetComponentInChildren<Help>().SetDescription("");
        leftKnee.GetComponentInChildren<Help>().SetDescription("");
        rightFoot.GetComponentInChildren<Help>().SetDescription("");
        leftFoot.GetComponentInChildren<Help>().SetDescription("");
        rightBigToe.GetComponentInChildren<Help>().SetDescription("");
        leftBigToe.GetComponentInChildren<Help>().SetDescription("");
        rightIndexToe.GetComponentInChildren<Help>().SetDescription("");
        leftIndexToe.GetComponentInChildren<Help>().SetDescription("");
        rightMiddleToe.GetComponentInChildren<Help>().SetDescription("");
        leftMiddleToe.GetComponentInChildren<Help>().SetDescription("");
        rightRingToe.GetComponentInChildren<Help>().SetDescription("");
        leftRingToe.GetComponentInChildren<Help>().SetDescription("");
        rightLittleToe.GetComponentInChildren<Help>().SetDescription("");
        leftLittleToe.GetComponentInChildren<Help>().SetDescription("");
    }

    public void SetCharacteristic()
    {
        characteristicsLayout.ArrangeCharacteristics(survivorData);
    }

    void SetStastics()
    {
        if (trophyBronze == null) return;
        trophyBronze.SetActive(survivorData.wonBronzeLeague);
        trophySilver.SetActive(survivorData.wonSilverLeague);
        trophyGold.SetActive(survivorData.wonGoldLeague);
        trophySeason.SetActive(survivorData.wonSeasonChampionship);
        trophyMelee.SetActive(survivorData.wonMeleeLeague);
        trophyRanged.SetActive(survivorData.wonRangedLeague);
        trophyCrafting.SetActive(survivorData.wonCraftingLeague);
        trophyWorld.SetActive(survivorData.wonWorldChampionship);

        LocalizedString localizedString = new("Basic", "Total Record")
        {
            Arguments = new[] { $"{survivorData.winCount}", $"{survivorData.rankDefenseCount}", $"{survivorData.loseCount}" }
        };
        totalRecord.text = $"{localizedString.GetLocalizedString()}\n";
        string winRate = survivorData.winCount + survivorData.rankDefenseCount + survivorData.loseCount > 0 ? 
            $"{(float)survivorData.winCount / (survivorData.winCount + survivorData.rankDefenseCount + survivorData.loseCount) * 100:0.##}" : "0";
        string rankDefenseRate = survivorData.winCount + survivorData.rankDefenseCount + survivorData.loseCount > 0 ? 
            $"{((float)survivorData.winCount + survivorData.rankDefenseCount) / (survivorData.winCount + survivorData.rankDefenseCount + survivorData.loseCount) * 100:0.##}" : "0";
        localizedString = new("Basic", "Win Rate")
        {
            Arguments = new[] { winRate, rankDefenseRate }
        };
        totalRecord.text += localizedString.GetLocalizedString();

        localizedString = new("Basic", "Total Record Gold+")
        {
            Arguments = new[] {$"{survivorData.winCountGoldPlus}", $"{survivorData.rankDefenseCountGoldPlus}", $"{survivorData.loseCountGoldPlus}" }
        };
        totalRecordGoldPlus.text = $"{localizedString.GetLocalizedString()}\n";
        winRate = survivorData.winCountGoldPlus + survivorData.rankDefenseCountGoldPlus + survivorData.loseCountGoldPlus > 0 ?
            $"{(float)survivorData.winCountGoldPlus / (survivorData.winCountGoldPlus + survivorData.rankDefenseCountGoldPlus + survivorData.loseCountGoldPlus) * 100:0.##}" : "0";
        rankDefenseRate = survivorData.winCountGoldPlus + survivorData.rankDefenseCountGoldPlus + survivorData.loseCountGoldPlus > 0 ?
            $"{((float)survivorData.winCountGoldPlus + survivorData.rankDefenseCountGoldPlus) / (survivorData.winCountGoldPlus + survivorData.rankDefenseCountGoldPlus + survivorData.loseCountGoldPlus) * 100:0.##}" : "0";
        localizedString = new("Basic", "Win Rate")
        {
            Arguments = new[] { winRate, rankDefenseRate }
        };
        totalRecordGoldPlus.text += localizedString.GetLocalizedString();

        localizedString = new("Basic", "Total Kill")
        {
            Arguments = new[] { $"{survivorData.totalKill}" }
        };
        totalKill.text = localizedString.GetLocalizedString();

        int time = (int)survivorData.totalSurvivedTime;
        localizedString = new("Basic", "Total Survival Time")
        {
            Arguments = new[] { $"{time / 3600}h {(time % 3600) / 60}m {time % 60}s" }
        };
        totalSurvivalTime.text = localizedString.GetLocalizedString();

        string total = $"{survivorData.totalRankPrize + survivorData.totalKillPrize}";
        localizedString = new("Basic", "Total Reward")
        {
            Arguments = new[] { total, $"{survivorData.totalRankPrize}", $"{survivorData.totalKillPrize}" }
        };
        totalPrize.text = localizedString.GetLocalizedString();

        total = $"{survivorData.totalTreatmentFee + survivorData.totalSurgeryFee}";
        localizedString = new("Basic", "Total Medical Cost")
        {
            Arguments = new[] { total, $"{survivorData.totalTreatmentFee}", $"{survivorData.totalSurgeryFee}" }
        };
        totalFee.text = localizedString.GetLocalizedString();

        localizedString = new("Basic", "Total Damage")
        {
            Arguments = new[] { $"{(int)survivorData.totalGiveDamage}" }
        };
        totalGiveDamage.text = localizedString.GetLocalizedString();

        localizedString = new("Basic", "Total Take Damage")
        {
            Arguments = new[] { $"{(int)survivorData.totalTakeDamage}" }
        };
        totalTakeDamage.text = localizedString.GetLocalizedString();
    }

    void OnLocaleChanged(Locale newLocale)
    {
        SetStastics();
    }
}

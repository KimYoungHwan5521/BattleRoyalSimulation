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
    [SerializeField] TextMeshProUGUI staminaText;
    [SerializeField] Image staminaBar;
    [SerializeField] TextMeshProUGUI leaguePointText;
    [SerializeField] Image leaguePointBar;
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI agilityText;
    [SerializeField] TextMeshProUGUI fightingText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] TextMeshProUGUI craftingText;
    [SerializeField] TextMeshProUGUI knowledgeText;
    [SerializeField] Image strengthBar;
    [SerializeField] Image agilityBar;
    [SerializeField] Image fightingBar;
    [SerializeField] Image shootingBar;
    [SerializeField] Image craftingBar;
    [SerializeField] Image knowledgeBar;
    [SerializeField] Image strenthRank;
    [SerializeField] Image agilityRank;
    [SerializeField] Image fightingRank;
    [SerializeField] Image shootingRank;
    [SerializeField] Image craftingRank;
    [SerializeField] Image knowledgeRank;
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

    [Header("Statistics")]
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
    [SerializeField] TextMeshProUGUI receivedTrainings;
    [SerializeField] TextMeshProUGUI consumedStaminas;
    [SerializeField] TextMeshProUGUI totalKill;
    [SerializeField] TextMeshProUGUI totalSurvivalTime;
    [SerializeField] TextMeshProUGUI totalPrize;
    [SerializeField] TextMeshProUGUI totalFee;
    [SerializeField] TextMeshProUGUI totalGiveDamage;
    [SerializeField] TextMeshProUGUI totalTakeDamage;
    [SerializeField] TextMeshProUGUI mostKillsInASingleMatch;

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

    bool statIncreaseAnimation;
    const float statIncreaseWait = 0.1f;
    float curStatIncreaseWait;
    const float statIncreaseTerm = 0.1f;
    float curStatIncreaseTerm;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void Update()
    {
        if(statIncreaseAnimation && staminaText != null)
        {
            curStatIncreaseWait += Time.deltaTime;
            if(curStatIncreaseWait > statIncreaseWait)
            {
                curStatIncreaseTerm += Time.deltaTime;
                if(curStatIncreaseTerm > statIncreaseTerm)
                {
                    if (survivorData.increaseComparedToPrevious_strength > 0) { survivorData.Strength = survivorData._strength + 1; survivorData.increaseComparedToPrevious_strength--; }
                    if (survivorData.increaseComparedToPrevious_agility > 0) { survivorData.Agility = survivorData._agility + 1; survivorData.increaseComparedToPrevious_agility--; }
                    if (survivorData.increaseComparedToPrevious_fighting > 0) { survivorData.Fighting = survivorData._fighting + 1; survivorData.increaseComparedToPrevious_fighting--; }
                    if (survivorData.increaseComparedToPrevious_shooting > 0) { survivorData.Shooting = survivorData._shooting + 1; survivorData.increaseComparedToPrevious_shooting--; }
                    if (survivorData.increaseComparedToPrevious_crafting > 0) { survivorData.Crafting = survivorData._crafting + 1; survivorData.increaseComparedToPrevious_crafting--; }
                    if (survivorData.increaseComparedToPrevious_knowledge > 0) { survivorData.Knowledge = survivorData._knowledge + 1; survivorData.increaseComparedToPrevious_knowledge--; }
                    if (survivorData.increaseComparedToPrevious_stamina > 0) { survivorData.Stamina += Mathf.Min(survivorData.increaseComparedToPrevious_stamina, 3); survivorData.increaseComparedToPrevious_stamina -= Mathf.Min(survivorData.increaseComparedToPrevious_stamina, 3); }
                    if (survivorData.increaseComparedToPrevious_stamina < 0) { survivorData.Stamina += Mathf.Max(survivorData.increaseComparedToPrevious_stamina, -3); survivorData.increaseComparedToPrevious_stamina -= Mathf.Max(survivorData.increaseComparedToPrevious_stamina, -3); }
                    SetInfo(survivorData, false);
                    if(survivorData.increaseComparedToPrevious_strength + survivorData.increaseComparedToPrevious_agility + survivorData.increaseComparedToPrevious_fighting + survivorData.increaseComparedToPrevious_shooting
                        +survivorData.increaseComparedToPrevious_crafting + survivorData.increaseComparedToPrevious_knowledge == 0 && survivorData.increaseComparedToPrevious_stamina == 0)
                    {
                        ResetInfo();
                    }
                }
            }
        }
    }

    public void ResetInfo()
    {
        statIncreaseAnimation = false;
        curStatIncreaseWait = 0f;
        curStatIncreaseTerm = 0f;
    }

    public void SetInfo(LocalizedString survivorName, int strength, int agility, int fighting, int shooting, int crafting,
        int knowledge, int characteristicsCount, int price, Tier tier)
    {
        survivorData = new(survivorName, strength, agility, fighting, shooting, crafting, knowledge, price, tier);

        if (tierImage != null)
        {
            Enum.TryParse(tier.ToString(), out ResourceEnum.Sprite tierSprite); 
            tierImage.sprite = ResourceManager.Get(tierSprite);
            tierImage.GetComponent<Help>().SetDescriptionWithKey(tier.ToString());
        }
        
        survivorNameText.StringReference = survivorName;
        CharacteristicManager.AddRandomCharacteristics(survivorData, characteristicsCount, true);
        survivorData._stamina = survivorData.MaxStamina;
        if (scheduledTrainingText != null) scheduledTrainingText.StringReference = new("Basic", $"{survivorData.assignedTraining}");
        strengthText.text = $"{survivorData.Strength}";
        agilityText.text = $"{survivorData.Agility}";
        fightingText.text = $"{survivorData.Fighting}";
        shootingText.text = $"{survivorData.Shooting}";
        craftingText.text = $"{survivorData.Crafting}";
        knowledgeText.text = $"{survivorData.Knowledge}";

        foreach (var characteristic in survivorData.characteristics) survivorData.price += characteristic.value;
        survivorData.price = Mathf.Max(1, survivorData.price);
        priceText.gameObject.SetActive(GameManager.Instance.OutGameUIManager.GameMode == GameMode.FreeManagement);
        priceText.text = $"$ {survivorData.price}";

        strengthBar.fillAmount = (float)survivorData.Strength / survivorData.MaxStrength;
        agilityBar.fillAmount = (float)survivorData.Agility / survivorData.MaxAgility;
        fightingBar.fillAmount = (float)survivorData.Fighting / survivorData.MaxFighting;
        shootingBar.fillAmount = (float)survivorData.Shooting / survivorData.MaxShooting;
        craftingBar.fillAmount = (float)survivorData.Crafting / survivorData.MaxCrafting;
        knowledgeBar.fillAmount = (float)survivorData.Knowledge / survivorData.MaxKnowledge;

        strenthRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Strength + 19) / 20, 7)];
        agilityRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Agility + 19) / 20, 7)];
        fightingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Fighting + 19) / 20, 7)];
        shootingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Shooting + 19) / 20, 7)];
        craftingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Crafting + 19) / 20, 7)];
        knowledgeRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((survivorData.Knowledge + 19) / 20, 7)];

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
        survivorNameText.StringReference = wantSurvivorData.localizedSurvivorName;
        if (leaguePointText != null)
        {
            leaguePointText.text = $"{wantSurvivorData.promotePoint} / 100";
            leaguePointBar.fillAmount = wantSurvivorData.promotePoint / 100f;
        }
        if(staminaText != null)
        {
            staminaText.text = $"{wantSurvivorData.Stamina} / {wantSurvivorData.MaxStamina}";
            staminaBar.fillAmount = (float)wantSurvivorData.Stamina / wantSurvivorData.MaxStamina;
        }
        strengthText.text = $"{wantSurvivorData.Strength}";
        agilityText.text = $"{wantSurvivorData.Agility}";
        fightingText.text = $"{wantSurvivorData.Fighting}";
        shootingText.text = $"{wantSurvivorData.Shooting}";
        craftingText.text = $"{wantSurvivorData.Crafting}";
        knowledgeText.text = $"{wantSurvivorData.Knowledge}";

        strengthBar.fillAmount = (float)wantSurvivorData.Strength / survivorData.MaxStrength;
        agilityBar.fillAmount = (float)wantSurvivorData.Agility / survivorData.MaxAgility;
        fightingBar.fillAmount = (float)wantSurvivorData.Fighting / survivorData.MaxFighting;
        shootingBar.fillAmount = (float)wantSurvivorData.Shooting / survivorData.MaxShooting;
        craftingBar.fillAmount = (float)wantSurvivorData.Crafting / survivorData.MaxCrafting;
        knowledgeBar.fillAmount = (float)wantSurvivorData.Knowledge / survivorData.MaxKnowledge;

        if (strenthRank != null)
        {
            strenthRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Strength + 19) / 20, 6)];
            agilityRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Agility + 19) / 20, 6)];
            fightingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Fighting + 19) / 20, 6)];
            shootingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Shooting + 19) / 20, 6)];
            craftingRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Crafting + 19) / 20, 6)];
            knowledgeRank.sprite = GameManager.Instance.OutGameUIManager.rankSprites[Mathf.Min((wantSurvivorData.Knowledge + 19) / 20, 6)];
        }

        //if (showIncrease)
        //{
        //    if (wantSurvivorData.increaseComparedToPrevious_strength > -1) strengthText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_strength})</color>";
        //    if (wantSurvivorData.increaseComparedToPrevious_agility > -1) agilityText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_agility})</color>";
        //    if (wantSurvivorData.increaseComparedToPrevious_fighting > -1) fightingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_fighting})</color>";
        //    if (wantSurvivorData.increaseComparedToPrevious_shooting > -1) shootingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_shooting})</color>";
        //    if (wantSurvivorData.increaseComparedToPrevious_crafting > -1) craftingText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_crafting})</color>";
        //    if (wantSurvivorData.increaseComparedToPrevious_knowledge > -1) knowledgeText.text += $" <color=green>(бу{wantSurvivorData.increaseComparedToPrevious_knowledge})</color>";
        //}
        SetInjury(wantSurvivorData.injuries);
        SetCharacteristic();
        SetStastics();
    }

    public void StatIncreaseAnimation()
    {
        statIncreaseAnimation = true;
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
            if (injury.degree == 1)
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
            else if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged)
            {
                Color wantColor = new(0.6f, 0.6f, 0.6f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.ArtificialPartsTransplanted;
                    if (subpartInjury == null)
                    {
                        subpartImage.color = wantColor;
                    }
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
                }
            }
            else if(injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged)
            {
                Color wantColor = new(1f, 0f, 1f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.AugmentedPartsTransplanted;
                    if (subpartInjury == null)
                    {
                        subpartImage.color = wantColor;
                    }
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
                }
            }
            else if (injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged)
            {
                Color wantColor = new(0f, 1f, 1f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.TranscendantPartsTransplanted;
                    if (subpartInjury == null)
                    {
                        subpartImage.color = wantColor;
                    }
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
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
        neck.color = Color.white;
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
        neck.GetComponentInChildren<Help>().SetDescription("");
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

        receivedTrainings.text = $"{new LocalizedString("Basic", "Training Sessions").GetLocalizedString()} : {survivorData.receivedTrainings}";
        consumedStaminas.text = $"{new LocalizedString("Basic", "Stamina Spent").GetLocalizedString()} : {survivorData.consumedStaminas}";

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

        localizedString = new("Basic", "Most Kills in a Single Match")
        {
            Arguments = new[] { $"{survivorData.mostKillsInASingleMatch}" }
        };
        mostKillsInASingleMatch.text = localizedString.GetLocalizedString();
    }

    void OnLocaleChanged(Locale newLocale)
    {
        SetStastics();
    }
}

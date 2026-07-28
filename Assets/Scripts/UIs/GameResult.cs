using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public delegate void ReserveNotification();

public class GameResult : MonoBehaviour
{
    OutGameUIManager outGameUIManager;
    Calendar calendar;

    [SerializeField] GameObject gameResult;
    [SerializeField] TextMeshProUGUI gameResultText;
    [SerializeField] GameObject mySurvivorResult;
    [SerializeField] GameObject mySurvivorTreatmentCost;

    [SerializeField] TextMeshProUGUI survivedTimeText;
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI totalDamageText;
    [SerializeField] TextMeshProUGUI increaseFightingText;
    [SerializeField] TextMeshProUGUI increaseShootingText;
    [SerializeField] TextMeshProUGUI increaseCraftingText;
    [SerializeField] GameObject buttonKeepWatching;

    [SerializeField] TextMeshProUGUI winPrizeText;
    [SerializeField] TextMeshProUGUI killPrizeText;
    [SerializeField] TextMeshProUGUI totalTreatmentCostText;
    [SerializeField] TextMeshProUGUI totalProfitText;
    [SerializeField] GameObject[] treatments;

    [SerializeField] TextMeshProUGUI bettingRewardsText;
    [SerializeField] GameObject bettingPrediction;
    [SerializeField] GameObject[] predictionTable;
    [SerializeField] Image[] predictionsBG;
    [SerializeField] TextMeshProUGUI[] predictionsText;
    [SerializeField] TextMeshProUGUI[] rankingsText;

    [SerializeField] bool resultClaimed;
    readonly float resultDelay = 2f;
    [SerializeField] float curResultDelay;
    int lastTimeScale;
    bool gameOver;
    bool winWC;

    bool resultCalculated;
    bool cachedDidPlayerParticipate;

    int cachedTotalProfit;
    int cachedPromotePointRank;
    int cachedPromotePointKill;
    long cachedBettingRewards;
    float cachedBettingOdds;

    Survivor cachedPlayerSurvivor;

    readonly List<TreatmentResultData> cachedTreatments = new();

    class TreatmentResultData
    {
        public string injurySiteKey;
        public string treatmentKey;
        public int cost;
        public bool isBloodTransfusion;
    }
    bool cachedHasBetting;

    int cachedPredictionNumber;

    string[] cachedPredictionKeys;
    string[] cachedRankingKeys;
    Color[] cachedPredictionColors;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverCanvas;
    [SerializeField] GameObject viewChampionshipProgress;
    public LocalizeStringEvent gameOverMessage;
    [SerializeField] SurvivorInfo gameOverSurvivorInfo;
    [SerializeField] GameObject earnedAchievementsBox;
    [SerializeField] Button earnedAchievementsPrevious;
    [SerializeField] Button earnedAchievementsNext;
    [SerializeField] LocalizeStringEvent earnedAchievemetName;
    [SerializeField] Image earnedAchievementImage;
    [SerializeField] TextMeshProUGUI earnedAchievemetUnlockElementText;
    int earnAchievementsCurrentPage;

    ReserveNotification notification;

    private void Start()
    {
        outGameUIManager = GetComponent<OutGameUIManager>();
        calendar = GetComponent<Calendar>();
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void Update()
    {
        if(resultClaimed)
        {
            curResultDelay += Time.unscaledDeltaTime;
            if(curResultDelay > resultDelay )
            {
                curResultDelay = 0;
                resultClaimed = false;
                ShowGameResult(GameManager.Instance.BattleRoyaleManager.BattleWinner != null);
            }
        }
    }

    public void ResetData()
    {
        gameOver = false;
        winWC = false;

        resultCalculated = false;
        resultClaimed = false;
        curResultDelay = 0;

        cachedDidPlayerParticipate = false;
        cachedPlayerSurvivor = null;
        cachedTreatments.Clear();

        cachedTotalProfit = 0;
        cachedPromotePointRank = 0;
        cachedPromotePointKill = 0;
        cachedBettingRewards = 0;
        cachedBettingOdds = 0;
        cachedHasBetting = false;

        rememberTotalProfit = 0;
        rememberPromotePoint_Rank = 0;
        rememberPromotePoint_Kill = 0;
    }

    int rememberTotalProfit;
    int rememberPromotePoint_Rank;
    int rememberPromotePoint_Kill;
    public void ShowGameResult(bool isBattleEnd = true)
    {
        lastTimeScale = (int)Time.timeScale;
        Time.timeScale = 0;

        gameResult.SetActive(true);
        buttonKeepWatching.SetActive(!isBattleEnd);

        cachedDidPlayerParticipate =
            outGameUIManager.MySurvivorDataInBattleRoyale != null;

        mySurvivorResult.SetActive(cachedDidPlayerParticipate);
        mySurvivorTreatmentCost.SetActive(cachedDidPlayerParticipate);

        if (outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            GameManager.Instance.Option.SetSaveButtonInteractable(
                false, false, true);
        }
        else
        {
            GameManager.Instance.Option.SetSaveButtonInteractable(
                true, true, false);
        }

        if (!resultCalculated)
        {
            CalculateResult(cachedDidPlayerParticipate);
            resultCalculated = true;
        }

        RefreshResultUI();

        GameManager.Instance.FixLayout(
            gameResult.GetComponent<RectTransform>());
    }

    // playerWin | 1: win, 25: top 25%, 50: top 50%, -1 : bottom 50%
    int playerWin;
    int winPrize = 0;
    int killPrize = 0;
    int totalTreatmentCost = 0;
    List<Injury> injuryNeedSurgery = new();
    void CalculateResult(bool didPlayerParticipate)
    {
        cachedTotalProfit = 0;
        cachedPromotePointRank = 0;
        cachedPromotePointKill = 0;

        playerWin = -1;

        if (didPlayerParticipate)
        {
            cachedPlayerSurvivor =
                GameManager.Instance.BattleRoyaleManager.Survivors[0];

            CalculatePlayerRank(cachedPlayerSurvivor);
            ApplyWinLoseStatistics();
            CalculatePrizes(cachedPlayerSurvivor);
            CalculateTreatments(cachedPlayerSurvivor);
            cachedTotalProfit =
                winPrize +
                killPrize -
                totalTreatmentCost;
        }

        CalculateBettingResult();

        rememberTotalProfit = cachedTotalProfit;
        rememberPromotePoint_Rank = cachedPromotePointRank;
        rememberPromotePoint_Kill = cachedPromotePointKill;
    }

    void CalculatePlayerRank(Survivor playerSurvivor)
    {
        BattleRoyaleManager manager =
            GameManager.Instance.BattleRoyaleManager;

        if (manager.BattleWinner != null &&
            manager.BattleWinner.survivorID == 0)
        {
            playerWin = 1;
            return;
        }

        for (int i = 0; i < manager.rankings.Length; i++)
        {
            if (manager.rankings[i] != playerSurvivor.survivorName)
                continue;

            float percentile = ((float)i + 1) / GameManager.Instance.BattleRoyaleManager.Survivors.Count;

            if (percentile <= 0.25f)
                playerWin = 25;
            else if (percentile <= 0.5f)
                playerWin = 50;
            else
                playerWin = -1;

            return;
        }

        playerWin = -1;
    }

    void CalculatePrizes(Survivor playerSurvivor)
    {
        winPrize = 0;
        killPrize = 0;

        cachedPromotePointRank = 0;
        cachedPromotePointKill = 0;

        SurvivorData survivorData =
            playerSurvivor.LinkedSurvivorData;

        League league =
            calendar.LeagueReserveInfo[calendar.Today].league;

        GameManager.Instance.UnlockManager.Unlock(
            UnlockManager.UnlockCondition
                .FirstParticipateInBattleRoyale);

        switch (league)
        {
            case League.BronzeLeague:
                if (playerWin == 1)
                {
                    winPrize = 5000;
                    cachedPromotePointRank = 100;

                    AchievementManager.UnlockAchievement(
                        "Bronze Cup");

                    GameManager.Instance.UnlockManager.Unlock(
                        UnlockManager.UnlockCondition
                            .WinBronzeLeague);
                }
                else if (playerWin == 50)
                {
                    winPrize = 2500;
                    cachedPromotePointRank = 25;
                }

                killPrize =
                    playerSurvivor.KillCount * 500;

                cachedPromotePointKill =
                    playerSurvivor.KillCount * 10;
                break;

            case League.SilverLeague:
                if (playerWin == 1)
                {
                    winPrize = 10000;
                    cachedPromotePointRank = 100;

                    AchievementManager.UnlockAchievement(
                        "Silver Cup");

                    GameManager.Instance.UnlockManager.Unlock(
                        UnlockManager.UnlockCondition
                            .WinSilverLeague);
                }
                else if (playerWin == 25)
                {
                    winPrize = 5000;
                    cachedPromotePointRank = 50;
                }
                else if (playerWin == 50)
                {
                    winPrize = 2500;
                    cachedPromotePointRank = 25;
                }

                killPrize =
                    playerSurvivor.KillCount * 1000;

                cachedPromotePointKill =
                    playerSurvivor.KillCount * 10;
                break;

            case League.GoldLeague:
                if (playerWin == 1)
                {
                    winPrize = 20000;
                    cachedPromotePointRank = 100;

                    AchievementManager.UnlockAchievement(
                        "Gold Cup");

                    GameManager.Instance.UnlockManager.Unlock(
                        UnlockManager.UnlockCondition
                            .WinGoldLeague);

                    if (outGameUIManager.GameMode ==
                        GameMode.FreeManagement)
                    {
                        survivorData
                            .haveQualifyToParticipateInSeasonChampionship =
                            true;
                    }
                }
                else if (playerWin == 25)
                {
                    winPrize = 10000;
                    cachedPromotePointRank = 50;
                }
                else if (playerWin == 50)
                {
                    winPrize = 5000;
                    cachedPromotePointRank = 25;
                }

                killPrize =
                    playerSurvivor.KillCount * 2000;

                cachedPromotePointKill =
                    playerSurvivor.KillCount * 10;
                break;

            case League.SeasonChampionship:
                if (playerWin == 1)
                {
                    winPrize = 10000;
                }
                else if (playerWin == 25)
                {
                    winPrize = 5000;
                }
                else if (playerWin == 50)
                {
                    winPrize = 2500;
                }

                killPrize =
                    playerSurvivor.KillCount * 1000;

                cachedPromotePointRank =
                    Mathf.Max(
                        10 -
                        GameManager.Instance
                            .BattleRoyaleManager
                            .playerSurvivorRank,
                        0);

                cachedPromotePointKill =
                    playerSurvivor.KillCount;

                if (outGameUIManager.GameMode ==
                    GameMode.FreeManagement)
                {
                    // 참가에 사용한 시즌 챔피언십 티켓 소모
                    survivorData
                        .haveQualifyToParticipateInSeasonChampionship =
                        false;
                }

                break;

            case League.WorldChampionship:
                if (playerWin == 1)
                {
                    winPrize = 20000;
                }
                else if (playerWin == 25)
                {
                    winPrize = 10000;
                }
                else if (playerWin == 50)
                {
                    winPrize = 5000;
                }

                killPrize =
                    playerSurvivor.KillCount * 2000;

                cachedPromotePointRank =
                    Mathf.Max(
                        10 -
                        GameManager.Instance
                            .BattleRoyaleManager
                            .playerSurvivorRank,
                        0);

                cachedPromotePointKill =
                    playerSurvivor.KillCount;

                if (outGameUIManager.GameMode ==
                    GameMode.FreeManagement)
                {
                    // 참가에 사용한 월드 챔피언십 티켓 소모
                    survivorData
                        .haveQualifyToParticipateInWorldChampionship =
                        false;
                }

                break;

            case League.MeleeLeague:
                if (playerWin == 1)
                {
                    winPrize = 40000;

                    AchievementManager.UnlockAchievement(
                        "Melee Champion");
                }
                else if (playerWin == 25)
                {
                    winPrize = 20000;
                }
                else if (playerWin == 50)
                {
                    winPrize = 10000;
                }

                killPrize =
                    playerSurvivor.KillCount * 4000;
                break;

            case League.RangeLeague:
                if (playerWin == 1)
                {
                    winPrize = 40000;

                    AchievementManager.UnlockAchievement(
                        "Shooting Champion");
                }
                else if (playerWin == 25)
                {
                    winPrize = 20000;
                }
                else if (playerWin == 50)
                {
                    winPrize = 10000;
                }

                killPrize =
                    playerSurvivor.KillCount * 4000;
                break;

            case League.CraftingLeague:
                if (playerWin == 1)
                {
                    winPrize = 40000;

                    AchievementManager.UnlockAchievement(
                        "Crafting Champion");
                }
                else if (playerWin == 25)
                {
                    winPrize = 20000;
                }
                else if (playerWin == 50)
                {
                    winPrize = 10000;
                }

                killPrize =
                    playerSurvivor.KillCount * 4000;
                break;

            default:
                Debug.LogWarning(
                    $"Unsupported league result: {league}");
                break;
        }

        if (playerWin != 1)
        {
            bool regularLeague =
                league == League.BronzeLeague ||
                league == League.SilverLeague ||
                league == League.GoldLeague;

            bool freeManagementChampionship =
                outGameUIManager.GameMode ==
                    GameMode.FreeManagement &&
                (league == League.SeasonChampionship ||
                 league == League.WorldChampionship);

            if (regularLeague ||
                freeManagementChampionship)
            {
                survivorData.royalLoader = false;
            }
        }

        if (survivorData.mostKillsInASingleMatch <
            playerSurvivor.KillCount)
        {
            survivorData.mostKillsInASingleMatch =
                playerSurvivor.KillCount;
        }
    }

    void CalculateTreatments(Survivor playerSurvivor)
    {
        totalTreatmentCost = 0;
        cachedTreatments.Clear();
        injuryNeedSurgery.Clear();

        foreach (Injury injury in playerSurvivor.injuries)
        {
            if (playerSurvivor.rememberAlreadyHaveInjury.TryGetValue(
                    injury.site,
                    out int alreadyHad))
            {
                if (injury.degree == 1)
                {
                    int cost =
                        outGameUIManager.MeasureTreatmentCost(
                            injury,
                            0);

                    AddTreatmentResult(
                        injury,
                        "Replace Prosthetic",
                        cost);

                    if (outGameUIManager.GameMode ==
                        GameMode.SingleCareerRun)
                    {
                        injuryNeedSurgery.Add(injury);
                    }
                }
                else if (injury.degree > 0)
                {
                    int cost =
                        outGameUIManager.MeasureTreatmentCost(
                            injury,
                            alreadyHad);

                    AddTreatmentResult(
                        injury,
                        "Prosthetic Repair",
                        cost);

                    switch (injury.type)
                    {
                        case InjuryType.ArtificialPartsDamaged:
                            injury.type =
                                InjuryType.ArtificialPartsTransplanted;
                            break;

                        case InjuryType.AugmentedPartsDamaged:
                            injury.type =
                                InjuryType.AugmentedPartsTransplanted;
                            break;

                        case InjuryType.TranscendantPartsDamaged:
                            injury.type =
                                InjuryType.TranscendantPartsTransplanted;
                            break;
                    }

                    injury.degree = 0;
                }
            }
            else
            {
                string treatmentKey;

                if (injury.degree == 1)
                {
                    treatmentKey = "ArtificialPartsTransplanted";

                    if (outGameUIManager.GameMode ==
                        GameMode.SingleCareerRun)
                    {
                        injuryNeedSurgery.Add(injury);
                    }
                }
                else
                {
                    treatmentKey = injury.type.ToString();
                }

                int cost =
                    outGameUIManager.MeasureTreatmentCost(
                        injury,
                        0);

                AddTreatmentResult(
                    injury,
                    treatmentKey,
                    cost);
            }
        }

        if (playerSurvivor.maxBlood > 0 &&
            playerSurvivor.curBlood /
                playerSurvivor.maxBlood < 0.8f)
        {
            int bloodTransfusionFee =
                (int)((playerSurvivor.maxBlood -
                       playerSurvivor.curBlood) * 0.1f);

            AddBloodTransfusionResult(
                bloodTransfusionFee);
        }
    }

    void AddTreatmentResult(
    Injury injury,
    string treatmentKey,
    int cost)
    {
        cachedTreatments.Add(new TreatmentResultData
        {
            injurySiteKey = injury.site.ToString(),
            treatmentKey = treatmentKey,
            cost = cost
        });

        totalTreatmentCost += cost;
    }

    void AddBloodTransfusionResult(int cost)
    {
        cachedTreatments.Add(new TreatmentResultData
        {
            cost = cost,
            isBloodTransfusion = true
        });

        totalTreatmentCost += cost;
    }

    void CalculateBettingResult()
    {
        cachedHasBetting =
            outGameUIManager.GameMode ==
                GameMode.FreeManagement &&
            outGameUIManager.BettingAmount > 0;

        cachedPredictionNumber =
            outGameUIManager.PredictionNumber;

        cachedBettingRewards = 0;
        cachedBettingOdds = 0;

        cachedPredictionKeys =
            new string[cachedPredictionNumber];

        cachedRankingKeys =
            new string[cachedPredictionNumber];

        cachedPredictionColors =
            new Color[cachedPredictionNumber];

        if (!cachedHasBetting)
            return;

        BattleRoyaleManager battleRoyaleManager =
            GameManager.Instance.BattleRoyaleManager;

        for (int i = 0;
             i < cachedPredictionNumber;
             i++)
        {
            LocalizedString prediction =
                outGameUIManager.Predictions[i];

            LocalizedString ranking =
                i < battleRoyaleManager.rankings.Length
                    ? battleRoyaleManager.rankings[i]
                    : null;

            cachedPredictionKeys[i] =
                prediction?.TableEntryReference.Key;

            cachedRankingKeys[i] =
                ranking?.TableEntryReference.Key;
        }

        int correctExactRanking = 0;
        int correctOnlyRankedIn = 0;

        for (int i = 0;
             i < cachedPredictionNumber;
             i++)
        {
            bool found = false;

            for (int j = 0;
                 j < cachedPredictionNumber;
                 j++)
            {
                if (string.IsNullOrEmpty(
                        cachedPredictionKeys[i]) ||
                    cachedPredictionKeys[i] !=
                        cachedRankingKeys[j])
                {
                    continue;
                }

                if (i == j)
                {
                    correctExactRanking++;

                    cachedPredictionColors[i] =
                        new Color(
                            0.48f,
                            1f,
                            0.44f);
                }
                else
                {
                    correctOnlyRankedIn++;

                    cachedPredictionColors[i] =
                        new Color(
                            0.89f,
                            0.93f,
                            0.39f);
                }

                found = true;
                break;
            }

            if (!found)
            {
                cachedPredictionColors[i] =
                    new Color(
                        0.88f,
                        0.43f,
                        0.43f);
            }
        }

        cachedBettingOdds =
            outGameUIManager.GetOdds(
                correctExactRanking,
                correctOnlyRankedIn);

        if (cachedBettingOdds >= 10)
        {
            AchievementManager.UnlockAchievement(
                "King of Betting");
        }

        if (cachedBettingOdds >= 100)
        {
            AchievementManager.UnlockAchievement(
                "God of Betting");
        }

        cachedBettingRewards =
            (long)(outGameUIManager.BettingAmount *
                   cachedBettingOdds);

        cachedBettingRewards =
            Math.Min(
                cachedBettingRewards,
                99999999);

        cachedTotalProfit +=
            (int)cachedBettingRewards -
            outGameUIManager.BettingAmount;
    }

    void RefreshBettingUI()
    {
        if (!cachedHasBetting)
        {
            bettingPrediction.SetActive(false);
            bettingRewardsText.text = $"{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : $ 0";
            return;
        }

        bettingPrediction.SetActive(true);

        for (int i = 0;
             i < predictionTable.Length;
             i++)
        {
            bool active =
                i < cachedPredictionNumber;

            predictionTable[i].SetActive(active);

            if (!active)
                continue;

            predictionsText[i]
                .GetComponent<LocalizeStringEvent>()
                .StringReference =
                    outGameUIManager.Predictions[i];

            if (string.IsNullOrEmpty(
                    cachedRankingKeys[i]))
            {
                rankingsText[i].text = "?";
            }
            else
            {
                rankingsText[i].text =
                    new LocalizedString(
                        "Name",
                        cachedRankingKeys[i]
                    ).GetLocalizedString();
            }

            predictionsBG[i].color =
                cachedPredictionColors[i];
        }

        bettingRewardsText.text =
            $"{new LocalizedString("Basic", "Bet Amount :").GetLocalizedString()} " + $"$ <color=red>- " + $"{outGameUIManager.BettingAmount}</color>\n" +
            $"{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : " + $"<color=green>$ " +
            $"{cachedBettingRewards}</color>\n" + $"($ {outGameUIManager.BettingAmount} x " + $"{cachedBettingOdds:0.##})";
    }

    void ApplyWinLoseStatistics()
    {
        if (playerWin == 1)
        {
            if (AchievementManager.GetStat(
                    "Total_Win",
                    out int totalWin))
            {
                AchievementManager.SetStat(
                    "Total_Win",
                    totalWin + 1);

                if (totalWin + 1 >= 10)
                {
                    AchievementManager.UnlockAchievement(
                        "Tactician");
                }
            }
        }
        else
        {
            if (AchievementManager.GetStat(
                    "Total_Lose",
                    out int totalLose))
            {
                AchievementManager.SetStat(
                    "Total_Lose",
                    totalLose + 1);

                if (totalLose + 1 >= 10)
                {
                    AchievementManager.UnlockAchievement(
                        "Experience");
                }
            }
        }
    }

    void RefreshResultUI()
    {
        if (cachedDidPlayerParticipate &&
            cachedPlayerSurvivor != null)
        {
            LocalizedString resultText = playerWin == 1
                ? new LocalizedString(
                    "Basic",
                    "Your survivor won!")
                : new LocalizedString(
                    "Basic",
                    "Your survivor was defeated.");

            resultText.Arguments = new object[]
            {
            outGameUIManager
                .MySurvivorDataInBattleRoyale
                .localizedSurvivorName
                .GetLocalizedString()
            };

            gameResultText.text = resultText.GetLocalizedString();

            survivedTimeText.text =
                $"{new LocalizedString("Basic", "Survival Time").GetLocalizedString()} : " +
                $"{(int)cachedPlayerSurvivor.SurvivedTime / 60:00m} " +
                $"{(int)cachedPlayerSurvivor.SurvivedTime % 60:00s}";

            killsText.text =
                $"{new LocalizedString("Basic", "Kill").GetLocalizedString()} : " +
                $"{cachedPlayerSurvivor.KillCount}";

            totalDamageText.text =
                $"{new LocalizedString("Basic", "Total damage dealt").GetLocalizedString()} : " +
                $"{(int)cachedPlayerSurvivor.TotalDamage}";

            increaseFightingText.text =
                $"{new LocalizedString("Basic", "Fighting").GetLocalizedString()} + " +
                $"{cachedPlayerSurvivor.IncreaseFighting}";

            increaseShootingText.text =
                $"{new LocalizedString("Basic", "Shooting").GetLocalizedString()} + " +
                $"{cachedPlayerSurvivor.IncreaseShooting}";

            increaseCraftingText.text =
                $"{new LocalizedString("Basic", "Crafting").GetLocalizedString()} + " +
                $"{cachedPlayerSurvivor.IncreaseCrafting}";

            winPrizeText.text =
                $"{new LocalizedString("Basic", "Rank Prize").GetLocalizedString()} : " +
                $"<color=green>$ {winPrize}</color>";

            killPrizeText.text =
                $"{new LocalizedString("Basic", "Kill reward").GetLocalizedString()} : " +
                $"<color=green>$ {killPrize}</color>";

            RefreshTreatmentUI();
        }
        else
        {
            BattleRoyaleManager manager =
                GameManager.Instance.BattleRoyaleManager;

            gameResultText.text = manager.BattleWinner != null
                ? new LocalizedString("Basic", "wins!")
                {
                    Arguments = new object[]
                    {
                    manager.BattleWinner
                        .survivorName
                        .GetLocalizedString()
                    }
                }.GetLocalizedString()
                : new LocalizedString(
                    "Basic",
                    "Result"
                ).GetLocalizedString();
        }

        RefreshBettingUI();

        if (cachedTotalProfit >= 0)
        {
            totalProfitText.text =
                $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : " +
                $"<color=green>$ {cachedTotalProfit}</color>";
        }
        else
        {
            totalProfitText.text =
                $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : " +
                $"<color=red>- $ {-cachedTotalProfit}</color>";
        }
    }

    void RefreshTreatmentUI()
    {
        for (int i = 0; i < treatments.Length; i++)
        {
            if (i >= cachedTreatments.Count)
            {
                treatments[i].SetActive(false);
                continue;
            }

            TreatmentResultData result = cachedTreatments[i];
            TextMeshProUGUI[] texts =
                treatments[i]
                    .GetComponentsInChildren<TextMeshProUGUI>(true);

            if (result.isBloodTransfusion)
            {
                texts[0].text =
                    new LocalizedString(
                        "Basic",
                        "Blood transfusion cost"
                    ).GetLocalizedString();
            }
            else
            {
                string site =
                    new LocalizedString(
                        "Injury",
                        result.injurySiteKey
                    ).GetLocalizedString();

                string treatment =
                    new LocalizedString(
                        "Injury",
                        result.treatmentKey
                    ).GetLocalizedString();

                texts[0].text = $"{site} {treatment}";
            }

            texts[1].text =
                $"<color=red>- $ {result.cost}</color>";

            treatments[i].SetActive(true);
        }

        totalTreatmentCostText.text =
            $"{new LocalizedString("Basic", "Total medical cost").GetLocalizedString()} : " +
            $"<color=red>- $ {totalTreatmentCost}</color>";
    }

    void Promote(SurvivorData survivor)
    {
        switch(calendar.LeagueReserveInfo[calendar.Today].league)
        {
            case League.BronzeLeague:
                survivor.tier = Tier.Silver;
                if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
                {
                    outGameUIManager.UpgradeFacility();
                    outGameUIManager.objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective2").GetLocalizedString()}";
                    notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                }
                break;
            case League.SilverLeague:
                survivor.tier = Tier.Gold;
                if (outGameUIManager.GameMode == GameMode.SingleCareerRun)
                {
                    outGameUIManager.UpgradeFacility();
                    outGameUIManager.objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective3").GetLocalizedString()}";
                    notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                }
                break;
            case League.GoldLeague:
                //calendar.NeareastSeasonChampionship.reserver = survivor;
                if (outGameUIManager.GameMode == GameMode.SingleCareerRun)
                {
                    survivor.haveQualifyToParticipateInSeasonChampionship = true;
                    notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "SeasonChampionship").GetLocalizedString()); };
                    outGameUIManager.UpgradeFacility();
                    notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                }
                else
                {
                    survivor.haveQualifyToParticipateInSeasonChampionship = true;
                    notification += () => { outGameUIManager.Alert("Alert:Obtain Season Championship Ticket", survivor.localizedSurvivorName.GetLocalizedString()); };
                }
                break;
            case League.SeasonChampionship:
                //calendar.NeareastWorldChampionship.reserver = survivor;
                int characteristic = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.ChokingUnderPressure);
                if (characteristic != -1)
                {
                    survivor.characteristics.RemoveAt(characteristic);
                    CharacteristicManager.AddCharaicteristic(survivor, CharacteristicType.ClutchPerformance, true);
                    notification += () => { outGameUIManager.Alert("Alert:Overcame", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Characteristic", "ClutchPerformance").GetLocalizedString(), new LocalizedString("Characteristic", "ChokingUnderPressure").GetLocalizedString()); };
                    AchievementManager.UnlockAchievement("Overcome");
                }
                if (outGameUIManager.GameMode == GameMode.SingleCareerRun) notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "WorldChampionship").GetLocalizedString()); };
                else
                {
                    survivor.haveQualifyToParticipateInWorldChampionship = true;
                    notification += () => { outGameUIManager.Alert("Alert:Obtain World Championship Ticket", survivor.localizedSurvivorName.GetLocalizedString()); };
                }
                break;
        }
    }

    public void DelayedShowGameResult()
    {
        resultClaimed = true;
    }

    void ExitBattleEvent()
    {
        outGameUIManager.Money += rememberTotalProfit;

        if (outGameUIManager.MySurvivorDataInBattleRoyale == null)
        {
            return;
        }

        Survivor playerSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[0];
        if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null && GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorID == 0) playerWin = 1;
        else
        {
            for (int i = 0; i < GameManager.Instance.BattleRoyaleManager.rankings.Length; i++)
            {
                float percentile = ((float)i + 1) / GameManager.Instance.BattleRoyaleManager.Survivors.Count;
                if (GameManager.Instance.BattleRoyaleManager.rankings[i] == playerSurvivor.survivorName)
                {
                    if (percentile <= 0.25f) playerWin = 25;
                    else if (percentile <= 0.5f) playerWin = 50;
                    else playerWin = -1;
                    break;
                }
            }
        }
        if (calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship) playerSurvivor.LinkedSurvivorData.haveQualifyToParticipateInSeasonChampionship = false;
        else if (calendar.LeagueReserveInfo[calendar.Today].league == League.WorldChampionship) playerSurvivor.LinkedSurvivorData.haveQualifyToParticipateInWorldChampionship = false;
        if (outGameUIManager.GameMode == GameMode.FreeManagement)
        {
            if(playerWin == 1) Promote(playerSurvivor.LinkedSurvivorData);
        }
        else
        {
            Debug.Log($"Promote Point : {rememberPromotePoint_Rank + rememberPromotePoint_Kill}");
            if(!outGameUIManager.Championship)
            {
                playerSurvivor.LinkedSurvivorData.increaseComparedToPrevious_promotePoint += rememberPromotePoint_Rank + rememberPromotePoint_Kill;
                playerSurvivor.LinkedSurvivorData.promotePoint_Rank = rememberPromotePoint_Rank;
                playerSurvivor.LinkedSurvivorData.promotePoint_Kill = rememberPromotePoint_Kill;
            }
            if (playerSurvivor.LinkedSurvivorData.promotePoint + playerSurvivor.LinkedSurvivorData.increaseComparedToPrevious_promotePoint >= 100) Promote(playerSurvivor.LinkedSurvivorData);
            notification += () =>
            {
                switch (calendar.LeagueReserveInfo[calendar.Today].league)
                {
                    case League.BronzeLeague:
                    case League.SilverLeague:
                    case League.GoldLeague:
                    case League.SeasonChampionship:
                    case League.WorldChampionship:
                        string gainStats = "";
                        if (playerWin == 1)
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(2, 2, 2, 2, 2, 2);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Strength").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Agility").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Fighting").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Shooting").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Crafting").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Knowledge").GetLocalizedString()} + 2";
                        }
                        else
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(1, 1, 1, 1, 1, 1);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Strength").GetLocalizedString()} + 1, {new LocalizedString("Basic", "Agility").GetLocalizedString()} + 1, {new LocalizedString("Basic", "Fighting").GetLocalizedString()} + 1, {new LocalizedString("Basic", "Shooting").GetLocalizedString()} + 1, {new LocalizedString("Basic", "Crafting").GetLocalizedString()} + 1, {new LocalizedString("Basic", "Knowledge").GetLocalizedString()} + 1";
                        }
                        outGameUIManager.ResetSelectedSurvivorInfo();
                        outGameUIManager.Alert("Alert:Gain stat from match", gainStats);
                        break;
                    case League.MeleeLeague:
                        if (playerWin == 1)
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(4, 4, 4, 0, 0, 0);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Strength").GetLocalizedString()} + 4, {new LocalizedString("Basic", "Agility").GetLocalizedString()} + 4, {new LocalizedString("Basic", "Fighting").GetLocalizedString()} + 4";
                        }
                        else
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(2, 2, 2, 0, 0, 0);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Strength").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Agility").GetLocalizedString()} + 2, {new LocalizedString("Basic", "Fighting").GetLocalizedString()} + 2";
                        }
                        outGameUIManager.ResetSelectedSurvivorInfo();
                        outGameUIManager.Alert("Alert:Gain stat from match", gainStats);
                        break;
                    case League.RangeLeague:
                        if (playerWin == 1)
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(0, 0, 0, 12, 0, 0);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Shooting").GetLocalizedString()} + 12";
                        }
                        else
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(0, 0, 0, 6, 0, 0);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Shooting").GetLocalizedString()} + 6";
                        }
                        outGameUIManager.ResetSelectedSurvivorInfo();
                        outGameUIManager.Alert("Alert:Gain stat from match", gainStats);
                        break;
                    case League.CraftingLeague:
                        if (playerWin == 1)
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(0, 0, 0, 0, 6, 6);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Crafting").GetLocalizedString()} + 6, {new LocalizedString("Basic", "Knowledge").GetLocalizedString()} + 6";
                        }
                        else
                        {
                            playerSurvivor.LinkedSurvivorData.IncreaseStatsReserve(0, 0, 0, 0, 3, 3);
                            gainStats = $"\n\n{new LocalizedString("Basic", "Crafting").GetLocalizedString()} + 3, {new LocalizedString("Basic", "Knowledge").GetLocalizedString()} + 3";
                        }
                        outGameUIManager.ResetSelectedSurvivorInfo();
                        outGameUIManager.Alert("Alert:Gain stat from match", gainStats);
                        break;
                }
                switch(calendar.LeagueReserveInfo[calendar.Today].league)
                {
                    case League.BronzeLeague:
                    case League.SilverLeague:
                    case League.GoldLeague:
                        outGameUIManager.PromoteAnimation(calendar.LeagueReserveInfo[calendar.Today].league);
                        break;
                }
            };
        }
        if (outGameUIManager.MySurvivorDataInBattleRoyale != null) LinkStastics(outGameUIManager.MySurvivorDataInBattleRoyale);
    }

    public void ExitBattle(bool goTitle = false)
    {
        gameResult.SetActive(false);

        if (!resultCalculated)
        {
            cachedDidPlayerParticipate = outGameUIManager.MySurvivorDataInBattleRoyale != null;

            CalculateResult(cachedDidPlayerParticipate);
            resultCalculated = true;
        }

        ExitBattleEvent();
        ClearBattleRoyale();
        if (!goTitle)
        {
            if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
            {
                // 여기서 수술
                foreach (var injury in injuryNeedSurgery)
                {
                    injury.type = InjuryType.ArtificialPartsTransplanted;
                    injury.degree = 0;
                }

                RecordChampionshipProgress();
                ContestantsMaintain();
            }

            if(gameOver)
            {
                GameOver();
            }
            else
            {
                
                // Auto save
                //GameManager.Instance.Save(0);
                //GameManager.Instance.Option.SetSaveButtonInteractable(true);

                GameManager.Instance.inGameUICanvas.SetActive(false);
                GameManager.Instance.outCanvas.SetActive(true);
                GameManager.Instance.globalCanvas.SetActive(true);

                notification?.Invoke();
                notification = null;
                GameManager.Instance.OutGameUIManager.EndTheDayWeekend();
                GameManager.Instance.OutGameUIManager.ResetSelectedSurvivorInfo();
            }
        }
        else
        {
            gameResult.SetActive(false);
        }
        GameManager.Instance.DestroyBattleRoyaleManager();
    }

    void RecordChampionshipProgress()
    {
        if (!outGameUIManager.Championship) return;
        foreach(var survivor in GameManager.Instance.BattleRoyaleManager.Survivors)
        {
            OutGameUIManager.ChampionshipData cSurvivor = outGameUIManager.championshipDatas.Find(x => x.SurvivorName.TableEntryReference.Key == survivor.LinkedSurvivorData.localizedSurvivorName.TableEntryReference.Key);
            int rank = 0;
            for(int i = 0; i < 25; i++)
            {
                if (GameManager.Instance.BattleRoyaleManager.rankings[i].TableEntryReference.Key == cSurvivor.SurvivorName.TableEntryReference.Key)
                {
                    rank = i;
                    break;
                }
            }
            cSurvivor.points.Add(Mathf.Max(0, 10 - rank) + survivor.KillCount);
            cSurvivor.killPoints.Add(survivor.KillCount);
        }
        SortChampionshipRanking();
        outGameUIManager.championshipHeldCount++;
        outGameUIManager.OpenChampionshipProgress();
        if(outGameUIManager.championshipHeldCount >= 3)
        {
            int playerSurvivorRank = outGameUIManager.championshipDatas.Find(x => x.SurvivorName.TableEntryReference.Key == outGameUIManager.MySurvivorsData[0].localizedSurvivorName.TableEntryReference.Key).currentRank;
            if (calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship)
            {
                // 시챔 끝
                if(playerSurvivorRank < 5)
                {
                    GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinSeasonChampionship);
                    if (playerSurvivorRank == 0)
                    {
                        AchievementManager.UnlockAchievement("Season Champion");
                        outGameUIManager.MySurvivorsData[0].wonSeasonChampionship = true;
                        GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinSeasonChampionship);
                        Promote(outGameUIManager.MySurvivorsData[0]);
                    }
                    // 상위 5인 월챔 진출
                    for (int i = 5; i<25; i++)
                    {
                        // 6위부터 제거
                        outGameUIManager.contestantsData.Remove(outGameUIManager.contestantsData.Find(x => x.localizedSurvivorName.TableEntryReference.Key == outGameUIManager.championshipDatas[i].SurvivorName.TableEntryReference.Key));
                    }
                    // 챔피언쉽 데이터 초기화
                    outGameUIManager.championshipDatas.Clear();
                    outGameUIManager.championshipHeldCount = 0;
                    foreach (var survivor in outGameUIManager.contestantsData) outGameUIManager.championshipDatas.Add(new(survivor));
                }
                else
                {
                    gameOver = true;
                    gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
                }
            }
            else
            {
                // 월챔 끝
                gameOver = true;
                if(playerSurvivorRank == 0)
                {
                    winWC = true;
                    outGameUIManager.MySurvivorsData[0].wonWorldChampionship = true;
                    switch (outGameUIManager.Difficulty)
                    {
                        case 1:
                            AchievementManager.UnlockAchievement("Hard");
                            break;
                        case 2:
                            AchievementManager.UnlockAchievement("Very Hard");
                            break;
                        case 3:
                            AchievementManager.UnlockAchievement("Expert");
                            break;
                        case 4:
                            AchievementManager.UnlockAchievement("Hardcore");
                            break;
                        case 5:
                            AchievementManager.UnlockAchievement("Nightmare");
                            break;
                        case 6:
                            AchievementManager.UnlockAchievement("Hell");
                            break;
                        default:
                            AchievementManager.UnlockAchievement("World Champion");
                            GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinWorldChampionship);
                            break;
                    }
                    if (outGameUIManager.MySurvivorsData[0].royalLoader) AchievementManager.UnlockAchievement("Royal Loader");
                    gameOverMessage.StringReference = new("Basic", "GameOver:Win World Champion");
                }
                else
                {
                    gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
                }
            }
        }
    }

    void SortChampionshipRanking()
    {
        foreach (var survivor in outGameUIManager.championshipDatas) survivor.beforeRank = survivor.currentRank;
        List<OutGameUIManager.ChampionshipData> sortedChampionshipDatas = outGameUIManager.championshipDatas
            .OrderByDescending(x => x.TotalPoint).ThenByDescending(x => x.TotalKillPoint).ThenByDescending(x => x.points[^1]).ThenByDescending(x => x.killPoints[^1]).ToList();
        outGameUIManager.championshipDatas = sortedChampionshipDatas;
        for (int i=0; i<25; i++) outGameUIManager.championshipDatas[i].currentRank = i;
    }

    void ContestantsMaintain()
    {
        if (!outGameUIManager.Championship || outGameUIManager.championshipDatas.Count < 25) return;

        // 상대들도 수술해주고, 실전경험을 통한 능력치 상승
        float chanceTranscendant = outGameUIManager.Difficulty switch
        {
            6 => 0.5f,
            5 => 0.25f,
            _ => 0f,
        };
        float chanceAugment = outGameUIManager.Difficulty switch
        {
            0 => 0f,
            1 => 0.25f,
            2 => 0.5f,
            3 => 0.75f,
            _ => 1f,
        };

        int artificial;
        for(int i=1; i<outGameUIManager.contestantsData.Count; i++)
        {
            var survivor = outGameUIManager.contestantsData[i];
            List<Injury> rememberRemove = new();
            foreach (var injury in survivor.injuries)
            {
                if(injury.degree == 1)
                {
                    float rand = UnityEngine.Random.Range(0, 1f);
                    if (rand < chanceTranscendant) artificial = 3;
                    else if (rand < chanceTranscendant + chanceAugment) artificial = 2;
                    else artificial = 1;
                    injury.type = artificial == 3 ? InjuryType.TranscendantPartsTransplanted : artificial == 2 ? InjuryType.AugmentedPartsTransplanted : InjuryType.ArtificialPartsTransplanted;
                    injury.degree = 0;
                }
                else
                {
                    switch(injury.type)
                    {
                        case InjuryType.TranscendantPartsTransplanted:
                        case InjuryType.TranscendantPartsDamaged:
                            artificial = 3;
                            break;
                        case InjuryType.AugmentedPartsTransplanted:
                        case InjuryType.AugmentedPartsDamaged:
                            artificial = 2;
                            break;
                        case InjuryType.ArtificialPartsTransplanted:
                        case InjuryType.ArtificialPartsDamaged:
                            artificial = 1;
                            break;
                        default:
                            artificial = 0;
                            break;
                    }
                    if(artificial == 0)
                    {
                        rememberRemove.Add(injury);
                    }
                    else
                    {
                        injury.type = artificial == 3 ? InjuryType.TranscendantPartsTransplanted : artificial == 2 ? InjuryType.AugmentedPartsTransplanted : InjuryType.ArtificialPartsTransplanted;
                        injury.degree = 0;
                    }
                }
            }
            foreach (var remove in rememberRemove) survivor.injuries.Remove(remove);

            var championshipInfo = outGameUIManager.championshipDatas.Find(x => x.SurvivorName.TableEntryReference.Key == survivor.localizedSurvivorName.TableEntryReference.Key);
            if (championshipInfo.points[^1] - championshipInfo.killPoints[^1] == 10)
            {
                survivor.IncreaseStats(2, 2, 2, 2, 2, 2);
            }
            else
            {
                survivor.IncreaseStats(1, 1, 1, 1, 1, 1);
            }
        }
    }

    public void ClearBattleRoyale()
    {
        if (GameManager.Instance.BattleRoyaleManager == null) return;
        AudioSource bgsfx = GameManager.Instance.BattleRoyaleManager.bgsfx;
        SoundManager.StopSFX(bgsfx);
        bgsfx.minDistance = 1;
        bgsfx.maxDistance = 500;
        GameManager.Instance.GetComponent<InGameUIManager>().SetTimeScale(1);
        SoundManager.Play(ResourceEnum.BGM.the_birth_of_hip_hop);
        foreach (Survivor survivor in GameManager.Instance.BattleRoyaleManager.Survivors)
        {
            foreach (GameObject blood in survivor.bloods) PoolManager.Despawn(blood);
            foreach (GameObject buried in survivor.burieds) PoolManager.Despawn(buried);
        }
    }

    public void GameOver()
    {
        gameOverCanvas.SetActive(true);
        // 생존자 통계 보여주기
        gameOverSurvivorInfo.SetInfo(outGameUIManager.MySurvivorsData[0], false);

        viewChampionshipProgress.SetActive(outGameUIManager.Championship);
        
        SetEarnedAchievements();
        if (winWC)
        {
            SoundManager.PlayUISFX(ResourceEnum.SFX.Fanfare2);
            SoundManager.PlayUISFX(ResourceEnum.SFX.Cheers);
        }
        GameManager.Instance.Option.DeleteSaveData(0);
    }

    void LinkStastics(SurvivorData survivor)
    {
        var league = calendar.LeagueReserveInfo[calendar.Today].league;
        bool goldPlus = league != League.BronzeLeague && league != League.SilverLeague;
        if(playerWin == 1)
        {
            survivor.winCount++;
            if(goldPlus) survivor.winCountGoldPlus++;
            switch(league)
            {
                case League.BronzeLeague:
                    survivor.wonBronzeLeague = true;
                    break;
                case League.SilverLeague:
                    survivor.wonSilverLeague = true;
                    break;
                case League.GoldLeague:
                    survivor.wonGoldLeague = true;
                    break;
                case League.SeasonChampionship:
                    if (outGameUIManager.GameMode == GameMode.FreeManagement) survivor.wonSeasonChampionship = true;
                    break;
                case League.WorldChampionship:
                    if (outGameUIManager.GameMode == GameMode.FreeManagement) survivor.wonWorldChampionship = true;
                    break;
                case League.MeleeLeague:
                    survivor.wonMeleeLeague = true;
                    break;
                case League.RangeLeague:
                    survivor.wonRangedLeague = true;
                    break;
                case League.CraftingLeague:
                    survivor.wonCraftingLeague = true;
                    break;
            }
            if (survivor.wonBronzeLeague && survivor.wonSilverLeague && survivor.wonGoldLeague && survivor.wonSeasonChampionship
                && survivor.wonWorldChampionship && survivor.wonMeleeLeague && survivor.wonRangedLeague && survivor.wonCraftingLeague)
                AchievementManager.UnlockAchievement("Legend");
        }
        else if(playerWin > 1)
        {
            survivor.rankDefenseCount++;
            if (goldPlus) survivor.rankDefenseCountGoldPlus++;
        }
        else
        {
            survivor.loseCount++;
            if (goldPlus) survivor.loseCountGoldPlus++;
        }
        Survivor pawn = GameManager.Instance.BattleRoyaleManager.Survivors[0];
        survivor.totalKill += pawn.KillCount;
        if (survivor.totalKill >= 30) AchievementManager.UnlockAchievement("Notorious");
        if (playerWin == 1 && pawn.KillCount == 0) AchievementManager.UnlockAchievement("Vulture Victory");
        survivor.totalSurvivedTime += pawn.SurvivedTime;
        PlayerPrefs.SetFloat("Total Survival Time", PlayerPrefs.GetFloat("Total Survival Time") + pawn.SurvivedTime);
        AchievementManager.SetStat("Total_SurvivalTime", PlayerPrefs.GetFloat("Total Survival Time"));
        if (PlayerPrefs.GetFloat("Total Survival Time") >= 3600) AchievementManager.UnlockAchievement("1 hour");
        survivor.totalGiveDamage += pawn.TotalDamage;
        survivor.totalTakeDamage += pawn.MaxHP - pawn.CurHP;
        survivor.totalRankPrize += winPrize;
        survivor.totalKillPrize += killPrize;
        survivor.totalTreatmentFee += totalTreatmentCost;
    }

    public void KeepWatching()
    {
        gameResult.SetActive(false);
        Time.timeScale = lastTimeScale;
    }

    void SetEarnedAchievements()
    {
        if(AchievementManager.earnedAchievementsInThisRun.Count == 0)
        {
            earnedAchievementsBox.SetActive(false);
        }
        else
        {
            earnedAchievementsBox.SetActive(true);
            earnAchievementsCurrentPage = 0;
            SetEarnedAchievements(0);
        }
    }

    void SetEarnedAchievements(int index)
    {
        earnedAchievementsPrevious.interactable = index > 0;
        earnedAchievementsNext.interactable = index < AchievementManager.earnedAchievementsInThisRun.Count - 1;
        earnedAchievemetName.StringReference = new("Achievement", AchievementManager.earnedAchievementsInThisRun[index]);
        string key = AchievementManager.earnedAchievementsInThisRun[index];
        AchievementUIManager.AchievementInfo achievement = AchievementUIManager.AchievementInfos.Find(x => x.achievementKey == key);
        key = key.Replace(" ", "").Replace("-", "");
        if (char.IsDigit(key[0])) key = "_" + key;
        if (Enum.TryParse(key, out ResourceEnum.Sprite spriteE)) earnedAchievementImage.sprite = ResourceManager.Get(spriteE);
        else earnedAchievementImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
        if (achievement != null && !achievement.unlockElementName.Equals(""))
        {
            string unlockElement = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Basic", "Characteristic").GetLocalizedString() : new LocalizedString("Basic", "Training").GetLocalizedString();
            string unlockElementDetail = achievement.unlockElement == AchievementUIManager.UnlockElement.Characteristic ? new LocalizedString("Characteristic", achievement.unlockElementName).GetLocalizedString() : new LocalizedString("Training", achievement.unlockElementName).GetLocalizedString();
            earnedAchievemetUnlockElementText.text = $"{new LocalizedString("Basic", "Unlock").GetLocalizedString()} : {unlockElement} - {unlockElementDetail}";
        }
        else
        {
            earnedAchievemetUnlockElementText.text = "";
        }
    }

    public void TurnPageEarnedAchievements(int value)
    {
        if (AchievementManager.earnedAchievementsInThisRun.Count == 0) return;
        earnAchievementsCurrentPage = Mathf.Clamp(earnAchievementsCurrentPage + value, 0, AchievementManager.earnedAchievementsInThisRun.Count - 1);
        SetEarnedAchievements(earnAchievementsCurrentPage);
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (!resultCalculated || !gameResult.activeSelf) return;
        RefreshResultUI();

        GameManager.Instance.FixLayout(gameResult.GetComponent<RectTransform>());
    }

}

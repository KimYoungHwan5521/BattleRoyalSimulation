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

        bool didPlayerParticipate = outGameUIManager.MySurvivorDataInBattleRoyale != null;
        mySurvivorResult.SetActive(didPlayerParticipate);
        mySurvivorTreatmentCost.SetActive(didPlayerParticipate);
        GameManager.Instance.Option.SetSaveButtonInteractable(true, true);
        SetText(didPlayerParticipate, out int totalProfit, out int promotePoint_Rank, out int promotePoint_Kill);
        rememberTotalProfit = totalProfit;
        rememberPromotePoint_Rank = promotePoint_Rank;
        rememberPromotePoint_Kill = promotePoint_Kill;
        //gameOver = outGameUIManager.Money < 0;
        //if (outGameUIManager.Money < 0) gameOverMessage.StringReference = new("Basic", "GameOver:Can't Pay Fee");
        GameManager.Instance.FixLayout(gameResult.GetComponent<RectTransform>());
    }

    // playerWin | 1: win, 25: top 25%, 50: top 50%, -1 : bottom 50%
    int playerWin;
    int winPrize = 0;
    int killPrize = 0;
    int totalTreatmentCost = 0;
    List<Injury> injuryNeedSurgery = new();
    void SetText(bool didPlayerParticipate, out int totalProfit, out int promotePoint_Rank, out int promotePoint_Kill)
    {
        totalProfit = 0;
        promotePoint_Rank = 0;
        promotePoint_Kill = 0;
        playerWin = -1;
        if (didPlayerParticipate)
        {
            GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.FirstParticipateInBattleRoyale);
            Survivor playerSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[0];
            if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null && GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorID == 0) playerWin = 1;
            else
            {
                for(int i=0; i<GameManager.Instance.BattleRoyaleManager.rankings.Length; i++)
                {
                    float percentile = ((float)i + 1) / outGameUIManager.contestantsData.Count;
                    if (GameManager.Instance.BattleRoyaleManager.rankings[i] == playerSurvivor.survivorName)
                    {
                        if (percentile <= 0.25f) playerWin = 25;
                        else if (percentile <= 0.5f) playerWin = 50;
                        else playerWin = -1;
                        break;
                    }
                }
            }
            LocalizedString resultText = playerWin == 1 ? new LocalizedString("Basic", "Your survivor won!") : new LocalizedString("Basic", "Your survivor was defeated.");
            if(playerWin == 1)
            {
                if (AchievementManager.GetStat("Total_Win", out int totalWin))
                {
                    AchievementManager.SetStat("Total_Win", totalWin + 1);
                    if (totalWin + 1 >= 10) AchievementManager.UnlockAchievement("Tactician");
                }
            }
            else
            {
                if (AchievementManager.GetStat("Total_Lose", out int totalLose))
                {
                    AchievementManager.SetStat("Total_Lose", totalLose + 1);
                    if (totalLose + 1 >= 10) AchievementManager.UnlockAchievement("Experience");
                }
            }
            resultText.Arguments = new[] { outGameUIManager.MySurvivorDataInBattleRoyale.localizedSurvivorName.GetLocalizedString() };
            gameResultText.text = resultText.GetLocalizedString();
            survivedTimeText.text = $"{new LocalizedString("Basic", "Survival Time").GetLocalizedString()} : {(int)playerSurvivor.SurvivedTime / 60:00m} {(int)playerSurvivor.SurvivedTime - ((int)playerSurvivor.SurvivedTime / 60) * 60:00s}";
            killsText.text = $"{new LocalizedString("Basic", "Kill").GetLocalizedString()} : {playerSurvivor.KillCount}";
            totalDamageText.text = $"{new LocalizedString("Basic", "Total damage dealt").GetLocalizedString()} : {(int)playerSurvivor.TotalDamage}";
            increaseFightingText.text = $"{new LocalizedString("Basic", "Fighting").GetLocalizedString()} + {playerSurvivor.IncreaseFighting}";
            increaseShootingText.text = $"{new LocalizedString("Basic", "Shooting").GetLocalizedString()} + {playerSurvivor.IncreaseShooting}";
            increaseCraftingText.text = $"{new LocalizedString("Basic", "Crafting").GetLocalizedString()} + {playerSurvivor.IncreaseCrafting}";

            winPrize = 0;
            killPrize = 0;
            totalTreatmentCost = 0;
            switch (calendar.LeagueReserveInfo[calendar.Today].league)
            {
                case League.BronzeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 5000;
                        AchievementManager.UnlockAchievement("Bronze Cup");
                        GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinBronzeLeague);
                        promotePoint_Rank = 100;
                    }
                    else if (playerWin == 50)
                    {
                        winPrize = 2500;
                        promotePoint_Rank = 25;
                    }
                    killPrize = playerSurvivor.KillCount * 500;
                    promotePoint_Kill = playerSurvivor.KillCount * 10;
                    break;
                case League.SilverLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 10000;
                        AchievementManager.UnlockAchievement("Silver Cup");
                        GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinSilverLeague);
                        promotePoint_Rank = 100;
                    }
                    else if (playerWin == 25)
                    {
                        winPrize = 5000;
                        promotePoint_Rank = 50;
                    }
                    else if (playerWin == 50)
                    {
                        winPrize = 2500;
                        promotePoint_Rank = 25;
                    }
                    killPrize = playerSurvivor.KillCount * 1000;
                    promotePoint_Kill = playerSurvivor.KillCount * 10;
                    break;
                case League.GoldLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 20000;
                        AchievementManager.UnlockAchievement("Gold Cup");
                        GameManager.Instance.UnlockManager.Unlock(UnlockManager.UnlockCondition.WinGoldLeague);
                        promotePoint_Rank = 100;
                    }
                    else if (playerWin == 25)
                    {
                        winPrize = 10000;
                        promotePoint_Rank = 50;
                    }
                    else if (playerWin == 50)
                    {
                        winPrize = 5000;
                        promotePoint_Rank = 25;
                    }
                    killPrize = playerSurvivor.KillCount * 2000;
                    promotePoint_Kill = playerSurvivor.KillCount * 10;
                    break;
                case League.SeasonChampionship:
                    if (playerWin == 1)
                    {
                        winPrize = 10000;
                    }
                    else if (playerWin == 25) winPrize = 5000;
                    else if (playerWin == 50) winPrize = 2500;
                    killPrize = playerSurvivor.KillCount * 1000;
                    int rank = GameManager.Instance.BattleRoyaleManager.playerSurvivorRank;
                    promotePoint_Rank = Mathf.Max((10 - rank), 0);
                    promotePoint_Kill = playerSurvivor.KillCount;
                    //playerSurvivor.LinkedSurvivorData.haveQualifyToParticipateInSeasonChampionship = false;
                    break;
                case League.WorldChampionship:
                    if (playerWin == 1)
                    {
                        winPrize = 20000;
                    }
                    else if (playerWin == 25) winPrize = 10000;
                    else if (playerWin == 50) winPrize = 5000;
                    killPrize = playerSurvivor.KillCount * 2000;
                    //gameOver = true;
                    //gameOverMessage.StringReference = new("Basic", playerWin == 1 ? "GameOver:Win World Champion" : "GameOver:Lose");
                    rank = GameManager.Instance.BattleRoyaleManager.playerSurvivorRank;
                    promotePoint_Rank = Mathf.Max((10 - rank), 0);
                    promotePoint_Kill = playerSurvivor.KillCount;
                    //playerSurvivor.LinkedSurvivorData.haveQualifyToParticipateInWorldChampionship = false;
                    break;
                case League.MeleeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 40000;
                        AchievementManager.UnlockAchievement("Melee Champion");
                    }
                    else if (playerWin == 25) winPrize = 20000;
                    else if (playerWin == 50) winPrize = 10000;
                    killPrize = playerSurvivor.KillCount * 4000;
                    break;
                case League.RangeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 40000;
                        AchievementManager.UnlockAchievement("Shooting Champion");
                    }
                    else if (playerWin == 25) winPrize = 20000;
                    else if (playerWin == 50) winPrize = 10000;
                    killPrize = playerSurvivor.KillCount * 4000;
                    break;
                case League.CraftingLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 40000;
                        AchievementManager.UnlockAchievement("Crafting Champion");
                    }
                    else if (playerWin == 25) winPrize = 20000;
                    else if (playerWin == 50) winPrize = 10000;
                    killPrize = playerSurvivor.KillCount * 4000;
                    break;
            }
            if(playerWin != 1 && (calendar.LeagueReserveInfo[calendar.Today].league == League.BronzeLeague || calendar.LeagueReserveInfo[calendar.Today].league == League.SilverLeague || calendar.LeagueReserveInfo[calendar.Today].league == League.GoldLeague))
            {
                playerSurvivor.LinkedSurvivorData.royalLoader = false;
            }
            //switch(calendar.LeagueReserveInfo[calendar.Today].league)
            //{
            //    case League.BronzeLeague:
            //        if (playerWin != 1)
            //        {
            //            playerSurvivor.LinkedSurvivorData.royalLoader = false;
            //            if (calendar.Today == 24)
            //            {
            //                gameOver = true;
            //                gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
            //            }
            //        }
            //        break;
            //    case League.SilverLeague:
            //        if (playerWin != 1)
            //        {
            //            playerSurvivor.LinkedSurvivorData.royalLoader = false;
            //            if (calendar.Today == 53)
            //            {
            //                gameOver = true;
            //                gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
            //            }
            //        }
            //        break;
            //    case League.GoldLeague:
            //        if (playerWin != 1)
            //        {
            //            playerSurvivor.LinkedSurvivorData.royalLoader = false;
            //            if (calendar.Today > 77 && calendar.NeareastSeasonChampionship.reserver == null && calendar.NeareastWorldChampionship.reserver == null)
            //            {
            //                gameOver = true;
            //                gameOverMessage.StringReference = new("Basic", "GameOver:Failed to achieve the objective.");
            //            }
            //        }
            //        break;
            //    case League.SeasonChampionship:
            //        if (playerWin != 1)
            //        {
            //            playerSurvivor.LinkedSurvivorData.royalLoader = false;
            //            if (calendar.Today > 77)
            //            {
            //                gameOver = true;
            //                gameOverMessage.StringReference = new("Basic", "GameOver:Lose");
            //            }
            //        }
            //        break;
            //}
            winPrizeText.text = $"{new LocalizedString("Basic", "Rank Prize").GetLocalizedString()} : <color=green>$ {winPrize}</color>";
            killPrizeText.text = $"{new LocalizedString("Basic", "Kill reward").GetLocalizedString()} : <color=green>$ {killPrize}</color>";
            if (playerSurvivor.LinkedSurvivorData.mostKillsInASingleMatch < playerSurvivor.KillCount) playerSurvivor.LinkedSurvivorData.mostKillsInASingleMatch = playerSurvivor.KillCount;
            
            injuryNeedSurgery = new();
            for (int i = 0; i < treatments.Length; i++)
            {
                if (i < playerSurvivor.injuries.Count + 1)
                {
                    if (i < playerSurvivor.injuries.Count)
                    {
                        if (playerSurvivor.rememberAlreadyHaveInjury.ContainsKey(playerSurvivor.injuries[i].site))
                        {
                            if(playerSurvivor.injuries[i].degree == 1)
                            {
                                treatments[i].SetActive(true);
                                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{new LocalizedString("Injury", playerSurvivor.injuries[i].site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", "Replace Prosthetic").GetLocalizedString()}";
                                int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i], 0);
                                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                                totalTreatmentCost += cost;

                                injuryNeedSurgery.Add(playerSurvivor.injuries[i]);
                            }
                            else if (playerSurvivor.injuries[i].degree > 0)
                            {
                                treatments[i].SetActive(true);
                                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{new LocalizedString("Injury", playerSurvivor.injuries[i].site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", "Prosthetic Repair").GetLocalizedString()}";
                                int alreadyHad = playerSurvivor.rememberAlreadyHaveInjury[playerSurvivor.injuries[i].site];
                                int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i], alreadyHad);
                                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                                totalTreatmentCost += cost;

                                if (playerSurvivor.injuries[i].type == InjuryType.ArtificialPartsDamaged) playerSurvivor.injuries[i].type = InjuryType.ArtificialPartsTransplanted;
                                else if (playerSurvivor.injuries[i].type == InjuryType.AugmentedPartsDamaged) playerSurvivor.injuries[i].type = InjuryType.AugmentedPartsTransplanted;
                                else if (playerSurvivor.injuries[i].type == InjuryType.TranscendantPartsDamaged) playerSurvivor.injuries[i].type = InjuryType.TranscendantPartsTransplanted;
                                playerSurvivor.injuries[i].degree = 0;
                            }
                            else
                            {
                                treatments[i].SetActive(false);
                                continue;
                            }
                        }
                        else
                        {
                            treatments[i].SetActive(true);
                            if (playerSurvivor.injuries[i].degree == 1)
                            {
                                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{new LocalizedString("Injury", playerSurvivor.injuries[i].site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", "ArtificialPartsTransplanted").GetLocalizedString()}";
                                injuryNeedSurgery.Add(playerSurvivor.injuries[i]);
                            }
                            else treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{new LocalizedString("Injury", playerSurvivor.injuries[i].site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", playerSurvivor.injuries[i].type.ToString()).GetLocalizedString()}";
                            int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i], 0);
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                            //treatments[i].GetComponentInChildren<Help>().SetDescription("");
                            totalTreatmentCost += cost;
                        }
                    }
                    else
                    {
                        if (playerSurvivor.curBlood / playerSurvivor.maxBlood < 0.8f)
                        {
                            // 수혈비
                            int bloodTransfusionFee = (int)((playerSurvivor.maxBlood - playerSurvivor.curBlood) * 0.1f);
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = new LocalizedString("Basic", "Blood transfusion cost").GetLocalizedString();
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {bloodTransfusionFee}</color>";
                            //treatments[i].GetComponentInChildren<Help>().SetDescription(new LocalizedString("Basic", "Help:Blood transfusion cost").GetLocalizedString());
                            treatments[i].SetActive(true);
                        }
                        else treatments[i].SetActive(false);
                    }
                }
                else treatments[i].SetActive(false);
            }
            totalTreatmentCostText.text = $"{new LocalizedString("Basic", "Total medical cost").GetLocalizedString()} : <color=red>- $ {totalTreatmentCost}</color>";
            totalProfit = winPrize + killPrize - totalTreatmentCost;

        }
        else
        {
            if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null) gameResultText.text = $"{new LocalizedString("Basic", "wins!") { Arguments = new[] { GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorName.GetLocalizedString() } }.GetLocalizedString()}";
            else gameResultText.text = $"{new LocalizedString("Basic", "Result").GetLocalizedString()}"; ;
        }

        // Betting Result
        //if (outGameUIManager.BettingAmount > 0)
        //{
        //    bettingPrediction.SetActive(true);
        //    for (int i = 0; i < predictionTable.Length; i++)
        //    {
        //        if (i < outGameUIManager.PredictionNumber)
        //        {
        //            predictionTable[i].SetActive(true);
        //            predictionsText[i].GetComponent<LocalizeStringEvent>().StringReference = outGameUIManager.Predictions[i];
        //            if (GameManager.Instance.BattleRoyaleManager.rankings[i] == null) rankingsText[i].text = "?";
        //            else rankingsText[i].text = GameManager.Instance.BattleRoyaleManager.rankings[i].GetLocalizedString();
        //        }
        //        else predictionTable[i].SetActive(false);
        //    }
        //    int correctExactRanking = 0;
        //    int correctOnlyRankedIn = 0;
        //    for (int i = 0; i < outGameUIManager.PredictionNumber; i++)
        //    {
        //        bool doContinue = false;
        //        for (int j = 0; j < outGameUIManager.PredictionNumber; j++)
        //        {
        //            if (predictionsText[i].text == rankingsText[j].text)
        //            {
        //                if (i == j)
        //                {
        //                    correctExactRanking++;
        //                    predictionsBG[i].color = new Color(0.48f, 1f, 0.44f);
        //                }
        //                else
        //                {
        //                    correctOnlyRankedIn++;
        //                    predictionsBG[i].color = new Color(0.89f, 0.93f, 0.39f);
        //                }
        //                doContinue = true;
        //                continue;
        //            }
        //        }
        //        if (doContinue) continue;
        //        predictionsBG[i].color = new Color(0.88f, 0.43f, 0.43f);
        //    }
        //    float odds = outGameUIManager.GetOdds(correctExactRanking, correctOnlyRankedIn);
        //    if (odds >= 10) AchievementManager.UnlockAchievement("King of Betting");
        //    if (odds >= 100) AchievementManager.UnlockAchievement("God of Betting");
        //    long bettingRewards = (long)(outGameUIManager.BettingAmount * odds);
        //    if (bettingRewards > 99999999) bettingRewards = 99999999;
        //    bettingRewardsText.text = $"{new LocalizedString("Basic", "Bet Amount :").GetLocalizedString()} $ <color=red>- {outGameUIManager.BettingAmount}</color>\n{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : <color=green>$ {bettingRewards}</color>\n($ {outGameUIManager.BettingAmount} x {odds:0.##})";
        //    totalProfit += (int)bettingRewards - outGameUIManager.BettingAmount;
        //}
        //else
        //{
        //    bettingPrediction.SetActive(false);
        //    bettingRewardsText.text = $"{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : $ 0";
        //}

        if (totalProfit >= 0) totalProfitText.text = $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : <color=green>$ {totalProfit}</color>";
        else totalProfitText.text = $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : <color=red>- $ {-totalProfit}</color>";

    }

    void Promote(SurvivorData survivor)
    {
        switch(calendar.LeagueReserveInfo[calendar.Today].league)
        {
            case League.BronzeLeague:
                survivor.tier = Tier.Silver;
                outGameUIManager.UpgradeFacility();
                outGameUIManager.objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective2").GetLocalizedString()}";
                notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                break;
            case League.SilverLeague:
                survivor.tier = Tier.Gold;
                outGameUIManager.UpgradeFacility();
                outGameUIManager.objectiveText.text = $"{new LocalizedString("Basic", "Objective").GetLocalizedString()} : {new LocalizedString("Basic", "Objective3").GetLocalizedString()}";
                notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                break;
            case League.GoldLeague:
                //calendar.NeareastSeasonChampionship.reserver = survivor;
                survivor.haveQualifyToParticipateInSeasonChampionship = true;
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "SeasonChampionship").GetLocalizedString()); };
                if (outGameUIManager.trainingLevel < 4)
                {
                    outGameUIManager.UpgradeFacility();
                    notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                }
                //notification += () => { outGameUIManager.Alert("Alert:Obtain Season Championship Ticket", survivor.localizedSurvivorName.GetLocalizedString()); };
                break;
            case League.SeasonChampionship:
                //calendar.NeareastWorldChampionship.reserver = survivor;
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "WorldChampionship").GetLocalizedString()); };
                //if (outGameUIManager.trainingLevel < 5)
                //{
                //    outGameUIManager.UpgradeFacility();
                //    notification += () => { outGameUIManager.Alert("Alert:Facility upgraded."); };
                //}
                //survivor.haveQualifyToParticipateInWorldChampionship = true;
                //notification += () => { outGameUIManager.Alert("Alert:Obtain World Championship Ticket", survivor.localizedSurvivorName.GetLocalizedString()); };
                int characteristic = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.ChokingUnderPressure);
                if (characteristic != -1)
                {
                    survivor.characteristics.RemoveAt(characteristic);
                    CharacteristicManager.AddCharaicteristic(survivor, CharacteristicType.ClutchPerformance, true);
                    notification += () => { outGameUIManager.Alert("Alert:Overcame", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Characteristic", "ClutchPerformance").GetLocalizedString(), new LocalizedString("Characteristic", "ChokingUnderPressure").GetLocalizedString()); };
                    AchievementManager.UnlockAchievement("Overcome");
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

        Survivor playerSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[0];
        if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null && GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorID == 0) playerWin = 1;
        else
        {
            for (int i = 0; i < GameManager.Instance.BattleRoyaleManager.rankings.Length; i++)
            {
                float percentile = (float)i + 1 / GameManager.Instance.BattleRoyaleManager.rankings.Length;
                if (GameManager.Instance.BattleRoyaleManager.rankings[i] == playerSurvivor.survivorName)
                {
                    if (percentile <= 0.25f) playerWin = 25;
                    else if (percentile <= 0.5f) playerWin = 50;
                    else playerWin = -1;
                    break;
                }
            }
        }
        //if (playerWin == 1) Promote(playerSurvivor.LinkedSurvivorData);
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
        if (outGameUIManager.MySurvivorDataInBattleRoyale != null) LinkStastics();
    }

    public void ExitBattle(bool goTitle = false)
    {
        gameResult.SetActive(false);
        ExitBattleEvent();
        ClearBattleRoyale();
        if (!goTitle)
        {
            // 여기서 수술
            foreach (var injury in injuryNeedSurgery)
            {
                injury.type = InjuryType.ArtificialPartsTransplanted;
                injury.degree = 0;
            }

            RecordChampionshipProgress();
            ContestantsMaintain();

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
                if (outGameUIManager.Championship) outGameUIManager.OpenChampionshipProgress();
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
        if(outGameUIManager.championshipHeldCount >= 3)
        {
            int playerSurvivorRank = outGameUIManager.championshipDatas.Find(x => x.SurvivorName.TableEntryReference.Key == outGameUIManager.MySurvivorsData[0].localizedSurvivorName.TableEntryReference.Key).currentRank;
            if (calendar.LeagueReserveInfo[calendar.Today].league == League.SeasonChampionship)
            {
                // 시챔 끝
                if(playerSurvivorRank < 5)
                {
                    if(playerSurvivorRank == 0)
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

    void LinkStastics()
    {
        SurvivorData survivor = outGameUIManager.MySurvivorDataInBattleRoyale;
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
                //case League.SeasonChampionship:
                //    survivor.wonSeasonChampionship = true;
                //    break;
                //case League.WorldChampionship:
                //    survivor.wonWorldChampionship = true;
                //    break;
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
        string key = AchievementManager.earnedAchievementsInThisRun[index].Replace(" ", "").Replace("-", "");
        AchievementUIManager.AchievementInfo achievement = AchievementUIManager.AchievementInfos.Find(x => x.achievementKey == key);
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
        if (GameManager.Instance.BattleRoyaleManager == null || GameManager.Instance.BattleRoyaleManager.BattleWinner == null) return;
        SetText(outGameUIManager.MySurvivorDataInBattleRoyale != null, out int _, out int _, out int _);
    }

}

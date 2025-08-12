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

    public void ShowGameResult(bool isBattleEnd = true)
    {
        lastTimeScale = (int)Time.timeScale;
        Time.timeScale = 0;
        gameResult.SetActive(true);
        buttonKeepWatching.SetActive(!isBattleEnd);

        bool didPlayerParticipate = outGameUIManager.MySurvivorDataInBattleRoyale != null;
        mySurvivorResult.SetActive(didPlayerParticipate);
        mySurvivorTreatmentCost.SetActive(didPlayerParticipate);
        SetText(didPlayerParticipate, out int totalProfit);
        outGameUIManager.Money += totalProfit;
        GameManager.Instance.FixLayout(gameResult.GetComponent<RectTransform>());
    }

    // playerWin | 1: win, 25: top 25%, 50: top 50%, -1 : bottom 50%
    int playerWin;
    int winPrize = 0;
    int killPrize = 0;
    int totalTreatmentCost = 0;
    void SetText(bool didPlayerParticipate, out int totalProfit)
    {
        totalProfit = 0;
        playerWin = -1;
        if (didPlayerParticipate)
        {
            Survivor playerSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[0];
            if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null && GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorID == 0) playerWin = 1;
            else
            {
                for(int i=0; i<GameManager.Instance.BattleRoyaleManager.rankings.Length; i++)
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

            winPrize = 0;
            killPrize = 0;
            totalTreatmentCost = 0;
            switch (calendar.LeagueReserveInfo[calendar.Today].league)
            {
                case League.BronzeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 1000;
                        AchievementManager.UnlockAchievement("Bronze Cup");
                    }
                    else if (playerWin == 50) winPrize = 500;
                    killPrize = playerSurvivor.KillCount * 100;
                    break;
                case League.SilverLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 3000;
                        AchievementManager.UnlockAchievement("Silver Cup");
                    }
                    else if (playerWin == 25) winPrize = 1500;
                    else if (playerWin == 50) winPrize = 750;
                    killPrize = playerSurvivor.KillCount * 200;
                    break;
                case League.GoldLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 10000;
                        AchievementManager.UnlockAchievement("Gold Cup");
                    }
                    else if (playerWin == 25) winPrize = 5000;
                    else if (playerWin == 50) winPrize = 2500;
                    killPrize = playerSurvivor.KillCount * 500;
                    break;
                case League.SeasonChampionship:
                    if (playerWin == 1)
                    {
                        winPrize = 30000;
                        AchievementManager.UnlockAchievement("Season Champion");
                    }
                    else if (playerWin == 25) winPrize = 15000;
                    else if (playerWin == 50) winPrize = 7500;
                    killPrize = playerSurvivor.KillCount * 1000;
                    break;
                case League.WorldChampionship:
                    if (playerWin == 1)
                    {
                        winPrize = 100000;
                        AchievementManager.UnlockAchievement("World Champion");
                        if (playerSurvivor.LinkedSurvivorData.loseCount == 0) AchievementManager.UnlockAchievement("Royal Loader");
                    }
                    else if (playerWin == 25) winPrize = 50000;
                    else if (playerWin == 50) winPrize = 25000;
                    killPrize = playerSurvivor.KillCount * 3000;
                    break;
                case League.MeleeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 30000;
                        AchievementManager.UnlockAchievement("Melee Champion");
                    }
                    else if (playerWin == 25) winPrize = 15000;
                    else if (playerWin == 50) winPrize = 7500;
                    break;
                case League.RangeLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 30000;
                        AchievementManager.UnlockAchievement("Shooting Champion");
                    }
                    else if (playerWin == 25) winPrize = 15000;
                    else if (playerWin == 50) winPrize = 7500;
                    break;
                case League.CraftingLeague:
                    if (playerWin == 1)
                    {
                        winPrize = 30000;
                        AchievementManager.UnlockAchievement("Crafting Champion");
                    }
                    else if (playerWin == 25) winPrize = 15000;
                    else if (playerWin == 50) winPrize = 7500;
                    break;
            }
            winPrizeText.text = $"{new LocalizedString("Basic", "Victory reward").GetLocalizedString()} : <color=green>$ {winPrize}</color>";
            killPrizeText.text = $"{new LocalizedString("Basic", "Kill reward").GetLocalizedString()} : <color=green>$ {killPrize}</color>";
            for (int i = 0; i < treatments.Length; i++)
            {
                if (i < playerSurvivor.injuries.Count + 1)
                {
                    if (i < playerSurvivor.injuries.Count)
                    {
                        if (playerSurvivor.rememberAlreadyHaveInjury.Contains(playerSurvivor.injuries[i].site))
                        {
                            treatments[i].SetActive(false);
                            continue;
                        }
                        treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{new LocalizedString("Injury", playerSurvivor.injuries[i].site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", playerSurvivor.injuries[i].type.ToString()).GetLocalizedString()}";
                        int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i]);
                        treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                        treatments[i].GetComponentInChildren<Help>().SetDescription("");
                        totalTreatmentCost += cost;
                        treatments[i].SetActive(true);
                    }
                    else
                    {
                        if (playerSurvivor.curBlood / playerSurvivor.maxBlood < 0.8f)
                        {
                            // ¼öÇ÷ºñ
                            int bloodTransfusionFee = (int)((playerSurvivor.maxBlood - playerSurvivor.curBlood) * 0.1f);
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = new LocalizedString("Basic", "Blood transfusion cost").GetLocalizedString();
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {bloodTransfusionFee}</color>";
                            treatments[i].GetComponentInChildren<Help>().SetDescription(new LocalizedString("Basic", "Help:Blood transfusion cost").GetLocalizedString());
                            treatments[i].SetActive(true);
                        }
                        else treatments[i].SetActive(false);
                    }
                }
                else treatments[i].SetActive(false);
            }
            totalTreatmentCostText.text = $"{new LocalizedString("Basic", "Total medical cost").GetLocalizedString()} : <color=red>- $ {totalTreatmentCost}</color>";
            totalProfit = winPrize + killPrize - totalTreatmentCost;

            if (playerWin == 1) Promote(playerSurvivor.LinkedSurvivorData);
        }
        else
        {
            if (GameManager.Instance.BattleRoyaleManager.BattleWinner != null) gameResultText.text = $"{new LocalizedString("Basic", "wins!") { Arguments = new[] { GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorName.GetLocalizedString() } }.GetLocalizedString()}";
            else ExitBattle();
        }

        // Betting Result
        if (outGameUIManager.BettingAmount > 0)
        {
            bettingPrediction.SetActive(true);
            for (int i = 0; i < predictionTable.Length; i++)
            {
                if (i < outGameUIManager.PredictionNumber)
                {
                    predictionTable[i].SetActive(true);
                    predictionsText[i].GetComponent<LocalizeStringEvent>().StringReference = outGameUIManager.Predictions[i];
                    rankingsText[i].text = GameManager.Instance.BattleRoyaleManager.rankings[i].GetLocalizedString();
                }
                else predictionTable[i].SetActive(false);
            }
            int correctExactRanking = 0;
            int correctOnlyRankedIn = 0;
            for (int i = 0; i < outGameUIManager.PredictionNumber; i++)
            {
                bool doContinue = false;
                for (int j = 0; j < outGameUIManager.PredictionNumber; j++)
                {
                    if (predictionsText[i].text == rankingsText[j].text)
                    {
                        if (i == j)
                        {
                            correctExactRanking++;
                            predictionsBG[i].color = new Color(0.48f, 1f, 0.44f);
                        }
                        else
                        {
                            correctOnlyRankedIn++;
                            predictionsBG[i].color = new Color(0.89f, 0.93f, 0.39f);
                        }
                        doContinue = true;
                        continue;
                    }
                }
                if (doContinue) continue;
                predictionsBG[i].color = new Color(0.88f, 0.43f, 0.43f);
            }
            float odds = outGameUIManager.GetOdds(correctExactRanking, correctOnlyRankedIn);
            if (odds >= 100) AchievementManager.UnlockAchievement("God of Betting");
            else if (odds >= 10) AchievementManager.UnlockAchievement("King of Betting");
            int bettingRewards = (int)(outGameUIManager.BettingAmount * odds);
            bettingRewardsText.text = $"{new LocalizedString("Basic", "Bet Amount :").GetLocalizedString()} $ <color=red>- {outGameUIManager.BettingAmount}</color>\n{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : <color=green>$ {bettingRewards}</color>\n($ {outGameUIManager.BettingAmount} x {odds:0.##})";
            totalProfit += bettingRewards - outGameUIManager.BettingAmount;
        }
        else
        {
            bettingPrediction.SetActive(false);
            bettingRewardsText.text = $"{new LocalizedString("Basic", "Betting payout").GetLocalizedString()} : $ 0";
        }

        if (totalProfit >= 0) totalProfitText.text = $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : <color=green>$ {totalProfit}</color>";
        else totalProfitText.text = $"{new LocalizedString("Basic", "Net profit/loss").GetLocalizedString()} : <color=red>- $ {-totalProfit}</color>";

    }

    void Promote(SurvivorData survivor)
    {
        switch(calendar.LeagueReserveInfo[calendar.Today].league)
        {
            case League.BronzeLeague:
                survivor.tier = Tier.Silver;
                break;
            case League.SilverLeague:
                survivor.tier = Tier.Gold;
                break;
            case League.GoldLeague:
                calendar.NeareastSeasonChampionship.reserver = survivor;
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "SeasonChampionship").GetLocalizedString()); };
                break;
            case League.SeasonChampionship:
                calendar.NeareastWorldChampionship.reserver = survivor;
                int characteristic = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.ChokingUnderPressure);
                if (characteristic != -1)
                {
                    survivor.characteristics.RemoveAt(characteristic);
                    CharacteristicManager.AddCharaicteristic(survivor, CharacteristicType.ClutchPerformance);
                    notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Characteristic", "ClutchPerformance").GetLocalizedString(), new LocalizedString("Characteristic", "ChokingUnderPressure").GetLocalizedString()); };
                    AchievementManager.UnlockAchievement("Overcome");
                }
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", "WorldChampionship").GetLocalizedString()); };
                break;
        }
    }

    public void DelayedShowGameResult()
    {
        resultClaimed = true;
    }

    public void ExitBattle(bool goTitle = false)
    {
        gameResult.SetActive(false);
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
        if(outGameUIManager.MySurvivorDataInBattleRoyale != null) LinkStastics();
        GameManager.Instance.inGameUICanvas.SetActive(false);
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.globalCanvas.SetActive(true);
        if(!goTitle)
        {
            GameManager.Instance.OutGameUIManager.EndTheDayWeekend();
            GameManager.Instance.OutGameUIManager.CheckTrainable(GameManager.Instance.BattleRoyaleManager.Survivors[0].LinkedSurvivorData);
            GameManager.Instance.OutGameUIManager.ResetSelectedSurvivorInfo();
            notification?.Invoke();
            // Auto save
            GameManager.Instance.Save(0);
            GameManager.Instance.Option.SetSaveButtonInteractable(true);
        }
        notification = null;
        GameManager.Instance.DestroyBattleRoyaleManager();
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
                case League.SeasonChampionship:
                    survivor.wonSeasonChampionship = true;
                    break;
                case League.WorldChampionship:
                    survivor.wonWorldChampionship = true;
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

    void OnLocaleChanged(Locale newLocale)
    {
        if (GameManager.Instance.BattleRoyaleManager == null || GameManager.Instance.BattleRoyaleManager.BattleWinner == null) return;
        SetText(outGameUIManager.MySurvivorDataInBattleRoyale != null, out int _);
    }

}

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

    void ShowGameResult(bool isBattleEnd)
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

    void SetText(bool didPlayerParticipate, out int totalProfit)
    {
        totalProfit = 0;
        if (didPlayerParticipate)
        {
            Survivor playerSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[0];
            bool playerWin = GameManager.Instance.BattleRoyaleManager.BattleWinner != null && GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorID == 0;
            LocalizedString resultText = playerWin ? new LocalizedString("Table", "Your survivor won!") : new LocalizedString("Table", "Your survivor was defeated.");
            resultText.Arguments = new[] { outGameUIManager.MySurvivorDataInBattleRoyale.localizedSurvivorName.GetLocalizedString() };
            gameResultText.text = resultText.GetLocalizedString();
            survivedTimeText.text = $"{new LocalizedString("Table", "Survival Time").GetLocalizedString()} : {(int)playerSurvivor.SurvivedTime / 60:00m} {(int)playerSurvivor.SurvivedTime - ((int)playerSurvivor.SurvivedTime / 60) * 60:00s}";
            killsText.text = $"{new LocalizedString("Table", "Kill").GetLocalizedString()} : {playerSurvivor.KillCount}";
            totalDamageText.text = $"{new LocalizedString("Table", "Total damage dealt").GetLocalizedString()} : {(int)playerSurvivor.TotalDamage}";

            int winPrize = 0;
            int killPrize = 0;
            int totalTreatmentCost = 0;
            switch (calendar.LeagueReserveInfo[calendar.Today].league)
            {
                case League.BronzeLeague:
                    if (playerWin) winPrize = 1000;
                    killPrize = playerSurvivor.KillCount * 100;
                    break;
                case League.SilverLeague:
                    if (playerWin) winPrize = 3000;
                    killPrize = playerSurvivor.KillCount * 200;
                    break;
                case League.GoldLeague:
                    if (playerWin) winPrize = 10000;
                    killPrize = playerSurvivor.KillCount * 500;
                    break;
                case League.SeasonChampionship:
                    if (playerWin) winPrize = 30000;
                    killPrize = playerSurvivor.KillCount * 1000;
                    break;
                case League.WorldChampionship:
                    if (playerWin) winPrize = 100000;
                    killPrize = playerSurvivor.KillCount * 3000;
                    break;
            }
            winPrizeText.text = $"{new LocalizedString("Table", "Victory reward").GetLocalizedString()} : <color=green>$ {winPrize}</color>";
            killPrizeText.text = $"{new LocalizedString("Table", "Kill reward").GetLocalizedString()} : <color=green>$ {killPrize}</color>";
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
                        //treatments[i].GetComponentInChildren<Help>().SetDescription(playerSurvivor.injuries[i].site);
                        totalTreatmentCost += cost;
                        treatments[i].SetActive(true);
                    }
                    else
                    {
                        if (playerSurvivor.curBlood / playerSurvivor.maxBlood < 0.8f)
                        {
                            // ¼öÇ÷ºñ
                            int bloodTransfusionFee = (int)((playerSurvivor.maxBlood - playerSurvivor.curBlood) * 0.1f);
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = new LocalizedString("Table", "Blood transfusion cost").GetLocalizedString();
                            treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {bloodTransfusionFee}</color>";
                            treatments[i].GetComponentInChildren<Help>().SetDescription(new LocalizedString("Table", "Help:Blood transfusion cost").GetLocalizedString());
                            treatments[i].SetActive(true);
                        }
                        else treatments[i].SetActive(false);
                    }
                }
                else treatments[i].SetActive(false);
            }
            totalTreatmentCostText.text = $"{new LocalizedString("Table", "Total medical cost").GetLocalizedString()} : <color=red>- $ {totalTreatmentCost}</color>";
            totalProfit = winPrize + killPrize - totalTreatmentCost;

            if (playerWin) Promote(playerSurvivor.LinkedSurvivorData);
        }
        else
        {
            gameResultText.text = $"{new LocalizedString("Table", "wins!") { Arguments = new[] { GameManager.Instance.BattleRoyaleManager.BattleWinner.survivorName } }.GetLocalizedString()}";
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
            int bettingRewards = (int)(outGameUIManager.BettingAmount * odds);
            bettingRewardsText.text = $"{new LocalizedString("Table", "Bet Amount :").GetLocalizedString()} $ <color=red>- {outGameUIManager.BettingAmount}</color>\n{new LocalizedString("Table", "Betting payout").GetLocalizedString()} : <color=green>$ {bettingRewards}</color>\n($ {outGameUIManager.BettingAmount} x {odds:0.##})";
            totalProfit += bettingRewards - outGameUIManager.BettingAmount;
        }
        else
        {
            bettingPrediction.SetActive(false);
            bettingRewardsText.text = $"{new LocalizedString("Table", "Betting payout").GetLocalizedString()} : $ 0";
        }

        if (totalProfit >= 0) totalProfitText.text = $"{new LocalizedString("Table", "Net profit/loss").GetLocalizedString()} : <color=green>$ {totalProfit}</color>";
        else totalProfitText.text = $"{new LocalizedString("Table", "Net profit/loss").GetLocalizedString()} : <color=red>- $ {-totalProfit}</color>";

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
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Table", "Season Championship").GetLocalizedString()); };
                break;
            case League.SeasonChampionship:
                calendar.NeareastWorldChampionship.reserver = survivor;
                int characteristic = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.ChokingUnderPressure);
                if (characteristic != -1)
                {
                    survivor.characteristics.RemoveAt(characteristic);
                    CharacteristicManager.AddCharaicteristic(survivor, CharacteristicType.ClutchPerformance);
                    notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Characteristic", "ClutchPerformance").GetLocalizedString(), new LocalizedString("Characteristic", "ChokingUnderPressure").GetLocalizedString()); };
                }
                notification += () => { outGameUIManager.Alert("Alert:Auto Reserve", survivor.localizedSurvivorName.GetLocalizedString(), new LocalizedString("Table", "World Championship").GetLocalizedString()); };
                break;
        }
    }

    public void DelayedShowGameResult()
    {
        resultClaimed = true;
    }

    public void ExitBattle()
    {
        gameResult.SetActive(false);
        AudioSource bgsfx = GameManager.Instance.BattleRoyaleManager.bgsfx;
        SoundManager.StopSFX(bgsfx);
        bgsfx.minDistance = 1;
        bgsfx.maxDistance = 500;
        SoundManager.Play(ResourceEnum.BGM.the_birth_of_hip_hop);
        foreach (Survivor survivor in GameManager.Instance.BattleRoyaleManager.Survivors)
        {
            foreach (GameObject blood in survivor.bloods) PoolManager.Despawn(blood);
            foreach (GameObject buried in survivor.burieds) PoolManager.Despawn(buried);
        }
        GameManager.Instance.inGameUICanvas.SetActive(false);
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.globalCanvas.SetActive(true);
        GameManager.Instance.OutGameUIManager.EndTheDayWeekend();
        GameManager.Instance.OutGameUIManager.CheckTrainable(GameManager.Instance.BattleRoyaleManager.Survivors[0].LinkedSurvivorData);
        GameManager.Instance.OutGameUIManager.ResetSelectedSurvivorInfo();
        GameManager.Instance.OutGameUIManager.contestantsData.Clear();
        GameManager.Instance.BattleRoyaleManager.Destroy();
        notification?.Invoke();
        notification = null;
        // Auto save
        GameManager.Instance.Save(0);
        GameManager.Instance.Option.SetSaveButtonInteractable(true);
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

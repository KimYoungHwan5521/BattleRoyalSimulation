using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameResult : MonoBehaviour
{
    OutGameUIManager outGameUIManager => GameManager.Instance.OutGameUIManager;
    Calendar calendar => GetComponent<Calendar>();

    [SerializeField] GameObject gameResult;
    [SerializeField] TextMeshProUGUI gameResultText;
    [SerializeField] TextMeshProUGUI survivedTimeText;
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI totalDamageText;
    [SerializeField] GameObject buttonKeepWatching;

    [SerializeField] TextMeshProUGUI winPrizeText;
    [SerializeField] TextMeshProUGUI killPrizeText;
    [SerializeField] TextMeshProUGUI totalTreatmentCostText;
    [SerializeField] TextMeshProUGUI totalProfitText;
    [SerializeField] GameObject[] treatments;

    bool resultClaimed;
    float resultDelay = 2f;
    float curResultDelay;
    int lastTimeScale;

    private void Update()
    {
        if(resultClaimed)
        {
            curResultDelay += Time.unscaledDeltaTime;
            if(curResultDelay > resultDelay )
            {
                curResultDelay = 0;
                resultClaimed = false;
                ShowGameResult(BattleRoyaleManager.BattleWinner != null);
            }
        }
    }

    void ShowGameResult(bool isBattleEnd)
    {
        lastTimeScale = (int)Time.timeScale;
        Time.timeScale = 0;
        gameResult.SetActive(true);
        buttonKeepWatching.SetActive(!isBattleEnd);
        
        Survivor playerSurvivor = BattleRoyaleManager.Survivors[0];
        bool playerWin = BattleRoyaleManager.BattleWinner != null && BattleRoyaleManager.BattleWinner.survivorID == 0;
        gameResultText.text = playerWin ? $"Your Survivor({outGameUIManager.MySurvivorDataInBattleRoyale.survivorName}) Has Won!" : $"Your Survivor({outGameUIManager.MySurvivorDataInBattleRoyale.survivorName}) Has Lost";
        survivedTimeText.text = $"Survived Time : {(int)playerSurvivor.SurvivedTime / 60:00m} {(int)playerSurvivor.SurvivedTime - ((int)playerSurvivor.SurvivedTime / 60) * 60:00s}";
        killsText.text = $"Kills : {playerSurvivor.KillCount}";
        totalDamageText.text = $"Total Damage : {(int)playerSurvivor.TotalDamage}";

        int winPrize = 0;
        int killPrize = 0;
        int totalTreatmentCost = 0;
        if(playerWin)
        {
            switch(calendar.LeagueReserveInfo[calendar.Today].league)
            {
                case League.BronzeLeague:
                    winPrize = 1000;
                    killPrize = playerSurvivor.KillCount * 100;
                    break;
                case League.SilverLeague:
                    winPrize = 3000;
                    killPrize = playerSurvivor.KillCount * 200;
                    break;
                case League.GoldLeague:
                    winPrize = 10000;
                    killPrize = playerSurvivor.KillCount * 500;
                    break;
                case League.SeasonChampionship:
                    winPrize = 30000;
                    killPrize = playerSurvivor.KillCount * 1000;
                    break;
                case League.WorldChampionship:
                    winPrize = 100000;
                    killPrize = playerSurvivor.KillCount * 3000;
                    break;
            }
        }
        winPrizeText.text = $"Win Prize : <color=green>$ {winPrize}</color>";
        killPrizeText.text = $"Kill Prize : <color=green>$ {killPrize}</color>";
        for(int i = 0; i<treatments.Length; i++)
        {
            if (i < playerSurvivor.injuries.Count)
            {
                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{playerSurvivor.injuries[i].site} {playerSurvivor.injuries[i].type}";
                int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i]);
                treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                treatments[i].GetComponentInChildren<Help>().SetDescription(playerSurvivor.injuries[i].site);
                totalTreatmentCost += cost;
            }
            else treatments[i].SetActive(false);
        }
        totalTreatmentCostText.text = $"Total Treatment Cost : <color=red>- $ {totalTreatmentCost}</color>";
        int totalProfit = winPrize + killPrize - totalTreatmentCost;
        if (totalProfit >= 0) totalProfitText.text = $"Total Profit/Loss : <color=green>$ {totalProfit}</color>";
        else totalProfitText.text = $"Total Profit/Loss : <color=red>- $ {-totalProfit}</color>";
    }

    public void DelayedShowGameResult()
    {
        resultClaimed = true;
    }

    public void ExitBattle()
    {
        gameResult.SetActive(false);
        GameManager.Instance.inGameUICanvas.SetActive(false);
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.globalCanvas.SetActive(true);
        GameManager.Instance.OutGameUIManager.EndTheDayWeekend();
        GameManager.Instance.OutGameUIManager.ResetSelectedSurvivorInfo();
    }

    public void KeepWatching()
    {
        gameResult.SetActive(false);
        Time.timeScale = lastTimeScale;
    }

    void OnCancel(InputValue value)
    {
        if(value.Get<float>() > 0 && BattleRoyaleManager.BattleWinner == null)
        {
            gameResult.SetActive(true);
        }
    }
}

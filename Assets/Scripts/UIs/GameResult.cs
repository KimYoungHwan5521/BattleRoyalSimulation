using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public delegate void ReserveNotification();

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

    [SerializeField] bool resultClaimed;
    readonly float resultDelay = 2f;
    [SerializeField] float curResultDelay;
    int lastTimeScale;

    ReserveNotification notification;

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
        switch(calendar.LeagueReserveInfo[calendar.Today].league)
        {
            case League.BronzeLeague:
                if(playerWin) winPrize = 1000;
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
        winPrizeText.text = $"Win Prize : <color=green>$ {winPrize}</color>";
        killPrizeText.text = $"Kill Prize : <color=green>$ {killPrize}</color>";
        for(int i = 0; i<treatments.Length; i++)
        {
            if (i < playerSurvivor.injuries.Count + 1)
            {
                if(i < playerSurvivor.injuries.Count)
                {
                    if (playerSurvivor.rememberAlreadyHaveInjury.Contains(playerSurvivor.injuries[i].site))
                    {
                        treatments[i].SetActive(false);
                        continue;
                    }
                    treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{playerSurvivor.injuries[i].site} {playerSurvivor.injuries[i].type}";
                    int cost = outGameUIManager.MeasureTreatmentCost(playerSurvivor.injuries[i]);
                    treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {cost}</color>";
                    treatments[i].GetComponentInChildren<Help>().SetDescription(playerSurvivor.injuries[i].site);
                    totalTreatmentCost += cost;
                    treatments[i].SetActive(true);
                }
                else 
                {
                    if(playerSurvivor.curBlood / playerSurvivor.maxBlood < 0.8f)
                    {
                        // ¼öÇ÷ºñ
                        int bloodTransfusionFee = (int)((playerSurvivor.maxBlood - playerSurvivor.curBlood) * 0.1f);
                        treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"Blood transfusion fee";
                        treatments[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"<color=red>- $ {bloodTransfusionFee}</color>";
                        treatments[i].GetComponentInChildren<Help>().SetDescription("$1 per 10mL");
                        treatments[i].SetActive(true);
                    }
                    else treatments[i].SetActive(false);
                }
            }
            else treatments[i].SetActive(false);
        }
        totalTreatmentCostText.text = $"Total Treatment Cost : <color=red>- $ {totalTreatmentCost}</color>";
        int totalProfit = winPrize + killPrize - totalTreatmentCost;
        if (totalProfit >= 0) totalProfitText.text = $"Total Profit/Loss : <color=green>$ {totalProfit}</color>";
        else totalProfitText.text = $"Total Profit/Loss : <color=red>- $ {-totalProfit}</color>";
        outGameUIManager.Money += totalProfit;

        if (playerWin) Promote(playerSurvivor.LinkedSurvivorData);
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
                notification += () => { outGameUIManager.Alert($"{survivor.survivorName} has been booked for next Season Championship\n<i>(In the meantime, he can join other leagues)</i>"); };
                break;
            case League.SeasonChampionship:
                calendar.NeareastWorldChampionship.reserver = survivor;
                int characteristic = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.ChokingUnderPressure);
                if (characteristic != -1)
                {
                    survivor.characteristics.RemoveAt(characteristic);
                    CharacteristicManager.AddCharaicteristic(survivor, CharacteristicType.ClutchPerformance);
                    notification += () => { outGameUIManager.Alert($"{survivor.survivorName} overcame <i>Chocking Under Pressure</i> and acquired new characteristic <i>Clutch Performance</i>"); };
                }
                notification += () => { outGameUIManager.Alert($"{survivor.survivorName} has been booked for next World Championship\n<i>(In the meantime, he can join other leagues)</i>"); };
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
        foreach(Survivor survivor in BattleRoyaleManager.Survivors)
        {
            foreach (GameObject blood in survivor.bloods) PoolManager.Despawn(blood);
        }
        GameManager.Instance.inGameUICanvas.SetActive(false);
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.globalCanvas.SetActive(true);
        GameManager.Instance.OutGameUIManager.CheckTrainable(BattleRoyaleManager.Survivors[0].LinkedSurvivorData);
        GameManager.Instance.OutGameUIManager.EndTheDayWeekend();
        GameManager.Instance.OutGameUIManager.ResetSelectedSurvivorInfo();
        notification?.Invoke();
        notification = null;
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

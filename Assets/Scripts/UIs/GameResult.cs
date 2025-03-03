using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameResult : MonoBehaviour
{
    [SerializeField] GameObject gameResult;
    [SerializeField] TextMeshProUGUI gameResultText;
    [SerializeField] TextMeshProUGUI survivedTimeText;
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] TextMeshProUGUI totalDamageText;
    [SerializeField] GameObject buttonKeepWatching;

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
        gameResultText.text = BattleRoyaleManager.BattleWinner != null && BattleRoyaleManager.BattleWinner.survivorID == 0 ? "Your Survivor Has Won!" : "Your Survivor Has Lost";
        survivedTimeText.text = $"Survived Time : {(int)playerSurvivor.SurvivedTime / 60:00m} {(int)playerSurvivor.SurvivedTime - ((int)playerSurvivor.SurvivedTime / 60) * 60:00s}";
        killsText.text = $"Kills : {playerSurvivor.KillCount}";
        totalDamageText.text = $"Total Damage : {(int)playerSurvivor.TotalDamage}";
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

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

    private void Update()
    {
        if(resultClaimed)
        {
            curResultDelay += Time.unscaledDeltaTime;
            if(curResultDelay > resultDelay )
            {
                curResultDelay = 0;
                resultClaimed = false;
                ShowGameResult(BattleRoyalManager.BattleWinner != null);
            }
        }
    }

    void ShowGameResult(bool isBattleEnd)
    {
        Time.timeScale = 0;
        gameResult.SetActive(true);
        buttonKeepWatching.SetActive(!isBattleEnd);
        Survivor survivor = BattleRoyalManager.Survivors[0];
        gameResultText.text = BattleRoyalManager.BattleWinner == survivor ? "Your Survivor Has Won!" : "Your Survivor Has Lost";
        survivedTimeText.text = $"Survived Time : {(int)BattleRoyalManager.battleTime / 60:00m} {(int)BattleRoyalManager.battleTime - ((int)BattleRoyalManager.battleTime / 60) * 60:00s}";
        killsText.text = $"Kills : {survivor.KillCount}";
        totalDamageText.text = $"Total Damage : {(int)survivor.TotalDamage}";
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
        Time.timeScale = 1;
    }

    void OnCancel(InputValue value)
    {
        if(value.Get<float>() > 0 && BattleRoyalManager.BattleWinner == null)
        {
            gameResult.SetActive(true);
        }
    }
}

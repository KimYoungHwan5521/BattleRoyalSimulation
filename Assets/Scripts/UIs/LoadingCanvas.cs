using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    [SerializeField] Image loadingBar;
    [SerializeField] TextMeshProUGUI loadingProgressText;
    [SerializeField] TextMeshProUGUI loadInfo;
    [SerializeField] TextMeshProUGUI tooltipText;
    bool loading;
    float tooltipCool = 10;
    float curTooltipCool;

    List<string> tooltips = new()
    {
        "The expected value of a bet is greater than 1.",
        "The maximum value of an ability is 100 by default, but some survivors with certain characteristics can exceed 100.",
        "만약 훈련으로 능력치가 오르지 않는다면 시설을 업그레이드 하십시오.",
    };

    private void Start()
    {
        ResetTooltip();
    }

    private void Update()
    {
        if (loading)
        {
            curTooltipCool += Time.unscaledDeltaTime;
            if (curTooltipCool > tooltipCool)
            {
                curTooltipCool = 0;
                ResetTooltip();
            }
        }
    }

    public void SetLoadInfo(string info, int numerator, int denominator)
    {
        if (denominator == 0) return;
        loading = true;
        loadingBar.fillAmount = (float)numerator / denominator;
        loadingProgressText.text = $"{(float)numerator / denominator * 100} %";
        loadInfo.text = info;
    }

    public void CloseLoadInfo()
    {
        curTooltipCool = 0;
        loading = false;
    }

    void ResetTooltip()
    {
        int rand = Random.Range(0, tooltips.Count);
        tooltipText.text = tooltips[rand];
    }
}
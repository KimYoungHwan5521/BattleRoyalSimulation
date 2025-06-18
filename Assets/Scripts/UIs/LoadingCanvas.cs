using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
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

    List<LocalizedString> tooltips = new()
    {
        new("Table", "The expected value of the bet is greater than 1."),
        new("Table", "Base stats cap at 100, but can exceed with traits."),
        new("Table", "If training doesn't raise stats, upgrade your training facility."),
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
        tooltipText.text = tooltips[rand].GetLocalizedString();
    }
}
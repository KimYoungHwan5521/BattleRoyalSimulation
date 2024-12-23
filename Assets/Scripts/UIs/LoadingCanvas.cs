using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    [SerializeField] protected Image loadingBar;
    [SerializeField] protected TextMeshProUGUI loadingProgressText;
    [SerializeField] protected TextMeshProUGUI loadInfo;

    public void SetLoadInfo(string info, int numerator, int denominator)
    {
        if (denominator == 0) return;

        loadingBar.fillAmount = (float)numerator / denominator;
        loadingProgressText.text = $"{(float)numerator / denominator * 100} %";
        loadInfo.text = info;
    }
}
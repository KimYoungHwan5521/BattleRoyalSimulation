using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Description : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descriptionText;
    private void LateUpdate()
    {
        if (descriptionText.GetComponent<RectTransform>().rect.width > 600)
        {
            descriptionText.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            descriptionText.rectTransform.sizeDelta = new(600, 0);
        }
    }

    private void OnDisable()
    {
        descriptionText.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void SetText(string text)
    {
        descriptionText.text = text;
    }
}

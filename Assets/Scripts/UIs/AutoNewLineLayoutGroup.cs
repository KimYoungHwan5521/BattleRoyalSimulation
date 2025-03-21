using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoNewLineLayoutGroup : MonoBehaviour
{
    Rect Rect => GetComponent<RectTransform>().rect;
    [SerializeField] GameObject[] characteristicsBox;
    [SerializeField] float paddingTop;

    public void ArrangeCharacteristics(SurvivorData survivorData)
    {
        for (int i = 0; i < characteristicsBox.Length; i++)
        {
            if (i < survivorData.characteristics.Count)
            {
                characteristicsBox[i].GetComponentInChildren<TextMeshProUGUI>().text = survivorData.characteristics[i].characteristicName;
                characteristicsBox[i].GetComponent<Help>().SetDescription(survivorData.characteristics[i].description);
                characteristicsBox[i].GetComponent<RectTransform>().sizeDelta = new(Mathf.Min(survivorData.characteristics[i].characteristicName.Length * 19, Rect.width - 1), 40);
                characteristicsBox[i].SetActive(true);
            }
            else
            {
                characteristicsBox[i].SetActive(false);
            }
        }
        ArrangeChildren();
    }

    public void ArrangeChildren()
    {
        float width = 0;
        int line = 0;

        List<RectTransform> children = new();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) children.Add(child.GetComponent<RectTransform>());
        }

        float childHeight = children.Count > 0 ? children[0].rect.height : 0;
        foreach (RectTransform child in children)
        {
            if(line == 0 || width + 5 + child.rect.width > Rect.width)
            {
                line++;
                width = 0;
            }
            else
            {
                width += 5;
            }
            child.anchoredPosition = new(width, -(child.rect.height + 5) * (line - 1) - paddingTop);
            width += child.rect.width;
        }

        GetComponent<RectTransform>().sizeDelta = new(Rect.width, line * childHeight + 5 * Mathf.Max(0, (line - 1)));
    }
}

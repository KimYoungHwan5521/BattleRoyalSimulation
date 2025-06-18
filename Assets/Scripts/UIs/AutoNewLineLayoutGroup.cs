using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class AutoNewLineLayoutGroup : MonoBehaviour
{
    Rect Rect => GetComponent<RectTransform>().rect;
    [SerializeField] GameObject[] characteristicsBox;
    [SerializeField] float paddingTop;
    TextMeshProUGUI[] characteristicsText;

    SurvivorData linkedSurvivor;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        FindCharacteristicsText();
    }

    void FindCharacteristicsText()
    {
        characteristicsText = new TextMeshProUGUI[characteristicsBox.Length];
        for(int i = 0; i < characteristicsText.Length; i++) { characteristicsText[i] = characteristicsBox[i].GetComponentInChildren<TextMeshProUGUI>(); }

    }

    public void ArrangeCharacteristics(SurvivorData survivorData)
    {
        if (characteristicsText ==  null) FindCharacteristicsText();
        linkedSurvivor = survivorData;
        for (int i = 0; i < characteristicsBox.Length; i++)
        {
            if (i < survivorData.characteristics.Count)
            {
                characteristicsBox[i].SetActive(true);
                characteristicsText[i].text = survivorData.characteristics[i].characteristicName.GetLocalizedString();
                characteristicsBox[i].GetComponent<Help>().SetDescription(survivorData.characteristics[i].description);
                LayoutRebuilder.ForceRebuildLayoutImmediate(characteristicsText[i].rectTransform);
                characteristicsBox[i].GetComponent<RectTransform>().sizeDelta = new(characteristicsText[i].rectTransform.rect.width + 6, 40);
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

    void OnLocaleChanged(Locale newLocale)
    {
        if (linkedSurvivor == null) return;
        ArrangeCharacteristics(linkedSurvivor);
    }
}

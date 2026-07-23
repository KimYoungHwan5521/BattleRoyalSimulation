using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using static TMPro.TMP_Dropdown;

[RequireComponent(typeof(TMP_Dropdown))]
public class LocalizedDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public List<LocalizedString> keys = new();
    public int Value
    {
        get => dropdown.value;
        set => dropdown.value = value;
    }

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    public void AddLocalizedOptions(List<LocalizedString> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            keys.Add(options[i]);
            dropdown.options.Add(new OptionData(options[i].GetLocalizedString()));
        }

        dropdown.RefreshShownValue();
    }

    public void AddKeys(params LocalizedString[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            keys.Add(options[i]);
            dropdown.options.Add(new OptionData(options[i].GetLocalizedString()));
        }

        dropdown.RefreshShownValue();
    }

    public void ClearOptions()
    {
        keys.Clear();
        dropdown.options?.Clear();
    }

    public void RelocalizeOptions()
    {
        int value = dropdown.value;
        dropdown.ClearOptions();
        for (int i = 0; i < keys.Count; i++)
        {
            dropdown.options.Add(new OptionData(keys[i].GetLocalizedString()));
        }
        dropdown.value = value;
        dropdown.RefreshShownValue();
    }
}

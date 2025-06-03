using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI saveDataInfo;
    public GameObject saveButton;
    public GameObject deleteButton;
    public bool isEmpty;

    public void SetInfo(string info)
    {
        saveDataInfo.text = info;
    }
    
}

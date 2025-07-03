using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class SaveSlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI saveDataInfo;
    public GameObject saveButton;
    public GameObject deleteButton;
    public bool isEmpty;

    public void SetInfo(string info, int date = -1)
    {
        if (isEmpty) saveDataInfo.text = "";
        else
        {
            Calendar calendar = GameManager.Instance.Calendar;
            var localizedIngameTime = new LocalizedString("Table", "Date Format");
            localizedIngameTime.Arguments = new[] {(2101 + (date / 28) / 12).ToString(), new LocalizedString("Table", calendar.monthName[date / 28]).GetLocalizedString(), (date % 28 + 1).ToString(), new LocalizedString("Table", calendar.dateName[date % 7]).GetLocalizedString() };
            saveDataInfo.text = $"{localizedIngameTime.GetLocalizedString()}";
        }
        saveDataInfo.text += info;
    }
    
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ScheduleTraining : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject easySchedule;
    [SerializeField] GameObject selectSchedule;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
        {
            easySchedule.SetActive(true);
            GameManager.Instance.openedWindows.Push(easySchedule);
            selectSchedule.transform.position = Input.mousePosition;
            TextMeshProUGUI[] texts = selectSchedule.GetComponentsInChildren<TextMeshProUGUI>();
            SurvivorData survivor = GetComponent<SurvivorInfo>().survivorData;
            selectSchedule.GetComponent<EasySchedule>().linkedSurvivor = GetComponent<SurvivorInfo>();
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].text = "";
                Training training = ((Training[])Enum.GetValues(typeof(Training)))[i + 1];
                if (survivor.assignedTraining == training) texts[i].text += "¢º";
                texts[i].text += new LocalizedString("Basic", $"Training:{training}").GetLocalizedString();

                bool trainable = GameManager.Instance.OutGameUIManager.Trainable(survivor, training);
                texts[i].GetComponentInParent<Button>().enabled = trainable;
                if (trainable) texts[i].GetComponentInParent<Image>().color = Color.white;
                else texts[i].GetComponentInParent<Image>().color = new Color(1, 0.6467f, 0.6467f);
            }
        }
    }
}

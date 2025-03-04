using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Calendar : CustomObject
{
    static int today = 1;
    public static int Today
    {
        get { return today; }
        set
        {
            today = value;
        }
    }
    public static int Week{ get { return 1 + (today - 1) / 7; } }
    public static int Month { get { return 1 + (today - 1) / 28; } }
    public static int Year { get { return 2101 + (Month - 1) / 12; } }

    string[] monthName = { 
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    int calendarPage = 1;
    int CalendarPage
    {
        get { return calendarPage; }
        set
        {
            calendarPage = value;
            monthText.text = monthName[(calendarPage - 1) % 12];
            yearText.text = $"{2101 + (calendarPage - 1) / 12}";
            weeksText[0].text = $"{(calendarPage - 1) * 4 + 1}";
            weeksText[1].text = $"{(calendarPage - 1) * 4 + 2}";
            weeksText[2].text = $"{(calendarPage - 1) * 4 + 3}";
            weeksText[3].text = $"{(calendarPage - 1) * 4 + 4}";
            for (int i = 0; i < 28; i++)
            {
                datesGone[i].SetActive((calendarPage - 1) * 28 + i < today - 1);
                if(i % 7 == 5) datesEvent[i].sprite = ResourceManager.Get(ResourceEnum.Sprite.BronzeLeague);
            }
        }
    }

    [Header("Calendar UI")]
    [SerializeField] TextMeshProUGUI monthText;
    [SerializeField] TextMeshProUGUI yearText;
    [SerializeField] TextMeshProUGUI[] weeksText;
    [SerializeField] GameObject[] dates;
    [SerializeField] GameObject[] datesGone;
    [SerializeField] Image[] datesEvent;

    protected override void Start()
    {
        base.Start();
        datesGone = new GameObject[28];
        datesEvent = new Image[28];
    }

    public override void MyStart()
    {
        for (int i = 0; i < dates.Length; i++)
        {
            datesGone[i] = dates[i].transform.Find("Gone").gameObject;
            datesEvent[i] = dates[i].transform.Find("Event").GetComponent<Image>();
        }
        TurnPageCalendar(0);
    }

    public void TurnPageCalendar(int value)
    {
        CalendarPage = Mathf.Clamp(calendarPage + value, 1, 36);
    }
}

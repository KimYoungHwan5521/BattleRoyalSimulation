using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum League { None, BronzeLeague, SilverLeague, GoldLeague, SeasonChampionship, WorldChampionship  };

public class Calendar : CustomObject
{
    class LeagueReserveData
    {
        public League league;
        public SurvivorData reserver;

        public LeagueReserveData(League league)
        {
            this.league = league;
        }
    }

    Dictionary<int, LeagueReserveData> leagueReserveInfo = new();

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
                if (leagueReserveInfo.ContainsKey(i + 28 * (calendarPage - 1)))
                {
                    if (Enum.TryParse(leagueReserveInfo[i + 28 * (calendarPage - 1)].league.ToString(), out ResourceEnum.Sprite result))
                    {
                        datesEvent[i].sprite = ResourceManager.Get(result);
                    }
                    else Debug.LogWarning($"Can't find sprite : {leagueReserveInfo[i + 28 * (calendarPage - 1)].league}");
                    reserved[i].SetActive(leagueReserveInfo[(calendarPage - 1) * 28 + i].reserver != null);
                }
            }
        }
    }

    OutGameUIManager outGameUIManager;

    [Header("Calendar UI")]
    [SerializeField] TextMeshProUGUI monthText;
    [SerializeField] TextMeshProUGUI yearText;
    [SerializeField] TextMeshProUGUI[] weeksText;
    [SerializeField] GameObject[] dates;
    GameObject[] datesGone;
    Image[] datesEvent;
    GameObject[] reserved;

    [Header("Reserve Battle Royale")]
    [SerializeField] GameObject reserveForm;
    [SerializeField] TextMeshProUGUI reserveText;
    [SerializeField] TMP_Dropdown survivorWhoParticipateInBattleRoyaleDropdown;
    [SerializeField] GameObject reserveButton;
    [SerializeField] GameObject reserveCancelButton;
    int wantReserveDate;
    SurvivorData wantReserver;
    
    protected override void Start()
    {
        base.Start();
        outGameUIManager = GetComponent<OutGameUIManager>();
        datesGone = new GameObject[28];
        datesEvent = new Image[28];
        reserved = new GameObject[28];

        for(int i=0; i<1008; i++)
        {
            if (i % 336 == 335)
            {
                leagueReserveInfo.Add(i, new(League.WorldChampionship));
            }
            else if(i % 84 == 83)
            {
                leagueReserveInfo.Add(i, new(League.SeasonChampionship));
            }
            
            if(i % 28 == 27)
            {
                if(!leagueReserveInfo.ContainsKey(i))
                {
                    leagueReserveInfo.Add(i, new(League.GoldLeague));
                }
                else
                {
                    leagueReserveInfo.Add(i - 1, new(League.GoldLeague));
                }
            }

            if (i % 14 == 13)
            {
                if (leagueReserveInfo.ContainsKey(i))
                {
                    if (!leagueReserveInfo.ContainsKey(i - 1))
                    {
                        leagueReserveInfo.Add(i - 1, new(League.SilverLeague));
                    }
                }
                else
                {
                    leagueReserveInfo.Add(i, new(League.SilverLeague));
                }
            }

            if ((i - 1) % 7 == 5 && !leagueReserveInfo.ContainsKey(i - 1))
            {
                leagueReserveInfo.Add(i - 1, new(League.BronzeLeague));
            }
        }
    }

    public override void MyStart()
    {
        for (int i = 0; i < dates.Length; i++)
        {
            datesGone[i] = dates[i].transform.Find("Gone").gameObject;
            datesEvent[i] = dates[i].transform.Find("Event").GetComponent<Image>();
            reserved[i] = datesEvent[i].transform.GetChild(0).gameObject;
        }
        TurnPageCalendar(0);
    }

    public void TurnPageCalendar(int value)
    {
        CalendarPage = Mathf.Clamp(calendarPage + value, 1, 36);
    }

    public void OpenReserveBattleRoyaleForm(int date)
    {
        wantReserveDate = date + 28 * (calendarPage - 1);
        if (leagueReserveInfo.ContainsKey(date + 28 * (calendarPage - 1)))
        {
            if (leagueReserveInfo[date + 28 * (calendarPage - 1)].reserver == null)
            {
                reserveText.text = "Choose who want participate in battle royale.";
                SetBattleRoyaleReserveBox();
                survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(true);
                reserveButton.SetActive(true);
                reserveCancelButton.SetActive(false);
            }
            else
            {
                reserveText.text = $"The Battle Royale for that date has been booked by \"{leagueReserveInfo[date + 28 * (calendarPage - 1)].reserver.survivorName}\"";
                survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(false);
                reserveButton.SetActive(false);
                reserveCancelButton.SetActive(true);
            }
            reserveForm.SetActive(true);
        }
    }

    public void SetBattleRoyaleReserveBox()
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(outGameUIManager.SurvivorsDropdown.options);
        wantReserver = outGameUIManager.MySurvivorsData.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
    }

    public void OnSurvivorParticipatingInBattleRoyaleSelected()
    {
        wantReserver = outGameUIManager.MySurvivorsData.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
    }

    public void ReserveBattleRoyale()
    {
        if(!wantReserver.isReserved)
        {
            leagueReserveInfo[wantReserveDate].reserver = wantReserver;
            wantReserver.isReserved = true;
            TurnPageCalendar(0);
        }
        else
        {
            outGameUIManager.Alert($"\"{wantReserver.survivorName}\" already has other reservations. " +
                $"\nLeagues other than the Championship can be reserved one at a time.");
        }
    }

    public void CancelReservation()
    {
        leagueReserveInfo[wantReserveDate].reserver.isReserved = false;
        leagueReserveInfo[wantReserveDate].reserver = null;
        TurnPageCalendar(0);
    }
}

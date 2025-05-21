using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum League { None, BronzeLeague, SilverLeague, GoldLeague, SeasonChampionship, WorldChampionship  };
public class LeagueReserveData
{
    public League league;
    public ResourceEnum.Prefab map;
    public int itemPool;
    public SurvivorData reserver;

    public LeagueReserveData(League league, ResourceEnum.Prefab map, int itemPool)
    {
        this.league = league;
        this.map = map;
        this.itemPool = itemPool;
    }
}

public class Calendar : CustomObject
{
    Dictionary<int, LeagueReserveData> leagueReserveInfo = new();
    public Dictionary<int, LeagueReserveData> LeagueReserveInfo => leagueReserveInfo;

    public LeagueReserveData NeareastSeasonChampionship
    {
        get
        {
            for(int i = Today; i <1008; i++)
            {
                if(i % 112 == 83) return leagueReserveInfo[i];
            }
            return null;
        }
    }

    public LeagueReserveData NeareastWorldChampionship
    {
        get
        {
            for (int i = Today; i < 1008; i++)
            {
                if (i % 336 == 335) return leagueReserveInfo[i];
            }
            return null;
        }
    }

    int today = 0;
    public int Today
    {
        get { return today; }
        set
        {
            if (leagueReserveInfo.ContainsKey(today) && leagueReserveInfo[today].reserver != null)
            {
                leagueReserveInfo[today].reserver.isReserved = false;
                leagueReserveInfo[today].reserver = null;
            }

            today = value;
            todayText.text = $"{monthName[Month - 1]} {(today % 28) + 1}, {Year}, ({dateName[today % 7]})";
            if(value > 0)
            {
                outGameUIManager.SurvivorsRecovery();
                outGameUIManager.ResetHireMarket();
                if (value % 336 == 0) AddLeagueReserveInfo(1);
            }

        }
    }
    public int Month { get { return 1 + today / 28; } }
    public int Year { get { return 2101 + (Month - 1) / 12; } }

    readonly string[] monthName = { 
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    readonly string[] dateName =
    {
        "Mon", "Tue", "WED", "THU", "FRI", "<color=blue>SAT</color>", "<color=red>SUN</color>"
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
            for (int i = 0; i < 28; i++)
            {
                datesGone[i].SetActive((calendarPage - 1) * 28 + i < today);
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
    [SerializeField] GameObject[] dates;
    GameObject[] datesGone;
    Image[] datesEvent;
    GameObject[] reserved;

    [Header("Global UI")]
    [SerializeField] TextMeshProUGUI todayText;

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
        AddLeagueReserveInfo(3);
    }

    int curMaxYear = 0;
    void AddLeagueReserveInfo(int howManyYears)
    {
        for (int i = curMaxYear * 336; i < (curMaxYear + howManyYears) * 3 * 336; i++)
        {
            if (i % 336 == 0)
            {
                leagueReserveInfo.Add(i, new(League.WorldChampionship, ResourceEnum.Prefab.Map_5x5_01, 4));
            }

            if (i % 112 == 83)
            {
                leagueReserveInfo.Add(i, new(League.SeasonChampionship, ResourceEnum.Prefab.Map_5x5_01, 3));
            }

            if (i % 28 == 27)
            {
                if (!leagueReserveInfo.ContainsKey(i))
                {
                    ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_4x4_01, (int)ResourceEnum.Prefab.Map_5x5_01);
                    leagueReserveInfo.Add(i, new(League.GoldLeague, map, 2));
                }
                else if (leagueReserveInfo[i].league != League.SeasonChampionship)
                {
                    ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_4x4_01, (int)ResourceEnum.Prefab.Map_5x5_01);
                    leagueReserveInfo.Add(i - 1, new(League.GoldLeague, map, 2));
                }
            }

            if (i % 14 == 13)
            {
                if (leagueReserveInfo.ContainsKey(i))
                {
                    if (!leagueReserveInfo.ContainsKey(i - 1))
                    {
                        ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_3x3_01, (int)ResourceEnum.Prefab.Map_4x4_01);
                        leagueReserveInfo.Add(i - 1, new(League.SilverLeague, map, 1));
                    }
                }
                else
                {
                    ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_3x3_01, (int)ResourceEnum.Prefab.Map_4x4_01);
                    leagueReserveInfo.Add(i, new(League.SilverLeague, map, 1));
                }
            }

            if ((i - 1) % 7 == 5 && !leagueReserveInfo.ContainsKey(i - 1))
            {
                ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_2x2_01, (int)ResourceEnum.Prefab.Map_3x3_01);
                leagueReserveInfo.Add(i - 1, new(League.BronzeLeague, map, 0));
            }
        }
        curMaxYear += howManyYears;
    }

    public override void MyStart()
    {
        for (int i = 0; i < dates.Length; i++)
        {
            datesGone[i] = dates[i].transform.Find("Gone").gameObject;
            datesEvent[i] = dates[i].transform.Find("Event").GetComponent<Image>();
            reserved[i] = datesEvent[i].transform.GetChild(0).gameObject;
            TextMeshProUGUI dateText = dates[i].GetComponentInChildren<TextMeshProUGUI>();
            dateText.text = $"{i + 1}";
            dateText.raycastTarget = false;
        }
        Today = 0;
        TurnPageCalendar(0);
    }

    public void TurnPageCalendar(int value)
    {
        CalendarPage = Mathf.Clamp(calendarPage + value, 1, 36);
    }

    public void OpenReserveBattleRoyaleForm(int date)
    {
        wantReserveDate = date + 28 * (calendarPage - 1);
        if (leagueReserveInfo.ContainsKey(wantReserveDate))
        {
            if(today == wantReserveDate)
            {
                outGameUIManager.OpenConfirmWindow("Go battle royale?", 
                    () => {
                        //outGameUIManager.StartBattleRoyale(leagueReserveInfo[wantReserveDate].reserver);
                        outGameUIManager.OpenBettingRoom();
                        outGameUIManager.calendarObject.SetActive(false);
                    });
            }
            else
            {
                if (leagueReserveInfo[wantReserveDate].league == League.SeasonChampionship || leagueReserveInfo[wantReserveDate].league == League.WorldChampionship)
                {
                    outGameUIManager.Alert("Championships cannot register. If you win the Gold League, you will be registered for the Season Championship automatically, and if you win the Season Championship, you will be registered for the World Championship automatically.");
                }
                else
                {
                    if (leagueReserveInfo[date + 28 * (calendarPage - 1)].reserver == null)
                    {
                        reserveText.text = "Choose who want register for battle royale.";
                        SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
                        survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(true);
                        reserveButton.SetActive(true);
                        reserveCancelButton.SetActive(false);
                    }
                    else
                    {
                        reserveText.text = $"The registered participant for that day is \"{leagueReserveInfo[wantReserveDate].reserver.survivorName}\"";
                        survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(false);
                        reserveButton.SetActive(false);
                        reserveCancelButton.SetActive(true);
                    }
                    reserveForm.SetActive(true);
                }

            }
        }
    }

    public void SetBattleRoyaleReserveBox(Tier tier)
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        //survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(outGameUIManager.SurvivorsDropdown.options);
        List<SurvivorData> allSurvivor = outGameUIManager.MySurvivorsData;
        for(int i=0; i< allSurvivor.Count; i++)
            if (allSurvivor[i].tier == tier) survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { allSurvivor[i].survivorName }));
        if(survivorWhoParticipateInBattleRoyaleDropdown.options.Count < 1)
        {
            survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { "[No qualified survivors]" }));
            reserveButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            wantReserver = allSurvivor.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
            reserveButton.GetComponent<Button>().interactable = true;
        }
    }

    public void OnSurvivorParticipatingInBattleRoyaleSelected()
    {
        wantReserver = outGameUIManager.MySurvivorsData.Find(x => x.survivorName == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
    }

    public void ReserveBattleRoyale()
    {
        if (wantReserver.tier != GetNeedTier(leagueReserveInfo[wantReserveDate].league))
        {
            outGameUIManager.Alert($"{wantReserver.survivorName}'s tier does not match this league.\n" +
                $"({wantReserver.survivorName}'s tier : {wantReserver.tier}, league need tier : {GetNeedTier(leagueReserveInfo[wantReserveDate].league)})");
        }
        else if (wantReserver.isReserved)
        {
            outGameUIManager.Alert($"\"{wantReserver.survivorName}\" is already registered for another match.\n" +
                $"Leagues other than the Championship can register one at a time.");
        }
        else if (wantReserver.injuries.Count > 0)
        {
            bool availiable = true;
            bool injured = false;
            foreach (Injury injury in wantReserver.injuries)
            {
                if(injury.site == InjurySite.Organ && injury.degree >= 1)
                {
                    availiable = false;
                    break;
                }
                if (injury.degree > 0)
                {
                    injured = true;
                }
            }

            if (!availiable)
            {
                outGameUIManager.Alert("He can't register battle royale.\n<color=red><i>(Cause : Organ Rupture)</i></color>");
            }
            else if (injured)
            {
                outGameUIManager.OpenConfirmWindow($"{wantReserver.survivorName} is injured. Should he still register for Battle Royale?", () =>
                {
                    Reserve();
                });
            }
            else Reserve();
        }
        else Reserve();
    }

    public void Reserve()
    {
        leagueReserveInfo[wantReserveDate].reserver = wantReserver;
        wantReserver.isReserved = true;
        TurnPageCalendar(0);
        outGameUIManager.Alert("Battle royale has been registered.");
    }

    public Tier GetNeedTier(League league)
    {
        switch(league)
        {
            case League.BronzeLeague:
                return Tier.Bronze;
            case League.SilverLeague:
                return Tier.Silver;
            case League.GoldLeague:
                return Tier.Gold;
            default:
                return Tier.Gold;
        }
    }

    public void CancelReservation()
    {
        leagueReserveInfo[wantReserveDate].reserver.isReserved = false;
        leagueReserveInfo[wantReserveDate].reserver = null;
        TurnPageCalendar(0);
    }

    public void LoadLeagueReserveInfo(Dictionary<int, LeagueReserveData> data)
    {
        leagueReserveInfo.Clear();
        leagueReserveInfo = data;
    }

    public void LoadToday(int data)
    {
        today = data;
    }
}

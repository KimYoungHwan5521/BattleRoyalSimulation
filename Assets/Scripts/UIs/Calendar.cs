using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public enum League { None, BronzeLeague, SilverLeague, GoldLeague, SeasonChampionship, WorldChampionship, MeleeLeague, RangeLeague, CraftingLeague };
public class LeagueReserveData
{
    public League league;
    public ResourceEnum.Prefab map;
    public int itemPool;
    public SurvivorData reserver;

    public LeagueReserveData(League league, ResourceEnum.Prefab map)
    {
        this.league = league;
        this.map = map;
        SetItemPool(league);
    }

    public void SetItemPool(League league)
    {
        itemPool = league switch
        {
            League.BronzeLeague => 0,
            League.SilverLeague => 1,
            League.GoldLeague => 2,
            League.SeasonChampionship => 3,
            League.WorldChampionship => 4,
            League.MeleeLeague => 5,
            League.RangeLeague => 6,
            League.CraftingLeague => 7,
            _ => 4
        };
    }
}

public class Calendar : CustomObject
{
    [SerializeField] GameObject scheduleByEachSurvivor;
    [SerializeField] GameObject[] schedules;
    [SerializeField] GameObject tip;

    Dictionary<int, LeagueReserveData> leagueReserveInfo = new();
    public Dictionary<int, LeagueReserveData> LeagueReserveInfo => leagueReserveInfo;

    public LeagueReserveData NeareastSeasonChampionship
    {
        get
        {
            for (int i = today + 1; i < today + 83; i++)
            {
                if (leagueReserveInfo.ContainsKey(i) && leagueReserveInfo[i].league == League.SeasonChampionship) return leagueReserveInfo[i];
            }
            return null;
        }
    }

    public int NeareastSeasonChampionshipDate
    {
        get
        {
            for (int i = today + 1; i < today + 83; i++)
            {
                if (leagueReserveInfo.ContainsKey(i) && leagueReserveInfo[i].league == League.SeasonChampionship) return i;
            }
            return -1;
        }
    }

    public LeagueReserveData NeareastWorldChampionship
    {
        get
        {
            for (int i = today + 1; i < today + 83; i++)
            {
                if (leagueReserveInfo.ContainsKey(i) && leagueReserveInfo[i].league == League.WorldChampionship) return leagueReserveInfo[i];
            }
            return null;
        }
    }

    public int NeareastWorldChampionshipDate
    {
        get
        {
            for (int i = today + 1; i < today + 83; i++)
            {
                if (leagueReserveInfo.ContainsKey(i) && leagueReserveInfo[i].league == League.SeasonChampionship) return i;
            }
            return -1;
        }
    }

    int today = 0;
    public int Today
    {
        get { return today; }
        set
        {
            today = value;
            //tip.SetActive(leagueReserveInfo.ContainsKey(value) && leagueReserveInfo[value].reserver != null);
            //outGameUIManager.scheduleAnim.SetBool("Tutorial", outGameUIManager.tutorial && leagueReserveInfo.ContainsKey(value));
            //if (outGameUIManager.tutorial && leagueReserveInfo.ContainsKey(value)) outGameUIManager.Alert("Click today's league in the schedule to go to the Battle Royale.");
            if (leagueReserveInfo.ContainsKey(value) && leagueReserveInfo[value].reserver != null)
            {
                if (today == 24 || today == 53 || today == 81) outGameUIManager.Alert("Alert:Last Chance For Objective");
                else outGameUIManager.Alert("Alert:Reserve Reminder");
            }

            
            if (leagueReserveInfo.ContainsKey(value - 1) && leagueReserveInfo[value - 1].reserver != null)
            {
                leagueReserveInfo[value - 1].reserver.isReserved = false;
                leagueReserveInfo[value - 1].reserver.reservedDate = -1;
                leagueReserveInfo[value - 1].reserver = null;
            }
            participationConfirmed = false;

            string localizedMonth = new LocalizedString("Basic", monthName[(Month - 1) % 12]).GetLocalizedString();
            string localizedDateName = new LocalizedString("Basic", dateName[today % 7]).GetLocalizedString();
            LocalizedString date = new("Basic", "Date Format");
            date.Arguments = new[] { Year.ToString(), localizedMonth, (today % 28 + 1).ToString(), localizedDateName };
            todayText.text = date.GetLocalizedString();
            GameManager.Instance.FixLayout(todayText.transform.parent.GetComponent<RectTransform>());
            //outGameUIManager.HideEndTheWeekend(value % 7 > 4);
            if (value > 0)
            {
                outGameUIManager.SurvivorsRecovery();
                outGameUIManager.ResetHireMarket();
                //if (value % 336 == 0) AddLeagueReserveInfo(1);
            }

            //if (leagueReserveInfo.ContainsKey(value) && (outGameUIManager.contestantsData == null || outGameUIManager.contestantsData.Count == 0)) outGameUIManager.SetContestants();

            //if(today == 21 && outGameUIManager.MySurvivorsData[0].tier == Tier.Bronze)
            //{
            //    SetLeagueInfo(outGameUIManager.MySurvivorsData[0], 24);
            //    outGameUIManager.Alert("Alert:Auto Reserve For Objective");
            //}
            //else if(today == 49 && outGameUIManager.MySurvivorsData[0].tier == Tier.Silver)
            //{
            //    SetLeagueInfo(outGameUIManager.MySurvivorsData[0], 53);
            //    outGameUIManager.Alert("Alert:Auto Reserve For Objective");
            //}
            //else if(today == 77 && NeareastSeasonChampionship.reserver == null && NeareastWorldChampionship.reserver == null)
            //{
            //    SetLeagueInfo(outGameUIManager.MySurvivorsData[0], 81);
            //    outGameUIManager.Alert("Alert:Last Week Auto Reserve");
            //}

            // Auto save
            if(Today > 0) GameManager.Instance.Save(0);
        }
    }
    public int Month { get { return 1 + today / 28; } }
    public int Year { get { return 2101 + (Month - 1) / 12; } }

    public readonly string[] monthName = {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public readonly string[] dateName =
    {
        "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"
    };

    int calendarPage = 1;
    int CalendarPage
    {
        get { return calendarPage; }
        set
        {
            calendarPage = value;
            monthText.GetComponent<LocalizeStringEvent>().StringReference = new("Basic", monthName[(calendarPage - 1) % 12]);
            yearText.text = $"{2101 + (calendarPage - 1) / 12}";
            for (int i = 0; i < 28; i++)
            {
                datesGone[i].SetActive((calendarPage - 1) * 28 + i < today);
                if (leagueReserveInfo.ContainsKey(i + 28 * (calendarPage - 1)))
                {
                    datesEvent[i].sprite = LoadSprite(leagueReserveInfo[i + 28 * (calendarPage - 1)].league, LocalizationSettings.SelectedLocale.Identifier.Code);
                    //if (outGameUIManager.tutorial && i + 28 * (calendarPage - 1) == Today && datesAnimator[i].gameObject.activeInHierarchy) datesAnimator[i].SetBool("Tutorial", true);
                    //else if (datesAnimator[i] != null && datesAnimator[i].gameObject.activeInHierarchy) datesAnimator[i].SetBool("Tutorial", false);
                    if (leagueReserveInfo[(calendarPage - 1) * 28 + i].reserver != null)
                    {
                        reserved[i].SetActive(true);
                        reserved[i].GetComponent<Help>().SetDescriptionWithKey("Reserved:", leagueReserveInfo[(calendarPage - 1) * 28 + i].reserver.localizedSurvivorName.GetLocalizedString());
                    }
                    else reserved[i].SetActive(false);
                }
                else
                {
                    datesEvent[i].sprite = null;
                    reserved[i].SetActive(false);
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
    Animator[] datesAnimator;
    public GameObject CalendarObject => outGameUIManager.CalendarObject;
    [SerializeField] TextMeshProUGUI weeksText;
    [SerializeField] GameObject[] scr_dates;
    GameObject[] scr_datesGone;
    Image[] scr_datesEvent;

    [Header("Global UI")]
    [SerializeField] TextMeshProUGUI todayText;

    [Header("Reserve Battle Royale")]
    [SerializeField] GameObject reserveForm;
    [SerializeField] LocalizeStringEvent reserveText;
    [SerializeField] TMP_Dropdown survivorWhoParticipateInBattleRoyaleDropdown;
    [SerializeField] TextMeshProUGUI watchableLeagueTodayText;
    [SerializeField] GameObject reserveButton;
    [SerializeField] GameObject reserveCancelButton;
    [SerializeField] GameObject participateButton;
    [SerializeField] GameObject notParticipateButton;
    public bool participationConfirmed;
    int wantReserveDate;
    SurvivorData wantReserver;
    [SerializeField] Image minimapImage;
    [SerializeField] ScrollRect farmableItemsScrollRect;
    [SerializeField] TextMeshProUGUI farmableItemsText;
    //         itemPool,          itemType,      count
    public Dictionary<int, Dictionary<ItemManager.Items, int>> itemPool = new();

    [SerializeField] GameObject selectLeagueBG;
    [SerializeField] LocalizedDropdown selectLeagueDropdown;

    protected override void Start()
    {
        base.Start();
        outGameUIManager = GetComponent<OutGameUIManager>();
        datesGone = new GameObject[28];
        datesEvent = new Image[28];
        reserved = new GameObject[28];
        datesAnimator = new Animator[28];
        scr_datesGone = new GameObject[7];
        scr_datesEvent = new Image[7];
        AddLeagueReserveInfo(3);
        LoadItemPool();
    }

    public void ResetData()
    {
        Today = 0;
        curMaxYear = 0;
        leagueReserveInfo.Clear();
        AddLeagueReserveInfo(3);
    }

    public void ResetCalendar()
    {
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            leagueReserveInfo.Clear();
            AddLeagueReserveInfo(1);
        }
    }

    int curMaxYear = 0;
    public int CurMaxYear => curMaxYear;
    void AddLeagueReserveInfo(int howManyYears)
    {
        //for (int i = curMaxYear * 336; i < (curMaxYear + howManyYears) * 336; i++)
        //for (int i = 0; i < 84; i++)
        //{
        //    if (i % 7 == 3 && i < 27)
        //    {
        //        ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_2x2_01, (int)ResourceEnum.Prefab.Map_3x3_01);
        //        leagueReserveInfo[i] = new(League.BronzeLeague, map);
        //    }
        //    if (i % 7 == 4 && i < 55)
        //    {
        //        ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_3x3_01, (int)ResourceEnum.Prefab.Map_4x4_01);
        //        leagueReserveInfo[i] = new(League.SilverLeague, map);
        //    }
        //    if (i % 7 == 5 && i < 77 || i > 77 && i % 7 == 4)
        //    {
        //        ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_4x4_01, (int)ResourceEnum.Prefab.Map_5x5_01);
        //        leagueReserveInfo[i] = new(League.GoldLeague, map);
        //    }
        //    if (i % 28 == 27 && i < 77 || i > 77 && i % 7 == 5)
        //    {
        //        leagueReserveInfo[i] = new(League.SeasonChampionship, ResourceEnum.Prefab.Map_5x5_01);
        //    }
        //    if (i % 84 == 83)
        //    {
        //        leagueReserveInfo[i] = new(League.WorldChampionship, ResourceEnum.Prefab.Map_5x5_01);
        //    }
        //    if (i % 28 == 6)
        //    {
        //        leagueReserveInfo[i] = new(League.MeleeLeague, ResourceEnum.Prefab.Map_5x5_01);
        //    }
        //    else if (i % 28 == 13)
        //    {
        //        leagueReserveInfo[i] = new(League.RangeLeague, ResourceEnum.Prefab.Map_5x5_01);
        //    }
        //    else if (i % 28 == 20)
        //    {
        //        leagueReserveInfo[i] = new(League.CraftingLeague, ResourceEnum.Prefab.Map_5x5_01);
        //    }
        //}
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            ResourceEnum.Prefab map = (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_2x2_01, (int)ResourceEnum.Prefab.Map_3x3_01);
            leagueReserveInfo[6] = new(League.BronzeLeague, map);
        }
        else
        {

            curMaxYear += howManyYears;
        }
    }

    public void SetLeagueInfo(SurvivorData reserver, int reserveDate)
    {
        leagueReserveInfo[reserveDate].reserver = reserver;
        //League league = League.None;
        //if (leagueReserveInfo[reserveDate].league == League.MeleeLeague || leagueReserveInfo[reserveDate].league == League.RangeLeague || leagueReserveInfo[reserveDate].league == League.CraftingLeague)
        //    league = leagueReserveInfo[reserveDate].league;
        //else if (reserver.haveQualifyToParticipateInSeasonChampionship) league = League.SeasonChampionship;
        //else if (reserver.haveQualifyToParticipateInWorldChampionship) league = League.WorldChampionship;
        //else if (reserver.tier == Tier.Gold) league = League.GoldLeague;
        //else if (reserver.tier == Tier.Silver) league = League.SilverLeague;
        //else league = League.BronzeLeague;
        //leagueReserveInfo[reserveDate].league = league;
        //leagueReserveInfo[reserveDate].map = league switch
        //{ 
        //    League.BronzeLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_2x2_01, (int)ResourceEnum.Prefab.Map_3x3_01),
        //    League.SilverLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_3x3_01, (int)ResourceEnum.Prefab.Map_4x4_01),
        //    League.GoldLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_4x4_01, (int)ResourceEnum.Prefab.Map_5x5_01),
        //    _ => ResourceEnum.Prefab.Map_5x5_01
        //};
        //leagueReserveInfo[reserveDate].SetItemPool(league);
        TurnPageCalendar(0);
    }

    void LoadItemPool()
    {
        // Bronze League
        Dictionary<ItemManager.Items, int> items = new()
        {
            { ItemManager.Items.Components, 20 },
            { ItemManager.Items.Salvages, 40 },
            { ItemManager.Items.Chemicals, 10 },
            { ItemManager.Items.Gunpowder, 20 },
            { ItemManager.Items.Knife, 4 },
            { ItemManager.Items.Bat, 2 },
            { ItemManager.Items.LongSword, 1 },
            { ItemManager.Items.Revolver, 4 },
            { ItemManager.Items.Pistol, 4 },
            { ItemManager.Items.SubMachineGun, 2 },
            { ItemManager.Items.ShotGun, 2 },
            { ItemManager.Items.AssaultRifle, 1 },
            { ItemManager.Items.SniperRifle, 1 },
            { ItemManager.Items.Bullet_Revolver, 20 },
            { ItemManager.Items.Bullet_Pistol, 20 },
            { ItemManager.Items.Bullet_SubMachineGun, 20 },
            { ItemManager.Items.Bullet_ShotGun, 20 },
            { ItemManager.Items.Bullet_AssaultRifle, 10 },
            { ItemManager.Items.Bullet_SniperRifle, 10 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 4 },
            { ItemManager.Items.LowLevelBulletproofVest, 4 },
            { ItemManager.Items.BandageRoll, 10 },
        };
        itemPool.Add(0, items);
        // Silver League
        items = new()
        {
            { ItemManager.Items.Components, 50 },
            { ItemManager.Items.Salvages, 80 },
            { ItemManager.Items.Chemicals, 30 },
            { ItemManager.Items.Gunpowder, 40 },
            { ItemManager.Items.Knife, 9 },
            { ItemManager.Items.Dagger, 3 },
            { ItemManager.Items.Bat, 3 },
            { ItemManager.Items.LongSword, 3 },
            { ItemManager.Items.Shovel, 3 },
            { ItemManager.Items.Revolver, 8 },
            { ItemManager.Items.Pistol, 8 },
            { ItemManager.Items.SubMachineGun, 4 },
            { ItemManager.Items.ShotGun, 4 },
            { ItemManager.Items.AssaultRifle, 2 },
            { ItemManager.Items.SniperRifle, 2 },
            { ItemManager.Items.Bullet_Revolver, 40 },
            { ItemManager.Items.Bullet_Pistol, 40 },
            { ItemManager.Items.Bullet_SubMachineGun, 40 },
            { ItemManager.Items.Bullet_ShotGun, 40 },
            { ItemManager.Items.Bullet_AssaultRifle, 20 },
            { ItemManager.Items.Bullet_SniperRifle, 20 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 8 },
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 4 },
            { ItemManager.Items.LowLevelBulletproofVest, 8 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 4 },
            { ItemManager.Items.BandageRoll, 30 },
            { ItemManager.Items.BearTrap, 9 },
        };
        itemPool.Add(1, items);
        // Gold League
        items = new()
        {
            { ItemManager.Items.AdvancedComponent, 16 },
            { ItemManager.Items.Components, 100 },
            { ItemManager.Items.Salvages, 160 },
            { ItemManager.Items.Chemicals, 50 },
            { ItemManager.Items.Gunpowder, 100 },
            { ItemManager.Items.Knife, 16 },
            { ItemManager.Items.Dagger, 5 },
            { ItemManager.Items.Bat, 5 },
            { ItemManager.Items.LongSword, 8 },
            { ItemManager.Items.Shovel, 5 },
            { ItemManager.Items.Revolver, 12 },
            { ItemManager.Items.Pistol, 12 },
            { ItemManager.Items.SubMachineGun, 8 },
            { ItemManager.Items.ShotGun, 8 },
            { ItemManager.Items.SniperRifle, 4 },
            { ItemManager.Items.AssaultRifle, 4 },
            { ItemManager.Items.Bazooka, 4 },
            { ItemManager.Items.Bullet_Revolver, 60 },
            { ItemManager.Items.Bullet_Pistol, 60 },
            { ItemManager.Items.Bullet_SubMachineGun, 60 },
            { ItemManager.Items.Bullet_ShotGun, 60 },
            { ItemManager.Items.Bullet_AssaultRifle, 30 },
            { ItemManager.Items.Bullet_SniperRifle, 30 },
            { ItemManager.Items.Rocket_Bazooka, 30 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 12 },
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 4 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 1 },
            { ItemManager.Items.LowLevelBulletproofVest, 12 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 4 },
            { ItemManager.Items.HighLevelBulletproofVest, 1 },
            { ItemManager.Items.BandageRoll, 60 },
            { ItemManager.Items.BearTrap, 16 },
            { ItemManager.Items.LandMine, 8 },
            { ItemManager.Items.NoiseTrap, 4 },
            { ItemManager.Items.ChemicalTrap, 4 },
            { ItemManager.Items.ShrapnelTrap, 1 },
            { ItemManager.Items.ExplosiveTrap, 1 },
        };
        itemPool.Add(2, items);
        // Season Championship
        items = new()
        {
            { ItemManager.Items.AdvancedComponent, 25 },
            { ItemManager.Items.Components, 160 },
            { ItemManager.Items.Salvages, 250 },
            { ItemManager.Items.Chemicals, 100 },
            { ItemManager.Items.Gunpowder, 160 },
            { ItemManager.Items.Knife, 25 },
            { ItemManager.Items.Dagger, 10 },
            { ItemManager.Items.Bat, 10 },
            { ItemManager.Items.LongSword, 10 },
            { ItemManager.Items.Shovel, 10 },
            { ItemManager.Items.Revolver, 25 },
            { ItemManager.Items.Pistol, 25 },
            { ItemManager.Items.SubMachineGun, 12 },
            { ItemManager.Items.ShotGun, 12 },
            { ItemManager.Items.SniperRifle, 6 },
            { ItemManager.Items.AssaultRifle, 6 },
            { ItemManager.Items.Bazooka, 6 },
            { ItemManager.Items.Bullet_Revolver, 50 },
            { ItemManager.Items.Bullet_Pistol, 50 },
            { ItemManager.Items.Bullet_SubMachineGun, 50 },
            { ItemManager.Items.Bullet_ShotGun, 50 },
            { ItemManager.Items.Bullet_AssaultRifle, 25 },
            { ItemManager.Items.Bullet_SniperRifle, 25 },
            { ItemManager.Items.Rocket_Bazooka, 25 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 25 },
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 12 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 6 },
            { ItemManager.Items.LowLevelBulletproofVest, 25 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 12 },
            { ItemManager.Items.HighLevelBulletproofVest, 6 },
            { ItemManager.Items.BandageRoll, 100 },
            { ItemManager.Items.BearTrap, 25 },
            { ItemManager.Items.LandMine, 12 },
            { ItemManager.Items.NoiseTrap, 12 },
            { ItemManager.Items.ChemicalTrap, 12 },
            { ItemManager.Items.ShrapnelTrap, 6 },
            { ItemManager.Items.ExplosiveTrap, 6 },
        };
        itemPool.Add(3, items);
        // World Championship
        items = new()
        {
            { ItemManager.Items.AdvancedComponent, 37 },
            { ItemManager.Items.Components, 200 },
            { ItemManager.Items.Salvages, 300 },
            { ItemManager.Items.Chemicals, 125 },
            { ItemManager.Items.Gunpowder, 175 },
            { ItemManager.Items.Knife, 25 },
            { ItemManager.Items.Dagger, 10 },
            { ItemManager.Items.Bat, 10 },
            { ItemManager.Items.LongSword, 10 },
            { ItemManager.Items.Shovel, 10 },
            { ItemManager.Items.Revolver, 25 },
            { ItemManager.Items.Pistol, 25 },
            { ItemManager.Items.SubMachineGun, 15 },
            { ItemManager.Items.ShotGun, 15 },
            { ItemManager.Items.SniperRifle, 5 },
            { ItemManager.Items.AssaultRifle, 5 },
            { ItemManager.Items.Bazooka, 5 },
            { ItemManager.Items.Bullet_Revolver, 25 },
            { ItemManager.Items.Bullet_Pistol, 25 },
            { ItemManager.Items.Bullet_SubMachineGun, 25 },
            { ItemManager.Items.Bullet_ShotGun, 25 },
            { ItemManager.Items.Bullet_AssaultRifle, 12 },
            { ItemManager.Items.Bullet_SniperRifle, 12 },
            { ItemManager.Items.Rocket_Bazooka, 25 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 25 },
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 12 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 5 },
            { ItemManager.Items.LowLevelBulletproofVest, 25 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 12 },
            { ItemManager.Items.HighLevelBulletproofVest, 5 },
            { ItemManager.Items.BandageRoll, 100 },
            { ItemManager.Items.BearTrap, 50 },
            { ItemManager.Items.LandMine, 25 },
            { ItemManager.Items.NoiseTrap, 12 },
            { ItemManager.Items.ChemicalTrap, 25 },
            { ItemManager.Items.ShrapnelTrap, 12 },
            { ItemManager.Items.ExplosiveTrap, 12 },
        };
        itemPool.Add(4, items);
        // Melee League
        items = new()
        {
            { ItemManager.Items.Knife, 10 },
            { ItemManager.Items.Dagger, 10 },
            { ItemManager.Items.Bat, 10 },
            { ItemManager.Items.LongSword, 10 },
            { ItemManager.Items.Shovel, 10 },
            { ItemManager.Items.BandageRoll, 100 },
        };
        itemPool.Add(5, items);
        // Range League
        items = new()
        {
            { ItemManager.Items.Bow, 50 },
            { ItemManager.Items.AdvancedBow, 25 },
            { ItemManager.Items.Revolver, 25 },
            { ItemManager.Items.Pistol, 25 },
            { ItemManager.Items.SubMachineGun, 15 },
            { ItemManager.Items.ShotGun, 15 },
            { ItemManager.Items.SniperRifle, 5 },
            { ItemManager.Items.AssaultRifle, 5 },
            { ItemManager.Items.Bazooka, 5 },
            { ItemManager.Items.LASER, 1 },
            { ItemManager.Items.Arrow, 100 },
            { ItemManager.Items.Bullet_Revolver, 25 },
            { ItemManager.Items.Bullet_Pistol, 25 },
            { ItemManager.Items.Bullet_SubMachineGun, 25 },
            { ItemManager.Items.Bullet_ShotGun, 25 },
            { ItemManager.Items.Bullet_AssaultRifle, 12 },
            { ItemManager.Items.Bullet_SniperRifle, 12 },
            { ItemManager.Items.Rocket_Bazooka, 25 },
            { ItemManager.Items.LowLevelBulletproofHelmet, 25 },
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 12 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 5 },
            { ItemManager.Items.LegendaryBulletproofHelmet, 1 },
            { ItemManager.Items.LowLevelBulletproofVest, 25 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 12 },
            { ItemManager.Items.HighLevelBulletproofVest, 5 },
            { ItemManager.Items.LegendaryBulletproofVest, 1 },
            { ItemManager.Items.BandageRoll, 100 },
        };
        itemPool.Add(6, items);
        // Crafting League
        items = new()
        {
            { ItemManager.Items.AdvancedComponent, 10 },
            { ItemManager.Items.Components, 500 },
            { ItemManager.Items.Salvages, 800 },
            { ItemManager.Items.Chemicals, 300 },
            { ItemManager.Items.Gunpowder, 800 },
            { ItemManager.Items.BandageRoll, 100 },
        };
        itemPool.Add(7, items);
    }

    public override void MyStart()
    {
        for (int i = 0; i < dates.Length; i++)
        {
            datesGone[i] = dates[i].transform.Find("Gone").gameObject;
            datesEvent[i] = dates[i].transform.Find("Event").GetComponent<Image>();
            reserved[i] = datesEvent[i].transform.GetChild(0).gameObject;
            datesAnimator[i] = dates[i].GetComponentInChildren<Animator>(true);
            TextMeshProUGUI dateText = dates[i].GetComponentInChildren<TextMeshProUGUI>();
            dateText.text = $"{i + 1}";
            dateText.raycastTarget = false;
        }
        for(int i = 0; i < scr_dates.Length; i++)
        {
            scr_dates[i] = scr_dates[i].transform.Find("Gone").gameObject;
            scr_datesEvent[i] = scr_dates[i].transform.Find("Event").GetComponent<Image>();
        }
        Today = 0;
        TurnPageCalendar(0);
        bool colorChanged = false;
        GameManager.Instance.ObjectUpdate += () =>
        {
            if (survivorWhoParticipateInBattleRoyaleDropdown.IsExpanded)
            {
                if (!colorChanged)
                {
                    var dropdownSprites = survivorWhoParticipateInBattleRoyaleDropdown.transform.Find("Dropdown List").Find("Viewport").Find("Content").GetComponentsInChildren<NullClass>();
                    for (int i = 0; i < survivorWhoParticipateInBattleRoyaleDropdown.options.Count; i++)
                    {
                        bool exit = false;
                        SurvivorData targetSurvivorData = outGameUIManager.MySurvivorsData.Find(
                            x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[i].text.Split(" : ")[0]);
                        // 예외처리
                        if(targetSurvivorData == null)
                        {
                            Debug.LogError($"Can't find survivor name : {survivorWhoParticipateInBattleRoyaleDropdown.options[i].text.Split(" : ")[0]}");
                            break;
                        }

                        foreach (var injury in targetSurvivorData.injuries)
                        {
                            if ((injury.degree > 0 && injury.type != InjuryType.ArtificialPartsDamaged) || injury.degree >= 1)
                            {
                                dropdownSprites[i].GetComponent<Image>().color = new Color(1, 0.6467f, 0.6467f);
                                exit = true;
                                break;
                            }
                        }
                        if (!exit) dropdownSprites[i].GetComponent<Image>().color = Color.white;
                    }
                    colorChanged = true;
                }
            }
            else colorChanged = false;
        };

        selectLeagueDropdown.ClearOptions();
        selectLeagueDropdown.AddLocalizedOptions(new() { new("Basic", "MeleeLeague"), new("Basic", "RangeLeague"), new("Basic", "CraftingLeague") });

        Sprite sprite = ResourceManager.Get(ResourceEnum.Sprite.MeleeLeague);
        selectLeagueDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite); 
        sprite = ResourceManager.Get(ResourceEnum.Sprite.RangeLeague);
        selectLeagueDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);
        sprite = ResourceManager.Get(ResourceEnum.Sprite.CraftingLeague);
        selectLeagueDropdown.GetComponent<DropdownSpritesData>().sprites.Add(sprite);

        GameManager.Instance.ObjectUpdate += () =>
        {
            if (selectLeagueDropdown.dropdown.IsExpanded)
            {
                var dropdownSprites = selectLeagueDropdown.transform.Find("Dropdown List").GetComponentsInChildren<DropdownSprite>();
                for (int i = 0; i < selectLeagueDropdown.GetComponent<DropdownSpritesData>().sprites.Count; i++)
                {
                    Image image = dropdownSprites[i].GetComponent<Image>();
                    image.sprite = selectLeagueDropdown.GetComponent<DropdownSpritesData>().sprites[i];
                    if (image.sprite != null) image.GetComponent<AspectRatioFitter>().aspectRatio = image.sprite.textureRect.width / image.sprite.textureRect.height;
                }
            }
        };

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void OpenCalendar()
    {
        CalendarObject.SetActive(true);
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            SetSingleCareerRunCalendar();
        }
        else
        {
            CalendarPage = today / 28 + 1;
        }
        GameManager.Instance.openedWindows.Push(CalendarObject);
    }

    void SetSingleCareerRunCalendar()
    {
        weeksText.GetComponent<LocalizeStringEvent>().StringReference = new("Basic", "Weeks") { Arguments = new[] { $"{ Today % 7 + 1 }" } };
        League league = default;
        ResourceEnum.Prefab map;
        if (outGameUIManager.MySurvivorsData[0].haveQualifyToParticipateInSeasonChampionship)
        {
            if(Today / 7 > 11)
            {
                if(!leagueReserveInfo.ContainsKey(7 * (Today / 7)))
                {
                    league = League.SeasonChampionship;
                    map = ResourceEnum.Prefab.Map_5x5_01;

                    leagueReserveInfo.Add(7 * (Today / 7), new(league, map));
                    leagueReserveInfo.Add(1 + 7 * (Today / 7), new(league, map));
                    leagueReserveInfo.Add(2 + 7 * (Today / 7), new(league, map));

                    league = League.WorldChampionship;
                    leagueReserveInfo.Add(3 + 7 * (Today / 7), new(league, map));
                    leagueReserveInfo.Add(4 + 7 * (Today / 7), new(league, map));
                    leagueReserveInfo.Add(5 + 7 * (Today / 7), new(league, map));
                }
            }
            else
            {
                if (!leagueReserveInfo.ContainsKey(6 + 7 * (Today / 7)))
                {
                    league = League.MeleeLeague;
                    map = ResourceEnum.Prefab.Map_5x5_01;

                    leagueReserveInfo.Add(6 + 7 * (Today / 7), new(league, map));
                }
            }
        }
        else
        {
            if (!leagueReserveInfo.ContainsKey(6 + 7 * (Today / 7)))
            {
                league = outGameUIManager.MySurvivorsData[0].tier switch
                {
                    Tier.Bronze => League.BronzeLeague,
                    Tier.Silver => League.SilverLeague,
                    Tier.Gold or _ => League.GoldLeague,
                };
                map = league switch
                {
                    League.BronzeLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_2x2_01, (int)ResourceEnum.Prefab.Map_3x3_01),
                    League.SilverLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_3x3_01, (int)ResourceEnum.Prefab.Map_4x4_01),
                    League.GoldLeague => (ResourceEnum.Prefab)UnityEngine.Random.Range((int)ResourceEnum.Prefab.Map_4x4_01, (int)ResourceEnum.Prefab.Map_5x5_01),
                    _ => ResourceEnum.Prefab.Map_5x5_01
                };

                leagueReserveInfo.Add(6 + 7 * (Today / 7), new(league, map));
            }
        }

        for (int i = 0; i < 7; i++)
        {
            scr_datesGone[i].SetActive(i < today % 7);
            if(leagueReserveInfo.ContainsKey(i + 7 * (Today / 7)))
            {
                datesEvent[i].sprite = LoadSprite(leagueReserveInfo[i + 7 * (Today / 7)].league, LocalizationSettings.SelectedLocale.Identifier.Code);
            }
            else
            {
                datesEvent[i].sprite = null;
            }
        }
    }

    public void TurnPageCalendar(int value)
    {
        CalendarPage = Mathf.Clamp(calendarPage + value, 1, 3);
    }

    void OpenSelectLeagueForm()
    {
        selectLeagueBG.SetActive(true);
        GameManager.Instance.openedWindows.Push(selectLeagueBG);
    }

    public void SelectLeague()
    {
        outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale", () =>
        {
            selectLeagueBG.SetActive(false);
            CalendarObject.SetActive(false);
            leagueReserveInfo[Today].league = selectLeagueDropdown.dropdown.value switch
            {
                1 => League.MeleeLeague,
                2 => League.RangeLeague,
                3 or _ => League.CraftingLeague,
            };
            outGameUIManager.SkipBetting();
        });
    }

    public void OpenReserveBattleRoyaleForm(int date)
    {
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            if (leagueReserveInfo.ContainsKey(Today))
            {
                if (outGameUIManager.MySurvivorsData[0].haveQualifyToParticipateInSeasonChampionship)
                {
                    OpenSelectLeagueForm();
                }
                else
                {
                    outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale", () =>
                    {
                        CalendarObject.SetActive(false);
                        outGameUIManager.SkipBetting();
                    });
                }
            }
        }
        //wantReserveDate = date + 28 * (calendarPage - 1);
        //if(leagueReserveInfo.ContainsKey(wantReserveDate))
        //{
        //    if (leagueReserveInfo[wantReserveDate].league == League.SeasonChampionship || leagueReserveInfo[wantReserveDate].league == League.WorldChampionship)
        //    {
        //        if (leagueReserveInfo[wantReserveDate].reserver == null) outGameUIManager.Alert("Alert:Reserve Championship");
        //        else if (wantReserveDate == Today)
        //        {
        //            outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale", () =>
        //            {
        //                CalendarObject.SetActive(false);
        //                outGameUIManager.SkipBetting();
        //            });
        //        }
        //    }
        //    else if (CheckTier(outGameUIManager.MySurvivorsData[0].tier, leagueReserveInfo[wantReserveDate].league))
        //    {
        //        if(wantReserveDate == Today)
        //        {
        //            if(today == 24 || today == 53 || today > 77)
        //            {
        //                outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale", () =>
        //                {
        //                    CalendarObject.SetActive(false);
        //                    outGameUIManager.SkipBetting();
        //                });
        //            }
        //            else 
        //            {
        //                if(outGameUIManager.CheckHaveInjury(out int expectedDateOfFullyRecovery))
        //                {
        //                    outGameUIManager.Alert("Alert:Can't Battle Royale", outGameUIManager.MySurvivorsData[0].localizedSurvivorName.GetLocalizedString(), $"{expectedDateOfFullyRecovery}");
        //                }
        //                else
        //                {
        //                    outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale", () =>
        //                    {
        //                        CalendarObject.SetActive(false);
        //                        outGameUIManager.SkipBetting();
        //                    });
        //                }
        //            }
        //        }
        //        else if (leagueReserveInfo[wantReserveDate].reserver == null)
        //        {
        //            if (outGameUIManager.MySurvivorsData[0].isReserved)
        //            {
        //                outGameUIManager.OpenConfirmWindow($"Confirm:Rebook", () =>
        //                {
        //                    CancelAllReservation();
        //                    Reserve();
        //                }, outGameUIManager.MySurvivorsData[0].localizedSurvivorName.GetLocalizedString());
        //            }
        //            else
        //            {
        //                outGameUIManager.OpenConfirmWindow("Confirm:Reserve Battle Royale", () =>
        //                {
        //                    Reserve();
        //                });
        //            }
        //        }
        //        else
        //        {
        //            if (wantReserveDate >= 77 && Today >= 77) return;
        //            outGameUIManager.OpenConfirmWindow("Confirm:Cancel Reservation", () =>
        //            {
        //                CancelReservation();
        //            });
        //        }

        //    }
        //    else
        //    {
        //        outGameUIManager.Alert("Alert:Tier Not Match", outGameUIManager.MySurvivorsData[0].localizedSurvivorName.GetLocalizedString(), new LocalizedString("Basic", outGameUIManager.MySurvivorsData[0].tier.ToString()).GetLocalizedString());
        //    }
        //}



        //if (leagueReserveInfo.ContainsKey(wantReserveDate))
        //{
        //    if (today == wantReserveDate)
        //    {
        //        watchableLeagueTodayText.text = $"{new LocalizedString("Basic", "League you can spectate if you skip today").GetLocalizedString()} : {new LocalizedString("Basic", leagueReserveInfo[wantReserveDate].league.ToString()).GetLocalizedString()}";
        //        //if (leagueReserveInfo[Today].reserver != null || participationConfirmed || leagueReserveInfo[Today].league == League.SeasonChampionship || leagueReserveInfo[Today].league == League.WorldChampionship)
        //        if (leagueReserveInfo[Today].reserver != null || participationConfirmed)
        //        {
        //            outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale",
        //                () =>
        //                {
        //                    outGameUIManager.OpenBettingRoom();
        //                    outGameUIManager.calendarObject.SetActive(false);
        //                });
        //        }
        //        else
        //        {
        //            reserveForm.SetActive(true);
        //            GameManager.Instance.openedWindows.Push(reserveForm);
        //            reserveText.StringReference = new LocalizedString("Basic", "Select the survivor to participate in the battle royale.");
        //            reserveText.RefreshString();
        //            SetLeagueInfo(wantReserveDate);
        //            //SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
        //            SetBattleRoyaleReserveBox();
        //            survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(true);
        //            reserveButton.SetActive(false);
        //            reserveCancelButton.SetActive(false);
        //            participateButton.SetActive(true);
        //            notParticipateButton.SetActive(true);
        //        }
        //    }
        //    else
        //    {
        //        watchableLeagueTodayText.text = "";
        //        if (leagueReserveInfo[wantReserveDate].league == League.SeasonChampionship || leagueReserveInfo[wantReserveDate].league == League.WorldChampionship)
        //        {
        //            outGameUIManager.Alert("Alert:Reserve Championship");
        //        }
        //        else
        //        {
        //            reserveForm.SetActive(true);
        //            GameManager.Instance.openedWindows.Push(reserveForm);
        //            if (leagueReserveInfo[date + 28 * (calendarPage - 1)].reserver == null)
        //            {
        //                reserveText.StringReference = new LocalizedString("Basic", "Select a survivor to reserve.");
        //                reserveText.RefreshString();
        //                SetLeagueInfo(wantReserveDate);
        //                //SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
        //                SetBattleRoyaleReserveBox();
        //                survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(true);
        //                reserveButton.SetActive(true);
        //                reserveCancelButton.SetActive(false);
        //                participateButton.SetActive(false);
        //                notParticipateButton.SetActive(false);
        //            }
        //            else
        //            {
        //                var temp = new LocalizedString("Basic", "Reserved Survivor");
        //                temp.Arguments = new[] { new { param0 = leagueReserveInfo[wantReserveDate].reserver.localizedSurvivorName.GetLocalizedString() } };
        //                reserveText.StringReference = temp;
        //                reserveText.RefreshString();
        //                SetLeagueInfo(wantReserveDate);
        //                reserveText.GetComponent<LocalizeStringEvent>().RefreshString();
        //                survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(false);
        //                reserveButton.SetActive(false);
        //                reserveCancelButton.SetActive(true);
        //                participateButton.SetActive(false);
        //                notParticipateButton.SetActive(false);
        //            }
        //        }
        //    }
        //}
    }

    bool CheckTier(Tier tier, League league)
    {
        if (league == League.MeleeLeague || league == League.RangeLeague || league == League.CraftingLeague) return true;
        switch (tier)
        {
            case Tier.Bronze:
                return league == League.BronzeLeague;
            case Tier.Silver:
                return league == League.SilverLeague;
            case Tier.Gold:
                return league == League.GoldLeague;
            default:
                return false;
        }
    }

    void SetLeagueInfo(int wantReserveDate)
    {
        string minimapName = $"Minimap{leagueReserveInfo[wantReserveDate].map.ToString()[3..]}";
        if (Enum.TryParse(minimapName, out ResourceEnum.Sprite sprite))
        {
            minimapImage.sprite = ResourceManager.Get(sprite);
        }
        else Debug.LogWarning($"Minimap not found : {minimapName}");

        farmableItemsText.text = string.Empty;
        foreach(var item in itemPool[leagueReserveInfo[wantReserveDate].itemPool])
        {
            farmableItemsText.text += $"{new LocalizedString("Item", item.Key.ToString()).GetLocalizedString()} x {item.Value},\n";
        }
        GameManager.Instance.FixLayout(farmableItemsText.GetComponent<RectTransform>());
        farmableItemsScrollRect.verticalNormalizedPosition = 1;
    }

    public void SetBattleRoyaleReserveBox()
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        League wantLeague;
        Sprite leagueSprite;
        List<SurvivorData> allSurvivor = outGameUIManager.MySurvivorsData;
        for (int i = 0; i < allSurvivor.Count; i++)
        {
            List<TMP_Dropdown.OptionData> options = new();
            if (leagueReserveInfo[wantReserveDate].league == League.MeleeLeague) wantLeague = League.MeleeLeague;
            else if (leagueReserveInfo[wantReserveDate].league == League.RangeLeague) wantLeague = League.RangeLeague;
            else if (leagueReserveInfo[wantReserveDate].league == League.CraftingLeague) wantLeague = League.CraftingLeague;
            //else if (allSurvivor[i].haveQualifyToParticipateInSeasonChampionship) wantLeague = League.SeasonChampionship;
            //else if (allSurvivor[i].haveQualifyToParticipateInWorldChampionship) wantLeague = League.WorldChampionship;
            else if (allSurvivor[i].tier == Tier.Gold) wantLeague = League.GoldLeague;
            else if (allSurvivor[i].tier == Tier.Silver) wantLeague = League.SilverLeague;
            else  wantLeague = League.BronzeLeague;

            if (Enum.TryParse<ResourceEnum.Sprite>($"{wantLeague}Untagged", out var result)) leagueSprite = ResourceManager.Get(result);
            else
            {
                Debug.LogWarning($"Can't find league sprite : {wantLeague}");
                leagueSprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
            }

            options.Add(new TMP_Dropdown.OptionData($"{allSurvivor[i].localizedSurvivorName.GetLocalizedString()} : {LocalizationSettings.StringDatabase.GetLocalizedString("Basic", wantLeague.ToString())}", leagueSprite));
            survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(options);
        }
        wantReserver = allSurvivor.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
        reserveButton.GetComponent<Button>().interactable = true;
        participateButton.GetComponent<Button>().interactable = true;

        survivorWhoParticipateInBattleRoyaleDropdown.captionText.text = survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text;
        survivorWhoParticipateInBattleRoyaleDropdown.captionImage.sprite = survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].image;
        OnSurvivorParticipatingInBattleRoyaleSelected();
    }

    public void SetBattleRoyaleReserveBox(Tier tier)
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        List<SurvivorData> allSurvivor = outGameUIManager.MySurvivorsData;
        if (leagueReserveInfo[wantReserveDate].league == League.MeleeLeague || leagueReserveInfo[wantReserveDate].league == League.RangeLeague || leagueReserveInfo[wantReserveDate].league == League.CraftingLeague)
        {
            for (int i = 0; i < allSurvivor.Count; i++)
                survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { allSurvivor[i].localizedSurvivorName.GetLocalizedString() }));
        }
        else
        {
            for (int i = 0; i < allSurvivor.Count; i++)
                if (allSurvivor[i].tier == tier) survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { allSurvivor[i].localizedSurvivorName.GetLocalizedString() }));
        }
        if (survivorWhoParticipateInBattleRoyaleDropdown.options.Count < 1)
        {
            survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { $"[{new LocalizedString("Basic", "No eligible survivor").GetLocalizedString()}]" }));
            reserveButton.GetComponent<Button>().interactable = false;
            participateButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            wantReserver = allSurvivor.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
            reserveButton.GetComponent<Button>().interactable = true;
            participateButton.GetComponent<Button>().interactable = true;
        }
        survivorWhoParticipateInBattleRoyaleDropdown.captionText.text = survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text;
    }

    public void OnSurvivorParticipatingInBattleRoyaleSelected()
    {
        wantReserver = outGameUIManager.MySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text.Split(" : ")[0]);
    }

    public void ReserveBattleRoyale()
    {
        if (outGameUIManager.MySurvivorsData[0].isReserved)
        {
            outGameUIManager.Alert($"Alert:Already Resistered", outGameUIManager.MySurvivorsData[0].localizedSurvivorName.GetLocalizedString());
            //outGameUIManager.OpenConfirmWindow($"Confirm:Rebook", () =>
            //{
            //    CancelAllReservation();
            //    Reserve();
            //}, outGameUIManager.MySurvivorsData[0].localizedSurvivorName.GetLocalizedString());
        }
        //else if (NeareastSeasonChampionship.reserver == wantReserver || NeareastWorldChampionship.reserver == wantReserver)
        //{
        //    outGameUIManager.OpenConfirmWindow("Confirm:Reserve Who Reserved In Championship", () =>
        //    {
        //        if (wantReserver.injuries.Count > 0) AskAboutInjury();
        //        else Reserve();
        //    });
        //}
        else if (outGameUIManager.MySurvivorsData[0].injuries.Count > 0) AskAboutInjury();
        else Reserve();
    }

    public void ParticipateBattleRoyale()
    {
        //if (NeareastSeasonChampionship.reserver == wantReserver || NeareastWorldChampionship.reserver == wantReserver)
        //{
        //    outGameUIManager.OpenConfirmWindow("Confirm:Reserve Who Reserved In Championship", () =>
        //    {
        //        if (wantReserver.injuries.Count > 0) AskAboutInjury(false);
        //        else Participate();
        //    });
        //}
        //else 
        if (wantReserver.injuries.Count > 0) AskAboutInjury(false);
        else Participate();
    }

    public void Reserve()
    {
        SetLeagueInfo(outGameUIManager.MySurvivorsData[0], wantReserveDate);
        outGameUIManager.MySurvivorsData[0].isReserved = true;
        outGameUIManager.MySurvivorsData[0].reservedDate = wantReserveDate;
        TurnPageCalendar(0);
        outGameUIManager.Alert("Alert:Battle royale reserved.");
    }

    void Participate()
    {
        outGameUIManager.OpenConfirmWindow("Confirm:Participate", () =>
        {
            Participation();
        });
    }

    void Participation()
    {
        outGameUIManager.contestantsData = null;
        participationConfirmed = true;
        SetLeagueInfo(wantReserver, Today);
        outGameUIManager.OpenBettingRoom();
        outGameUIManager.CalendarObject.SetActive(false);
        reserveForm.SetActive(false);
    }

    public void NotParticipateBattleRoyale()
    {
        outGameUIManager.OpenConfirmWindow("Confirm:Participate", () =>
        { 
            participationConfirmed = true;
            outGameUIManager.OpenBettingRoom();
            outGameUIManager.CalendarObject.SetActive(false);
            reserveForm.SetActive(false);
        });
    }

    // -1 : Not able, 0 : Injured but able, 1 : Able
    public int CapableBattleRoyale(SurvivorData survivor, out string cause)
    {
        cause = "";
        bool injured = false;
        int eyeInjury = 0;
        int handInjury = 0;
        foreach (Injury injury in survivor.injuries)
        {
            if (injury.site == InjurySite.Organ && injury.degree >= 1)
            {
                cause = $"{new LocalizedString("Injury", "Organ").GetLocalizedString()} {new LocalizedString("Injury", "Rupture").GetLocalizedString()}";
                return -1;
            }
            if (injury.site == InjurySite.LeftEye && injury.degree >= 1 || injury.site == InjurySite.RightEye && injury.degree >= 1) eyeInjury++;
            if ((injury.site == InjurySite.LeftHand || injury.site == InjurySite.LeftArm) && injury.degree >= 1 || (injury.site == InjurySite.RightHand || injury.site == InjurySite.RightArm) && injury.degree >= 1) handInjury++;
            if (eyeInjury >= 2)
            {
                cause = $"{new LocalizedString("Injury", "Blind in both eyes").GetLocalizedString()}";
                return -1;
            }
            if (handInjury >= 2)
            {
                cause = $"{new LocalizedString("Injury", "Cannot use both hands").GetLocalizedString()}";
                return -1;
            }
            if (injury.degree > 0 && injury.type != InjuryType.ArtificialPartsDamaged && injury.type != InjuryType.AugmentedPartsDamaged && injury.type != InjuryType.TranscendantPartsDamaged || injury.degree >= 1)
            {
                injured = true;
            }
        }
        return injured ? 0 : 1;
    }

    // reserve = false => participate
    void AskAboutInjury(bool reserve = true)
    {
        bool availiable = true;
        bool injured = false;
        string cause = "";
        int able = CapableBattleRoyale(wantReserver, out cause);
        availiable = able > -1;
        injured = able < 1;

        if (!availiable)
        {
            if(reserve) outGameUIManager.Alert("Alert:Reserve Fail", cause);
            else outGameUIManager.Alert("Alert:Participate Fail", cause);
        }
        else if (injured)
        {
            string askText = reserve ? "Confirm:Reserve Battle Royale Who Have Injury" : "Confirm:Participate Battle Royale Who Have Injury";
            UnityAction action = reserve? () => Reserve() : () => Participation();
            outGameUIManager.OpenConfirmWindow(askText, action, wantReserver.localizedSurvivorName.GetLocalizedString());
        }
        else
        {
            if(reserve) Reserve();
            else Participate();
        }
    }

    public Tier GetNeedTier(League league)
    {
        return league switch
        {
            League.BronzeLeague => Tier.Bronze,
            League.SilverLeague => Tier.Silver,
            League.GoldLeague => Tier.Gold,
            _ => Tier.Bronze,
        };
    }

    public void CancelReservation()
    {
        leagueReserveInfo[wantReserveDate].reserver.isReserved = false;
        leagueReserveInfo[wantReserveDate].reserver = null;
        TurnPageCalendar(0);
    }

    public void CancelAllReservation()
    {
        foreach(var (date, reserveInfo) in leagueReserveInfo)
        {
            if(reserveInfo.reserver != null) reserveInfo.reserver.isReserved |= false;
            reserveInfo.reserver = null;
        }
        outGameUIManager.contestantsData = null;
    }

    public void OpenScheduleByEachSurvivor()
    {
        for (int i = 0; i < schedules.Length; i++)
        {
            if (i < outGameUIManager.MySurvivorsData.Count)
            {
                SurvivorData survivor = outGameUIManager.MySurvivorsData[i];
                var scheduleTexts = schedules[i].GetComponentsInChildren<TextMeshProUGUI>();
                scheduleTexts[0].GetComponent<LocalizeStringEvent>().StringReference = survivor.localizedSurvivorName;
                scheduleTexts[1].GetComponent<LocalizeStringEvent>().StringReference = new("Basic", survivor.tier.ToString());
                if (survivor.isReserved)
                {
                    int date = survivor.reservedDate;
                    LocalizedString dateFormatYMD = new("Basic", "Date Format YMD");
                    dateFormatYMD.Arguments = new[] { $"{2101 + (date / 28) / 12}", $"{new LocalizedString("Basic", $"{monthName[(date / 28) % 12]}").GetLocalizedString()}", $"{date % 28 + 1}" };
                    scheduleTexts[2].text = $"{dateFormatYMD.GetLocalizedString()}\n({new LocalizedString("Basic", $"{leagueReserveInfo[date].league}").GetLocalizedString()})";
                    LocalizedString dateFormatYMD2 = new("Basic", "Date Format YMD");
                    dateFormatYMD2.Arguments = new[] { $"{2101 + (NeareastSeasonChampionshipDate / 28) / 12}", $"{new LocalizedString("Basic", $"{monthName[(NeareastSeasonChampionshipDate / 28) % 12]}").GetLocalizedString()}", $"{NeareastSeasonChampionshipDate % 28 + 1}" };
                    if (NeareastSeasonChampionship.reserver == survivor) scheduleTexts[2].text += $"\n{dateFormatYMD2.GetLocalizedString()}\n({new LocalizedString("Basic", "SeasonChampionship").GetLocalizedString()})";
                    else if (NeareastWorldChampionship.reserver == survivor) scheduleTexts[2].text += $"\n{dateFormatYMD2.GetLocalizedString()}\n({new LocalizedString("Basic", "WorldChampionship").GetLocalizedString()})";
                }
                else
                {
                    LocalizedString dateFormatYMD = new("Basic", "Date Format YMD");
                    dateFormatYMD.Arguments = new[] {$"{2101 + (NeareastSeasonChampionshipDate / 28) / 12}", $"{new LocalizedString("Basic", $"{monthName[(NeareastSeasonChampionshipDate / 28) % 12]}")}", $"{NeareastSeasonChampionshipDate % 28 + 1}" };
                    if (NeareastSeasonChampionship.reserver == survivor) scheduleTexts[2].text = $"{dateFormatYMD.GetLocalizedString()}\n({new LocalizedString("Basic", "SeasonChampionship").GetLocalizedString()})";
                    else if (NeareastWorldChampionship.reserver == survivor) scheduleTexts[2].text = $"{dateFormatYMD.GetLocalizedString()}\n({new LocalizedString("Basic", "WorldChampionship").GetLocalizedString()})";
                    else scheduleTexts[2].text = "-";
                }

                schedules[i].SetActive(true);
            }
            else schedules[i].SetActive(false);
        }
        scheduleByEachSurvivor.SetActive(true);
        GameManager.Instance.openedWindows.Push(scheduleByEachSurvivor);
    }

    public void CloseAll()
    {
        scheduleByEachSurvivor.SetActive(false);
        CalendarObject.SetActive(false);
        reserveForm.SetActive(false);
    }

    void OnLocaleChanged(Locale newLocale)
    {
        Today = today;
        if (!leagueReserveInfo.ContainsKey(wantReserveDate)) return;
        farmableItemsText.text = "";
        foreach (var item in itemPool[leagueReserveInfo[wantReserveDate].itemPool])
        {
            farmableItemsText.text += $"{new LocalizedString("Item", item.Key.ToString()).GetLocalizedString()} x {item.Value},\n";
        }
        if (leagueReserveInfo.ContainsKey(Today))
            watchableLeagueTodayText.text = $"{new LocalizedString("Basic", "League you can spectate if you skip today").GetLocalizedString()} : {new LocalizedString("Basic", leagueReserveInfo[wantReserveDate].league.ToString()).GetLocalizedString()}";
        else watchableLeagueTodayText.text = "";
        //SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
        //SetBattleRoyaleReserveBox();
        CalendarPage = calendarPage;
        OpenScheduleByEachSurvivor();
    }

    public IEnumerator LoadLeagueReserveInfo(Dictionary<int, LeagueReserveData> data)
    {
        GameManager.ClaimLoadInfo("Loading calendar...", 1, 3);
        leagueReserveInfo.Clear();
        leagueReserveInfo = data;
        yield return null;
    }

    public void LoadToday(int today, int curMaxYear, bool participationConfirmed)
    {
        Today = today;
        this.curMaxYear = curMaxYear;
        this.participationConfirmed = participationConfirmed;
        TurnPageCalendar(0);
    }

    Sprite LoadSprite(League league, string localeCode)
    {
        string code = localeCode switch
        {
            "zh-Hans" => "SC",
            "zh-Hant" => "TC",
            "fr-FR" => "FR",
            "de-DE" => "DE",
            "ja-JP" => "JP",
            "ko-KR" => "KR",
            "pt-BR" or "pt-PT" => "PT",
            "ru-RU" => "RU",
            "es-ES" or "es-MX" => "ES",
            _ => ""
        };

        //string leagueType = league switch
        //{ 
        //    League.MeleeLeague => "MeleeLeague",
        //    League.RangeLeague => "RangeLeague",
        //    League.CraftingLeague => "CraftingLeague",
        //    _ => "RegularLeague"
        //};
        string leagueType;
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun)
        {
            leagueType = league switch
            {
                League.MeleeLeague or League.RangeLeague or League.CraftingLeague => "EventLeague",
                _ => league.ToString()
            };
        }
        else
        {
            leagueType = league.ToString();
        }

        if (Enum.TryParse($"{leagueType}{code}", out ResourceEnum.Sprite result))
        {
            return ResourceManager.Get(result);
        }
        else
        {
            Debug.LogWarning($"Can't find sprite : {leagueType}");
            return default;
        }
    }
}

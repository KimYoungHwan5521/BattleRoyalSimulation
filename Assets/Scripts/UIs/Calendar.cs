using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    public LeagueReserveData(League league, ResourceEnum.Prefab map, int itemPool)
    {
        this.league = league;
        this.map = map;
        this.itemPool = itemPool;
    }
}

public class Calendar : CustomObject
{
    [SerializeField] GameObject scheduleByEachSurvivor;
    [SerializeField] GameObject[] schedules;

    Dictionary<int, LeagueReserveData> leagueReserveInfo = new();
    public Dictionary<int, LeagueReserveData> LeagueReserveInfo => leagueReserveInfo;

    public LeagueReserveData NeareastSeasonChampionship
    {
        get
        {
            for (int i = Today; i < 1008; i++)
            {
                if (i % 112 == 83) return leagueReserveInfo[i];
            }
            return null;
        }
    }

    public int NeareastSeasonChampionshipDate
    {
        get
        {
            for (int i = Today; i < 1008; i++)
            {
                if (i % 112 == 83) return i;
            }
            return -1;
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

    public int NeareastWorldChampionshipDate
    {
        get
        {
            for (int i = Today; i < 1008; i++)
            {
                if (i % 336 == 335) return i;
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
            if (leagueReserveInfo.ContainsKey(today) && leagueReserveInfo[today].reserver != null)
            {
                leagueReserveInfo[today].reserver.isReserved = false;
                leagueReserveInfo[today].reserver.reservedDate = -1;
                leagueReserveInfo[today].reserver = null;
            }

            today = value;
            string localizedMonth = new LocalizedString("Basic", monthName[Month - 1]).GetLocalizedString();
            string localizedDateName = new LocalizedString("Basic", dateName[today % 7]).GetLocalizedString();
            LocalizedString date = new("Basic", "Date Format");
            date.Arguments = new[] { Year.ToString(), localizedMonth, (today % 28 + 1).ToString(), localizedDateName };
            todayText.text = date.GetLocalizedString();
            if (value > 0)
            {
                outGameUIManager.SurvivorsRecovery();
                outGameUIManager.ResetHireMarket();
                if (value % 336 == 0) AddLeagueReserveInfo(1);
            }

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
            //monthText.text = monthName[(calendarPage - 1) % 12];
            monthText.GetComponent<LocalizeStringEvent>().StringReference = new("Basic", monthName[(calendarPage - 1) % 12]);
            yearText.text = $"{2101 + (calendarPage - 1) / 12}";
            for (int i = 0; i < 28; i++)
            {
                datesGone[i].SetActive((calendarPage - 1) * 28 + i < today);
                if (leagueReserveInfo.ContainsKey(i + 28 * (calendarPage - 1)))
                {
                    datesEvent[i].sprite = LoadSprite(leagueReserveInfo[i + 28 * (calendarPage - 1)].league, LocalizationSettings.SelectedLocale.Identifier.Code);
                    if (leagueReserveInfo[(calendarPage - 1) * 28 + i].reserver != null)
                    {
                        reserved[i].SetActive(true);
                        reserved[i].GetComponent<Help>().SetDescriptionWithKey("Reserved:", leagueReserveInfo[(calendarPage - 1) * 28 + i].reserver.localizedSurvivorName.GetLocalizedString());
                    }
                    else reserved[i].SetActive(false);
                }
                else datesEvent[i].sprite = null;
            }
        }
    }

    OutGameUIManager outGameUIManager;

    [Header("Calendar UI")]
    [SerializeField] GameObject calendarObject;
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
    [SerializeField] LocalizeStringEvent reserveText;
    [SerializeField] TMP_Dropdown survivorWhoParticipateInBattleRoyaleDropdown;
    [SerializeField] GameObject reserveButton;
    [SerializeField] GameObject reserveCancelButton;
    int wantReserveDate;
    SurvivorData wantReserver;
    [SerializeField] Image minimapImage;
    [SerializeField] ScrollRect farmableItemsScrollRect;
    [SerializeField] TextMeshProUGUI farmableItemsText;
    //         itemPool,          itemType,      count
    public Dictionary<int, Dictionary<ItemManager.Items, int>> itemPool = new();

    protected override void Start()
    {
        base.Start();
        outGameUIManager = GetComponent<OutGameUIManager>();
        datesGone = new GameObject[28];
        datesEvent = new Image[28];
        reserved = new GameObject[28];
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

    int curMaxYear = 0;
    public int CurMaxYear => curMaxYear;
    void AddLeagueReserveInfo(int howManyYears)
    {
        for (int i = curMaxYear * 336; i < (curMaxYear + howManyYears) * 3 * 336; i++)
        {
            if (i % 336 == 335)
            {
                leagueReserveInfo.Add(i, new(League.WorldChampionship, ResourceEnum.Prefab.Map_5x5_01, 4));
            }
            else if (i % 336 == 48)
            {
                leagueReserveInfo.Add(i, new(League.MeleeLeague, ResourceEnum.Prefab.Map_5x5_01, 5));
            }
            else if (i % 336 == 160)
            {
                leagueReserveInfo.Add(i, new(League.RangeLeague, ResourceEnum.Prefab.Map_5x5_01, 6));
            }
            else if (i % 336 == 272)
            {
                leagueReserveInfo.Add(i, new(League.CraftingLeague, ResourceEnum.Prefab.Map_5x5_01, 7));
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

    void LoadItemPool()
    {
        // Bronze League
        Dictionary<ItemManager.Items, int> items = new()
        {
            { ItemManager.Items.Components, 20 },
            { ItemManager.Items.Salvages, 40 },
            { ItemManager.Items.Chemicals, 10 },
            { ItemManager.Items.Gunpowder, 20 },
            { ItemManager.Items.Knife, 3 },
            { ItemManager.Items.Bat, 3 },
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
            { ItemManager.Items.Components, 40 },
            { ItemManager.Items.Salvages, 80 },
            { ItemManager.Items.Chemicals, 20 },
            { ItemManager.Items.Gunpowder, 40 },
            { ItemManager.Items.Knife, 3 },
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
            { ItemManager.Items.Components, 60 },
            { ItemManager.Items.Salvages, 120 },
            { ItemManager.Items.Chemicals, 30 },
            { ItemManager.Items.Gunpowder, 60 },
            { ItemManager.Items.Knife, 5 },
            { ItemManager.Items.Dagger, 5 },
            { ItemManager.Items.Bat, 5 },
            { ItemManager.Items.LongSword, 5 },
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
            { ItemManager.Items.Components, 80 },
            { ItemManager.Items.Salvages, 160 },
            { ItemManager.Items.Chemicals, 40 },
            { ItemManager.Items.Gunpowder, 80 },
            { ItemManager.Items.Knife, 10 },
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
            { ItemManager.Items.AdvancedComponent, 36 },
            { ItemManager.Items.Components, 100 },
            { ItemManager.Items.Salvages, 200 },
            { ItemManager.Items.Chemicals, 50 },
            { ItemManager.Items.Gunpowder, 100 },
            { ItemManager.Items.Knife, 10 },
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
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 15 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 5 },
            { ItemManager.Items.LowLevelBulletproofVest, 25 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 15 },
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
            { ItemManager.Items.MiddleLevelBulletproofHelmet, 15 },
            { ItemManager.Items.HighLevelBulletproofHelmet, 5 },
            { ItemManager.Items.LowLevelBulletproofVest, 25 },
            { ItemManager.Items.MiddleLevelBulletproofVest, 15 },
            { ItemManager.Items.HighLevelBulletproofVest, 5 },
            { ItemManager.Items.BandageRoll, 100 },
        };
        itemPool.Add(6, items);
        // Crafting League
        items = new()
        {
            { ItemManager.Items.AdvancedComponent, 50 },
            { ItemManager.Items.Components, 200 },
            { ItemManager.Items.Salvages, 400 },
            { ItemManager.Items.Chemicals, 100 },
            { ItemManager.Items.Gunpowder, 400 },
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
            TextMeshProUGUI dateText = dates[i].GetComponentInChildren<TextMeshProUGUI>();
            dateText.text = $"{i + 1}";
            dateText.raycastTarget = false;
        }
        Today = 0;
        TurnPageCalendar(0);
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void OpenCalendar()
    {
        calendarObject.SetActive(true);
        CalendarPage = today / 28 + 1;
        GameManager.Instance.openedWindows.Push(calendarObject);
    }

    public void TurnPageCalendar(int value)
    {
        CalendarPage = Mathf.Clamp(calendarPage + value, 1, curMaxYear * 3);
    }

    public void OpenReserveBattleRoyaleForm(int date)
    {
        wantReserveDate = date + 28 * (calendarPage - 1);
        if (leagueReserveInfo.ContainsKey(wantReserveDate))
        {
            if (today == wantReserveDate)
            {
                outGameUIManager.OpenConfirmWindow("Confirm:Go Battle Royale",
                    () =>
                    {
                        GameManager.Instance.Option.SetSaveButtonInteractable(false);
                        outGameUIManager.OpenBettingRoom();
                        outGameUIManager.calendarObject.SetActive(false);
                    });
            }
            else
            {
                if (leagueReserveInfo[wantReserveDate].league == League.SeasonChampionship || leagueReserveInfo[wantReserveDate].league == League.WorldChampionship)
                {
                    outGameUIManager.Alert("Alert:Reserve Championship");
                }
                else
                {
                    reserveForm.SetActive(true);
                    GameManager.Instance.openedWindows.Push(reserveForm);
                    if (leagueReserveInfo[date + 28 * (calendarPage - 1)].reserver == null)
                    {
                        reserveText.StringReference = new LocalizedString("Basic", "Select a survivor to reserve.");
                        reserveText.RefreshString();
                        SetLeagueInfo(wantReserveDate);
                        SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
                        survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(true);
                        reserveButton.SetActive(true);
                        reserveCancelButton.SetActive(false);
                    }
                    else
                    {
                        var temp = new LocalizedString("Basic", "Reserved Survivor");
                        temp.Arguments = new[] { new { param0 = leagueReserveInfo[wantReserveDate].reserver.localizedSurvivorName.GetLocalizedString() } };
                        reserveText.StringReference = temp;
                        reserveText.RefreshString();
                        SetLeagueInfo(wantReserveDate);
                        reserveText.GetComponent<LocalizeStringEvent>().RefreshString();
                        survivorWhoParticipateInBattleRoyaleDropdown.gameObject.SetActive(false);
                        reserveButton.SetActive(false);
                        reserveCancelButton.SetActive(true);
                    }
                }

            }
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

    public void SetBattleRoyaleReserveBox(Tier tier)
    {
        survivorWhoParticipateInBattleRoyaleDropdown.ClearOptions();
        //survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(outGameUIManager.SurvivorsDropdown.options);
        List<SurvivorData> allSurvivor = outGameUIManager.MySurvivorsData;
        for (int i = 0; i < allSurvivor.Count; i++)
            if (allSurvivor[i].tier == tier) survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { allSurvivor[i].localizedSurvivorName.GetLocalizedString() }));
        if (survivorWhoParticipateInBattleRoyaleDropdown.options.Count < 1)
        {
            survivorWhoParticipateInBattleRoyaleDropdown.AddOptions(new List<string>(new string[] { $"[{new LocalizedString("Basic", "No eligible survivor").GetLocalizedString()}]" }));
            reserveButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            wantReserver = allSurvivor.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
            reserveButton.GetComponent<Button>().interactable = true;
        }
        survivorWhoParticipateInBattleRoyaleDropdown.captionText.text = survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text;
    }

    public void OnSurvivorParticipatingInBattleRoyaleSelected()
    {
        wantReserver = outGameUIManager.MySurvivorsData.Find(x => x.localizedSurvivorName.GetLocalizedString() == survivorWhoParticipateInBattleRoyaleDropdown.options[survivorWhoParticipateInBattleRoyaleDropdown.value].text);
    }

    public void ReserveBattleRoyale()
    {
        if (wantReserver.tier != GetNeedTier(leagueReserveInfo[wantReserveDate].league))
        {
            Debug.LogWarning("!");
            //outGameUIManager.Alert($"{wantReserver.survivorName}'s tier does not match this league.\n" +
            //    $"({wantReserver.survivorName}'s tier : {wantReserver.tier}, league need tier : {GetNeedTier(leagueReserveInfo[wantReserveDate].league)})");
        }
        else if (wantReserver.isReserved)
        {
            outGameUIManager.Alert($"Alert:Already Resistered", wantReserver.localizedSurvivorName.GetLocalizedString());
        }
        else if (NeareastSeasonChampionship.reserver == wantReserver || NeareastWorldChampionship.reserver == wantReserver)
        {
            outGameUIManager.OpenConfirmWindow("Confirm:Reserve Who Reserved In Championship", () =>
            {
                if (wantReserver.injuries.Count > 0) AskAboutInjury();
                else Reserve();
            });
        }
        else if (wantReserver.injuries.Count > 0) AskAboutInjury();
        else Reserve();
    }

    public void Reserve()
    {
        leagueReserveInfo[wantReserveDate].reserver = wantReserver;
        wantReserver.isReserved = true;
        wantReserver.reservedDate = wantReserveDate;
        TurnPageCalendar(0);
        outGameUIManager.Alert("Alert:Battle royale reserved.");
    }

    void AskAboutInjury()
    {
        bool availiable = true;
        bool injured = false;
        string cause = "";
        int eyeInjury = 0;
        int handInjury = 0;
        foreach (Injury injury in wantReserver.injuries)
        {
            if (injury.site == InjurySite.Organ && injury.degree >= 1)
            {
                availiable = false;
                cause = $"{new LocalizedString("Injury", "Organ").GetLocalizedString()} {new LocalizedString("Injury", "Rupture").GetLocalizedString()}";
                break;
            }
            if (injury.site == InjurySite.LeftEye && injury.degree >= 1 || injury.site == InjurySite.RightEye && injury.degree >= 1) eyeInjury++;
            if ((injury.site == InjurySite.LeftHand || injury.site == InjurySite.LeftArm) && injury.degree >= 1 || (injury.site == InjurySite.RightHand || injury.site == InjurySite.RightArm) && injury.degree >= 1) handInjury++;
            if ( eyeInjury >= 2 )
            {
                availiable = false;
                cause = $"{new LocalizedString("Injury", "Blind in both eyes").GetLocalizedString()}";
                break;
            }
            if (handInjury >= 2)
            {
                availiable = false;
                cause = $"{new LocalizedString("Injury", "Cannot use both hands").GetLocalizedString()}";
                break;
            }
            if (injury.degree > 0)
            {
                injured = true;
            }
        }

        if (!availiable)
        {
            outGameUIManager.Alert("Alert:Reserve Fail", "Organ Rupture");
        }
        else if (injured)
        {
            outGameUIManager.OpenConfirmWindow("Confirm:Reserve Battle Royale Who Have Injury", () =>
            {
                Reserve();
            }, wantReserver.localizedSurvivorName.GetLocalizedString());
        }
        else Reserve();
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
                    scheduleTexts[2].text = $"{date % 28 + 1} {monthName[date / 28]}, {2101 + (date / 28) / 12}\n({leagueReserveInfo[date].league})";
                    if (NeareastSeasonChampionship.reserver == survivor) scheduleTexts[2].text += $"\n{NeareastSeasonChampionshipDate % 28 + 1} {monthName[NeareastSeasonChampionshipDate / 28]}, {2101 + (NeareastSeasonChampionshipDate / 28) / 12}\n(Season Championship)";
                    else if (NeareastWorldChampionship.reserver == survivor) scheduleTexts[2].text += $"\n{NeareastWorldChampionshipDate % 28 + 1} {monthName[NeareastWorldChampionshipDate / 28]}, {2101 + (NeareastWorldChampionshipDate / 28) / 12}\n(World Championship)";
                }
                else
                {
                    if (NeareastSeasonChampionship.reserver == survivor) scheduleTexts[2].text = $"{NeareastSeasonChampionshipDate % 28 + 1} {monthName[NeareastSeasonChampionshipDate / 28]}, {2101 + (NeareastSeasonChampionshipDate / 28) / 12}\n(Season Championship)";
                    else if (NeareastWorldChampionship.reserver == survivor) scheduleTexts[2].text = $"{NeareastWorldChampionshipDate % 28 + 1} {monthName[NeareastWorldChampionshipDate / 28]}, {2101 + (NeareastWorldChampionshipDate / 28) / 12}\n(World Championship)";
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
        calendarObject.SetActive(false);
        reserveForm.SetActive(false);
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (!leagueReserveInfo.ContainsKey(wantReserveDate)) return;
        farmableItemsText.text = "";
        foreach (var item in itemPool[leagueReserveInfo[wantReserveDate].itemPool])
        {
            farmableItemsText.text += $"{new LocalizedString("Item", item.Key.ToString()).GetLocalizedString()} x {item.Value},\n";
        }
        SetBattleRoyaleReserveBox(GetNeedTier(leagueReserveInfo[wantReserveDate].league));
        Today = today;
        CalendarPage = calendarPage;
    }

    public IEnumerator LoadLeagueReserveInfo(Dictionary<int, LeagueReserveData> data)
    {
        GameManager.ClaimLoadInfo("Loading calendar...", 1, 3);
        leagueReserveInfo.Clear();
        leagueReserveInfo = data;
        yield return null;
    }

    public void LoadToday(int today, int curMaxYear)
    {
        Today = today;
        this.curMaxYear = curMaxYear;
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
            "es-ES" => "ES",
            _ => ""
        };

        if (Enum.TryParse($"{league}{code}", out ResourceEnum.Sprite result))
        {
            return ResourceManager.Get(result);
        }
        else
        {
            Debug.LogWarning($"Can't find sprite : {league}");
            return default;
        }
    }
}

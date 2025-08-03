using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum Tier { Bronze, Silver, Gold }

[Serializable]
public class SurvivorData
{
    public int id = -1;
    // Stats
    [SerializeField] string survivorName;
    public string SurvivorName => survivorName;
    public LocalizedString localizedSurvivorName;
    public int _strength;
    public int _agility;
    public int _fighting;
    public int _shooting;
    public int _knowledge;
    public int _luck;
    public int _crafting;

    public int Strength
    {
        get
        {
            int result = _strength;
            if (HaveCharacteristic(CharacteristicType.MuscleDeficiency)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Strongman)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Powerhouse)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Assassin)) result -= 10;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Giant)) result = (int)(result * 1.3f);
            else if (HaveCharacteristic(CharacteristicType.Dwarf)) result = (int)(result * 0.7f);
            else if (HaveCharacteristic(CharacteristicType.BigMan)) result = (int)(result * 1.15f);
            if (ClutchThePerformance) result += 10;
            else if(ChockingUnderPressure) result -= 10;

            if (id > 0 && result >= 100) AcheivementManager.UnlockAchievement("Powerhouse");
            return Mathf.Max(result, 0);
        }
    }
    public int Agility
    {
        get
        {
            int result = _agility;
            if (HaveCharacteristic(CharacteristicType.Heavyfooted)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Lightfooted)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Assassin)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result -= 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (ClutchThePerformance) result += 10;
            else if (ChockingUnderPressure) result -= 10;

            if (id > 0 && result >= 100) AcheivementManager.UnlockAchievement("Quick-Footed");
            return Mathf.Max(result, 0);
        }
    }
    public int Fighting
    {
        get
        {
            int result = _fighting;
            if (HaveCharacteristic(CharacteristicType.ClumsyFighter)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Brawler)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Fighter)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Boxer)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (ClutchThePerformance) result += 10;
            else if (ChockingUnderPressure) result -= 10;

            if (id > 0 && result >= 100) AcheivementManager.UnlockAchievement("Martial Artist");
            return Mathf.Max(result, 0);
        }
    }
    public int Shooting
    {
        get
        {
            int result = _shooting;
            if (HaveCharacteristic(CharacteristicType.PoorAim)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Sniper)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Sharpshooter)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            if (ClutchThePerformance) result += 10;
            else if (ChockingUnderPressure) result -= 10;

            if (id > 0 && result >= 100) AcheivementManager.UnlockAchievement("Sharpshooter");
            return Mathf.Max(result, 0);
        }
    }
    public int Knowledge
    {
        get
        {
            int result = _knowledge;
            if (HaveCharacteristic(CharacteristicType.Dunce)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Smart)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Genius)) result += 20;
            if (ClutchThePerformance) result += 10;
            else if (ChockingUnderPressure) result -= 10;

            if (id > 0 && result >= 100) AcheivementManager.UnlockAchievement("Genius");
            return Mathf.Max(result, 0);
        }
    }

    public int StatTotal => Strength + Agility + Fighting + Shooting + Knowledge;

    public int Luck
    {
        get
        {
            int result = _luck;
            if(HaveCharacteristic(CharacteristicType.LuckGuy)) result += 25;
            else if(HaveCharacteristic(CharacteristicType.Cursed)) result -= 25;
            else if(HaveCharacteristic(CharacteristicType.Blessed)) result += 50;
            return result;
        }
    }

    public int Crafting
    {
        get { return _crafting; }
        set
        {
            _crafting = Math.Min(value, 100);
        }
    }

    public List<Characteristic> characteristics = new();
    public int price;

    bool HaveCharacteristic(CharacteristicType characteristic)
    {
        return characteristics.FindIndex(x => x.type == characteristic) > -1;
    }

    bool ClutchThePerformance
    {
        get
        {
            if(HaveCharacteristic(CharacteristicType.ClutchPerformance))
            {
                Calendar calendar = GameManager.Instance.Calendar;
                if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
                {
                    League league = calendar.LeagueReserveInfo[calendar.Today].league;
                    if (league == League.SeasonChampionship || league == League.WorldChampionship) return true;
                }
            }
            return false;
        }
    }

    bool ChockingUnderPressure
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.ChokingUnderPressure))
            {
                Calendar calendar = GameManager.Instance.Calendar;
                if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
                {
                    League league = calendar.LeagueReserveInfo[calendar.Today].league;
                    if (league == League.SeasonChampionship || league == League.WorldChampionship) return true;
                }
            }
            return false;
        }
    }
    
    // League
    public Tier tier;
    public bool isReserved;
    public int reservedDate = -1;
    
    // Training
    public Training assignedTraining;
    public int increaseComparedToPrevious_strength = -1;
    public int increaseComparedToPrevious_agility = -1;
    public int increaseComparedToPrevious_fighting = -1;
    public int increaseComparedToPrevious_shooting = -1;
    public int increaseComparedToPrevious_knowledge = -1;

    // Injury, Surgery
    public List<Injury> injuries = new();
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public LocalizedString localizedScheduledSurgeryName;
    public int scheduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;
    bool recoverySerumAdministered;
    public bool RecoverySerumAdministered
    {
        get => recoverySerumAdministered;
        set
        {
            recoverySerumAdministered = value;
            if (value) recoverySerumMedicalEffectLeft = 3;
        }
    }
    public int recoverySerumMedicalEffectLeft;

    // Strategy
    public ItemManager.Items priority1Weapon = ItemManager.Items.AssaultRifle;
    public Dictionary<StrategyCase, StrategyData> strategyDictionary = new();
    [SerializeField] public ItemManager.Craftable priority1Crafting = null;
    public int priority1CraftingToInt = -1;
    public bool[] craftingAllows;

    // Stastics
    public int winCount;
    public int rankDefenseCount;
    public int loseCount;
    public int winCountGoldPlus;
    public int rankDefenseCountGoldPlus;
    public int loseCountGoldPlus;
    public int totalKill;
    public float totalSurvivedTime;
    public int totalRankPrize;
    public int totalKillPrize;
    public int totalTreatmentFee;
    public int totalSurgeryFee;
    public float totalGiveDamage;
    public float totalTakeDamage;
    public bool wonBronzeLeague;
    public bool wonSilverLeague;
    public bool wonGoldLeague;
    public bool wonSeasonChampionship;
    public bool wonWorldChampionship;
    public bool wonMeleeLeague;
    public bool wonRangedLeague;
    public bool wonCraftingLeague;
    public int craftingCount;

    public SurvivorData(LocalizedString localizedSurvivorName, int strength, int agility, int fighting, int shooting, int knowledge, int price, Tier tier)
    {
        this.localizedSurvivorName = localizedSurvivorName;
        survivorName = localizedSurvivorName.TableEntryReference.Key;
        _strength = strength;
        _agility = agility;
        _fighting = fighting;
        _shooting = shooting;
        _knowledge = knowledge;
        _luck = 50;
        _crafting = 0;
        this.price = price;
        this.tier = tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for(int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public SurvivorData(SurvivorData survivorData)
    {
        survivorName = survivorData.survivorName;
        localizedSurvivorName = new LocalizedString("Name", survivorName);
        _strength = survivorData._strength;
        _agility = survivorData._agility;
        _fighting = survivorData._fighting;
        _shooting = survivorData._shooting;
        _knowledge = survivorData._knowledge;
        _luck = survivorData._luck;
        price = survivorData.price;
        tier = survivorData.tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for (int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public void IncreaseStats(int strength, int agility, int fighting, int shooting, int knowledge)
    {
        if (_strength + strength > 100 || _agility + agility > 100 || _fighting + fighting > 100 || _shooting + shooting > 100 || _knowledge + knowledge > 100) return;
        _strength += strength;
        _agility += agility;
        _fighting += fighting;
        _shooting += shooting;
        _knowledge += knowledge;

        increaseComparedToPrevious_strength += strength;
        increaseComparedToPrevious_agility += agility;
        increaseComparedToPrevious_fighting += fighting;
        increaseComparedToPrevious_shooting += shooting;
        increaseComparedToPrevious_knowledge += knowledge;
    }
}

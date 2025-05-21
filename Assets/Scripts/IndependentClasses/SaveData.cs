using System;
using System.Collections;
using System.Collections.Generic;

#region Survivors
[Serializable]
public class StrategyDictionaryEntry
{
    public StrategyCase key;
    public StrategyData value;

    public StrategyDictionaryEntry(StrategyCase key, StrategyData value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class SurvivorSaveData
{
    public int id;
    public string survivorName;
    public int strength;
    public int agility;
    public int fighting;
    public int shooting;
    public int knowledge;
    public int luck;
    public int price;
    public Tier tier;
    public bool isReserved;
    public List<Injury> injuries;
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public int shceduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;
    public ItemManager.Items priority1Weapon;
    public List<StrategyDictionaryEntry> strategyDictionaryEntries;
}

[Serializable]
public class MySurvivorListSaveData
{
    public List<SurvivorSaveData> survivorSaveDatas;
}
#endregion

[Serializable]
public class LeagueReserveEntrySaveData
{
    public int key; // Dictionary¿« ≈∞
    public int leagueId;
    public string mapName;
    public int itemPool;
    public int reserverId;

    public LeagueReserveEntrySaveData(int key, int leagueId, string mapName, int itemPool, int reserverId)
    {
        this.key = key;
        this.leagueId = leagueId;
        this.mapName = mapName;
        this.itemPool = itemPool;
        this.reserverId = reserverId;
    }
}

[Serializable]
public class LeagueReserveDictionarySaveData
{
    public List<LeagueReserveEntrySaveData> entries = new();
}

[Serializable]
public class ETCData
{
    // out game ui manager
    public int money;
    public int mySurvivorsId;
    public int survivorHireLimit;
    public int fightTrainingLevel;
    public int shootingTrainingLevel;
    public int agilityTrainingLevel;
    public int weightTrainingLevel;
    // calendar
    public int today;

    public ETCData(int money, int mySurvivorsId, int survivorHireLimit, int fightTrainingLevel, int shootingTrainingLevel, int agilityTrainingLevel, int weightTrainingLevel, int today)
    {
        this.money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.survivorHireLimit = survivorHireLimit;
        this.fightTrainingLevel = fightTrainingLevel;
        this.shootingTrainingLevel = shootingTrainingLevel;
        this.agilityTrainingLevel = agilityTrainingLevel;
        this.weightTrainingLevel = weightTrainingLevel;
        this.today = today;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

#region Survivors
[Serializable]
public class SaveDataInfo
{
    public string gameVersion;
    public string savedTime;
    public string ingameDate;

    public SaveDataInfo(string gameVersion, string savedTime, string ingameDate)
    {
        this.gameVersion = gameVersion;
        this.savedTime = savedTime;
        this.ingameDate = ingameDate;
    }
}

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
    public List<Characteristic> characteristics;
    public int price;
    public Tier tier;
    public bool isReserved;
    public int reservedDate;
    public Training assignedTraining;
    public int increaseComparedToPrevious_strength;
    public int increaseComparedToPrevious_agility;
    public int increaseComparedToPrevious_fighting;
    public int increaseComparedToPrevious_shooting;
    public int increaseComparedToPrevious_knowledge;
    public List<Injury> injuries;
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public int shceduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;
    public bool recoverySerumAdministered;
    public int recoverySerumMedicalEffectLeft;
    public ItemManager.Items priority1Weapon;
    public List<StrategyDictionaryEntry> strategyDictionaryEntries;
    public ItemManager.Craftable priority1Crafting;
    public int priority1CraftingToInt;
    public bool[] craftingAllows;
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
    public int runningLevel;
    public int weightTrainingLevel;
    public int studyingLevel;
    public SurvivorData[] hireMarketSurvivorData = new SurvivorData[3];
    public bool[] soldOut = new bool[3];
    // calendar
    public int today;
    public int curMaxYear;

    public ETCData(int money, int mySurvivorsId, int survivorHireLimit, int fightTrainingLevel, int shootingTrainingLevel, int runningLevel, int weightTrainingLevel, int studyingLevel, int today, int curMaxYear)
    {
        this.money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.survivorHireLimit = survivorHireLimit;
        this.fightTrainingLevel = fightTrainingLevel;
        this.shootingTrainingLevel = shootingTrainingLevel;
        this.runningLevel = runningLevel;
        this.weightTrainingLevel = weightTrainingLevel;
        this.studyingLevel = studyingLevel;
        this.today = today;
        this.curMaxYear = curMaxYear;
    }
}
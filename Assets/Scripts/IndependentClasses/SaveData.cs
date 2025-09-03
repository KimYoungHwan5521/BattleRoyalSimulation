using System;
using System.Collections;
using System.Collections.Generic;

#region Survivors
[Serializable]
public class SaveDataInfo
{
    public string gameVersion;
    public string savedTime;
    public int ingameDate;

    public SaveDataInfo(string gameVersion, string savedTime, int ingameDate)
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
    public int crafting;
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
    //public string scheduledSurgeryName;
    public string localizedScheduledSurgeryTable;
    public string localizedScheduledSurgeryEntry;
    public int shceduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;
    //public bool recoverySerumAdministered;
    //public int recoverySerumMedicalEffectLeft;
    public ItemManager.Items priority1Weapon;
    public List<StrategyDictionaryEntry> strategyDictionaryEntries;
    public ItemManager.Craftable priority1Crafting;
    public int priority1CraftingToInt;
    public bool[] craftingAllows;
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
    public List<SurvivorData> contestantsData = new List<SurvivorData>();
    // calendar
    public int today;
    public int curMaxYear;
    public bool participationConfirmed;

    public ETCData(int money, int mySurvivorsId, int survivorHireLimit, int fightTrainingLevel, int shootingTrainingLevel, int runningLevel, int weightTrainingLevel, int studyingLevel, List<SurvivorData> contestantsData, int today, int curMaxYear, bool participationConfirmed)
    {
        this.money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.survivorHireLimit = survivorHireLimit;
        this.fightTrainingLevel = fightTrainingLevel;
        this.shootingTrainingLevel = shootingTrainingLevel;
        this.runningLevel = runningLevel;
        this.weightTrainingLevel = weightTrainingLevel;
        this.studyingLevel = studyingLevel;
        this.contestantsData = contestantsData;
        this.today = today;
        this.curMaxYear = curMaxYear;
        this.participationConfirmed = participationConfirmed;
    }
}
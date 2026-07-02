using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class StrategyDictionarySaveData
{
    public ItemManager.Items priority1Weapon = ItemManager.Items.LASER;
    public ItemManager.Items priority2Weapon = ItemManager.Items.AssaultRifle;
    public Dictionary<StrategyCase, StrategyData> strategyDictionary = new();
    public ItemManager.Craftable priority1Crafting = null;
    public ItemManager.Craftable priority2Crafting = null;
    public int priority1CraftingToInt = -1;
    public int priority2CraftingToInt = -1;
    public bool[] craftingAllows;
    public int repairCondition = 70;

    public List<StrategyDictionaryEntry> entries = new();

    public StrategyDictionarySaveData(SurvivorData data)
    {
        priority1Weapon = data.priority1Weapon;
        priority2Weapon = data.priority2Weapon;

        priority1Crafting = data.priority1Crafting;
        priority2Crafting = data.priority2Crafting;

        priority1CraftingToInt = data.priority1CraftingToInt;
        priority2CraftingToInt = data.priority2CraftingToInt;

        craftingAllows = data.craftingAllows != null
            ? (bool[])data.craftingAllows.Clone()
            : null;

        repairCondition = data.repairCondition;

        strategyDictionary = data.strategyDictionary != null
            ? new Dictionary<StrategyCase, StrategyData>(data.strategyDictionary)
            : new Dictionary<StrategyCase, StrategyData>();

        entries = new List<StrategyDictionaryEntry>();

        foreach (var kv in strategyDictionary)
        {
            entries.Add(
                new StrategyDictionaryEntry(kv.Key, kv.Value)
            );
        }
    }

    public Dictionary<StrategyCase, StrategyData> CreateStrategyDictionary()
    {
        Dictionary<StrategyCase, StrategyData> result = new();

        if (entries == null)
            return result;

        foreach (var entry in entries)
        {
            result[entry.key] = entry.value;
        }

        return result;
    }
}

[Serializable]
public class SurvivorSaveData
{
    public int id;
    public string survivorName;
    public int stamina;
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
    //public bool haveQualifyToParticipateInSeasonChampionship;
    //public bool haveQualifyToParticipateInWorldChampionship;
    public int increaseComparedToPrevious_strength;
    public int increaseComparedToPrevious_agility;
    public int increaseComparedToPrevious_fighting;
    public int increaseComparedToPrevious_shooting;
    public int increaseComparedToPrevious_crafting;
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
    public ItemManager.Items priority2Weapon;
    public List<StrategyDictionaryEntry> strategyDictionaryEntries;
    public ItemManager.Craftable priority1Crafting;
    public ItemManager.Craftable priority2Crafting;
    public int priority1CraftingToInt;
    public int priority2CraftingToInt;
    public bool[] craftingAllows;
    public int repairCondition;
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
    public int mostKillsInASingleMatch;
    public bool wonBronzeLeague;
    public bool wonSilverLeague;
    public bool wonGoldLeague;
    public bool wonSeasonChampionship;
    public bool wonWorldChampionship;
    public bool wonMeleeLeague;
    public bool wonRangedLeague;
    public bool wonCraftingLeague;
    public int craftingCount;
    public bool royalLoader;
    public int receivedTrainings;
    public int consumedStaminas;
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
public class UnlockStatusDictionary
{
    public UnlockManager.UnlockCondition unlockCondition;
    public bool isUnlocked;

    public UnlockStatusDictionary(UnlockManager.UnlockCondition unlockCondition, bool isUnlocked)
    {
        this.unlockCondition = unlockCondition;
        this.isUnlocked = isUnlocked;
    }
}

[Serializable]
public class ETCData
{
    // out game ui manager
    public int difficulty;
    public int money;
    public int mySurvivorsId;
    public int trainingLevel;
    public List<TrainingInfo> trainings = new();
    public int survivorHireLimit;
    public SurvivorData[] hireMarketSurvivorData = new SurvivorData[3];
    public bool[] soldOut = new bool[3];
    public List<SurvivorData> contestantsData = new List<SurvivorData>();
    // calendar
    public int today;
    public int curMaxYear;
    public bool participationConfirmed;
    // unlock
    public List<UnlockStatusDictionary> unlockStatus = new();

    public List<string> earnedAchievements = new();

    public ETCData(int difficulty, int money, int mySurvivorsId, int trainingLevel, TrainingCard[] trainingCards, int survivorHireLimit, List<SurvivorData> contestantsData, int today, int curMaxYear, bool participationConfirmed,
        Dictionary<UnlockManager.UnlockCondition, bool> unlockStatus)
    {
        this.difficulty = difficulty;
        this.money = money;
        this.mySurvivorsId = mySurvivorsId;
        this.trainingLevel = trainingLevel;
        this.survivorHireLimit = survivorHireLimit;
        this.contestantsData = contestantsData;
        this.today = today;
        this.curMaxYear = curMaxYear;
        this.participationConfirmed = participationConfirmed;
        earnedAchievements = AchievementManager.earnedAchievementsInThisRun;

        this.unlockStatus = new();
        foreach (var kv in unlockStatus)
        {
            this.unlockStatus.Add(new(kv.Key, kv.Value));
        }

        trainings = new();
        foreach (var trainingCard in trainingCards)
        {
            trainings.Add(trainingCard.LinkedTraining);
        }
    }
}
using System.Collections.Generic;
using System;

public static class SaveManager
{
    public static SurvivorSaveData ToSaveData(SurvivorData data)
    {
        var saveData = new SurvivorSaveData
        {
            id = data.id,
            survivorName = data.survivorName,
            hp = data.hp,
            power = data._power,
            attackSpeed = data._attackSpeed,
            moveSpeed = data._moveSpeed,
            farmingSpeed = data._farmingSpeed,
            shooting = data._shooting,
            luck = data.luck,
            price = data.price,
            tier = data.tier,
            isReserved = data.isReserved,
            injuries = data.injuries,
            surgeryScheduled = data.surgeryScheduled,
            scheduledSurgeryName = data.scheduledSurgeryName,
            shceduledSurgeryCost = data.shceduledSurgeryCost,
            surgerySite = data.surgerySite,
            surgeryType = data.surgeryType,
            surgeryCharacteristic = data.surgeryCharacteristic,
            priority1Weapon = data.priority1Weapon,
            strategyDictionaryEntries = new()
        };

        foreach (var kv in data.strategyDictionary)
        {
            saveData.strategyDictionaryEntries.Add(new StrategyDictionaryEntry(kv.Key, kv.Value));
        }

        return saveData;
    }

    public static SurvivorData FromSaveData(SurvivorSaveData saveData)
    {
        var survivor = new SurvivorData(saveData.survivorName, saveData.hp, saveData.power,
            saveData.attackSpeed, saveData.moveSpeed, saveData.farmingSpeed, saveData.shooting,
            saveData.price, saveData.tier);

        survivor.id = saveData.id;
        survivor.luck = saveData.luck;
        survivor.isReserved = saveData.isReserved;
        survivor.injuries = saveData.injuries ?? new();
        survivor.surgeryScheduled = saveData.surgeryScheduled;
        survivor.scheduledSurgeryName = saveData.scheduledSurgeryName;
        survivor.shceduledSurgeryCost = saveData.shceduledSurgeryCost;
        survivor.surgerySite = saveData.surgerySite;
        survivor.surgeryType = saveData.surgeryType;
        survivor.surgeryCharacteristic = saveData.surgeryCharacteristic;
        survivor.priority1Weapon = saveData.priority1Weapon;

        survivor.strategyDictionary.Clear();
        foreach (var entry in saveData.strategyDictionaryEntries)
        {
            survivor.strategyDictionary[entry.key] = entry.value;
        }

        return survivor;
    }

    public static LeagueReserveDictionarySaveData ToSaveData(Dictionary<int, LeagueReserveData> data)
    {
        var saveData = new LeagueReserveDictionarySaveData();

        foreach (var kv in data)
        {
            int key = kv.Key;
            LeagueReserveData value = kv.Value;

            saveData.entries.Add(new LeagueReserveEntrySaveData(
                key,
                (int)value.league,
                value.map.ToString(),
                value.reserver?.id ?? -1
            ));
        }

        return saveData;
    }

    public static Dictionary<int, LeagueReserveData> FromSaveData(LeagueReserveDictionarySaveData saveData)
    {
        Dictionary<int, LeagueReserveData> result = new();

        foreach (var entry in saveData.entries)
        {
            var league = (League)entry.leagueId;
            var map = (ResourceEnum.Prefab)Enum.Parse(typeof(ResourceEnum.Prefab), entry.mapName);
            SurvivorData reserver = GameManager.Instance.OutGameUIManager.MySurvivorsData.Find(x => x.id == entry.reserverId);

            var reserveData = new LeagueReserveData(league, map);
            reserveData.reserver = reserver;

            result[entry.key] = reserveData;
        }

        return result;
    }

    public static ETCData ToSaveData()
    {
        OutGameUIManager outGameUIManager = GameManager.Instance.OutGameUIManager;
        Calendar calendar = GameManager.Instance.Calendar;
        ETCData result = new(
            outGameUIManager.Money, 
            outGameUIManager.MySurvivorsId, 
            outGameUIManager.SurvivorHireLimit,
            outGameUIManager.FightTrainingLevel,
            outGameUIManager.ShootingTrainingLevel,
            outGameUIManager.AgilityTrainingLevel,
            outGameUIManager.WeightTrainingLevel,
            calendar.Today
            );
        return result;
    }

    public static void FromSaveData(ETCData data)
    {
        GameManager.Instance.OutGameUIManager.LoadData(
        data.money,
        data.mySurvivorsId,
        data.survivorHireLimit,
        data.fightTrainingLevel,
        data.shootingTrainingLevel,
        data.agilityTrainingLevel,
        data.weightTrainingLevel
            );
        GameManager.Instance.Calendar.LoadToday(data.today);
    }
}

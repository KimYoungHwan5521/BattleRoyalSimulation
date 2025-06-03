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
            strength = data._strength,
            agility = data._agility,
            fighting = data._fighting,
            shooting = data._shooting,
            knowledge = data._knowledge,
            luck = data._luck,
            characteristics = data.characteristics,
            price = data.price,
            tier = data.tier,
            isReserved = data.isReserved,
            assignedTraining = data.assignedTraining,
            increaseComparedToPrevious_strength = data.increaseComparedToPrevious_strength,
            increaseComparedToPrevious_agility = data.increaseComparedToPrevious_agility,
            increaseComparedToPrevious_fighting = data.increaseComparedToPrevious_fighting,
            increaseComparedToPrevious_shooting = data.increaseComparedToPrevious_shooting,
            increaseComparedToPrevious_knowledge = data.increaseComparedToPrevious_knowledge,
            injuries = data.injuries,
            surgeryScheduled = data.surgeryScheduled,
            scheduledSurgeryName = data.scheduledSurgeryName,
            shceduledSurgeryCost = data.shceduledSurgeryCost,
            surgerySite = data.surgerySite,
            surgeryType = data.surgeryType,
            surgeryCharacteristic = data.surgeryCharacteristic,
            priority1Weapon = data.priority1Weapon,
            priority1Crafting = data.priority1Crafting,
            priority1CraftingToInt = data.priority1CraftingToInt,
            craftingAllows = data.craftingAllows,
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
        SurvivorData survivor = new(saveData.survivorName, saveData.strength, saveData.agility,
            saveData.fighting, saveData.shooting, saveData.knowledge, saveData.price, saveData.tier)
        {
            id = saveData.id,
            _luck = saveData.luck,
            characteristics = saveData.characteristics,
            isReserved = saveData.isReserved,
            assignedTraining = saveData.assignedTraining,
            increaseComparedToPrevious_strength = saveData.increaseComparedToPrevious_strength,
            increaseComparedToPrevious_agility = saveData.increaseComparedToPrevious_agility,
            increaseComparedToPrevious_fighting = saveData.increaseComparedToPrevious_fighting,
            increaseComparedToPrevious_shooting = saveData.increaseComparedToPrevious_shooting,
            increaseComparedToPrevious_knowledge = saveData.increaseComparedToPrevious_knowledge,
            injuries = saveData.injuries ?? new(),
            surgeryScheduled = saveData.surgeryScheduled,
            scheduledSurgeryName = saveData.scheduledSurgeryName,
            shceduledSurgeryCost = saveData.shceduledSurgeryCost,
            surgerySite = saveData.surgerySite,
            surgeryType = saveData.surgeryType,
            surgeryCharacteristic = saveData.surgeryCharacteristic,
            priority1Weapon = saveData.priority1Weapon,
            priority1Crafting = saveData.priority1Crafting,
            priority1CraftingToInt = saveData.priority1CraftingToInt,
            craftingAllows = saveData.craftingAllows,
        };

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
                value.itemPool,
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
            int itemPool = entry.itemPool;
            SurvivorData reserver = GameManager.Instance.OutGameUIManager.MySurvivorsData.Find(x => x.id == entry.reserverId);

            var reserveData = new LeagueReserveData(league, map, itemPool);
            reserveData.reserver = reserver;

            result[entry.key] = reserveData;
        }

        return result;
    }

}

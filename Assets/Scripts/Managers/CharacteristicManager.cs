using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacteristicType
{
    EagleEye,
    BadEye,
    KeenHearing,
    BadHearing,
    ClutchPerformance,
    ChokingUnderPressure,
    Boxer,
    Giant,
    Dwarf,
}

[Serializable]
public struct Characteristic
{
    public CharacteristicType type;
    public string characteristicName;
    public string description;
    public CharacteristicType[] notPossbleTogether;
    public int value;

    public Characteristic(CharacteristicType type, string characteristicName, string description, int value, params CharacteristicType[] notPossibleTogether)
    {
        this.type = type;
        this.characteristicName = characteristicName; 
        this.description = description;
        this.value = value;
        this.notPossbleTogether = notPossibleTogether;
    }
}

public class CharacteristicManager
{
    static List<Characteristic> characteristics = new();
    public static List<Characteristic> Characteristics => characteristics;

    public IEnumerator Initiate()
    {
        GameManager.ClaimLoadInfo("Loading characteristics...");
        characteristics.Add(new(CharacteristicType.EagleEye, "Eagle eye", "Eye sight + 30%", 0, CharacteristicType.BadEye));
        characteristics.Add(new(CharacteristicType.BadEye, "Bad eye", "Eye sight - 30%", 0, CharacteristicType.EagleEye));
        characteristics.Add(new(CharacteristicType.KeenHearing, "Keen hearing", "Hearing ability + 30%", 0, CharacteristicType.BadHearing));
        characteristics.Add(new(CharacteristicType.BadHearing, "Bad hearing", "Hearing ability - 30%", 0, CharacteristicType.KeenHearing));
        characteristics.Add(new(CharacteristicType.ClutchPerformance, "Clutch performance", "Most abilities increase during the championship", 0, CharacteristicType.ChokingUnderPressure));
        characteristics.Add(new(CharacteristicType.ChokingUnderPressure, "Choking under pressure", "Most abilities decrease during the championship", 0, CharacteristicType.ClutchPerformance));
        characteristics.Add(new(CharacteristicType.Giant, "Giant", "Size, HP, Attack damage + 30%/ Attack speed, Move speed - 30%", 0, CharacteristicType.Dwarf));
        characteristics.Add(new(CharacteristicType.Dwarf, "Dwarf", "Size, HP, Attack damage - 30%/ Attack speed, Move speed + 30%", 0, CharacteristicType.Giant));
        characteristics.Add(new(CharacteristicType.Boxer, "Boxer", "Attack damage, Attack speed + 20%/ Increase hit, guard, avoid rate when fighting", 0));
        yield return null;
    }

    public static bool AddCharaicteristic(SurvivorData survivor, Characteristic wantCharacteristic)
    {
        if(survivor.characteristics.Contains(wantCharacteristic)) return false;
        foreach(Characteristic survivorChar in survivor.characteristics)
        {
            if(survivorChar.notPossbleTogether.ToList().Contains(wantCharacteristic.type))
            {
                return false;
            }
        }
        survivor.characteristics.Add(wantCharacteristic);
        return true;
    }

    public static void AddRandomCharacteristics(SurvivorData survivor, int howMany)
    {
        int check = 0;
        for(int i = 0; i < howMany; i++)
        {
            if(check > 100)
            {
                Debug.LogWarning("Infinite loop detected");
                return;
            }
            if(!AddCharaicteristic(survivor, characteristics[UnityEngine.Random.Range(0, characteristics.Count())]))
            {
                i--;
                check++;
                continue;
            }
        }
    }
}

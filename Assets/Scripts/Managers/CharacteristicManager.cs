using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public enum CharacteristicType
{
    HawkEye,
    BadEye,
    KeenHearing,
    BadHearing,
    ClutchPerformance,
    ChokingUnderPressure,
    MuscleDeficiency,
    Strongman,
    Powerhouse,
    Heavyfooted,
    Lightfooted,
    Assassin,
    ClumsyFighter,
    Brawler,
    Fighter,
    PoorAim,
    Sniper,
    Sharpshooter,
    Dunce,
    Smart,
    Genius,
    Giant,
    Dwarf,
    BigMan,
    CarefulShooter,
    Fragile,
    Sturdy,
    Avenger,
    Regenerator,
    UpsAndDowns,
    LuckGuy,
    Cursed,
    Blessed,
    ClumsyHand,
    Dexterous,
    Engineer,
    Fatty,
    Soldier,
    Boxer,
    Luchador,
    QuickDrawer,
}

public enum CharacteristicRarity { Common, Uncommon, Rare }

[Serializable]
public struct Characteristic
{
    public CharacteristicType type;
    public LocalizedString characteristicName;
    public LocalizedString description;
    public CharacteristicRarity rarity;
    public CharacteristicType[] notPossibleTogether;
    public int value;

    public Characteristic(CharacteristicType type, CharacteristicRarity rarity, int value, params CharacteristicType[] notPossibleTogether)
    {
        this.type = type;
        characteristicName = new LocalizedString("Characteristic", type.ToString()); 
        description = new LocalizedString("Characteristic", $"Help:{type}");
        this.rarity = rarity;
        this.value = value;
        this.notPossibleTogether = notPossibleTogether;
    }
}

public class CharacteristicManager
{
    static List<Characteristic> characteristics = new();
    public static List<Characteristic> Characteristics => characteristics;

    public IEnumerator Initiate()
    {
        GameManager.ClaimLoadInfo("Loading characteristics...");
        characteristics.Add(new(CharacteristicType.HawkEye, CharacteristicRarity.Common, 0, CharacteristicType.BadEye));
        characteristics.Add(new(CharacteristicType.BadEye, CharacteristicRarity.Common, 0, CharacteristicType.HawkEye));
        characteristics.Add(new(CharacteristicType.KeenHearing, CharacteristicRarity.Common, 0, CharacteristicType.BadHearing));
        characteristics.Add(new(CharacteristicType.BadHearing, CharacteristicRarity.Common, 0, CharacteristicType.KeenHearing));
        characteristics.Add(new(CharacteristicType.ClutchPerformance, CharacteristicRarity.Rare, 0, CharacteristicType.ChokingUnderPressure));
        characteristics.Add(new(CharacteristicType.ChokingUnderPressure, CharacteristicRarity.Uncommon, 0, CharacteristicType.ClutchPerformance));
        characteristics.Add(new(CharacteristicType.Giant, CharacteristicRarity.Uncommon, 0, CharacteristicType.Dwarf, CharacteristicType.BigMan));
        characteristics.Add(new(CharacteristicType.Dwarf, CharacteristicRarity.Uncommon, 0, CharacteristicType.Giant, CharacteristicType.BigMan));
        characteristics.Add(new(CharacteristicType.BigMan, CharacteristicRarity.Common, 0, CharacteristicType.Giant, CharacteristicType.Dwarf));
        characteristics.Add(new(CharacteristicType.MuscleDeficiency, CharacteristicRarity.Common, 0, CharacteristicType.Strongman, CharacteristicType.Powerhouse));
        characteristics.Add(new(CharacteristicType.Strongman, CharacteristicRarity.Common, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Powerhouse));
        characteristics.Add(new(CharacteristicType.Powerhouse, CharacteristicRarity.Uncommon, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman));
        characteristics.Add(new(CharacteristicType.Heavyfooted, CharacteristicRarity.Common, 0, CharacteristicType.Lightfooted));
        characteristics.Add(new(CharacteristicType.Lightfooted, CharacteristicRarity.Common, 0, CharacteristicType.Heavyfooted));
        characteristics.Add(new(CharacteristicType.Assassin, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.ClumsyFighter, CharacteristicRarity.Common, 0, CharacteristicType.Brawler, CharacteristicType.Fighter));
        characteristics.Add(new(CharacteristicType.Brawler, CharacteristicRarity.Common, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Fighter));
        characteristics.Add(new(CharacteristicType.Fighter, CharacteristicRarity.Uncommon, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler));
        characteristics.Add(new(CharacteristicType.PoorAim, CharacteristicRarity.Common, 0, CharacteristicType.Sniper, CharacteristicType.Sharpshooter));
        characteristics.Add(new(CharacteristicType.Sniper, CharacteristicRarity.Common, 0, CharacteristicType.PoorAim, CharacteristicType.Sharpshooter));
        characteristics.Add(new(CharacteristicType.Sharpshooter, CharacteristicRarity.Uncommon, 0, CharacteristicType.PoorAim, CharacteristicType.Sniper));
        characteristics.Add(new(CharacteristicType.Dunce, CharacteristicRarity.Common, 0, CharacteristicType.Smart, CharacteristicType.Genius));
        characteristics.Add(new(CharacteristicType.Smart, CharacteristicRarity.Uncommon, 0, CharacteristicType.Dunce, CharacteristicType.Genius));
        characteristics.Add(new(CharacteristicType.Genius, CharacteristicRarity.Rare, 0, CharacteristicType.Dunce, CharacteristicType.Smart));
        characteristics.Add(new(CharacteristicType.CarefulShooter, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Fragile, CharacteristicRarity.Uncommon, 0, CharacteristicType.Sturdy));
        characteristics.Add(new(CharacteristicType.Sturdy, CharacteristicRarity.Rare, 0, CharacteristicType.Fragile));
        characteristics.Add(new(CharacteristicType.Avenger, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Regenerator, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.UpsAndDowns, CharacteristicRarity.Common, 0));
        characteristics.Add(new(CharacteristicType.LuckGuy, CharacteristicRarity.Uncommon, 0, CharacteristicType.Cursed, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Cursed, CharacteristicRarity.Uncommon, 0, CharacteristicType.LuckGuy, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Blessed, CharacteristicRarity.Rare, 0, CharacteristicType.LuckGuy, CharacteristicType.Cursed));
        characteristics.Add(new(CharacteristicType.ClumsyHand, CharacteristicRarity.Common, 0, CharacteristicType.Dexterous, CharacteristicType.Engineer));
        characteristics.Add(new(CharacteristicType.Dexterous, CharacteristicRarity.Common, 0, CharacteristicType.ClumsyHand, CharacteristicType.Engineer));
        characteristics.Add(new(CharacteristicType.Engineer, CharacteristicRarity.Uncommon, 0, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous));
        characteristics.Add(new(CharacteristicType.Fatty, CharacteristicRarity.Common, 0));
        characteristics.Add(new(CharacteristicType.Soldier, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.Boxer, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Luchador, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.QuickDrawer, CharacteristicRarity.Uncommon, 0, CharacteristicType.CarefulShooter));
        yield return null;
    }

    public static bool AddCharaicteristic(SurvivorData survivor, CharacteristicType wantCharacteristic, int rarity = -1)
    {
        int hasAleady = survivor.characteristics.FindIndex(x => x.type  == wantCharacteristic);
        if(hasAleady > -1) return false;
        foreach(Characteristic survivorChar in survivor.characteristics)
        {
            if(survivorChar.notPossibleTogether.ToList().Contains(wantCharacteristic))
            {
                return false;
            }
        }
        Characteristic characteristic = characteristics.Find(x => x.type == wantCharacteristic);
        if(rarity > -1 && (int)characteristic.rarity != rarity) return false;
        survivor.characteristics.Add(characteristic);
        return true;
    }

    public static void AddRandomCharacteristics(SurvivorData survivor, int howMany)
    {
        int check = 0;

        int wantRarity;
        float rand = UnityEngine.Random.Range(0, 1f);
        if (rand < 0.9f) wantRarity = 0;
        else if (rand < 0.99f) wantRarity = 1;
        else wantRarity = 2;

        for(int i = 0; i < howMany; i++)
        {
            if(check > 1000)
            {
                Debug.LogWarning("Infinite loop detected");
                return;
            }
            if (!AddCharaicteristic(survivor, (CharacteristicType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CharacteristicType)).Length), wantRarity))
            {
                i--;
                check++;
                continue;
            }
            else
            {
                // 추가 했으면 새로운 rarity
                rand = UnityEngine.Random.Range(0, 1f);
                if (rand < 0.9f) wantRarity = 0;
                else if (rand < 0.99f) wantRarity = 1;
                else wantRarity = 2;
            }
        }
    }
}

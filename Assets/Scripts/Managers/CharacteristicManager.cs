using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}

public enum CharacteristicRarity { Common, Uncommon, Rare }

[Serializable]
public struct Characteristic
{
    public CharacteristicType type;
    public string characteristicName;
    public CharacteristicRarity rarity;
    public string description;
    public CharacteristicType[] notPossbleTogether;
    public int value;

    public Characteristic(CharacteristicType type, string characteristicName, CharacteristicRarity rarity, string description, int value, params CharacteristicType[] notPossibleTogether)
    {
        this.type = type;
        this.characteristicName = characteristicName; 
        this.rarity = rarity;
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
        characteristics.Add(new(CharacteristicType.HawkEye, "Hawk eye", CharacteristicRarity.Common, "Eye sight + 30%", 0, CharacteristicType.BadEye));
        characteristics.Add(new(CharacteristicType.BadEye, "Bad eye", CharacteristicRarity.Common, "Eye sight - 30%", 0, CharacteristicType.HawkEye));
        characteristics.Add(new(CharacteristicType.KeenHearing, "Keen hearing", CharacteristicRarity.Common, "Hearing ability + 30%", 0, CharacteristicType.BadHearing));
        characteristics.Add(new(CharacteristicType.BadHearing, "Bad hearing", CharacteristicRarity.Common, "Hearing ability - 30%", 0, CharacteristicType.KeenHearing));
        characteristics.Add(new(CharacteristicType.ClutchPerformance, "Clutch performance", CharacteristicRarity.Rare, "Abilities increase during the championship", 0, CharacteristicType.ChokingUnderPressure));
        characteristics.Add(new(CharacteristicType.ChokingUnderPressure, "Choking under pressure", CharacteristicRarity.Uncommon, "Abilities decrease during the championship", 0, CharacteristicType.ClutchPerformance));
        characteristics.Add(new(CharacteristicType.Giant, "Giant", CharacteristicRarity.Uncommon, "Size, Strength + 30%", 0, CharacteristicType.Dwarf));
        characteristics.Add(new(CharacteristicType.Dwarf, "Dwarf", CharacteristicRarity.Uncommon, "Size, Strength - 30%", 0, CharacteristicType.Giant));
        characteristics.Add(new(CharacteristicType.MuscleDeficiency, "Muscle deficiency", CharacteristicRarity.Common, "Strength - 10", 0, CharacteristicType.Strongman, CharacteristicType.Powerhouse));
        characteristics.Add(new(CharacteristicType.Strongman, "Strongman", CharacteristicRarity.Common, "Strength + 10", 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Powerhouse));
        characteristics.Add(new(CharacteristicType.Powerhouse, "Powerhouse", CharacteristicRarity.Uncommon, "Strength + 20", 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman));
        characteristics.Add(new(CharacteristicType.Heavyfooted, "Heavyfooted", CharacteristicRarity.Common, "Agility - 10", 0, CharacteristicType.Lightfooted));
        characteristics.Add(new(CharacteristicType.Lightfooted, "Lightfooted", CharacteristicRarity.Common, "Agility + 10", 0, CharacteristicType.Heavyfooted));
        characteristics.Add(new(CharacteristicType.Assassin, "Assassin", CharacteristicRarity.Rare, "Agility + 10, Make less noise when farming and walking", 0));
        characteristics.Add(new(CharacteristicType.ClumsyFighter, "Clumsy fighter", CharacteristicRarity.Common, "Fighting - 10", 0, CharacteristicType.Brawler, CharacteristicType.Fighter));
        characteristics.Add(new(CharacteristicType.Brawler, "Brawler", CharacteristicRarity.Common, "Fighting + 10", 0, CharacteristicType.ClumsyFighter, CharacteristicType.Fighter));
        characteristics.Add(new(CharacteristicType.Fighter, "Fighter", CharacteristicRarity.Uncommon, "Fighting + 20", 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler));
        characteristics.Add(new(CharacteristicType.PoorAim, "Poor aim", CharacteristicRarity.Common, "Shooting - 10", 0, CharacteristicType.Sniper, CharacteristicType.Sharpshooter));
        characteristics.Add(new(CharacteristicType.Sniper, "Sniper", CharacteristicRarity.Common, "Shooting + 10", 0, CharacteristicType.PoorAim, CharacteristicType.Sharpshooter));
        characteristics.Add(new(CharacteristicType.Sharpshooter, "Sharpshooter", CharacteristicRarity.Uncommon, "Shooting + 20", 0, CharacteristicType.PoorAim, CharacteristicType.Sniper));
        characteristics.Add(new(CharacteristicType.Dunce, "Dunce", CharacteristicRarity.Common, "Knowledge - 10", 0, CharacteristicType.Smart, CharacteristicType.Genius));
        characteristics.Add(new(CharacteristicType.Smart, "Smart", CharacteristicRarity.Uncommon, "Knowledge + 10", 0, CharacteristicType.Dunce, CharacteristicType.Genius));
        characteristics.Add(new(CharacteristicType.Genius, "Genius", CharacteristicRarity.Rare, "Knowledge + 20", 0, CharacteristicType.Dunce, CharacteristicType.Smart));
        characteristics.Add(new(CharacteristicType.CarefulShooter, "Careful shooter", CharacteristicRarity.Uncommon, "Aiming delay +, Increase shot accuracy", 0));
        characteristics.Add(new(CharacteristicType.Fragile, "Fragile", CharacteristicRarity.Uncommon, "Increase injury rate/ Decrease recovery rate, hemostasis rate", 0, CharacteristicType.Sturdy));
        characteristics.Add(new(CharacteristicType.Sturdy, "Sturdy", CharacteristicRarity.Rare, "Decrease injury rate / Increase recovery rate, hemostasis rate", 0, CharacteristicType.Fragile));
        characteristics.Add(new(CharacteristicType.Avenger, "Avenger", CharacteristicRarity.Uncommon, "Increase fighting when bleeding/ Decrease hemostasis rate", 0));
        characteristics.Add(new(CharacteristicType.Regenerator, "Regenerator", CharacteristicRarity.Uncommon, "Increase hemostasis rate/ Blood and HP regenerate", 0));
        characteristics.Add(new(CharacteristicType.UpsAndDowns, "Ups and downs", CharacteristicRarity.Common, "Abilities increase or decrease depending on the condition of the day. (It will be determined when the battle royale starts)", 0));
        characteristics.Add(new(CharacteristicType.LuckGuy, "Luck Guy", CharacteristicRarity.Uncommon, "Luck + 50%", 0, CharacteristicType.Cursed, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Cursed, "Cursed", CharacteristicRarity.Uncommon, "Luck - 50%", 0, CharacteristicType.LuckGuy, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Blessed, "Blessed", CharacteristicRarity.Rare, "Luck + 100%", 0, CharacteristicType.LuckGuy, CharacteristicType.Cursed));
        characteristics.Add(new(CharacteristicType.ClumsyHand, "Clumsy hand", CharacteristicRarity.Common, "Crafting speed - 30%", 0, CharacteristicType.Dexterous, CharacteristicType.Engineer));
        characteristics.Add(new(CharacteristicType.Dexterous, "Dexterous", CharacteristicRarity.Common, "Crafting speed + 30%", 0, CharacteristicType.ClumsyHand, CharacteristicType.Engineer));
        characteristics.Add(new(CharacteristicType.Engineer, "Engineer", CharacteristicRarity.Uncommon, "Crafting speed + 60%", 0, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous));
        yield return null;
    }

    public static bool AddCharaicteristic(SurvivorData survivor, CharacteristicType wantCharacteristic, int rarity = -1)
    {
        int hasAleady = survivor.characteristics.FindIndex(x => x.type  == wantCharacteristic);
        if(hasAleady > -1) return false;
        foreach(Characteristic survivorChar in survivor.characteristics)
        {
            if(survivorChar.notPossbleTogether.ToList().Contains(wantCharacteristic))
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

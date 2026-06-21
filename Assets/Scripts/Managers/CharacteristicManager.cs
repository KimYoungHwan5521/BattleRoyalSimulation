using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public enum CharacteristicType
{
    HawkEye = 0,
    BadEye = 1,
    KeenHearing = 2,
    BadHearing = 3,
    ClutchPerformance = 4,
    ChokingUnderPressure = 5,
    MuscleDeficiency = 6,
    Strongman = 7,
    Powerhouse = 8,
    Heavyfooted = 9,
    Lightfooted = 10,
    Assassin = 11,
    ClumsyFighter = 12,
    Brawler = 13,
    Fighter = 14,
    PoorAim = 15,
    Sniper = 16,
    Sharpshooter = 17,
    Dunce = 18,
    Smart = 19,
    Genius = 20,
    Giant = 21,
    Dwarf = 22,
    BigMan = 23,
    CarefulShooter = 24,
    Fragile = 25,
    Sturdy = 26,
    Avenger = 27,
    Regenerator = 28,
    UpsAndDowns = 29,
    LuckGuy = 30,
    Cursed = 31,
    Blessed = 32,
    ClumsyHand = 33,
    Dexterous = 34,
    Engineer = 35,
    Fatty = 36,
    Soldier = 37,
    Boxer = 38,
    Luchador = 39,
    QuickDrawer = 40,

    // 1.4
    SwordSaint = 41,
    KnifeFighter = 42,
    MasterArcher = 43,
    TasteOfBlood = 44,
    FieldMedic = 45,
    TrapExpert = 46,
    PoisonImmune = 47,

    // 2.0
    DiceMan = 48,
    Coward = 49, 
    LethalWeapon = 50,
    StreetFighter = 51,
    ScentofBlood = 52,
    BodyEnhancementAdvocate = 53,
    AugmentationFanatic = 54,
    EasilyExhausted = 55,
    Tireless = 56,
    IronMan = 57,
    TwoHearts = 58,
    ThreeHearts = 59,
    Overzealous = 60,
    FastRecharge = 61,
    FastLearner = 62,
    Gifted = 63,
    Potential = 64,

    Physical100 = 65,
    MMAHeavyweightChampion = 66,
    Marine = 67,
    Blacksmith = 68,
    StrengthMage = 69,
    MMALightweightChampion = 70,
    MobileStrikeForce = 71,
    MacGyver = 72,
    SurvivalExpert = 73,
    Commando = 74,
    StateAlchemist = 75,
    CleverFighter = 76,
    WeaponsEngineer = 77,
    BallisticsMajor = 78,
    GeniusEngineer = 79,

    Prospect = 80,
    DarkHorse = 81,
    Zombie = 82,
    Vampire = 83,
    Challenger = 84,
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
        characteristics.Add(new(CharacteristicType.MuscleDeficiency, CharacteristicRarity.Common, 0, CharacteristicType.Strongman, CharacteristicType.Powerhouse, 
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage));
        characteristics.Add(new(CharacteristicType.Strongman, CharacteristicRarity.Common, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Powerhouse, 
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage));
        characteristics.Add(new(CharacteristicType.Powerhouse, CharacteristicRarity.Uncommon, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, 
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage));
        characteristics.Add(new(CharacteristicType.Heavyfooted, CharacteristicRarity.Common, 0, CharacteristicType.Lightfooted, CharacteristicType.Assassin, 
            CharacteristicType.Physical100, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert));
        characteristics.Add(new(CharacteristicType.Lightfooted, CharacteristicRarity.Common, 0, CharacteristicType.Heavyfooted, CharacteristicType.Assassin, 
            CharacteristicType.Physical100, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert));
        characteristics.Add(new(CharacteristicType.Assassin, CharacteristicRarity.Uncommon, 0, CharacteristicType.Lightfooted, CharacteristicType.Heavyfooted, 
            CharacteristicType.Physical100, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert));
        characteristics.Add(new(CharacteristicType.ClumsyFighter, CharacteristicRarity.Common, 0, CharacteristicType.Brawler, CharacteristicType.Fighter, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.MMALightweightChampion, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter));
        characteristics.Add(new(CharacteristicType.Brawler, CharacteristicRarity.Common, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Fighter, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.MMALightweightChampion, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter));
        characteristics.Add(new(CharacteristicType.Fighter, CharacteristicRarity.Uncommon, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.MMALightweightChampion, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter));
        characteristics.Add(new(CharacteristicType.PoorAim, CharacteristicRarity.Common, 0, CharacteristicType.Sniper, CharacteristicType.Sharpshooter, 
            CharacteristicType.Marine, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.Sniper, CharacteristicRarity.Common, 0, CharacteristicType.PoorAim, CharacteristicType.Sharpshooter, 
            CharacteristicType.Marine, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.Sharpshooter, CharacteristicRarity.Uncommon, 0, CharacteristicType.PoorAim, CharacteristicType.Sniper, 
            CharacteristicType.Marine, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.ClumsyHand, CharacteristicRarity.Common, 0, CharacteristicType.Dexterous, CharacteristicType.Engineer, 
            CharacteristicType.Blacksmith, CharacteristicType.MacGyver, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Dexterous, CharacteristicRarity.Common, 0, CharacteristicType.ClumsyHand, CharacteristicType.Engineer, 
            CharacteristicType.Blacksmith, CharacteristicType.MacGyver, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Engineer, CharacteristicRarity.Uncommon, 0, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, 
            CharacteristicType.Blacksmith, CharacteristicType.MacGyver, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Dunce, CharacteristicRarity.Common, 0, CharacteristicType.Smart, CharacteristicType.Genius, 
            CharacteristicType.StrengthMage, CharacteristicType.SurvivalExpert, CharacteristicType.CleverFighter, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Smart, CharacteristicRarity.Common, 0, CharacteristicType.Dunce, CharacteristicType.Genius, 
            CharacteristicType.StrengthMage, CharacteristicType.SurvivalExpert, CharacteristicType.CleverFighter, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Genius, CharacteristicRarity.Uncommon, 0, CharacteristicType.Dunce, CharacteristicType.Smart, 
            CharacteristicType.StrengthMage, CharacteristicType.SurvivalExpert, CharacteristicType.CleverFighter, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.CarefulShooter, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Fragile, CharacteristicRarity.Uncommon, 0, CharacteristicType.Sturdy));
        characteristics.Add(new(CharacteristicType.Sturdy, CharacteristicRarity.Rare, 0, CharacteristicType.Fragile));
        characteristics.Add(new(CharacteristicType.Avenger, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Regenerator, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.UpsAndDowns, CharacteristicRarity.Common, 0, CharacteristicType.DiceMan));
        characteristics.Add(new(CharacteristicType.LuckGuy, CharacteristicRarity.Uncommon, 0, CharacteristicType.Cursed, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Cursed, CharacteristicRarity.Uncommon, 0, CharacteristicType.LuckGuy, CharacteristicType.Blessed));
        characteristics.Add(new(CharacteristicType.Blessed, CharacteristicRarity.Rare, 0, CharacteristicType.LuckGuy, CharacteristicType.Cursed));
        characteristics.Add(new(CharacteristicType.Fatty, CharacteristicRarity.Common, 0));
        characteristics.Add(new(CharacteristicType.Soldier, CharacteristicRarity.Uncommon, 0, CharacteristicType.FieldMedic, CharacteristicType.Marine, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando));
        characteristics.Add(new(CharacteristicType.Boxer, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.Luchador, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.QuickDrawer, CharacteristicRarity.Uncommon, 0, CharacteristicType.CarefulShooter));
        // 1.4
        characteristics.Add(new(CharacteristicType.SwordSaint, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.KnifeFighter, CharacteristicRarity.Common, 0));
        characteristics.Add(new(CharacteristicType.MasterArcher, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.TasteOfBlood, CharacteristicRarity.Uncommon, 0, CharacteristicType.ScentofBlood));
        characteristics.Add(new(CharacteristicType.FieldMedic, CharacteristicRarity.Uncommon, 0, CharacteristicType.Soldier, CharacteristicType.Marine, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando));
        characteristics.Add(new(CharacteristicType.TrapExpert, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.PoisonImmune, CharacteristicRarity.Rare, 0));
        // 2.0
        characteristics.Add(new(CharacteristicType.DiceMan, CharacteristicRarity.Uncommon, 0, CharacteristicType.UpsAndDowns));
        characteristics.Add(new(CharacteristicType.Coward, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.LethalWeapon, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.StreetFighter, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.ScentofBlood, CharacteristicRarity.Rare, 0, CharacteristicType.TasteOfBlood));
        characteristics.Add(new(CharacteristicType.BodyEnhancementAdvocate, CharacteristicRarity.Uncommon, 0, CharacteristicType.AugmentationFanatic));
        characteristics.Add(new(CharacteristicType.AugmentationFanatic, CharacteristicRarity.Rare, 0, CharacteristicType.BodyEnhancementAdvocate));
        characteristics.Add(new(CharacteristicType.EasilyExhausted, CharacteristicRarity.Common, 0, CharacteristicType.Tireless, CharacteristicType.IronMan, CharacteristicType.Overzealous));
        characteristics.Add(new(CharacteristicType.Tireless, CharacteristicRarity.Uncommon, 0, CharacteristicType.EasilyExhausted, CharacteristicType.IronMan, CharacteristicType.Overzealous));
        characteristics.Add(new(CharacteristicType.IronMan, CharacteristicRarity.Rare, 0, CharacteristicType.Tireless, CharacteristicType.EasilyExhausted, CharacteristicType.Overzealous));
        characteristics.Add(new(CharacteristicType.TwoHearts, CharacteristicRarity.Common, 0, CharacteristicType.ThreeHearts));
        characteristics.Add(new(CharacteristicType.ThreeHearts, CharacteristicRarity.Rare, 0, CharacteristicType.TwoHearts));
        characteristics.Add(new(CharacteristicType.Overzealous, CharacteristicRarity.Common, 0, CharacteristicType.EasilyExhausted, CharacteristicType.Tireless, CharacteristicType.IronMan, CharacteristicType.Gifted, CharacteristicType.FastLearner));
        characteristics.Add(new(CharacteristicType.FastRecharge, CharacteristicRarity.Uncommon, 0));
        characteristics.Add(new(CharacteristicType.FastLearner, CharacteristicRarity.Uncommon, 0, CharacteristicType.Gifted, CharacteristicType.Overzealous));
        characteristics.Add(new(CharacteristicType.Gifted, CharacteristicRarity.Rare, 0, CharacteristicType.FastLearner, CharacteristicType.Overzealous));
        characteristics.Add(new(CharacteristicType.Potential, CharacteristicRarity.Uncommon, 0));

        characteristics.Add(new(CharacteristicType.Physical100, CharacteristicRarity.Rare, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, CharacteristicType.Powerhouse, CharacteristicType.Heavyfooted, CharacteristicType.Lightfooted, CharacteristicType.Assassin, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert));
        characteristics.Add(new(CharacteristicType.MMAHeavyweightChampion, CharacteristicRarity.Rare, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, CharacteristicType.Powerhouse, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, CharacteristicType.Fighter, 
            CharacteristicType.Physical100, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage, CharacteristicType.Commando, CharacteristicType.MMALightweightChampion, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter));
        characteristics.Add(new(CharacteristicType.Marine, CharacteristicRarity.Rare, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, CharacteristicType.Powerhouse, CharacteristicType.PoorAim, CharacteristicType.Sniper, CharacteristicType.Sharpshooter, CharacteristicType.Soldier, CharacteristicType.FieldMedic,
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Blacksmith, CharacteristicType.StrengthMage, CharacteristicType.WeaponsEngineer, CharacteristicType.MobileStrikeForce, CharacteristicType.Commando, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.Blacksmith, CharacteristicRarity.Rare, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, CharacteristicType.Powerhouse, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, CharacteristicType.Engineer,
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.StrengthMage, CharacteristicType.WeaponsEngineer, CharacteristicType.MacGyver, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.StrengthMage, CharacteristicRarity.Rare, 0, CharacteristicType.MuscleDeficiency, CharacteristicType.Strongman, CharacteristicType.Powerhouse, CharacteristicType.Dunce, CharacteristicType.Smart, CharacteristicType.Genius,
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.GeniusEngineer, CharacteristicType.SurvivalExpert, CharacteristicType.CleverFighter, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.MMALightweightChampion, CharacteristicRarity.Rare, 0, CharacteristicType.Heavyfooted, CharacteristicType.Lightfooted, CharacteristicType.Assassin, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, CharacteristicType.Fighter, 
            CharacteristicType.Physical100, CharacteristicType.MMAHeavyweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter));
        characteristics.Add(new(CharacteristicType.MobileStrikeForce, CharacteristicRarity.Rare, 0, CharacteristicType.Heavyfooted, CharacteristicType.Lightfooted, CharacteristicType.Assassin, CharacteristicType.PoorAim, CharacteristicType.Sniper, CharacteristicType.Sharpshooter, CharacteristicType.Soldier, CharacteristicType.FieldMedic, 
            CharacteristicType.Physical100, CharacteristicType.Marine, CharacteristicType.MMALightweightChampion, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert, CharacteristicType.Commando, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.MacGyver, CharacteristicRarity.Rare, 0, CharacteristicType.Heavyfooted, CharacteristicType.Lightfooted, CharacteristicType.Assassin, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, CharacteristicType.Engineer, 
            CharacteristicType.Physical100, CharacteristicType.Blacksmith, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.SurvivalExpert, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.SurvivalExpert, CharacteristicRarity.Rare, 0, CharacteristicType.Heavyfooted, CharacteristicType.Lightfooted, CharacteristicType.Assassin, CharacteristicType.Dunce, CharacteristicType.Smart, CharacteristicType.Genius, 
            CharacteristicType.Physical100, CharacteristicType.StrengthMage, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.CleverFighter, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.Commando, CharacteristicRarity.Rare, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, CharacteristicType.Fighter, CharacteristicType.PoorAim, CharacteristicType.Sniper, CharacteristicType.Sharpshooter,  CharacteristicType.Soldier, CharacteristicType.FieldMedic, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Marine, CharacteristicType.MMALightweightChampion, CharacteristicType.MobileStrikeForce, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));
        characteristics.Add(new(CharacteristicType.StateAlchemist, CharacteristicRarity.Rare, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, CharacteristicType.Fighter, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, CharacteristicType.Engineer, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.Blacksmith, CharacteristicType.MMALightweightChampion, CharacteristicType.MacGyver, CharacteristicType.Commando, CharacteristicType.CleverFighter, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.CleverFighter, CharacteristicRarity.Rare, 0, CharacteristicType.ClumsyFighter, CharacteristicType.Brawler, CharacteristicType.Fighter, CharacteristicType.Dunce, CharacteristicType.Smart, CharacteristicType.Genius, 
            CharacteristicType.MMAHeavyweightChampion, CharacteristicType.StrengthMage, CharacteristicType.MMALightweightChampion, CharacteristicType.SurvivalExpert, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.WeaponsEngineer, CharacteristicRarity.Rare, 0, CharacteristicType.PoorAim, CharacteristicType.Sniper, CharacteristicType.Sharpshooter, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, CharacteristicType.Engineer,
            CharacteristicType.Marine, CharacteristicType.Blacksmith, CharacteristicType.MobileStrikeForce, CharacteristicType.MacGyver, CharacteristicType.Commando, CharacteristicType.StateAlchemist, CharacteristicType.BallisticsMajor, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.BallisticsMajor, CharacteristicRarity.Rare, 0, CharacteristicType.PoorAim, CharacteristicType.Sniper, CharacteristicType.Sharpshooter, CharacteristicType.Dunce, CharacteristicType.Smart, CharacteristicType.Genius,
            CharacteristicType.Marine, CharacteristicType.StrengthMage, CharacteristicType.MobileStrikeForce, CharacteristicType.SurvivalExpert, CharacteristicType.Commando, CharacteristicType.CleverFighter, CharacteristicType.WeaponsEngineer, CharacteristicType.GeniusEngineer));
        characteristics.Add(new(CharacteristicType.GeniusEngineer, CharacteristicRarity.Rare, 0, CharacteristicType.ClumsyHand, CharacteristicType.Dexterous, CharacteristicType.Engineer, CharacteristicType.Dunce, CharacteristicType.Smart, CharacteristicType.Genius,
            CharacteristicType.Blacksmith, CharacteristicType.StrengthMage, CharacteristicType.MacGyver, CharacteristicType.SurvivalExpert, CharacteristicType.StateAlchemist, CharacteristicType.CleverFighter, CharacteristicType.WeaponsEngineer, CharacteristicType.BallisticsMajor));

        characteristics.Add(new(CharacteristicType.Prospect, CharacteristicRarity.Common, 0, CharacteristicType.DarkHorse, CharacteristicType.Challenger));
        characteristics.Add(new(CharacteristicType.DarkHorse, CharacteristicRarity.Uncommon, 0, CharacteristicType.Prospect, CharacteristicType.Challenger));
        characteristics.Add(new(CharacteristicType.Zombie, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.Vampire, CharacteristicRarity.Rare, 0));
        characteristics.Add(new(CharacteristicType.Challenger, CharacteristicRarity.Common, 0, CharacteristicType.Prospect, CharacteristicType.DarkHorse));
        yield return null;
    }

    public static bool AddCharaicteristic(SurvivorData survivor, CharacteristicType wantCharacteristic, int rarity = -1)
    {
        int hasAleady = survivor.characteristics.FindIndex(x => x.type  == wantCharacteristic);
        if(hasAleady > -1 || !UnlockCheck(wantCharacteristic)) return false;
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
        if (rand < 0.7f) wantRarity = 0;
        else if (rand < 0.9f) wantRarity = 1;
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

    public static bool UnlockCheck(CharacteristicType characteristic)
    {
        bool result;
        switch(characteristic)
        {
            case CharacteristicType.TrapExpert:
                SteamUserStats.GetAchievement("Sun Tzu", out result);
                break;
            case CharacteristicType.SwordSaint:
                SteamUserStats.GetAchievement("Sword Master", out result);
                break;
            case CharacteristicType.ClutchPerformance:
                SteamUserStats.GetAchievement("Overcome", out result);
                break;
            case CharacteristicType.PoisonImmune:
                SteamUserStats.GetAchievement("Viper", out result);
                break;
            case CharacteristicType.Assassin:
                SteamUserStats.GetAchievement("Quick-Footed", out result);
                break;
            case CharacteristicType.Avenger:
                SteamUserStats.GetAchievement("Bloody Hands", out result);
                break;
            case CharacteristicType.TasteOfBlood:
                SteamUserStats.GetAchievement("Notorious", out result);
                break;
            case CharacteristicType.QuickDrawer:
                SteamUserStats.GetAchievement("Sharpshooter", out result);
                break;
            case CharacteristicType.Dexterous:
                SteamUserStats.GetAchievement("Craftsman", out result);
                break;
            case CharacteristicType.Sharpshooter:
                SteamUserStats.GetAchievement("Gunslinger", out result);
                break;
            case CharacteristicType.Sniper:
                SteamUserStats.GetAchievement("Sniper", out result);
                break;
            case CharacteristicType.Fighter:
                SteamUserStats.GetAchievement("Bruce Lee", out result);
                break;
            case CharacteristicType.Regenerator:
                SteamUserStats.GetAchievement("Severe Bleeding", out result);
                break;
            case CharacteristicType.Genius:
                SteamUserStats.GetAchievement("Genius", out result);
                break;
            case CharacteristicType.UpsAndDowns:
                SteamUserStats.GetAchievement("Experience", out result);
                break;
            case CharacteristicType.Giant:
                SteamUserStats.GetAchievement("Gold Cup", out result);
                break;
            case CharacteristicType.Powerhouse:
                SteamUserStats.GetAchievement("Silver Cup", out result);
                break;
            case CharacteristicType.BigMan:
                SteamUserStats.GetAchievement("Bronze Cup", out result);
                break;
            case CharacteristicType.LuckGuy:
                SteamUserStats.GetAchievement("1 hour", out result);
                break;
            case CharacteristicType.Blessed:
                SteamUserStats.GetAchievement("Royal Loader", out result);
                break;
            default:
                return true;
        };
        return result;
    }
}

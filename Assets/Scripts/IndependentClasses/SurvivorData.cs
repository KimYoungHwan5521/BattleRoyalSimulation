using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public enum Tier { Bronze, Silver, Gold }

[Serializable]
public class SurvivorData
{
    public int id = -1;
    // Stats
    [SerializeField] string survivorName;
    public string SurvivorName => survivorName;
    public LocalizedString localizedSurvivorName;
    int _maxStamina;
    public int MaxStamina
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.TwoHearts)) return _maxStamina + 30;
            else if (HaveCharacteristic(CharacteristicType.ThreeHearts)) return _maxStamina + 100;
            return _maxStamina;
        }
    }
    public int _stamina;
    public int Stamina
    {
        get => _stamina;
        set
        {
            int before = _stamina;
            _stamina = Mathf.Clamp(value, 0, MaxStamina);
            if(_stamina - before > 0)
            {
                int changed = _stamina - before;
                // 회복
                PlayerPrefs.SetInt("Total Stamina Recovery", PlayerPrefs.GetInt("Total Stamina Recovery") + changed);
                AchievementManager.GetStat("Total_StaminaRecovery", out int original);
                AchievementManager.SetStat("Total_StaminaRecovery", original + changed);
            }
            else
            {
                // 소모
                int changed = before - _stamina;
                consumedStaminas += changed;
                PlayerPrefs.SetInt("Total Stamina Consumption", PlayerPrefs.GetInt("Total Stamina Consumption") + changed);
                AchievementManager.GetStat("Total_StaminaConsumption", out int original);
                AchievementManager.SetStat("Total_StaminaConsumption", original + changed);
            }
        }
    }
    public int staminaConsumption;
    public int _strength;
    public int _agility;
    public int _fighting;
    public int _shooting;
    public int _knowledge;
    public int _luck;
    public int _crafting;

    public int Strength
    {
        get
        {
            int result = _strength;
            if (HaveCharacteristic(CharacteristicType.MuscleDeficiency)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Strongman)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Powerhouse) || HaveCharacteristic(CharacteristicType.Physical100) || HaveCharacteristic(CharacteristicType.MMAHeavyweightChampion) 
                || HaveCharacteristic(CharacteristicType.Marine) || HaveCharacteristic(CharacteristicType.Blacksmith) || HaveCharacteristic(CharacteristicType.StrengthMage)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (HaveCharacteristic(CharacteristicType.Giant)) result = (int)(result * 1.3f);
            else if (HaveCharacteristic(CharacteristicType.Dwarf)) result = (int)(result * 0.7f);
            else if (HaveCharacteristic(CharacteristicType.BigMan)) result = (int)(result * 1.15f);

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Powerhouse");
            return Mathf.Max(result, 0);
        }
        set
        {
            if(HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _strength + Strength > 120) _strength = _strength + 120 - Strength;
                else _strength = Mathf.Max(0, value);
            }
            else
                _strength = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxStrength
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;
            int result = 100;
            if (HaveCharacteristic(CharacteristicType.MuscleDeficiency)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Strongman)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Powerhouse) || HaveCharacteristic(CharacteristicType.Physical100) || HaveCharacteristic(CharacteristicType.MMAHeavyweightChampion)
                || HaveCharacteristic(CharacteristicType.Marine) || HaveCharacteristic(CharacteristicType.Blacksmith) || HaveCharacteristic(CharacteristicType.StrengthMage)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (HaveCharacteristic(CharacteristicType.Giant)) result = (int)(result * 1.3f);
            else if (HaveCharacteristic(CharacteristicType.Dwarf)) result = (int)(result * 0.7f);
            else if (HaveCharacteristic(CharacteristicType.BigMan)) result = (int)(result * 1.15f);
            return result;
        }
    }

    public int Agility
    {
        get
        {
            int result = _agility;
            if (HaveCharacteristic(CharacteristicType.Heavyfooted)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Lightfooted)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Assassin) || HaveCharacteristic(CharacteristicType.MMALightweightChampion) || HaveCharacteristic(CharacteristicType.MobileStrikeForce)
                 || HaveCharacteristic(CharacteristicType.MacGyver) || HaveCharacteristic(CharacteristicType.SurvivalExpert) || HaveCharacteristic(CharacteristicType.Physical100)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result -= 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Quick-Footed");
            return Mathf.Max(result, 0);
        }
        set
        {
            if (HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _agility + Agility > 120) _agility = _agility + 120 - Agility;
                else _agility = Mathf.Max(0, value);
            }
            else
                _agility = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxAgility
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;

            int result = 100;
            if (HaveCharacteristic(CharacteristicType.Heavyfooted)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Lightfooted)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Assassin) || HaveCharacteristic(CharacteristicType.MMALightweightChampion) || HaveCharacteristic(CharacteristicType.MobileStrikeForce)
                 || HaveCharacteristic(CharacteristicType.MacGyver) || HaveCharacteristic(CharacteristicType.SurvivalExpert) || HaveCharacteristic(CharacteristicType.Physical100)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Fatty)) result -= 10;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;
            return result;
        }
    }

    public int Fighting
    {
        get
        {
            int result = _fighting;
            if (HaveCharacteristic(CharacteristicType.ClumsyFighter)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Brawler)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Fighter) || HaveCharacteristic(CharacteristicType.MMAHeavyweightChampion) || HaveCharacteristic(CharacteristicType.MMALightweightChampion)
                 || HaveCharacteristic(CharacteristicType.Commando) || HaveCharacteristic(CharacteristicType.StateAlchemist) || HaveCharacteristic(CharacteristicType.CleverFighter)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Boxer)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Martial Artist");
            return Mathf.Max(result, 0);
        }
        set
        {
            if (HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _fighting + Fighting > 120) _fighting = _fighting + 120 - Fighting;
                else _fighting = Mathf.Max(0, value);
            }
            else
                _fighting = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxFighting
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;

            int result = 100;
            if (HaveCharacteristic(CharacteristicType.ClumsyFighter)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Brawler)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Fighter) || HaveCharacteristic(CharacteristicType.MMAHeavyweightChampion) || HaveCharacteristic(CharacteristicType.MMALightweightChampion)
                 || HaveCharacteristic(CharacteristicType.Commando) || HaveCharacteristic(CharacteristicType.StateAlchemist) || HaveCharacteristic(CharacteristicType.CleverFighter)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Boxer)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Luchador)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;
            return result;
        }
    }

    public int Shooting
    {
        get
        {
            int result = _shooting;
            if (HaveCharacteristic(CharacteristicType.PoorAim)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Sniper)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Sharpshooter) || HaveCharacteristic(CharacteristicType.Marine) || HaveCharacteristic(CharacteristicType.MobileStrikeForce)
                 || HaveCharacteristic(CharacteristicType.Commando) || HaveCharacteristic(CharacteristicType.WeaponsEngineer) || HaveCharacteristic(CharacteristicType.BallisticsMajor)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.FieldMedic)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Sharpshooter");
            return Mathf.Max(result, 0);
        }
        set
        {
            if (HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _shooting + Shooting > 120) _shooting = _shooting + 120 - Shooting;
                else _shooting = Mathf.Max(0, value);
            }
            else
                _shooting = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxShooting
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;

            int result = 100;
            if (HaveCharacteristic(CharacteristicType.PoorAim)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Sniper)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Sharpshooter) || HaveCharacteristic(CharacteristicType.Marine) || HaveCharacteristic(CharacteristicType.MobileStrikeForce)
                 || HaveCharacteristic(CharacteristicType.Commando) || HaveCharacteristic(CharacteristicType.WeaponsEngineer) || HaveCharacteristic(CharacteristicType.BallisticsMajor)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Soldier)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.FieldMedic)) result += 10;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;
            return result;
        }
    }

    public int Crafting
    {
        get
        {
            int result = _crafting;
            if (HaveCharacteristic(CharacteristicType.ClumsyHand)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Dexterous)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Engineer) || HaveCharacteristic(CharacteristicType.Blacksmith) || HaveCharacteristic(CharacteristicType.MacGyver)
                 || HaveCharacteristic(CharacteristicType.StateAlchemist) || HaveCharacteristic(CharacteristicType.WeaponsEngineer) || HaveCharacteristic(CharacteristicType.GeniusEngineer)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Engineer");
            return Mathf.Max(result, 0);
        }
        set
        {
            if (HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _crafting + Crafting > 120) _crafting = _crafting + 120 - Crafting;
                else _crafting = Mathf.Max(0, value);
            }
            else
                _crafting = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxCrafting
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;

            int result = 100;
            if (HaveCharacteristic(CharacteristicType.ClumsyHand)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Dexterous)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Engineer) || HaveCharacteristic(CharacteristicType.Blacksmith) || HaveCharacteristic(CharacteristicType.MacGyver)
                 || HaveCharacteristic(CharacteristicType.StateAlchemist) || HaveCharacteristic(CharacteristicType.WeaponsEngineer) || HaveCharacteristic(CharacteristicType.GeniusEngineer)) result += 20;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;
            return result;
        }
    }

    public int Knowledge
    {
        get
        {
            int result = _knowledge;
            if (HaveCharacteristic(CharacteristicType.Dunce)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Smart)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Genius) || HaveCharacteristic(CharacteristicType.StrengthMage) || HaveCharacteristic(CharacteristicType.SurvivalExpert)
                 || HaveCharacteristic(CharacteristicType.CleverFighter) || HaveCharacteristic(CharacteristicType.BallisticsMajor) || HaveCharacteristic(CharacteristicType.GeniusEngineer)) result += 20;
            if (HaveCharacteristic(CharacteristicType.FieldMedic)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;

            if (id >= 0 && result >= 100) AchievementManager.UnlockAchievement("Genius");
            return Mathf.Max(result, 0);
        }
        set
        {
            if (HaveCharacteristic(CharacteristicType.Potential))
            {
                if (value - _knowledge + Knowledge > 120) _knowledge = _knowledge + 120 - Knowledge;
                else _knowledge = Mathf.Max(0, value);
            }
            else
                _knowledge = Mathf.Clamp(value, 0, 100);
        }
    }

    public int MaxKnowledge
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.Potential)) return 120;

            int result = 100;
            if (HaveCharacteristic(CharacteristicType.Dunce)) result -= 10;
            else if (HaveCharacteristic(CharacteristicType.Smart)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Genius) || HaveCharacteristic(CharacteristicType.StrengthMage) || HaveCharacteristic(CharacteristicType.SurvivalExpert)
                 || HaveCharacteristic(CharacteristicType.CleverFighter) || HaveCharacteristic(CharacteristicType.BallisticsMajor) || HaveCharacteristic(CharacteristicType.GeniusEngineer)) result += 20;
            if (HaveCharacteristic(CharacteristicType.FieldMedic)) result += 5;
            if (HaveCharacteristic(CharacteristicType.Prospect)) result += 5;
            else if (HaveCharacteristic(CharacteristicType.DarkHorse)) result += 10;
            else if (HaveCharacteristic(CharacteristicType.Challenger)) result -= 10;
            return result;
        }
    }

    public int StatTotal => _strength + _agility + _fighting + _shooting + _crafting + _knowledge;


    public int Luck
    {
        get
        {
            int result = _luck;
            if(HaveCharacteristic(CharacteristicType.LuckGuy)) result += 25;
            else if(HaveCharacteristic(CharacteristicType.Cursed)) result -= 25;
            else if(HaveCharacteristic(CharacteristicType.Blessed)) result += 50;
            return result;
        }
    }

    public List<Characteristic> characteristics = new();
    public int price;

    public bool HaveCharacteristic(CharacteristicType characteristic)
    {
        return characteristics.FindIndex(x => x.type == characteristic) > -1;
    }

    bool ClutchThePerformance
    {
        get
        {
            if(HaveCharacteristic(CharacteristicType.ClutchPerformance))
            {
                Calendar calendar = GameManager.Instance.Calendar;
                if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
                {
                    League league = calendar.LeagueReserveInfo[calendar.Today].league;
                    if (league == League.SeasonChampionship || league == League.WorldChampionship) return true;
                }
            }
            return false;
        }
    }

    bool ChockingUnderPressure
    {
        get
        {
            if (HaveCharacteristic(CharacteristicType.ChokingUnderPressure))
            {
                Calendar calendar = GameManager.Instance.Calendar;
                if (calendar.LeagueReserveInfo.ContainsKey(calendar.Today))
                {
                    League league = calendar.LeagueReserveInfo[calendar.Today].league;
                    if (league == League.SeasonChampionship || league == League.WorldChampionship) return true;
                }
            }
            return false;
        }
    }
    
    // League
    public Tier tier;
    public bool isReserved;
    public int reservedDate = -1;
    //public bool haveQualifyToParticipateInSeasonChampionship;
    //public bool haveQualifyToParticipateInWorldChampionship;

    // Training
    public int increaseComparedToPrevious_strength = 0;
    public int increaseComparedToPrevious_agility = 0;
    public int increaseComparedToPrevious_fighting = 0;
    public int increaseComparedToPrevious_shooting = 0;
    public int increaseComparedToPrevious_crafting = 0;
    public int increaseComparedToPrevious_knowledge = 0;
    public int increaseComparedToPrevious_stamina = 0;

    // Injury, Surgery
    public List<Injury> injuries = new();
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public LocalizedString localizedScheduledSurgeryName;
    public int scheduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;

    // Strategy
    public ItemManager.Items priority1Weapon = ItemManager.Items.LASER;
    public ItemManager.Items priority2Weapon = ItemManager.Items.AssaultRifle;
    public Dictionary<StrategyCase, StrategyData> strategyDictionary = new();
    [SerializeField] public ItemManager.Craftable priority1Crafting = null;
    [SerializeField] public ItemManager.Craftable priority2Crafting = null;
    public int priority1CraftingToInt = -1;
    public int priority2CraftingToInt = -1;
    public bool[] craftingAllows;
    public int repairCondition = 70;

    // Statistics
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
    public bool royalLoader = true;
    public int receivedTrainings;
    public int consumedStaminas;

    public SurvivorData(LocalizedString localizedSurvivorName, int strength, int agility, int fighting, int shooting, int crafting, int knowledge, int price, Tier tier)
    {
        this.localizedSurvivorName = localizedSurvivorName;
        survivorName = localizedSurvivorName.TableEntryReference.Key;
        _maxStamina = 100;
        _strength = strength;
        _agility = agility;
        _fighting = fighting;
        _shooting = shooting;
        _crafting = crafting;
        _knowledge = knowledge;
        _luck = 50;
        this.price = price;
        this.tier = tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for(int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public SurvivorData(SurvivorData survivorData)
    {
        survivorName = survivorData.survivorName;
        localizedSurvivorName = new LocalizedString("Name", survivorName);
        _maxStamina = 100;
        _strength = survivorData._strength;
        _agility = survivorData._agility;
        _fighting = survivorData._fighting;
        _shooting = survivorData._shooting;
        _crafting = survivorData._crafting;
        _knowledge = survivorData._knowledge;
        _luck = survivorData._luck;
        price = survivorData.price;
        tier = survivorData.tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for (int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public void IncreaseStats(int strength, int agility, int fighting, int shooting, int crafting, int knowledge)
    {
        Strength = _strength + strength;
        Agility = _agility + agility;
        Fighting = _fighting + fighting;
        Shooting = _shooting + shooting;
        Crafting = _crafting + crafting;
        Knowledge = _knowledge + knowledge;

        //increaseComparedToPrevious_strength += strength;
        //increaseComparedToPrevious_agility += agility;
        //increaseComparedToPrevious_fighting += fighting;
        //increaseComparedToPrevious_shooting += shooting;
        //increaseComparedToPrevious_crafting += crafting;
        //increaseComparedToPrevious_knowledge += knowledge;
    }

    public void IncreaseStatsReserve(int strength, int agility, int fighting, int shooting, int crafting, int knowledge)
    {
        increaseComparedToPrevious_strength += strength;
        increaseComparedToPrevious_agility += agility;
        increaseComparedToPrevious_fighting += fighting;
        increaseComparedToPrevious_shooting += shooting;
        increaseComparedToPrevious_crafting += crafting;
        increaseComparedToPrevious_knowledge += knowledge;
    }

    public void StaminaConsomtionReserve(int value)
    {
        increaseComparedToPrevious_stamina += value;
    }
}

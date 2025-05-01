using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tier { Bronze, Silver, Gold }

[Serializable]
public class SurvivorData
{
    public int id = -1;
    // Stats
    public string survivorName;
    public int _strength;
    public int _agility;
    public int _fighting;
    public int _shooting;
    public int _knowledge;

    public float MaxHp => _strength + 100;
    public float AttackDamage => (120f + _strength + _fighting) / 16f;
    public float AttackSpeed => (120f + _agility + _fighting) / 160f;
    public float MoveSpeed => (60f + _agility) * 3f / 80f;
    public float FarmingSpeed => (60f + _agility) / 80f;
    public float Shooting => _shooting / 20f;
    public float luck;
    public List<Characteristic> characteristics = new();
    public int price;
    
    // League
    public Tier tier;
    public bool isReserved;
    
    // Training
    public Training assignedTraining;
    public float increaseComparedToPrevious_strength;
    public int increaseComparedToPrevious_agility;
    public int increaseComparedToPrevious_fighting;
    public int increaseComparedToPrevious_shooting;
    public int increaseComparedToPrevious_knowledge;

    // Injury, Surgery
    public List<Injury> injuries = new();
    public bool surgeryScheduled;
    public string scheduledSurgeryName;
    public int shceduledSurgeryCost;
    public InjurySite surgerySite;
    public SurgeryType surgeryType;
    public CharacteristicType surgeryCharacteristic;

    // Strategy
    public ItemManager.Items priority1Weapon = ItemManager.Items.SniperRifle;
    public Dictionary<StrategyCase, StrategyData> strategyDictionary = new();
    [SerializeField] public ItemManager.Craftable priority1Crafting = null;
    public int priority1CraftingToInt = -1;
    public bool[] craftingAllows;

    public SurvivorData(string survivorName, int strength, int agility, int fighting, int shooting, int knowledge, int price, Tier tier)
    {
        this.survivorName = survivorName;
        _strength = strength;
        _agility = agility;
        _fighting = fighting;
        _shooting = shooting;
        _knowledge = knowledge;
        luck = 50;
        this.price = price;
        this.tier = tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for(int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public SurvivorData(SurvivorData survivorData)
    {
        survivorName = survivorData.survivorName;
        _strength = survivorData._strength;
        _agility = survivorData._agility;
        _fighting = survivorData._fighting;
        _shooting = survivorData._shooting;
        _knowledge = survivorData._knowledge;
        luck = survivorData.luck;
        price = survivorData.price;
        tier = survivorData.tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
        craftingAllows = new bool[ItemManager.craftables.Count];
        for (int i = 0; i < craftingAllows.Length; i++) craftingAllows[i] = true;
    }

    public void IncreaseStats(int strength, int agility, int fighting, int shooting, int knowledge)
    {
        _strength += strength;
        _agility += agility;
        _fighting += fighting;
        _shooting += shooting;
        _knowledge += knowledge;

        increaseComparedToPrevious_strength += strength;
        increaseComparedToPrevious_agility += agility;
        increaseComparedToPrevious_fighting += fighting;
        increaseComparedToPrevious_shooting += shooting;
        increaseComparedToPrevious_knowledge += knowledge;
    }
}

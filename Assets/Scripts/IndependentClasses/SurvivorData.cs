using System;
using System.Collections;
using System.Collections.Generic;

public enum Tier { Bronze, Silver, Gold }

[Serializable]
public class SurvivorData
{
    public int id = -1;
    // Stats
    public string survivorName;
    public float hp;
    public int _power;
    public float AttackDamage => (60f + _power) / 8f;
    public int _attackSpeed;
    public float AttackSpeed => (60f + _attackSpeed) / 80f;
    public int _moveSpeed;
    public float MoveSpeed => (60f + _moveSpeed) * 3f / 80f;
    public int _farmingSpeed;
    public float FarmingSpeed => (60f + _farmingSpeed) / 80f;
    public int _shooting;
    public float Shooting => _shooting / 20f;
    public float luck;
    public List<Characteristic> characteristics = new();
    public int price;
    
    // League
    public Tier tier;
    public bool isReserved;
    
    // Training
    public Training assignedTraining;
    public float increaseComparedToPrevious_hp;
    public int increaseComparedToPrevious_power;
    public int increaseComparedToPrevious_attackSpeed;
    public int increaseComparedToPrevious_moveSpeed;
    public int increaseComparedToPrevious_farmingSpeed;
    public int increaseComparedToPrevious_shooting;

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

    public SurvivorData(string survivorName, float hp, int power, int attackSpeed, int moveSpeed,
        int farmingSpeed, int shooting, int price, Tier tier)
    {
        this.survivorName = survivorName;
        this.hp = hp;
        _power = power;
        _attackSpeed = attackSpeed;
        _moveSpeed = moveSpeed;
        _farmingSpeed = farmingSpeed;
        _shooting = shooting;
        luck = 50;
        this.price = price;
        this.tier = tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
    }

    public SurvivorData(SurvivorData survivorData)
    {
        survivorName = survivorData.survivorName;
        hp = survivorData.hp;
        _power = survivorData._power;
        _attackSpeed = survivorData._attackSpeed;
        _moveSpeed = survivorData._moveSpeed;
        _farmingSpeed = survivorData._farmingSpeed;
        _shooting = survivorData._shooting;
        luck = survivorData.luck;
        price = survivorData.price;
        tier = survivorData.tier;
        Strategy.ResetStrategyDictionary(strategyDictionary);
    }

    public void IncreaseStats(float hp, int power, int attackSpeed, int moveSpeed, int farmingSpeed, int shooting)
    {
        this.hp += hp;
        _power += power;
        _attackSpeed += attackSpeed;
        _moveSpeed += moveSpeed;
        _farmingSpeed += farmingSpeed;
        _shooting += shooting;

        increaseComparedToPrevious_hp += hp;
        increaseComparedToPrevious_power += power;
        increaseComparedToPrevious_attackSpeed += attackSpeed;
        increaseComparedToPrevious_moveSpeed += moveSpeed;
        increaseComparedToPrevious_farmingSpeed += farmingSpeed;
        increaseComparedToPrevious_shooting += shooting;
    }
}

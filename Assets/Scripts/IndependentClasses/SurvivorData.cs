using System;
using System.Collections;
using System.Collections.Generic;

public enum Tier { Bronze, Silver, Gold }

public class StrategyData
{
    public int action = 0;
    public int elseAction = 0;
    public int conditionConut = 0;
    public ConditionData[] conditions;

    public StrategyData(int action, int elseAction, int conditionConut, ConditionData[] conditions = null)
    {
        this.action = action;
        this.elseAction = elseAction;
        this.conditionConut = conditionConut;
        if(conditions == null)
        {
            this.conditions = new ConditionData[5];
            for (int i = 0; i < this.conditions.Length; i++) this.conditions[i] = new(0, 0, 0, 0, 0);
        }
        else this.conditions = conditions;
    }
}

[Serializable]
public class SurvivorData
{
    // Stats
    public string survivorName;
    public float hp;
    public float attackDamage;
    public float attackSpeed;
    public float moveSpeed;
    public float farmingSpeed;
    public float shooting;
    public float luck;
    public List<Characteristic> characteristics = new();
    public int price;
    
    // League
    public Tier tier;
    public bool isReserved;
    
    // Training
    public Training assignedTraining;
    public float increaseComparedToPrevious_hp;
    public float increaseComparedToPrevious_attackDamage;
    public float increaseComparedToPrevious_attackSpeed;
    public float increaseComparedToPrevious_moveSpeed;
    public float increaseComparedToPrevious_farmingSpeed;
    public float increaseComparedToPrevious_shooting;

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

    void SetStrategyDictionary()
    {
        strategyDictionary.Add(StrategyCase.SawAnEnemyAndItIsInAttackRange, new(0, 0, 0));
        strategyDictionary.Add(StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange, new(0, 0, 0));
        strategyDictionary.Add(StrategyCase.HeardDistinguishableSound, new(0, 0, 0));
        strategyDictionary.Add(StrategyCase.HeardIndistinguishableSound, new(1, 1, 0));
    }

    public SurvivorData(string survivorName, float hp, float attackDamage, float attackSpeed, float moveSpeed,
        float farmingSpeed, float shooting, int price, Tier tier)
    {
        this.survivorName = survivorName;
        this.hp = hp;
        this.attackDamage = attackDamage;
        this.attackSpeed = attackSpeed;
        this.moveSpeed = moveSpeed;
        this.farmingSpeed = farmingSpeed;
        this.shooting = shooting;
        luck = 50;
        this.price = price;
        this.tier = tier;
        SetStrategyDictionary();
    }

    public SurvivorData(SurvivorData survivorData)
    {
        survivorName = survivorData.survivorName;
        hp = survivorData.hp;
        attackDamage = survivorData.attackDamage;
        attackSpeed = survivorData.attackSpeed;
        moveSpeed = survivorData.moveSpeed;
        farmingSpeed = survivorData.farmingSpeed;
        shooting = survivorData.shooting;
        luck = survivorData.luck;
        price = survivorData.price;
        tier = survivorData.tier;
        SetStrategyDictionary();
    }

    public void IncreaseStats(float hp, float attackDamage, float attackSpeed, float moveSpeed, float farmingSpeed, float shooting)
    {
        this.hp += hp;
        this.attackDamage += attackDamage;
        this.attackSpeed += attackSpeed;
        this.moveSpeed += moveSpeed;
        this.farmingSpeed += farmingSpeed;
        this.shooting += shooting;

        increaseComparedToPrevious_hp += hp;
        increaseComparedToPrevious_attackDamage += attackDamage;
        increaseComparedToPrevious_attackSpeed += attackSpeed;
        increaseComparedToPrevious_moveSpeed += moveSpeed;
        increaseComparedToPrevious_farmingSpeed += farmingSpeed;
        increaseComparedToPrevious_shooting += shooting;
    }
}

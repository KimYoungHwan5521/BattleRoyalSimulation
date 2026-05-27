using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ItemManager
{
    public enum Items
    {
        NotValid = 0,

        // Melee Weapons
        Knife = 1,
        Dagger = 2,
        Bat = 3,
        LongSword = 4,
        Shovel = 5,

        // Ranged Weapons
        Revolver = 6,
        Pistol = 7,
        AssaultRifle = 8,
        SubMachineGun = 9,
        ShotGun = 10,
        SniperRifle = 11,
        Bazooka = 12,
        LASER = 13,
        Bow = 14,
        AdvancedBow = 15,

        // Enchanted Weapons
        Knife_Enchanted = 16,
        Dagger_Enchanted = 17,
        LongSword_Enchanted = 18,

        // Bullets
        Bullet_Revolver = 19,
        Bullet_Pistol = 20,
        Bullet_AssaultRifle = 21,
        Bullet_SubMachineGun = 22,
        Bullet_ShotGun = 23,
        Bullet_SniperRifle = 24,
        Rocket_Bazooka = 25,
        Arrow = 26,
        Arrow_Enchanted = 27,

        // BulletproofHats
        LowLevelBulletproofHelmet = 28,
        MiddleLevelBulletproofHelmet = 29,
        HighLevelBulletproofHelmet = 30,
        LegendaryBulletproofHelmet = 31,

        // BulletproofVests
        LowLevelBulletproofVest = 32,
        MiddleLevelBulletproofVest = 33,
        HighLevelBulletproofVest = 34,
        LegendaryBulletproofVest = 35,

        // Consumables
        BandageRoll = 36,
        HemostaticBandageRoll = 37,
        Poison = 38,
        Antidote = 39,
        Potion = 40,
        AdvancedPotion = 41,

        // Crafting Materials
        Components = 42,
        AdvancedComponent = 43,
        Chemicals = 44,
        Gunpowder = 45,
        Salvages = 46,

        // Traps
        BearTrap = 47,
        BearTrap_Enchanted = 48,
        LandMine = 49,
        NoiseTrap = 50,
        ChemicalTrap = 51,
        ShrapnelTrap = 52,
        ExplosiveTrap = 53,

        // ETC
        WalkingAid = 54,
        TrapDetectionDevice = 55,
        BiometricRader = 56,
        EnergyBarrier = 57,
    }

    public static Dictionary<Items, List<Item>> itemDictionary = new();

    [Serializable]
    public class Craftable
    {
        public Items itemType;
        public int requiredKnowledge;
        public int needAdvancedComponentCount;
        public int needComponentsCount;
        public int needChemicalsCount;
        public int needSalvagesCount;
        public int needGunpowderCount;
        public int outputAmount;
        public int craftingAnimNumber;
        public Dictionary<Items, int> etcNeedItems = new();
        public float craftingTime;

        public Craftable(Items itemType, int requiredKnowledge, int needAdvancedComponentCount, int needComponentsCount, int needChemicalsCount, int needSalvagesCount, int needGunpowderCount, int outputAmount, int craftingAnimNumber, float craftingTime, params KeyValuePair<Items, int>[] etcNeedItems)
        {
            this.itemType = itemType;
            this.requiredKnowledge = requiredKnowledge;
            this.needAdvancedComponentCount = needAdvancedComponentCount;
            this.needComponentsCount = needComponentsCount;
            this.needChemicalsCount = needChemicalsCount;
            this.needSalvagesCount = needSalvagesCount;
            this.needGunpowderCount = needGunpowderCount;
            this.outputAmount = outputAmount;
            this.craftingAnimNumber = craftingAnimNumber;
            this.craftingTime = craftingTime;
            foreach(var needItem in  etcNeedItems) this.etcNeedItems.Add(needItem.Key, needItem.Value);
        }
    }

    public static List<Craftable> craftables = new();

    public IEnumerator Initiate()
    {
        // Crafting anim number - 0 : Crafting, 1 : Chemicals, 2 : Enchant
        craftables.Add(new Craftable(Items.WalkingAid, 5, 0, 1, 0, 2, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Bow, 7, 0, 0, 0, 2, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Arrow, 7, 0, 0, 0, 1, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Poison, 10, 0, 0, 2, 3, 0, 3, 1, 3.5f));
        craftables.Add(new Craftable(Items.Revolver, 15, 0, 2, 0, 4, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Antidote, 20, 0, 0, 2, 0, 0, 1, 1, 3.5f));
        craftables.Add(new Craftable(Items.Pistol, 25, 0, 2, 0, 4, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.HemostaticBandageRoll, 30, 0, 0, 2, 0, 0, 1, 1, 3.5f, new KeyValuePair<Items, int>(Items.BandageRoll, 1)));
        craftables.Add(new Craftable(Items.AdvancedBow, 32, 0, 2, 0, 2, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.ShotGun, 35, 0, 4, 0, 4, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.Bullet_Pistol, 40, 0, 0, 0, 1, 1, 2, 0, 7f));
        craftables.Add(new Craftable(Items.Bullet_SubMachineGun, 40, 0, 0, 0, 1, 1, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Bullet_Revolver, 40, 0, 0, 0, 1, 1, 4, 0, 7f));
        craftables.Add(new Craftable(Items.Bullet_AssaultRifle, 40, 0, 0, 0, 2, 2, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Bullet_SniperRifle, 40, 0, 0, 0, 1, 2, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Bullet_ShotGun, 40, 0, 0, 0, 3, 1, 1, 0, 7f));
        craftables.Add(new Craftable(Items.SubMachineGun, 45, 0, 4, 0, 4, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.LowLevelBulletproofHelmet, 48, 0, 0, 0, 7, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Potion, 50, 0, 0, 3, 1, 0, 1, 1, 3.5f));
        craftables.Add(new Craftable(Items.LowLevelBulletproofVest, 52, 0, 0, 0, 10, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.BearTrap, 53, 0, 2, 0, 3, 0, 3, 0, 10f));
        craftables.Add(new Craftable(Items.Rocket_Bazooka, 55, 0, 1, 0, 0, 3, 1, 0, 7f));
        craftables.Add(new Craftable(Items.AssaultRifle, 60, 0, 6, 0, 4, 0, 1, 0, 18f));
        craftables.Add(new Craftable(Items.NoiseTrap, 65, 1, 0, 0, 3, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.MiddleLevelBulletproofHelmet, 68, 0, 3, 0, 8, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.SniperRifle, 70, 0, 6, 0, 4, 0, 1, 0, 18f));
        craftables.Add(new Craftable(Items.MiddleLevelBulletproofVest, 68, 0, 3, 0, 11, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.LandMine, 73, 1, 1, 0, 1, 2, 3, 0, 10f));
        craftables.Add(new Craftable(Items.ChemicalTrap, 77, 0, 1, 0, 0, 0, 1, 1, 7, new KeyValuePair<Items, int>(Items.Poison, 3)));
        craftables.Add(new Craftable(Items.Bazooka, 80, 0, 8, 0, 4, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.ShrapnelTrap, 81, 0, 1, 0, 6, 1, 1, 0, 7f));
        craftables.Add(new Craftable(Items.ExplosiveTrap, 85, 1, 0, 1, 0, 3, 1, 0, 14f));
        craftables.Add(new Craftable(Items.HighLevelBulletproofHelmet, 88, 0, 6, 0, 9, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.HighLevelBulletproofVest, 92, 0, 6, 0, 12, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.BiometricRader, 96, 1, 8, 0, 2, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.TrapDetectionDevice, 100, 2, 5, 0, 2, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.AdvancedPotion, 104, 0, 0, 3, 0, 0, 1, 1, 3.5f, new KeyValuePair<Items, int>(Items.Potion, 1)));
        craftables.Add(new Craftable(Items.LegendaryBulletproofHelmet, 108, 0, 6, 0, 0, 0, 1, 0, 35f, new KeyValuePair<Items, int>(Items.HighLevelBulletproofHelmet, 1), new KeyValuePair<Items, int>(Items.MiddleLevelBulletproofHelmet, 2), new KeyValuePair<Items, int>(Items.LowLevelBulletproofHelmet, 4)));
        craftables.Add(new Craftable(Items.AdvancedComponent, 110, 0, 4, 0, 0, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.LegendaryBulletproofVest, 112, 0, 6, 0, 0, 0, 1, 0, 35f, new KeyValuePair<Items, int>(Items.HighLevelBulletproofVest, 1), new KeyValuePair<Items, int>(Items.MiddleLevelBulletproofVest, 2), new KeyValuePair<Items, int>(Items.LowLevelBulletproofVest, 4)));
        craftables.Add(new Craftable(Items.EnergyBarrier, 116, 2, 4, 0, 8, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.LASER, 120, 2, 8, 2, 4, 0, 1, 0, 21f));
        yield return null;
    }

    public static void AddItems(Items wantItem, int count, CraftingQuality quality = CraftingQuality.NotCrafted)
    {
        int start = 0;
        int end = count;
        if(!itemDictionary.ContainsKey(wantItem))
        {
            itemDictionary.Add(wantItem, new List<Item>());
        }
        else
        {
            itemDictionary[wantItem].AddRange(new Item[count]);
            start += itemDictionary[wantItem].Count;
            end += itemDictionary[wantItem].Count;
        }

        switch(wantItem)
        {
            // Melee Weapons
            case Items.Knife:
                for(int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.5f, NeedHand.OneHand, DamageType.Slash, 30, 1.7f, 0, quality));
                break;
            case Items.Dagger:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 1f, NeedHand.OneHand, DamageType.Slash, 40, 2f, 1, quality));
                break;
            case Items.Bat:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 1f, NeedHand.OneOrTwoHand, DamageType.Strike, 35, 2f, 1, quality));
                break;
            case Items.LongSword:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 2f, NeedHand.OneOrTwoHand, DamageType.Slash, 50, 2.4f, 1, quality));
                break;
            case Items.Shovel:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 2f, NeedHand.OneOrTwoHand, DamageType.Strike, 45, 2f, 1, quality));
                break;
            // Ranged Weapons
            case Items.Pistol:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch(quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 60;
                            attackRange = 25.1f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 50;
                            attackRange = 22.6f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 30;
                            attackRange = 17.6f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 20;
                            attackRange = 15.1f;
                            break;
                        default:
                            attackDamage = 40;
                            attackRange = 20.1f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.625f, NeedHand.OneHand, attackDamage, attackRange, 2f, 38f, 0.7f, 17, 3f, 0, 1, quality));
                }
                break;
            case Items.Revolver:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 110;
                            attackRange = 25f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 95;
                            attackRange = 22.5f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 65;
                            attackRange = 17.5f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 50;
                            attackRange = 15f;
                            break;
                        default:
                            attackDamage = 80;
                            attackRange = 20f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 1.1f, NeedHand.OneHand, attackDamage, attackRange, 2f, 27f, 1f, 7, 3f, 0, 0, quality));
                }
                break;
            case Items.ShotGun:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 60;
                            attackRange = 25.2f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 50;
                            attackRange = 22.7f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 30;
                            attackRange = 17.7f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 20;
                            attackRange = 15.2f;
                            break;
                        default:
                            attackDamage = 40;
                            attackRange = 20.2f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.4f, NeedHand.TwoHand, attackDamage, attackRange, 2f, 40f, 1.8f, 4, 1f, 2, 4, quality));
                }
                break;
            case Items.SubMachineGun:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 60;
                            attackRange = 31f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 50;
                            attackRange = 28f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 30;
                            attackRange = 22f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 20;
                            attackRange = 19f;
                            break;
                        default:
                            attackDamage = 40;
                            attackRange = 25f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.0f, NeedHand.TwoHand, attackDamage, attackRange, 2f, 40f, 0.075f, 30, 3f, 2, 3, quality));
                }
                break;
            case Items.AssaultRifle:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 150;
                            attackRange = 60f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 130;
                            attackRange = 55f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 90;
                            attackRange = 45f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 70;
                            attackRange = 40f;
                            break;
                        default:
                            attackDamage = 110;
                            attackRange = 50f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.8f, NeedHand.TwoHand, attackDamage, attackRange, 2f, 71f, 0.1f, 30, 3f, 2, 2, quality));
                }
                break;
            case Items.SniperRifle:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 300;
                            attackRange = 130f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 250;
                            attackRange = 110f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 150;
                            attackRange = 70f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 100;
                            attackRange = 50f;
                            break;
                        default:
                            attackDamage = 200;
                            attackRange = 90f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.7f, NeedHand.TwoHand, attackDamage, attackRange, 3f, 86f, 2.0f, 5, 4f, 2, 5, quality));
                }
                break;
            case Items.Bazooka:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 600;
                            attackRange = 50f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 500;
                            attackRange = 45f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 300;
                            attackRange = 35f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 200;
                            attackRange = 30f;
                            break;
                        default:
                            attackDamage = 400;
                            attackRange = 40f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 7.9f, NeedHand.TwoHand, attackDamage, attackRange, 3f, 20f, 3f, 1, 5f, 2, 6, quality));
                }
                break;
            case Items.LASER:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 150;
                            attackRange = 55f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 125;
                            attackRange = 50f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 90;
                            attackRange = 40f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 80;
                            attackRange = 35f;
                            break;
                        default:
                            attackDamage = 100;
                            attackRange = 45f;
                            break;
                    }
                    RangedWeapon laser = new(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.6f, NeedHand.OneHand, attackDamage, attackRange, 3f, 10f, 0.5f, 100, 3f, 0, 7, quality);
                    laser.Reload(100);
                    itemDictionary[wantItem].Add(laser);
                }
                break;
            case Items.Bow:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 25;
                            attackRange = 36f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 22.5f;
                            attackRange = 33f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 17.5f;
                            attackRange = 27f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 15;
                            attackRange = 24f;
                            break;
                        default:
                            attackDamage = 20;
                            attackRange = 30f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 1f, NeedHand.TwoHand, attackDamage, attackRange, 3f, 12f, 3f, 1, 1f, 3, 8, quality, false));
                }
                break;
            case Items.AdvancedBow:
                for (int i = start; i < end; i++)
                {
                    float attackDamage;
                    float attackRange;
                    switch (quality)
                    {
                        case CraftingQuality.Masterpiece:
                            attackDamage = 75;
                            attackRange = 80f;
                            break;
                        case CraftingQuality.Excellent:
                            attackDamage = 67.5f;
                            attackRange = 70f;
                            break;
                        case CraftingQuality.Common:
                            attackDamage = 52.5f;
                            attackRange = 50f;
                            break;
                        case CraftingQuality.Poor:
                            attackDamage = 45;
                            attackRange = 40f;
                            break;
                        default:
                            attackDamage = 60;
                            attackRange = 60f;
                            break;
                    }
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, NeedHand.TwoHand, attackDamage, attackRange, 3f, 18f, 2f, 1, 1f, 3, 9, quality, false));
                }
                break;
            // Bullets
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.008f, quality, 7));
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.006f, quality, 17));
                break;
            case Items.Bullet_AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.016f, quality, 30));
                break;
            case Items.Bullet_SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.006f, quality, 30));
                break;
            case Items.Bullet_ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.032f, quality, 4));
                break;
            case Items.Bullet_SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.012f, quality, 5));
                break;
            case Items.Rocket_Bazooka:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 5f, quality, 1));
                break;
            case Items.Arrow:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.8f, quality, 5));
                break;
            case Items.Arrow_Enchanted:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.8f, quality));
                break;
            // Helmets
            case Items.LowLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 30,
                        CraftingQuality.Excellent => 25,
                        CraftingQuality.Common => 15,
                        CraftingQuality.Poor => 10,
                        _ => 20,
                    };
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.7f, defense, defense * 10, quality));
                }
                break;
            case Items.MiddleLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 50,
                        CraftingQuality.Excellent => 45,
                        CraftingQuality.Common => 35,
                        CraftingQuality.Poor => 30,
                        _ => 40,
                    };
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()), 1.2f, defense, defense * 10, quality));
                }
                break;
            case Items.HighLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 100,
                        CraftingQuality.Excellent => 80,
                        CraftingQuality.Common => 55,
                        CraftingQuality.Poor => 50,
                        _ => 60,
                    };
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()), 1.6f, defense, defense * 10, quality));
                }
                break;
            case Items.LegendaryBulletproofHelmet:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 300,
                        CraftingQuality.Excellent => 250,
                        CraftingQuality.Common => 150,
                        CraftingQuality.Poor => 100,
                        _ => 200,
                    };
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()), 7f, defense, defense * 10, quality));
                }
                break;
            // Vests
            case Items.LowLevelBulletproofVest:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 20,
                        CraftingQuality.Excellent => 15,
                        CraftingQuality.Common => 7.5f,
                        CraftingQuality.Poor => 5,
                        _ => 10,
                    };
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, defense, defense * 10, quality));
                }
                break;
            case Items.MiddleLevelBulletproofVest:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 40,
                        CraftingQuality.Excellent => 35,
                        CraftingQuality.Common => 25,
                        CraftingQuality.Poor => 20,
                        _ => 30,
                    };
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()), 7f, defense, defense * 10, quality));
                }
                break;
            case Items.HighLevelBulletproofVest:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 70,
                        CraftingQuality.Excellent => 60,
                        CraftingQuality.Common => 45,
                        CraftingQuality.Poor => 40,
                        _ => 50,
                    };
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()), 10f, defense, defense * 10, quality));
                }
                break;
            case Items.LegendaryBulletproofVest:
                for (int i = start; i < end; i++)
                {
                    float defense = quality switch
                    {
                        CraftingQuality.Masterpiece => 150,
                        CraftingQuality.Excellent => 125,
                        CraftingQuality.Common => 85,
                        CraftingQuality.Poor => 70,
                        _ => 100,
                    };
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()), 36f, defense, defense * 100, quality));
                }
                break;
            // Consumables
            case Items.BandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.027f, quality));
                break;
            case Items.HemostaticBandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.127f, quality));
                break;
            case Items.Poison:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.167f, quality));
                break;
            case Items.Antidote:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.1f, quality));
                break;
            case Items.Potion:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.7f, quality));
                break;
            case Items.AdvancedPotion:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 1.3f, quality));
                break;
            // Crafting Materials
            case Items.Components:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 1f, quality));
                break;
            case Items.AdvancedComponent:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.1f, quality));
                break;
            case Items.Chemicals:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.1f, quality));
                break;
            case Items.Gunpowder:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.066f, quality));
                break;
            case Items.Salvages:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.1f, quality));
                break;
            // Traps
            case Items.BearTrap:
                for (int i = start; i < end; i++)
                {
                    float damage = quality switch
                    {
                        CraftingQuality.Masterpiece => 60,
                        CraftingQuality.Excellent => 50,
                        CraftingQuality.Common => 30,
                        CraftingQuality.Poor => 20,
                        _ => 40,
                    };
                    itemDictionary[wantItem].Add(new Buriable(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, damage, quality));
                }
                break;
            case Items.BearTrap_Enchanted:
                for (int i = start; i < end; i++)
                {
                    Buriable buriable = new(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, 40, quality);
                    buriable.Enchant();
                    itemDictionary[wantItem].Add(buriable);
                }
                break;
            case Items.LandMine:
                for (int i = start; i < end; i++)
                {
                    float damage = quality switch
                    {
                        CraftingQuality.Masterpiece => 140,
                        CraftingQuality.Excellent => 120,
                        CraftingQuality.Common => 80,
                        CraftingQuality.Poor => 60,
                        _ => 100,
                    };
                    itemDictionary[wantItem].Add(new Buriable(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, damage, quality));
                }
                break;
            case Items.NoiseTrap:
                for (int i = start; i < end; i++)
                {
                    float noiseVolume = quality switch
                    {
                        CraftingQuality.Masterpiece => 3600,
                        CraftingQuality.Excellent => 2500,
                        CraftingQuality.Common => 900,
                        CraftingQuality.Poor => 400,
                        _ => 1600,
                    };
                    itemDictionary[wantItem].Add(new NoiseTrap(wantItem, new LocalizedString("Item", wantItem.ToString()), 2f, noiseVolume, quality));
                }
                break;
            case Items.ChemicalTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ChemicalTrap(wantItem, new LocalizedString("Item", wantItem.ToString()), 3f, quality));
                break;
            case Items.ShrapnelTrap:
                for (int i = start; i < end; i++)
                    {
                        float damage = quality switch
                        {
                            CraftingQuality.Masterpiece => 60,
                            CraftingQuality.Excellent => 50,
                            CraftingQuality.Common => 30,
                            CraftingQuality.Poor => 20,
                            _ => 40,
                        };
                        itemDictionary[wantItem].Add(new ShrapnelTrap(wantItem, new LocalizedString("Item", wantItem.ToString()), 7f, damage, quality));
                    }
                break;
            case Items.ExplosiveTrap:
                for (int i = start; i < end; i++)
                {
                    float damage = quality switch
                    {
                        CraftingQuality.Masterpiece => 140,
                        CraftingQuality.Excellent => 120,
                        CraftingQuality.Common => 80,
                        CraftingQuality.Poor => 60,
                        _ => 100,
                    };
                    float explosionRange = quality switch
                    {
                        CraftingQuality.Masterpiece => 3f,
                        CraftingQuality.Excellent => 2.5f,
                        CraftingQuality.Common => 1.75f,
                        CraftingQuality.Poor => 1.5f,
                        _ => 2f,
                    };
                    itemDictionary[wantItem].Add(new ExplosiveTrap(wantItem, new LocalizedString("Item", wantItem.ToString()), 5f, damage, explosionRange, quality));
                }
                break;
            // ETC
            case Items.WalkingAid:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 0.3f, quality));
                break;
            case Items.TrapDetectionDevice:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 2.3f, quality));
                break;
            case Items.BiometricRader:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 2.4f, quality));
                break;
            case Items.EnergyBarrier:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()), 3.4f, quality));
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }

    public static bool CheckUseQuality(Items itemType)
    {
        return itemType switch
        {
            Items.Pistol or Items.Revolver or Items.ShotGun or Items.SubMachineGun or Items.AssaultRifle or Items.SniperRifle or Items.Bazooka 
            or Items.LASER or Items.Bow or Items.AdvancedBow or Items.LowLevelBulletproofHelmet or Items.MiddleLevelBulletproofHelmet 
            or Items.HighLevelBulletproofHelmet or Items.LegendaryBulletproofHelmet or Items.LowLevelBulletproofVest 
            or Items.MiddleLevelBulletproofVest or Items.HighLevelBulletproofVest or Items.LegendaryBulletproofVest
            or Items.Potion or Items.AdvancedPotion or Items.BearTrap or Items.LandMine or Items.NoiseTrap or Items.ShrapnelTrap 
            or Items.ExplosiveTrap or Items.TrapDetectionDevice or Items.BiometricRader or Items.EnergyBarrier => true,
            _ => false
        };
    }
}

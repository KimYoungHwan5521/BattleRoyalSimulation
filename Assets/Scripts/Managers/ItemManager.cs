using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ItemManager
{
    public enum Items
    {
        NotValid,
        // Melee Weapons
        Knife,
        Dagger,
        Bat,
        LongSword,
        Shovel,
        // Ranged Weapons
        Revolver,
        Pistol,
        AssaultRifle,
        SubMachineGun,
        ShotGun,
        SniperRifle,
        Bazooka,
        LASER,
        // Enchanted Weapons
        Knife_Enchanted,
        Dagger_Enchanted,
        LongSword_Enchanted,
        // Bullets
        Bullet_Revolver,
        Bullet_Pistol,
        Bullet_AssaultRifle,
        Bullet_SubMachineGun,
        Bullet_ShotGun,
        Bullet_SniperRifle,
        Rocket_Bazooka,
        // BulletproofHats
        LowLevelBulletproofHelmet,
        MiddleLevelBulletproofHelmet,
        HighLevelBulletproofHelmet,
        LegendaryBulletproofHelmet,
        // BulletproofVests
        LowLevelBulletproofVest,
        MiddleLevelBulletproofVest,
        HighLevelBulletproofVest,
        LegendaryBulletproofVest,
        // Consumables
        BandageRoll,
        HemostaticBandageRoll,
        Poison,
        Antidote,
        Potion,
        AdvancedPotion,
        // Crafting Materials
        Components,
        AdvancedComponent,
        Chemicals,
        Gunpowder,
        Salvages,
        // Traps
        BearTrap,
        BearTrap_Enchanted,
        LandMine,
        NoiseTrap,
        ChemicalTrap,
        ShrapnelTrap,
        ExplosiveTrap,
        // ETC
        WalkingAid,
        TrapDetectionDevice,
        BiometricRader,
        EnergyBarrier,
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
        craftables.Add(new Craftable(Items.Poison, 10, 0, 0, 2, 3, 0, 3, 1, 3.5f));
        craftables.Add(new Craftable(Items.Revolver, 15, 0, 2, 0, 4, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.Antidote, 20, 0, 0, 2, 0, 0, 2, 1, 3.5f));
        craftables.Add(new Craftable(Items.Pistol, 25, 0, 2, 0, 4, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.ShotGun, 35, 0, 4, 0, 4, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.HemostaticBandageRoll, 30, 0, 0, 2, 0, 0, 1, 1, 3.5f, new KeyValuePair<Items, int>(Items.BandageRoll, 1)));
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
        craftables.Add(new Craftable(Items.BiometricRader, 96, 2, 8, 0, 2, 0, 1, 0, 14f));
        craftables.Add(new Craftable(Items.TrapDetectionDevice, 100, 3, 5, 0, 2, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.AdvancedPotion, 104, 0, 0, 3, 0, 0, 1, 1, 3.5f, new KeyValuePair<Items, int>(Items.Potion, 1)));
        craftables.Add(new Craftable(Items.LegendaryBulletproofHelmet, 108, 0, 6, 0, 0, 0, 1, 0, 35f, new KeyValuePair<Items, int>(Items.HighLevelBulletproofHelmet, 1), new KeyValuePair<Items, int>(Items.MiddleLevelBulletproofHelmet, 2), new KeyValuePair<Items, int>(Items.LowLevelBulletproofHelmet, 4)));
        craftables.Add(new Craftable(Items.AdvancedComponent, 110, 0, 4, 0, 0, 0, 1, 0, 7f));
        craftables.Add(new Craftable(Items.LegendaryBulletproofVest, 112, 0, 6, 0, 0, 0, 1, 0, 35f, new KeyValuePair<Items, int>(Items.HighLevelBulletproofVest, 1), new KeyValuePair<Items, int>(Items.MiddleLevelBulletproofVest, 2), new KeyValuePair<Items, int>(Items.LowLevelBulletproofVest, 4)));
        craftables.Add(new Craftable(Items.EnergyBarrier, 116, 4, 8, 0, 4, 0, 1, 0, 21f));
        craftables.Add(new Craftable(Items.LASER, 120, 4, 8, 2, 8, 0, 1, 0, 21f));
        yield return null;
    }

    public static void AddItems(Items wantItem, int count)
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
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.5f, NeedHand.OneHand, DamageType.Slash, 30, 1.7f, 0));
                break;
            case Items.Dagger:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1f, NeedHand.OneHand, DamageType.Slash, 40, 2f, 1));
                break;
            case Items.Bat:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1f, NeedHand.OneOrTwoHand, DamageType.Strike, 35, 2f, 1));
                break;
            case Items.LongSword:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 2f, NeedHand.OneOrTwoHand, DamageType.Slash, 50, 2.4f, 1));
                break;
            case Items.Shovel:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 2f, NeedHand.OneOrTwoHand, DamageType.Strike, 45, 2f, 1));
                break;
            // Ranged Weapons
            case Items.Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.625f, NeedHand.OneHand, 40, 20.1f, 2f, 38f, 0.7f, 17, 3f, 0, 1));
                break;
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1.1f, NeedHand.OneHand, 80, 20f, 2f, 27f, 1f, 7, 3f, 0, 0));
                break;
            case Items.ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.4f, NeedHand.TwoHand, 40, 20.2f, 2f, 40f, 1.8f, 4, 1f, 2, 4));
                break;
            case Items.SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.0f, NeedHand.TwoHand, 40, 25f, 2f, 40f, 0.075f, 30, 3f, 2, 3));
                break;
            case Items.AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.8f, NeedHand.TwoHand, 110, 50f, 2f, 71f, 0.1f, 30, 3f, 2, 2));
                break;
            case Items.SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.7f, NeedHand.TwoHand, 200, 90f, 3f, 86f, 2.0f, 5, 4f, 2, 5));
                break;
            case Items.Bazooka:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 7.9f, NeedHand.TwoHand, 200, 40f, 3f, 10f, 10f, 1, 10f, 2, 6));
                break;
            case Items.LASER:
                for (int i = start; i < end; i++)
                {
                    RangedWeapon laser = new(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.6f, NeedHand.OneHand, 100, 45f, 3f, 10f, 0.5f, 100, 3f, 0, 7);
                    laser.Reload(100);
                    itemDictionary[wantItem].Add(laser);
                }
                break;
            // Bullets
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.008f, 7));
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.006f, 17));
                break;
            case Items.Bullet_AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.016f, 30));
                break;
            case Items.Bullet_SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.006f, 30));
                break;
            case Items.Bullet_ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.032f, 4));
                break;
            case Items.Bullet_SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.012f, 5));
                break;
            case Items.Rocket_Bazooka:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 5f, 1));
                break;
            // Helmets
            case Items.LowLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.7f, 20));
                break;
            case Items.MiddleLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1.2f, 40));
                break;
            case Items.HighLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1.6f, 60));
                break;
            case Items.LegendaryBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 7f, 200));
                break;
            // Vests
            case Items.LowLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3f, 10));
                break;
            case Items.MiddleLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 7f, 30));
                break;
            case Items.HighLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 10f, 50));
                break;
            case Items.LegendaryBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 36f, 70));
                break;
            // Consumables
            case Items.BandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.027f));
                break;
            case Items.HemostaticBandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.127f));
                break;
            case Items.Poison:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.167f));
                break;
            case Items.Antidote:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.1f));
                break;
            case Items.Potion:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.7f));
                break;
            case Items.AdvancedPotion:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1.3f));
                break;
            // Crafting Materials
            case Items.Components:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 1f));
                break;
            case Items.AdvancedComponent:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.1f));
                break;
            case Items.Chemicals:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.1f));
                break;
            case Items.Gunpowder:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.066f));
                break;
            case Items.Salvages:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.1f));
                break;
            // Traps
            case Items.BearTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Buriable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3f));
                break;
            case Items.BearTrap_Enchanted:
                for (int i = start; i < end; i++)
                {
                    Buriable buriable = new(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3f);
                    buriable.Enchant();
                    itemDictionary[wantItem].Add(buriable);
                }
                break;
            case Items.LandMine:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Buriable(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3f));
                break;
            case Items.NoiseTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new NoiseTrap(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 2f));
                break;
            case Items.ChemicalTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ChemicalTrap(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3f));
                break;
            case Items.ShrapnelTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ShrapnelTrap(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 7f));
                break;
            case Items.ExplosiveTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ExplosiveTrap(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 5f));
                break;
            // ETC
            case Items.WalkingAid:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 0.3f));
                break;
            case Items.TrapDetectionDevice:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 2.3f));
                break;
            case Items.BiometricRader:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 2.4f));
                break;
            case Items.EnergyBarrier:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, new LocalizedString("Item", wantItem.ToString()).GetLocalizedString(), 3.4f));
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

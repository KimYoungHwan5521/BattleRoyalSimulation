using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // BulletproofVests
        LowLevelBulletproofVest,
        MiddleLevelBulletproofVest,
        HighLevelBulletproofVest,
        // Consumables
        BandageRoll,
        HemostaticBandageRoll,
        Poison,
        Antidote,
        Potion,
        // Crafting Materials
        Components,
        AdvancedComponent,
        Chemicals,
        Gunpowder,
        Salvages,
        // Traps
        BearTrap,
        LandMine,
        NoiseTrap,
        ChemicalTrap,
        ShrapnelTrap,
        ExplosiveTrap,
        // ETC
        WalkingAid,
        TrapDetectionDevice,
        BiometricRader,
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

        public Craftable(Items itemType, int requiredKnowledge, int needAdvancedComponentCount, int needComponentsCount, int needChemicalsCount, int needSalvagesCount, int needGunpowderCount, int outputAmount, int craftingAnimNumber, params KeyValuePair<Items, int>[] etcNeedItems)
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
            foreach(var needItem in  etcNeedItems) this.etcNeedItems.Add(needItem.Key, needItem.Value);
        }
    }

    public static List<Craftable> craftables = new();

    public IEnumerator Initiate()
    {
        craftables.Add(new Craftable(Items.WalkingAid, 5, 0, 1, 0, 2, 0, 1, 0));
        craftables.Add(new Craftable(Items.Poison, 10, 0, 0, 2, 3, 0, 3, 1));
        craftables.Add(new Craftable(Items.Pistol, 15, 0, 2, 0, 4, 0, 1, 0));
        craftables.Add(new Craftable(Items.Antidote, 20, 0, 0, 2, 0, 0, 2, 1));
        craftables.Add(new Craftable(Items.HemostaticBandageRoll, 30, 0, 0, 2, 0, 0, 1, 1, new KeyValuePair<Items, int>(Items.BandageRoll, 1)));
        craftables.Add(new Craftable(Items.SubMachineGun, 35, 0, 4, 0, 4, 0, 1, 0));
        craftables.Add(new Craftable(Items.Bullet_Pistol, 40, 0, 0, 0, 1, 1, 2, 0));
        craftables.Add(new Craftable(Items.Bullet_SubMachineGun, 40, 0, 0, 0, 1, 1, 1, 0));
        craftables.Add(new Craftable(Items.Bullet_Revolver, 40, 0, 0, 0, 1, 1, 4, 0));
        craftables.Add(new Craftable(Items.Bullet_AssaultRifle, 40, 0, 0, 0, 2, 2, 1, 0));
        craftables.Add(new Craftable(Items.Bullet_SniperRifle, 40, 0, 0, 0, 1, 2, 1, 0));
        craftables.Add(new Craftable(Items.Bullet_ShotGun, 40, 0, 0, 0, 3, 1, 1, 0));
        craftables.Add(new Craftable(Items.BearTrap, 45, 0, 2, 0, 3, 0, 3, 0));
        craftables.Add(new Craftable(Items.Potion, 50, 0, 0, 6, 1, 0, 1, 1));
        craftables.Add(new Craftable(Items.NoiseTrap, 55, 1, 0, 0, 3, 0, 1, 0));
        craftables.Add(new Craftable(Items.AssaultRifle, 60, 0, 6, 0, 4, 0, 1, 0));
        craftables.Add(new Craftable(Items.Rocket_Bazooka, 75, 0, 1, 0, 0, 3, 1, 0));
        craftables.Add(new Craftable(Items.LandMine, 80, 1, 1, 0, 1, 2, 3, 0));
        craftables.Add(new Craftable(Items.ChemicalTrap, 83, 0, 1, 0, 0, 0, 1, 1, new KeyValuePair<Items, int>(Items.Poison, 3)));
        craftables.Add(new Craftable(Items.ShrapnelTrap, 86, 0, 1, 0, 6, 1, 1, 0));
        craftables.Add(new Craftable(Items.ExplosiveTrap, 90, 1, 0, 1, 0, 3, 1, 0));
        craftables.Add(new Craftable(Items.Bazooka, 95, 0, 8, 0, 4, 0, 1, 0));
        craftables.Add(new Craftable(Items.TrapDetectionDevice, 99, 2, 2, 1, 0, 0, 1, 0));
        craftables.Add(new Craftable(Items.BiometricRader, 100, 3, 2, 1, 0, 0, 1, 0));
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
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, "Knife", 0.5f, NeedHand.OneHand, DamageType.Cut, 30, 1.7f, 0));
                break;
            case Items.Dagger:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, "Dagger", 1f, NeedHand.OneHand, DamageType.Cut, 40, 2f, 1));
                break;
            case Items.Bat:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, "Bat", 1f, NeedHand.OneOrTwoHand, DamageType.Strike, 15, 2f, 1));
                break;
            case Items.LongSword:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, "LongSword", 2f, NeedHand.OneOrTwoHand, DamageType.Cut, 30, 2.4f, 1));
                break;
            case Items.Shovel:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new MeleeWeapon(wantItem, "Shovel", 2f, NeedHand.OneOrTwoHand, DamageType.Strike, 25, 2f, 1));
                break;
            // Ranged Weapons
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "Revolver", 1.1f, NeedHand.OneHand, 50, 20f, 2f, 27f, 1f, 7, 3f, 0, 0));
                break;
            case Items.Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "Pistol", 0.625f, NeedHand.OneHand, 30, 20.1f, 2f, 38f, 0.7f, 17, 3f, 0, 1));
                break;
            case Items.AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "AssaultRifle", 3.8f, NeedHand.TwoHand, 70, 50f, 2f, 71f, 0.1f, 30, 3f, 2, 2));
                break;
            case Items.SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "SubMachineGun", 3.0f, NeedHand.TwoHand, 30, 25f, 2f, 40f, 0.075f, 30, 3f, 2, 3));
                break;
            case Items.ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "ShotGun", 3.4f, NeedHand.TwoHand, 20, 20.2f, 2f, 40f, 1.8f, 4, 1f, 2, 4));
                break;
            case Items.SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "SniperRifle", 3.7f, NeedHand.TwoHand, 100, 75f, 3f, 78f, 2.0f, 5, 4f, 2, 5));
                break;
            case Items.Bazooka:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new RangedWeapon(wantItem, "Bazooka", 7.9f, NeedHand.TwoHand, 200, 37.5f, 3f, 10f, 10f, 1, 3f, 2, 6));
                break;
            // Bullets
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(Revolver)", 0.008f, 7));
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(Pistol)", 0.006f, 17));
                break;
            case Items.Bullet_AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(AssaultRifle)", 0.016f, 30));
                break;
            case Items.Bullet_SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(SubMachineGun)", 0.006f, 30));
                break;
            case Items.Bullet_ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(ShotGun)", 0.032f, 4));
                break;
            case Items.Bullet_SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Bullet(SniperRifle)", 0.012f, 5));
                break;
            case Items.Rocket_Bazooka:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Rocket(Bazooka)", 5f, 1));
                break;
            // Vests
            case Items.LowLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, "Low Level Bulletproof Vest", 3f, 15));
                break;
            case Items.MiddleLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, "Middle Level Bulletproof Vest", 7f, 25));
                break;
            case Items.HighLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofVest(wantItem, "High Level Bulletproof Vest", 10f, 40));
                break;
            // Helmets
            case Items.LowLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, "Low Level Bulletproof Helmet", 0.7f, 60));
                break;
            case Items.MiddleLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, "Middle Level Bulletproof Helmet", 1.2f, 100));
                break;
            case Items.HighLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new BulletproofHelmet(wantItem, "High Level Bulletproof Helmet", 1.6f, 140));
                break;
            // Consumables
            case Items.BandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "BandageRoll", 0.027f));
                break;
            case Items.HemostaticBandageRoll:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Hemostatic BandageRoll", 0.127f));
                break;
            case Items.Poison:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Poison", 0.167f));
                break;
            case Items.Antidote:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Antidote", 0.1f));
                break;
            case Items.Potion:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Potion", 0.7f));
                break;
            // Crafting Materials
            case Items.Components:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Components", 1f));
                break;
            case Items.AdvancedComponent:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Advanced Component", 0.1f));
                break;
            case Items.Chemicals:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Chemicals", 0.1f));
                break;
            case Items.Gunpowder:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Gunpowder", 0.066f));
                break;
            case Items.Salvages:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Consumable(wantItem, "Salvages", 0.1f));
                break;
            // Traps
            case Items.BearTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Buriable(wantItem, "Bear Trap", 3f));
                break;
            case Items.LandMine:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Buriable(wantItem, "Land Mine", 3f));
                break;
            case Items.NoiseTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new NoiseTrap(wantItem, "Noise Trap", 2f));
                break;
            case Items.ChemicalTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ChemicalTrap(wantItem, "Chemical Trap", 3f));
                break;
            case Items.ShrapnelTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ShrapnelTrap(wantItem, "Shrapnel Trap", 7f));
                break;
            case Items.ExplosiveTrap:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new ExplosiveTrap(wantItem, "Explosive Trap", 5f));
                break;
            // ETC
            case Items.WalkingAid:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Walking Aid", 0.3f));
                break;
            case Items.TrapDetectionDevice:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Trap Detection Device", 2.3f));
                break;
            case Items.BiometricRader:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem].Add(new Item(wantItem, "Biometric Rader", 2.4f));
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

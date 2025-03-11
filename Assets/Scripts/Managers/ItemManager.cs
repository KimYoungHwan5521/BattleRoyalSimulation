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
        // Bullets
        Bullet_Revolver,
        Bullet_Pistol,
        Bullet_AssaultRifle,
        Bullet_SubMachineGun,
        Bullet_ShotGun,
        Bullet_SniperRifle,
        // BulletproofHat
        LowLevelBulletproofHelmet,
        MiddleLevelBulletproofHelmet,
        HighLevelBulletproofHelmet,
        // BulletproofVest
        LowLevelBulletproofVest,
        MiddleLevelBulletproofVest,
        HighLevelBulletproofVest,
    }
    public Dictionary<Items, Item[]> itemDictionary = new();
    public IEnumerator Initiate()
    {
        yield return null;
    }

    public void AddItems(Items wantItem, int count)
    {
        int start = 0;
        int end = count;
        if(!itemDictionary.ContainsKey(wantItem))
        {
            itemDictionary.Add(wantItem, new Item[count]);
        }
        else
        {
            start += itemDictionary[wantItem].Length;
            end += itemDictionary[wantItem].Length;
        }

        switch(wantItem)
        {
            // Melee Weapons
            case Items.Knife:
                for(int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon(wantItem, "Knife", 0.5f, DamageType.Cut, 40, 1.7f, 0);
                break;
            case Items.Dagger:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon(wantItem, "Dagger", 1f, DamageType.Cut, 50, 2f, 1);
                break;
            case Items.Bat:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon(wantItem, "Bat", 1f, DamageType.Strike, 25, 2f, 1);
                break;
            case Items.LongSword:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon(wantItem, "LongSword", 2f, DamageType.Cut, 40, 2.4f, 1);
                break;
            case Items.Shovel:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon(wantItem, "Shovel", 2f, DamageType.Strike, 35, 2f, 1);
                break;
            // Ranged Weapons
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "Revolver", 1.1f, 50, 20f, 2f, 27f, 1f, 7, 3f, 0, 0);
                break;
            case Items.Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "Pistol", 0.625f, 30, 20.1f, 2f, 38f, 0.7f, 17, 3f, 0, 1);
                break;
            case Items.AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "AssaultRifle", 3.8f, 70, 50f, 2f, 71f, 0.1f, 30, 3f, 2, 2);
                break;
            case Items.SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "SubMachineGun", 3.0f, 30, 25f, 2f, 40f, 0.075f, 30, 3f, 2, 3);
                break;
            case Items.ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "ShotGun", 3.4f, 20, 20.2f, 2f, 40f, 1.8f, 4, 1f, 2, 4);
                break;
            case Items.SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon(wantItem, "SniperRifle", 3.7f, 100, 75f, 3f, 78f, 2.0f, 5, 3f, 2, 5);
                break;
            // Bullets
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(Revolver)", 0.008f, 7);
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(Pistol)", 0.006f, 17);
                break;
            case Items.Bullet_AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(AssaultRifle)", 0.016f, 30);
                break;
            case Items.Bullet_SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(SubMachineGun)", 0.006f, 30);
                break;
            case Items.Bullet_ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(ShotGun)", 0.032f, 4);
                break;
            case Items.Bullet_SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item(wantItem, "Bullet(SniperRifle)", 0.012f, 5);
                break;
            case Items.LowLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofVest(wantItem, "LowLevelBulletproofVest", 3f, 15);
                break;
            case Items.MiddleLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofVest(wantItem, "MiddleLevelBulletproofVest", 7f, 25);
                break;
            case Items.HighLevelBulletproofVest:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofVest(wantItem, "HighLevelBulletproofVest", 10f, 40);
                break;
            case Items.LowLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofHelmet(wantItem, "LowLevelBulletproofHelmet", 0.7f, 60);
                break;
            case Items.MiddleLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofHelmet(wantItem, "MiddleLevelBulletproofHelmet", 1.2f, 100);
                break;
            case Items.HighLevelBulletproofHelmet:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new BulletproofHelmet(wantItem, "HighLevelBulletproofHelmet", 1.6f, 140);
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

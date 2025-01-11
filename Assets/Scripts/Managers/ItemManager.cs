using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{
    public enum Items
    {
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
                    itemDictionary[wantItem][i] = new MeleeWeapon("Knife", 0.5f, 40, 1.7f, 0);
                break;
            case Items.Dagger:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon("Dagger", 1f, 50, 2f, 1);
                break;
            case Items.Bat:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon("Bat", 1f, 25, 2f, 1);
                break;
            case Items.LongSword:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon("LongSword", 2f, 40, 2.4f, 1);
                break;
            case Items.Shovel:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new MeleeWeapon("Shovel", 2f, 35, 2f, 1);
                break;
            // Ranged Weapons
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("Revolver", 1.1f, 50, 20f, 1f, 10f, 1f, 7, 3f, 0, 0);
                break;
            case Items.Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("Pistol", 0.625f, 30, 20.1f, 1f, 12f, 0.8f, 17, 3f, 0, 1);
                break;
            case Items.AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("AssaultRifle", 3.8f, 70, 50f, 2f, 20f, 0.1f, 30, 3f, 1, 2);
                break;
            case Items.SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("SubMachineGun", 3.0f, 30, 25f, 2f, 12f, 0.075f, 30, 3f, 1, 3);
                break;
            case Items.ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("ShotGun", 3.4f, 10, 20.2f, 2f, 9f, 1.1f, 4, 1f, 1, 4);
                break;
            case Items.SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("SniperRifle", 3.7f, 100, 75f, 2f, 30f, 2.0f, 5, 3f, 1, 5);
                break;
            // Bullets
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Revolver)", 0.008f, 7);
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Pistol)", 0.006f, 17);
                break;
            case Items.Bullet_AssaultRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(AssaultRifle)", 0.016f, 30);
                break;
            case Items.Bullet_SubMachineGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(SubMachineGun)", 0.006f, 30);
                break;
            case Items.Bullet_ShotGun:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Bullet_ShotGun)", 0.032f, 4);
                break;
            case Items.Bullet_SniperRifle:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Bullet_SniperRifle)", 0.012f, 5);
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

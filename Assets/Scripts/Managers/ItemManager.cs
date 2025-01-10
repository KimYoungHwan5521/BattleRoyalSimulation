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
        // Bullets
        Bullet_Revolver,
        Bullet_Pistol,
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
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("Revolver", 1f, 30, 20f, 1f, 10f, 1f, 6, 3f, 0, 0);
                break;
            case Items.Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("Pistol", 1f, 30, 21f, 1f, 12f, 0.8f, 17, 3f, 0, 1);
                break;
            case Items.Bullet_Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Revolver)", 0.008f, 30);
                break;
            case Items.Bullet_Pistol:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new Item("Bullet(Pistol)", 0.008f, 34);
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

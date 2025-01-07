using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{
    public enum Items
    {
        // Melee Weapons
        Knife,
        // Ranged Weapons
        Revolver,
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
            case Items.Revolver:
                for (int i = start; i < end; i++)
                    itemDictionary[wantItem][i] = new RangedWeapon("Revolver", 1f, 30, 20f, 1f, 10f, 1f, 6, 3f, 0);
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

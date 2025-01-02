using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{
    public enum Items
    {
        Knife,
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
                    itemDictionary[wantItem][i] = new MeleeWeapon("Knife", 2f, 40, 1.7f);
                break;
            default:
                Debug.LogAssertion($"Unknown item key : {wantItem}");
                break;
        }
    }
}

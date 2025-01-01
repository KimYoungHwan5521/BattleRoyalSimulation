using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyalManager
{
    FarmingSection[] farmingSections;
    List<Item> farmingItems = new();

    public IEnumerator Initiate()
    {
        farmingItems.Add(ItemManager.knife);
        int farmingSectionIndex;
        int boxIndex;
        for(int i=0; i<farmingItems.Count; i++)
        {
            farmingSections = GameObject.FindObjectsOfType<FarmingSection>();
            farmingSectionIndex = Random.Range(0, farmingSections.Length);
            boxIndex = Random.Range(0, farmingSections[farmingSectionIndex].boxes.Count);
            farmingSections[farmingSectionIndex].boxes[boxIndex].Items.Add(farmingItems[i]);

            GameManager.ClaimLoadInfo("Load items", i, farmingItems.Count);
        }
        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyalManager
{
    FarmingSection[] farmingSections;
    List<Item> farmingItems = new();

    public IEnumerator Initiate()
    {
        // AddItems(ItemManager.Items.Knife, 2);
        //AddItems(ItemManager.Items.Revolver, 1);
        //AddItems(ItemManager.Items.Bullet_Revolver, 1);
        //AddItems(ItemManager.Items.Pistol, 1);
        //AddItems(ItemManager.Items.Bullet_Pistol, 1);
        //AddItems(ItemManager.Items.AssaultRifle, 1);
        //AddItems(ItemManager.Items.Bullet_AssaultRifle, 4);
        //AddItems(ItemManager.Items.SubMachineGun, 1);
        //AddItems(ItemManager.Items.Bullet_SubMachineGun, 4);
        AddItems(ItemManager.Items.ShotGun, 1);
        AddItems(ItemManager.Items.Bullet_ShotGun, 4);
        AddItems(ItemManager.Items.SniperRifle, 1);
        AddItems(ItemManager.Items.Bullet_SniperRifle, 4);
        int boxIndex;
        int curruntIndex = 0;
        int remainder;

        farmingSections = GameObject.FindObjectsOfType<FarmingSection>();
        for(int i=0; i< farmingSections.Length; i++)
        {
            remainder = i < farmingItems.Count % farmingSections.Length ? 1 : 0;
            for(int j=0; j < farmingItems.Count / farmingSections.Length + remainder; j++)
            {
                boxIndex = Random.Range(0, farmingSections[i].boxes.Length);
                farmingSections[i].boxes[boxIndex].Items.Add(farmingItems[curruntIndex]);
                curruntIndex++;
                GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
            }
        }
        yield return null;
    }

    void AddItems(ItemManager.Items wantItem, int count = 1)
    {
        GameManager.Instance.ItemManager.AddItems(wantItem, count);
        for(int i=0; i< count; i++)
        {
            farmingItems.Add(GameManager.Instance.ItemManager.itemDictionary[wantItem][i]);
        }
    }
}

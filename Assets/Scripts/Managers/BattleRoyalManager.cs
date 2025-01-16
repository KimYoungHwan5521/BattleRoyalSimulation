using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyalManager
{
    Area[] areas;
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
        AddItems(ItemManager.Items.SubMachineGun, 1);
        AddItems(ItemManager.Items.Bullet_SubMachineGun, 4);
        //AddItems(ItemManager.Items.ShotGun, 1);
        //AddItems(ItemManager.Items.Bullet_ShotGun, 4);
        AddItems(ItemManager.Items.SniperRifle, 1);
        AddItems(ItemManager.Items.Bullet_SniperRifle, 4);

        farmingItems = farmingItems.Shuffle();

        int boxIndex;
        int curruntIndex = 0;
        int areaRemainder;
        int sectionRemainder;

        areas = GameObject.FindObjectsOfType<Area>();

        for(int i=0; i< areas.Length; i++)
        {
            areaRemainder = i < farmingItems.Count % areas.Length ? 1 : 0;
            int areaItemNum = farmingItems.Count / areas.Length + areaRemainder;
            for(int j=0; j < areas[i].farmingSections.Length; j++)
            {
                sectionRemainder = j < areaItemNum % areas[i].farmingSections.Length ? 1 : 0;
                int sectionItemNum = areaItemNum / areas[i].farmingSections.Length + sectionRemainder;
                for (int k=0; k < sectionItemNum; k++)
                {
                    boxIndex = Random.Range(0, areas[i].farmingSections[j].boxes.Length);
                    areas[i].farmingSections[j].boxes[boxIndex].Items.Add(farmingItems[curruntIndex]);
                    curruntIndex++;
                    GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
                }
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

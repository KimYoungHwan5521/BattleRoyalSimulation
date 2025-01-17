using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyalManager
{
    Area[] areas;
    List<Item> farmingItems = new();

    public int survivorNumber = 10;
    List<Survivor> survivors = new();

    public IEnumerator Initiate()
    {
        areas = GameObject.FindObjectsOfType<Area>();

        ItemSetting();
        ItemPlacing();
        SpawnPlayers();
        yield return null;
    }

    void AddItems(ItemManager.Items wantItem, int count = 1)
    {
        GameManager.Instance.ItemManager.AddItems(wantItem, count);
        for (int i = 0; i < count; i++)
        {
            farmingItems.Add(GameManager.Instance.ItemManager.itemDictionary[wantItem][i]);
        }
    }

    void ItemSetting()
    {
        // AddItems(ItemManager.Items.Knife, 2);
        AddItems(ItemManager.Items.Revolver, 1);
        AddItems(ItemManager.Items.Bullet_Revolver, 4);
        AddItems(ItemManager.Items.Pistol, 1);
        AddItems(ItemManager.Items.Bullet_Pistol, 4);
        AddItems(ItemManager.Items.AssaultRifle, 1);
        AddItems(ItemManager.Items.Bullet_AssaultRifle, 4);
        AddItems(ItemManager.Items.SubMachineGun, 1);
        AddItems(ItemManager.Items.Bullet_SubMachineGun, 4);
        AddItems(ItemManager.Items.ShotGun, 1);
        AddItems(ItemManager.Items.Bullet_ShotGun, 4);
        AddItems(ItemManager.Items.SniperRifle, 1);
        AddItems(ItemManager.Items.Bullet_SniperRifle, 4);

    }

    void ItemPlacing()
    {
        farmingItems = farmingItems.Shuffle();

        int boxIndex;
        int curruntIndex = 0;
        int areaRemainder;
        int sectionRemainder;

        areas = areas.Shuffle();
        for (int i = 0; i < areas.Length; i++)
        {
            areaRemainder = i < farmingItems.Count % areas.Length ? 1 : 0;
            int areaItemNum = farmingItems.Count / areas.Length + areaRemainder;
            for (int j = 0; j < areas[i].farmingSections.Length; j++)
            {
                sectionRemainder = j < areaItemNum % areas[i].farmingSections.Length ? 1 : 0;
                int sectionItemNum = areaItemNum / areas[i].farmingSections.Length + sectionRemainder;
                for (int k = 0; k < sectionItemNum; k++)
                {
                    boxIndex = Random.Range(0, areas[i].farmingSections[j].boxes.Length);
                    areas[i].farmingSections[j].boxes[boxIndex].Items.Add(farmingItems[curruntIndex]);
                    curruntIndex++;
                    GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
                }
            }
        }
    }

    void SpawnPlayers()
    {
        areas = areas.Shuffle();

        int survivorIndex = 1;
        int survivorRemainder;
        for (int i = 0; i < areas.Length; i++)
        {
            survivorRemainder = i < survivorNumber % areas.Length ? 1 : 0;
            int areaSurvivorNum = survivorNumber / areas.Length + survivorRemainder;
            for (int j = 0; j < areaSurvivorNum; j++)
            {
                Vector2 spawnPosition = new(
                    areas[i].transform.position.x + Random.Range(-areas[i].transform.localScale.x * 0.5f, areas[i].transform.localScale.x * 0.5f),
                    areas[i].transform.position.y + Random.Range(-areas[i].transform.localScale.y * 0.5f, areas[i].transform.localScale.y * 0.5f)
                    );
                Survivor survivor = PoolManager.Spawn(ResourceEnum.Prefab.Survivor, spawnPosition).GetComponent<Survivor>();
                for (int k = 0; k < areas.Length; k++) survivor.farmingAreas.Add(areas[k], false);
                survivor.CurrentFarmingArea = areas[i];
                survivor.survivorName = $"Survivor{survivorIndex}";
                survivorIndex++;
                survivors.Add(survivor);
            }
        }
    }

    public void SetProhibitArea(int number)
    {
        int count = 0;
        foreach (Area area in areas) if (!area.IsProhibited) count++;
        if (number > count) return;
        for (int i = 0; i < number; i++)
        {
            Area candidate = areas[Random.Range(0, areas.Length)];
            if (candidate.IsProhibited)
            {
                i--;
                continue;
            }
            else
            {
                candidate.IsProhibited = true;
                foreach (Survivor survivor in survivors)
                {
                    if (survivor.CurrentFarmingArea == candidate) survivor.LeaveCurrentArea();
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BattleRoyaleManager
{
    OutGameUIManager OutGameUIManager => GameManager.Instance.OutGameUIManager;
    InGameUIManager InGameUIManager => GameManager.Instance.GetComponent<InGameUIManager>();
    Calendar Calendar_ => GameManager.Instance.GetComponent<Calendar>();
    public Animator count3Animator;

    GameObject map;
    Area[] areas;
    List<Item> farmingItems = new();

    public int survivorNumber = 4;
    public bool isBattleRoyaleStart;
    public float battleTime;
    float areaProhibitTime = 30;
    float curAreaProhibitTime;
    int prohibitAtOnce = 1;

    List<Survivor> survivors = new();
    public List<Survivor> Survivors => survivors;
    List<Survivor> aliveSurvivors = new();
    public List<Survivor> AliveSurvivors => aliveSurvivors;
    Survivor battleWinner;
    public Survivor BattleWinner => battleWinner;
    public string[] rankings;

    public static Vector3[] colorInfo = new Vector3[] 
    { 
        new(1, 1, 1), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), 
        new(1, 1, 0), new(1, 0, 1), new(0, 1, 1), new(0, 0, 0), 
        new(0.5f, 0, 0), new(0.5f, 0, 1), new(0.5f, 1, 0), new(0.5f, 1, 1),
        new(0, 0.5f, 0), new(0, 0.5f, 1), new(1, 0.5f, 0), new(1, 0.5f, 1),
        new(0, 0, 0.5f), new(0, 1, 0.5f), new(1, 0, 0.5f), new(1, 1, 0.5f),
        new(0.5f, 0.5f, 0), new(0.5f, 0.5f, 1), new(0.5f, 0, 0.5f), new(0.5f, 1, 0.5f),
        new(0, 0.5f, 0.5f), new(1, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f),
    };

    public IEnumerator Initiate()
    {
        count3Animator = GameManager.Instance.count3.GetComponent<Animator>();
        GameManager.Instance.ManagerUpdate -= BattleRoyaleManagerUpdate;
        GameManager.Instance.ManagerUpdate += BattleRoyaleManagerUpdate;

        GameManager.ClaimLoadInfo("Loading a map...");
        LoadMap();
        MapSetting();
        GameManager.ClaimLoadInfo("Loading items...");
        ItemSetting();
        ItemPlacing();
        GameManager.ClaimLoadInfo("Spawn survivors...");
        SpawnPlayers();

        BattleRoyaleStart();
        yield return null;
        GameManager.CloseLoadInfo();
    }

    void BattleRoyaleManagerUpdate()
    {
        if(isBattleRoyaleStart)
        {
            battleTime += Time.deltaTime;
            curAreaProhibitTime += Time.deltaTime;
            if(curAreaProhibitTime > areaProhibitTime)
            {
                SetProhibitArea(prohibitAtOnce);
                foreach (var survivor in aliveSurvivors) survivor.FindNewNearestFarmingTarget();
                curAreaProhibitTime = 0;
            }
        }
    }

    void LoadMap()
    {
        if (Enum.TryParse(Calendar_.LeagueReserveInfo[Calendar_.Today].map.ToString(), out ResourceEnum.NavMeshData navMeshDataEnum))
        {
            map = GameObject.Instantiate(ResourceManager.Get(Calendar_.LeagueReserveInfo[Calendar_.Today].map));
            NavMeshData navMeshData = GameManager.Instance.NavMeshSurface.navMeshData = ResourceManager.Get(navMeshDataEnum);
            NavMesh.RemoveAllNavMeshData();
            NavMesh.AddNavMeshData(navMeshData);
            areas = GameObject.FindObjectsOfType<Area>();
            foreach (Area area in areas) area.Initiate();
        }
        else
        {
            Debug.LogError("Failed parse map to NavMeshData");
        }
    }

    void MapSetting()
    {
        switch (Calendar_.LeagueReserveInfo[Calendar_.Today].league)
        {
            case League.BronzeLeague:
                survivorNumber = 4;
                prohibitAtOnce = 1;
                areaProhibitTime = 30;
                InGameUIManager.SetCameraLimit(75, 75);
                break;
            case League.SilverLeague:
                survivorNumber = 9;
                prohibitAtOnce = 2;
                areaProhibitTime = 40;
                InGameUIManager.SetCameraLimit(125, 125);
                break;
            case League.GoldLeague:
                survivorNumber = 16;
                prohibitAtOnce = 3;
                areaProhibitTime = 50;
                InGameUIManager.SetCameraLimit(175, 175);
                break;
            case League.SeasonChampionship:
            case League.WorldChampionship:
                survivorNumber = 25;
                prohibitAtOnce = 4;
                areaProhibitTime = 60;
                InGameUIManager.SetCameraLimit(225, 225);
                break;
        }
    }

    void AddItems(ItemManager.Items wantItem, int count = 1)
    {
        ItemManager.AddItems(wantItem, count);
        for (int i = 0; i < count; i++)
        {
            farmingItems.Add(ItemManager.itemDictionary[wantItem][i]);
        }
    }

    void ItemSetting()
    {
        ItemManager.itemDictionary.Clear();
        AddItems(ItemManager.Items.Bazooka, 1);
        //AddItems(ItemManager.Items.Rocket_Bazooka, 10);
        //AddItems(ItemManager.Items.AdvancedComponent, 1);
        //AddItems(ItemManager.Items.Components, 10);
        AddItems(ItemManager.Items.Salvages, 200);
        //AddItems(ItemManager.Items.Chemicals, 10);
        AddItems(ItemManager.Items.Gunpowder, 100);
        AddItems(ItemManager.Items.Knife, 1);
        AddItems(ItemManager.Items.Dagger, 1);
        AddItems(ItemManager.Items.Bat, 1);
        AddItems(ItemManager.Items.LongSword, 1);
        AddItems(ItemManager.Items.Shovel, 1);
        AddItems(ItemManager.Items.Revolver, 3);
        AddItems(ItemManager.Items.Pistol, 3);
        AddItems(ItemManager.Items.AssaultRifle, 1);
        AddItems(ItemManager.Items.SubMachineGun, 2);
        AddItems(ItemManager.Items.ShotGun, 1);
        AddItems(ItemManager.Items.SniperRifle, 1);
        //AddItems(ItemManager.Items.Bullet_Revolver, 30);
        //AddItems(ItemManager.Items.Bullet_Pistol, 30);
        //AddItems(ItemManager.Items.Bullet_AssaultRifle, 10);
        //AddItems(ItemManager.Items.Bullet_SubMachineGun, 20);
        //AddItems(ItemManager.Items.Bullet_ShotGun, 20);
        //AddItems(ItemManager.Items.Bullet_SniperRifle, 10);
        AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 4);
        AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 2);
        AddItems(ItemManager.Items.HighLevelBulletproofHelmet, 1);
        AddItems(ItemManager.Items.LowLevelBulletproofVest, 4);
        AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 2);
        AddItems(ItemManager.Items.HighLevelBulletproofVest, 1);
        AddItems(ItemManager.Items.BandageRoll, 10);

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
                    boxIndex = UnityEngine.Random.Range(0, areas[i].farmingSections[j].boxes.Length);
                    areas[i].farmingSections[j].boxes[boxIndex].items.Add(farmingItems[curruntIndex]);
                    curruntIndex++;
                    GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
                }
            }
        }
    }

    void SpawnPlayers()
    {
        rankings = new string[25];

        areas = areas.Shuffle();

        int survivorIndex = 0;
        int survivorRemainder;
        for (int i = 0; i < areas.Length; i++)
        {
            survivorRemainder = i < survivorNumber % areas.Length ? 1 : 0;
            int areaSurvivorNum = survivorNumber / areas.Length + survivorRemainder;
            for (int j = 0; j < areaSurvivorNum; j++)
            {
                Vector2 spawnPosition = new(
                    areas[i].transform.position.x + UnityEngine.Random.Range(-25 + 3, 25 - 3),
                    areas[i].transform.position.y + UnityEngine.Random.Range(-25 + 3, 25 - 3)
                    );
                Survivor survivor = PoolManager.Spawn(ResourceEnum.Prefab.Survivor, spawnPosition).GetComponent<Survivor>();
                for (int k = 0; k < areas.Length; k++) survivor.farmingAreas.Add(areas[k], false);
                survivor.CurrentFarmingArea = areas[i];
                survivor.survivorID = survivorIndex;
                //if (survivorIndex == 0) survivor.SetSurvivorInfo(OutGameUIManager.MySurvivorDataInBattleRoyale);
                //else survivor.SetSurvivorInfo(OutGameUIManager.CreateRandomSurvivorData());
                survivor.SetSurvivorInfo(OutGameUIManager.contestantsData[survivorIndex]);

                if(survivorIndex < colorInfo.Length)
                {
                    survivor.SetColor(colorInfo[survivorIndex]);
                }
                else
                {
                    survivor.SetColor(new Vector3(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1)));
                }
                survivorIndex++;
                survivors.Add(survivor);
                aliveSurvivors.Add(survivor);
            }
        }
        GameManager.Instance.GetComponent<InGameUIManager>().SetLeftSurvivors(aliveSurvivors.Count);
    }

    public void SetProhibitArea(int number)
    {
        int count = 0;
        foreach (Area area in areas) if (area.IsProhibited_Plan) area.IsProhibited = true;
        foreach (Area area in areas) if (!area.IsProhibited_Plan && !area.IsProhibited) count++;
        if (count == 1) return;
        if (number >= count) number = count;
        int esc = 0;
        for (int i = 0; i < number; i++)
        {
            esc++;
            if (esc > 100)
            {
                Debug.LogWarning("Infinite loop detected!");
                return;
            }
            Area candidate = areas[UnityEngine.Random.Range(0, areas.Length)];
            if (candidate.IsProhibited || candidate.IsProhibited_Plan)
            {
                i--;
                continue;
            }
            else
            {
                if(count == 1 || !CheckPathBlock(candidate))
                {
                    candidate.IsProhibited_Plan = true;
                    foreach (Survivor survivor in survivors)
                    {
                        if (survivor.CurrentFarmingArea == candidate) survivor.LeaveCurrentArea();
                    }
                }
                else
                {
                    i--;
                    continue;
                }
            }
        }
    }

    // true : blocked, false : not blocked
    bool CheckPathBlock(Area wantProhibit)
    {
        Dictionary<Area, bool> leftAreas = new();
        foreach(Area area in areas)
        {
            if(!area.IsProhibited && !area.IsProhibited_Plan && area != wantProhibit)
            {
                leftAreas.Add(area, false);
            }
        }

        Queue<Area> queue = new();
        List<Area> remember = new();
        queue.Enqueue(leftAreas.ElementAt(0).Key);
        remember.Add(leftAreas.ElementAt(0).Key);
        leftAreas[queue.Peek()] = true;
        int esc = 0;
        while(queue.Count > 0)
        {
            esc++;
            if (esc > 100)
            {
                Debug.LogWarning("Infinite loop detected!");
                break;
            }
            foreach(Area adjecentArea in queue.Peek().adjacentAreas)
            {
                if(!remember.Contains(adjecentArea) && !adjecentArea.IsProhibited && !adjecentArea.IsProhibited_Plan && adjecentArea != wantProhibit)
                {
                    remember.Add(adjecentArea);
                    queue.Enqueue(adjecentArea);
                    leftAreas[adjecentArea] = true;
                }
            }
            queue.Dequeue();
        }
        return remember.Count < leftAreas.Count;
    }

    public void SurvivorDead(Survivor survivor)
    {
        if(aliveSurvivors.Contains(survivor))
        {
            aliveSurvivors.Remove(survivor);
            GameManager.Instance.GetComponent<InGameUIManager>().SetLeftSurvivors(aliveSurvivors.Count);
            rankings[aliveSurvivors.Count] = survivor.survivorName;
            InGameUIManager.SetSurvivorRank(rankings[aliveSurvivors.Count], aliveSurvivors.Count);
            if (aliveSurvivors.Count == 1)
            {
                GameManager.Instance.GetComponent<GameResult>().DelayedShowGameResult();
                battleWinner = aliveSurvivors[0];
                rankings[0] = aliveSurvivors[0].survivorName;
                InGameUIManager.SetSurvivorRank(rankings[0], 0);
                isBattleRoyaleStart = false;
            }
        }
    }

    void BattleRoyaleStart()
    {
        foreach (Survivor survivor in survivors)
        {
            survivor.GetComponent<NavMeshAgent>().enabled = true;
        }
        Camera.main.transform.position = new(survivors[0].transform.position.x, survivors[0].transform.position.y, -10);
        Camera.main.orthographicSize = 10;
        InGameUIManager.SetTimeScale(0);
        InGameUIManager.ClearLog();
        InGameUIManager.SetPredictionUI();
        battleWinner = null;
        count3Animator.gameObject.SetActive(true);
        count3Animator.SetTrigger("Count");
        GameManager.CloseLoadInfo();
    }

    public void Destroy()
    {
        foreach (Survivor survivor in survivors) survivor.MyDestroy();

        foreach (Area area in areas) if (area.garbageObjects != null) foreach (GameObject garbageObject in area.garbageObjects) GameObject.Destroy(garbageObject);
        GameObject.Destroy(map);

    }
    public Area GetArea(Vector2 position)
    {
        float distance;
        float minDistance = float.MaxValue;
        Area nearest = null;
        foreach (var area in areas)
        {
            Transform areaTransform = area.transform;
            distance = Vector2.Distance(position, areaTransform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = area;
            }
        }
        return nearest;
    }
}

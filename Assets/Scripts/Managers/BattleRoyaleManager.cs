using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Localization;

public class BattleRoyaleManager
{
    OutGameUIManager OutGameUIManager => GameManager.Instance.OutGameUIManager;
    InGameUIManager InGameUIManager => GameManager.Instance.GetComponent<InGameUIManager>();
    Calendar Calendar_ => GameManager.Instance.GetComponent<Calendar>();
    public Animator count3Animator;
    public AudioSource bgsfx;

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
    public LocalizedString[] rankings;

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
        SoundManager.StopBGM();
        SoundManager.Play(ResourceEnum.SFX.forest, Vector3.zero, true, out bgsfx);
        count3Animator = GameManager.Instance.count3.GetComponent<Animator>();
        GameManager.Instance.ManagerUpdate -= BattleRoyaleManagerUpdate;
        GameManager.Instance.ManagerUpdate += BattleRoyaleManagerUpdate;

        Calendar calendar = GameManager.Instance.Calendar;
        MapSetting();
        yield return LoadMap();
        yield return ItemSetting(calendar.LeagueReserveInfo[calendar.Today].itemPool);
        yield return ItemPlacing();
        yield return SpawnPlayers();

        yield return BattleRoyaleStart();
        GameManager.CloseLoadInfo();
    }

    void BattleRoyaleManagerUpdate()
    {
        if(bgsfx != null) bgsfx.transform.position = Camera.main.transform.position;
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

    IEnumerator LoadMap()
    {
        GameManager.ClaimLoadInfo("Loading a map...");
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
            yield return null;
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
            default:
                survivorNumber = 25;
                prohibitAtOnce = 4;
                areaProhibitTime= 60;
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

    IEnumerator ItemSetting(int itemPool)
    {
        farmingItems = new();
        Calendar calendar = GameManager.Instance.Calendar;
        foreach(var item in calendar.itemPool[itemPool])
        {
            AddItems(item.Key, item.Value);
        }
        yield return null;
    }

    IEnumerator ItemPlacing()
    {
        farmingItems = farmingItems.Shuffle();

        int curruntIndex = 0;
        int areaRemainder;
        int sectionRemainder;
        int boxRemainder;

        areas = areas.Shuffle();
        for (int i = 0; i < areas.Length; i++)
        {
            areaRemainder = i < farmingItems.Count % areas.Length ? 1 : 0;
            int areaItemNum = farmingItems.Count / areas.Length + areaRemainder;
            areas[i].farmingSections = areas[i].farmingSections.Shuffle();
            for (int j = 0; j < areas[i].farmingSections.Length; j++)
            {
                sectionRemainder = j < areaItemNum % areas[i].farmingSections.Length ? 1 : 0;
                int sectionItemNum = areaItemNum / areas[i].farmingSections.Length + sectionRemainder;
                areas[i].farmingSections[j].boxes = areas[i].farmingSections[j].boxes.Shuffle();
                for (int k = 0; k < areas[i].farmingSections[j].boxes.Length; k++)
                {
                    boxRemainder = k < sectionItemNum % areas[i].farmingSections[j].boxes.Length ? 1 : 0;
                    areas[i].farmingSections[j].boxes[k].items.Add(farmingItems[curruntIndex]);
                    curruntIndex++;
                    GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
                }
            }
            yield return null;
        }
    }

    IEnumerator SpawnPlayers()
    {
        rankings = new LocalizedString[25];

        areas = areas.Shuffle();

        int survivorIndex = 0;
        int survivorRemainder; 
        for (int i = 0; i < areas.Length; i++)
        {
            survivorRemainder = i < survivorNumber % areas.Length ? 1 : 0;
            int areaSurvivorNum = survivorNumber / areas.Length + survivorRemainder;
            for (int j = 0; j < areaSurvivorNum; j++)
            {
                GameManager.ClaimLoadInfo("Spawn survivors...", survivorIndex, survivorNumber);
                Vector2 spawnPosition = new(
                    areas[i].transform.position.x + UnityEngine.Random.Range(-25 + 3, 25 - 3),
                    areas[i].transform.position.y + UnityEngine.Random.Range(-25 + 3, 25 - 3)
                    );
                Survivor survivor = PoolManager.Spawn(ResourceEnum.Prefab.Survivor, spawnPosition).GetComponent<Survivor>();
                for (int k = 0; k < areas.Length; k++) survivor.farmingAreas.Add(areas[k], false);
                survivor.CurrentFarmingArea = areas[i];
                survivor.lastCurrentArea = areas[i];
                int thereIsPlayerSurvivor = OutGameUIManager.MySurvivorDataInBattleRoyale == null ? 1 : 0;
                survivor.survivorID = survivorIndex + thereIsPlayerSurvivor;
                //if (survivorIndex == 0) survivor.SetSurvivorInfo(OutGameUIManager.MySurvivorDataInBattleRoyale);
                //else survivor.SetSurvivorInfo(OutGameUIManager.CreateRandomSurvivorData());
                survivor.SetSurvivorInfo(OutGameUIManager.contestantsData[survivorIndex]);
                if (survivorIndex == 0 && OutGameUIManager.MySurvivorDataInBattleRoyale != null) survivor.playerSurvivor = true;

                if (survivorIndex + thereIsPlayerSurvivor < colorInfo.Length)
                {
                    survivor.SetColor(colorInfo[survivorIndex + thereIsPlayerSurvivor]);
                }
                else
                {
                    survivor.SetColor(new Vector3(UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1)));
                }
                survivorIndex++;
                survivors.Add(survivor);
                aliveSurvivors.Add(survivor);
                yield return null;
            }
        }
        GameManager.Instance.GetComponent<InGameUIManager>().SetLeftSurvivors(aliveSurvivors.Count);
    }

    public void SetProhibitArea(int number)
    {
        int count = 0;
        foreach (Area area in areas)
        {
            if (area.IsProhibited_Plan) area.IsProhibited = true;
        }
        foreach (Area area in areas) if (!area.IsProhibited_Plan && !area.IsProhibited) count++;
        if (count == 1)
        {
            return;
        }
        if (number >= count) number = count;
        int esc = 0;
        for (int i = 0; i < number; i++)
        {
            esc++;
            if (esc > 1000)
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
                if(!CheckPathBlock(candidate))
                {
                    candidate.IsProhibited_Plan = true;
                    foreach (Survivor survivor in survivors)
                    {
                        survivor.RemoveProhibitArea(candidate);
                        if (count - number == 1) survivor.LastArea();
                    }
                }
                else
                {
                    i--;
                    continue;
                }
            }
        }
        foreach(Area area in areas)
        {
            area.GetDistances();
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
            if (esc > 1000)
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
                if (battleWinner.playerSurvivor && battleWinner.KillCount >= survivorNumber - 1) AchievementManager.UnlockAchievement("Ace");
                rankings[0] = aliveSurvivors[0].survivorName;
                InGameUIManager.SetSurvivorRank(rankings[0], 0);
                isBattleRoyaleStart = false;
            }
        }
    }

    IEnumerator BattleRoyaleStart()
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
        yield return null;
        GameManager.CloseLoadInfo();
    }

    public void Destroy()
    {
        GameManager.Instance.SoundManager.Enqueue(bgsfx);
        bgsfx = null;
        foreach (Survivor survivor in survivors) survivor.MyDestroy();

        foreach (Area area in areas) if (area.garbageObjects != null) foreach (GameObject garbageObject in area.garbageObjects) GameObject.Destroy(garbageObject);
        GameObject.Destroy(map);
        GameManager.Instance.ManagerUpdate -= BattleRoyaleManagerUpdate;
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

    public Area GetArea(Vector2 position, Area lastArea)
    {
        float distance;
        float minDistance = Vector2.Distance(position, lastArea.transform.position) - 0.01f;
        Area nearest = lastArea;
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

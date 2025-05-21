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
        SoundManager.StopBGM();
        SoundManager.Play(ResourceEnum.SFX.forest, Vector3.zero, true, out bgsfx);
        bgsfx.maxDistance = float.MaxValue;
        bgsfx.minDistance = float.MaxValue;
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
        switch(itemPool)
        {
            case 4:
                // World championship
                AddItems(ItemManager.Items.AdvancedComponent, 36);
                AddItems(ItemManager.Items.Components, 100);
                AddItems(ItemManager.Items.Salvages, 200);
                AddItems(ItemManager.Items.Chemicals, 50);
                AddItems(ItemManager.Items.Gunpowder, 100);
                AddItems(ItemManager.Items.Knife, 10);
                AddItems(ItemManager.Items.Dagger, 10);
                AddItems(ItemManager.Items.Bat, 10);
                AddItems(ItemManager.Items.LongSword, 10);
                AddItems(ItemManager.Items.Shovel, 10);
                AddItems(ItemManager.Items.Revolver, 25);
                AddItems(ItemManager.Items.Pistol, 25);
                AddItems(ItemManager.Items.SubMachineGun, 15);
                AddItems(ItemManager.Items.ShotGun, 15);
                AddItems(ItemManager.Items.SniperRifle, 5);
                AddItems(ItemManager.Items.AssaultRifle, 5);
                AddItems(ItemManager.Items.Bazooka, 5);
                AddItems(ItemManager.Items.Bullet_Revolver, 25);
                AddItems(ItemManager.Items.Bullet_Pistol, 25);
                AddItems(ItemManager.Items.Bullet_SubMachineGun, 25);
                AddItems(ItemManager.Items.Bullet_ShotGun, 25);
                AddItems(ItemManager.Items.Bullet_AssaultRifle, 12);
                AddItems(ItemManager.Items.Bullet_SniperRifle, 12);
                AddItems(ItemManager.Items.Rocket_Bazooka, 25);
                AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 25);
                AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 15);
                AddItems(ItemManager.Items.HighLevelBulletproofHelmet, 5);
                AddItems(ItemManager.Items.LowLevelBulletproofVest, 25);
                AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 15);
                AddItems(ItemManager.Items.HighLevelBulletproofVest, 5);
                AddItems(ItemManager.Items.BandageRoll, 100);
                AddItems(ItemManager.Items.BearTrap, 50);
                AddItems(ItemManager.Items.LandMine, 25);
                AddItems(ItemManager.Items.NoiseTrap, 12);
                AddItems(ItemManager.Items.Chemicals, 25);
                AddItems(ItemManager.Items.ShrapnelTrap, 12);
                AddItems(ItemManager.Items.ExplosiveTrap, 12);
                break;
            case 3:
                // Season championship
                AddItems(ItemManager.Items.AdvancedComponent, 25);
                AddItems(ItemManager.Items.Components, 80);
                AddItems(ItemManager.Items.Salvages, 160);
                AddItems(ItemManager.Items.Chemicals, 40);
                AddItems(ItemManager.Items.Gunpowder, 80);
                AddItems(ItemManager.Items.Knife, 10);
                AddItems(ItemManager.Items.Dagger, 10);
                AddItems(ItemManager.Items.Bat, 10);
                AddItems(ItemManager.Items.LongSword, 10);
                AddItems(ItemManager.Items.Shovel, 10);
                AddItems(ItemManager.Items.Revolver, 25);
                AddItems(ItemManager.Items.Pistol, 25);
                AddItems(ItemManager.Items.SubMachineGun, 12);
                AddItems(ItemManager.Items.ShotGun, 12);
                AddItems(ItemManager.Items.SniperRifle, 6);
                AddItems(ItemManager.Items.AssaultRifle, 6);
                AddItems(ItemManager.Items.Bazooka, 6);
                AddItems(ItemManager.Items.Bullet_Revolver, 50);
                AddItems(ItemManager.Items.Bullet_Pistol, 50);
                AddItems(ItemManager.Items.Bullet_SubMachineGun, 50);
                AddItems(ItemManager.Items.Bullet_ShotGun, 50);
                AddItems(ItemManager.Items.Bullet_AssaultRifle, 25);
                AddItems(ItemManager.Items.Bullet_SniperRifle, 25);
                AddItems(ItemManager.Items.Rocket_Bazooka, 25);
                AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 25);
                AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 12);
                AddItems(ItemManager.Items.HighLevelBulletproofHelmet, 6);
                AddItems(ItemManager.Items.LowLevelBulletproofVest, 25);
                AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 12);
                AddItems(ItemManager.Items.HighLevelBulletproofVest, 6);
                AddItems(ItemManager.Items.BandageRoll, 100);
                AddItems(ItemManager.Items.BearTrap, 25);
                AddItems(ItemManager.Items.LandMine, 12);
                AddItems(ItemManager.Items.NoiseTrap, 12);
                AddItems(ItemManager.Items.Chemicals, 12);
                AddItems(ItemManager.Items.ShrapnelTrap, 6);
                AddItems(ItemManager.Items.ExplosiveTrap, 6);
                break;
            case 2:
                // Gold league
                AddItems(ItemManager.Items.AdvancedComponent, 16);
                AddItems(ItemManager.Items.Components, 60);
                AddItems(ItemManager.Items.Salvages, 120);
                AddItems(ItemManager.Items.Chemicals, 30);
                AddItems(ItemManager.Items.Gunpowder, 60);
                AddItems(ItemManager.Items.Knife, 5);
                AddItems(ItemManager.Items.Dagger, 5);
                AddItems(ItemManager.Items.Bat, 5);
                AddItems(ItemManager.Items.LongSword, 5);
                AddItems(ItemManager.Items.Shovel, 5);
                AddItems(ItemManager.Items.Revolver, 12);
                AddItems(ItemManager.Items.Pistol, 12);
                AddItems(ItemManager.Items.SubMachineGun, 8);
                AddItems(ItemManager.Items.ShotGun, 8);
                AddItems(ItemManager.Items.SniperRifle, 4);
                AddItems(ItemManager.Items.AssaultRifle, 4);
                AddItems(ItemManager.Items.Bazooka, 4);
                AddItems(ItemManager.Items.Bullet_Revolver, 60);
                AddItems(ItemManager.Items.Bullet_Pistol, 60);
                AddItems(ItemManager.Items.Bullet_SubMachineGun, 60);
                AddItems(ItemManager.Items.Bullet_ShotGun, 60);
                AddItems(ItemManager.Items.Bullet_AssaultRifle, 30);
                AddItems(ItemManager.Items.Bullet_SniperRifle, 30);
                AddItems(ItemManager.Items.Rocket_Bazooka, 30);
                AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 12);
                AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 4);
                AddItems(ItemManager.Items.HighLevelBulletproofHelmet, 1);
                AddItems(ItemManager.Items.LowLevelBulletproofVest, 12);
                AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 4);
                AddItems(ItemManager.Items.HighLevelBulletproofVest, 1);
                AddItems(ItemManager.Items.BandageRoll, 60);
                AddItems(ItemManager.Items.BearTrap, 16);
                AddItems(ItemManager.Items.LandMine, 8);
                AddItems(ItemManager.Items.NoiseTrap, 4);
                AddItems(ItemManager.Items.Chemicals, 4);
                AddItems(ItemManager.Items.ShrapnelTrap, 1);
                AddItems(ItemManager.Items.ExplosiveTrap, 1);
                break;
            case 1:
                // Silver league
                AddItems(ItemManager.Items.Components, 40);
                AddItems(ItemManager.Items.Salvages, 80);
                AddItems(ItemManager.Items.Chemicals, 20);
                AddItems(ItemManager.Items.Gunpowder, 40);
                AddItems(ItemManager.Items.Knife, 3);
                AddItems(ItemManager.Items.Dagger, 3);
                AddItems(ItemManager.Items.Bat, 3);
                AddItems(ItemManager.Items.LongSword, 3);
                AddItems(ItemManager.Items.Shovel, 3);
                AddItems(ItemManager.Items.Revolver, 8);
                AddItems(ItemManager.Items.Pistol, 8);
                AddItems(ItemManager.Items.SubMachineGun, 4);
                AddItems(ItemManager.Items.ShotGun, 4);
                AddItems(ItemManager.Items.AssaultRifle, 2);
                AddItems(ItemManager.Items.SniperRifle, 2);
                AddItems(ItemManager.Items.Bazooka, 2);
                AddItems(ItemManager.Items.Bullet_Revolver, 40);
                AddItems(ItemManager.Items.Bullet_Pistol, 40);
                AddItems(ItemManager.Items.Bullet_SubMachineGun, 40);
                AddItems(ItemManager.Items.Bullet_ShotGun, 40);
                AddItems(ItemManager.Items.Bullet_AssaultRifle, 20);
                AddItems(ItemManager.Items.Bullet_SniperRifle, 20);
                AddItems(ItemManager.Items.Rocket_Bazooka, 20);
                AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 8);
                AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 4);
                AddItems(ItemManager.Items.LowLevelBulletproofVest, 8);
                AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 4);
                AddItems(ItemManager.Items.BandageRoll, 30);
                AddItems(ItemManager.Items.BearTrap, 9);
                break;
            case 0:
            default:
                // Bronze league
                AddItems(ItemManager.Items.Components, 20);
                AddItems(ItemManager.Items.Salvages, 40);
                AddItems(ItemManager.Items.Chemicals, 10);
                AddItems(ItemManager.Items.Gunpowder, 20);
                AddItems(ItemManager.Items.Knife, 3);
                AddItems(ItemManager.Items.Bat, 3);
                AddItems(ItemManager.Items.Revolver, 4);
                AddItems(ItemManager.Items.Pistol, 4);
                AddItems(ItemManager.Items.SubMachineGun, 2);
                AddItems(ItemManager.Items.ShotGun, 2);
                AddItems(ItemManager.Items.AssaultRifle, 1);
                AddItems(ItemManager.Items.SniperRifle, 1);
                AddItems(ItemManager.Items.Bullet_Revolver, 20);
                AddItems(ItemManager.Items.Bullet_Pistol, 20);
                AddItems(ItemManager.Items.Bullet_SubMachineGun, 20);
                AddItems(ItemManager.Items.Bullet_ShotGun, 20);
                AddItems(ItemManager.Items.Bullet_AssaultRifle, 10);
                AddItems(ItemManager.Items.Bullet_SniperRifle, 10);
                AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 4);
                AddItems(ItemManager.Items.LowLevelBulletproofVest, 4);
                AddItems(ItemManager.Items.BandageRoll, 10);
                break;
        }
        yield return null;
    }

    IEnumerator ItemPlacing()
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
            yield return null;
        }
    }

    IEnumerator SpawnPlayers()
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
                GameManager.ClaimLoadInfo("Spawn survivors...", survivorIndex, survivorNumber);
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
                yield return null;
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

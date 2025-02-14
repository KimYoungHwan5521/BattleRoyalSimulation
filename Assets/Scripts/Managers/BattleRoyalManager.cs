using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BattleRoyalManager
{
    Animator count3Animator;

    Area[] areas;
    List<Item> farmingItems = new();

    public int survivorNumber = 4;
    public static bool isBattleRoyalStart;
    public static float battleTime;
    float areaProhibitTime = 60;
    float curAreaProhibitTime;

    static List<Survivor> survivors = new();
    public static List<Survivor> Survivors => survivors;
    static List<Survivor> aliveSurvivors = new();
    public static List<Survivor> AliveSurvivors => aliveSurvivors;
    static Survivor battleWinner;
    public static Survivor BattleWinner => battleWinner;

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
        areas = GameObject.FindObjectsOfType<Area>();
        GameManager.Instance.ManagerUpdate += BattleRoyalManagerUpdate;

        ResetArea();
        ItemSetting();
        ItemPlacing();
        SpawnPlayers();

        BattleRoyalStart();
        yield return null;
    }

    void BattleRoyalManagerUpdate()
    {
        if(isBattleRoyalStart)
        {
            battleTime += Time.deltaTime;
            curAreaProhibitTime += Time.deltaTime;
            if(curAreaProhibitTime > areaProhibitTime)
            {
                SetProhibitArea(1);
                curAreaProhibitTime = 0;
            }
        }
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
        GameManager.Instance.ItemManager.itemDictionary.Clear();
        farmingItems.Clear();
        AddItems(ItemManager.Items.Knife, 1);
        AddItems(ItemManager.Items.Dagger, 1);
        AddItems(ItemManager.Items.Bat, 1);
        AddItems(ItemManager.Items.LongSword, 1);
        AddItems(ItemManager.Items.Shovel, 1);
        AddItems(ItemManager.Items.Revolver, 3);
        AddItems(ItemManager.Items.Bullet_Revolver, 30);
        AddItems(ItemManager.Items.Pistol, 3);
        AddItems(ItemManager.Items.Bullet_Pistol, 30);
        AddItems(ItemManager.Items.AssaultRifle, 1);
        AddItems(ItemManager.Items.Bullet_AssaultRifle, 10);
        AddItems(ItemManager.Items.SubMachineGun, 2);
        AddItems(ItemManager.Items.Bullet_SubMachineGun, 20);
        AddItems(ItemManager.Items.ShotGun, 1);
        AddItems(ItemManager.Items.Bullet_ShotGun, 20);
        AddItems(ItemManager.Items.SniperRifle, 1);
        AddItems(ItemManager.Items.Bullet_SniperRifle, 10);
        AddItems(ItemManager.Items.LowLevelBulletproofHelmet, 4);
        AddItems(ItemManager.Items.MiddleLevelBulletproofHelmet, 2);
        AddItems(ItemManager.Items.HighLevelBulletproofHelmet, 1);
        AddItems(ItemManager.Items.LowLevelBulletproofVest, 4);
        AddItems(ItemManager.Items.MiddleLevelBulletproofVest, 2);
        AddItems(ItemManager.Items.HighLevelBulletproofVest, 1);

    }

    void ItemPlacing()
    {
        foreach(Area area in areas)
        {
            foreach(FarmingSection section in area.farmingSections)
            {
                foreach(Box box in  section.boxes)
                {
                    box.items.Clear();
                }
            }
        }

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
                    areas[i].farmingSections[j].boxes[boxIndex].items.Add(farmingItems[curruntIndex]);
                    curruntIndex++;
                    GameManager.ClaimLoadInfo("Placing items", curruntIndex, farmingItems.Count);
                }
            }
        }
    }

    void SpawnPlayers()
    {
        foreach(Survivor survivor in survivors)
        {
            GameObject.Destroy(survivor.gameObject);
        }

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
                    areas[i].transform.position.x + Random.Range(-areas[i].transform.localScale.x * 0.5f + 20, areas[i].transform.localScale.x * 0.5f - 20),
                    areas[i].transform.position.y + Random.Range(-areas[i].transform.localScale.y * 0.5f + 20, areas[i].transform.localScale.y * 0.5f - 20)
                    );
                Survivor survivor = PoolManager.Spawn(ResourceEnum.Prefab.Survivor, spawnPosition).GetComponent<Survivor>();
                for (int k = 0; k < areas.Length; k++) survivor.farmingAreas.Add(areas[k], false);
                survivor.CurrentFarmingArea = areas[i];
                survivor.survivorID = survivorIndex;
                if(survivorIndex == 0) survivor.SetSurvivorInfo(SurvivorManager.MySurvivorInfo);
                else survivor.survivorName = $"Survivor{survivorIndex}";

                if(survivorIndex < colorInfo.Length)
                {
                    survivor.SetColor(colorInfo[survivorIndex]);
                }
                else
                {
                    survivor.SetColor(new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)));
                }
                survivorIndex++;
                survivors.Add(survivor);
                aliveSurvivors.Add(survivor);
            }
        }
    }

    void ResetArea()
    {
        foreach(Area area in areas)
        {
            area.ResetProhibitArea();
        }
    }

    public void SetProhibitArea(int number)
    {
        int count = 0;
        foreach (Area area in areas) if (area.IsProhibited_Plan) area.IsProhibited = true;
        foreach (Area area in areas) if (!area.IsProhibited_Plan && !area.IsProhibited) count++;
        if (number >= count) return;
        int esc = 0;
        for (int i = 0; i < number; i++)
        {
            esc++;
            if (esc > 100)
            {
                Debug.LogWarning("Infinite loop detected!");
                return;
            }
            Area candidate = areas[Random.Range(0, areas.Length)];
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

    public static void SurvivorDead(Survivor survivor)
    {
        if(aliveSurvivors.Contains(survivor))
        {
            aliveSurvivors.Remove(survivor);
            if (survivor.survivorID == 0) GameManager.Instance.GetComponent<GameResult>().DelayedShowGameResult();
            if (aliveSurvivors.Count == 1)
            {
                GameManager.Instance.GetComponent<GameResult>().DelayedShowGameResult();
                battleWinner = survivor;
                isBattleRoyalStart = false;
            }
        }
    }

    void BattleRoyalStart()
    {
        if(count3Animator == null)
        {
            count3Animator = GameObject.FindAnyObjectByType<Count3>().GetComponent<Animator>();
            count3Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        foreach (Survivor survivor in survivors)
        {
            survivor.GetComponent<NavMeshAgent>().enabled = true;
        }
        Camera.main.transform.position = new(survivors[0].transform.position.x, survivors[0].transform.position.y, -10);
        Time.timeScale = 0;
        battleWinner = null;
        count3Animator.SetTrigger("Count");
        GameManager.CloseLoadInfo();
    }
}

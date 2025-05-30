using System.Collections;
using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public delegate void CustomStart();
public delegate void CustomUpdate();
public delegate void CustomDestroy();

public class GameManager : MonoBehaviour
{
    public static string gameVirsion = "1.0";

    public CustomStart ManagerStart;
    public CustomUpdate ManagerUpdate;

    public CustomStart ObjectStart;
    public CustomUpdate ObjectUpdate;
    public CustomDestroy ObjectDestroy;

    static GameManager instance;
    public static GameManager Instance => instance;
    ResourceManager resourceManager;
    public ResourceManager ResourceManager => resourceManager;
    SoundManager soundManager;
    public SoundManager SoundManager => soundManager;
    PoolManager poolManager;
    public PoolManager PoolManager => poolManager;
    CharacteristicManager characteristicManager;
    public CharacteristicManager CharacteristicManager => characteristicManager;
    ItemManager itemManager;
    public ItemManager ItemManager => itemManager;
    BattleRoyaleManager battleRoyaleManager;
    public BattleRoyaleManager BattleRoyaleManager => battleRoyaleManager;

    OutGameUIManager outGameUIManger;
    public OutGameUIManager OutGameUIManager => outGameUIManger;
    Calendar calendar;
    public Calendar Calendar => calendar;

    public LoadingCanvas loadingCanvas;
    public GameObject inGameUICanvas;
    public GameObject outCanvas;
    public GameObject globalCanvas;
    public GameObject optionCanvas;

    public GameObject count3;
    public GameObject description;
    public NavMeshSurface NavMeshSurface => GetComponent<NavMeshSurface>();

    bool gameReady;

    void Awake()
    {
        instance = this;
    }

    public IEnumerator Start()
    {
        resourceManager = new ResourceManager();
        yield return resourceManager.Initiate();
        soundManager = new SoundManager();
        yield return soundManager.Initiate();
        poolManager = new PoolManager();
        yield return poolManager.Initiate();
        characteristicManager = new CharacteristicManager();
        yield return characteristicManager.Initiate();
        itemManager = new ItemManager();
        yield return itemManager.Initiate();

        outGameUIManger = GetComponent<OutGameUIManager>();
        calendar = GetComponent<Calendar>();

        gameReady = true;
        CloseLoadInfo();
        SoundManager.Play(ResourceEnum.BGM.the_birth_of_hip_hop);
    }

    public IEnumerator BattleRoyaleStart()
    {
        ClaimLoadInfo("Loading battle royale");
        yield return null;
        outCanvas.SetActive(false);
        globalCanvas.SetActive(false);
        inGameUICanvas.SetActive(true);
        yield return battleRoyaleManager = new BattleRoyaleManager();
        yield return battleRoyaleManager.Initiate();
    }

    public void Test()
    {
        GetComponent<Calendar>().Today++;
        GetComponent<Calendar>().TurnPageCalendar(0);
    }

    void Update()
    {
        if (!gameReady) return;

        ManagerStart?.Invoke();
        ManagerStart = null;
        ObjectStart?.Invoke();
        ObjectStart = null;

        ManagerUpdate?.Invoke();
        ObjectUpdate?.Invoke();

        ObjectDestroy?.Invoke();
        ObjectDestroy = null;
    }

    #region Save / Load
    void SaveMySurvivorList(List<SurvivorData> mySurvivors)
    {
        var saveData = new MySurvivorListSaveData
        {
            survivorSaveDatas = mySurvivors.ConvertAll(SaveManager.ToSaveData)
        };
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("MySurvivorList", json);
        PlayerPrefs.Save();
    }

    List<SurvivorData> LoadMySurvivorList()
    {
        string json = PlayerPrefs.GetString("MySurvivorList", "{}");
        var saveData = JsonUtility.FromJson<MySurvivorListSaveData>(json);
        return saveData.survivorSaveDatas.ConvertAll(SaveManager.FromSaveData);
    }

    void SaveLeagueReserve(Dictionary<int, LeagueReserveData> data)
    {
        var saveData = SaveManager.ToSaveData(data);
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("LeagueReserveData", json);
        PlayerPrefs.Save();
    }

    Dictionary<int, LeagueReserveData> LoadLeagueReserve()
    {
        string json = PlayerPrefs.GetString("LeagueReserveData", "{}");
        var saveData = JsonUtility.FromJson<LeagueReserveDictionarySaveData>(json);
        return SaveManager.FromSaveData(saveData);
    }

    void SaveETCData()
    {
        ETCData saveData = new(
            OutGameUIManager.Money,
            OutGameUIManager.MySurvivorsId,
            OutGameUIManager.SurvivorHireLimit,
            OutGameUIManager.FightTrainingLevel,
            OutGameUIManager.ShootingTrainingLevel,
            OutGameUIManager.AgilityTrainingLevel,
            OutGameUIManager.WeightTrainingLevel,
            calendar.Today
            );
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("ETCData", json);
        PlayerPrefs.Save();
    }

    public IEnumerator LoadETCData()
    {
        GameManager.ClaimLoadInfo("Loading ETC data...", 2, 3);
        string json = PlayerPrefs.GetString("ETCData", "{}");
        var saveData = JsonUtility.FromJson<ETCData>(json);
        OutGameUIManager.LoadData(
        saveData.money,
        saveData.mySurvivorsId,
        saveData.survivorHireLimit,
        saveData.fightTrainingLevel,
        saveData.shootingTrainingLevel,
        saveData.agilityTrainingLevel,
        saveData.weightTrainingLevel
            );
        calendar.LoadToday(saveData.today);
        yield return null;
    }

    public void Save()
    {
        SaveMySurvivorList(outGameUIManger.MySurvivorsData);
        SaveLeagueReserve(calendar.LeagueReserveInfo);
        SaveETCData();
        PlayerPrefs.SetInt("HaveSaveData", 1);
    }

    public IEnumerator Load()
    {
        ClaimLoadInfo("Loading save data...");
        yield return outGameUIManger.LoadMySurvivorData(LoadMySurvivorList());
        yield return calendar.LoadLeagueReserveInfo(LoadLeagueReserve());
        yield return LoadETCData();
        outGameUIManger.ResetHireMarket();
        outGameUIManger.ResetSurvivorsDropdown();
        ClaimLoadInfo("Setting markets...", 3, 3);
        CloseLoadInfo();
    }
    #endregion

    public static void ClaimLoadInfo(string info, int numerator = 0, int denominator = 1)
    {
        if (instance && instance.loadingCanvas)
        {
            instance.loadingCanvas.SetLoadInfo(info, numerator, denominator);
            instance.loadingCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("There is no GameManager or loadingCanvas");
        }
    }

    public static void CloseLoadInfo()
    {
        if (instance && instance.loadingCanvas)
        {
            instance.loadingCanvas.CloseLoadInfo();
            instance.loadingCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("There is no GameManager or loadingCanvas");
        }
    }


    public void FixLayout(RectTransform rect)
    {
        StartCoroutine(FixLayoutNextFrame(rect));
    }

    IEnumerator FixLayoutNextFrame(RectTransform rect)
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    void OnCancel(InputValue value)
    {
        optionCanvas.SetActive(!optionCanvas.activeSelf);
        if(BattleRoyaleManager != null && BattleRoyaleManager.isBattleRoyaleStart)
        {
            GetComponent<InGameUIManager>().SetTimeScale(0);
        }
    }
}
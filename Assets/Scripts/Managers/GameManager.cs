using NavMeshPlus.Components;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public delegate void CustomStart();
public delegate void CustomUpdate();
public delegate void CustomDestroy();

public class GameManager : MonoBehaviour
{
    public static string gameVersion = "2.2";
    [SerializeField] TextMeshProUGUI[] versionTexts;

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

    TrainingManager trainingManager;
    public TrainingManager TrainingManager => trainingManager;

    AchievementUIManager achievementUIManager;
    public AchievementUIManager AchievementUIManager => achievementUIManager;

    OutGameUIManager outGameUIManger;
    public OutGameUIManager OutGameUIManager => outGameUIManger;
    Calendar calendar;
    public Calendar Calendar => calendar;
    UnlockManager unlockManager;
    public UnlockManager UnlockManager => unlockManager;
    [SerializeField] Option option;
    public Option Option => option;
    [SerializeField] Title title;
    public Title Title => title;

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
        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI 초기화 실패");
            //Application.Quit();
        }
        else
        {
            Debug.Log("SteamAPI 초기화 성공");
        }
    }

    private void OnApplicationQuit()
    {
        SteamAPI.Shutdown();
    }

    public IEnumerator Start()
    {
        if(PlayerPrefs.GetInt("ResolutionWidth") > 0)
        {
            Screen.SetResolution(PlayerPrefs.GetInt("ResolutionWidth"), PlayerPrefs.GetInt("ResolutionHeight"), (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode"));
        }

        loadingCanvas.gameObject.SetActive(true);

        resourceManager = new ResourceManager();
        yield return resourceManager.Initiate();
        soundManager = new SoundManager();
        yield return soundManager.Initiate();
        poolManager = new PoolManager();
        yield return poolManager.Initiate();
        achievementUIManager = new AchievementUIManager();
        yield return achievementUIManager.Initiate();
        characteristicManager = new CharacteristicManager();
        yield return characteristicManager.Initiate();
        trainingManager = new TrainingManager();
        yield return trainingManager.Initiate();
        itemManager = new ItemManager();
        yield return itemManager.Initiate();

        title.title.SetActive(true);
        outGameUIManger = GetComponent<OutGameUIManager>();
        calendar = GetComponent<Calendar>();
        unlockManager = GetComponent<UnlockManager>();
        yield return unlockManager.Initiate();
        inGameUICanvas.SetActive(false);
        foreach (var versionText in versionTexts) versionText.text = $"Version - {gameVersion}";

        CheckSaveData();

        Application.logMessageReceived += (log, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                outGameUIManger.DebugLog(log + "\n" + stack);
                outGameUIManger.Alert("Alert:Error");
            }
        };
#if UNITY_EDITOR
        Application.runInBackground = false;
#else
        Application.runInBackground = true;
#endif

        gameReady = true;
        CloseLoadInfo();
        SoundManager.Play(ResourceEnum.BGM.the_birth_of_hip_hop);
    }

    public void CheckSaveData()
    {
        string json = PlayerPrefs.GetString($"SaveDataInfo0", "{}");
        title.CheckSaveData(json != "{}");
    }

    public void ResetData(int difficulty)
    {
        OutGameUIManager.ResetData(difficulty);
        calendar.ResetData();
        GetComponent<GameResult>().ResetData();
        unlockManager.RelockAll();
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

    public void Test(int wantDate)
    {
        //calendar.Today = 83;
        //outGameUIManger.MySurvivorsData[0].tier = Tier.Gold;
        //outGameUIManger.MySurvivorsData[0].IncreaseStats(100, 100, 100, 100, 100, 100);
        //calendar.LeagueReserveInfo[83].reserver = outGameUIManger.MySurvivorsData[0];
        //outGameUIManger.SetContestants();
    }

    public void Test2(int wantNumber)
    {
        BattleRoyaleManager.SetProhibitArea(wantNumber);
    }
    void Update()
    {
        if (!gameReady) return;
        SteamAPI.RunCallbacks(); // 필수!

        ManagerStart?.Invoke();
        ManagerStart = null;
        ObjectStart?.Invoke();
        ObjectStart = null;

        ManagerUpdate?.Invoke();
        ObjectUpdate?.Invoke();

        ObjectDestroy?.Invoke();
        ObjectDestroy = null;
    }

    public void DestroyBattleRoyaleManager()
    {
        if (battleRoyaleManager == null) return;
        battleRoyaleManager.Destroy();
        battleRoyaleManager = null;
    }

    #region Save / Load
    void SaveSaveDataInfo(int slot)
    {
        string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int ingameDate = calendar.Today;
        var saveData = new SaveDataInfo(gameVersion, saveTime, ingameDate);
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"SaveDataInfo{slot}", json);
        PlayerPrefs.Save();

        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"SaveDataInfo{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    IEnumerator LoadSaveDataInfo(int slot)
    {
        if (SteamRemoteStorage.FileExists($"SaveDataInfo{slot}.json"))
        {
            Debug.Log("Steam Cloud 로드 성공!");
            int fileSize = SteamRemoteStorage.GetFileSize($"SaveDataInfo{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"SaveDataInfo{slot}.json", bytes, fileSize);

            string json = Encoding.UTF8.GetString(bytes);
            var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
        }
        else
        {
            string json = PlayerPrefs.GetString($"SaveDataInfo{slot}", "{}");
            var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
            string loadedDataGameVersion = saveData.gameVersion;
            if( loadedDataGameVersion != gameVersion )
            {
                ManagerStart += () => OutGameUIManager.Alert("The saved data does not match the current game version. The game may not function properly.");
            }
        }
        yield return null;
    }

    void SaveMySurvivorList(List<SurvivorData> mySurvivors, int slot)
    {
        var saveData = new MySurvivorListSaveData
        {
            survivorSaveDatas = mySurvivors.ConvertAll(SaveManager.ToSaveData)
        };
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"MySurvivorList{slot}", json);
        PlayerPrefs.Save();
        
        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"MySurvivorList{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    List<SurvivorData> LoadMySurvivorList(int slot)
    {
        MySurvivorListSaveData saveData = null;
        if (SteamRemoteStorage.FileExists($"MySurvivorList{slot}.json"))
        {
            int fileSize = SteamRemoteStorage.GetFileSize($"MySurvivorList{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"MySurvivorList{slot}.json", bytes, fileSize);

            string json = Encoding.UTF8.GetString(bytes);
            saveData = JsonUtility.FromJson<MySurvivorListSaveData>(json);
        }
        else
        {
            string json = PlayerPrefs.GetString($"MySurvivorList{slot}", "{}");
            saveData = JsonUtility.FromJson<MySurvivorListSaveData>(json);
        }
        return saveData.survivorSaveDatas.ConvertAll(SaveManager.FromSaveData);
    }

    void SaveLeagueReserve(Dictionary<int, LeagueReserveData> data, int slot)
    {
        var saveData = SaveManager.ToSaveData(data);
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"LeagueReserveData{slot}", json);
        PlayerPrefs.Save();

        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"LeagueReserveData{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    Dictionary<int, LeagueReserveData> LoadLeagueReserve(int slot)
    {
        LeagueReserveDictionarySaveData saveData = null;
        if (SteamRemoteStorage.FileExists($"LeagueReserveData{slot}.json"))
        {
            int fileSize = SteamRemoteStorage.GetFileSize($"LeagueReserveData{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"LeagueReserveData{slot}.json", bytes, fileSize);

            string json = Encoding.UTF8.GetString(bytes);
            saveData = JsonUtility.FromJson<LeagueReserveDictionarySaveData>(json);
        }
        else
        {
            string json = PlayerPrefs.GetString($"LeagueReserveData{slot}", "{}");
            saveData = JsonUtility.FromJson<LeagueReserveDictionarySaveData>(json);
        }
        return SaveManager.FromSaveData(saveData);
    }

    void SaveETCData(int slot)
    {
        ETCData saveData = new(
            OutGameUIManager.GameMode,
            OutGameUIManager.Difficulty,
            OutGameUIManager.Money,
            OutGameUIManager.MySurvivorsId,
            OutGameUIManager.trainingLevel,
            OutGameUIManager.trainingCards,
            OutGameUIManager.SurvivorHireLimit,
            OutGameUIManager.contestantsData,
            OutGameUIManager.Championship,
            OutGameUIManager.championshipHeldCount,
            OutGameUIManager.championshipDatas,
            calendar.Today,
            calendar.CurMaxYear,
            calendar.participationConfirmed,
            unlockManager.unlockStatus
            )
        {
            hireMarketSurvivorData =
            new SurvivorData[]{
                outGameUIManger.survivorsInHireMarket[0].survivorData,
                outGameUIManger.survivorsInHireMarket[1].survivorData,
                outGameUIManger.survivorsInHireMarket[2].survivorData,
            },
            soldOut = new bool[]
            {
                outGameUIManger.survivorsInHireMarket[0].SoldOut,
                outGameUIManger.survivorsInHireMarket[1].SoldOut,
                outGameUIManger.survivorsInHireMarket[2].SoldOut,
            }
        };
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString($"ETCData{slot}", json);
        PlayerPrefs.Save();

        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"ETCData{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    public IEnumerator LoadETCData(int slot)
    {
        GameManager.ClaimLoadInfo("Loading ETC data...", 2, 3);
        ETCData saveData = null;
        if (SteamRemoteStorage.FileExists($"ETCData{slot}.json"))
        {
            int fileSize = SteamRemoteStorage.GetFileSize($"ETCData{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"ETCData{slot}.json", bytes, fileSize);

            string json = Encoding.UTF8.GetString(bytes);
            saveData = JsonUtility.FromJson<ETCData>(json);
        }
        else
        {
            string json = PlayerPrefs.GetString($"ETCData{slot}", "{}");
            saveData = JsonUtility.FromJson<ETCData>(json);
        }

        OutGameUIManager.LoadData(
            saveData.gameMode,
            saveData.difficulty,
            saveData.money,
            saveData.mySurvivorsId,
            saveData.trainingLevel,
            saveData.trainings,
            saveData.survivorHireLimit,
            saveData.contestantsData,
            saveData.championship,
            saveData.championshipHeldCount,
            saveData.championshipDatas
            );
        outGameUIManger.survivorsInHireMarket[0].SetInfo(saveData.hireMarketSurvivorData[0], false);
        outGameUIManger.survivorsInHireMarket[1].SetInfo(saveData.hireMarketSurvivorData[1], false);
        outGameUIManger.survivorsInHireMarket[2].SetInfo(saveData.hireMarketSurvivorData[2], false);
        outGameUIManger.survivorsInHireMarket[0].SoldOut = saveData.soldOut[0];
        outGameUIManger.survivorsInHireMarket[1].SoldOut = saveData.soldOut[1];
        outGameUIManger.survivorsInHireMarket[2].SoldOut = saveData.soldOut[2];
        calendar.LoadToday(saveData.today, saveData.curMaxYear, saveData.participationConfirmed);
        unlockManager.LoadUnlockStatus(saveData.unlockStatus);

        AchievementManager.earnedAchievementsInThisRun = saveData.earnedAchievements;
        yield return null;
    }

    void SaveStrategy(int slot)
    {
        var wrapper = new StrategyDictionarySaveData(OutGameUIManager.MySurvivorsData[0]);

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString($"StrategyPreset{slot}", json);
        PlayerPrefs.Save();

        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"StrategyPreset{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    public void LoadStrategy(int slot)
    {
        string json = "{}";
        if (SteamRemoteStorage.FileExists($"StrategyPreset{slot}.json"))
        {
            int fileSize = SteamRemoteStorage.GetFileSize($"StrategyPreset{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"StrategyPreset{slot}.json", bytes, fileSize);

            json = Encoding.UTF8.GetString(bytes);
        }
        else
        {
            json = PlayerPrefs.GetString($"StrategyPreset{slot}", "{}");
        }
        if (json.Equals("{}")) return;

        StrategyDictionarySaveData saveData = JsonUtility.FromJson<StrategyDictionarySaveData>(json);
        
        OutGameUIManager.MySurvivorsData[0].priority1Weapon = saveData.priority1Weapon;
        OutGameUIManager.MySurvivorsData[0].priority2Weapon = saveData.priority2Weapon;
        OutGameUIManager.MySurvivorsData[0].priority1Crafting = saveData.priority1Crafting;
        OutGameUIManager.MySurvivorsData[0].priority2Crafting = saveData.priority2Crafting;
        OutGameUIManager.MySurvivorsData[0].priority1CraftingToInt = saveData.priority1CraftingToInt;
        OutGameUIManager.MySurvivorsData[0].priority2CraftingToInt = saveData.priority2CraftingToInt;
        OutGameUIManager.MySurvivorsData[0].craftingAllows = saveData.craftingAllows;
        OutGameUIManager.MySurvivorsData[0].repairCondition = saveData.repairCondition;
        OutGameUIManager.MySurvivorsData[0].strategyDictionary = saveData.CreateStrategyDictionary();
}

    public void Save(int slot)
    {
        SaveSaveDataInfo(slot);
        SaveMySurvivorList(outGameUIManger.MySurvivorsData, slot);
        SaveLeagueReserve(calendar.LeagueReserveInfo, slot);
        SaveETCData(slot);
        SaveStrategy(0);
        //Option.ReloadSavedata();
        //string message = slot == 0 ? "Alert:Game Autosaved." : "Alert:Game Saved.";
        //OutGameUIManager.Alert(message);
    }

    public IEnumerator Load(int slot)
    {
        gameReady = false;
        outCanvas.SetActive(true);
        if (BattleRoyaleManager != null) GetComponent<GameResult>().ExitBattle(true);
        ClaimLoadInfo("Loading save data...");
        yield return LoadSaveDataInfo(slot);
        yield return outGameUIManger.LoadMySurvivorData(LoadMySurvivorList(slot));
        yield return calendar.LoadLeagueReserveInfo(LoadLeagueReserve(slot));
        yield return LoadETCData(slot);
        outGameUIManger.CloseAll();
        calendar.CloseAll();
        ClaimLoadInfo("Setting markets...", 3, 3);
        outGameUIManger.ResetSurvivorsDropdown();
        ClaimLoadInfo("Version checking...", 0, 1);
        yield return VersionCompatible(slot);
        ClaimLoadInfo("Version checking...", 1, 1);
        CloseLoadInfo();
        gameReady = true;
        title.title.SetActive(false);
        option.SetSaveButtonInteractable(true, true);

        OutGameUIManager.Alert("Alert:Load Successful");
    }

    IEnumerator VersionCompatible(int slot)
    {
        string json = PlayerPrefs.GetString($"SaveDataInfo{slot}", "{}");
        var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
        string loadedDataGameVersion = saveData.gameVersion;
        int loadedDataGameVersionInt1 = int.Parse(loadedDataGameVersion.Split('.')[0]);
        int loadedDataGameVersionInt2 = int.Parse(loadedDataGameVersion.Split('.')[1]);
        
        if (loadedDataGameVersionInt2 <= 1)
        {
            calendar.ResetCalendar();
        }
        unlockManager.CheckAlreadyLocked(loadedDataGameVersionInt1 <= 1 && loadedDataGameVersionInt2 <= 3);
        yield return null;
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

    public Stack<GameObject> openedWindows = new();
    void OnCancel(InputValue value)
    {
        if(openedWindows.Count > 0)
        {
            GameObject top = openedWindows.Pop();
            if (top.activeSelf) top.SetActive(false);
            else OnCancel(value);
        }
        else
        {
            optionCanvas.SetActive(true);
            openedWindows.Push(optionCanvas);
            if(BattleRoyaleManager != null && BattleRoyaleManager.isBattleRoyaleStart)
            {
                GetComponent<InGameUIManager>().SetTimeScale(0);
            }
        }
    }
}
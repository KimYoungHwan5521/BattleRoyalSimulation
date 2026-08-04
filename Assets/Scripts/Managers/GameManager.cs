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
    public static string gameVersion = "2.3.1";
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

    OutGameUIManager outGameUIManager;
    public OutGameUIManager OutGameUIManager => outGameUIManager;
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
        outGameUIManager = GetComponent<OutGameUIManager>();
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
                outGameUIManager.DebugLog(log + "\n" + stack);
                outGameUIManager.Alert("Alert:Error");
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
        title.haveSaveData = json != "{}";
    }

    public void ResetData(GameMode wantMode, int difficulty)
    {
        OutGameUIManager.MySurvivorsData.Clear();
        OutGameUIManager.ResetData(wantMode, difficulty);
        calendar.ResetData(wantMode);
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
        GetComponent<GameResult>().ResetData();
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

        outGameUIManager.SetChampionship(false);
        outGameUIManager.championshipHeldCount = 0;
        outGameUIManager.championshipDatas.Clear();
        calendar.Today = 83;
    }

    public void Test2(int wantNumber)
    {
        //Option.DeleteSaveData(0);
        //CheckSaveData();
        OutGameUIManager.MySurvivorsData[0].injuries.Add(new(InjurySite.RightArm, InjuryType.Contusion, 0.5f));
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
        string json;
        if (SteamRemoteStorage.FileExists($"SaveDataInfo{slot}.json"))
        {
            Debug.Log("Steam Cloud 로드 성공!");
            int fileSize = SteamRemoteStorage.GetFileSize($"SaveDataInfo{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"SaveDataInfo{slot}.json", bytes, fileSize);

            json = Encoding.UTF8.GetString(bytes);
        }
        else
        {
            json = PlayerPrefs.GetString($"SaveDataInfo{slot}", "{}");
        }
        var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
        string loadedDataGameVersion = saveData.gameVersion;
        if( loadedDataGameVersion != gameVersion )
        {
            ManagerStart += () => OutGameUIManager.Alert("The saved data does not match the current game version. The game may not function properly.");
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
        ETCData saveData = new();
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

        calendar.LoadToday(saveData);
        OutGameUIManager.LoadData(saveData);
        calendar.RefreshTodayUI();
        unlockManager.LoadUnlockStatus(saveData.unlockStatus);

        AchievementManager.earnedAchievementsInThisRun = saveData.earnedAchievements;
        yield return null;
    }

    public void SaveStrategy(int slot, SurvivorData survivor, string presetName = "")
    {
        if (OutGameUIManager.MySurvivorsData != null && OutGameUIManager.MySurvivorsData.Count == 0) return;
        var wrapper = new StrategyDictionarySaveData(slot, survivor, presetName);

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString($"StrategyPreset{slot}", json);
        PlayerPrefs.Save();

        // Steam 클라우드에 업로드
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        bool success = SteamRemoteStorage.FileWrite($"StrategyPreset{slot}.json", bytes, bytes.Length);
        if (!success) Debug.LogWarning("Steam Cloud 저장 실패");
    }

    public bool DeleteStrategy(int slot)
    {
        string key = $"StrategyPreset{slot}";
        string fileName = $"{key}.json";

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        if (!SteamManager.Initialized ||
            !SteamRemoteStorage.FileExists(fileName))
        {
            return true;
        }

        return SteamRemoteStorage.FileDelete(fileName);
    }

    public StrategyDictionarySaveData LoadStrategy(int slot)
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
        if (json.Equals("{}")) return null;

        StrategyDictionarySaveData saveData = JsonUtility.FromJson<StrategyDictionarySaveData>(json);

        if (saveData == null) return null;

        // JsonUtility가 Dictionary를 불러올 수 없으므로 entries로 복원
        saveData.strategyDictionary = saveData.CreateStrategyDictionary();

        return saveData;
    }

    public void Save(int slot, bool alert = true)
    {
        SaveSaveDataInfo(slot);
        SaveMySurvivorList(outGameUIManager.MySurvivorsData, slot);
        SaveLeagueReserve(calendar.LeagueReserveInfo, slot);
        SaveETCData(slot);
        if(outGameUIManager.GameMode == GameMode.SingleCareerRun) SaveStrategy(0, OutGameUIManager.MySurvivorsData[0]);
        Option.ReloadSavedata();
        //string message = slot == 0 ? "Alert:Game Autosaved." : "Alert:Game Saved.";
        if(alert) OutGameUIManager.Alert("Alert:Game Saved.");
    }

    public IEnumerator Load(int slot)
    {
        gameReady = false;
        outCanvas.SetActive(true);
        if (BattleRoyaleManager != null) GetComponent<GameResult>().ExitBattle(true);
        ClaimLoadInfo("Loading save data...");
        yield return LoadSaveDataInfo(slot);
        yield return outGameUIManager.LoadMySurvivorData(LoadMySurvivorList(slot));
        yield return calendar.LoadLeagueReserveInfo(LoadLeagueReserve(slot));
        yield return LoadETCData(slot);
        outGameUIManager.CloseAll();
        calendar.CloseAll();
        ClaimLoadInfo("Setting markets...", 3, 3);
        outGameUIManager.ResetSurvivorsDropdown();
        ClaimLoadInfo("Version checking...", 0, 1);
        yield return VersionCompatible(slot);
        ClaimLoadInfo("Version checking...", 1, 1);
        CloseLoadInfo();
        gameReady = true;
        title.selectGameMode.SetActive(false);
        title.title.SetActive(false);

        if (slot == 0) option.SetSaveButtonInteractable(false, false, true);
        else option.SetSaveButtonInteractable(true, true, false);

        OutGameUIManager.Alert("Alert:Load Successful");
    }

    IEnumerator VersionCompatible(int slot)
    {
        string json = "{}";
        if (SteamRemoteStorage.FileExists($"SaveDataInfo{slot}.json"))
        {
            int fileSize = SteamRemoteStorage.GetFileSize($"SaveDataInfo{slot}.json");
            byte[] bytes = new byte[fileSize];
            SteamRemoteStorage.FileRead($"SaveDataInfo{slot}.json", bytes, fileSize);

            json = Encoding.UTF8.GetString(bytes);
        }
        else
        {
            json = PlayerPrefs.GetString($"SaveDataInfo{slot}", "{}");
        }
        var saveData = JsonUtility.FromJson<SaveDataInfo>(json);
        string loadedDataGameVersion = saveData.gameVersion;
        Debug.Log($"Saved Data Version : {saveData.gameVersion}");
        int loadedDataGameVersionInt1 = int.Parse(loadedDataGameVersion.Split('.')[0]);
        int loadedDataGameVersionInt2 = int.Parse(loadedDataGameVersion.Split('.')[1]);
        int currentGameVersionInt1 = int.Parse(gameVersion.Split('.')[0]);
        int currentGameVersionInt2 = int.Parse(gameVersion.Split('.')[1]);
        
        if (loadedDataGameVersionInt2 <= 1)
        {
            calendar.ResetCalendar();
        }
        //unlockManager.CheckAlreadyLocked(loadedDataGameVersionInt1 < currentGameVersionInt1 || loadedDataGameVersionInt1 == currentGameVersionInt1 && loadedDataGameVersionInt2 < currentGameVersionInt2);
        unlockManager.CheckAlreadyLocked(true);
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
        description.SetActive(false);

        if (openedWindows.Count > 0)
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
using System.Collections;
using System.Resources;
using UnityEngine;

public delegate void CustomStart();
public delegate void CustomUpdate();
public delegate void CustomDestroy();

public class GameManager : MonoBehaviour
{
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
    ItemManager itemManager;
    public ItemManager ItemManager => itemManager;
    BattleRoyalManager battleRoyalManager;
    public BattleRoyalManager BattleRoyalManager => battleRoyalManager;
    

    public LoadingCanvas loadingCanvas;

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
        itemManager = new ItemManager();
        yield return itemManager.Initiate();

        battleRoyalManager = new BattleRoyalManager();
        yield return battleRoyalManager.Initiate();

        CloseLoadInfo();
    }

    public void Test()
    {
        battleRoyalManager.SetProhibitArea(2);
    }
    public void Test2()
    {
        Time.timeScale = 4;
    }

    void Update()
    {
        ManagerStart?.Invoke();
        ManagerStart = null;
        ObjectStart?.Invoke();
        ObjectStart = null;

        ManagerUpdate?.Invoke();
        ObjectUpdate?.Invoke();

        ObjectDestroy?.Invoke();
        ObjectDestroy = null;
    }
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
            instance.loadingCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("There is no GameManager or loadingCanvas");
        }
    }
}
using System.Collections;
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
    BattleRoyaleManager battleRoyaleManager;
    public BattleRoyaleManager BattleRoyalManager => battleRoyaleManager;
    

    public LoadingCanvas loadingCanvas;
    public GameObject inGameUICanvas;
    public GameObject outCanvas;
    public GameObject globalCanvas;

    public GameObject count3;
    public GameObject description;

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
        itemManager = new ItemManager();
        yield return itemManager.Initiate();

        gameReady = true;
        CloseLoadInfo();
    }

    public IEnumerator BattleRoyaleStart()
    {
        outCanvas.SetActive(false);
        globalCanvas.SetActive(false);
        inGameUICanvas.SetActive(true);
        battleRoyaleManager = new BattleRoyaleManager();
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
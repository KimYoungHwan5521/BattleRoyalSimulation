using System.Collections;
using System.Resources;
using UnityEngine;

public delegate void CustomStart();
public delegate void CustomUpdate(float deltaTime);
public delegate void CustomDestroy();

public class GameManager : MonoBehaviour
{
    public CustomStart ManagerStart;
    public CustomUpdate ManagerUpdate;

    public CustomStart GroundStart;
    public CustomUpdate GroundUpdate;

    public CustomStart ObjectStart;
    public CustomUpdate ObjectUpdate;
    public CustomDestroy ObjectDestroy;

    static GameManager instance;
    public static GameManager Instance => instance;
    ResourceManager resourceManager;
    public ResourceManager ResourceManager => resourceManager;
    PoolManager poolManager;
    public PoolManager PoolManager => poolManager;

    public LoadingCanvas loadingCanvas;

    void Awake()
    {
        instance = this;
    }
    public IEnumerator Start()
    {
        resourceManager = new ResourceManager();
        yield return resourceManager.Initiate();
        poolManager = new PoolManager();
        yield return poolManager.Initiate();

        CloseLoadInfo();
    }

    void Update()
    {
        ManagerStart?.Invoke();
        ManagerStart = null;
        GroundStart?.Invoke();
        GroundStart = null;
        ObjectStart?.Invoke();
        ObjectStart = null;

        ManagerUpdate?.Invoke(Time.deltaTime);
        GroundUpdate?.Invoke(Time.deltaTime);
        ObjectUpdate?.Invoke(Time.deltaTime);

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
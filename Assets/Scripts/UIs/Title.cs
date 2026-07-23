using Steamworks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] string discordLink;
    public GameObject title;
    [SerializeField] GameObject continueBtn;
    [SerializeField] GameObject newGameBtn;
    [SerializeField] GameObject credits;
    [SerializeField] RectTransform buttonsRect;

    [SerializeField] GameObject selectGameMode;
    [SerializeField] GameObject askLoadData;
    [SerializeField] GameObject selectDifficulty;
    [SerializeField] Toggle[] difficulties;
    [SerializeField] Transform focusArrow;

    Color[] difficultyColors = { new(0, 1, 0), new(1, 1, 0), new(1, 0.75f, 0), new(1, 0.5f, 0), new(1, 0.25f, 0), new(1, 0, 0), new(0.75f, 0, 0) };
    public bool haveSaveData;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void CheckUnlockDifficulty()
    {
        if(!SteamManager.Initialized) return;
        if (SteamUserStats.GetAchievement("World Champion", out bool normal)) difficulties[1].GetComponentInChildren<Animator>(true).SetBool("Unlock", normal);
        if (SteamUserStats.GetAchievement("Hard", out bool hard)) difficulties[2].GetComponentInChildren<Animator>(true).SetBool("Unlock", hard);
        if (SteamUserStats.GetAchievement("Very Hard", out bool veryHard)) difficulties[3].GetComponentInChildren<Animator>(true).SetBool("Unlock", veryHard);
        if (SteamUserStats.GetAchievement("Expert", out bool expert)) difficulties[4].GetComponentInChildren<Animator>(true).SetBool("Unlock", expert);
        if (SteamUserStats.GetAchievement("Hardcore", out bool hardcore)) difficulties[5].GetComponentInChildren<Animator>(true).SetBool("Unlock", hardcore);
        if (SteamUserStats.GetAchievement("Nightmare", out bool nightmare)) difficulties[6].GetComponentInChildren<Animator>(true).SetBool("Unlock", nightmare);
    }

    int wantDifficulty;
    public void FocusDifficulty(int index)
    {
        wantDifficulty = index;
        focusArrow.position = new(difficulties[index].GetComponent<RectTransform>().position.x - 225, difficulties[index].GetComponent<RectTransform>().position.y);
        for(int i = 0; i < difficulties.Length; i++)
        {
            Color c = difficultyColors[i];
            c = i == index ? c * new Color(0.78f, 0.78f, 0.78f, 1f) : c;
            difficulties[i].GetComponent<Image>().color = c;
        }
    }

    public void OpenSelectMode()
    {
        selectGameMode.SetActive(true);
        GameManager.Instance.openedWindows.Push(selectDifficulty);
    }

    public void SelectSingleCareerRun()
    {
        if (haveSaveData)
        {
            askLoadData.SetActive(true);
            //Continue();
        }
        else OpenSelectDifficulty();
    }

    public void DeleteSingleCareerRunSaveData()
    {
        GameManager.Instance.Option.DeleteSaveData(0);
        askLoadData.SetActive(false);
        OpenSelectDifficulty();
    }

    public void SelectFreeManagement()
    {
        Load();
    }

    public void OpenSelectDifficulty()
    {
        selectDifficulty.SetActive(true);
        FocusDifficulty(0);
        CheckUnlockDifficulty();
        GameManager.Instance.openedWindows.Push(selectDifficulty);
    }

    public void NewGameSingleCareerRun()
    {
        selectDifficulty.SetActive(false);
        selectGameMode.SetActive(false);
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.ResetData(GameMode.SingleCareerRun, wantDifficulty);
        title.SetActive(false);
        AchievementManager.earnedAchievementsInThisRun = new();
        GameManager.Instance.Option.SetSaveButtonInteractable(false, false);
    }

    public void Continue()
    {
        selectGameMode.SetActive(false);
        StartCoroutine(GameManager.Instance.Load(0));
    }

    public void Load()
    {
        GameManager.Instance.Option.OpenSaveSlot(false);
    }

    public void Options()
    {
        GameManager.Instance.optionCanvas.SetActive(true);
        GameManager.Instance.openedWindows.Push(GameManager.Instance.optionCanvas);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void OpenDiscord()
    {
        Application.OpenURL(discordLink);
    }

    public void Credits()
    {
        credits.SetActive(true);
        GameManager.Instance.openedWindows.Push(credits);
    }

    void OnLocaleChanged(Locale newLocale)
    {
        GameManager.Instance.FixLayout(buttonsRect);
    }
}

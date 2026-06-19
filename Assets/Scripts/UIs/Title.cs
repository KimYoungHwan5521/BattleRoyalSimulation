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

    [SerializeField] Toggle[] difficulties;
    [SerializeField] Transform focusArrow;

    Color[] difficultyColors = { new(0, 1, 0), new(1, 1, 0), new(1, 0.75f, 0), new(1, 0.5f, 0), new(1, 0.25f, 0), new(1, 0, 0), new(0.75f, 0, 0) };

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void CheckSaveData(bool haveSaveData)
    {
        continueBtn.SetActive(haveSaveData);
        newGameBtn.SetActive(!haveSaveData);
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

    public void NewGame()
    {
        GameManager.Instance.ResetData(wantDifficulty);
        title.SetActive(false);
        AchievementManager.earnedAchievementsInThisRun = new();
        GameManager.Instance.outCanvas.SetActive(true);
        GameManager.Instance.Option.SetSaveButtonInteractable(true);
    }

    public void Continue()
    {
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

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    public GameObject title;
    [SerializeField] GameObject credits;
    [SerializeField] RectTransform buttonsRect;

    private void Start()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void NewGame()
    {
        GameManager.Instance.ResetData();
        title.SetActive(false);
        GameManager.Instance.Option.SetSaveButtonInteractable(true);
    }

    public void Continue()
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

    public void Credits()
    {
        credits.SetActive(true);
        GameManager.Instance.openedWindows.Push(credits);
    }

    void OnLocaleChanged(Locale newLocale)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsRect);
    }
}

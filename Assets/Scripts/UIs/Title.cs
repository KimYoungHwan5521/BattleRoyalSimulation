using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] GameObject title;
    [SerializeField] Button continueButton;
    private void Start()
    {
        continueButton.interactable = PlayerPrefs.GetInt("HaveSaveData") != 0;
    }

    public void NewGame()
    {
        title.SetActive(false);
    }

    public void Continue()
    {
        StartCoroutine(GameManager.Instance.Load());
        title.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}

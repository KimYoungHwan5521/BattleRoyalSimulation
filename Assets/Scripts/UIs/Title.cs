using UnityEngine;

public class Title : MonoBehaviour
{
    public GameObject title;

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

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}

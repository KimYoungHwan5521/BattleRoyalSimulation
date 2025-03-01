using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Help : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rect;
    [SerializeField] string description;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        RectTransform descriptionRT = GameManager.Instance.description.GetComponent<RectTransform>();
        TextMeshProUGUI descriptionText = descriptionRT.GetComponentInChildren<TextMeshProUGUI>();
        descriptionText.text = description;
        descriptionRT.sizeDelta = new(descriptionText.rectTransform.rect.width, descriptionText.rectTransform.rect.height);
        descriptionRT.anchoredPosition = 
            new(Mathf.Clamp(rect.anchoredPosition.x + 25, -Screen.width / 2, Screen.width / 2 - descriptionRT.rect.width),
            Mathf.Clamp(rect.anchoredPosition.y - 25, -Screen.height / 2 + descriptionRT.rect.height, Screen.height / 2));
        descriptionRT.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.description.SetActive(false);
    }
}

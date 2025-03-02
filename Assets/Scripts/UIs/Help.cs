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
        descriptionRT.position = 
            new(Mathf.Clamp(rect.position.x + 25, 0, Screen.width - descriptionRT.rect.width),
            Mathf.Clamp(rect.position.y - 25, descriptionRT.rect.height, Screen.height));
        descriptionRT.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.Instance.description.SetActive(false);
    }
}

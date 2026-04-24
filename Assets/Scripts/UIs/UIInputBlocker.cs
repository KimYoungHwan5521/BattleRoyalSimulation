using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputBlocker : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        eventData.Use();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        eventData.Use();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        eventData.Use();
    }
}
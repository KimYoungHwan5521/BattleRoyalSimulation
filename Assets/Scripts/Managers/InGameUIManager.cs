using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameUIManager : MonoBehaviour
{
    bool isClicked;
    Vector2 clickPos;
    Vector3 cameraPosBeforeClick;

    Vector2 navVector;

    [SerializeField] CustomObject selectedObject;
    [SerializeField] GameObject selectedObjectInfo;
    [SerializeField] TextMeshProUGUI selectedObjectName;
    [SerializeField] GameObject selectedObjectsCurrentWeapon;
    [SerializeField] GameObject selectedObjectsCurrentHelmet;
    [SerializeField] GameObject selectedObjectsCurrentVest;
    [SerializeField] GameObject[] selectedObjectsItems;

    private void Update()
    {
        ManualCameraMove();
        SetSelectedObjectInfo();
    }
    //public void RegularSpeed() { Time.timeScale = 1; timeScaleText.text = $"x{(int)Time.timeScale}"; }
    //public void Accelerate() { if (Time.timeScale < 16) Time.timeScale *= 2; else Time.timeScale = 1; timeScaleText.text = $"x{(int)Time.timeScale}"; }

    void ManualCameraMove()
    {
        Camera.main.transform.position += (Vector3)navVector * Camera.main.orthographicSize * 0.2f;
        if (isClicked)
        {
            Camera.main.transform.position = cameraPosBeforeClick + ((Vector3)clickPos - Input.mousePosition) * 0.02f * 0.2f * Camera.main.orthographicSize;
        }
    }

    void OnNavigate(InputValue value)
    {
        navVector = value.Get<Vector2>();
    }

    void OnScrollWheel(InputValue value)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - value.Get<Vector2>().y * 0.01f, 1, 30);
    }

    void OnClick(InputValue value)
    {
        isClicked = value.Get<float>() > 0;
        if (isClicked)
        {
            clickPos = Input.mousePosition;
            cameraPosBeforeClick = Camera.main.transform.position;
            
            SelectObject();
        }

    }

    void SelectObject()
    {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);

        bool selectedNotNull = false;
        foreach (Collider2D hit in hits)
        {
            if(!hit.isTrigger && hit.TryGetComponent(out CustomObject clickedObject))
            {
                if(clickedObject is Survivor || clickedObject is Box)
                {
                    selectedObject = clickedObject;
                    selectedNotNull = true;
                    break;
                }
            }
        }
        if(!selectedNotNull) selectedObject = null;
    }

    void SetSelectedObjectInfo()
    {
        if(selectedObject == null)
        {
            selectedObjectInfo.SetActive(false);
        }
        else
        {
            selectedObjectInfo.SetActive(true);
            if(selectedObject is Survivor)
            {
                Survivor selectedSurvivor = selectedObject as Survivor;
                selectedObjectName.text = selectedSurvivor.survivorName;
                selectedObjectsCurrentWeapon.GetComponentInChildren<TextMeshProUGUI>().text = selectedSurvivor.IsValid(selectedSurvivor.CurrentWeapon) ? selectedSurvivor.CurrentWeapon.itemName : "None";
                selectedObjectsCurrentHelmet.GetComponentInChildren<TextMeshProUGUI>().text = selectedSurvivor.IsValid(selectedSurvivor.CurrentHelmet) ? selectedSurvivor.CurrentHelmet.itemName : "None";
                selectedObjectsCurrentVest.GetComponentInChildren<TextMeshProUGUI>().text = selectedSurvivor.IsValid(selectedSurvivor.CurrentVest) ? selectedSurvivor.CurrentVest.itemName : "None";
                selectedObjectsCurrentWeapon.SetActive(true);
                selectedObjectsCurrentHelmet.SetActive(true);
                selectedObjectsCurrentVest.SetActive(true);
                for(int i = 0; i<selectedObjectsItems.Length; i++)
                {
                    if(selectedSurvivor.Inventory.Count > i)
                    {
                        selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedSurvivor.Inventory[i].itemName} x {selectedSurvivor.Inventory[i].amount}";
                        selectedObjectsItems[i].SetActive(true);
                    }
                    else
                    {
                        selectedObjectsItems[i].SetActive(false);
                    }
                }
            }
            else if(selectedObject is Box)
            {
                Box selectedBox = selectedObject as Box;
                selectedObjectName.text = "Box";
                selectedObjectsCurrentWeapon.SetActive(false);
                selectedObjectsCurrentHelmet.SetActive(false);
                selectedObjectsCurrentVest.SetActive(false);
                for (int i = 0; i < selectedObjectsItems.Length; i++)
                {
                    if (selectedBox.items.Count > i)
                    {
                        selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedBox.items[i].itemName} x {selectedBox.items[i].amount}";
                        selectedObjectsItems[i].SetActive(true);
                    }
                    else
                    {
                        selectedObjectsItems[i].SetActive(false);
                    }
                }
            }
            else
            {
                selectedObjectInfo.SetActive(false);
            }
        }
    }
}

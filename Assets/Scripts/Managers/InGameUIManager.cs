using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    bool isClicked;
    Vector2 clickPos;
    Vector3 cameraPosBeforeClick;

    Vector2 navVector;

    [SerializeField] GraphicRaycaster[] raycasters;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] RectTransform display;
    Transform cameraTarget;
    OutGameUIManager outGameUIManager;

    [Header("Left Top")]
    [SerializeField] TextMeshProUGUI leftSurvivors;
    [SerializeField] GameObject predictionResult;
    [SerializeField] GameObject[] predictionResultRows;
    [SerializeField] TextMeshProUGUI[] predictionResultPredictions;
    [SerializeField] Image[] predictionResultPortraits;
    [SerializeField] Image[] predictionResultBGs;
    [SerializeField] TextMeshProUGUI[] predictionResultResults;

    [Header("Toolbar")]
    [SerializeField] TextMeshProUGUI currentTimeScaleText;

    [Header("Log")]
    [SerializeField] ScrollRect logScrollView;
    [SerializeField] TextMeshProUGUI log;

    [Header("Selected Object Info")]
    [SerializeField] GameObject autoFocus;
    [SerializeField] GameObject seletedImage;

    [SerializeField] CustomObject selectedObject;
    public CustomObject SelectedObject;
    [SerializeField] GameObject selectedObjectInfo;

    [SerializeField] Image selectedObjectImage;
    [SerializeField] TextMeshProUGUI selectedObjectName;

    [SerializeField] GameObject selectedSurvivorsHealthBar;
    [SerializeField] Image selectedSurvivorsHealthBarImage;
    [SerializeField] TextMeshProUGUI selectedSurvivorsHealthText;

    [SerializeField] GameObject selectedSurvivorBleedingBar;
    [SerializeField] Image selectedSurvivorBleedingBarImage;
    [SerializeField] Animator bleedingAnim;

    [SerializeField] GameObject seletedObjectTab;

    int currentTab = 1;
    public int CurrentTab
    {
        get { return currentTab; }
        set
        {
            currentTab = value;
            statTab.SetActive(value == 0);
            inventoryTab.SetActive(value == 1);
        }
    }

    [Header("Stat / Status")]
    [SerializeField] GameObject statTab;
    [SerializeField] Image powerBar;
    [SerializeField] Image attackSpeedBar;
    [SerializeField] Image moveSpeedBar;
    [SerializeField] Image farmingSpeedBar;
    [SerializeField] Image shootingBar;
    [SerializeField] TextMeshProUGUI powerText;
    [SerializeField] TextMeshProUGUI attackSpeedText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI farmingSpeedText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] AutoNewLineLayoutGroup characteristics;

    [SerializeField] GameObject[] injuries;

    [Header("Inventory")]
    [SerializeField] GameObject inventoryTab;
    [SerializeField] GameObject selectedObjectsCurrentWeapon;
    Image selectedObjectsCurrentWeaponImage;
    TextMeshProUGUI selectedObjectsCurrentWeaponText;

    [SerializeField] GameObject selectedObjectsCurrentHelmet;
    Image selectedObjectsCurrentHelmetImage;
    TextMeshProUGUI selectedObjectsCurrentHelmetText;

    [SerializeField] GameObject selectedObjectsCurrentVest;
    Image selectedObjectsCurrentVestImage;
    TextMeshProUGUI selectedObjectsCurrentVestText;

    [SerializeField] GameObject[] selectedObjectsItems;
    
    private void Start()
    {
        outGameUIManager = GetComponent<OutGameUIManager>();
        selectedObjectsCurrentWeaponImage = selectedObjectsCurrentWeapon.GetComponentInChildren<Image>();
        selectedObjectsCurrentWeaponText = selectedObjectsCurrentWeapon.GetComponentInChildren<TextMeshProUGUI>();
        selectedObjectsCurrentHelmetImage = selectedObjectsCurrentHelmet.GetComponentInChildren<Image>();
        selectedObjectsCurrentHelmetText = selectedObjectsCurrentHelmet.GetComponentInChildren<TextMeshProUGUI>();
        selectedObjectsCurrentVestImage = selectedObjectsCurrentVest.GetComponentInChildren<Image>();
        selectedObjectsCurrentVestText = selectedObjectsCurrentVest.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        AutoCameraMove();
        ManualCameraMove();
        UpdateSelectedObjectInfo();
    }

    void AutoCameraMove()
    {
        if(cameraTarget != null) Camera.main.transform.position = new(cameraTarget.transform.position.x, cameraTarget.transform.position.y, -10);
    }

    void ManualCameraMove()
    {
        Camera.main.transform.position += (Vector3)navVector * Camera.main.orthographicSize * 0.1f;
        if (!IsPointerOverUI() && isClicked)
        {
            Camera.main.transform.position = cameraPosBeforeClick + ((Vector3)clickPos - Input.mousePosition) * 0.02f * 0.2f * Camera.main.orthographicSize;
            cameraTarget = null;
        }
    }

    void OnNavigate(InputValue value)
    {
        navVector = value.Get<Vector2>();
    }

    void OnScrollWheel(InputValue value)
    {
        if(!IsPointerOverUI())Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - value.Get<Vector2>().y * 0.02f, 1, 100);
    }

    void OnClick(InputValue value)
    {
        if (IsPointerOverUI()) return;

        isClicked = value.Get<float>() > 0;
        if (isClicked)
        {
            clickPos = Input.mousePosition;
            cameraPosBeforeClick = Camera.main.transform.position;
        }
        else if(Vector2.Distance(Input.mousePosition, clickPos) < 30f)
        {
            SelectObject();
        }
    }

    public void SetTimeScale(int wantScale)
    {
        Time.timeScale = wantScale;
        currentTimeScaleText.text = $"x {wantScale}";
    }

    public void TimeScaleUp()
    {
        Time.timeScale = Mathf.Clamp(Time.timeScale + 1, Time.timeScale, 5);
        currentTimeScaleText.text = $"x {(int)Time.timeScale}";
    }

    public void SetLeftSurvivors(int survivorsCount)
    {
        leftSurvivors.text = $"Left Survivors : {survivorsCount}";
    }

    public void SetPredictionUI()
    {
        predictionResult.SetActive(outGameUIManager.BettingAmount > 0);
        for(int i=0; i<predictionResultRows.Length; i++)
        {
            if (i < outGameUIManager.PredictionNumber)
            {
                predictionResultRows[i].SetActive(true);
                predictionResultPredictions[i].text = outGameUIManager.Predictions[i];
                predictionResultPortraits[i].color = GameManager.Instance.BattleRoyaleManager.Survivors.Find(x => x.survivorName == outGameUIManager.Predictions[i]).GetComponent<SpriteRenderer>().color;
                predictionResultBGs[i].color = Color.white;
                predictionResultResults[i].text = "";
            }
            else predictionResultRows[i].SetActive(false);
        }
    }

    public void SetSurvivorRank(string survivorName, int survivorRank)
    {
        for(int i=0; i<outGameUIManager.Predictions.Length; i++)
        {
            if (outGameUIManager.Predictions[i] == survivorName)
            {
                predictionResultResults[i].text = survivorRank switch
                {
                    0 => "1st",
                    1 => "2nd",
                    2 => "3rd",
                    _ => $"{survivorRank + 1}th",
                };
                if (i == survivorRank) predictionResultBGs[i].color = new Color(0.48f, 1f, 0.44f);
                else if(survivorRank < outGameUIManager.PredictionNumber) predictionResultBGs[i].color = new Color(0.89f, 0.93f, 0.39f);
                else predictionResultBGs[i].color = new Color(0.88f, 0.43f, 0.43f);
                return;
            }
        }
    }

    public void ShowKillLog(string victim, string cause)
    {
        TextMeshProUGUI killLog = PoolManager.Spawn(ResourceEnum.Prefab.KillLog, display).GetComponentInChildren<TextMeshProUGUI>();
        string message = $"<color=red>{victim}</color> has defeated by <color=yellow>{cause}</color>";
        killLog.text = message;
        log.text += "\n" + message;
        logScrollView.verticalNormalizedPosition = 0;
    }

    public void ClearLog()
    {
        log.text = ""; 
        powerText.GetComponent<Help>().SetDescription("");
        attackSpeedText.GetComponent<Help>().SetDescription("");
        moveSpeedText.GetComponent<Help>().SetDescription("");
        farmingSpeedText.GetComponent<Help>().SetDescription("");
        shootingText.GetComponent<Help>().SetDescription("");
    }

    public void AutoFocus()
    {
        if (autoFocus.GetComponent<Toggle>().isOn && selectedObject != null)
        {
            cameraTarget = selectedObject.transform;
        }
        else cameraTarget = null;
    }

    void SelectObject()
    {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);

        bool selectedNotNull = false;
        foreach (Collider2D hit in hits)
        {
            if(!hit.isTrigger)
            {
                if(hit.TryGetComponent(out CustomObject clickedObject))
                {
                    if(clickedObject is Survivor || clickedObject is Box)
                    {
                        selectedObject = clickedObject;
                        seletedObjectTab.SetActive(clickedObject is Survivor);
                        if (clickedObject is Survivor)
                        {
                            Survivor survivor = clickedObject as Survivor;
                            cameraTarget = selectedObject.transform;
                            SetSelectedObjectInfoOnce();
                        }
                        else CurrentTab = 1;
                        selectedNotNull = true;
                        break;
                    }
                }
            }
            else
            {
                if(hit.TryGetComponent(out Survivor survivor) && survivor.IsDead)
                {
                    selectedObject = survivor;
                    SetSelectedObjectInfoOnce();
                    selectedNotNull = true;
                    break;
                }
            }
        }
        if(!selectedNotNull) selectedObject = null;
    }

    void SetSelectedObjectInfoOnce()
    {
        if (selectedObject == null) return;
        if(selectedObject is Survivor)
        {
            Survivor selectedSurvivor = selectedObject as Survivor;
            selectedObjectImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Survivor);
            Vector3 colorVector = BattleRoyaleManager.colorInfo[selectedSurvivor.survivorID];
            selectedObjectImage.color = new(colorVector.x, colorVector.y, colorVector.z);
            selectedObjectName.text = selectedSurvivor.survivorName;

            powerBar.fillAmount = selectedSurvivor.LinkedSurvivorData._power / 100f;
            attackSpeedBar.fillAmount = selectedSurvivor.LinkedSurvivorData._attackSpeed / 100f;
            moveSpeedBar.fillAmount = selectedSurvivor.LinkedSurvivorData._moveSpeed / 100f;
            farmingSpeedBar.fillAmount = selectedSurvivor.LinkedSurvivorData._farmingSpeed / 100f;
            shootingBar.fillAmount = selectedSurvivor.LinkedSurvivorData._shooting / 100f;

            UpdatableSelectedObjectInfo(selectedObject);
        }
        else if(selectedObject is Box)
        {
            Box selectedBox = selectedObject as Box;
            selectedObjectImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Box);
            selectedObjectImage.color = Color.white;
            selectedObjectName.text = "Box";
            selectedSurvivorsHealthBar.SetActive(false);
            selectedSurvivorBleedingBar.SetActive(false);
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
        }
    }

    void UpdatableSelectedObjectInfo(CustomObject selectedObject)
    {
        if(selectedObject is Survivor)
        {
            UpdateSelectedObjectStat(selectedObject as Survivor);
            UpdateSelectedObjectInjury(selectedObject as Survivor);
        }
        UpdateSelectedObjectInventory(selectedObject);
    }

    public void UpdateSelectedObjectStat(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        powerText.GetComponent<Help>().SetDescription("");
        attackSpeedText.GetComponent<Help>().SetDescription("");
        moveSpeedText.GetComponent<Help>().SetDescription("");
        farmingSpeedText.GetComponent<Help>().SetDescription("");
        shootingText.GetComponent<Help>().SetDescription("");

        powerText.text = $"{survivor.LinkedSurvivorData._power}";
        if (survivor.affectionList_AttackDamage.Count > 0)
        {
            if (survivor.AttackDamage > survivor.LinkedSurvivorData.AttackDamage) powerText.text += $"(<#009900>��)</color>";
            else powerText.text += $"(<color=red>��)</color>";
            string description = "Affected by : ";
            foreach (string affect in survivor.affectionList_AttackDamage) description += $"{affect}, ";
            powerText.GetComponent<Help>().SetDescription(description);
        }
        attackSpeedText.text = $"{survivor.AttackSpeed:0.##}";
        if (survivor.affectionList_AttackSpeed.Count > 0)
        {
            if (survivor.AttackSpeed > survivor.LinkedSurvivorData.AttackSpeed) attackSpeedText.text += $"(<#009900>��)</color>";
            else attackSpeedText.text += $"(<color=red>��)</color>";
            string description = "Affected by : ";
            foreach (string affect in survivor.affectionList_AttackSpeed) description += $"{affect}, ";
            attackSpeedText.GetComponent<Help>().SetDescription(description);
        }
        moveSpeedText.text = $"{survivor.MoveSpeed:0.##}";
        if (survivor.affectionList_MoveSpeed.Count > 0)
        {
            if (survivor.MoveSpeed > survivor.LinkedSurvivorData.MoveSpeed) moveSpeedText.text += $"(<#009900>��)</color>";
            else moveSpeedText.text += $"(<color=red>��)</color>";
            string description = "Affected by : ";
            foreach (string affect in survivor.affectionList_MoveSpeed) description += $"{affect}, ";
            moveSpeedText.GetComponent<Help>().SetDescription(description);
        }
        farmingSpeedText.text = $"{survivor.FarmingSpeed:0.##}";
        if (survivor.affectionList_FarmingSpeed.Count > 0)
        {
            if (survivor.FarmingSpeed > survivor.LinkedSurvivorData.FarmingSpeed) farmingSpeedText.text += $"(<#009900>��)</color>";
            else farmingSpeedText.text += $"(<color=red>��)</color>";
            string description = "Affected by : ";
            foreach (string affect in survivor.affectionList_FarmingSpeed) description += $"{affect}, ";
            farmingSpeedText.GetComponent<Help>().SetDescription(description);
        }
        shootingText.text = $"{survivor.Shooting:0.##}";
        if (survivor.affectionList_Shooting.Count > 0)
        {
            if (survivor.Shooting > survivor.LinkedSurvivorData.Shooting) shootingText.text += $"(<#009900>��)</color>";
            else shootingText.text += $"(<color=red>��)</color>";
            string description = "Affected by : ";
            foreach (string affect in survivor.affectionList_Shooting) description += $"{affect}, ";
            shootingText.GetComponent<Help>().SetDescription(description);
        }
        characteristics.ArrangeCharacteristics(survivor.LinkedSurvivorData);
    }

    public void UpdateSelectedObjectInjury(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        for (int i = 0; i < injuries.Length; i++)
        {
            if (survivor.injuries.Count > i)
            {
                injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{survivor.injuries[i].site} {survivor.injuries[i].type}";
                injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{survivor.injuries[i].degree:0.##}";
                injuries[i].GetComponentInChildren<Help>().SetDescription(survivor.injuries[i].site);
                injuries[i].SetActive(true);
            }
            else
            {
                injuries[i].SetActive(false);
            }
        }
    }

    public void UpdateSelectedObjectInventory(CustomObject selected)
    {
        if (selected != selectedObject) return;
        if(selected is Survivor)
        {
            Survivor selectedSurvivor = selected as Survivor;
            if (selectedSurvivor.CurrentWeapon != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentWeapon.itemType}", out var weaponSpriteEnum))
            {
                selectedObjectsCurrentWeaponImage.sprite = ResourceManager.Get(weaponSpriteEnum);
                selectedObjectsCurrentWeaponImage.GetComponent<AspectRatioFitter>().aspectRatio
                    = selectedObjectsCurrentWeaponImage.sprite.textureRect.width / selectedObjectsCurrentWeaponImage.sprite.textureRect.height;
            }
            else selectedObjectsCurrentWeaponImage.sprite = null;
            if (selectedSurvivor.IsValid(selectedSurvivor.CurrentWeapon))
            {
                if (selectedSurvivor.CurrentWeapon is RangedWeapon)
                {
                    int validBulletAmount = selectedSurvivor.ValidBullet != null ? selectedSurvivor.ValidBullet.amount : 0;
                    selectedObjectsCurrentWeaponText.text = $"{selectedSurvivor.CurrentWeapon.itemName} ({selectedSurvivor.CurrentWeaponAsRangedWeapon.CurrentMagazine} / {validBulletAmount})";
                }
                else
                {
                    selectedObjectsCurrentWeaponText.text = selectedSurvivor.CurrentWeapon.itemName;
                }
            }
            else
            {
                selectedObjectsCurrentWeaponText.text = "None";
            }

            if (selectedSurvivor.CurrentHelmet != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentHelmet.itemType}", out var helmetSpriteEnum))
                selectedObjectsCurrentHelmetImage.sprite = ResourceManager.Get(helmetSpriteEnum);
            else selectedObjectsCurrentHelmetImage.sprite = null;
            selectedObjectsCurrentHelmetText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentHelmet) ? selectedSurvivor.CurrentHelmet.itemName : "None";

            if (selectedSurvivor.CurrentVest != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentVest.itemType}", out var vestSpriteEnum))
                selectedObjectsCurrentVestImage.sprite = ResourceManager.Get(vestSpriteEnum);
            else selectedObjectsCurrentVestImage.sprite = null;
            selectedObjectsCurrentVestText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentVest) ? selectedSurvivor.CurrentVest.itemName : "None";

            selectedObjectsCurrentWeapon.SetActive(true);
            selectedObjectsCurrentHelmet.SetActive(true);
            selectedObjectsCurrentVest.SetActive(true);
            for (int i = 0; i < selectedObjectsItems.Length; i++)
            {
                if (selectedSurvivor.Inventory.Count > i)
                {
                    if (Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.Inventory[i].itemType}", out var spriteEnum))
                    {
                        Image itemImage = selectedObjectsItems[i].GetComponentInChildren<Image>();
                        itemImage.sprite = ResourceManager.Get(spriteEnum);
                        selectedObjectsItems[i].GetComponentInChildren<AspectRatioFitter>().aspectRatio
                            = itemImage.sprite.textureRect.width / itemImage.sprite.textureRect.height;
                    }
                    else
                    {
                        selectedObjectsItems[i].GetComponentInChildren<Image>().sprite = null;
                    }
                    selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedSurvivor.Inventory[i].itemName} x {selectedSurvivor.Inventory[i].amount}";
                    selectedObjectsItems[i].SetActive(true);
                }
                else
                {
                    selectedObjectsItems[i].SetActive(false);
                }
            }
        }
        else if(selected is Box)
        {
            Box selectedBox = selected as Box;
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
            for (int i = 0; i < selectedObjectsItems.Length; i++)
            {
                if (selectedBox.items.Count > i)
                {
                    if (Enum.TryParse<ResourceEnum.Sprite>($"{selectedBox.items[i].itemType}", out var spriteEnum))
                    {
                        selectedObjectsItems[i].GetComponentInChildren<Image>().sprite = ResourceManager.Get(spriteEnum);
                    }
                    else
                    {
                        selectedObjectsItems[i].GetComponentInChildren<Image>().sprite = null;
                    }
                    selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedBox.items[i].itemName} x {selectedBox.items[i].amount}";
                    selectedObjectsItems[i].SetActive(true);
                }
                else
                {
                    selectedObjectsItems[i].SetActive(false);
                }
            }

        }
    }

    void UpdateSelectedObjectInfo()
    {
        if(selectedObject == null)
        {
            seletedImage.SetActive(false);
            selectedObjectInfo.SetActive(false);
        }
        else
        {
            seletedImage.transform.position = selectedObject.transform.position;
            seletedImage.SetActive(true);
            selectedObjectInfo.SetActive(true);
            if(selectedObject is Survivor)
            {
                Survivor selectedSurvivor = selectedObject as Survivor;
                autoFocus.SetActive(true);
                selectedSurvivorsHealthBar.SetActive(true);
                selectedSurvivorBleedingBar.SetActive(true);

                selectedSurvivorsHealthBarImage.fillAmount = selectedSurvivor.CurHP / selectedSurvivor.MaxHP;
                selectedSurvivorsHealthText.text = $"{selectedSurvivor.CurHP:0} / {selectedSurvivor.MaxHP:0}";

                selectedSurvivorBleedingBarImage.fillAmount = (selectedSurvivor.maxBlood - selectedSurvivor.curBlood) / (selectedSurvivor.maxBlood * 0.5f);

                bleedingAnim.SetBool("Bleeding", selectedSurvivor.BleedingAmount > 0);
                
            }
            else if(selectedObject is Box)
            {
                autoFocus.SetActive(false);
                selectedSurvivorsHealthBar.SetActive(false);
                selectedSurvivorBleedingBar.SetActive(false);
            }
            else
            {
                selectedObjectInfo.SetActive(false);
            }
        }
    }

    bool IsPointerOverUI()
    {
        PointerEventData pointerData = new(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        foreach (GraphicRaycaster raycaster in raycasters)
        {
            raycaster.Raycast(pointerData, results);
            return results.Count > 0;
        }

        return false;
    }
}

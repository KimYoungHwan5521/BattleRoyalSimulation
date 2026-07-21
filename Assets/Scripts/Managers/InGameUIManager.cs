using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    bool isClicked;
    Vector2 clickPos;
    Vector3 cameraPosBeforeClick;

    Vector2 navVector;

    int lastTimeScale = 1;

    [SerializeField] GraphicRaycaster[] raycasters;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] RectTransform display;
    Transform cameraTarget;
    OutGameUIManager outGameUIManager;
    float cameraLeftLimit = -25;
    float cameraDownLimit = -25;
    float cameraRightLimit = 75;
    float cameraUpLimit = 75;

    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 100f;
    [SerializeField] private float zoomSensitivity = 0.05f;
    [SerializeField] private float zoomSmoothTime = 0.12f;

    private float targetZoom;
    private float zoomVelocity;

    bool rankChangeAnimation;
    float curRankChangeAnimTime;

    [Header("Left Top")]
    [SerializeField] TextMeshProUGUI leftSurvivors;
    [SerializeField] GameObject predictionResult;
    [SerializeField] GameObject[] predictionResultRows;
    [SerializeField] TextMeshProUGUI[] predictionResultPredictions;
    [SerializeField] Image[] predictionResultPortraits;
    [SerializeField] Image[] predictionResultBGs;
    [SerializeField] TextMeshProUGUI[] predictionResultResults;
    int predictionLeft;
    [SerializeField] Button foldOtherSurvivors;
    [SerializeField] GameObject[] otherSurvivorsResultRows;
    [SerializeField] TextMeshProUGUI[] otherSurvivorsNames;
    [SerializeField] Image[] otherSurvivorsPortraits;
    [SerializeField] Image[] otherSurvivorsResultBGs;
    [SerializeField] TextMeshProUGUI[] otherSurvivorsResults;
    [SerializeField] RectTransform otherSurvivorsBox;
    List<int> targetRank;
    List<float> rankChangeDistances;

    [Header("Middle Top")]
    [SerializeField] TextMeshProUGUI currentBattleTimer;
    [SerializeField] TextMeshProUGUI nextProhibitTimer;

    [Header("Toolbar")]
    [SerializeField] TextMeshProUGUI currentTimeScaleText;
    [SerializeField] GameObject exitBattleRoyale;

    [Header("Log")]
    [SerializeField] ScrollRect logScrollView;
    [SerializeField] TextMeshProUGUI log;

    [Header("Selected Object Info")]
    [SerializeField] GameObject autoFocus;
    [SerializeField] GameObject seletedImage;

    [SerializeField] CustomObject selectedObject;
    [SerializeField] GameObject selectedObjectInfo;
    [SerializeField] GameObject simulationTip;

    [SerializeField] TextMeshProUGUI selectedObjectKillCount;
    [SerializeField] Image selectedObjectImage;
    [SerializeField] TextMeshProUGUI selectedObjectName;
    [SerializeField] TextMeshProUGUI selectedObjectCurrentStatus;

    [SerializeField] GameObject selectedSurvivorsHealthBar;
    [SerializeField] Image selectedSurvivorsHealthBarImage;
    [SerializeField] TextMeshProUGUI selectedSurvivorsHealthText;

    [SerializeField] GameObject selectedSurvivorBleedingBar;
    [SerializeField] Image selectedSurvivorBleedingBarImage;
    [SerializeField] Animator bleedingAnim;

    [SerializeField] GameObject selectedObjectTab;

    int currentTab = 1;
    public int CurrentTab
    {
        get { return currentTab; }
        set
        {
            currentTab = value;
            statTab.SetActive(value == 0);
            if (value == 0 && selectedObject is Survivor selectedSurvivor) characteristics.ArrangeCharacteristics(selectedSurvivor.LinkedSurvivorData);
            inventoryTab.SetActive(value == 1);
            injuriesTab.SetActive(value == 2);
        }
    }

    [Header("Stat / Status")]
    [SerializeField] GameObject statTab;
    [SerializeField] Image strengthBar;
    [SerializeField] Image agilityBar;
    [SerializeField] Image fightingBar;
    [SerializeField] Image shootingBar;
    [SerializeField] Image craftingBar;
    [SerializeField] Image knowledgeBar;
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI agilityText;
    [SerializeField] TextMeshProUGUI fightingText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] TextMeshProUGUI craftingText;
    [SerializeField] TextMeshProUGUI knowledgeText;
    [SerializeField] AutoNewLineLayoutGroup characteristics;

    [Header("Inventory")]
    public Sprite[] craftingQualityOutlines;
    [SerializeField] GameObject inventoryTab;
    [SerializeField] GameObject selectedObjectsCurrentWeapon;
    [SerializeField] Image selectedObjectsCurrentWeaponOutline;
    [SerializeField] Image selectedObjectsCurrentWeaponImage;
    TextMeshProUGUI selectedObjectsCurrentWeaponText;

    [SerializeField] GameObject selectedObjectsCurrentHelmet;
    [SerializeField] Image selectedObjectsCurrentHelmetOutline;
    [SerializeField] Image selectedObjectsCurrentHelmetImage;
    TextMeshProUGUI selectedObjectsCurrentHelmetText;
    [SerializeField] GameObject selectedObjectsCurrentHelmetDurability;

    [SerializeField] GameObject selectedObjectsCurrentVest;
    [SerializeField] Image selectedObjectsCurrentVestOutline;
    [SerializeField] Image selectedObjectsCurrentVestImage;
    TextMeshProUGUI selectedObjectsCurrentVestText;
    [SerializeField] GameObject selectedObjectsCurrentVestDurability;

    [SerializeField] TMP_Dropdown selectedObjectInventorySortDropdown;
    [SerializeField] Transform selectedObjectsInventory;
    GameObject[] selectedObjectsItems;

    [Header("Injuries")]
    [SerializeField] GameObject injuriesTab;
    [SerializeField] GameObject[] injuries;
    [Header("Head")]
    [SerializeField] Image head;
    [SerializeField] Image rightEye;
    [SerializeField] Image leftEye;
    [SerializeField] Image rightEar;
    [SerializeField] Image leftEar;
    [SerializeField] Image cheek;
    [SerializeField] Image nose;
    [SerializeField] Image jaw;
    [SerializeField] Image neck;
    [Header("Torso")]
    [SerializeField] Image chest;
    [SerializeField] Image ribs;
    [SerializeField] Image abdomen;
    [SerializeField] Image organs;
    [Header("Arm")]
    [SerializeField] Image rightArm;
    [SerializeField] Image leftArm;
    [SerializeField] Image rightHand;
    [SerializeField] Image leftHand;
    [SerializeField] Image rightThumb;
    [SerializeField] Image leftThumb;
    [SerializeField] Image rightIndexFinger;
    [SerializeField] Image leftIndexFinger;
    [SerializeField] Image rightMiddleFinger;
    [SerializeField] Image leftMiddleFinger;
    [SerializeField] Image rightRingFinger;
    [SerializeField] Image leftRingFinger;
    [SerializeField] Image rightLittleFinger;
    [SerializeField] Image leftLittleFinger;
    [Header("Leg")]
    [SerializeField] Image rightLeg;
    [SerializeField] Image leftLeg;
    [SerializeField] Image rightKnee;
    [SerializeField] Image leftKnee;
    [SerializeField] Image rightFoot;
    [SerializeField] Image leftFoot;
    [SerializeField] Image rightBigToe;
    [SerializeField] Image leftBigToe;
    [SerializeField] Image rightIndexToe;
    [SerializeField] Image leftIndexToe;
    [SerializeField] Image rightMiddleToe;
    [SerializeField] Image leftMiddleToe;
    [SerializeField] Image rightRingToe;
    [SerializeField] Image leftRingToe;
    [SerializeField] Image rightLittleToe;
    [SerializeField] Image leftLittleToe;

    private void Awake()
    {
        targetZoom = Camera.main.orthographicSize;
        outGameUIManager = GetComponent<OutGameUIManager>();
        selectedObjectsCurrentWeaponText = selectedObjectsCurrentWeapon.GetComponentInChildren<TextMeshProUGUI>();
        selectedObjectsCurrentHelmetText = selectedObjectsCurrentHelmet.GetComponentInChildren<TextMeshProUGUI>();
        selectedObjectsCurrentVestText = selectedObjectsCurrentVest.GetComponentInChildren<TextMeshProUGUI>();

        selectedObjectsItems = new GameObject[selectedObjectsInventory.childCount];
        for (int i = 0; i < selectedObjectsInventory.childCount; i++) selectedObjectsItems[i] = selectedObjectsInventory.GetChild(i).gameObject;
    }

    private void Start()
    {
        selectedObjectInventorySortDropdown.ClearOptions();
        selectedObjectInventorySortDropdown.AddOptions(new List<string> {
            new LocalizedString("Item", "Acquired").GetLocalizedString(),
            new LocalizedString("Basic", "Item Type").GetLocalizedString()
            });

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void Update()
    {
        AutoCameraMove();
        ManualCameraMove();
        UpdateSelectedObjectInfo();
        OnKeyboardInput();
        if (GameManager.Instance.BattleRoyaleManager == null || !GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart) return;
        currentBattleTimer.text = $"{(int)GameManager.Instance.BattleRoyaleManager.battleTime / 60:00} : {(int)GameManager.Instance.BattleRoyaleManager.battleTime % 60:00}";
        nextProhibitTimer.text = $"{(int)GameManager.Instance.BattleRoyaleManager.NextProhibitTime / 60:00} : {(int)GameManager.Instance.BattleRoyaleManager.NextProhibitTime % 60:00}";
    }

    private void LateUpdate()
    {
        Camera.main.orthographicSize = Mathf.SmoothDamp(
            Camera.main.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        if(rankChangeAnimation)
        {
            curRankChangeAnimTime += Time.unscaledDeltaTime;
            for(int i=0; i< rankChangeDistances.Count; i++)
            {
                otherSurvivorsResultRows[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, rankChangeDistances[i]) * Time.unscaledDeltaTime;
            }
            if(curRankChangeAnimTime > 1f)
            {
                for (int i = 0; i < rankChangeDistances.Count; i++)
                {
                    otherSurvivorsResultRows[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, targetRank[i] * -30);
                }
                rankChangeAnimation = false;
            }
        }
    }

    void AutoCameraMove()
    {
        if (autoFocus.GetComponent<Toggle>().isOn && cameraTarget != null) Camera.main.transform.position = new(cameraTarget.transform.position.x, cameraTarget.transform.position.y, -10);
    }

    void ManualCameraMove()
    {
        Vector3 cameraPos;
        cameraPos = Camera.main.transform.position + (Vector3)navVector * Camera.main.orthographicSize * 0.05f;
        if(navVector.magnitude > 0.1f) autoFocus.GetComponent<Toggle>().isOn = false;
        if (!IsPointerOverUI() && isClicked)
        {
            cameraPos = cameraPosBeforeClick + ((Vector3)clickPos - Input.mousePosition) * 0.02f * 0.2f * Camera.main.orthographicSize;
            cameraTarget = null;
            autoFocus.GetComponent<Toggle>().isOn = false;
        }
        Camera.main.transform.position = new(Mathf.Clamp(cameraPos.x, cameraLeftLimit, cameraRightLimit), Mathf.Clamp(cameraPos.y, cameraDownLimit, cameraUpLimit), -10);
    }

    public void CameraMoveToSelectedObject()
    {
        if (selectedObject == null) return;
        cameraTarget = selectedObject.transform;
        Camera.main.transform.position = new(cameraTarget.transform.position.x, cameraTarget.transform.position.y, -10);
    }

    public void SetCameraLimit(float rightLimit, float upLimit)
    {
        cameraRightLimit = rightLimit;
        cameraUpLimit = upLimit;
    }

    void OnKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SetTimeScale(1);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SetTimeScale(2);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SetTimeScale(3);
    }

    void OnSpace(InputValue value)
    {
        Pause();
    }

    void OnNavigate(InputValue value)
    {
        navVector = value.Get<Vector2>();
        cameraTarget = null;
    }

    void OnScrollWheel(InputValue value)
    {

        if (IsPointerOverUI())
            return;

        float scroll = value.Get<Vector2>().y;

        targetZoom = Mathf.Clamp(
            targetZoom - scroll * zoomSensitivity,
            minZoom,
            maxZoom
        );
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
        else if (Vector2.Distance(Input.mousePosition, clickPos) < 30f)
        {
            SelectObject();
        }
    }

    public void SetZoom(float value)
    {
        targetZoom = Mathf.Clamp(value, minZoom, maxZoom);
    }

    public void SetTimeScale(int wantScale)
    {
        Time.timeScale = wantScale;
        GameManager.Instance.SoundManager.PitchShift(wantScale);
        currentTimeScaleText.text = $"x {wantScale}";
    }

    public void TimeScaleUp()
    {
        if (!GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart) return;
        SetTimeScale((int)Mathf.Min(Time.timeScale + 1, 3));
    }

    public void Pause()
    {
        if (Time.timeScale > 0)
        {
            lastTimeScale = (int)Time.timeScale;
            SetTimeScale(0);
        }
        else
        {
            SetTimeScale(lastTimeScale);
            lastTimeScale = 0;
        }
    }

    public void SetLeftSurvivors(int survivorsCount)
    {
        leftSurvivors.text = $"{new LocalizedString("Basic", "Remaining survivors:").GetLocalizedString()} {survivorsCount}";
    }

    public void SetPredictionUI()
    {
        exitBattleRoyale.SetActive(false);
        if (outGameUIManager.BettingAmount > 0)
        {
            predictionResult.SetActive(true);
            predictionLeft = outGameUIManager.PredictionNumber;
            int j = 0;
            for (int i=0; i < GameManager.Instance.BattleRoyaleManager.Survivors.Count; i++)
            {
                bool isPredictedOne = false;
                if (j < outGameUIManager.PredictionNumber)
                {
                    for (int k = 0; k < outGameUIManager.PredictionNumber; k++)
                    {
                        if (GameManager.Instance.BattleRoyaleManager.Survivors[i].LinkedSurvivorData.localizedSurvivorName == outGameUIManager.Predictions[k])
                        {
                            predictionResultRows[k].SetActive(true);
                            predictionResultPredictions[k].GetComponent<LocalizeStringEvent>().StringReference = GameManager.Instance.BattleRoyaleManager.Survivors[i].LinkedSurvivorData.localizedSurvivorName;
                            predictionResultPortraits[k].color = GameManager.Instance.BattleRoyaleManager.Survivors[i].GetComponent<SpriteRenderer>().color;
                            predictionResultBGs[k].color = Color.white;
                            predictionResultResults[k].text = "";
                            predictionResultRows[k].GetComponent<PredictedSurvivor>().linkedSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[i];
                            j++;
                            isPredictedOne = true;
                            break;
                        }
                    }
                }
                if(!isPredictedOne)
                {
                    otherSurvivorsResultRows[i - j].SetActive(true);
                    otherSurvivorsNames[i - j].GetComponent<LocalizeStringEvent>().StringReference = GameManager.Instance.BattleRoyaleManager.Survivors[i].LinkedSurvivorData.localizedSurvivorName;
                    otherSurvivorsPortraits[i - j].color = GameManager.Instance.BattleRoyaleManager.Survivors[i].GetComponent<SpriteRenderer>().color;
                    otherSurvivorsResultBGs[i - j].color = Color.white;
                    otherSurvivorsResults[i - j].text = "";
                    otherSurvivorsResultRows[i - j].GetComponent<PredictedSurvivor>().linkedSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[i];
                }
            }
            for(int i=outGameUIManager.PredictionNumber; i < predictionResultRows.Length; i++)
            {
                predictionResultRows[i].SetActive(false);
            }
            for(int i=outGameUIManager.contestantsData.Count - outGameUIManager.PredictionNumber; i<otherSurvivorsResultRows.Length; i++)
            {
                otherSurvivorsResultRows [i].SetActive(false);
            }
        }
        else
        {
            predictionResult.SetActive(false);
            targetRank = new();
            rankChangeDistances = new();
            curRankChangeAnimTime = 0;
            rankChangeAnimation = false;
            otherSurvivorsBox.sizeDelta = new Vector2(otherSurvivorsBox.rect.width, 30 * outGameUIManager.contestantsData.Count);
            for (int i=0; i<otherSurvivorsResultRows.Length; i++)
            {
                if (i < outGameUIManager.contestantsData.Count)
                {
                    otherSurvivorsResultRows[i].SetActive(true);
                    targetRank.Add(i);
                    otherSurvivorsResultRows[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i) * -30);
                    rankChangeDistances.Add(0);
                    otherSurvivorsNames[i].GetComponent<LocalizeStringEvent>().StringReference = GameManager.Instance.BattleRoyaleManager.Survivors[i].LinkedSurvivorData.localizedSurvivorName;
                    otherSurvivorsPortraits[i].color = GameManager.Instance.BattleRoyaleManager.Survivors[i].GetComponent<SpriteRenderer>().color;
                    otherSurvivorsResultBGs[i].color = Color.white;
                    otherSurvivorsResults[i].text = "";
                    otherSurvivorsResultRows[i].GetComponent<PredictedSurvivor>().linkedSurvivor = GameManager.Instance.BattleRoyaleManager.Survivors[i];
                }
                else otherSurvivorsResultRows[i].SetActive(false);
            }
            if (outGameUIManager.MySurvivorDataInBattleRoyale == null) exitBattleRoyale.SetActive(true);
        }
    }

    public void SetSurvivorRank(LocalizedString survivorName, int survivorRank)
    {
        bool isPredictedOne = false;
        int predictionNumber = outGameUIManager.BettingAmount == 0 ? 0 : outGameUIManager.PredictionNumber;
        for (int i = 0; i < predictionNumber; i++)
        {
            if (outGameUIManager.Predictions[i] == null) break;
            if (outGameUIManager.Predictions[i].TableEntryReference.Key == survivorName.TableEntryReference.Key)
            {
                isPredictedOne = true;
                predictionLeft--;
                predictionResultResults[i].text = survivorRank switch
                {
                    0 => "1st",
                    1 => "2nd",
                    2 => "3rd",
                    _ => $"{survivorRank + 1}th",
                };
                if (i == survivorRank) predictionResultBGs[i].color = new Color(0.48f, 1f, 0.44f);
                else if (survivorRank < outGameUIManager.PredictionNumber) predictionResultBGs[i].color = new Color(0.89f, 0.93f, 0.39f);
                else predictionResultBGs[i].color = new Color(0.88f, 0.43f, 0.43f);

                break;
            }
        }
        if(!isPredictedOne)
        {
            for(int i = 0; i < outGameUIManager.contestantsData.Count - predictionNumber; i++)
            {
                if (otherSurvivorsNames[i].GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference.Key == survivorName.TableEntryReference.Key)
                {
                    otherSurvivorsResults[i].text = survivorRank switch
                    {
                        0 => "1st",
                        1 => "2nd",
                        2 => "3rd",
                        _ => $"{survivorRank + 1}th",
                    };
                    if(survivorRank > 0) otherSurvivorsResultBGs[i].color = new Color(0.88f, 0.43f, 0.43f);
                    //for(int k=0; k<targetRank.Count; k++) Debug.Log($"Bef targetRank[{k}] = {targetRank[k]}");
                    targetRank[i] = survivorRank;
                    for(int j=i+1; j < outGameUIManager.contestantsData.Count - predictionNumber; j++)
                    {
                        if (targetRank[j] <= survivorRank)targetRank[j]--;
                    }
                    //for(int k=0; k<targetRank.Count; k++) Debug.Log($"Aft targetRank[{k}] = {targetRank[k]}");
                    RankChangeAnimation();
                    break;
                }
            }
        }
        if (predictionLeft == 0 && (outGameUIManager.MySurvivorDataInBattleRoyale == null || GameManager.Instance.BattleRoyaleManager.Survivors[0].IsDead && !outGameUIManager.Championship)) exitBattleRoyale.SetActive(true);
    }

    void RankChangeAnimation()
    {
        for(int i = 0; i<rankChangeDistances.Count; i++)
        {
            rankChangeDistances[i] = (targetRank[i] * -30 - otherSurvivorsResultRows[i].GetComponent<RectTransform>().anchoredPosition.y);
        }
        curRankChangeAnimTime = 0;
        rankChangeAnimation = true;
    }

    bool otherSurvivorsFolded;
    public void FoldOtherSurvivors()
    {
        int predictionNumber = outGameUIManager.BettingAmount == 0 ? 0 : outGameUIManager.PredictionNumber;
        if(otherSurvivorsFolded)
        {
            for (int i = 0; i < outGameUIManager.contestantsData.Count - predictionNumber; i++)
            {
                otherSurvivorsResultRows[i].SetActive(true);
            }
            foldOtherSurvivors.GetComponentInChildren<TextMeshProUGUI>().text = "▲";
        }
        else
        {
            for(int i=0; i<otherSurvivorsResultRows.Length; i++)
            {
                otherSurvivorsResultRows[i].SetActive(false);
            }
            foldOtherSurvivors.GetComponentInChildren<TextMeshProUGUI>().text = "▼";
        }
        otherSurvivorsFolded = !otherSurvivorsFolded;
    }

    public void ShowKillLog(string victim, string cause)
    {
        TextMeshProUGUI killLog = PoolManager.Spawn(ResourceEnum.Prefab.KillLog, display).GetComponentInChildren<TextMeshProUGUI>();
        LocalizedString lsMessage = new("Basic", "Kill Message")
        {
            Arguments = new[] { victim, cause }
        };
        string message = lsMessage.GetLocalizedString();
        killLog.text = message;
        log.text += "\n" + message;
        GameManager.Instance.FixLayout(log.GetComponent<RectTransform>());
        logScrollView.verticalNormalizedPosition = 0;
    }

    public void AddLog(string message)
    {
        log.text += "\n" + message;
        GameManager.Instance.FixLayout(log.GetComponent<RectTransform>());
        logScrollView.verticalNormalizedPosition = 0;
    }

    public void ClearLog()
    {
        log.text = "";
        strengthText.GetComponent<Help>().SetDescription("");
        agilityText.GetComponent<Help>().SetDescription("");
        fightingText.GetComponent<Help>().SetDescription("");
        knowledgeText.GetComponent<Help>().SetDescription("");
        shootingText.GetComponent<Help>().SetDescription("");
        craftingText.GetComponent<Help>().SetDescription("");
    }

    void SelectObject()
    {
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);

        bool selectedNotNull = false;
        foreach (Collider2D hit in hits)
        {
            if (!hit.isTrigger)
            {
                if (hit.TryGetComponent(out CustomObject clickedObject))
                {
                    if (clickedObject is Survivor || clickedObject is Box)
                    {
                        selectedObject = clickedObject;
                        selectedObjectTab.SetActive(clickedObject is Survivor);
                        if (clickedObject is Survivor)
                        {
                            Survivor survivor = clickedObject as Survivor;
                            cameraTarget = selectedObject.transform;
                        }
                        else CurrentTab = 1;
                    }
                    //else if (clickedObject is Trap)
                    //{
                    //    selectedObjectTab.SetActive(false);
                    //}
                    else return;
                    SetSelectedObjectInfoOnce();
                    selectedNotNull = true;
                    break;
                }
            }
            else
            {
                if (hit.TryGetComponent(out Survivor survivor) && survivor.IsDead)
                {
                    selectedObject = survivor;
                    SetSelectedObjectInfoOnce();
                    selectedNotNull = true;
                    break;
                }
            }
        }
        if (!selectedNotNull) selectedObject = null;
    }

    public void SelectObject(Survivor survivor)
    {
        selectedObject = survivor;
        selectedObjectTab.SetActive(true);
        cameraTarget = selectedObject.transform;
        Camera.main.transform.position = new(selectedObject.transform.position.x, selectedObject.transform.position.y, -10);
        SetSelectedObjectInfoOnce();
    }

    void SetSelectedObjectInfoOnce()
    {
        if (selectedObject == null) return;
        if (selectedObject is Survivor)
        {
            Survivor selectedSurvivor = selectedObject as Survivor;
            selectedObjectImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Survivor);
            Vector3 colorVector = BattleRoyaleManager.colorInfo[selectedSurvivor.survivorID];
            selectedObjectImage.color = new(colorVector.x, colorVector.y, colorVector.z);
            selectedObjectName.text = selectedSurvivor.survivorName.GetLocalizedString();

            strengthBar.fillAmount = selectedSurvivor.CorrectedStrength / 100f;
            agilityBar.fillAmount = selectedSurvivor.CorrectedAgility / 100f;
            fightingBar.fillAmount = selectedSurvivor.CorrectedFighting / 100f;
            shootingBar.fillAmount = selectedSurvivor.CorrectedShooting / 100f;
            craftingBar.fillAmount = selectedSurvivor.CorrectedCrafting / 100f;
            knowledgeBar.fillAmount = selectedSurvivor.LinkedSurvivorData._knowledge / 100f;

            strengthText.text = selectedSurvivor.CorrectedStrength.ToString();
            agilityText.text = selectedSurvivor.CorrectedAgility.ToString();
            fightingText.text = selectedSurvivor.CorrectedFighting.ToString();
            shootingText.text = selectedSurvivor.CorrectedShooting.ToString();
            craftingText.text = selectedSurvivor.CorrectedCrafting.ToString();
            knowledgeText.text = selectedSurvivor.LinkedSurvivorData.Knowledge.ToString();

            characteristics.ArrangeCharacteristics(selectedSurvivor.LinkedSurvivorData);

        }
        else if (selectedObject is Box)
        {
            //Box selectedBox = selectedObject as Box;
            selectedObjectImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Box);
            selectedObjectImage.color = Color.white;
#if UNITY_EDITOR
            selectedObjectName.text = selectedObject.transform.position.ToString();
#else
            selectedObjectName.text = new LocalizedString("Basic", "Box").GetLocalizedString();
#endif
            selectedSurvivorsHealthBar.SetActive(false);
            selectedSurvivorBleedingBar.SetActive(false);
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
            selectedObjectCurrentStatus.text = "";
        }
        else
        {
            string name = selectedObject.name.Split('(')[0];
            if (Enum.TryParse(name, out ResourceEnum.Sprite sprite)) selectedObjectImage.sprite = ResourceManager.Get(sprite);
            selectedObjectImage.color = Color.white;
            selectedObjectName.text = name;
            selectedSurvivorsHealthBar.SetActive(false);
            selectedSurvivorBleedingBar.SetActive(false);
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
        }
        UpdatableSelectedObjectInfo(selectedObject);
    }

    void UpdatableSelectedObjectInfo(CustomObject selectedObject)
    {
        if (selectedObject is Survivor)
        {
            //UpdateSelectedObjectStat(selectedObject as Survivor);
            UpdateSelectedObjectInjury(selectedObject as Survivor);
            UpdateSelectedObjectKillCount(selectedObject as Survivor);
            UpdateSelectedObjectStatus(selectedObject as Survivor);
        }
        UpdateSelectedObjectInventory(selectedObject);
    }

    public void UpdateSelectedObjectStat(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        strengthText.GetComponent<Help>().SetDescription("");
        agilityText.GetComponent<Help>().SetDescription("");
        fightingText.GetComponent<Help>().SetDescription("");
        knowledgeText.GetComponent<Help>().SetDescription("");
        shootingText.GetComponent<Help>().SetDescription("");
        craftingText.GetComponent<Help>().SetDescription("");
    }

    Image GetTargetImage(InjurySite site)
    {
        Image targetPart = null;
        targetPart = site switch
        {
            InjurySite.None => throw new NotImplementedException(),
            InjurySite.Head => head,
            InjurySite.RightEye => rightEye,
            InjurySite.LeftEye => leftEye,
            InjurySite.RightEar => rightEar,
            InjurySite.LeftEar => leftEar,
            InjurySite.Cheek => cheek,
            InjurySite.Nose => nose,
            InjurySite.Jaw => jaw,
            InjurySite.Skull => head,
            InjurySite.Brain => head,
            InjurySite.Neck => neck,
            InjurySite.Chest => chest,
            InjurySite.Ribs => ribs,
            InjurySite.Abdomen => abdomen,
            InjurySite.Organ => organs,
            InjurySite.RightArm => rightArm,
            InjurySite.LeftArm => leftArm,
            InjurySite.RightHand => rightHand,
            InjurySite.LeftHand => leftHand,
            InjurySite.RightThumb => rightThumb,
            InjurySite.RightIndexFinger => rightIndexFinger,
            InjurySite.RightMiddleFinger => rightMiddleFinger,
            InjurySite.RightRingFinger => rightRingFinger,
            InjurySite.RightLittleFinger => rightLittleFinger,
            InjurySite.LeftThumb => leftThumb,
            InjurySite.LeftIndexFinger => leftIndexFinger,
            InjurySite.LeftMiddleFinger => leftMiddleFinger,
            InjurySite.LeftRingFinger => leftRingFinger,
            InjurySite.LeftLittleFinger => leftLittleFinger,
            InjurySite.RightLeg => rightLeg,
            InjurySite.LeftLeg => leftLeg,
            InjurySite.RightKnee => rightKnee,
            InjurySite.LeftKnee => leftKnee,
            InjurySite.RightFoot => rightFoot,
            InjurySite.LeftFoot => leftFoot,
            InjurySite.RightBigToe => rightBigToe,
            InjurySite.LeftBigToe => leftBigToe,
            InjurySite.RightIndexToe => rightIndexToe,
            InjurySite.LeftIndexToe => leftIndexToe,
            InjurySite.RightMiddleToe => rightMiddleToe,
            InjurySite.LeftMiddleToe => leftMiddleToe,
            InjurySite.RightRingToe => rightRingToe,
            InjurySite.LeftRingToe => leftRingToe,
            InjurySite.RightLittleToe => rightLittleToe,
            InjurySite.LeftLittleToe => leftLittleToe,
            _ => throw new NotImplementedException()
        };
        return targetPart;
    }

    public void UpdateSelectedObjectInjury(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        ResetInjuryInfo();

        foreach (var injury in survivor.injuries)
        {
            Image targetPart = GetTargetImage(injury.site);
            if (injury.degree == 1)
            {
                Color wantColor = new(0.3f, 0.3f, 0.3f);
                targetPart.color = wantColor;
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>().SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
                }
            }
            else if (injury.type == InjuryType.ArtificialPartsTransplanted || injury.type == InjuryType.ArtificialPartsDamaged)
            {
                Color wantColor = new(0.6f, 0.6f, 0.6f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.ArtificialPartsTransplanted;
                    if (subpartInjury == null) subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
                }
            }
            else if (injury.type == InjuryType.AugmentedPartsTransplanted || injury.type == InjuryType.AugmentedPartsDamaged)
            {
                Color wantColor = new(1f, 0f, 1f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.AugmentedPartsTransplanted;
                    if (subpartInjury == null) subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
                }
            }
            else if (injury.type == InjuryType.TranscendantPartsTransplanted || injury.type == InjuryType.TranscendantPartsDamaged)
            {
                Color wantColor = new(0f, 1f, 1f);
                targetPart.color = wantColor * (1 - injury.degree * 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    Injury subpartInjury = survivor.injuries.Find(x => x.site == subpart);
                    float subpartInjuryDegree = subpartInjury != null ? subpartInjury.degree : 0;
                    InjuryType subpartInjuryType = subpartInjury != null ? subpartInjury.type : InjuryType.TranscendantPartsTransplanted;
                    if (subpartInjury == null) subpartImage.color = wantColor;
                    subpartImage.GetComponentInChildren<Help>()
                        .SetDescription($"{new LocalizedString("Injury", subpart.ToString()).GetLocalizedString()} {new LocalizedString("Injury", subpartInjuryType.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {subpartInjuryDegree:0.##}");
                }
            }
            else
            {
                targetPart.color = new Color(1f, (1 - injury.degree) * 0.7f, (1 - injury.degree) * 0.7f);
            }
            targetPart.GetComponentInChildren<Help>().SetDescription($"{new LocalizedString("Injury", injury.site.ToString()).GetLocalizedString()} {new LocalizedString("Injury", injury.type.ToString()).GetLocalizedString()}\n{new LocalizedString("Injury", "Degree").GetLocalizedString()} : {injury.degree:0.##}");
        }
    }

    void ResetInjuryInfo()
    {
        head.color = Color.white;
        rightEye.color = Color.white;
        leftEye.color = Color.white;
        rightEar.color = Color.white;
        leftEar.color = Color.white;
        cheek.color = Color.white;
        nose.color = Color.white;
        jaw.color = Color.white;
        neck.color = Color.white;
        chest.color = Color.white;
        ribs.color = Color.white;
        abdomen.color = Color.white;
        organs.color = Color.white;
        rightArm.color = Color.white;
        leftArm.color = Color.white;
        rightHand.color = Color.white;
        leftHand.color = Color.white;
        rightThumb.color = Color.white;
        leftThumb.color = Color.white;
        rightIndexFinger.color = Color.white;
        leftIndexFinger.color = Color.white;
        rightMiddleFinger.color = Color.white;
        leftMiddleFinger.color = Color.white;
        rightRingFinger.color = Color.white;
        leftRingFinger.color = Color.white;
        rightLittleFinger.color = Color.white;
        leftLittleFinger.color = Color.white;
        rightLeg.color = Color.white;
        leftLeg.color = Color.white;
        rightKnee.color = Color.white;
        leftKnee.color = Color.white;
        rightFoot.color = Color.white;
        leftFoot.color = Color.white;
        rightBigToe.color = Color.white;
        leftBigToe.color = Color.white;
        rightIndexToe.color = Color.white;
        leftIndexToe.color = Color.white;
        rightMiddleToe.color = Color.white;
        leftMiddleToe.color = Color.white;
        rightRingToe.color = Color.white;
        leftRingToe.color = Color.white;
        rightLittleToe.color = Color.white;
        leftLittleToe.color = Color.white;

        head.GetComponentInChildren<Help>().SetDescription("");
        rightEye.GetComponentInChildren<Help>().SetDescription("");
        leftEye.GetComponentInChildren<Help>().SetDescription("");
        rightEar.GetComponentInChildren<Help>().SetDescription("");
        leftEar.GetComponentInChildren<Help>().SetDescription("");
        cheek.GetComponentInChildren<Help>().SetDescription("");
        nose.GetComponentInChildren<Help>().SetDescription("");
        jaw.GetComponentInChildren<Help>().SetDescription("");
        chest.GetComponentInChildren<Help>().SetDescription("");
        ribs.GetComponentInChildren<Help>().SetDescription("");
        abdomen.GetComponentInChildren<Help>().SetDescription("");
        organs.GetComponentInChildren<Help>().SetDescription("");
        rightArm.GetComponentInChildren<Help>().SetDescription("");
        leftArm.GetComponentInChildren<Help>().SetDescription("");
        rightHand.GetComponentInChildren<Help>().SetDescription("");
        leftHand.GetComponentInChildren<Help>().SetDescription("");
        rightThumb.GetComponentInChildren<Help>().SetDescription("");
        leftThumb.GetComponentInChildren<Help>().SetDescription("");
        rightIndexFinger.GetComponentInChildren<Help>().SetDescription("");
        leftIndexFinger.GetComponentInChildren<Help>().SetDescription("");
        rightMiddleFinger.GetComponentInChildren<Help>().SetDescription("");
        leftMiddleFinger.GetComponentInChildren<Help>().SetDescription("");
        rightRingFinger.GetComponentInChildren<Help>().SetDescription("");
        leftRingFinger.GetComponentInChildren<Help>().SetDescription("");
        rightLittleFinger.GetComponentInChildren<Help>().SetDescription("");
        leftLittleFinger.GetComponentInChildren<Help>().SetDescription("");
        rightLeg.GetComponentInChildren<Help>().SetDescription("");
        leftLeg.GetComponentInChildren<Help>().SetDescription("");
        rightKnee.GetComponentInChildren<Help>().SetDescription("");
        leftKnee.GetComponentInChildren<Help>().SetDescription("");
        rightFoot.GetComponentInChildren<Help>().SetDescription("");
        leftFoot.GetComponentInChildren<Help>().SetDescription("");
        rightBigToe.GetComponentInChildren<Help>().SetDescription("");
        leftBigToe.GetComponentInChildren<Help>().SetDescription("");
        rightIndexToe.GetComponentInChildren<Help>().SetDescription("");
        leftIndexToe.GetComponentInChildren<Help>().SetDescription("");
        rightMiddleToe.GetComponentInChildren<Help>().SetDescription("");
        leftMiddleToe.GetComponentInChildren<Help>().SetDescription("");
        rightRingToe.GetComponentInChildren<Help>().SetDescription("");
        leftRingToe.GetComponentInChildren<Help>().SetDescription("");
        rightLittleToe.GetComponentInChildren<Help>().SetDescription("");
        leftLittleToe.GetComponentInChildren<Help>().SetDescription("");
    }

    public void UpdateSelectedObjectKillCount(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        selectedObjectKillCount.text = survivor.KillCount.ToString();
    }

    public void UpdateSelectedObjectStatus(Survivor survivor)
    {
        if (survivor != selectedObject) return;
        if(survivor.IsDead)
        {
            selectedObjectCurrentStatus.text = "";
        }
        else
        {
            selectedObjectCurrentStatus.text = survivor.CurrentStatus switch
            {
                Survivor.Status.Farming or Survivor.Status.FarmingBox => new LocalizedString("Basic", "Looting").GetLocalizedString(),
                Survivor.Status.InCombat => new LocalizedString("Basic", "In Battle").GetLocalizedString(),
                Survivor.Status.InvestigateThreateningSound => new LocalizedString("Basic", "Investigating noise").GetLocalizedString(),
                Survivor.Status.Maintain => new LocalizedString("Basic", "Preparing for combat").GetLocalizedString(),
                Survivor.Status.Trapping => new LocalizedString("Basic", "Setting trap").GetLocalizedString(),
                Survivor.Status.TraceEnemy => new LocalizedString("Basic", "Chasing enemy").GetLocalizedString(),
                Survivor.Status.RunAway => new LocalizedString("Basic", "Fleeing").GetLocalizedString(),
                Survivor.Status.TrapDisarming => new LocalizedString("Basic", "Disarming trap").GetLocalizedString(),
                Survivor.Status.Crafting => new LocalizedString("Basic", "Crafting:") { Arguments = new[] { new LocalizedString("Item", survivor.CurrentCrafting.itemType.ToString()).GetLocalizedString() } }.GetLocalizedString(),
                Survivor.Status.Wearing => survivor.CurrentWearingHelmet != null ? new LocalizedString("Basic", "Wearing:") { Arguments = new[] { new LocalizedString("Item", survivor.CurrentWearingHelmet.itemType.ToString()).GetLocalizedString() } }.GetLocalizedString() : new LocalizedString("Basic", "Wearing:") { Arguments = new[] { new LocalizedString("Item", survivor.CurrentWearingVest.itemType.ToString()).GetLocalizedString() } }.GetLocalizedString(),
                Survivor.Status.Reparing => survivor.CurrentRepairing == 1 ? new LocalizedString("Basic", "Repairing:") { Arguments = new[] { new LocalizedString("Item", survivor.CurrentHelmet.itemType.ToString()).GetLocalizedString() } }.GetLocalizedString() : new LocalizedString("Basic", "Repairing:") { Arguments = new[] { new LocalizedString("Item", survivor.CurrentVest.itemType.ToString()).GetLocalizedString() } }.GetLocalizedString(),
                Survivor.Status.Enchanting => new LocalizedString("Basic", "Enchanting").GetLocalizedString(),
                Survivor.Status.FindingEnemy => new LocalizedString("Basic", "FindingEnemy").GetLocalizedString(),
                _ => survivor.CurrentStatus.ToString()
            };
        }
        GameManager.Instance.FixLayout(selectedObjectCurrentStatus.GetComponent<RectTransform>());
    }

    public void UpdateSelectedObjectInventory()
    {
        if(selectedObject != null) UpdatableSelectedObjectInfo(selectedObject);
    }

    public void UpdateSelectedObjectInventory(CustomObject selected)
    {
        if (selected != selectedObject) return;
        if (selected is Survivor)
        {
            Survivor selectedSurvivor = selected as Survivor;
            if (selectedSurvivor.CurrentWeapon != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentWeapon.itemType}", out var weaponSpriteEnum))
            {
                selectedObjectsCurrentWeaponImage.sprite = ResourceManager.Get(weaponSpriteEnum);
                selectedObjectsCurrentWeaponImage.GetComponent<AspectRatioFitter>().aspectRatio
                    = selectedObjectsCurrentWeaponImage.sprite.rect.width / selectedObjectsCurrentWeaponImage.sprite.rect.height;
                selectedObjectsCurrentWeaponOutline.sprite = craftingQualityOutlines[(int)selectedSurvivor.CurrentWeapon.quality];
                selectedObjectsCurrentWeapon.GetComponent<Help>().SetDescription(selectedSurvivor.CurrentWeapon.itemType, selectedSurvivor.CurrentWeapon.quality);
            }
            else
            {
                selectedObjectsCurrentWeaponImage.sprite = null;
                selectedObjectsCurrentWeaponOutline.sprite = craftingQualityOutlines[0];
                selectedObjectsCurrentWeapon.GetComponent<Help>().SetDescription("");
            }
            if (selectedSurvivor.IsValid(selectedSurvivor.CurrentWeapon))
            {
                if (selectedSurvivor.CurrentWeapon is RangedWeapon rangeWeapon && rangeWeapon.NeedPreload)
                {
                    //int validBulletAmount = selectedSurvivor.ValidBullet != null ? selectedSurvivor.ValidBullet.amount : 0;
                    selectedObjectsCurrentWeaponText.text = $"{selectedSurvivor.CurrentWeapon.itemName.GetLocalizedString()} ({selectedSurvivor.CurrentWeaponAsRangedWeapon.CurrentMagazine} / {selectedSurvivor.CurrentWeaponAsRangedWeapon.MagazineCapacity})";
                }
                //else if(selectedSurvivor.CurrentWeapon is MeleeWeapon meleeWeapon)
                //{
                //    if (meleeWeapon.IsEnchanted) selectedObjectsCurrentWeaponText.text = $"{selectedSurvivor.CurrentWeapon.itemName}(Poison enchanted)";
                //    else selectedObjectsCurrentWeaponText.text = selectedSurvivor.CurrentWeapon.itemName.GetLocalizedString();
                //}
                else
                {
                    selectedObjectsCurrentWeaponText.text = selectedSurvivor.CurrentWeapon.itemName.GetLocalizedString();
                }
            }
            else
            {
                selectedObjectsCurrentWeaponText.text = new LocalizedString("Basic", "None").GetLocalizedString();
            }

            if (selectedSurvivor.CurrentHelmet != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentHelmet.itemType}", out var helmetSpriteEnum))
            {
                selectedObjectsCurrentHelmetImage.sprite = ResourceManager.Get(helmetSpriteEnum);
                selectedObjectsCurrentHelmetOutline.sprite = craftingQualityOutlines[(int)selectedSurvivor.CurrentHelmet.quality];
                selectedObjectsCurrentHelmet.GetComponent<Help>().SetDescription(selectedSurvivor.CurrentHelmet.itemType, selectedSurvivor.CurrentHelmet.quality);
            }
            else
            {
                selectedObjectsCurrentHelmetImage.sprite = null;
                selectedObjectsCurrentHelmetOutline.sprite = craftingQualityOutlines[0];
                selectedObjectsCurrentHelmet.GetComponent<Help>().SetDescription("");
            }
            selectedObjectsCurrentHelmetText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentHelmet) ? selectedSurvivor.CurrentHelmet.itemName.GetLocalizedString() : new LocalizedString("Basic", "None").GetLocalizedString();

            if (selectedSurvivor.CurrentVest != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentVest.itemType}", out var vestSpriteEnum))
            {
                selectedObjectsCurrentVestImage.sprite = ResourceManager.Get(vestSpriteEnum);
                selectedObjectsCurrentVestOutline.sprite = craftingQualityOutlines[(int)selectedSurvivor.CurrentVest.quality];
                selectedObjectsCurrentVest.GetComponent<Help>().SetDescription(selectedSurvivor.CurrentVest.itemType, selectedSurvivor.CurrentVest.quality);
            }
            else
            {
                selectedObjectsCurrentVestImage.sprite = null;
                selectedObjectsCurrentVestOutline.sprite = craftingQualityOutlines[0];
                selectedObjectsCurrentVest.GetComponent<Help>().SetDescription("");
            } 
            selectedObjectsCurrentVestText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentVest) ? selectedSurvivor.CurrentVest.itemName.GetLocalizedString() : new LocalizedString("Basic", "None").GetLocalizedString();

            selectedObjectsCurrentWeapon.SetActive(true);
            selectedObjectsCurrentHelmet.SetActive(true);
            selectedObjectsCurrentVest.SetActive(true);
            List<Item> selectedSurvivorsInventory = selectedSurvivor.Inventory.ToList();
            // 아이템 타입 순
            if (selectedObjectInventorySortDropdown.value == 1) selectedSurvivorsInventory.Sort((x, y) => CompareItemType(x.itemType, y.itemType));
            for (int i = 0; i < selectedObjectsItems.Length; i++)
            {
                if (selectedSurvivorsInventory.Count > i)
                {
                    if (Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivorsInventory[i].itemType}", out var spriteEnum))
                    {
                        Image itemImage = selectedObjectsItems[i].GetComponentsInChildren<Image>()[^1];
                        itemImage.sprite = ResourceManager.Get(spriteEnum);
                        selectedObjectsItems[i].GetComponentInChildren<AspectRatioFitter>().aspectRatio
                            = itemImage.sprite.rect.width / itemImage.sprite.rect.height;
                        selectedObjectsItems[i].GetComponentsInChildren<Image>()[0].sprite = selectedSurvivorsInventory[i].quality == CraftingQuality.NotCrafted ? null : craftingQualityOutlines[(int)(selectedSurvivorsInventory[i].quality)];
                        
                        Image outline = selectedObjectsItems[i].GetComponentsInChildren<Image>()[0];
                        Color color = outline.color;
                        color.a = selectedSurvivorsInventory[i].quality == CraftingQuality.NotCrafted ? 0f : 1f;
                        outline.color = color;
                    }
                    else
                    {
                        selectedObjectsItems[i].GetComponentsInChildren<Image>()[^1].sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
                    }
                    selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedSurvivorsInventory[i].itemName.GetLocalizedString()} x {selectedSurvivorsInventory[i].amount}";
                    selectedObjectsItems[i].GetComponent<Help>().SetDescription(selectedSurvivorsInventory[i].itemType, selectedSurvivorsInventory[i].quality);
                    selectedObjectsItems[i].SetActive(true);
                }
                else
                {
                    selectedObjectsItems[i].SetActive(false);
                }
            }
        }
        else if (selected is Box)
        {
            Box selectedBox = selected as Box;
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
            selectedObjectKillCount.text = "-";
            for (int i = 0; i < selectedObjectsItems.Length; i++)
            {
                if (selectedBox.items.Count > i)
                {
                    if (selectedBox.items[i] == null) break;
                    if (Enum.TryParse<ResourceEnum.Sprite>($"{selectedBox.items[i].itemType}", out var spriteEnum))
                    {
                        selectedObjectsItems[i].GetComponentsInChildren<Image>()[^1].sprite = ResourceManager.Get(spriteEnum);
                    }
                    else
                    {
                        selectedObjectsItems[i].GetComponentsInChildren<Image>()[^1].sprite = ResourceManager.Get(ResourceEnum.Sprite.Unknown);
                    }
                    selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedBox.items[i].itemName.GetLocalizedString()} x {selectedBox.items[i].amount}";
                    Image outline = selectedObjectsItems[i].GetComponentsInChildren<Image>()[0];

                    Color color = outline.color;
                    color.a = selectedBox.items[i].quality == CraftingQuality.NotCrafted ? 0f : 1f;
                    outline.color = color;
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
        if (selectedObject == null)
        {
            seletedImage.SetActive(false);
            selectedObjectInfo.SetActive(false);
            simulationTip.SetActive(true);
        }
        else
        {
            seletedImage.transform.position = selectedObject.transform.position;
            seletedImage.SetActive(true);
            selectedObjectInfo.SetActive(true);
            simulationTip.SetActive(false);
            if (selectedObject is Survivor)
            {
                Survivor selectedSurvivor = selectedObject as Survivor;
                autoFocus.SetActive(true);
                selectedSurvivorsHealthBar.SetActive(true);
                selectedSurvivorBleedingBar.SetActive(true);

                selectedSurvivorsHealthBarImage.fillAmount = selectedSurvivor.CurHP / selectedSurvivor.MaxHP;
                selectedSurvivorsHealthText.text = $"{selectedSurvivor.CurHP:0} / {selectedSurvivor.MaxHP:0}";

                selectedSurvivorBleedingBarImage.fillAmount = (selectedSurvivor.maxBlood - selectedSurvivor.curBlood) / (selectedSurvivor.maxBlood * 0.5f);

                bleedingAnim.SetBool("Bleeding", selectedSurvivor.BleedingAmount > 0);

                if(selectedSurvivor.IsValid(selectedSurvivor.CurrentHelmet))
                {
                    selectedObjectsCurrentHelmetDurability.SetActive(true);
                    selectedObjectsCurrentHelmetDurability.GetComponentsInChildren<Image>()[1].fillAmount = selectedSurvivor.CurrentHelmet.DurabilityPercent;
                    selectedObjectsCurrentHelmetDurability.GetComponent<Help>().SetDescriptionWithKey("Durability", $"{selectedSurvivor.CurrentHelmet.DurabilityPercent * 100:0}", $"{Math.Min(selectedSurvivor.CurrentHelmet.DurabilityPercent * 100 + 50, 100):0}");
                }
                else
                {
                    selectedObjectsCurrentHelmetDurability.SetActive(false);
                }

                if (selectedSurvivor.IsValid(selectedSurvivor.CurrentVest))
                {
                    selectedObjectsCurrentVestDurability.SetActive(true);
                    selectedObjectsCurrentVestDurability.GetComponentsInChildren<Image>()[1].fillAmount = selectedSurvivor.CurrentVest.DurabilityPercent;
                    selectedObjectsCurrentVestDurability.GetComponent<Help>().SetDescriptionWithKey("Durability", $"{selectedSurvivor.CurrentVest.DurabilityPercent * 100:0}", $"{Math.Min(selectedSurvivor.CurrentVest.DurabilityPercent * 100 + 50, 100):0}");
                }
                else
                {
                    selectedObjectsCurrentVestDurability.SetActive(false);
                }
            }
        }
    }

    public void ExitBattleRoyale()
    {
        if (GameManager.Instance.BattleRoyaleManager == null || !GameManager.Instance.BattleRoyaleManager.isBattleRoyaleStart) return;
        SetTimeScale(0);
        outGameUIManager.OpenConfirmWindow("Confirm:Exit Battle Royale", () =>
        {
            // GameManager.Instance.GetComponent<GameResult>().ShowGameResult();
            GameManager.Instance.GetComponent<GameResult>().ExitBattle();
        });
    }

    int CompareItemType(ItemManager.Items x, ItemManager.Items y)
    {
        return (int)x < (int)y ? -1 : 1;
    }

    void OnLocaleChanged(Locale newLocale)
    {
        if (GameManager.Instance.BattleRoyaleManager == null) return;
        leftSurvivors.text = $"{new LocalizedString("Basic", "Remaining survivors:").GetLocalizedString()} {GameManager.Instance.BattleRoyaleManager.AliveSurvivors.Count}";
        selectedObjectInventorySortDropdown.options[0].text = new LocalizedString("Basic", "Acquired").GetLocalizedString();
        selectedObjectInventorySortDropdown.options[1].text = new LocalizedString("Basic", "Item Type").GetLocalizedString();
        if (selectedObject != null) SetSelectedObjectInfoOnce();
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

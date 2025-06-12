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
    [SerializeField] Image knowledgeBar;
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI agilityText;
    [SerializeField] TextMeshProUGUI fightingText;
    [SerializeField] TextMeshProUGUI shootingText;
    [SerializeField] TextMeshProUGUI knowledgeText;
    [SerializeField] AutoNewLineLayoutGroup characteristics;

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
        if(autoFocus.GetComponent<Toggle>().isOn && cameraTarget != null) Camera.main.transform.position = new(cameraTarget.transform.position.x, cameraTarget.transform.position.y, -10);
    }

    void ManualCameraMove()
    {
        Vector3 cameraPos;
        cameraPos = Camera.main.transform.position + (Vector3)navVector * Camera.main.orthographicSize * 0.1f;
        if (!IsPointerOverUI() && isClicked)
        {
            cameraPos = cameraPosBeforeClick + ((Vector3)clickPos - Input.mousePosition) * 0.02f * 0.2f * Camera.main.orthographicSize;
            cameraTarget = null;
        }
        Camera.main.transform.position = new(Mathf.Clamp(cameraPos.x, cameraLeftLimit, cameraRightLimit), Mathf.Clamp(cameraPos.y, cameraDownLimit, cameraUpLimit), -10);
    }

    public void SetCameraLimit(float rightLimit, float upLimit)
    {
        cameraRightLimit = rightLimit;
        cameraUpLimit = upLimit;
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
        GameManager.Instance.SoundManager.PitchShift(wantScale);
        currentTimeScaleText.text = $"x {wantScale}";
    }

    public void TimeScaleUp()
    {
        Time.timeScale = Mathf.Min(Time.timeScale + 1, Time.timeScale, 3);
        currentTimeScaleText.text = $"x {(int)Time.timeScale}";
    }

    public void Pause()
    {
        if(Time.timeScale > 0)
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
        leftSurvivors.text = $"Left Survivors : {survivorsCount}";
    }

    public void SetPredictionUI()
    {
        if(outGameUIManager.BettingAmount > 0)
        {
            predictionResult.SetActive(true);
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
        else predictionResult.SetActive(false);
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
                    selectedObject = clickedObject;
                    if(clickedObject is Survivor || clickedObject is Box)
                    {
                        selectedObjectTab.SetActive(clickedObject is Survivor);
                        if (clickedObject is Survivor)
                        {
                            Survivor survivor = clickedObject as Survivor;
                            cameraTarget = selectedObject.transform;
                        }
                        else CurrentTab = 1;
                    }
                    else
                    {
                        selectedObjectTab.SetActive(false);
                    }
                    SetSelectedObjectInfoOnce();
                    selectedNotNull = true;
                    break;
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

            strengthBar.fillAmount = selectedSurvivor.CorrectedStrength / 100f;
            agilityBar.fillAmount = selectedSurvivor.CorrectedAgility / 100f;
            fightingBar.fillAmount = selectedSurvivor.CorrectedFighting / 100f;
            shootingBar.fillAmount = selectedSurvivor.CorrectedShooting / 100f;
            knowledgeBar.fillAmount = selectedSurvivor.CorrectedKnowledge / 100f;

            strengthText.text = selectedSurvivor.CorrectedStrength.ToString();
            agilityText.text = selectedSurvivor.CorrectedAgility.ToString();
            fightingText.text = selectedSurvivor.CorrectedFighting.ToString();
            shootingText.text = selectedSurvivor.CorrectedShooting.ToString();
            knowledgeText.text = selectedSurvivor.CorrectedKnowledge.ToString();

            characteristics.ArrangeCharacteristics(selectedSurvivor.LinkedSurvivorData);

        }
        else if(selectedObject is Box)
        {
            //Box selectedBox = selectedObject as Box;
            selectedObjectImage.sprite = ResourceManager.Get(ResourceEnum.Sprite.Box);
            selectedObjectImage.color = Color.white;
            selectedObjectName.text = "Box";
            selectedSurvivorsHealthBar.SetActive(false);
            selectedSurvivorBleedingBar.SetActive(false);
            selectedObjectsCurrentWeapon.SetActive(false);
            selectedObjectsCurrentHelmet.SetActive(false);
            selectedObjectsCurrentVest.SetActive(false);
        }
        else
        {
            string name = selectedObject.name.Split('(')[0];
            if(Enum.TryParse(name, out ResourceEnum.Sprite sprite)) selectedObjectImage.sprite = ResourceManager.Get(sprite);
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
        if(selectedObject is Survivor)
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
            InjurySite.RightIndexFinger => leftThumb,
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
        //for (int i = 0; i < injuries.Length; i++)
        //{
        //    if (survivor.injuries.Count > i)
        //    {
        //        injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{survivor.injuries[i].site} {survivor.injuries[i].type}";
        //        injuries[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{survivor.injuries[i].degree:0.##}";
        //        injuries[i].GetComponentInChildren<Help>().SetDescription(survivor.injuries[i].site);
        //        injuries[i].SetActive(true);
        //    }
        //    else
        //    {
        //        injuries[i].SetActive(false);
        //    }
        //}
        ResetInjuryInfo();
        foreach (var injury in survivor.injuries)
        {
            Image targetPart = GetTargetImage(injury.site);
            if (injury.type == InjuryType.ArtificialPartsTransplanted)
            {
                targetPart.color = new Color(0.5f, 0.5f, 0.5f);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = new Color(0.5f, 0.5f, 0.5f);
                    subpartImage.GetComponentInChildren<Help>().SetDescription($"{injury.site} {injury.type}\nDegree : {injury.degree:0.##}");
                }
            }
            else if (injury.degree == 1)
            {
                targetPart.color = new Color(0.5f, 0, 0);
                List<InjurySite> subparts = Injury.GetSubparts(injury.site);
                foreach (var subpart in subparts)
                {
                    Image subpartImage = GetTargetImage(subpart);
                    subpartImage.color = new Color(0.5f, 0, 0);
                    subpartImage.GetComponentInChildren<Help>().SetDescription($"{injury.site} {injury.type}\nDegree : {injury.degree:0.##}");
                }
            }
            else
            {
                targetPart.color = new Color(1f, (1 - injury.degree) * 0.7f, (1 - injury.degree) * 0.7f);
            }
            targetPart.GetComponentInChildren<Help>().SetDescription($"{injury.site} {injury.type}\nDegree : {injury.degree:0.##}");
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
        selectedObjectCurrentStatus.text = survivor.CurrentStatus switch
        {
            Survivor.Status.Farming or Survivor.Status.FarmingBox => "Farming",
            Survivor.Status.InCombat => "In combat",
            Survivor.Status.InvestigateThreateningSound => "Investigating treatening sound",
            Survivor.Status.Maintain => "Maintaining",
            Survivor.Status.Trapping => "Trapping",
            Survivor.Status.TraceEnemy => "Tracing enemy",
            Survivor.Status.RunAway => "Running away",
            Survivor.Status.TrapDisarming => "Trap Disarming",
            Survivor.Status.Crafting => $"Crafting : {survivor.CurrentCrafting.itemType}",
            Survivor.Status.Enchanting => $"Enchanting",
            _ => survivor.CurrentStatus.ToString()
        };
        GameManager.Instance.FixLayout(selectedObjectCurrentStatus.GetComponent<RectTransform>());
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
                else if(selectedSurvivor.CurrentWeapon is MeleeWeapon meleeWeapon)
                {
                    if (meleeWeapon.IsEnchanted) selectedObjectsCurrentWeaponText.text = $"{selectedSurvivor.CurrentWeapon.itemName}(Poison enchanted)";
                    else selectedObjectsCurrentWeaponText.text = selectedSurvivor.CurrentWeapon.itemName.GetLocalizedString();
                }
                else
                {
                    selectedObjectsCurrentWeaponText.text = selectedSurvivor.CurrentWeapon.itemName.GetLocalizedString();
                }
            }
            else
            {
                selectedObjectsCurrentWeaponText.text = "None";
            }

            if (selectedSurvivor.CurrentHelmet != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentHelmet.itemType}", out var helmetSpriteEnum))
                selectedObjectsCurrentHelmetImage.sprite = ResourceManager.Get(helmetSpriteEnum);
            else selectedObjectsCurrentHelmetImage.sprite = null;
            selectedObjectsCurrentHelmetText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentHelmet) ? selectedSurvivor.CurrentHelmet.itemName.GetLocalizedString() : "None";

            if (selectedSurvivor.CurrentVest != null && Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivor.CurrentVest.itemType}", out var vestSpriteEnum))
                selectedObjectsCurrentVestImage.sprite = ResourceManager.Get(vestSpriteEnum);
            else selectedObjectsCurrentVestImage.sprite = null;
            selectedObjectsCurrentVestText.text = selectedSurvivor.IsValid(selectedSurvivor.CurrentVest) ? selectedSurvivor.CurrentVest.itemName.GetLocalizedString() : "None";

            selectedObjectsCurrentWeapon.SetActive(true);
            selectedObjectsCurrentHelmet.SetActive(true);
            selectedObjectsCurrentVest.SetActive(true);
            List<Item> selectedSurvivorsInventory = selectedSurvivor.Inventory;
            for (int i = 0; i < selectedObjectsItems.Length; i++)
            {
                if (selectedSurvivorsInventory.Count > i)
                {
                    if (Enum.TryParse<ResourceEnum.Sprite>($"{selectedSurvivorsInventory[i].itemType}", out var spriteEnum))
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
                    selectedObjectsItems[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{selectedSurvivorsInventory[i].itemName} x {selectedSurvivorsInventory[i].amount}";
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

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public enum StrategyCase
{
    WeaponPriority = 0,
    SawAnEnemyAndItIsInAttackRange = 1,
    SawAnEnemyAndItIsOutsideOfAttackRange = 2,
    HeardDistinguishableSound = 3,
    HeardIndistinguishableSound = 4,
    WhenThereAreMultipleEnemiesInSightWhoIsTheTarget = 5,
    CraftingPriority = 6,
    CraftingAllow = 7,
    RepairCondition = 8,
    WhenAnEnemyDisappearsFromSight = 9,
}

[Serializable]
public class ConditionData
{
    public int andOr;
    public int variable1;
    public int operator_;
    public int variable2;
    public int inputInt;

    public ConditionData(int andOr, int variable1, int operator_, int variable2, int inputInt)
    {
        this.andOr = andOr;
        this.variable1 = variable1;
        this.operator_ = operator_;
        this.variable2 = variable2;
        this.inputInt = inputInt;
    }
}

[Serializable]
public class StrategyData
{
    public int action = 0;
    public int elseAction = 0;
    public int conditionConut = 0;
    public ConditionData[] conditions;
    public int etcValue1;
    public int etcValue2;

    public StrategyData(int action, int elseAction, int conditionConut, ConditionData[] conditions = null)
    {
        this.action = action;
        this.elseAction = elseAction;
        this.conditionConut = conditionConut;
        if (conditions == null)
        {
            this.conditions = new ConditionData[5];
            for (int i = 0; i < this.conditions.Length; i++) this.conditions[i] = new(0, 0, 0, 0, 0);
        }
        else this.conditions = conditions;
    }
}

public class Strategy : MonoBehaviour
{
    [SerializeField] GameObject[] conditions;
    [HideInInspector] public int activeConditionCount = 0;
    [HideInInspector] public TMP_Dropdown[] andOrs;
    GameObject[] notValids;
    [HideInInspector] public TMP_Dropdown[] variable1s;
    [HideInInspector] public TMP_Dropdown[] operators;
    [HideInInspector] public TMP_Dropdown[] variable2s;
    GameObject[] inputFieldsGameObject;
    GameObject[] inputFieldsPercents;
    [HideInInspector] public TMP_InputField[] inputFields;
    [SerializeField] GameObject action;
    public LocalizedDropdown ActionDropdown
    {
        get
        {
            if (action == null) return null;
            else return action.GetComponentInChildren<LocalizedDropdown>();
        }
    }
    [SerializeField] GameObject elseAction;
    public LocalizedDropdown ElseActionDropdown
    {
        get
        {
            if (elseAction == null) return null;
            else
            {
                return elseAction.GetComponentInChildren<LocalizedDropdown>();
            }
        }
    }
    [SerializeField] TMP_InputField intagerInput;
    public TMP_InputField IntagerInput => intagerInput;
    [SerializeField] LocalizedDropdown spareDropdown1;
    public LocalizedDropdown SpareDropdown1 => spareDropdown1;
    [SerializeField] LocalizedDropdown spareDropdown2;
    public LocalizedDropdown SpareDropdown2 => spareDropdown2;

    public StrategyCase strategyCase;
    [SerializeField] bool noCondition;
    public bool NoCondition => noCondition;
    StrategyData copyStrategy;
    public bool hasChanged;
    public bool HasChanged
    {
        get => hasChanged;
        set => hasChanged = value;
    }

    public string CaseName => transform.Find("Case Name").GetComponentInChildren<TextMeshProUGUI>().text;

    public bool[] craftableAllows;

    public void Initialize()
    {
        if(strategyCase == StrategyCase.CraftingAllow) craftableAllows = new bool[ItemManager.craftables.Count];
        if(intagerInput != null) intagerInput.onValueChanged.AddListener((value) => { ValidateInput(intagerInput, value, 0, 99); });
        if (noCondition) return;
        andOrs = new TMP_Dropdown[conditions.Length];
        notValids = new GameObject[conditions.Length];
        variable1s = new TMP_Dropdown[conditions.Length];
        operators = new TMP_Dropdown[conditions.Length];
        variable2s = new TMP_Dropdown[conditions.Length];
        inputFieldsGameObject = new GameObject[conditions.Length];
        inputFields = new TMP_InputField[conditions.Length];
        inputFieldsPercents = new GameObject[conditions.Length];
        for (int i=0; i<conditions.Length; i++)
        {
            GameObject condition = conditions[i];
            TMP_Dropdown[] dropdowns = condition.GetComponentsInChildren<TMP_Dropdown>(true);
            andOrs[i] = dropdowns[0];
            notValids[i] = condition.GetComponentsInChildren<Image>(true)[0].gameObject;
            variable1s[i] = dropdowns[1];
            operators[i] = dropdowns[2];
            variable2s[i] = dropdowns[3];
            inputFieldsGameObject[i] = condition.transform.Find("Input Field").gameObject;
            inputFields[i] = inputFieldsGameObject[i].GetComponentInChildren<TMP_InputField>();
            inputFieldsPercents[i] = inputFieldsGameObject[i].transform.Find("Percent").gameObject;
            inputFields[i].pointSize = 29;
            inputFields[i].characterLimit = 2;
            inputFields[i].text = "0";
            int index = i;
            inputFields[index].onValueChanged.AddListener((value) => { ValidateInput(inputFields[index], value, 0, 100); });

            andOrs[i].ClearOptions();
            andOrs[i].AddOptions(new List<string>(new string[] { "AND", "OR" }));
            notValids[i].SetActive(false);
            variable1s[i].ClearOptions();
            variable1s[i].AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "My weapon").GetLocalizedString(), new LocalizedString("Basic", "Enemy's weapon").GetLocalizedString(), new LocalizedString("Basic", "My health").GetLocalizedString(), new LocalizedString("Basic", "That enemy").GetLocalizedString(), new LocalizedString("Basic", "Distance to enemy").GetLocalizedString() }));
            OnVariable1Changed(i);

            andOrs[i].onValueChanged.AddListener((value) => hasChanged = true);
            variable1s[i].onValueChanged.AddListener((value) => hasChanged = true);
            operators[i].onValueChanged.AddListener((value) => hasChanged = true);
            variable2s[i].onValueChanged.AddListener((value) => hasChanged = true);
            inputFields[i].onValueChanged.AddListener((value) => hasChanged = true);

            condition.SetActive(false);
        }
        ActionDropdown.dropdown.onValueChanged.AddListener((value) => hasChanged = true);
        ElseActionDropdown.dropdown.onValueChanged.AddListener((value) => hasChanged = true);
        andOrs[0].gameObject.SetActive(false);
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    public void ResetConditions()
    {
        if (noCondition) return;
        DeleteCondition();
        DeleteCondition();
        DeleteCondition();
        DeleteCondition();
        DeleteCondition();
    }

    public void AddCondition()
    {
        if (activeConditionCount > 4) return;
        conditions[activeConditionCount].SetActive(true);
        conditions[activeConditionCount].transform.Find("Delete Condition").gameObject.SetActive(true);
        for(int i=0; i<activeConditionCount; i++) conditions[i].transform.Find("Delete Condition").gameObject.SetActive(false);
        elseAction.SetActive(true);
        activeConditionCount++;
        GameManager.Instance.FixLayout(GetComponent<RectTransform>());
        hasChanged = true;
    }

    public void DeleteCondition()
    {
        activeConditionCount = Mathf.Max(activeConditionCount - 1, 0);
        conditions[activeConditionCount].SetActive(false);
        if(activeConditionCount > 0) conditions[activeConditionCount - 1].transform.Find("Delete Condition").gameObject.SetActive(true);
        if(activeConditionCount == 0) elseAction.SetActive(false);
        // active가 꺼진 후에 StartCoroutine을 하면 에러가 뜨기 때문에 게임매니저가 StartCoroutine 호출
        GameManager.Instance.FixLayout(GetComponent<RectTransform>());
        hasChanged = true;
    }

    public void OnVariable1Changed(int conditionNumber)
    {
        operators[conditionNumber].ClearOptions();
        variable2s[conditionNumber].ClearOptions();
        switch(variable1s[conditionNumber].value)
        {
            case 0: // My weapon
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Melee weapon").GetLocalizedString(), new LocalizedString("Basic", "Ranged weapon (with bullets)").GetLocalizedString(), new LocalizedString("Basic", "None or ranged weapon (without bullets)").GetLocalizedString() }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case 1: // The enemy's weapon
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Melee weapon").GetLocalizedString(), new LocalizedString("Basic", "Ranged weapon").GetLocalizedString(), new LocalizedString("Basic", "None").GetLocalizedString() }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case 2: // My HP
                operators[conditionNumber].AddOptions(new List<string>(new string[] { ">", "<" }));
                variable2s[conditionNumber].gameObject.SetActive(false);
                inputFieldsPercents[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(true);
                break;
            case 3: // The enemy
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "Saw me.").GetLocalizedString() }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case 4: // Distance with the enemy
                operators[conditionNumber].AddOptions(new List<string>(new string[] { ">", "<" }));
                variable2s[conditionNumber].gameObject.SetActive(false);
                inputFieldsPercents[conditionNumber].gameObject.SetActive(false);
                inputFieldsGameObject[conditionNumber].SetActive(true);
                break;
            default:
                Debug.LogWarning($"Wrong condition case : {variable1s[conditionNumber].options[variable1s[conditionNumber].value].text}");
                break;
        }
    }

    void ValidateInput(TMP_InputField inputField, string value, int min, int max)
    {
        if (string.IsNullOrEmpty(value)) // 빈 문자열 체크
        {
            inputField.text = "0";
            return;
        }

        if (int.TryParse(value, out int number))
        {
            number = Mathf.Clamp(number, min, max);
            if (inputField.text != number.ToString()) // 무한 루프 방지
                inputField.text = number.ToString();
        }
        else
        {
            inputField.text = "0"; // 숫자가 아닐 경우 0으로 설정
        }
    }

    public static void ResetStrategyDictionary(Dictionary<StrategyCase, StrategyData> wantDictionary)
    {
        wantDictionary.Clear();
        wantDictionary.Add(StrategyCase.SawAnEnemyAndItIsInAttackRange, new(0, 0, 0));
        wantDictionary.Add(StrategyCase.SawAnEnemyAndItIsOutsideOfAttackRange, new(0, 0, 0));
        wantDictionary.Add(StrategyCase.WhenAnEnemyDisappearsFromSight, new(0, 0, 0));
        wantDictionary.Add(StrategyCase.HeardDistinguishableSound, new(0, 0, 0));
        wantDictionary.Add(StrategyCase.HeardIndistinguishableSound, new(1, 1, 0));
        wantDictionary.Add(StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget, new(1, 0, 0));
        wantDictionary.Add(StrategyCase.RepairCondition, new(70, 0, 0));
    }

    public void CopyStrategy()
    {
        if (strategyCase == StrategyCase.CraftingAllow)
        {
            for(int i=0; i<craftableAllows.Length;i++)
            {
                craftableAllows[i] = GameManager.Instance.OutGameUIManager.craftableAllows[i].GetComponentInChildren<Toggle>().isOn;
            }
            copyStrategy = new(0, 0, 0);
            return;
        }
        else if(strategyCase == StrategyCase.RepairCondition)
        {
            if (int.TryParse(intagerInput.text, out int input)) copyStrategy = new(input, 0, 0);
            else return;
        }
        else if (noCondition)
        {
            copyStrategy = new(ActionDropdown != null ? ActionDropdown.Value : 0, ElseActionDropdown != null ? ElseActionDropdown.Value : 0, 0);
            if(strategyCase == StrategyCase.CraftingPriority)
            {
                copyStrategy.etcValue1 = spareDropdown1.Value;
                copyStrategy.etcValue2 = spareDropdown2.Value;
            }
        }
        else
        {
            ConditionData[] conditions = new ConditionData[5];
            for (int i = 0; i < conditions.Length; i++)
            {
                conditions[i] = new(andOrs[i].value, variable1s[i].value, operators[i].value, variable2s[i].value, int.Parse(inputFields[i].text));
            }
            copyStrategy = new(ActionDropdown.Value, ElseActionDropdown.Value, activeConditionCount, conditions);
        }
    }

    public void PasteStrategy()
    {
        if (copyStrategy == null) return;
        if(strategyCase == StrategyCase.CraftingAllow)
        {
            for (int i = 0; i<craftableAllows.Length;i++)
            {
                // 0 : Allow, 1 : Not Allow
                int allow = craftableAllows[i] ? 0 : 1;
                GameManager.Instance.OutGameUIManager.craftableAllows[i].GetComponentsInChildren<Toggle>()[allow].isOn = true;
            }
            return;
        }
        if(ActionDropdown != null && ActionDropdown.keys.Count > copyStrategy.action && ActionDropdown.dropdown.interactable) ActionDropdown.Value = copyStrategy.action;
        if(ElseActionDropdown != null && ElseActionDropdown.keys.Count > copyStrategy.elseAction) ElseActionDropdown.Value = copyStrategy.elseAction;
        if (intagerInput != null) intagerInput.text = copyStrategy.action.ToString();
        if(!noCondition)
        {
            ResetConditions();
            for (int i = 0; i < copyStrategy.conditionConut; i++)
            {
                AddCondition();
                andOrs[i].value = copyStrategy.conditions[i].andOr;
                variable1s[i].value = copyStrategy.conditions[i].variable1;
                operators[i].value = copyStrategy.conditions[i].operator_;
                variable2s[i].value = copyStrategy.conditions[i].variable2;
                inputFields[i].text = copyStrategy.conditions[i].inputInt.ToString();
            }
        }
    }

    public void PasteThisStrategyToAllOtherSurvivor(bool all)
    {
        OutGameUIManager outGameUIManager = GameManager.Instance.OutGameUIManager;

        SurvivorData sourceSurvivor = outGameUIManager.SurvivorWhoWantEstablishStrategy;

        bool CanCraft(SurvivorData survivor, ItemManager.Craftable craftable)
        {
            if (craftable == null)
            {
                return false;
            }

            bool trapExpertAndTraps = survivor.characteristics.FindIndex(x => x.type == CharacteristicType.TrapExpert) != -1
                &&
                (
                    craftable.itemType == ItemManager.Items.BearTrap ||
                    craftable.itemType == ItemManager.Items.NoiseTrap ||
                    craftable.itemType == ItemManager.Items.ShrapnelTrap ||
                    craftable.itemType == ItemManager.Items.ChemicalTrap ||
                    craftable.itemType == ItemManager.Items.ExplosiveTrap ||
                    craftable.itemType ==
                        ItemManager.Items.TrapDetectionDevice
                );

            return craftable.requiredKnowledge <= survivor.Knowledge || trapExpertAndTraps;
        }

        int GetVisibleCraftableIndex(SurvivorData survivor, ItemManager.Craftable selectedCraftable)
        {
            if (selectedCraftable == null) return -1;

            int visibleIndex = 0;

            foreach (ItemManager.Craftable craftable in ItemManager.craftables)
            {
                if (!CanCraft(survivor, craftable)) continue;

                if (craftable.itemType == selectedCraftable.itemType)
                {
                    return visibleIndex;
                }

                visibleIndex++;
            }
            return -1;
        }

        int GetMaximumCraftingQuality(SurvivorData survivor)
        {
            if (survivor.Crafting > 60)
            {
                return 4; // Masterpiece
            }

            if (survivor.Crafting > 40)
            {
                return 3; // Excellent
            }

            if (survivor.Crafting > 20)
            {
                return 2; // Average
            }

            if (survivor.Crafting > 0)
            {
                return 1; // Shoddy
            }

            return 0; // Botched
        }

        ItemManager.Craftable GetSelectedCraftable(
            LocalizedDropdown dropdown)
        {
            if (dropdown == null ||
                dropdown.Value <= 0 ||
                dropdown.Value >= dropdown.keys.Count)
            {
                return null;
            }

            string itemKey =
                dropdown.keys[dropdown.Value]
                    .TableEntryReference.Key;

            return ItemManager.craftables.Find(x =>
                x.itemType.ToString() == itemKey);
        }

        ItemManager.Craftable selectedPriority1Crafting =
            strategyCase == StrategyCase.CraftingPriority
                ? GetSelectedCraftable(ActionDropdown)
                : null;

        ItemManager.Craftable selectedPriority2Crafting =
            strategyCase == StrategyCase.CraftingPriority
                ? GetSelectedCraftable(ElseActionDropdown)
                : null;

        foreach (SurvivorData survivor in
                 outGameUIManager.MySurvivorsData)
        {
            if (survivor.strategyDictionary == null)
            {
                survivor.strategyDictionary = new();
                Strategy.ResetStrategyDictionary(
                    survivor.strategyDictionary);
            }

            switch (strategyCase)
            {
                case StrategyCase.WeaponPriority:
                    {
                        string priority1Key =
                            ActionDropdown.keys[ActionDropdown.Value]
                                .TableEntryReference.Key;

                        string priority2Key =
                            ElseActionDropdown.keys[ElseActionDropdown.Value]
                                .TableEntryReference.Key;

                        bool priority1Parsed =
                            Enum.TryParse(
                                priority1Key,
                                out ItemManager.Items priority1Weapon);

                        bool priority2Parsed =
                            Enum.TryParse(
                                priority2Key,
                                out ItemManager.Items priority2Weapon);

                        bool priority1Fixed =
                            survivor.characteristics.FindIndex(x =>
                                x.type ==
                                    CharacteristicType.SniperRifleFanatic ||
                                x.type ==
                                    CharacteristicType.BazookaFanatic) != -1;

                        if (priority1Parsed && !priority1Fixed)
                        {
                            survivor.priority1Weapon = priority1Weapon;
                        }

                        if (priority2Parsed)
                        {
                            survivor.priority2Weapon = priority2Weapon;
                        }

                        break;
                    }

                case StrategyCase.CraftingPriority:
                    {
                        bool targetCanCraftPriority1 =
                            selectedPriority1Crafting != null &&
                            CanCraft(
                                survivor,
                                selectedPriority1Crafting);

                        if (!targetCanCraftPriority1)
                        {
                            // 우선순위 1을 제작할 수 없으면
                            // 우선순위 1과 2를 모두 None으로 설정합니다.
                            survivor.priority1Crafting = null;
                            survivor.priority1CraftingToInt = -1;

                            survivor.priority2Crafting = null;
                            survivor.priority2CraftingToInt = -1;
                        }
                        else
                        {
                            survivor.priority1Crafting =
                                selectedPriority1Crafting;

                            survivor.priority1CraftingToInt =
                                GetVisibleCraftableIndex(
                                    survivor,
                                    selectedPriority1Crafting);

                            bool targetCanCraftPriority2 =
                                selectedPriority2Crafting != null &&
                                CanCraft(
                                    survivor,
                                    selectedPriority2Crafting);

                            if (targetCanCraftPriority2)
                            {
                                survivor.priority2Crafting =
                                    selectedPriority2Crafting;

                                survivor.priority2CraftingToInt =
                                    GetVisibleCraftableIndex(
                                        survivor,
                                        selectedPriority2Crafting);
                            }
                            else
                            {
                                // 우선순위 2만 제작 불가능한 경우
                                // 우선순위 2만 None으로 설정합니다.
                                survivor.priority2Crafting = null;
                                survivor.priority2CraftingToInt = -1;
                            }
                        }

                        int maximumQuality =
                            GetMaximumCraftingQuality(survivor);

                        int priority1MinimumQuality =
                            Mathf.Min(
                                SpareDropdown1.Value,
                                maximumQuality);

                        int priority2MinimumQuality =
                            Mathf.Min(
                                SpareDropdown2.Value,
                                maximumQuality);

                        survivor.craftingPriority1MinimumQuality =
                            priority1MinimumQuality;

                        survivor.craftingPriority2MinimumQuality =
                            priority2MinimumQuality;

                        StrategyData craftingPriorityData =
                            new(
                                ActionDropdown.Value,
                                ElseActionDropdown.Value,
                                0);

                        craftingPriorityData.etcValue1 =
                            priority1MinimumQuality;

                        craftingPriorityData.etcValue2 =
                            priority2MinimumQuality;

                        survivor.strategyDictionary[
                            StrategyCase.CraftingPriority
                        ] = craftingPriorityData;

                        break;
                    }

                case StrategyCase.CraftingAllow:
                    {
                        int count = Mathf.Min(
                            ItemManager.craftables.Count,
                            survivor.craftingAllows.Length);

                        for (int i = 0; i < count; i++)
                        {
                            ItemManager.Craftable craftable =
                                ItemManager.craftables[i];

                            /*
                             * 원본 생존자가 제작할 수 있는 범위까지만
                             * 제작 허용 설정을 복사합니다.
                             *
                             * 대상 생존자가 제작할 수 없는 항목이어도
                             * 원본 생존자가 제작 가능한 항목이면 값을
                             * 복사합니다. 대상 UI에서는 숨겨져 있어도
                             * craftingAllows에는 값이 저장됩니다.
                             */
                            if (!CanCraft(sourceSurvivor, craftable))
                            {
                                continue;
                            }

                            Toggle[] toggles =
                                outGameUIManager.craftableAllows[i]
                                    .GetComponentsInChildren<Toggle>(true);

                            if (toggles.Length > 0)
                            {
                                survivor.craftingAllows[i] =
                                    toggles[0].isOn;
                            }
                        }

                        break;
                    }

                case StrategyCase.RepairCondition:
                    {
                        if (int.TryParse(
                            IntagerInput.text,
                            out int repairCondition))
                        {
                            survivor.strategyDictionary[
                                StrategyCase.RepairCondition
                            ] = new(
                                repairCondition,
                                0,
                                0);
                        }

                        break;
                    }

                default:
                    {
                        if (noCondition)
                        {
                            survivor.strategyDictionary[strategyCase] =
                                new(
                                    ActionDropdown != null
                                        ? ActionDropdown.Value
                                        : 0,
                                    ElseActionDropdown != null
                                        ? ElseActionDropdown.Value
                                        : 0,
                                    0);

                            break;
                        }

                        /*
                         * Survivor.ApplyStrategies()가 conditions[0]부터
                         * conditions[4]까지 접근하므로 항상 길이 5로
                         * 생성합니다.
                         */
                        ConditionData[] conditionData =
                            new ConditionData[5];

                        for (int i = 0;
                             i < conditionData.Length;
                             i++)
                        {
                            int inputValue = 0;

                            if (inputFields[i] != null)
                            {
                                int.TryParse(
                                    inputFields[i].text,
                                    out inputValue);
                            }

                            conditionData[i] =
                                new ConditionData(
                                    andOrs[i].value,
                                    variable1s[i].value,
                                    operators[i].value,
                                    variable2s[i].value,
                                    inputValue);
                        }

                        survivor.strategyDictionary[strategyCase] =
                            new StrategyData(
                                ActionDropdown.Value,
                                ElseActionDropdown.Value,
                                activeConditionCount,
                                conditionData);

                        break;
                    }
            }
        }

        if (!all)
        {
            outGameUIManager.Alert(
                "Strategy pasted and saved.");
        }
    }

    void OnLocaleChanged(Locale locale)
    {
        for(int i=0; i<conditions.Length;i++)
        {
            andOrs[i].ClearOptions();
            andOrs[i].AddOptions(new List<string>(new string[] { "AND", "OR" }));
            notValids[i].SetActive(false);
            variable1s[i].ClearOptions();
            variable1s[i].AddOptions(new List<string>(new string[] { new LocalizedString("Basic", "My weapon").GetLocalizedString(), new LocalizedString("Basic", "Enemy's weapon").GetLocalizedString(), new LocalizedString("Basic", "My health").GetLocalizedString(), new LocalizedString("Basic", "That enemy").GetLocalizedString(), new LocalizedString("Basic", "Distance to enemy").GetLocalizedString() }));
            OnVariable1Changed(i);
        }
    }
}

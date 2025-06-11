using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public enum StrategyCase
{
    WeaponPriority,
    SawAnEnemyAndItIsInAttackRange,
    SawAnEnemyAndItIsOutsideOfAttackRange,
    HeardDistinguishableSound,
    HeardIndistinguishableSound,
    WhenThereAreMultipleEnemiesInSightWhoIsTheTarget,
    CraftingPriority,
    CraftingAllow,
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
    TMP_Dropdown ActionDropdown => action.GetComponentInChildren<TMP_Dropdown>();
    [SerializeField] GameObject elseAction;
    TMP_Dropdown ElseActionDropdown => elseAction.GetComponentInChildren<TMP_Dropdown>();

    public StrategyCase strategyCase;
    [SerializeField] bool noCondition;
    public bool NoCondition => noCondition;
    StrategyData copyStrategy;
    public bool hasChanged;

    public string CaseName => transform.Find("Case Name").GetComponentInChildren<TextMeshProUGUI>().text;

    public bool[] craftableAllows;

    private void Start()
    {
        GameManager.Instance.OutGameUIManager.OpenStrategyRoom();
        if(strategyCase == StrategyCase.CraftingAllow) craftableAllows = new bool[ItemManager.craftables.Count];
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
            TMP_Dropdown[] dropdowns = condition.GetComponentsInChildren<TMP_Dropdown>();
            andOrs[i] = dropdowns[0];
            notValids[i] = condition.GetComponentInChildren<Image>().gameObject;
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
            inputFields[index].onValueChanged.AddListener((value) => { ValidateInput(inputFields[index], value); });

            andOrs[i].ClearOptions();
            andOrs[i].AddOptions(new List<string>(new string[] { "AND", "OR" }));
            notValids[i].SetActive(false);
            variable1s[i].ClearOptions();
            variable1s[i].AddOptions(new List<string>(new string[] { new LocalizedString("Table", "My weapon").GetLocalizedString(), new LocalizedString("Table", "Enemy's weapon").GetLocalizedString(), new LocalizedString("Table", "My health").GetLocalizedString(), new LocalizedString("Table", "That enemy").GetLocalizedString(), new LocalizedString("Table", "Distance to enemy").GetLocalizedString() }));
            OnVariable1Changed(i);

            andOrs[i].onValueChanged.AddListener((value) => hasChanged = true);
            variable1s[i].onValueChanged.AddListener((value) => hasChanged = true);
            operators[i].onValueChanged.AddListener((value) => hasChanged = true);
            variable2s[i].onValueChanged.AddListener((value) => hasChanged = true);
            inputFields[i].onValueChanged.AddListener((value) => hasChanged = true);

            condition.SetActive(false);
        }
        ActionDropdown.onValueChanged.AddListener((value) => hasChanged = true);
        ElseActionDropdown.onValueChanged.AddListener((value) => hasChanged = true);
        andOrs[0].gameObject.SetActive(false);
        GameManager.Instance.OutGameUIManager.CloseStrategyRoom();
    }

    public void SetDefault()
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
    }

    public void DeleteCondition()
    {
        activeConditionCount = Mathf.Max(activeConditionCount - 1, 0);
        conditions[activeConditionCount].SetActive(false);
        if(activeConditionCount > 0) conditions[activeConditionCount - 1].transform.Find("Delete Condition").gameObject.SetActive(true);
        if(activeConditionCount == 0) elseAction.SetActive(false);
        // active가 꺼진 후에 StartCoroutine을 하면 에러가 뜨기 때문에 게임매니저가 StartCoroutine 호출
        GameManager.Instance.FixLayout(GetComponent<RectTransform>());
    }

    public void OnVariable1Changed(int conditionNumber)
    {
        operators[conditionNumber].ClearOptions();
        variable2s[conditionNumber].ClearOptions();
        switch(variable1s[conditionNumber].value)
        {
            case 0: // My weapon
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Table", "Melee weapon").GetLocalizedString(), new LocalizedString("Table", "Ranged weapon (with bullets)").GetLocalizedString(), new LocalizedString("Table", "None or ranged weapon (without bullets)").GetLocalizedString() }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case 1: // The enemy's weapon
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Table", "Melee weapon").GetLocalizedString(), new LocalizedString("Table", "Ranged weapon").GetLocalizedString(), new LocalizedString("Table", "None").GetLocalizedString() }));
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
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { new LocalizedString("Table", "Saw me.").GetLocalizedString() }));
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

    void ValidateInput(TMP_InputField inputField, string value)
    {
        if (string.IsNullOrEmpty(value)) // 빈 문자열 체크
        {
            inputField.text = "0";
            return;
        }

        if (int.TryParse(value, out int number))
        {
            number = Mathf.Clamp(number, 0, 100);
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
        wantDictionary.Add(StrategyCase.HeardDistinguishableSound, new(0, 0, 0));
        wantDictionary.Add(StrategyCase.HeardIndistinguishableSound, new(1, 1, 0));
        wantDictionary.Add(StrategyCase.WhenThereAreMultipleEnemiesInSightWhoIsTheTarget, new(0, 0, 0));
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
        if (noCondition) copyStrategy = new(ActionDropdown.value, 0, 0);
        else
        {
            ConditionData[] conditions = new ConditionData[5];
            for (int i = 0; i < conditions.Length; i++)
            {
                conditions[i] = new(andOrs[i].value, variable1s[i].value, operators[i].value, variable2s[i].value, int.Parse(inputFields[i].text));
            }
            copyStrategy = new(ActionDropdown.value, ElseActionDropdown.value, activeConditionCount, conditions);
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
        ActionDropdown.value = copyStrategy.action;
        if(!noCondition)
        {
            SetDefault();
            for (int i = 0; i < copyStrategy.conditionConut; i++)
            {
                AddCondition();
                andOrs[i].value = copyStrategy.conditions[i].andOr;
                variable1s[i].value = copyStrategy.conditions[i].variable1;
                operators[i].value = copyStrategy.conditions[i].operator_;
                variable2s[i].value = copyStrategy.conditions[i].variable2;
                inputFields[i].text = copyStrategy.conditions[i].inputInt.ToString();
            }
            ElseActionDropdown.value = copyStrategy.elseAction;
        }
    }
}

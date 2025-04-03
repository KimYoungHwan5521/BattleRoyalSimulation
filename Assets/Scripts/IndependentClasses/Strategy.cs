using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StrategyCase
{
    SawAnEnemyAndItIsInAttackRange,
    SawAnEnemyAndItIsOutsideOfAttackRange,
    HeardDistinguishableSound,
    HeardIndistinguishableSound,
}

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
    [HideInInspector] public TMP_InputField[] inputFields;
    [SerializeField]GameObject elseAction;

    public StrategyCase strategyCase;

    private void Start()
    {
        andOrs = new TMP_Dropdown[conditions.Length];
        notValids = new GameObject[conditions.Length];
        variable1s = new TMP_Dropdown[conditions.Length];
        operators = new TMP_Dropdown[conditions.Length];
        variable2s = new TMP_Dropdown[conditions.Length];
        inputFieldsGameObject = new GameObject[conditions.Length];
        inputFields = new TMP_InputField[conditions.Length];
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
            inputFields[i].pointSize = 29;
            inputFields[i].characterLimit = 2;
            inputFields[i].text = "0";
            int index = i;
            inputFields[index].onValueChanged.AddListener((value) => { ValidateInput(inputFields[index], value); });

            andOrs[i].ClearOptions();
            andOrs[i].AddOptions(new List<string>(new string[] { "AND", "OR" }));
            notValids[i].SetActive(false);
            variable1s[i].ClearOptions();
            variable1s[i].AddOptions(new List<string>(new string[] { "My weapon", "Enemy's weapon", "My HP" }));
            OnVariable1Changed(i);

            condition.SetActive(false);
        }
        andOrs[0].gameObject.SetActive(false);
    }

    public void SetDefault()
    {
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
        switch(variable1s[conditionNumber].options[variable1s[conditionNumber].value].text)
        {
            case "My weapon":
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { "Melee weapon", "Ranged weapon", "None or Ranged with no bullet" }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case "Enemy's weapon":
                operators[conditionNumber].AddOptions(new List<string>(new string[] { "is", "is not" }));
                variable2s[conditionNumber].AddOptions(new List<string>(new string[] { "Melee weapon", "Ranged weapon", "None" }));
                variable2s[conditionNumber].gameObject.SetActive(true);
                inputFieldsGameObject[conditionNumber].SetActive(false);
                break;
            case "My HP":
                operators[conditionNumber].AddOptions(new List<string>(new string[] { ">", "<" }));
                variable2s[conditionNumber].gameObject.SetActive(false);
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
}

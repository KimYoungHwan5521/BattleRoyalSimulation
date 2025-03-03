using TMPro;
using UnityEngine;

public class Count3 : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI countText;
    void AE_Count3(int count)
    {
        if (count == 0)
        {
            countText.text = "Battle!";
            BattleRoyaleManager.isBattleRoyaleStart = true;
            BattleRoyaleManager.battleTime = 0;
            GameManager.Instance.GetComponent<InGameUIManager>().SetTimeScale(1);
        }
        else if(count == -1)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            countText.text = count.ToString();
        }
    }
}

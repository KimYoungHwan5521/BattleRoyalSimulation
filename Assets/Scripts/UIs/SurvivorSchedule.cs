using TMPro;
using UnityEngine;

public class SurvivorSchedule : MonoBehaviour
{
    public SurvivorData survivor;
    [SerializeField] TextMeshProUGUI survivorName;
    bool assignable;
    [SerializeField] Transform origin;
    [SerializeField] Transform target;
    [SerializeField] Training curTraining;
    public Training whereAmI;
    bool amIOrigin = true;

    public void SetSurvivorData(SurvivorData wantSurvivor, Training curTraining, bool trainable, Transform origin, Transform target)
    {
        survivor = wantSurvivor;
        if (trainable) survivorName.text = survivor.survivorName;
        else survivorName.text = $"<color=red>{survivor.survivorName}</color>";
        this.curTraining = curTraining;
        assignable = trainable;
        this.origin = origin;
        this.target = target;
        whereAmI = survivor.assignedTraining;
        amIOrigin = true;
    }

    public void Click()
    {
        if(assignable)
        {
            if(amIOrigin)
            {
                transform.SetParent(target, false);
            }
            else
            {
                transform.SetParent(origin, false);
            }
            if (whereAmI == curTraining) whereAmI = Training.None;
            else whereAmI = curTraining;
            amIOrigin = !amIOrigin;
            GameManager.Instance.FixLayout(GameManager.Instance.outCanvas.GetComponent<RectTransform>());
        }
    }
}

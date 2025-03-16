using TMPro;
using UnityEngine;

public class SurvivorSchedule : MonoBehaviour
{
    SurvivorData survivor;
    [SerializeField] TextMeshProUGUI survivorName;
    [SerializeField] GameObject checkBox;
    int trainingIndex;

    public void SetSurvivorData(SurvivorData wantSurvivor, int wantTraining, bool trainable)
    {
        survivor = wantSurvivor;
        trainingIndex = wantTraining;
        if (trainable) survivorName.text = survivor.survivorName;
        else survivorName.text = $"<color=red>{survivor.survivorName}</color>";

        checkBox.SetActive(survivor.assignedTraining == (Training)wantTraining);
    }

    public void Check()
    {
        if(survivor.assignedTraining == Training.None)
        {
            survivor.assignedTraining = (Training)trainingIndex;
            checkBox.SetActive(true);
        }
        else
        {
            survivor.assignedTraining = Training.None;
            checkBox.SetActive(false);
        }
    }
}

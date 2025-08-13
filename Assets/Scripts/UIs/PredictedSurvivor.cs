using UnityEngine;

public class PredictedSurvivor : MonoBehaviour
{
    public Survivor linkedSurvivor;

    public void FocusOnPredictedSurvivor()
    {
        if (linkedSurvivor == null) return;
        InGameUIManager inGameUIManager = GameManager.Instance.GetComponent<InGameUIManager>();
        inGameUIManager.SelectObject(linkedSurvivor);
    }
}

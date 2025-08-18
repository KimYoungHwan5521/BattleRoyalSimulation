using System;
using UnityEngine;

public class EasySchedule : MonoBehaviour
{
    [SerializeField] GameObject ownerCanvas;
    public SurvivorInfo linkedSurvivor;

    public void Schedule(int i)
    {
        linkedSurvivor.survivorData.assignedTraining = ((Training[])Enum.GetValues(typeof(Training)))[i];
        ownerCanvas.SetActive(false);
        GameManager.Instance.OutGameUIManager.AssignTraining();
        linkedSurvivor.SetInfo(linkedSurvivor.survivorData, false);
    }
}

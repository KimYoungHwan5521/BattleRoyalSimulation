using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Area : CustomObject
{
    public FarmingSection[] farmingSections;
    [SerializeField] bool isProhibited_Plan;
    public bool IsProhibited_Plan
    {
        get => isProhibited_Plan;
        set
        {
            isProhibited_Plan = value;
            markProhibitedArea_Plan.SetActive(value);
        }
    }
    bool isProhibited;
    public bool IsProhibited
    {
        get => isProhibited;
        set
        {
            isProhibited = value;
            markProhibitedArea.SetActive(value);
            IsProhibited_Plan = !value;

            markProhibitedArea.GetComponent<NavMeshObstacle>().carving = true;
            foreach(Survivor survivor in BattleRoyaleManager.Survivors)
            {
                if(survivor.GetCurrentArea() == this)
                {
                    GameObject linkObject = new GameObject("DynamicNavMeshLink");
                    NavMeshLink navMeshLink = linkObject.AddComponent<NavMeshLink>();

                    // 두 NavMesh 영역을 연결
                    navMeshLink.startPoint = survivor.transform.position;
                    navMeshLink.endPoint = survivor.FindNearest(survivor.farmingAreas).transform.position;
                    navMeshLink.width = 2f;
                    navMeshLink.UpdateLink();
                }
            }
        }
    }
    public Area[] adjacentAreas;
    [SerializeField] GameObject markProhibitedArea_Plan;
    [SerializeField] GameObject markProhibitedArea;

    public void Initiate()
    {
        farmingSections = GetComponentsInChildren<FarmingSection>();
        foreach (FarmingSection farmingSection in farmingSections)
        {
            farmingSection.ownerArea = this;
            farmingSection.boxes = farmingSection.GetComponentsInChildren<Box>();
            foreach (Box box in farmingSection.boxes)
            {
                box.ownerArea = this;
            }
        }

    }

    public void ResetProhibitArea()
    {
        IsProhibited_Plan = false;
        isProhibited = false;
        markProhibitedArea_Plan.SetActive(false);
        markProhibitedArea.SetActive(false);
    }
}

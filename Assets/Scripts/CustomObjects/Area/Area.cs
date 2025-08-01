using NavMeshPlus.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Area : CustomObject
{
    public FarmingSection[] farmingSections;
    public List<GameObject> garbageObjects = new();
    [SerializeField] NavMeshModifierVolume navMeshModifierVolume;
    [SerializeField] TilemapCollider2D water;

    [SerializeField] bool isProhibited_Plan;
    public bool IsProhibited_Plan
    {
        get => isProhibited_Plan;
        set
        {
            isProhibited_Plan = value;
            markProhibitedArea_Plan.SetActive(value);

            navMeshModifierVolume.area = 3;
            //if (water != null) water.enabled = true;
        }
    }
    [SerializeField] bool isProhibited;
    public bool IsProhibited
    {
        get => isProhibited;
        set
        {
            isProhibited = value;
            markProhibitedArea.SetActive(value);
            IsProhibited_Plan = !value;

            navMeshModifierVolume.area = 4;
            //if (water != null) water.enabled = true;
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

    public void TurnOffWater()
    {
        //if(water != null) water.enabled = false;
    }
}

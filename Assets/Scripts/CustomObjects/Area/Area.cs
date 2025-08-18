using NavMeshPlus.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Area : CustomObject
{
    public FarmingSection[] farmingSections;
    public List<GameObject> garbageObjects = new();
    [SerializeField] NavMeshModifierVolume navMeshModifierVolume;
    Dictionary<Area, int> distances;
    public Dictionary<Area, int> Distances => distances;

    [SerializeField] bool isProhibited_Plan;
    public bool IsProhibited_Plan
    {
        get => isProhibited_Plan;
        set
        {
            isProhibited_Plan = value;
            markProhibitedArea_Plan.SetActive(value);

            navMeshModifierVolume.area = 3;
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
        GetDistances();
    }

    public void GetDistances()
    {
        distances = new();
        List<Area> remember = new();
        distances[this] = 0;
        remember.Add(this);
        List<Area> candidates = new();
        foreach (Area area in adjacentAreas)
        {
            if(!area.isProhibited) candidates.Add(area);
        }
        int distance = 0;
        while (remember.Count < candidates.Count)
        {
            distance++;
            if (distance > 1000)
            {
                Debug.LogWarning("Infinite loop detected!");
                break;
            }
            foreach (Area area in candidates)
            {
                if (remember.Contains(area) || area.isProhibited) continue;
                distances[area] = distance;
                remember.Add(area);
            }
            foreach (Area area in candidates)
            {
                if (remember.Contains(area)) continue;
                foreach (Area adjacent in area.adjacentAreas) candidates.Add(adjacent);
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

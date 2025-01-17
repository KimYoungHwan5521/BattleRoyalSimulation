using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : CustomObject
{
    public FarmingSection[] farmingSections;
    bool isProhibited;
    public bool IsProhibited
    {
        get => isProhibited;
        set
        {
            isProhibited = value;
            markProhibitedArea.SetActive(value);
        }
    }
    public Area[] adjacentAreas;
    [SerializeField] GameObject markProhibitedArea;

    protected override void Start()
    {
        base.Start();

        farmingSections = GetComponentsInChildren<FarmingSection>();
    }
}

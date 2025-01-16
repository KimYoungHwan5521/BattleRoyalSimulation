using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : CustomObject
{
    public FarmingSection[] farmingSections;
    public bool isProhibited;

    protected override void Start()
    {
        base.Start();

        farmingSections = GetComponentsInChildren<FarmingSection>();
    }
}

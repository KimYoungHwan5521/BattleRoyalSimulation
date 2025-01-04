using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingSection : CustomObject
{
    public Box[] boxes;

    protected override void Start()
    {
        base.Start();

        boxes = GetComponentsInChildren<Box>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : CustomObject
{
    [SerializeField, Range(0, 1f)] float obstructionRate;
    public float OpstructionRate => obstructionRate;

    public void SetObstructionRate(float value)
    {
        obstructionRate = value;
    }
}

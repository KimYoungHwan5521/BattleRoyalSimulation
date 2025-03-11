using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : CustomObject
{
    [SerializeField] float obstructionRate;
    public float OpstructionRate => obstructionRate;

    public void SetObstructionRate(float value)
    {
        obstructionRate = value;
    }
}

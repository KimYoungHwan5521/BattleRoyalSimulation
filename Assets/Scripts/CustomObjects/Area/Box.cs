using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : CustomObject
{
    public Area ownerArea;
    public List<Item> items = new();
}

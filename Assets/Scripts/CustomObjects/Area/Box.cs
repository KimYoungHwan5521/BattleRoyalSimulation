using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Obstacle
{
    public Area ownerArea;
    public List<Item> items = new();
    public Collider2D Collider => GetComponent<Collider2D>();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager
{
    public static MeleeWeapon knife;
    public IEnumerator Initiate()
    {
        knife = new("knife", 2f, 40, 1.7f);
        yield return null;
    }
}

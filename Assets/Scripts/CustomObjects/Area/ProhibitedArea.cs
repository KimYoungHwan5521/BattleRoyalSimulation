using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProhibitedArea : CustomObject
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Survivor survivor))
        {
            survivor.inPorohibitedArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.TryGetComponent(out Survivor survivor))
        {
            survivor.inPorohibitedArea = false;
        }
    }
}

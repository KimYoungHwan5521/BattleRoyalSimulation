using UnityEngine;

public class ProhibitedArea : CustomObject
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Survivor survivor) && !collision.isTrigger)
        {
            survivor.InProhibitedArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.TryGetComponent(out Survivor survivor) && !collision.isTrigger)
        {
            survivor.InProhibitedArea = false;
        }
    }
}

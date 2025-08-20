using UnityEngine;

public class ProhibitedArea : CustomObject
{
    Collider2D col;

    protected override void Start()
    {
        base.Start();
        col = GetComponent<Collider2D>();
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if(collision.TryGetComponent(out Survivor survivor) && !collision.isTrigger)
    //    {
    //        // 단순히 닿아있는게 아니라 중심이 들어와 있는지로 판정
    //        survivor.InProhibitedArea = col.OverlapPoint(survivor.transform.position);
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{

    //    if (collision.TryGetComponent(out Survivor survivor) && !collision.isTrigger)
    //    {
    //        survivor.InProhibitedArea = false;
    //    }
    //}
}

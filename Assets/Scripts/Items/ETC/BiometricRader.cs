using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiometricRader : MonoBehaviour
{
    [SerializeField] Survivor owner;
    [SerializeField] List<Survivor> detecteds;

    [SerializeField] int cool = 60;
    int count = 0;
    void Update()
    {
        count++;
        if(count > cool)
        {
            count = 0;
            List<Vector2> vector2s = new();
            List<Survivor> removeReserve = new();
            for(int i=0; i< detecteds.Count; i++)
            {
                if (detecteds[i] == owner || detecteds[i].IsDead) removeReserve.Add(detecteds[i]);
                else vector2s.Add(detecteds[i].transform.position);
            }
            foreach(Survivor survivor in removeReserve) detecteds.Remove(survivor);
            owner.DetectSurvivor(vector2s);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.TryGetComponent(out Survivor survivor))
        {
            if(!detecteds.Contains(survivor)) detecteds.Add(survivor);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.isTrigger && collision.TryGetComponent(out Survivor survivor))
        {
            if (detecteds.Contains(survivor)) detecteds.Remove(survivor);
        }
    }
}

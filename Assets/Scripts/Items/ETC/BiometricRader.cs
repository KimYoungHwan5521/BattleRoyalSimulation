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
            Vector2[] vector2s = new Vector2[detecteds.Count];
            for(int i=0; i<vector2s.Length; i++)
            {
                vector2s[i] = detecteds[i].transform.position;
            }
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

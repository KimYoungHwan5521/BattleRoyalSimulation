using System;
using UnityEngine;

public class CustomObject : MonoBehaviour
{
    protected virtual void Start()
    {
        GameManager.Instance.ObjectStart += MyStart;
        GameManager.Instance.ObjectUpdate += MyUpdate;
    }

    protected virtual void MyStart() { }
    protected virtual void MyUpdate() { }
    protected virtual void MyDestroy()
    {
        GameManager.Instance.ObjectUpdate -= MyUpdate;
        Destroy(gameObject);
    }

    public virtual void PlaySFX(string wantSound)
    {
        if(Enum.TryParse(wantSound, out ResourceEnum.SFX result))
        {
            SoundManager.Play(result, transform.position);
        }
        else
        {
            Debug.LogWarning($"Can't find ResourceEnum.SFX : {wantSound}");
        }
    }
}
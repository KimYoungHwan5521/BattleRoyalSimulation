using System;
using UnityEngine;

public class CustomObject : MonoBehaviour
{
    protected virtual void Start()
    {
        GameManager.Instance.ObjectStart += MyStart;
        GameManager.Instance.ObjectUpdate += MyUpdate;
    }

    public virtual void MyStart() { }
    public virtual void MyUpdate() { }
    public virtual void MyDestroy()
    {
        GameManager.Instance.ObjectUpdate -= MyUpdate;
        Destroy(gameObject);
    }

    public virtual void PlaySFX(string wantSoundAndVolume)
    {
        PlaySFX(wantSoundAndVolume, this);
    }

    public virtual void PlaySFX(string wantSoundAndVolume, CustomObject noiseMaker)
    {
        wantSoundAndVolume = wantSoundAndVolume.Replace(" ", "");
        string wantSound = wantSoundAndVolume.Split(",")[0];
        if (Enum.TryParse(wantSound, out ResourceEnum.SFX result))
        {
            SoundManager.Play(result, transform.position);
            if (float.TryParse(wantSoundAndVolume.Split(",")[1], out float wantVolume))
            {
                foreach (Survivor survivor in BattleRoyaleManager.AliveSurvivors)
                {
                    if(noiseMaker != this) survivor.HearSound(wantVolume, transform.position, noiseMaker);
                }
            }
            else
            {
                Debug.LogWarning($"Unvalid wantSoundAndVoulume : {wantSoundAndVolume} (Valid : wantSound,wantVolume)");
            }
        }
        else
        {
            Debug.LogWarning($"Can't find ResourceEnum.SFX : {wantSound}");
        }
    }
}
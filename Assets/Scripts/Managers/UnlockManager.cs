using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    [SerializeField] Locked[] lockeds;

    public enum UnlockCondition
    {
        FirstParticipateInBattleRoyale,
    }
    public Dictionary<UnlockCondition, bool> unlockStatus = new();

    public IEnumerator Initiate()
    {
        unlockStatus.Add(UnlockCondition.FirstParticipateInBattleRoyale, false);
        yield return null;
    }

    public void LoadUnlockStatus(List<UnlockStatusDictionary> unlockStatusD)
    {
        foreach(var unlockS in unlockStatusD)
        {
            unlockStatus[unlockS.unlockCondition] = unlockS.isUnlocked;
        }
    }

    public void Unlock(UnlockCondition Condition)
    {
        foreach(var locked in lockeds)
        {
            if(locked.unlockCondition == Condition) locked.Unlock();
        }
    }

    public void CheckAlreadyLocked(bool oldVersion)
    {
        if(oldVersion)
        {

        }
        foreach(var locked in lockeds)
        {
            locked.gameObject.SetActive(!locked.IsUnlocked);
        }
    }
}

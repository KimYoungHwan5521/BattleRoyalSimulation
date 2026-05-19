using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    [SerializeField] Locked[] lockeds;

    public enum UnlockCondition
    {
        NoInfo =6,
        FirstParticipateInBattleRoyale = 0,
        WinBronzeLeague = 1,
        WinSilverLeague = 2,
        WinGoldLeague = 3,
        WinSeasonChampionship = 4,
        WinWorldChampionship = 5,
    }
    public Dictionary<UnlockCondition, bool> unlockStatus = new();

    public IEnumerator Initiate()
    {
        foreach (UnlockCondition condition in Enum.GetValues(typeof(UnlockCondition)))
        {
            unlockStatus.Add(condition, false);
        }
        yield return null;
    }

    public void LoadUnlockStatus(List<UnlockStatusDictionary> unlockStatusD)
    {
        RelockAll();
        foreach(var unlockS in unlockStatusD)
        {
            unlockStatus[unlockS.unlockCondition] = unlockS.isUnlocked;
            if (unlockS.isUnlocked) Unlock(unlockS.unlockCondition);
        }
    }

    public void Unlock(UnlockCondition condition)
    {
        unlockStatus[condition] = true;
        foreach(var locked in lockeds)
        {
            if(locked.unlockCondition == condition) locked.Unlock();
        }
    }

    public void RelockAll()
    {
        foreach (UnlockCondition condition in Enum.GetValues(typeof(UnlockCondition)))
        {
            unlockStatus[condition] = false;
        }
        foreach (var locked in lockeds)
        {
            locked.Lock();
        }
    }

    public void CheckAlreadyLocked(bool oldVersion)
    {
        if(oldVersion)
        {
            foreach(var survivor in GameManager.Instance.OutGameUIManager.MySurvivorsData)
            {
                if(survivor.wonWorldChampionship)
                {
                    unlockStatus[UnlockCondition.WinWorldChampionship] = true;
                    unlockStatus[UnlockCondition.WinSeasonChampionship] = true;
                    unlockStatus[UnlockCondition.WinGoldLeague] = true;
                    unlockStatus[UnlockCondition.WinSilverLeague] = true;
                    unlockStatus[UnlockCondition.WinBronzeLeague] = true;
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
                else if(survivor.wonSeasonChampionship)
                {
                    unlockStatus[UnlockCondition.WinSeasonChampionship] = true;
                    unlockStatus[UnlockCondition.WinGoldLeague] = true;
                    unlockStatus[UnlockCondition.WinSilverLeague] = true;
                    unlockStatus[UnlockCondition.WinBronzeLeague] = true;
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
                else if(survivor.wonGoldLeague)
                {
                    unlockStatus[UnlockCondition.WinGoldLeague] = true;
                    unlockStatus[UnlockCondition.WinSilverLeague] = true;
                    unlockStatus[UnlockCondition.WinBronzeLeague] = true;
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
                else if(survivor.wonSilverLeague)
                {
                    unlockStatus[UnlockCondition.WinSilverLeague] = true;
                    unlockStatus[UnlockCondition.WinBronzeLeague] = true;
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
                else if(survivor.wonBronzeLeague)
                {
                    unlockStatus[UnlockCondition.WinBronzeLeague] = true;
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
                else if(survivor.winCount + survivor.loseCount > 0)
                {
                    unlockStatus[UnlockCondition.FirstParticipateInBattleRoyale] = true;
                }
            }
        }
        foreach (var key in unlockStatus.Keys.ToList())
        {
            if (unlockStatus[key])
                Unlock(key);
        }
    }
}

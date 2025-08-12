using UnityEngine;
using Steamworks;

public static class AchievementManager
{
    public static void UnlockAchievement(string achievementId)
    {
        if (!SteamManager.Initialized) return;

        bool alreadyUnlocked;
        SteamUserStats.GetAchievement(achievementId, out alreadyUnlocked);

        if (!alreadyUnlocked)
        {
            SteamUserStats.SetAchievement(achievementId);
            SteamUserStats.StoreStats();
            Debug.Log($"업적 {achievementId} 달성!");
        }
    }

    public static void SetStat(string achievementId, int value)
    {
        if(!SteamManager.Initialized) return;
        SteamUserStats.SetStat(achievementId, value);
        SteamUserStats.StoreStats();
    }

    public static void SetStat(string achievementId, float value)
    {
        if (!SteamManager.Initialized) return;
        SteamUserStats.SetStat(achievementId, value);
        SteamUserStats.StoreStats();
    }

    public static bool GetStat(string achievementId, out int result)
    {
        if (!SteamManager.Initialized)
        {
            result = 0;
            return false;
        }
        SteamUserStats.GetStat(achievementId, out result);
        return true;
    }

    public static bool GetStat(string achievementId, out float result)
    {
        if (!SteamManager.Initialized)
        {
            result = 0;
            return false;
        }
        SteamUserStats.GetStat(achievementId, out result);
        return true;
    }
}

using UnityEngine;
using Steamworks;

public static class AcheivementManager
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
}

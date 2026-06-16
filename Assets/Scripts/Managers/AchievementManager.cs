using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

public class AchievementUIManager
{
    public enum UnlockElement { None, Characteristic, Training }
    public class AchievementInfo
    {
        public string achievementKey;
        public UnlockElement unlockElement;
        public string unlockElementName;
        public string statsKey;
        public int goalStat;

        public bool Unlocked
        {
            get
            {
                if (!SteamManager.Initialized) return false;
                SteamUserStats.GetAchievement(achievementKey, out bool unlocked);
                return unlocked;
            }
        }

        public int GetCurrentStat()
        {
            if (statsKey.Equals("") || !SteamManager.Initialized) return -1;
            SteamUserStats.GetStat(statsKey, out int curStat);
            return curStat;
        }

        public AchievementInfo(string achievementKey, UnlockElement unlockElement = UnlockElement.None, string unlockElementName = "", string statsKey = "", int goalStat = -1)
        {
            this.achievementKey = achievementKey;
            this.unlockElement = unlockElement;
            this.unlockElementName = unlockElementName;
            this.statsKey = statsKey;
            this.goalStat = goalStat;
        }
    }

    static List<AchievementInfo> achivementInfos = new();
    public static List<AchievementInfo> AchievementInfos => achivementInfos;

    public IEnumerator Initiate()
    {
        GameManager.ClaimLoadInfo("Loading Achievements...");
        achivementInfos.Add(new("Hundred-Thousandaire"));
        achivementInfos.Add(new("Royal Loader", UnlockElement.Characteristic, "Blessed"));
        achivementInfos.Add(new("Bronze Cup", UnlockElement.Characteristic, "BigMan"));
        achivementInfos.Add(new("Silver Cup", UnlockElement.Characteristic, "Powerhouse"));
        achivementInfos.Add(new("Gold Cup", UnlockElement.Characteristic, "Giant"));
        achivementInfos.Add(new("Season Champion", UnlockElement.Training, "Invite Season Champion"));
        achivementInfos.Add(new("World Champion", UnlockElement.Training, "Invite World Champion"));
        achivementInfos.Add(new("Melee Champion", UnlockElement.Training, "Invite Melee League Champion"));
        achivementInfos.Add(new("Shooting Champion", UnlockElement.Training, "Invite Shooting League Champion"));
        achivementInfos.Add(new("Crafting Champion", UnlockElement.Training, "Invite Crafting League Champion"));
        achivementInfos.Add(new("Bloody Hands", UnlockElement.Characteristic, "Avenger", "Total_Kill", 10));
        achivementInfos.Add(new("Bloody Arms"));
        achivementInfos.Add(new("Notorious", UnlockElement.Characteristic, "TasteOfBlood"));
        achivementInfos.Add(new("Tactician"));
        achivementInfos.Add(new("Legend", UnlockElement.Training, "Veteran Battle Royale Player Visit"));
        achivementInfos.Add(new("Vulture Victory"));
        achivementInfos.Add(new("Experience", UnlockElement.Characteristic, "UpsAndDowns"));
        achivementInfos.Add(new("Powerhouse", UnlockElement.Training, "Invite Strongman"));
        achivementInfos.Add(new("Quick-Footed", UnlockElement.Characteristic, "Assassin"));
        achivementInfos.Add(new("Martial Artist", UnlockElement.Training, "Invite MMA Champion"));
        achivementInfos.Add(new("Sharpshooter", UnlockElement.Characteristic, "QuickDrawer"));
        achivementInfos.Add(new("Genius", UnlockElement.Characteristic, "Genius"));
        achivementInfos.Add(new("Foreman", UnlockElement.Characteristic, "", "Total_Crafting", 100));
        achivementInfos.Add(new("Craftsman", UnlockElement.Characteristic, "Dexterous"));
        achivementInfos.Add(new("Sun Tzu", UnlockElement.Characteristic, "TrapExpert"));
        achivementInfos.Add(new("Overcome", UnlockElement.Characteristic, "ClutchPerformance"));
        achivementInfos.Add(new("1 hour", UnlockElement.Characteristic, "LuckGuy", "Total_SurvivalTime", 3600));
        achivementInfos.Add(new("Viper", UnlockElement.Characteristic, "PoisonImmune"));
        achivementInfos.Add(new("Bruce Lee", UnlockElement.Characteristic, "Fighter", "Total_BareHandKill", 30));
        achivementInfos.Add(new("Lethal Weapon", UnlockElement.Characteristic, "", "Total_MeleeKill", 30));
        achivementInfos.Add(new("Gunslinger", UnlockElement.Characteristic, "Sharpshooter", "Total_RangedKill", 30));
        achivementInfos.Add(new("Sword Master", UnlockElement.Characteristic, "SwordSaint", "Total_SowrdKill", 10));
        achivementInfos.Add(new("Sniper", UnlockElement.Characteristic, "Sniper", "Total_SniperKill", 10));
        achivementInfos.Add(new("Ace", UnlockElement.Training, "Invite Local Expert"));
        achivementInfos.Add(new("Severe Bleeding", UnlockElement.Characteristic, "Regenerator"));
        achivementInfos.Add(new("Augmented Prosthetic"));
        achivementInfos.Add(new("Transcendent Prosthetic"));
        achivementInfos.Add(new("Masterpiece"));
        yield return null;
    }
}
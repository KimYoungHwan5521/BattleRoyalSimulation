using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementManager
{
    public static List<string> earnedAchievementsInThisRun;

    public static void UnlockAchievement(string achievementId)
    {
        if (!SteamManager.Initialized) return;

        bool alreadyUnlocked;
        SteamUserStats.GetAchievement(achievementId, out alreadyUnlocked);

        if (!alreadyUnlocked)
        {
            SteamUserStats.SetAchievement(achievementId);
            SteamUserStats.StoreStats();
            if(GameManager.Instance.OutGameUIManager.GameMode == GameMode.SingleCareerRun) earnedAchievementsInThisRun.Add(achievementId);
            Debug.Log($"업적 {achievementId} 달성!");
        }
    }

    public static void SetStat(string achievementId, int value)
    {
        if(!SteamManager.Initialized) return;
        var achievement = AchievementUIManager.AchievementInfos.Find(x => x.achievementKey == achievementId);
        if (achievement != null && GameManager.Instance.OutGameUIManager.GameMode == GameMode.SingleCareerRun && achievement.Unlocked == false && value >= achievement.goalStat) earnedAchievementsInThisRun.Add(achievementId);
        bool success = SteamUserStats.SetStat(achievementId, value);
        SteamUserStats.StoreStats();
    }

    public static void SetStat(string achievementId, float value)
    {
        if (!SteamManager.Initialized) return;
        var achievement = AchievementUIManager.AchievementInfos.Find(x => x.achievementKey == achievementId);
        if (achievement != null && GameManager.Instance.OutGameUIManager.GameMode == GameMode.SingleCareerRun && achievement.Unlocked == false && value >= achievement.goalStat) earnedAchievementsInThisRun.Add(achievementId);
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
        public bool statIsInt;

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

        public float GetCurrentStatF()
        {
            if (statsKey.Equals("") || !SteamManager.Initialized) return -1;
            SteamUserStats.GetStat(statsKey, out float curStat);
            return curStat;
        }

        public AchievementInfo(string achievementKey, UnlockElement unlockElement = UnlockElement.None, string unlockElementName = "", string statsKey = "", int goalStat = -1, bool statIsInt = true)
        {
            this.achievementKey = achievementKey;
            this.unlockElement = unlockElement;
            this.unlockElementName = unlockElementName;
            this.statsKey = statsKey;
            this.goalStat = goalStat;
            this.statIsInt = statIsInt;
        }
    }

    static List<AchievementInfo> achivementInfos = new();
    public static List<AchievementInfo> AchievementInfos => achivementInfos;

    public IEnumerator Initiate()
    {
        // ※ Achievement / Training은 띄어쓰기 까지 일치 해야하고, Characterisstic은 띄어쓰기 붙여야함!!
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
        achivementInfos.Add(new("Bloody Arms", UnlockElement.Characteristic, "ScentofBlood", "Total_Kill", 100));
        achivementInfos.Add(new("Notorious", UnlockElement.Characteristic, "TasteOfBlood"));
        achivementInfos.Add(new("Tactician", UnlockElement.Characteristic, "DiceMan"));
        achivementInfos.Add(new("Legend", UnlockElement.Training, "Veteran Battle Royale Player Visit"));
        achivementInfos.Add(new("Vulture Victory", UnlockElement.Characteristic, "Coward"));
        achivementInfos.Add(new("Experience", UnlockElement.Characteristic, "UpsAndDowns"));
        achivementInfos.Add(new("Powerhouse", UnlockElement.Training, "Invite Strongman"));
        achivementInfos.Add(new("Quick-Footed", UnlockElement.Characteristic, "Assassin"));
        achivementInfos.Add(new("Martial Artist", UnlockElement.Training, "Invite MMA Champion"));
        achivementInfos.Add(new("Sharpshooter", UnlockElement.Characteristic, "QuickDrawer"));
        achivementInfos.Add(new("Genius", UnlockElement.Characteristic, "Genius"));
        achivementInfos.Add(new("Foreman", UnlockElement.Training, "Crafting Workshop", "Total_Crafting", 100));
        achivementInfos.Add(new("Craftsman", UnlockElement.Characteristic, "Dexterous"));
        achivementInfos.Add(new("Sun Tzu", UnlockElement.Characteristic, "TrapExpert"));
        achivementInfos.Add(new("Overcome", UnlockElement.Characteristic, "ClutchPerformance"));
        achivementInfos.Add(new("1 hour", UnlockElement.Characteristic, "LuckGuy", "Total_SurvivalTime", 3600, false));
        achivementInfos.Add(new("Viper", UnlockElement.Characteristic, "PoisonImmune"));
        achivementInfos.Add(new("Bruce Lee", UnlockElement.Characteristic, "StreetFighter", "Total_BareHandKill", 30));
        achivementInfos.Add(new("Lethal Weapon", UnlockElement.Characteristic, "LethalWeapon", "Total_MeleeKill", 30));
        achivementInfos.Add(new("Gunslinger", UnlockElement.Characteristic, "Sharpshooter", "Total_RangedKill", 30));
        achivementInfos.Add(new("Sword Master", UnlockElement.Characteristic, "SwordSaint", "Total_SowrdKill", 10));
        achivementInfos.Add(new("Sniper", UnlockElement.Characteristic, "Sniper", "Total_SniperKill", 10));
        achivementInfos.Add(new("Ace", UnlockElement.Training, "Invite Local Expert"));
        achivementInfos.Add(new("Pentakill", UnlockElement.Training, "Visit Hidden Master"));
        achivementInfos.Add(new("Severe Bleeding", UnlockElement.Characteristic, "Regenerator"));
        achivementInfos.Add(new("Augmented Prosthetic", UnlockElement.Characteristic, "BodyEnhancementAdvocate"));
        achivementInfos.Add(new("Transcendent Prosthetic", UnlockElement.Characteristic, "AugmentationFanatic"));
        achivementInfos.Add(new("Masterpiece"));
        // 2.0
        achivementInfos.Add(new("Engineer", UnlockElement.Characteristic, "Engineer"));
        achivementInfos.Add(new("Training Master", UnlockElement.Characteristic, "IronMan"));
        achivementInfos.Add(new("Discipline", UnlockElement.Characteristic, "TwoHearts", "Total_StaminaConsumption", 1000));
        achivementInfos.Add(new("Asceticism", UnlockElement.Characteristic, "ThreeHearts", "Total_StaminaConsumption", 5000));
        achivementInfos.Add(new("Rest", UnlockElement.Characteristic, "FastRecharge", "Total_StaminaRecovery", 1000));
        achivementInfos.Add(new("Hard", UnlockElement.Characteristic, "Prospect"));
        achivementInfos.Add(new("Very Hard", UnlockElement.Characteristic, "DarkHorse"));
        achivementInfos.Add(new("Expert", UnlockElement.Characteristic, "Potential"));
        achivementInfos.Add(new("Hardcore", UnlockElement.Characteristic, "Zombie"));
        achivementInfos.Add(new("Nightmare", UnlockElement.Characteristic, "ScentofBlood"));
        achivementInfos.Add(new("Hell", UnlockElement.Characteristic, "Challenger"));

        yield return null;
    }
}
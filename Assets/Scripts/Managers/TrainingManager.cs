using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public enum Stat { Strength = 0, Agility = 1, Fighting = 2, Shooting = 3, Crafting = 4, Knowledge = 5, Random = 6 }
public enum TrainingRarity { Common = 0, Uncommon = 1, Rare = 2 }
public class TrainingInfo
{
    public LocalizedString trainingName;
    public List<(int, int)> increaseStats;
    public TrainingRarity rarity;

    public TrainingInfo(string trainingKey, TrainingRarity rarity, params (int, int)[] increaseStats)
    {
        trainingName = new("Training", trainingKey);
        this.rarity = rarity;
        this.increaseStats = increaseStats.ToList();
    }
}

public class TrainingManager
{
    const float rareProbability = 1 / 16f;
    const float uncommonProbability = 1 / 4f;

    static List<TrainingInfo> trainings;
    public static List<TrainingInfo> Trainings => trainings;
    public IEnumerator Initiate()
    {
        GameManager.ClaimLoadInfo("Loading trainings...");
        trainings = new()
        {
            new("Strength Training", TrainingRarity.Common, (0, 3)),
            new("Running", TrainingRarity.Common, (1, 3)),
            new("Ammo Crate Carry", TrainingRarity.Common, (0, 2), (1, 1)),
            new("Loaded Run", TrainingRarity.Common, (1, 2), (0, 1)),
            new("Target Shooting", TrainingRarity.Common, (3, 3)),
            new("CQC Training", TrainingRarity.Common, (2, 2), (3, 1)),
            new("CQB Training", TrainingRarity.Common, (3, 2), (2, 1)),
            new("Machining", TrainingRarity.Common, (4, 3)),
            new("Mechanical Design", TrainingRarity.Common, (4, 2), (5, 1)),
            new("Military Logistics Engineering", TrainingRarity.Common, (5, 2), (4, 1)),
            new("Engineering", TrainingRarity.Common, (5, 3)),
            new("Judo", TrainingRarity.Common, (2, 1), (0, 1), (1, 1)),
            new("Special Operations Tactics", TrainingRarity.Common, (3, 1), (4, 1), (5, 1)),
            new("Boxing", TrainingRarity.Common, (2, 2), (1, 1)),
            new("Wrestling", TrainingRarity.Common, (0, 2), (2, 1)),
            new("MMA Training", TrainingRarity.Common, (2, 2), (0, 1)),
            new("Taekwondo", TrainingRarity.Common, (1, 2), (2, 1)),
            new("Individual Combat Training", TrainingRarity.Common, (1, 2), (3, 1)),

            new("Powerlifting", TrainingRarity.Uncommon, (0, 5)),
            new("Combat Engineering Training", TrainingRarity.Uncommon, (0, 3), (4, 2)),
            new("Ranger Training", TrainingRarity.Uncommon, (1, 3), (0, 2)),
            new("Electronic Engineering", TrainingRarity.Uncommon, (5, 4), (4, 1)),
            new("MMA Sparring", TrainingRarity.Uncommon, (2, 5)),

            new("Battle Royale Simulation", TrainingRarity.Rare, (3, 4), (2, 2), (4, 2)),
            new("Tactical Shooting Training", TrainingRarity.Rare, (3, 6), (1, 2)),
            new("Engineering Society", TrainingRarity.Rare, (5, 8)),

            new("Invite Local Expert", TrainingRarity.Common, (6, 5)),
            new("Visit Hidden Master", TrainingRarity.Uncommon, (6, 10)),
            new("Invite Strongman", TrainingRarity.Rare, (0, 8)),
            new("Invite MMA Champion", TrainingRarity.Rare, (2, 8)),
            new("Crafting Workshop", TrainingRarity.Rare, (4, 8)),
            new("Invite Melee League Champion", TrainingRarity.Uncommon, (2, 2), (0, 2), (1, 2)),
            new("Invite Shooting League Champion", TrainingRarity.Uncommon, (3, 6)),
            new("Invite Crafting League Champion", TrainingRarity.Uncommon, (4, 3), (5, 3)),
            new("Invite Season Champion", TrainingRarity.Uncommon, (0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1)),
            new("Invite World Champion", TrainingRarity.Rare, (0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2)),
            new("Veteran Battle Royale Player Visit", TrainingRarity.Rare, (6, 15))
        };
        yield return null;
    }

    public static TrainingInfo GetRandomTraining()
    {
        float rand = Random.Range(0, 1f);
        List<TrainingInfo> pool = new();
        if (rand < rareProbability) pool = trainings.FindAll(x => x.rarity == TrainingRarity.Rare);
        else if (rand < rareProbability + uncommonProbability) pool = trainings.FindAll(x => x.rarity == TrainingRarity.Uncommon);
        else pool = trainings.FindAll(x => x.rarity == TrainingRarity.Common);

        TrainingInfo training;
        for (int i = 0; i < 1000; i++)
        {
            training = pool[Random.Range(0, pool.Count)];
            if (UnlockCheck(training)) return training;
        }
        return trainings[0];
    }
    
    static bool UnlockCheck(TrainingInfo training)
    {
        bool result;
        switch(training.trainingName.TableEntryReference.Key)
        {
            case "Invite Local Expert": SteamUserStats.GetAchievement("Ace", out result); break;
            case "Visit Hidden Master": SteamUserStats.GetAchievement("Pentakill", out result); break;
            case "Invite Strongman": SteamUserStats.GetAchievement("Powerhouse", out result); break;
            case "Invite MMA Champion": SteamUserStats.GetAchievement("Martial Artist", out result); break;
            case "Crafting Workshop": SteamUserStats.GetAchievement("Foreman", out result); break;
            case "Invite Melee League Champion": SteamUserStats.GetAchievement("Melee Champion", out result); break;
            case "Invite Shooting League Champion": SteamUserStats.GetAchievement("Shooting Champion", out result); break;
            case "Invite Crafting League Champion": SteamUserStats.GetAchievement("Crafting Champion", out result); break;
            case "Invite Season Champion": SteamUserStats.GetAchievement("Season Champion", out result); break;
            case "Invite World Champion": SteamUserStats.GetAchievement("World Champion", out result); break;
            case "Veteran Battle Royale Player Visit": SteamUserStats.GetAchievement("Legend", out result); break;
            default: result = true; break;
        };
        return result;
    }
}

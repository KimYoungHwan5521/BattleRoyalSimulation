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
    public int staminaConsumtion;
    public int trainingDifficulty;
    public string GetTrainingExplain(bool greatSuccess)
    {
        string result = "";
        bool first = true;
        foreach (var value in increaseStats)
        {
            if (!string.IsNullOrEmpty(result)) result += ", ";
            result += value.Item1 switch
            {
                0 => new LocalizedString("Basic", "Strength").GetLocalizedString(),
                1 => new LocalizedString("Basic", "Agility").GetLocalizedString(),
                2 => new LocalizedString("Basic", "Fighting").GetLocalizedString(),
                3 => new LocalizedString("Basic", "Shooting").GetLocalizedString(),
                4 => new LocalizedString("Basic", "Crafting").GetLocalizedString(),
                5 => new LocalizedString("Basic", "Knowledge").GetLocalizedString(),
                6 => new LocalizedString("Basic", "Random").GetLocalizedString(),
                _ => new LocalizedString("Basic", "Strength").GetLocalizedString()
            };
            if (greatSuccess && first) 
            {
                int bonus = rarity == TrainingRarity.Common ? 1 : rarity == TrainingRarity.Uncommon ? 2 : 4;
                result += $" + {value.Item2 + bonus}";
            }
            else result += $" + {value.Item2}";
            first = false;
        }
        result += $"\n{new LocalizedString("Basic", "Stamina Cost") { Arguments = new[] { staminaConsumtion > 0 ? $"<color=red>-{staminaConsumtion}</color>" : $"<color=#367D38>+{-staminaConsumtion}</color>"} }.GetLocalizedString()}";
        return result;
    }

    public TrainingInfo(string trainingKey, TrainingRarity rarity, int staminaConsumtion, int trainingDifficulty, params (int, int)[] increaseStats)
    {
        trainingName = new("Training", trainingKey);
        this.rarity = rarity;
        this.staminaConsumtion = staminaConsumtion;
        this.trainingDifficulty = trainingDifficulty;
        this.increaseStats = increaseStats.ToList();
    }
}

public class TrainingManager
{
    const float rareProbability = 0.01f;
    const float uncommonProbability = 0.1f;

    static List<TrainingInfo> trainings;
    public static List<TrainingInfo> Trainings => trainings;
    public IEnumerator Initiate()
    {
        GameManager.ClaimLoadInfo("Loading trainings...");
        // ##스태미나 소비량, 난이도 공식
        // #레어 (나누기 8)
        // 스태미나 소비량
        // 힘, 민첩, 격투, 사격, 제작, 지식
        // 40, 37,   34,   31,   24,   -4
        // #언커먼 (나누기 5)
        // 30, 27,    24,   21,   15,  -8
        // #커먼 (나누기 3)
        // 25, 22     19,   16,   9,  -12
        // 난이도 (난이도는 그냥 평균값)
        // +20, +12, +16,  +8,   +4,   =16
        trainings = new()
        {
            new("Strength Training", TrainingRarity.Common, 25, 45, (0, 3)),
            new("Running", TrainingRarity.Common, 22, 34, (1, 3)),
            new("Ammo Crate Carry", TrainingRarity.Common, 24, 41, (0, 2), (1, 1)),
            new("Loaded Run", TrainingRarity.Common, 23, 38, (1, 2), (0, 1)),
            new("Target Shooting", TrainingRarity.Common, 16, 24, (3, 3)),
            new("CQC Training", TrainingRarity.Common, 18, 41, (2, 2), (3, 1)),
            new("CQB Training", TrainingRarity.Common, 17, 28, (3, 2), (2, 1)),
            new("Machining", TrainingRarity.Common, 9, 16, (4, 3)),
            new("Mechanical Design", TrainingRarity.Common, 2, 16, (4, 2), (5, 1)),
            new("Military Logistics Engineering", TrainingRarity.Common, -5, 16, (5, 2), (4, 1)),
            new("Engineering", TrainingRarity.Common, -12, 16, (5, 3)),
            new("Judo", TrainingRarity.Common, 23, 39, (2, 1), (0, 1), (1, 1)),
            new("Special Operations Tactics", TrainingRarity.Common, 4, 16, (3, 1), (4, 1), (5, 1)),
            new("Boxing", TrainingRarity.Common, 21, 36, (2, 2), (1, 1)),
            new("Wrestling", TrainingRarity.Common, 23, 42, (0, 2), (2, 1)),
            new("MMA Training", TrainingRarity.Common, 22, 39, (2, 2), (0, 1)),
            new("Taekwondo", TrainingRarity.Common, 22, 35, (1, 2), (2, 1)),
            new("Individual Combat Training", TrainingRarity.Common, 21, 32, (1, 2), (3, 1)),

            new("Powerlifting", TrainingRarity.Uncommon, 30, 50, (0, 5)),
            new("Combat Engineering Training", TrainingRarity.Uncommon, 24, 38, (0, 3), (4, 2)),
            new("Ranger Training", TrainingRarity.Uncommon, 28, 45, (1, 3), (0, 2)),
            new("Electronic Engineering", TrainingRarity.Uncommon, -3, 16, (5, 4), (4, 1)),
            new("MMA Sparring", TrainingRarity.Uncommon, 24, 40, (2, 5)),

            new("Battle Royale Simulation", TrainingRarity.Rare, 40, 60, (3, 4), (2, 2), (4, 2)),
            new("Tactical Shooting Training", TrainingRarity.Rare, 14, 20, (3, 6), (1, 2)),
            new("Engineering Society", TrainingRarity.Rare, -4, 16, (5, 8)),

            new("Invite Local Expert", TrainingRarity.Common, 25, 45, (6, 5)),
            new("Visit Hidden Master", TrainingRarity.Uncommon, 30, 50, (6, 10)),
            new("Invite Strongman", TrainingRarity.Rare, 40, 60, (0, 8)),
            new("Invite MMA Champion", TrainingRarity.Rare, 34, 50, (2, 8)),
            new("Crafting Workshop", TrainingRarity.Rare, 24, 32, (4, 8)),
            new("Invite Melee League Champion", TrainingRarity.Uncommon, 32, 48, (2, 2), (0, 2), (1, 2)),
            new("Invite Shooting League Champion", TrainingRarity.Uncommon, 25, 33, (3, 6)),
            new("Invite Crafting League Champion", TrainingRarity.Uncommon, 1, 16, (4, 3), (5, 3)),
            new("Invite Season Champion", TrainingRarity.Uncommon, 22, 34, (0, 1), (1, 1), (2, 1), (3, 1), (4, 1), (5, 1)),
            new("Invite World Champion", TrainingRarity.Rare, 40, 52, (0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2)),
            new("Veteran Battle Royale Player Visit", TrainingRarity.Rare, 40, 60, (6, 15))
        };
        yield return null;
    }

    public static TrainingInfo GetRandomTraining(int traiingLevel)
    {
        float rand = Random.Range(0, 1f);
        List<TrainingInfo> pool = new();
        if (rand < rareProbability * traiingLevel) pool = trainings.FindAll(x => x.rarity == TrainingRarity.Rare);
        else if (rand < (rareProbability + uncommonProbability) * traiingLevel) pool = trainings.FindAll(x => x.rarity == TrainingRarity.Uncommon);
        else pool = trainings.FindAll(x => x.rarity == TrainingRarity.Common);

        TrainingInfo training;
        for (int i = 0; i < 1000; i++)
        {
            training = pool[Random.Range(0, pool.Count)];
            if (UnlockCheck(training)) return training;
        }
        return trainings[0];
    }
    
    public static bool UnlockCheck(TrainingInfo training)
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

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingRoom : MonoBehaviour
{
    [SerializeField] GameObject trainingRoom;
    [SerializeField] GameObject trainingAssignForm;
    [SerializeField] GameObject scheduledTrainingByEachSurvivor;
    [SerializeField] GameObject[] scheduledTrainings;
    [SerializeField] TextMeshProUGUI weightTrainingNameText;
    [SerializeField] TextMeshProUGUI runningNameText;
    [SerializeField] TextMeshProUGUI fightTrainingNameText;
    [SerializeField] TextMeshProUGUI shootingTraningNameText;
    [SerializeField] TextMeshProUGUI studyingNameText;
    [SerializeField] TextMeshProUGUI weightTrainingExplain;
    [SerializeField] TextMeshProUGUI runningExplain;
    [SerializeField] TextMeshProUGUI fightingTrainingExplain;
    [SerializeField] TextMeshProUGUI shootingTrainingExplain;
    [SerializeField] TextMeshProUGUI studyExplain;
    [SerializeField] int fightTrainingLevel = 1;
    [SerializeField] int shootingTrainingLevel = 1;
    [SerializeField] int runningLevel = 1;
    [SerializeField] int weightTrainingLevel = 1;
    [SerializeField] int studyingLevel = 1;

    public int FightTrainingLevel => fightTrainingLevel;
    public int ShootingTrainingLevel => shootingTrainingLevel;
    public int AgilityTrainingLevel => runningLevel;
    public int WeightTrainingLevel => weightTrainingLevel;
    public int StudyLevel => studyingLevel;
    readonly int[] facilityUpgradeCost = { 5000, 12000, 30000 };
    [SerializeField] GameObject fightTrainingUpgradeButtion;
    [SerializeField] GameObject shootingTrainingUpgradeButtion;
    [SerializeField] GameObject runningUpgradeButtion;
    [SerializeField] GameObject weightTrainingUpgradeButtion;
    [SerializeField] GameObject studyingUpgradeButtion;
    [SerializeField] ScrollRect[] bookedTodayScrollRects;
    [SerializeField] TextMeshProUGUI weightTrainingBookers;
    [SerializeField] TextMeshProUGUI runningBookers;
    [SerializeField] TextMeshProUGUI fightTrainingBookers;
    [SerializeField] TextMeshProUGUI shootingTrainingBookers;
    [SerializeField] TextMeshProUGUI studyingBookers;

    [SerializeField] TextMeshProUGUI assignTrainingNameText;
    [SerializeField] Transform survivorsAssignedThis;
    [SerializeField] Transform survivorsWithoutSchedule;
    [SerializeField] Transform survivorsWithOtherSchedule;
    List<SurvivorSchedule> survivorSchedules = new();
    bool autoAssign = true;
    [SerializeField] GameObject autoAssignCheckBox;
}

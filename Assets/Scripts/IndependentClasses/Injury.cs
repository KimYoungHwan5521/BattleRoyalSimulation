using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InjurySiteMajor { Head, Torso, Arms, Legs }
public enum InjurySite
{
    None,
    // Head
    Head, RightEye, LeftEye, RightEar, LeftEar, Cheek, Nose, Jaw, Skull, Brain, Neck,

    // Torso
    Chest, Ribs, Abdomen, Organ,

    // Arms
    RightArm, LeftArm, RightHand, LeftHand, RightThumb, RightIndexFinger, RightMiddleFinger, RightRingFinger,
    RightLittleFinger, LeftThumb, LeftIndexFinger, LeftMiddleFinger, LeftRingFinger, LeftLittleFinger,

    // Legs
    RightLeg, LeftLeg, RightKnee, LeftKnee, RightFoot, LeftFoot, RightBigToe, LeftBigToe, RightIndexToe, LeftIndexToe,
    RightMiddleToe, LeftMiddleToe, RightRingToe, LeftRingToe, RightLittleToe, LeftLittleToe,
}
public enum InjuryType
{
    Contusion, // 타박상
    Fracture, // 골절
    Cutting, // 잘림/베임
    Amputation, // 절단
    GunshotWound, // 총상
    Damage, // 손상
    Rupture, // 파열
    Loss, // 손실
    Concussion, // 뇌진탕
    Burn, // 화상
    PermanentVisualImpairment,
    
    RecoveringFromSurgery,
    ArtificialPartsTransplanted,
    ArtificialPartsDamaged,
    AugmentedPartsTransplanted,
    AugmentedPartsDamaged,
    TranscendantPartsTransplanted,
    TranscendantPartsDamaged,
}

[Serializable]
public class Injury
{
    public InjurySite site;
    public InjuryType type;
    // 0: 완치, 1: 완전 손실(Loss)
    [Range(0, 1)] public float degree;

    public Injury(InjurySite site, InjuryType type, float degree)
    {
        this.site = site;
        this.type = type;
        this.degree = degree;
    }

    public static List<InjurySite> GetUpperParts(InjurySite subpart)
    {
        List<InjurySite> result = new();
        switch (subpart)
        {
            case InjurySite.RightThumb:
            case InjurySite.RightIndexFinger:
            case InjurySite.RightMiddleFinger:
            case InjurySite.RightRingFinger:
            case InjurySite.RightLittleFinger:
                result.Add(InjurySite.RightHand);
                result.Add(InjurySite.RightArm);
                break;
            case InjurySite.LeftThumb:
            case InjurySite.LeftIndexFinger:
            case InjurySite.LeftMiddleFinger:
            case InjurySite.LeftRingFinger:
            case InjurySite.LeftLittleFinger:
                result.Add(InjurySite.LeftHand);
                result.Add(InjurySite.LeftArm);
                break;
            case InjurySite.RightHand:
                result.Add(InjurySite.RightArm);
                break;
            case InjurySite.LeftHand:
                result.Add(InjurySite.LeftArm);
                break;
            case InjurySite.RightBigToe:
            case InjurySite.RightIndexToe:
            case InjurySite.RightMiddleToe:
            case InjurySite.RightRingToe:
            case InjurySite.RightLittleToe:
                result.Add(InjurySite.RightFoot);
                result.Add(InjurySite.RightKnee);
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftBigToe:
            case InjurySite.LeftIndexToe:
            case InjurySite.LeftMiddleToe:
            case InjurySite.LeftRingToe:
            case InjurySite.LeftLittleToe:
                result.Add(InjurySite.LeftFoot);
                result.Add(InjurySite.LeftKnee);
                result.Add(InjurySite.LeftLeg);
                break;
            case InjurySite.RightFoot:
                result.Add(InjurySite.RightKnee);
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftFoot:
                result.Add(InjurySite.LeftKnee);
                result.Add(InjurySite.LeftLeg);
                break;
            case InjurySite.RightKnee:
                result.Add(InjurySite.RightLeg);
                break;
            case InjurySite.LeftKnee:
                result.Add(InjurySite.LeftLeg);
                break;
            default:
                break;
        }
        return result;
    }
    public static List<InjurySite> GetSubparts(InjurySite upperPart)
    {
        List<InjurySite> result = new();
        if (upperPart == InjurySite.RightArm || upperPart == InjurySite.RightHand)
        {
            result.Add(InjurySite.RightThumb);
            result.Add(InjurySite.RightIndexFinger);
            result.Add(InjurySite.RightMiddleFinger);
            result.Add(InjurySite.RightRingFinger);
            result.Add(InjurySite.RightLittleFinger);
            if (upperPart == InjurySite.RightArm)
            {
                result.Add(InjurySite.RightHand);
            }
        }
        else if (upperPart == InjurySite.LeftArm || upperPart == InjurySite.LeftHand)
        {
            result.Add(InjurySite.LeftThumb);
            result.Add(InjurySite.LeftIndexFinger);
            result.Add(InjurySite.LeftMiddleFinger);
            result.Add(InjurySite.LeftRingFinger);
            result.Add(InjurySite.LeftLittleFinger);
            if (upperPart == InjurySite.LeftArm)
            {
                result.Add(InjurySite.LeftHand);
            }
        }
        else if (upperPart == InjurySite.RightLeg || upperPart == InjurySite.RightKnee || upperPart == InjurySite.RightFoot)
        {
            result.Add(InjurySite.RightBigToe);
            result.Add(InjurySite.RightIndexToe);
            result.Add(InjurySite.RightMiddleToe);
            result.Add(InjurySite.RightRingToe);
            result.Add(InjurySite.RightLittleToe);
            if (upperPart == InjurySite.RightLeg || upperPart == InjurySite.RightKnee)
            {
                result.Add(InjurySite.RightFoot);
                if (upperPart == InjurySite.RightLeg) result.Add(InjurySite.RightKnee);
            }
        }
        else if (upperPart == InjurySite.LeftLeg || upperPart == InjurySite.LeftKnee || upperPart == InjurySite.LeftFoot)
        {
            result.Add(InjurySite.LeftBigToe);
            result.Add(InjurySite.LeftIndexToe);
            result.Add(InjurySite.LeftMiddleToe);
            result.Add(InjurySite.LeftRingToe);
            result.Add(InjurySite.LeftLittleToe);
            if (upperPart == InjurySite.LeftLeg || upperPart == InjurySite.LeftKnee)
            {
                result.Add(InjurySite.LeftFoot);
                if (upperPart == InjurySite.LeftLeg) result.Add(InjurySite.LeftKnee);
            }
        }

        return result;
    }

}

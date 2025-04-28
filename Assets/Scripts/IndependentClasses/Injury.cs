using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InjurySiteMajor { Head, Torso, Arms, Legs }
public enum InjurySite
{
    None,
    // Head
    Head, RightEye, LeftEye, RightEar, LeftEar, Nose, Jaw, Skull, Brain,

    // Torso
    Chest, Libs, Abdomen, Organ,

    // Arms
    RightArm, LeftArm, RightHand, LeftHand, RightThumb, RightIndexFinger, RightMiddleFinger, RightRingFinger,
    RightLittleFinger, LeftThumb, LeftIndexFinger, LeftMiddleFinger, LeftRingFinger, LeftLittleFinger,

    // Legs
    RightLeg, LeftLeg, RightKnee, LeftKnee, RightAncle, LeftAncle, RightBigToe, LeftBigToe,
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
    RecoveringFromSurgery,
    ArtificalPartsTransplanted,
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
}

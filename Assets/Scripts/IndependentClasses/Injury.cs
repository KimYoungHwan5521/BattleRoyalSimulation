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
    Contusion, // Ÿ�ڻ�
    Fracture, // ����
    Cutting, // �߸�/����
    Amputation, // ����
    GunshotWound, // �ѻ�
    Damage, // �ջ�
    Rupture, // �Ŀ�
    Loss, // �ս�
    Concussion, // ������
    Burn, // ȭ��
    PermanentVisualImpairment,
    RecoveringFromSurgery,
    ArtificialPartsTransplanted,
}

[Serializable]
public class Injury
{
    public InjurySite site;
    public InjuryType type;
    // 0: ��ġ, 1: ���� �ս�(Loss)
    [Range(0, 1)] public float degree;

    public Injury(InjurySite site, InjuryType type, float degree)
    {
        this.site = site;
        this.type = type;
        this.degree = degree;
    }
}

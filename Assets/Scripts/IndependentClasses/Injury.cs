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
    RecoveringFromSurgery,
    ArtificalPartsTransplanted,
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

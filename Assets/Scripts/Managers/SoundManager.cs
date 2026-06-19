using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager
{
    AudioMixer currentMixer;
    AudioMixerGroup AMGmaster;
    AudioMixerGroup AMGbgm;
    AudioMixerGroup AMGsfx;

    AudioSource[] bgmArray = new AudioSource[2];
    const int sfxMaxNumber = 30;
    AudioSource[] sfxArray = new AudioSource[sfxMaxNumber];
    Queue<AudioSource> sfxQueue = new();

    Action AudioEffectUpdate;
    public IEnumerator Initiate()
    {
        currentMixer = ResourceManager.Mixer;
        if (currentMixer == null) Debug.LogWarning("AudioMixer not found!");
        AMGmaster = currentMixer.FindMatchingGroups("Master")[0];
        AMGbgm = currentMixer.FindMatchingGroups("BGM")[0];
        AMGsfx = currentMixer.FindMatchingGroups("SFX")[0];
        if (AMGmaster == null) Debug.LogWarning("AudioMixerGroup Master not found!");
        if (AMGbgm == null) Debug.LogWarning("AudioMixerGroup BGM not found!");
        if (AMGsfx == null) Debug.LogWarning("AudioMixerGroup SFX not found!");

        Transform soundContainer = new GameObject("Sound Container").transform;
        soundContainer.SetParent(GameManager.Instance.transform);
        // BGM 교체할 때 페이드인/아웃 하기위해 AudioSource를 2개 준비
        GameObject bgmCarrier = new("BGM Carrier", typeof(AudioSource), typeof(AudioSource));
        bgmCarrier.transform.SetParent(soundContainer);
        bgmArray = bgmCarrier.GetComponents<AudioSource>();

        for (int i = 0; i < bgmArray.Length; i++)
        {
            bgmArray[i].outputAudioMixerGroup = AMGbgm;
            bgmArray[i].loop = true;
            bgmArray[i].playOnAwake = false;
            // BGM은 거리 상관 없으므로
            bgmArray[i].maxDistance = float.MaxValue;
            bgmArray[i].minDistance = float.MaxValue;
        }

        for (int i = 0; i < sfxMaxNumber; i++)
        {
            GameObject sfxCarrier = new("SFX Carrier", typeof(AudioSource));
            sfxCarrier.transform.SetParent(soundContainer);
            AudioSource currentSource = sfxCarrier.GetComponent<AudioSource>();
            currentSource.outputAudioMixerGroup = AMGsfx;
            currentSource.playOnAwake = false;
            currentSource.spatialBlend = 1;
            sfxArray[i] = currentSource;
            sfxQueue.Enqueue(currentSource);
        }

        GameManager.Instance.ManagerUpdate += SoundManagerUpdate;
        yield return null;
    }

    void SoundManagerUpdate()
    {
        AudioEffectUpdate?.Invoke();
    }

    public void UpdateBGMMixer()
    {
        bgmArray[0].volume = Mathf.SmoothStep(bgmArray[0].volume, 1, Time.unscaledTime * 5);
        bgmArray[1].volume = Mathf.SmoothStep(bgmArray[1].volume, 0, Time.unscaledTime * 5);
        if (bgmArray[0].volume > 0.99)
        {
            AudioEffectUpdate -= UpdateBGMMixer;
        }
    }

    public static void Play(ResourceEnum.BGM wantBGM)
    {
        // 0 : 플레이 할 브금
        // 1 : 현재 플레이 중인 브금
        // 현재 0번을 1번으로 보내고 플레이 할 브금을 0에 넣기
        SoundManager soundManager = GameManager.Instance.SoundManager;
        soundManager.bgmArray[1].clip = soundManager.bgmArray[0].clip;
        soundManager.bgmArray[1].time = soundManager.bgmArray[0].time;
        soundManager.bgmArray[1].volume = soundManager.bgmArray[0].volume;

        soundManager.bgmArray[0].clip = ResourceManager.Get(wantBGM);
        soundManager.bgmArray[0].time = 0;
        soundManager.bgmArray[0].volume = 0;
        soundManager.bgmArray[0].Play();

        soundManager.AudioEffectUpdate -= soundManager.UpdateBGMMixer;
        soundManager.AudioEffectUpdate += soundManager.UpdateBGMMixer;
    }

    public static void Play(ResourceEnum.SFX wantSFX, Vector3 soundOrigin, bool loop = false)
    {
        Play(wantSFX, soundOrigin, loop, out AudioSource _);
    }

    public static void Play(ResourceEnum.SFX wantSFX, Vector3 soundOrigin, bool loop, out AudioSource source, bool is2D = false)
    {
        SoundManager soundManager = GameManager.Instance.SoundManager;
        AudioClip clip = ResourceManager.Get(wantSFX);
        if (soundManager.sfxQueue.TryDequeue(out AudioSource currentSource))
        {
            currentSource.clip = clip;
            currentSource.loop = loop;
            currentSource.spatialBlend = is2D ? 0 : 1;
            currentSource.transform.position = soundOrigin;
            currentSource.Play();
            source = currentSource;
            if (!loop) soundManager.sfxQueue.Enqueue(currentSource);
        }
        else source = null;
    }

    public static void PlayUISFX(ResourceEnum.SFX wantSFX)
    {
        Play(wantSFX, Vector3.zero, false, out AudioSource _, true);
    }

    public void Enqueue(AudioSource audioSource)
    {
        GameManager.Instance.SoundManager.sfxQueue.Enqueue(audioSource);
    }

    public static void StopBGM()
    {
        foreach (var bgm in GameManager.Instance.SoundManager.bgmArray)
        {
            bgm.Stop();
        }
    }

    public static void StopSFX(AudioSource source)
    {
        source.Stop();
        GameManager.Instance.SoundManager.sfxQueue.Enqueue(source);
    }

    public enum AudioMixerGroupType
    {
        Master, BGM, SFX
    }
    public void ToggleAudioMixerGroup(AudioMixerGroupType type, bool toggle, float value)
    {
        // 슬라이더 0~1 → dB
        float dB = Mathf.Log10(Mathf.Max(value, 0.001f)) * 20f;
        switch (type)
        {
            case AudioMixerGroupType.Master:
                AMGmaster.audioMixer.SetFloat("Master", toggle ? dB : -80);
                break;
            case AudioMixerGroupType.BGM:
                AMGmaster.audioMixer.SetFloat("BGM", toggle ? dB : -80);
                break;
            case AudioMixerGroupType.SFX:
                AMGmaster.audioMixer.SetFloat("SFX", toggle ? dB : -80);
                break;
        }
    }

    public void PitchShift(float wantPitch)
    {
        //foreach (var audioSource in GameManager.Instance.SoundManager.bgmArray) audioSource.pitch = wantPitch;
        foreach (var audioSource in GameManager.Instance.SoundManager.sfxArray)
        {
            audioSource.pitch = wantPitch > 0 ? wantPitch : 1;
            if (wantPitch > 0) currentMixer.SetFloat("SFX_Pitch", 1 / wantPitch);
            else currentMixer.SetFloat("SFX_Pitch", 1);
        }
    }
}

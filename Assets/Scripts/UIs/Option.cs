using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] Image bgmImage;
    [SerializeField] Image sfxImage;
    [SerializeField] Sprite on;
    [SerializeField] Sprite off;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;
    bool bgmOn = true;
    bool sfxOn = true;

    public void ToggleBGM()
    {
        bgmOn = !bgmOn;
        bgmImage.sprite = bgmOn ? on : off;
        bgmSlider.interactable = bgmOn;
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.BGM, bgmOn, bgmSlider.value);
    }

    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        sfxImage.sprite = sfxOn ? on : off;
        sfxSlider.interactable = sfxOn;
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.SFX, sfxOn, sfxSlider.value);
    }

    public void SlideBGM()
    {
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.BGM, true, bgmSlider.value);
    }

    public void SlideSFX()
    {
        GameManager.Instance.SoundManager.ToggleAudioMixerGroup(SoundManager.AudioMixerGroupType.SFX, true, sfxSlider.value);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour, IDataPersistence
{
    public static SoundMixerManager Instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] public Slider masterSlider;
    [SerializeField] public Slider sfxSlider;
    [SerializeField] public Slider musicSlider;

    public void LoadData(GameData data)
    {
        //GameAudio
        SoundMixerManager.Instance.SetMasterVolume(data.masterVolume);
        SoundMixerManager.Instance.SetMasterVolume(data.sfxVolume);
        SoundMixerManager.Instance.SetMasterVolume(data.musicVolume);
    }

    public void SaveData(GameData data)
    {
        //GameAudio
        data.masterVolume = SoundMixerManager.Instance.masterSlider.value;
        data.sfxVolume = SoundMixerManager.Instance.sfxSlider.value;
        data.musicVolume = SoundMixerManager.Instance.musicSlider.value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        float masterValue;
        audioMixer.GetFloat("masterVolume", out masterValue);
        masterSlider.value = Mathf.Pow(10, masterValue / 20f);

        float sfxValue;
        audioMixer.GetFloat("sfxVolume", out sfxValue);
        sfxSlider.value = Mathf.Pow(10, sfxValue / 20f);

        float musicValue;
        audioMixer.GetFloat("musicVolume", out musicValue);
        musicSlider.value = Mathf.Pow(10, musicValue / 20f);
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }
}

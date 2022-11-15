using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SoundManager : MonoBehaviour {
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private VideoPlayer videoVolumeSlider;
    void ChangeVolume() => AudioListener.volume = volumeSlider.value;

    private void Awake() {
        AudioListener.volume = PlayerPrefs.GetFloat("volume");
        volumeSlider.onValueChanged.AddListener(delegate { ChangeVolume(); });
        volumeSlider.value = AudioListener.volume;
    }

    private void Update() {
        if (videoVolumeSlider.GetDirectAudioVolume(0) != AudioListener.volume)
            videoVolumeSlider.SetDirectAudioVolume(0, AudioListener.volume);
    }


    public void ChangeVolume(float newVolume) {
        PlayerPrefs.SetFloat("volume", newVolume);
        AudioListener.volume = newVolume;
    }

    public void OnClick() {
        if (PlayerPrefs.GetFloat("volume") != AudioListener.volume)
            PlayerPrefs.SetFloat("volume", AudioListener.volume);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance;

    [SerializeField] private Slider volumeSlider;                       // Sound slider on menu
    [SerializeField] private VideoPlayer videoVolumeSlider;             // Video volume
    void ChangeVolume() => AudioListener.volume = volumeSlider.value;

    private bool isFading;                                              // Whether we're currently fading out the video volume

    private void Awake() {
        if (instance == null)
            instance = this;

        /* Update volume of game and slider depending on previously saved volume */
        AudioListener.volume = PlayerPrefs.GetFloat("volume");
        volumeSlider.onValueChanged.AddListener(delegate { ChangeVolume(); });
        volumeSlider.value = AudioListener.volume;
    }

    private void Update() {
        /* Always sync video volume to sound slider unless we're currently fading out (song change or restart) */
        if (!isFading && videoVolumeSlider.GetDirectAudioVolume(0) != AudioListener.volume)
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

    /// <summary>
    /// Slowly fades out the song to 0 volume
    /// </summary>
    /// <param name="time"></param> The amount of seconds the fading will take to reach 0 from any starting value
    /// <returns></returns>
    public IEnumerator FadeOutVideoVolume(float time) {
        isFading = true;
        float startVolume = videoVolumeSlider.GetDirectAudioVolume(0);
        while (videoVolumeSlider.GetDirectAudioVolume(0) > 0) {
            videoVolumeSlider.SetDirectAudioVolume(0, videoVolumeSlider.GetDirectAudioVolume(0) - (Time.deltaTime / time) * startVolume);
            yield return null;
        }
        isFading = false;
        yield return 0;
    }
}
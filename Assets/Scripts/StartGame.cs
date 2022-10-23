using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartGame : MonoBehaviour {
    [SerializeField] private GameObject CODA_App;
    [SerializeField] private GameObject CODA_Icon;
    [SerializeField] private GameObject game_Video;
    [SerializeField] private GameObject no_Watermark;
    [SerializeField] private GameObject start_Wallpaper;
    [SerializeField] private GameObject accuracy_Text;
    [SerializeField] private GameObject taskbarGlow;
    [SerializeField] private GameObject taskbarUnglow;
    [SerializeField] private GameObject instructions;

    private float fadeInFactor = 0.5f;
    private bool isFading;

    private LyricGenerator lyricGenerator;
    private void Start() {
        lyricGenerator = gameObject.GetComponent<LyricGenerator>();
        game_Video.GetComponent<VideoPlayer>().targetCameraAlpha = 0f;
        lyricGenerator.enabled = false;
        start_Wallpaper.SetActive(true);
    }

    private void Update() {
        if (isFading) {
            game_Video.GetComponent<VideoPlayer>().targetCameraAlpha += fadeInFactor * Time.deltaTime;
            no_Watermark.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, fadeInFactor * 12f * Time.deltaTime);
            taskbarGlow.GetComponent<Image>().color += new Color(0, 0, 0, fadeInFactor * Time.deltaTime);
            if (game_Video.GetComponent<VideoPlayer>().targetCameraAlpha >= 1f) {
                game_Video.GetComponent<VideoPlayer>().targetCameraAlpha = 1f;
                taskbarUnglow.SetActive(false);
                isFading = false;
            }
        }
    }

    public void ShowCODA_App() {
        start_Wallpaper.SetActive(false);
        CODA_Icon.SetActive(false);
        CODA_App.SetActive(true);
        instructions.SetActive(false);
        taskbarGlow.SetActive(true);
        game_Video.SetActive(true);
        accuracy_Text.SetActive(true);
        lyricGenerator.enabled = true;
        isFading = true;
    }

    public void HideCODA_App() {
        start_Wallpaper.SetActive(true);
        CODA_Icon.SetActive(true);
        CODA_App.SetActive(false);
        instructions.SetActive(true);
        taskbarGlow.SetActive(false);
        taskbarUnglow.SetActive(true);
        game_Video.SetActive(false);
        accuracy_Text.SetActive(false);
        lyricGenerator.enabled = false;
    }
}
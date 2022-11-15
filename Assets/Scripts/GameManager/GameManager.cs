using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    [SerializeField] private GameObject CODA_App;
    [SerializeField] private GameObject CODA_Icon;
    [SerializeField] private GameObject game_Video;
    [SerializeField] private GameObject no_Watermark;
    [SerializeField] private GameObject start_Wallpaper;
    [SerializeField] private GameObject accuracy_Text;
    [SerializeField] private GameObject taskbarGlow;
    [SerializeField] private GameObject taskbarUnglow;
    [SerializeField] private GameObject instructions;
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject windowsFade;
    [SerializeField] private GameObject playlist;
    [SerializeField] private GameObject pausePanel;

    public bool isPlaying;

    private LyricGenerator lyricGenerator;

    private void Awake() {
        if (instance = null)
            instance = this;
    }

    private void Start() {
        lyricGenerator = gameObject.GetComponent<LyricGenerator>();
        lyricGenerator.enabled = false;
        game_Video.SetActive(false);
        lyricGenerator.enabled = false;
        start_Wallpaper.SetActive(true);
        windowsFade.SetActive(true);
        isPlaying = false;

        CSVReader.instance.ReadCSV();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0))
            AudioManager.instance.Play("ButtonClick");
    }

    public void ShowCODA_App() {
        playlist.SetActive(true);
        PauseGame();
    }

    public void SetupSong() {
        lyricGenerator.enabled = true;
        game_Video.SetActive(true);
        game_Video.GetComponent<VideoPlayer>().clip = SongManager.instance.songs[SongManager.instance.currentSongIdx].videoClip;

        start_Wallpaper.SetActive(false);
        CODA_Icon.SetActive(false);
        CODA_App.SetActive(true);
        instructions.SetActive(false);
        title.SetActive(false);
        taskbarGlow.SetActive(true);
        accuracy_Text.SetActive(true);
        lyricGenerator.enabled = true;

        isPlaying = true;
    }

    public void HideCODA_App() {
        start_Wallpaper.SetActive(true);
        CODA_Icon.SetActive(true);
        CODA_App.SetActive(false);
        instructions.SetActive(true);
        title.SetActive(true);
        taskbarGlow.SetActive(false);
        taskbarUnglow.SetActive(true);
        game_Video.SetActive(false);
        accuracy_Text.SetActive(false);
        lyricGenerator.enabled = false;

        isPlaying = false;
    }

    public void PauseGame() {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        game_Video.GetComponent<VideoPlayer>().Pause();
        pausePanel.SetActive(true);
    }

    public void ResumeGame() {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (isPlaying)
            game_Video.GetComponent<VideoPlayer>().Play();
        pausePanel.SetActive(false);

        if (SongManager.instance.currentSongIdx != SongManager.instance.pausedSongSelectionIdx) {
            SongManager.instance.currentSongIdx = SongManager.instance.pausedSongSelectionIdx;
            CSVReader.instance.ReadCSV();
            if (isPlaying)
                StartCoroutine(ChangeSong());
        } 
        if (!isPlaying)
            SetupSong();
    }

    public void ResetGame() {
        LyricGenerator.instance.ResetStats();
        Symbol.UpdateSpeed();
    }

    public void RestartSong() {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (isPlaying)
            game_Video.GetComponent<VideoPlayer>().Play();
        pausePanel.SetActive(false);
        StartCoroutine(ChangeSong());
    }

    public void FadeIn() {
        windowsFade.GetComponent<Animator>().SetTrigger("fadeIn");
        game_Video.GetComponent<Animator>().SetTrigger("fadeOut");
    }

    public void FadeOut() {
        windowsFade.GetComponent<Animator>().SetTrigger("fastFadeOut");
        game_Video.GetComponent<Animator>().SetTrigger("fadeIn");
    }

    public IEnumerator ChangeSong() {
        FadeIn();
        lyricGenerator.enabled = false;
        yield return new WaitForSeconds(3.5f);
        lyricGenerator.enabled = true;
        game_Video.GetComponent<VideoPlayer>().clip = null;
        game_Video.GetComponent<VideoPlayer>().clip = SongManager.instance.songs[SongManager.instance.currentSongIdx].videoClip;
        ResetGame();
        FadeOut();
    }
}
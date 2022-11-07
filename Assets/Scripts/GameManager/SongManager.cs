using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour {
    public static SongManager instance;

    public SongCreator[] songs; 
    public int currentSongIdx;

    [SerializeField] private Image cover;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI singers;
    [SerializeField] private TextMeshProUGUI difficulty;
    [SerializeField] private TextMeshProUGUI length;

    private void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        currentSongIdx = 0;
        UpdateDescription();
    }

    public void NextSong() {
        currentSongIdx = (currentSongIdx + 1) % songs.Length;
        UpdateDescription();
    }

    public void PreviousSong() {
        currentSongIdx = (currentSongIdx - 1) % songs.Length;
        if (currentSongIdx < 0)
            currentSongIdx = songs.Length - 1;
        UpdateDescription();
    }

    private void UpdateDescription() {
        cover.sprite = songs[currentSongIdx].cover;
        title.text = songs[currentSongIdx].title; 
        singers.text = songs[currentSongIdx].singer;
        difficulty.text = songs[currentSongIdx].difficulty;
        length.text = CalculateLength(songs[currentSongIdx].videoClip.length);
        CSVReader.instance.ReadCSV();
    }

    private string CalculateLength(double timeInSeconds) {
        int minutes = (int) (timeInSeconds / 60F);
        int seconds = (int) (timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
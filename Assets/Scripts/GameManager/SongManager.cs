using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour {
    public static SongManager instance;

    public SongCreator[] songs; 
    public int currentSongIdx;
    public int pausedSongSelectionIdx;

    [SerializeField] private Image cover;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI singers;
    [SerializeField] private TextMeshProUGUI difficulty;
    [SerializeField] private TextMeshProUGUI length;

    private void Awake() {
        if (instance == null)
            instance = this;
    }

    private void Start() {
        currentSongIdx = 0;
        pausedSongSelectionIdx = currentSongIdx;
    }

    public void NextSong() {
        pausedSongSelectionIdx = (pausedSongSelectionIdx + 1) % songs.Length;
        UpdateDescription();
    }

    public void PreviousSong() {
        pausedSongSelectionIdx = (pausedSongSelectionIdx - 1) % songs.Length;
        if (pausedSongSelectionIdx < 0)
            pausedSongSelectionIdx = songs.Length - 1;
        UpdateDescription();
    }

    private void UpdateDescription() {
        cover.sprite = songs[pausedSongSelectionIdx].cover;
        title.text = songs[pausedSongSelectionIdx].title; 
        singers.text = songs[pausedSongSelectionIdx].singer;
        difficulty.text = songs[pausedSongSelectionIdx].difficulty;
        length.text = CalculateLength(songs[pausedSongSelectionIdx].videoClip.length);
    }

    private string CalculateLength(double timeInSeconds) {
        int minutes = (int) (timeInSeconds / 60F);
        int seconds = (int) (timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
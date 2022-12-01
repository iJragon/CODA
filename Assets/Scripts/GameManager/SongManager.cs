using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour {
    public static SongManager instance;

    public SongCreator[] songs;             // Songs we have on our playlist 
    public int currentSongIdx;              // Current song that is playing
    public int pausedSongSelectionIdx;      // Current selection of the song while we're changing the song (paused menu)

    /* Song details */
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
        /* Default to the first song on the playlist */
        currentSongIdx = 0;
        pausedSongSelectionIdx = currentSongIdx;
        UpdateDescription();
    }

    /// <summary>
    /// /* Go forward and wrap back to the beginning if we're at the end */
    /// </summary>
    public void NextSong() {
        pausedSongSelectionIdx = (pausedSongSelectionIdx + 1) % songs.Length;
        UpdateDescription();
    }

    /// <summary>
    /// /* Go backward and wrap to the end if we're at the beginning */
    /// </summary>
    public void PreviousSong() {
        pausedSongSelectionIdx = (pausedSongSelectionIdx - 1) % songs.Length;
        if (pausedSongSelectionIdx < 0)
            pausedSongSelectionIdx = songs.Length - 1;
        UpdateDescription();
    }

    /// <summary>
    /// Update the menu with the correctly selected song's details
    /// </summary>
    private void UpdateDescription() {
        cover.sprite = songs[pausedSongSelectionIdx].cover;
        title.text = songs[pausedSongSelectionIdx].title; 
        singers.text = songs[pausedSongSelectionIdx].singer;
        difficulty.text = songs[pausedSongSelectionIdx].difficulty;
        length.text = CalculateLength(songs[pausedSongSelectionIdx].videoClip.length);
    }

    /// <summary>
    /// Format the length of the song into minutes and seconds
    /// </summary>
    /// <param name="timeInSeconds"></param> length of the song in seconds total
    /// <returns></returns>
    private string CalculateLength(double timeInSeconds) {
        int minutes = (int) (timeInSeconds / 60F);
        int seconds = (int) (timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
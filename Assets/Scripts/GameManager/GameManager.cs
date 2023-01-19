using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
//using UnityEditor.Scripting.Python;
//using UnityEditor;
//using System.IO;


public class GameManager : MonoBehaviour {
    public static GameManager instance;

    [SerializeField] private GameObject CODA_App;           // App of CODA on the taskbar
    [SerializeField] private GameObject CODA_Icon;          // Icon of CODA on the desktop
    [SerializeField] private GameObject game_Video;         // Video that will play the current song
    [SerializeField] private GameObject no_Watermark;       // Wallpaper snippet that covers the watermark
    [SerializeField] private GameObject start_Wallpaper;    // Gradient background image
    [SerializeField] private GameObject accuracy_Text;      // Score (the notepad) on the desktop
    [SerializeField] private GameObject taskbarGlow;        // Glowing taskbar during gameplay
    [SerializeField] private GameObject taskbarUnglow;      // Regular taskbar before gameplay
    [SerializeField] private GameObject title;              // Logo on desktpop
    [SerializeField] private GameObject windowsFade;        // Fade in/out black image
    [SerializeField] private GameObject playlist;           // Playlist (song selection) pop-up
    [SerializeField] private GameObject pausePanel;         // Invisible panel for player to deselect pop-ups
    [SerializeField] private TextMeshProUGUI dDetect;       // Debugging text to see what sign player is doing

    /* If we are already mid-gameplay (otherwise, we just booted for the first time and are on the desktop screen */
    public bool isPlaying;

    private LyricGenerator lyricGenerator;
    private VideoPlayer videoPlayer;

    private string cheatCode; 

    private void Awake() {
        if (instance = null)
            instance = this;

        //PythonRunner.RunFile(Path.Combine(Application.dataPath, "Detection/app.py"));
    }

    private void Start() {
        /* Ensure gameplay stuff isn't enabled because we haven't started yet */
        lyricGenerator = gameObject.GetComponent<LyricGenerator>();
        videoPlayer = game_Video.GetComponent<VideoPlayer>();
        lyricGenerator.enabled = false;
        game_Video.SetActive(false);
        isPlaying = false;

        /* Show gradient background, logo, and start with the fading out of black image */
        start_Wallpaper.SetActive(true);
        windowsFade.SetActive(true);

        /* Read in the first song by default */
        CSVReader.instance.ReadCSV();

        /* Initiate the process to see whether our video is done playing. When it is, invoke OnVideoFinished */
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Update() {
        if (HandDetection.symbol != null)
            dDetect.text = char.ToUpper(HandDetection.symbol[0]) + HandDetection.symbol.Substring(1);

        /* Button click sound every time we click */
        if (Input.GetMouseButtonDown(0))
            AudioManager.instance.Play("ButtonClick");

        /* Reset if player is about to sign a new word */
        if (Input.GetKeyDown(KeyCode.Space))
            cheatCode = "";

        /* Keep track of all the keys the player is hitting */
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(vKey) && vKey != KeyCode.Space) {
                // If the player is currently holding spacebar, then append each letter to the currSign, denoting it's a word, not a letter
                // Otherwise, treat the key as a single alphanumeric character
                if (Input.GetKey(KeyCode.Space)) {
                    cheatCode += vKey;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Space) && cheatCode.ToLower().CompareTo("quit") == 0)
            StartCoroutine(SongComplete());
    }

    /// <summary>
    /// When the CODA app on desktop gets pressed 
    /// </summary>
    public void OnClickCODA_App() {
        playlist.SetActive(true);
        PauseGame();
    }

    /// <summary>
    /// Game is starting, turn on all the corresponding objects
    /// </summary>
    public void StartupGame() {
        /* Turn video on and set the video to current song */
        game_Video.SetActive(true);
        videoPlayer.clip = SongManager.instance.songs[SongManager.instance.currentSongIdx].videoClip;

        /* Activate the game visuals */
        CODA_App.SetActive(true);
        taskbarGlow.SetActive(true);
        accuracy_Text.SetActive(true);

        /* Deactivate the desktop visuals */
        start_Wallpaper.SetActive(false);
        CODA_Icon.SetActive(false);
        title.SetActive(false);
        playlist.SetActive(false);

        /* Start the game mechanics */
        lyricGenerator.enabled = true;
        isPlaying = true;
    }

    /// <summary>
    /// Return to the desktop screen, most likely when player exits out of CODA app
    /// </summary>
    public void HideCODA_App() {
        /* Activate the desktop visuals */
        start_Wallpaper.SetActive(true);
        CODA_Icon.SetActive(true);
        title.SetActive(true);
        taskbarUnglow.SetActive(true);

        /* Deactivate the game visuals */
        CODA_App.SetActive(false);
        taskbarGlow.SetActive(false);
        game_Video.SetActive(false);
        accuracy_Text.SetActive(false);

        /* Stop the game mechanics */
        lyricGenerator.enabled = false;
        isPlaying = false;
    }

    /// <summary>
    /// When the player presses the Start button in the song selection window
    /// </summary>
    public void StartGame() {
        ResumeGame();
        /* If we changed the song while we were paused, update the current song and update the symbol details */
        if (SongManager.instance.currentSongIdx != SongManager.instance.pausedSongSelectionIdx) {
            SongManager.instance.currentSongIdx = SongManager.instance.pausedSongSelectionIdx;
            CSVReader.instance.ReadCSV();
            // Start the visuals for changing the song (e.g. fading out)
            if (isPlaying)
                StartCoroutine(ChangeSong());
        }

        /* It's possible to pause while we're on the desktop screen, so upon startup (not playing), we start up the game the first time */
        if (!isPlaying)
            StartupGame();
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame() {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        /* Pause the video as well */
        videoPlayer.Pause();
        /* Put up the invisible pause panel up so we can deselect the current pop-up and resume the game */
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// Start the game
    /// </summary>
    public void ResumeGame() {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        pausePanel.SetActive(false);
        /* If we've already been playing, then replay the video. Otherwise, on startup, there is no video to play */
        if (isPlaying)
            videoPlayer.Play();
    }

    /// <summary>
    /// Called when the song has changed and we need to reset our accuracy stats and update the symbol speed according to song difficulty
    /// </summary>
    public void ResetGame() {
        LyricGenerator.instance.ClearScreen();
        Symbol.UpdateSpeed();
    }

    /// <summary>
    /// Called when we restart the song after being paused so we resume the video but transition to song change mechanics
    /// </summary>
    public void RestartSong() {
        /* Don't restart the song if we don't currently have a song playing */
        if (!isPlaying) {
            pausePanel.SetActive(false);
            return;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;

        /* Game was paused when player selects restart song option, so resume the video */
        if (isPlaying)
            videoPlayer.Play();
        StartCoroutine(ChangeSong());
    }

    /// <summary>
    /// Black wallpaper becomes opague, video becomes transparent
    /// </summary>
    public void FadeIn() {
        windowsFade.GetComponent<Animator>().SetTrigger("fadeIn");
        game_Video.GetComponent<Animator>().SetTrigger("fadeOut");
    }

    /// <summary>
    /// Black wallpaper becomes transparent, video becomes opague
    /// </summary>
    public void FadeOut() {
        windowsFade.GetComponent<Animator>().SetTrigger("fastFadeOut");
        game_Video.GetComponent<Animator>().SetTrigger("fadeIn");
    }

    /// <summary>
    /// Changing the song mechanics, involving updating the current video and resetting stats
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangeSong() {
        FadeIn();
        StartCoroutine(SoundManager.instance.FadeOutVideoVolume(3.5f));
        lyricGenerator.enabled = false;
        yield return new WaitForSeconds(3.5f);
        ResetGame();
        start_Wallpaper.SetActive(false);
        videoPlayer.clip = null;
        videoPlayer.clip = SongManager.instance.songs[SongManager.instance.currentSongIdx].videoClip;
        lyricGenerator.enabled = true;
        FadeOut();
    }

    /// <summary>
    /// Called when the video is finished playing
    /// </summary>
    /// <param name="vp"></param>
    private void OnVideoFinished(UnityEngine.Video.VideoPlayer vp) {
        StartCoroutine(SongComplete());
    }

    /// <summary>
    /// Called when the song is finished playing and transition to the stats 
    /// </summary>
    /// <returns></returns>
    public IEnumerator SongComplete() {
        FadeIn();
        yield return new WaitForSeconds(2f);
        windowsFade.GetComponent<Animator>().SetTrigger("fastFadeOut");
        LyricGenerator.signToAccuracy[] worstSigns = LyricGenerator.instance.GetWorstSigns();
        ResetGame();
        HideCODA_App(); 
    }
}
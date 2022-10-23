using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {
    [SerializeField] private GameObject CODA_App;
    [SerializeField] private GameObject Score_Text;

    private LyricGenerator lyricGenerator;
    private void Start() {
        lyricGenerator = gameObject.GetComponent<LyricGenerator>();
        lyricGenerator.enabled = false;
    }

    public void ShowCODA_App() {
        CODA_App.SetActive(true);
        Score_Text.SetActive(true);
        lyricGenerator.enabled = true;
    }

    public void HideCODA_App() {
        CODA_App.SetActive(false);
        Score_Text.SetActive(false);
        lyricGenerator.enabled = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class TrainModelScript : MonoBehaviour {
    private HandDetection detection;
    public bool enableRunTimeTraining;
    private bool isRecording;

    #region text
    [SerializeField] private TextMeshProUGUI FPSText;
    [SerializeField] private TextMeshProUGUI currSymbol;
    [SerializeField] private TextMeshProUGUI status;
    #endregion


    private enum statuses {
        Detecting,
        Recording,
        Capturing,
        ErrorFrom
    }
    private statuses currStatus;

    #region FPS
    private const float pollingTime = 1f;
    private float time;
    private int frameCount;
    private int currFrameRate;
    #endregion


    private string currKey;
    private string prevKey;
    private float captureTime;

    private void Start() {
        if (!enableRunTimeTraining)
            gameObject.GetComponent<TrainModelScript>().enabled = false;

        detection = gameObject.GetComponent<HandDetection>();

        currStatus = statuses.Detecting;
        isRecording = false;

        captureTime = 1f;
    }

    private void Update() {
        FPSText.text = "FPS: " + CalcFPS().ToString();
        if (HandDetection.symbol != null)
            currSymbol.text = "Current Symbol: " + char.ToUpper(HandDetection.symbol[0]) + HandDetection.symbol.Substring(1);

        status.text = currStatus.ToString();
        if (currStatus == statuses.Capturing || currStatus == statuses.ErrorFrom) {
            status.text += " \"" + prevKey + "\"";
        }

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            isRecording = !isRecording;
            currStatus = isRecording ? statuses.Recording : statuses.Detecting;
        }

        if (isRecording) {
            /* Keep track of all the keys the player is hitting */
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey) && vKey != KeyCode.Return 
                    && ((char)vKey).ToString().All(
                        c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) {
                    currKey += ((char)vKey).ToString().ToLower();
                }
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                try {
                    int key = int.Parse(currKey);
                    if (detection._PredictionDictionary.ContainsKey(key)) {
                        CaptureStatus(statuses.Capturing);
                        AddToKeypoint(key);
                    } else
                        CaptureStatus(statuses.ErrorFrom);
                } catch {
                    CaptureStatus(statuses.ErrorFrom);
                }

                prevKey = currKey;
                currKey = "";
            }
        }

        if (currStatus == statuses.ErrorFrom || currStatus == statuses.Capturing) {
            captureTime -= Time.deltaTime;
            if (captureTime <= 0) {
                currStatus = isRecording ? statuses.Recording : statuses.Detecting;
                captureTime = 1f;
            }
        }
    }

    private void AddToKeypoint(int key) {
        var csvPath = Application.dataPath + @"/Resources/keypoint.csv";

        StreamWriter keypoint = new StreamWriter(csvPath, true);

        keypoint.WriteLine(key + "," + string.Join(",", HandDetection.landmarks.Select(x => x.ToString())));
        keypoint.Flush();
        keypoint.Close();
    }

    private void CaptureStatus(statuses status) {
        currStatus = status;
        captureTime = 1f;
    }

    private int CalcFPS() {
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime) {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            time -= pollingTime;
            frameCount = 0;
            currFrameRate = frameRate;
        }
        return currFrameRate;
    }
}
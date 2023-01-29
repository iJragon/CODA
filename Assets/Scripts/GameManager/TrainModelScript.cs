using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class TrainModelScript : MonoBehaviour {
    private HandDetection detection;

    #region Text
    [SerializeField] private TextMeshProUGUI FPSText;
    [SerializeField] private TextMeshProUGUI currSymbol;
    [SerializeField] private TextMeshProUGUI status;
    #endregion

    #region FPS
    private const float pollingTime = 1f;
    private float time;
    private int frameCount;
    private int currFrameRate;
    #endregion

    #region Recording Variables
    private enum statuses {
        Detecting,
        Recording,
        Capturing,
        ErrorFrom
    }
    private statuses currStatus;
    public bool enableRunTimeTraining;
    private bool isRecording;
    private string currKey;
    private string prevKey;
    private float captureTime;
    #endregion

    private void Start() {
        if (!enableRunTimeTraining)
            gameObject.GetComponent<TrainModelScript>().enabled = false;

        detection = gameObject.GetComponent<HandDetection>();
        currStatus = statuses.Detecting;
        isRecording = false;
        captureTime = 1f;
    }

    private void Update() {
        #region Text
        FPSText.text = "FPS: " + CalcFPS().ToString();
        if (HandDetection.symbol != null)
            currSymbol.text = "Current Symbol: " + char.ToUpper(HandDetection.symbol[0]) + HandDetection.symbol.Substring(1);

        status.text = currStatus.ToString();
        if (currStatus == statuses.Capturing || currStatus == statuses.ErrorFrom) {
            status.text += " \"" + prevKey + "\"";
            captureTime -= Time.deltaTime;
            if (captureTime <= 0) {
                currStatus = isRecording ? statuses.Recording : statuses.Detecting;
                captureTime = 1f;
            }
        }
        #endregion

        #region Recording Mechanics
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            isRecording = !isRecording;
            currStatus = isRecording ? statuses.Recording : statuses.Detecting;
        }

        if (isRecording) {
            if (!string.IsNullOrEmpty(currKey) && currStatus == statuses.Recording)
                status.text += " \"" + currKey + "\"";

            /* Keep track of all the keys the player is hitting */
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey) && vKey != KeyCode.Return && vKey != KeyCode.Backspace
                    && ((char)vKey).ToString().All(
                        c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) {
                    currKey += ((char)vKey).ToString().ToLower();
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && !string.IsNullOrEmpty(currKey)) 
                currKey = currKey.Remove(currKey.Length - 1, 1);

            if (Input.GetKeyDown(KeyCode.Return))
                CapturePoint();
        }
        #endregion
    }

    public static void AddToKeypoint(int key, string path, List<double> landmarks) {
        var csvPath = Application.dataPath + path;

        StreamWriter keypoint = new StreamWriter(csvPath, true);

        keypoint.WriteLine(key + "," + string.Join(",", landmarks.Select(x => x.ToString())));
        keypoint.Flush();
        keypoint.Close();
    }

    private void CapturePoint() {
        string sign;
        if (string.IsNullOrEmpty(currKey))
            sign = prevKey;
        else {
            sign = currKey;
            prevKey = currKey;
            currKey = "";
        }

        if (detection.predictionMapToNums.ContainsKey(sign)) {
            int mappedNum = detection.predictionMapToNums[sign];
            CaptureStatus(statuses.Capturing);
            AddToKeypoint(mappedNum, @"/Resources/keypoint.csv", HandDetection.landmarks);
        } else
            currStatus = statuses.ErrorFrom;
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
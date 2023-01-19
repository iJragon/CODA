using Mediapipe;
using Mediapipe.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DetectionDebugger : MonoBehaviour {
    [SerializeField] TextMeshProUGUI temp;

    private void Update() {
        temp.text = char.ToUpper(HandDetection.symbol[0]) + HandDetection.symbol.Substring(1);
    }
}
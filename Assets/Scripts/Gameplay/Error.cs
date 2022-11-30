using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Error : MonoBehaviour {
    public void EnqueueMe() {
        LyricGenerator.instance.errorMessages.Enqueue(gameObject);
    }
}
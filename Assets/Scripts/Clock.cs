using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI clock;

    private void Start() {
        StartCoroutine(UpdateTime());
    }

    private IEnumerator UpdateTime() {
        while (true) {
            var today = System.DateTime.Now;
            clock.text = today.ToString("HH:mm");
            yield return new WaitForSeconds(0.2f);
        }
    }
}
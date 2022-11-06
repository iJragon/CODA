using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour {
    [SerializeField] private Image element; 

    public void SetColor(string status) {
        switch (status) {
            case "enter":
                element.color = new Color32(239, 53, 203, 255);
                break;
            case "down":
                element.color = new Color32(93, 81, 255, 255);
                break;
            case "exit":
                element.color = new Color32(1, 1, 1, 0);
                break;
        }
    }
}
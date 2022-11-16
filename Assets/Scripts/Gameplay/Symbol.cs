using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour {
    private const float bottomY = -5.4f;
    private char letter;
    public static float speed;
    public bool isDestroyed;

    public Symbol(char letter) {
        this.letter = letter;
    }

    private void Start() {
        UpdateSpeed();
    }

    private void Update() {
        /* Move down on the screen at a constant pace */
        gameObject.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        /* Remove the symbol and decrease the player's accuracy if it reaches the bottom */
        if (!isDestroyed && gameObject.transform.position.y < bottomY) {
            LyricGenerator.instance.RemoveLyric(letter);
        }
    }

    public void SetLetter(char letter) {
        this.letter = letter;
    }

    public void SetSprite(Sprite sprite) {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
    }

    public float GetWidth() {
        return gameObject.GetComponent<SpriteRenderer>().bounds.size.x;
    }

    public float GetHeight() {
        return gameObject.GetComponent<SpriteRenderer>().bounds.size.y;
    }

    public static void UpdateSpeed() {
        speed = SongManager.instance.songs[SongManager.instance.currentSongIdx].speed;
    }
}

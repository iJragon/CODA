using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour {
    private const float bottomY = -5.5f;
    private const float speed = 3f;
    private char letter;

    public Symbol(char letter) {
        this.letter = letter;
    }

    private void Update() {
        /* Move down on the screen at a constant pace */
        gameObject.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        /* Remove the symbol and decrease the player's accuracy if it reaches the bottom */
        if (gameObject.transform.position.y < bottomY) {
            LyricGenerator.instance.RemoveLyric(letter);
        }
    }

    public void setLetter(char letter) {
        this.letter = letter;
    }

    public void setSprite(Sprite sprite) {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }
}

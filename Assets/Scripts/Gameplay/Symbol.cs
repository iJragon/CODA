using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour {
    private const float bottomY = -5.4f;    // Bottom of the screen where it should despawn
    public static float speed;              // Speed at which it moves down on the screen 
    private string sign;
    public bool isDestroyed;                // Make sure it's only destroyed once (player might hit the letter at the last moment)

    private void Start() {
        UpdateSpeed();
    }

    private void Update() {
        /* Move down on the screen at a constant pace */
        gameObject.transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        /* Remove the symbol and decrease the player's accuracy if it reaches the bottom */
        if (!isDestroyed && gameObject.transform.position.y < bottomY)
            LyricGenerator.instance.RemoveLyric(sign);
    }

    public void SetSign(string sign) {
        this.sign = sign;
    }

    public string GetSign() {
        return sign;
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

    public void DestroyMe() {
        Destroy(gameObject);
    }

    /// <summary>
    /// This function is called whenever the song is changed and we need to update the speed for all symbols
    /// </summary>
    public static void UpdateSpeed() {
        speed = SongManager.instance.songs[SongManager.instance.currentSongIdx].speed;
    }
}

using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance;

    public Sound[] sounds;

    private void Awake() {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
            
        /* Create an AudioSource for each sound and assign all necessary fields */
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            // When game is paused, we can still hear click sound
            if (s.name == "ButtonClick")
                s.source.ignoreListenerPause = true;
        }
    }

    private void Start() {
        Play("Startup");
    }

    public void Play(string name) {
        /* Look through each sound in array of sounds to find matching name, then play it */
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }
}
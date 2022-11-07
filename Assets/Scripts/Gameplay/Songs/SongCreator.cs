using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Song", menuName = "Song/Song")]
public class SongCreator : ScriptableObject
{
    public Sprite cover;
    public string title;
    public string singer;
    public float offset;
    public float speed;
    public string difficulty;
    public VideoClip videoClip;
    public TextAsset songData;
}

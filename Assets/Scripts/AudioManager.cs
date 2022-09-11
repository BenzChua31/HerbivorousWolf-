using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource[] audios;
    private AudioSource song;

    // Start is called before the first frame update
    void Start()
    {
        song = audios[0];
        song.Play();
        Invoke("nextSong", song.clip.length);
    }

    // Update is called once per frame
    void Update()
    {

    }


    // For now it should loop the same normal state BGM song over and over again
    void nextSong()
    {
        song.Stop();
        song = audios[1];
        song.Play();
        Invoke("nextSong", song.clip.length);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{

    public AudioSource[] audios;
    private AudioSource song;
    private GameObject managers;

    // Start is called before the first frame update
    void Start()
    {
        song = audios[0];
        managers = GameObject.FindWithTag("Managers");
    }

    // Update is called once per frame
    void Update()
    {

        if (managers.GetComponent<MenuUIManager>().loadingT.anchoredPosition.y <= -500.0f && song.isPlaying == false)
        {
            managers.GetComponentInChildren<AudioSource>().Stop();
            song.Play();
            Invoke("nextSong", song.clip.length);
        }
    }

    // For now it should loop the same normal state BGM song over and over again
    void nextSong()
    {
        song.Stop();
        song = audios[1];
        song.Play();
        song.loop = true;
    }
}

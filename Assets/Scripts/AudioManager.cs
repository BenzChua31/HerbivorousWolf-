using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public AudioSource[] audios;
    private AudioSource song;

    // Start is called before the first frame update
    void Start()
    {
        song = audios[4]; // Initially, Main Menu audio will be playing
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playMainMenu()
    {
        playSong(4);
    }

    public void playLevel()
    {
        if (!audios[0].isPlaying && !audios[1].isPlaying)
        {
            song.Stop();
            song = audios[0];
            song.Play();
            Invoke("playGameSong", song.clip.length);
        }
    }

    public void playRustlingLeaves()
    {
        audios[5].Play();
        audios[5].loop = true;
    }

    public void stopRustlingLeaves()
    {
        audios[5].Stop();
    }

    private void playGameSong()
    {
        playSong(1);
    }

    private void playSong(int index)
    {
        song.Stop();
        song = audios[index];
        song.Play();
        song.loop = true;
    }
}

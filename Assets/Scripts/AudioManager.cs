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
        song.ignoreListenerPause = true;
        song.Play();
        song.loop = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayMainMenu()
    {
        PlaySong(4);
    }

    public void PlayLevel()
    {
        song.Stop();
        song = audios[0];
        song.Play();
        StartCoroutine(PlayGameSong(song.clip.length));
    }

    public void PlayRustlingLeaves()
    {
        audios[5].Play();
        audios[5].loop = true;
    }

    public void StopRustlingLeaves()
    {
        audios[5].Stop();
    }

    public void PlayKnockWall()
    {
        audios[7].Play();
        audios[7].loop = false;
    }

    IEnumerator PlayGameSong(float duration)
    {
        yield return new WaitForSeconds(duration);
        PlaySong(1);
        yield return null;
    }

    private void PlaySong(int index)
    {
        if (!audios[4].isPlaying) // Set MainMenu to take precedence over others
        {
            song.Stop();
            song = audios[index];
            song.Play();
            song.loop = true;
        }
    }
}

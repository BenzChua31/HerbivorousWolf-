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

    public void PlayEatPellet()
    {
        audios[8].Play();
        audios[8].loop = false;
    }

    public void PlayPowerUp()
    {
        audios[9].Play();
        audios[9].loop = false;
    }

    public void PlayBunnyDeath()
    {
        song.Pause();
        audios[10].Play();
        audios[10].loop = false;
        StartCoroutine(UnPauseMusic(audios[10].clip.length));
    }

    IEnumerator UnPauseMusic(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (song == audios[2]) { song.UnPause(); }
    }

    public void PlayScaredSong()
    {
        song.Stop();
        song = audios[2];
        song.Play();
        song.loop = true;
    }

    public void StopScaredSong()
    {
        PlaySong(1);
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

    public void PlayPerish()
    {
        audios[6].Play();
        audios[6].loop = false;
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

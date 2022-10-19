using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyController : MonoBehaviour
{

    private bool isScared;
    private bool isRecovering;
    private bool isBlinking;
    private Animator animator;
    private float scaredSeconds = 10;
    private UIManager uiManager;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        isScared = false;
        isRecovering = false;
        animator = gameObject.GetComponent<Animator>();
        uiManager = GameObject.FindWithTag("Managers").GetComponent<UIManager>();
        audioManager = GameObject.FindWithTag("Managers").GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isScared)
        {
            SetScaredTimer();
        }
    }

    public void ActivateScaredState() // Also resets if we stack power pellets
    {
        isScared = true;
        isRecovering = false;
        isBlinking = false;
        scaredSeconds = 10.0f;
        animator.SetBool("Panic", true);
        animator.SetBool("Recover", false);
        uiManager.ShowScaredTimer();
        uiManager.StopTimerBlink();
    }

    public bool IsScaredState()
    {
        return isScared;
    }

    private void SetScaredTimer()
    {
        scaredSeconds -= Time.deltaTime;

        if (scaredSeconds <= 0.0f)
        {
            scaredSeconds = 10.0f;
            isScared = false;
            animator.SetBool("Panic", false);
            animator.SetBool("Recover", false);
            uiManager.UpdateScaredTimer(10.0f);
            uiManager.HideScaredTimer();
            uiManager.StopTimerBlink();
            audioManager.StopScaredSong(); // return back to normal song
        }
        else
        {
            uiManager.UpdateScaredTimer(scaredSeconds);
        }

        if (scaredSeconds < 3.0f && scaredSeconds > 0.0f && !isRecovering) // Blinking Bunny at 3secs left
        {
            isRecovering = true;
            animator.SetBool("Recover", true);
        }

        // 3.99 is displayed as 3
        if (scaredSeconds < 4.0f && scaredSeconds > 0.0f && !isBlinking) // Blinking red timer 
        {
            isBlinking = true;
            uiManager.ActivateTimerBlink();
        }

    }

    public void PlayBunnyFuneral()
    {
        animator.SetBool("Dead", true);
        audioManager.PlayBunnyDeath();
        StartCoroutine(RemoveBunny(audioManager.audios[10].clip.length));
    }

    IEnumerator RemoveBunny(float duration) // Temporary
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

}

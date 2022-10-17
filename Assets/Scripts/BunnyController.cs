using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyController : MonoBehaviour
{

    private bool isScared;
    private bool isRecovering;
    private Animator animator;
    private float scaredSeconds = 0;
    private UIManager uiManager;

    // Start is called before the first frame update
    void Start()
    {
        isScared = false;
        isRecovering = false;
        animator = gameObject.GetComponent<Animator>();
        uiManager = GameObject.FindWithTag("Managers").GetComponent<UIManager>();
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
        scaredSeconds = 0.0f;
        animator.SetBool("Panic", true);
        animator.SetBool("Recovering", false);
        uiManager.ShowScaredTimer();
        uiManager.StopTimerBlink();
    }

    public bool IsScaredState()
    {
        return isScared;
    }

    private void SetScaredTimer()
    {
        scaredSeconds += Time.deltaTime;

        if (scaredSeconds >= 10.0f)
        {
            scaredSeconds = 0.0f;
            isScared = false;
            uiManager.UpdateScaredTimer(0.0f);
            uiManager.HideScaredTimer();
            uiManager.StopTimerBlink();
        }
        else
        {
            uiManager.UpdateScaredTimer(scaredSeconds);
        }

        if (scaredSeconds >= 7.0f && scaredSeconds < 10.0f && !isRecovering) // Blinking red timer & Bunny
        {
            isRecovering = true;
            animator.SetBool("Recovering", true);
            uiManager.ActivateTimerBlink();
        }

    }

}

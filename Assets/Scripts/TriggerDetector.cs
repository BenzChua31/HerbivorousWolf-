using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerDetector : MonoBehaviour
{
    private PacStudentController psc;
    private List<GhostController> bunnyControllers; // For power pellet to access all other bunny controllers
    private GhostController selfController; // To access its own controller
    private AudioManager audioManager;

    // UNITY has waitforsecondsrealtime to coroutine without being affected by TimeScale
    // We will set timeScale to 0 to pause the game and have UI continue

    void Awake()
    {
        audioManager = GameObject.FindWithTag("Managers").GetComponent<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        psc = GameObject.FindWithTag("WolfController").GetComponent<PacStudentController>();

        if (gameObject.CompareTag("Power"))
        {
            bunnyControllers = new List<GhostController>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Bunny"))
            {
                // Debug.Log("Total 16 calls"); 4 meat x 4 bunny
                bunnyControllers.Add(go.GetComponent<GhostController>()); 
            }
        }

        if (gameObject.CompareTag("Bunny")) { selfController = gameObject.GetComponent<GhostController>(); }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wolf"))
        {
            if (gameObject.CompareTag("Berry"))
            {
                UIManager.AddScore(10);
                psc.ConsumeBerry();
                audioManager.PlayEatPellet(); 
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Cherry"))
            {
                UIManager.AddScore(100);
                audioManager.PlayEatPellet();
                gameObject.SetActive(false); // we will have cherrycontroller handle the deletion
            }
            else if (gameObject.CompareTag("Power"))
            {
                audioManager.PlayPowerUp();
                audioManager.PlayScaredSong();
                foreach (GhostController bc in bunnyControllers)
                {
                    bc.ActivateScaredState();
                }
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Bunny"))
            {
                if (selfController.IsScaredState())
                {
                    UIManager.AddScore(300);
                    gameObject.GetComponent<CircleCollider2D>().enabled = false;
                    selfController.PlayBunnyFuneral();
                } 
                else
                {
                    psc.PlayWolfFuneral();
                }
            }
        }
    }
}

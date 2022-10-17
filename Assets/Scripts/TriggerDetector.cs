using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerDetector : MonoBehaviour
{
    private BunnyController bunnyController;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.CompareTag("Bunny"))
        {
            bunnyController = gameObject.GetComponent<BunnyController>();
        }
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
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Cherry"))
            {
                UIManager.AddScore(100);
                gameObject.SetActive(false); // we will have cherrycontroller handle the deletion
            }
            else if (gameObject.CompareTag("Power"))
            {
                bunnyController.ActivateScaredState();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Bunny"))
            {
                if (bunnyController.IsScaredState())
                {
                    Destroy(gameObject);
                } 
                else
                {
                    // Make PacStudent die here
                }
            }
        }
    }
}

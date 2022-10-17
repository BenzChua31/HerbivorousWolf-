using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerDetector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Power"))
            {
                UIManager.StartScaredTimer();
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Bunny"))
            {

            }
        }
    }
}

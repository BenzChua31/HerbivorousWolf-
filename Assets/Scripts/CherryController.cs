using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    private Camera cam;
    public GameObject cherryPrefab;
    private Tweener tweener;
    private bool activeCoroutine = false;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        tweener = gameObject.GetComponent<Tweener>();
    }

    void Update()
    {
        if (!activeCoroutine) { StartCoroutine(AddCherry()); }
    }

    IEnumerator AddCherry()
    {
        activeCoroutine = true;
        yield return new WaitForSeconds(10.0f);
        activeCoroutine = false;
        // Gets the world coordinates of the specified ViewPort positions. 
        // So that this works in relation to the User's ViewPort
        Vector3 tL = cam.ViewportToWorldPoint(new Vector3(-0.2f, 1.2f, 0));
        Vector3 tR = cam.ViewportToWorldPoint(new Vector3(1.2f, 1.2f, 0));
        Vector3 bR = cam.ViewportToWorldPoint(new Vector3(1.2f, -0.2f, 0));
        Vector3 bL = cam.ViewportToWorldPoint(new Vector3(-0.2f, -0.2f, 0));

        int chosenSide = Random.Range(0, 3);
        float chosenX;
        float chosenY;

        if (chosenSide == 0) // Top 
        {
            chosenX = Random.Range(tL.x, tR.x);
            chosenY = tL.y;
        }
        else if (chosenSide == 1) // Left
        {
            chosenX = tL.x;
            chosenY = Random.Range(bL.y, tL.y);
        }
        else if (chosenSide == 2) // Bottom
        {
            chosenX = Random.Range(bL.x, bR.x);
            chosenY = bL.y;
        }
        else // Right
        {
            chosenX = tR.x;
            chosenY = Random.Range(bR.y, tR.y);
        }

        GameObject cherry = Instantiate(cherryPrefab, new Vector3(chosenX, chosenY, 0), Quaternion.identity);
        cherry.GetComponent<SpriteRenderer>().sortingOrder = 3; // Make sure it is at the top
        Transform transform = cherry.GetComponent<Transform>();
        tweener.AddTween(transform, transform.position, transform.position * -1, 7.0f);
        StartCoroutine(RemoveCherry(cherry)); // Asynchronous check if cherry is done moving
    }

    IEnumerator RemoveCherry(GameObject cherry)
    {
        // Waits until tween completes then destroys cherry
        while (tweener.TweenExists(cherry.GetComponent<Transform>()))
        {
            yield return null;
        }
        Destroy(cherry);
        yield return null;
    }


}

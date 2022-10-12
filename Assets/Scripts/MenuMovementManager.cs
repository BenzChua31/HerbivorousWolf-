using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMovementManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemList;

    private Tweener tweener;

    // Start is called before the first frame update
    void Start()
    {
        tweener = gameObject.GetComponent<Tweener>();
    }

    // Update is called once per frame
    // BL : -10x -13y | BR : 10x -13y | TR : 10x 13y | TL : -10x 13y
    void Update()
    {
        foreach (GameObject item in itemList) // Hard-code this as I want the sprites to move on this fixed line
        {
            Transform iTransform = item.GetComponent<Transform>();
            float x = iTransform.position.x;
            float y = iTransform.position.y;
            if (x >= -13.0f && x < 10.0f && y == -13.0f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 0);
                AddTween(new Vector3(x + 1.0f, y, 0.0f), 0.5f); // BL --> BR, move 1.0f every .5 second
            }
            else if (x == 10.0f && y >= -13.0f && y < 13.0f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 90.0f);
                AddTween(new Vector3(x, y + 1.0f, 0.0f), 0.5f); // BR --> TR
            }
            else if (x <= 10.0f && x > -10.0f && y == 13.0f)
            {
                iTransform.rotation = Quaternion.Euler(0, 180.0f, 0);
                AddTween(new Vector3(x - 1.0f, y, 0.0f), 0.5f); // TR --> TL
            }
            else if (x == -10.0f && y <= 13.0f && y > -13.0f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 270.0f);
                AddTween(new Vector3(x, y - 1.0f, 0.0f), 0.5f); // TL --> BL
            }
        }
    }

    void AddTween(Vector3 end, float duration)
    {
        foreach (GameObject item in itemList)
        {
            Transform transform = item.GetComponent<Transform>();
            // Add to the first stationary object in the list
            if (tweener.AddTween(transform, transform.position, end, duration))
            {
                break;
            }
        }
    }


}

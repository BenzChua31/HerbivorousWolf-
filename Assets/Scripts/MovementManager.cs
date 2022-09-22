using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> itemList;
    
    private Tweener tweener;

    // Start is called before the first frame update
    void Start()
    {
        tweener = gameObject.GetComponent<Tweener>();
    }

    // Update is called once per frame
    // BL : -12.5x 9.5y | BR : -7.5x 9.5y | TR : -7.5x 13.5y | TL : -12.5x 13.5y
    void Update()
    {
        foreach (GameObject item in itemList) // Right now, there should be only one wolf so I'm going to hardcode the positions
        {
            Transform iTransform = item.GetComponent<Transform>();
            float x = iTransform.position.x;
            float y = iTransform.position.y;
            if (x == -12.5f && y == 9.5f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 90.0f);
                AddTween(new Vector3(-12.5f, 13.5f, 0.0f), 3.0f); // BL --> TL
            }
            else if (x == -7.5f && y == 9.5f)
            {
                iTransform.rotation = Quaternion.Euler(0, 180.0f, 0);
                AddTween(new Vector3(-12.5f, 9.5f, 0.0f), 3.0f); // BR --> BL
            }
            else if (x == -7.5f && y == 13.5f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 270.0f);
                AddTween(new Vector3(-7.5f, 9.5f, 0.0f), 3.0f); // TR --> BR
            }
            else if (x == -12.5f && y == 13.5f)
            {
                iTransform.rotation = Quaternion.Euler(0, 0, 0);
                AddTween(new Vector3(-7.5f, 13.5f, 0.0f), 3.0f); // TL --> TR
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

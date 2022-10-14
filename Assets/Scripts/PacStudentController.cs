using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{

    [SerializeField] private GameObject wolf;
    private Tweener tweener;
    private string lastInput = "";
    private string currentInput = "";
    private int[,] map;
    private int[] currentPos = { 5, 1 }; // Fixed Start Position in relation to the 2D map

    // Start is called before the first frame update
    void Start()
    {
        tweener = gameObject.GetComponent<Tweener>();
        map = LevelGenerator.getMap();
    }

    // Update is called once per frame
    void Update()
    {
        
        // lastInput will keep changing if player spams WASD
        // Tween will be added for the last key player press
        if (Input.GetKey(KeyCode.W))
        {
            lastInput = "W";
        }
        if (Input.GetKey(KeyCode.A))
        {
            lastInput = "A";
        }
        if (Input.GetKey(KeyCode.S))
        {
            lastInput = "S";
        }
        if (Input.GetKey(KeyCode.D))
        {
            lastInput = "D";
        }

        int statusL = Move(lastInput);
        if (statusL == 0)
        {
            Move(currentInput); // shuld naturally stop if no possible motion can be made
        }

    }

    private int Move(string inputType)
    {
        Transform transform = wolf.GetComponent<Transform>();
        Vector3 pos = wolf.transform.position;

        int row = currentPos[0];
        int col = currentPos[1];

        // The tweener will deny addition if an existing tween exists 
        if (inputType.Equals("W"))
        {
            return MoveUp(transform, pos, row, col);
        }
        else if (inputType.Equals("A"))
        {
            return MoveLeft(transform, pos, row, col);
        }
        else if (inputType.Equals("S"))
        {
            return MoveBtm(transform, pos, row, col);
        }
        else if (inputType.Equals("D"))
        {
            return MoveRight(transform, pos, row, col);
        }

        return -1;

    }

    private int MoveUp(Transform transform, Vector3 pos, int row, int col)
    {
        if (checkTop())
        {
            bool status = tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y + 1.0f), 1.0f);
            if (status)
            {
                currentPos[0] = row - 1;
                currentPos[1] = col;
                currentInput = "W";
                return 1;
            }
            return 0;
        }
        return -1;
    }

    private int MoveBtm(Transform transform, Vector3 pos, int row, int col)
    {
        if (checkBtm())
        {
            bool status = tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y - 1.0f), 1.0f);
            if (status)
            {
                currentPos[0] = row + 1;
                currentPos[1] = col;
                currentInput = "S";
                return 1;
            }
            return 0;
        }
        return -1;
    }

    private int MoveLeft(Transform transform, Vector3 pos, int row, int col)
    {
        if (checkLeft())
        {
            bool status = tweener.AddTween(transform, pos, new Vector2(pos.x - 1.0f, pos.y), 1.0f);
            if (status)
            {
                currentPos[0] = row;
                currentPos[1] = col - 1;
                currentInput = "A";
                return 1;
            }
            return 0;
        }
        return -1;
    }

    private int MoveRight(Transform transform, Vector3 pos, int row, int col)
    {
        if (checkRight())
        {
            bool status = tweener.AddTween(transform, pos, new Vector2(pos.x + 1.0f, pos.y), 1.0f);
            if (status)
            {
                currentPos[0] = row;
                currentPos[1] = col + 1;
                currentInput = "D";
                return 1;
            }
            return 0;
        }
        return -1;
    }

    // To check if walkable or not (True for walkable, False for not)
    private bool checkTop()
    {
        int row = currentPos[0];
        int col = currentPos[1];

        if (IsOutOfBounds(col, row))
        {
            return false;
        }

        int dest = map[row - 1, col];

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkBtm()
    {
        int row = currentPos[0];
        int col = currentPos[1];

        if (IsOutOfBounds(col, row))
        {
            return false;
        }

        int dest = map[row + 1, col];

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkLeft()
    {
        int row = currentPos[0];
        int col = currentPos[1];

        if (IsOutOfBounds(col, row))
        {
            return false;
        }

        int dest = map[row, col - 1];

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkRight()
    {
        int row = currentPos[0];
        int col = currentPos[1];

        if (IsOutOfBounds(col, row))
        {
            return false;
        }

        int dest = map[row, col + 1];

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool IsOutOfBounds(int di, int dj)
    {
        return (di < 0 || di >= map.GetLength(1) || dj < 0 || dj >= map.GetLength(0));
    }

}

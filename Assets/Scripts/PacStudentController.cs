using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{

    [SerializeField] private GameObject wolf;
    private Tweener tweener;
    private string lastInput = "";
    private string currentInput = "";
    private bool flippedH = false; // Initially it isn't flipped, first quad (TL)
    private bool flippedV = false;
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
                // why we need a separate method is because check() just checks for walkability of adjacent tiles
                // but says nothing about whether the Tween will get add or not

                // (TRY THIS FIRST) check if a TweenExists by using TweenExists and passing in the transform... first
                // then if no, we can check for the adjacent tiles being walkable or not
                // in this case, if checkTop() realises that we are entering a mirrored quadrant
                // it can immediately update flippedH/V since the motion is guaranteed alrdy
                // and in this case, the coordinates won't change as it is mirrored 
                // but for example in BL, moving up would be moving down the 2D array
                // and instead of getting the adjacent coordinates in the check() method, we pass it to them as a parameter
                // so we don't have to recalculate the direction inside here; we determine direction based on flippedH/V
                // then we no longer need different check(), just one that checks for walls or is mirrored.

                // use a separate method to check if we are at border
                // and update the currentPos accordingly along with updating the flippedH/V 

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
        int testRow = currentPos[0];
        int testCol = currentPos[1];

        // Problem: (READ ABOVE first)
        // For BL, going up would be testRow + 1 (this is the case for those that are flipped horizontally)
        // (prob need to get copy of flippedV, to determine what coordinates to check)
        // Purpose of check() is to check if the adjacent coordinates are walkable (not just at the border of quadrants)
        // Check for mirrored quadrants too

        // For cases where at the border of TL and wanting to move right, it is a guaranteed move as it is mirrored, so we can immediately return true
        // no need to check for walls etc... 

        int rs = IsOutOfBounds(testCol, testRow - 1); // Check top grid

        if (rs == 1) { return false; }

        int dest;
        dest = map[testRow - 1, testCol]; 

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkBtm()
    {
        int testRow = currentPos[0];
        int testCol = currentPos[1];

        int rs = IsOutOfBounds(testCol, testRow + 1); // Check bot grid

        if (rs == 1) { return false; }

        int dest;
        // No flip required
        if (rs == 0) { dest = map[testRow + 1, testCol]; }
        else { return true; } // Flip needed aka entering mirrored quadrant, so checkBtm is 100% true

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkLeft()
    {
        int testRow = currentPos[0];
        int testCol = currentPos[1];

        int rs = IsOutOfBounds(testCol, testRow); // Check left grid

        if (rs == 1) { return false; }

        int dest;
        // No need to check for left since, going left wont lead to another quadrant
        dest = map[testRow, testCol - 1]; 

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private bool checkRight()
    {
        int testRow = currentPos[0];
        int testCol = currentPos[1];

        int rs = IsOutOfBounds(testCol + 1, testRow); // Check right grid

        if (rs == 1) { return false; }

        int dest;
        // No flip required
        if (rs == 0) { dest = map[testRow, testCol + 1]; } 
        else { return true; } // Flip needed aka entering mirrored quadrant, so checkRight is 100% true

        return (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7);
    }

    private int IsOutOfBounds(int di, int dj)
    {
        if (di < 0 || dj < 0)
        {
            return 1;
        }
        else if (di >= map.GetLength(1) || dj >= map.GetLength(0))
        {
            return 2; // flip will occur aka entering mirrored quadrant
        }
        return 0;
    }

}

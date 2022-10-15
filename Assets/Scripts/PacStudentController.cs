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
        if (statusL == 0) // if checkWalkable returns a false, then we execute this
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
        // Debug.Log(row + " " + col); Coordinate tested, works 100%

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
        if (!tweener.TweenExists(transform))
        {
            int adjRow = row;
            int adjCol = col;

            if (flippedH) { adjRow += 1; }
            else { adjRow -= 1; }

            int rs = checkWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by checkWalkable method
            {
                currentInput = "W";
                tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y + 1.0f), 1.0f);
                if (rs == 2)
                {
                    currentPos[0] = row;
                    currentPos[1] = col;
                    return 1;
                }
                else
                {
                    currentPos[0] = adjRow;
                    currentPos[1] = adjCol;
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }

    private int MoveBtm(Transform transform, Vector3 pos, int row, int col)
    {
        if (!tweener.TweenExists(transform))
        {
            int adjRow = row;
            int adjCol = col;

            if (flippedH) { adjRow -= 1; }
            else { adjRow += 1; }

            int rs = checkWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by checkWalkable method
            {
                currentInput = "S";
                tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y - 1.0f), 1.0f);
                if (rs == 2)
                {
                    currentPos[0] = row;
                    currentPos[1] = col;
                    return 1;
                }
                else
                {
                    currentPos[0] = adjRow;
                    currentPos[1] = adjCol;
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }

    private int MoveLeft(Transform transform, Vector3 pos, int row, int col)
    {
        if (!tweener.TweenExists(transform))
        {
            int adjRow = row;
            int adjCol = col;

            if (flippedV) { adjCol += 1; }
            else { adjCol -= 1; }

            int rs = checkWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by checkWalkable method
            {
                currentInput = "A";
                tweener.AddTween(transform, pos, new Vector2(pos.x - 1.0f, pos.y), 1.0f);
                if (rs == 3)
                {
                    currentPos[0] = row;
                    currentPos[1] = col;
                    return 1;
                }
                else
                {
                    currentPos[0] = adjRow;
                    currentPos[1] = adjCol;
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }

    private int MoveRight(Transform transform, Vector3 pos, int row, int col)
    {
        if (!tweener.TweenExists(transform))
        {
            int adjRow = row;
            int adjCol = col;

            if (flippedV) { adjCol -= 1; }
            else { adjCol += 1; }

            int rs = checkWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by checkWalkable method
            {
                currentInput = "D";
                tweener.AddTween(transform, pos, new Vector2(pos.x + 1.0f, pos.y), 1.0f);
                if (rs == 3) 
                {
                    currentPos[0] = row;
                    currentPos[1] = col;
                    return 1;
                } 
                else
                {
                    currentPos[0] = adjRow;
                    currentPos[1] = adjCol;
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
        return -1;
    }

    // 0 -  false, 1 - true, 2 -  true + flipH, 3 - true + flipV
    private int checkWalkable(int row, int col)
    {
        // Check if coordinates are out of bounds first
        if (IsOutOfBounds(col, row)) 
        {
            return 0;
        }

        // Check if entering a mirrored quadrant 
        int rs = IsEnteringMirrored(col, row);
        if (rs == 1) // set FlipH to indicate that we are now in the flipped quadrant
        { 
            if (flippedH) { flippedH = false; }
            else { flippedH = true; }
            return 2; // If we are entering a mirrored quadrant, it is guaranteed that it is walkable
        }
        else if (rs == 2) // set FlipV to indicate that we are now in the flipped quadrant
        {
            if (flippedV) { flippedV = false; }
            else { flippedV = true; }
            return 3;
        }
        else
        { // If it is just regular movement within a quadrant, we then check for walls etc... 
            int dest = map[row, col];
            if (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7) { return 1; }
            else { return 0; }
        }

    }

    // If it is out of bounds aka outside the map
    private bool IsOutOfBounds(int di, int dj)
    {
        if (di < 0 || dj < 0) { return true; }
        return false;
    }

    // If it is entering a mirrored quadrant 
    // 0 - no mirror, 1 - flipH, 2 - flipV
    private int IsEnteringMirrored(int di, int dj)
    {
        if (di >= map.GetLength(1)) { return 2; }
        else if (dj >= map.GetLength(0)) { return 1; }
        return 0;
    }

}

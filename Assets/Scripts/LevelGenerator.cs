using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [SerializeField] private GameObject levelToRemove;
    [SerializeField] private List<GameObject> walls;
    private int[,] map;
    // Position relative to topL quadrant
    private const float topLx = -20.5f;
    private const float topLy = 10.5f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(levelToRemove);

        // Paste your map here
        // Also if u r willing to, please give me feedback on my code, as in the algorithm used or tidiness or best practices... 
        // Since GameDev is an elective for me, I'm primarily interested in Backend Software Development
        this.map = new int[,] 
                    {
                    {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
                    {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
                    {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
                    {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
                    {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
                    {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
                    {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
                    {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
                    {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
                    {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
                    {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
                    {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
                    {0,0,0,0,0,2,5,4,4,0,3,4,4,0},
                    {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
                    {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
                    };

        // Map/Level 
        GameObject generatedLevel = new GameObject("generatedLevel");
        Transform transformGL = generatedLevel.GetComponent<Transform>();

        // Create the TopL Quadrant first
        GameObject topL = new GameObject("topL");
        topL.GetComponent<Transform>().SetParent(transformGL, false);
        topL.GetComponent<Transform>().position = new Vector3(7, 4, 0);

        // Proceed to step through array
        for (int j = 0; j < map.GetLength(0); j++) // 15 rows
        {
            for (int i = 0; i < map.GetLength(1); i++) // 14 columns
            {
                int val = map[j, i];
                if (val == 1) CreateOuterC(i, j, topL.GetComponent<Transform>());
                if (val == 2) CreateOuterW(i, j, topL.GetComponent<Transform>());
                if (val == 3) CreateInnerC(i, j, topL.GetComponent<Transform>());
                if (val == 4) CreateInnerW(i, j, topL.GetComponent<Transform>());
                if (val == 5) CreateBerry(i, j, topL.GetComponent<Transform>());
                if (val == 6) CreateMeat(i, j, topL.GetComponent<Transform>());
                if (val == 7) CreateTCon(i, j, topL.GetComponent<Transform>());
            }
        }

        // Simply dupe the quadrants and modify its position
        // TopL : 7 4 0
        // TopR : -7 4 0
        // BotL : 7 -3 0
        // BotR : -7 -3 0
        // We need to delete the last row of BotL and BotR (a loop to check and delete for all elements with -3.5y position)

        GameObject topR = Instantiate(topL, new Vector3(-7, 4, 0), Quaternion.Euler(new Vector3(0, 180.0f, 0)));
        topR.name = "topR";
        topR.GetComponent<Transform>().SetParent(transformGL, false);

        GameObject botL = Instantiate(topL, new Vector3(7, -3, 0), Quaternion.Euler(new Vector3(0, 180.0f, 180.0f)));
        botL.name = "botL";
        botL.GetComponent<Transform>().SetParent(transformGL, false);

        foreach (Transform transform in botL.GetComponent<Transform>())
        {
            if (transform.position.y == -3.5f)
            {
                Destroy(transform.gameObject);
            }
        }

        // We dupe botL so that we can save a for-loop check
        GameObject botR = Instantiate(botL, new Vector3(-7, -3, 0), Quaternion.Euler(new Vector3(0, 0, 180.0f)));
        botR.name = "botR";
        botR.GetComponent<Transform>().SetParent(transformGL, false);


    }

    /* ---------------------------------------------------------------------------------------------- */
    // Make elements' position be in relation to their corresponding quadrant GameObject

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    private void CreateOuterC(int i, int j, Transform parent)
    {
        int rotation = CheckOuterC(i, j);
        int z = 0;

        if (rotation == 1) z = 90;
        else if (rotation == 2) z = 180;
        else if (rotation == 3) z = 270;

        GameObject wall = Instantiate(walls[1], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.Euler(0, 0, z));
        wall.GetComponent<Transform>().SetParent(parent, false);
    }

    // 0 - No Rotation, 1 - Z90
    private void CreateOuterW(int i, int j, Transform parent)
    {
        int rotation = CheckOuterW(i, j);
        int z = 0;

        if (rotation == 1) z = 90;

        GameObject wall = Instantiate(walls[2], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.Euler(0, 0, z));
        wall.GetComponent<Transform>().SetParent(parent, false);
    }

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    private void CreateInnerC(int i, int j, Transform parent)
    {
        int rotation = CheckInnerC(i, j);
        int z = 0;

        if (rotation == 1) z = 90;
        else if (rotation == 2) z = 180;
        else if (rotation == 3) z = 270;

        GameObject wall = Instantiate(walls[3], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.Euler(0, 0, z));
        wall.GetComponent<Transform>().SetParent(parent, false);
    }

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    private void CreateInnerW(int i, int j, Transform parent)
    {
        int rotation = CheckInnerW(i, j);
        int z = 0;

        if (rotation == 1) z = 90;
        else if (rotation == 2) z = 180;
        else if (rotation == 3) z = 270;

        GameObject wall = Instantiate(walls[4], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.Euler(0, 0, z));
        wall.GetComponent<Transform>().SetParent(parent, false);
    }

    private void CreateBerry(int i, int j, Transform parent)
    {
        GameObject berry = Instantiate(walls[5], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.identity);
        berry.GetComponent<Transform>().SetParent(parent, false);
    }

    private void CreateMeat(int i, int j, Transform parent)
    {
        GameObject meat = Instantiate(walls[6], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.identity);
        meat.GetComponent<Transform>().SetParent(parent, false);
    }

    // 0 - No Rotation, 1 - Z90, 2 - Z180, 3 - Z270
    // 4 - Y180 No Rotation, 5 - Y180 Z90, 6 - Y180 Z180, 7 - Y180 Z270
    private void CreateTCon(int i, int j, Transform parent)
    {
        int rotation = CheckTCon(i, j);
        int y = 0;
        int z = 0;

        if (rotation == 1) z = 90;
        else if (rotation == 2) z = 180;
        else if (rotation == 3) z = 270;
        else if (rotation == 4) { y = 180; }
        else if (rotation == 5) { y = 180; z = 90; }
        else if (rotation == 6) { y = 180; z = 180; }
        else if (rotation == 7) { y = 180; z = 270; }

        GameObject wall = Instantiate(walls[7], new Vector3(topLx + i, topLy - j, 0.0f), Quaternion.Euler(0, y, z));
        wall.GetComponent<Transform>().SetParent(parent, false);
    }

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    // Default is facing left
    private int CheckInnerW(int i, int j)
    {

        int[] dir = { -1, 0, 1, 0, -1, 0, 1 };
        for (int x = 0; x < 4; x++)
        {
            // Algorithm 2 | O(n) worst case.
            int fj = j + dir[x];
            int fi = i + dir[x + 1];

            // Check if coordinates are legit
            if (IsOutOfBounds(fi, fj))
            {
                continue;
            }

            int fval = map[fj, fi];
            
            // If there is a berry next to it, we immediately face that direction. 
            // This is assuming that there won't more than one berries adjacent to the wall.
            if ((fval == 5 || fval == 6))
            {
                    if (x == 0) return 3;
                    if (x == 1) return 2;
                    if (x == 2) return 1;
                    if (x == 3) return 0;
            }
        }

        // If no berries around, we perform a second loop, to check for empty walls, we immediately face the empty direction.  
        // This is assuming that there won't more than one empty cell adjacent to the wall.
        // If there is an empty wall, it will by default aim at the first empty cell direction. (Not sure how to come up with a responsive solution)
        for (int x = 0; x < 4; x++)
        {
            int fj = j + dir[x];
            int fi = i + dir[x + 1];

            // Check if coordinates are legit
            if (IsOutOfBounds(fi, fj))
            {
                continue;
            }

            int fval = map[fj, fi];

            if ((fval == 0))
            {
                if (x == 0) return 3;
                if (x == 1) return 2;
                if (x == 2) return 1;
                if (x == 3) return 0;
            }
        }

        return -1;
    }

    private int CheckOuterW(int i, int j)
    {
        int[] dir = { -1, 0, 1, 0, -1 };
        for (int x = 0; x < 4; x++)
        {
            int dj = j + dir[x];
            int di = i + dir[x + 1];

            // Check if coordinates are legit
            if (IsOutOfBounds(di, dj))
            {
                continue;
            }

            int val = map[dj, di];

            // Check if rotation needed (1 for yes), (0 for no)
            // Assuming walls/corners/tconnectors adjacent to it are connected to it. 
            if (val == 1 || val == 2 || val == 7)
            {
                // If top/bot exists, then no rotation (0), else (1)
                return (x % 2 == 0) ? 0 : 1;
            }
        }
        return -1;
    }

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    // Default is a top-left corner
    private int CheckOuterC(int i, int j)
    {
        int[] dir = { -1, 0, 1, 0, -1, 0 };
        for (int x = 0; x < 4; x++)
        {
            // Get first two dir to get all 4 corner combinations
            int fj = j + dir[x];
            int fi = i + dir[x + 1];
            int sj = j + dir[x + 1];
            int si = i + dir[x + 2];

            // Check if coordinates are legit
            if (IsOutOfBounds(fi, fj) || IsOutOfBounds(si, sj)) 
            {
                continue;
            }

            int fval = map[fj, fi];
            int sval = map[sj, si];

            // Assuming corners will never connect to another corner or tconnectors
            if (fval == 2 && sval == 2) 
            {
                if (x == 0) return 1; // Top & Right - 1
                if (x == 1) return 0; // Right & Bottom - 0
                if (x == 2) return 3; // Bottom & Left - 3
                if (x == 3) return 2; // Left & Top - 2
            }
        }
        return -1;
    }

    // 0 - No rotation, 1 - Rotate Z90, 2 - Rotate Z180, 3 - Rotate Z270
    // Default is a bottom-left corner
    private int CheckInnerC(int i, int j)
    {

        int chosenR = -1;
        int chosenFVal = -1;
        int chosenSVal = -1;

        int[] dir = { -1, 0, 1, 0, -1, 0, 1 };
        for (int x = 0; x < 4; x++)
        {
            // Get first two dir to get all 4 corner combinations
            int fj = j + dir[x];
            int fi = i + dir[x + 1];
            int sj = j + dir[x + 1];
            int si = i + dir[x + 2];

            // Check if coordinates are legit
            if (IsOutOfBounds(fi, fj) || IsOutOfBounds(si, sj))
            {
                continue;
            }
                
            int fval = map[fj, fi];
            int sval = map[sj, si];

            // Faces the direction where it encounters the first two 4/7 walls
            // Assuming walls and TCons are connected to it. 
            // Assuming no adjacent inner corners can be made. (Can't think of a solution for it to have adjacent corners)
            if ((fval == 4 || fval == 3 || fval == 7) && (sval == 4 || sval == 3 || sval == 7))
            {
                if (x == 0) // Top & Right - 0
                {
                    if (fval == 4 && sval == 4) return 0;
                    else if (chosenFVal == -1 && chosenSVal == -1) { chosenR = 0; chosenFVal = fval; chosenSVal = sval; }
                }; 
                if (x == 1) // Right & Bottom - 3
                {
                    if (fval == 4 && sval == 4) return 3;
                    else if (chosenFVal == -1 && chosenSVal == -1) { chosenR = 3; chosenFVal = fval; chosenSVal = sval; }; 
                }; 
                if (x == 2) // Bottom & Left - 2
                {
                    if (fval == 4 && sval == 4) return 2;
                    else if (chosenFVal == -1 && chosenSVal == -1) { chosenR = 2; chosenFVal = fval; chosenSVal = sval; };
                }; 
                if (x == 3) // Left & Top - 1
                {
                    if (fval == 4 && sval == 4) return 1;
                    else if (chosenFVal == -1 && chosenSVal == -1) { chosenR = 1; chosenFVal = fval; chosenSVal = sval; };
                }; 
            }
        }
        return chosenR;
    }

    // 0 - No Rotation, 1 - Z90, 2 - Z180, 3 - Z270
    // 4 - Y180 No Rotation, 5 - Y180 Z90, 6 - Y180 Z180, 7 - Y180 Z270
    private int CheckTCon(int i, int j)
    {
        int[] dir = { -1, 0, 1, 0, -1, 0 };
        for (int x = 0; x < 4; x++)
        {
            // Get first two dir to get all 4 corner combinations
            int fj = j + dir[x];
            int fi = i + dir[x + 1];
            int sj = j + dir[x + 1];
            int si = i + dir[x + 2];

            // Check if coordinates are legit
            if (IsOutOfBounds(fi, fj) || IsOutOfBounds(si, sj))
            {
                continue;
            }

            int fval = map[fj, fi];
            int sval = map[sj, si];

            // Assuming TCon can connect the outer wall/corner to the inner wall/corner
            // Assuming TCon can't connect to another TCon 
            // If there are two adjacent TCon walls, they aren't connected, just simply beside each other. 
            if ((fval == 1 || fval == 2) && (sval == 3 || sval == 4)) {
                if (x == 0) return 7;
                if (x == 1) return 4;
                if (x == 2) return 5;
                if (x == 3) return 6;
            }
            else if ((fval == 3 || fval == 4) && (sval == 1 || sval == 2))
            {
                if (x == 0) return 2;
                if (x == 1) return 1;
                if (x == 2) return 0;
                if (x == 3) return 3;
            }
        }
        return -1;
    }

        private bool IsOutOfBounds(int di, int dj)
    {
        return (di < 0 || di >= map.GetLength(1) || dj < 0 || dj >= map.GetLength(0));
    }

}

// Things to Note: 
// Each grid is 1 unit long 
// For TopL Quadrant: 
// TopL Corner: -20.5x, 10.5y
// TopR Corner: -6.5x, 10.5y | Rotation around Z-axis by 270
// BotR Corner: -7.5x, -3.5y | Rotation by 180
// BotL Corner: -20.5x, -3.5y | Rotation by 90
// Corners can be adjacent to each other
// All tiles/walls are 1,1,1 scale

// Algorithm for TConnectors:
// If current index position is 7, check:
// If bot is 3/4 and left is 1/2 --> No rotation
// If opposite --> Rotate Y by 180 then rotate z by 90
// If top is 3/4 and left is 1/2 --> Rotate Y by 180 then rotate z by 180
// If opposite --> Rotate z by -90
// If top is 3/4 and right is 1/2 --> Rotate Z by 180
// If opposite --> Rotate Y by 180 and Z by -90
// If bot is 3/4 and right is 1/2 --> Rotate Y by 180
// If opposite --> Rotate Z by 90




// Algorithm 1 for InnerW
//int fj = j + dir[x];
//int fi = i + dir[x + 1];
//int sj = j + dir[x + 1];
//int si = i + dir[x + 2];
//int tj = j + dir[x + 2];
//int ti = i + dir[x + 3];

//if (IsOutOfBounds(fi, fj) || IsOutOfBounds(si, sj) || IsOutOfBounds(ti, tj))
//{
//    continue;
//}

// int fval = map[fj, fi];
// int sval = map[sj, si];
// int tval = map[tj, ti];

// Assuming walls/corners/tconnectors adjacent to it are connected to it. 
// Assuming that there is always 1 adjacent tile which is a berry/meat or un-connected wall/corner. 
//if ((fval == 3 || fval == 4 || fval == 7) && (sval == 4 || sval == 5 || sval == 3 || sval == 6) && (tval == 3 || tval == 4 || tval == 7))
//{
//    // Whenever sval == 3 or 4, we face the opposite direction
//    if (sval == 3 || sval == 4)
//    {
//        if (x == 0) return 0;
//        if (x == 1) return 3;
//        if (x == 2) return 2;
//        if (x == 3) return 1;
//    }
//    else
//    {
//        if (x == 0) return 2;
//        if (x == 1) return 1;
//        if (x == 2) return 0;
//        if (x == 3) return 3;
//    }
// }
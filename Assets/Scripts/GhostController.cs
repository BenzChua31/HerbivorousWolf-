using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [SerializeField] private GameObject bunny;
    private Animator animator;
    private UIManager uiManager;
    private AudioManager audioManager;
    private PacStudentController wolfController;
    private Tweener tweener;

    private bool hasBeginCycle; // special var for bunny4 to track if it has started its around-the-map cycle
    private bool hasLeftSpawn; // if the bunny has left spawn area
    private bool isScared; // If the ghost is scared
    private bool isRecovering; // If the ghost is recovering
    private bool isBlinking; // If the timer is blinking
    public static bool quit; // If the game has quit, we want to disable all movement

    private float scaredSeconds = 10;
    private int bunnyType; // 1 : Further, 2 : Closer, 3 : Random, 4 : Clockwise around map along outside of map
   
    private bool flippedH; 
    private bool flippedV;
    private int[,] map;
    private int mapRows;
    private int mapCols;
    private int[] currentPos;
    private int[] spawnEntryPos = { 12, 13 };
    private string currentDirection; // I know enums exist but it's just my preference (W, A, S, D)
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        hasBeginCycle = false;
        hasLeftSpawn = false;
        isScared = false;
        isRecovering = false;
        quit = false;
        tweener = gameObject.GetComponent<Tweener>();
        animator = gameObject.GetComponent<Animator>();
        GameObject managers = GameObject.FindWithTag("Managers");
        uiManager = managers.GetComponent<UIManager>();
        audioManager = managers.GetComponent<AudioManager>();
        wolfController = managers.GetComponent<PacStudentController>();
        map = LevelGenerator.getMap();
        mapRows = map.GetLength(0);
        mapCols = map.GetLength(1);
        InitializeBunnyPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (isScared)
        {
            SetScaredTimer();
        }

        if (!quit)
        {
            if (!hasLeftSpawn) { LeaveSpawn(); }
            if (bunnyType == 4 && hasLeftSpawn && !hasBeginCycle) { }
            // DeterminePath();
        }
    }

    private void LeaveSpawn()
    {
        // Hard-coded as the spawn area will be fixed and I need the bunnies to leave the spawn area
        if ((bunnyType == 1 || bunnyType == 2) && currentPos[0] != 11)
        {
            MoveUp(transform, transform.position, currentPos[0] - 1, currentPos[1]);
        } 
        else if ((bunnyType == 3 || bunnyType == 4) && currentPos[0] != 11)
        {
            MoveBtm(transform, transform.position, currentPos[0] + 1, currentPos[1]);
        }
        else
        {
            hasLeftSpawn = true;
        }
    }

    // 1 : -0.5x 1.5y, 2 : 0.5x 1.5y, 3 : -0.5x 0.5y, 4 : 0.5x 0.5y
    // Determine and initialize their int[] pos based on their in-game coordinates
    private void InitializeBunnyPosition()
    {
        startPosition = bunny.transform.position;
        float x = startPosition.x;
        float y = startPosition.y;

        if (x == -0.5 && y == 1.5) 
        { 
            currentPos = new int[] { 13, 13 };
            flippedH = false; flippedV = false;
            bunnyType = 1;
        }
        else if (x == 0.5 && y == 1.5) 
        { 
            currentPos = new int[] { 13, 13 };
            flippedH = false; flippedV = true;
            bunnyType = 2;
        }
        else if(x == -0.5 && y == 0.5) 
        { 
            currentPos = new int[] { 14, 13 };
            flippedH = false; flippedV = false;
            bunnyType = 3;
        }
        else if(x == 0.5 && y == 0.5) 
        {
            currentPos = new int[] { 14, 13 };
            flippedH = false; flippedV = true;
            bunnyType = 4;
        }
    }

    public void ActivateScaredState() // Also resets if we stack power pellets
    {
        isScared = true;
        isRecovering = false;
        isBlinking = false;
        scaredSeconds = 10.0f;
        animator.SetBool("Panic", true);
        animator.SetBool("Recover", false);
        uiManager.ShowScaredTimer();
        uiManager.StopTimerBlink();
    }

    public bool IsScaredState()
    {
        return isScared;
    }

    private void SetScaredTimer()
    {
        scaredSeconds -= Time.deltaTime;

        if (scaredSeconds <= 0.0f)
        {
            scaredSeconds = 10.0f;
            isScared = false;
            animator.SetBool("Panic", false);
            animator.SetBool("Recover", false);
            uiManager.UpdateScaredTimer(10.0f);
            uiManager.HideScaredTimer();
            uiManager.StopTimerBlink();
            audioManager.StopScaredSong(); // return back to normal song
        }
        else
        {
            uiManager.UpdateScaredTimer(scaredSeconds);
        }

        if (scaredSeconds < 3.0f && scaredSeconds > 0.0f && !isRecovering) // Blinking Bunny at 3secs left
        {
            isRecovering = true;
            animator.SetBool("Recover", true);
        }

        // 3.99 is displayed as 3
        if (scaredSeconds < 4.0f && scaredSeconds > 0.0f && !isBlinking) // Blinking red timer 
        {
            isBlinking = true;
            uiManager.ActivateTimerBlink();
        }

    }

    public void PlayBunnyFuneral()
    {
        animator.SetBool("Dead", true);
        audioManager.PlayBunnyDeath();
        StartCoroutine(RemoveBunny(audioManager.audios[10].clip.length));
    }

    IEnumerator RemoveBunny(float duration) // Temporary
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    // WARNING: The algorithm implemented is not fast or save as much space
    // I couldn't scratch my head enough to come up with a faster algorithm
    // Thought about PriorityQueues w/ custom comparables but nah
    // Thought about using Sort() but that is worst 
    // Thought about A* but not quite sure how to implement it for a moving target 
       // Update: it is possible but it's too late, I already implemented an algorithm and got other assignments
       // during holidays i guess 
    // Current Algorithm is BFS: where we determine the next adjacent cell to move to based on DIST
    private void DeterminePath() // chooses one of the paths out of the available paths
    {
        Transform transform = gameObject.transform;

        if (!tweener.TweenExists(transform))
        {
            int row = currentPos[0];
            int col = currentPos[1];

            Dictionary<string, int[]> rs = GetAvailablePaths(row, col);

            // Order: left, top, right, bot
            List<int[]> dir = new List<int[]>();
            dir[0] = null; dir[1] = null;
            dir[2] = null; dir[3] = null;

            // -1 means null 
            int[] opts = { -1, -1, -1, -1 };

            // CalculateDistance of adjacent cell to wolf
            if (rs.ContainsKey("left"))
            {
                int[] leftrs = rs["left"];
                dir[0] = leftrs;
                opts[0] = CalculateDistToPac(leftrs[0], leftrs[1], leftrs[2], leftrs[3]);
            }
            if (rs.ContainsKey("top"))
            {
                int[] toprs = rs["top"];
                dir[1] = toprs;
                opts[1] = CalculateDistToPac(toprs[0], toprs[1], toprs[2], toprs[3]);
            }
            if (rs.ContainsKey("right"))
            {
                int[] rightrs = rs["right"];
                dir[2] = rightrs;
                opts[2] = CalculateDistToPac(rightrs[0], rightrs[1], rightrs[2], rightrs[3]);
            }
            if (rs.ContainsKey("bot"))
            {
                int[] botrs = rs["bot"];
                dir[3] = botrs;
                opts[3] = CalculateDistToPac(botrs[0], botrs[1], botrs[2], botrs[3]);
            }

            // Path determination for each direction will be in their own methods
            if (currentDirection.Equals("W")) { MoveDirectionW(dir, opts); }
            else if (currentDirection.Equals("A")) { MoveDirectionA(dir, opts); }
            else if (currentDirection.Equals("S")) { MoveDirectionS(dir, opts); }
            else if (currentDirection.Equals("D")) { MoveDirectionD(dir, opts); }

        }
    }

    private void MoveDirectionW(List<int[]> dir, int[] opts)
    {
        int bestPath;

        bestPath = GetBestPath(opts[0], opts[1], opts[2]);
        // Check if our preferred path can be taken. 
        // if dir is null, just straight away skip.
        if (bestPath == opts[1] && dir[1] != null)
        {
            MoveUp(transform, transform.position, dir[1][0], dir[1][1]);
        }
        else
        {
            bestPath = GetBestPath(opts[0], -1, opts[2]);
            // If preferred path unable to take, evaluate between the left and right path
            if (bestPath == opts[0] && dir[0] != null)
            {
                MoveLeft(transform, transform.position, dir[0][0], dir[0][1]);
            }
            else if (bestPath == opts[2] && dir[2] != null)
            {
                MoveRight(transform, transform.position, dir[2][0], dir[2][1]);
            }
            else
            {
                // Worst comes to worst, we have to backtrack (if we reached here, means this is the only path)
                // As there will always at least be a path
                MoveBtm(transform, transform.position, dir[3][0], dir[3][1]);
            }
        }
    }

    private void MoveDirectionA(List<int[]> dir, int[] opts)
    {
        int bestPath;

        bestPath = GetBestPath(opts[3], opts[0], opts[1]);
        if (bestPath == opts[0] && dir[0] != null)
        {
            MoveLeft(transform, transform.position, dir[0][0], dir[0][1]);
        }
        else
        {
            bestPath = GetBestPath(opts[3], -1, opts[1]);
            if (bestPath == opts[1] && dir[1] != null)
            {
                MoveUp(transform, transform.position, dir[1][0], dir[1][1]);
            }
            else if (bestPath == opts[3] && dir[3] != null)
            {
                MoveBtm(transform, transform.position, dir[3][0], dir[3][1]);
            }
            else
            {
                MoveRight(transform, transform.position, dir[2][0], dir[2][1]);
            }
        }
    }

    private void MoveDirectionS(List<int[]> dir, int[] opts)
    {
        int bestPath;

        bestPath = GetBestPath(opts[2], opts[3], opts[0]);
        if (bestPath == opts[3] && dir[3] != null)
        {
            MoveBtm(transform, transform.position, dir[3][0], dir[3][1]);
        }
        else
        {
            bestPath = GetBestPath(opts[2], -1, opts[0]);
            if (bestPath == opts[2] && dir[2] != null)
            {
                MoveRight(transform, transform.position, dir[2][0], dir[2][1]);
            }
            else if (bestPath == opts[0] && dir[0] != null)
            {
                MoveLeft(transform, transform.position, dir[0][0], dir[0][1]);
            }
            else
            {
                MoveUp(transform, transform.position, dir[1][0], dir[1][1]);
            }
        }
    }

    private void MoveDirectionD(List<int[]> dir, int[] opts)
    {
        int bestPath;

        bestPath = GetBestPath(opts[1], opts[2], opts[3]);
        if (bestPath == opts[2] && dir[2] != null)
        {
            MoveRight(transform, transform.position, dir[2][0], dir[2][1]);
        }
        else
        {
            bestPath = GetBestPath(opts[1], -1, opts[3]);
            if (bestPath == opts[1] && dir[1] != null)
            {
                MoveUp(transform, transform.position, dir[1][0], dir[1][1]);
            }
            else if (bestPath == opts[3] && dir[3] != null)
            {
                MoveBtm(transform, transform.position, dir[3][0], dir[3][1]);
            }
            else
            {
                MoveLeft(transform, transform.position, dir[0][0], dir[0][1]);
            }
        }
    }

    private int GetBestPath(int op1, int op2, int op3)
    {
        if (bunnyType == 1) { return MovementType1(op1, op2, op3); }
        else if (bunnyType == 2) { return MovementType2(op1, op2, op3); }
        else if (bunnyType == 3) { return MovementType3(op1, op2, op3); }
        else { return MovementType4(op1, op2, op3); }
    }

    private int MovementType1(int op1, int op2, int op3) // Furthest from pacstudent
    {
        // this one doesn't need a -1 filter.
        // if max returns -1, then all options are null
        return Mathf.Max(op1, op2, op3);
    }

    private int MovementType2(int op1, int op2, int op3) // Closest to PacStudent
    {
        // Do this so that it won't return -1 values
        // 1405 is the max possible distance you can get
        if (op1 == -1) { op1 = 1405; }
        if (op2 == -1) { op2 = 1405; }
        if (op3 == -1) { op3 = 1405; }

        return Mathf.Min(op1, op2, op3);
    }

    private int MovementType3(int op1, int op2, int op3) // Random
    {
        List<int> ops = new List<int>();
        if (op1 != -1) { ops.Add(op1); }
        if (op2 != -1) { ops.Add(op2); }
        if (op3 != -1) { ops.Add(op3); }

        int choice = Random.Range(0, ops.Count); 
        if (choice == 0) { return ops[0]; }
        else if (choice == 1) { return ops[1]; }
        else { return ops[2]; }
    }

    // Prioritize sticking to the outer walls (moving clockwise)
    // So movement priority is left --> top --> right (W)
    // bot --> left --> top (A)
    // right --> bot --> left (S)
    // top --> right --> bot (D)

    private int MovementType4(int op1, int op2, int op3)
    {
        if (op1 != -1) { return op1; } // Prioritize op1 (disregarding dist)
        else if (op2 != -1) { return op2; } // Else prioritize op2
        else { return op3; } // Lastly op3
        // if all -1, then their dir will be null and the final option will be called instead
    }

    // For type4 bunny to move to the bottom left corner (1, 1), fH = true, to begin its clockwise cycle
    private int CalculateDistToEdge(int row, int col, int fH, int fV)
    {
        int blR = 1;
        int blC = 1;
        int blFH = 1;
        int blFV = 0;

        int diffX;
        int diffY;

        // final index 13 + final index 14 (13 + 14 = 27)
        if (fH == blFH) { diffY = Mathf.Abs(row - blR); }
        else { diffY = 27 - blR - row + 1; }

        // final index 13 for both sides (13 + 13 = 26)
        if (fV == blFV) { diffX = Mathf.Abs(col - blC); }
        else { diffX = 26 - blC - col + 1; }

        return diffX * diffX + diffY * diffY; // don't need to sqrt, it is still unique
    }

    private int CalculateDistToPac(int row, int col, int fH, int fV)
    {
        int[] wolfPos = wolfController.GetWolfPosition();
        int wolfR = wolfPos[0];
        int wolfC = wolfPos[1];
        int wolfFH = wolfPos[2];
        int wolfFV = wolfPos[3];

        int diffX;
        int diffY;

        // final index 13 + final index 14 (13 + 14 = 27)
        if (fH == wolfFH) { diffY = Mathf.Abs(row - wolfR); } 
        else { diffY = 27 - wolfR - row + 1; }

        // final index 13 for both sides (13 + 13 = 26)
        if (fV == wolfFV) { diffX = Mathf.Abs(col - wolfC); }
        else { diffX = 26 - wolfC - col + 1; }

        return diffX * diffX + diffY * diffY; // don't need to sqrt, it is still unique
    }

    private Dictionary<string, int[]> GetAvailablePaths(int row, int col) 
    {
        // there will also be at least 1 available path (a corner, will give one path only)
        Dictionary<string, int[]> rs = new Dictionary<string, int[]>();

        int[] leftrs = CheckLeftPath(row, col);
        if (leftrs != null) { rs.Add("left", leftrs); }

        int[] toprs = CheckTopPath(row, col);
        if (toprs != null) { rs.Add("top", toprs); }

        int[] rightrs = CheckRightPath(row, col);
        if (rightrs != null) { rs.Add("right", rightrs); }

        int[] botrs = CheckBtmPath(row, col);
        if (botrs != null) { rs.Add("bot", botrs); }
        
        return rs;
    }

    // It is the same as MoveUp() in PacStudentController except, we don't want to immediately execute a tween
    // Instead, we will return the results (coordinate and quadrant) and if the option is chosen, then we simply 
    // update the position.
    private int[] CheckTopPath(int row, int col)
    {
        int adjRow = row;
        int adjCol = col;
        int fH = (flippedH) ? 1 : 0;
        int fV = (flippedV) ? 1 : 0;

        if (flippedH) { adjRow += 1; }
        else { adjRow -= 1; }

        int rs = CheckWalkable(row, col);

        if (rs != 0) 
        { 
            if (rs == 2) // A flip must be done
            {
                fH = Mathf.Abs(fH - 1);
                return new int[] { adjRow, adjCol, fH, fV }; 
                // 2nd and 3rd index will help indicate which quad this coordinate belongs to
            }
            else
            {
                return new int[] { adjRow, adjCol, fH, fV }; 
            }
        }
        else
        {
            return null;
        }
    }

    private int[] CheckBtmPath(int row, int col)
    {
        int adjRow = row;
        int adjCol = col;
        int fH = (flippedH) ? 1 : 0;
        int fV = (flippedV) ? 1 : 0;

        if (flippedH) { adjRow -= 1; }
        else { adjRow += 1; }

        int rs = CheckWalkable(row, col);

        if (rs != 0)
        {
            if (rs == 2)
            {
                fH = Mathf.Abs(fH - 1);
                return new int[] { adjRow, adjCol, fH, fV };
            }
            else
            {
                return new int[] { adjRow, adjCol, fH, fV };
            }
        }
        else
        {
            return null;
        }
    }

    private int[] CheckLeftPath(int row, int col)
    {
        int adjRow = row;
        int adjCol = col;
        int fH = (flippedH) ? 1 : 0;
        int fV = (flippedV) ? 1 : 0;

        if (flippedV) { adjCol += 1; }
        else { adjCol -= 1; }

        int rs = CheckWalkable(row, col);

        if (rs != 0)
        {
            if (rs == 3)
            {
                fV = Mathf.Abs(fV - 1);
                return new int[] { adjRow, adjCol, fH, fV };
            }
            else
            {
                return new int[] { adjRow, adjCol, fH, fV };
            }
        }
        else
        {
            return null;
        }
    }

    private int[] CheckRightPath(int row, int col)
    {
        int adjRow = row;
        int adjCol = col;
        int fH = (flippedH) ? 1 : 0;
        int fV = (flippedV) ? 1 : 0;

        if (flippedV) { adjCol -= 1; }
        else { adjCol += 1; }

        int rs = CheckWalkable(row, col);

        if (rs != 0)
        {
            if (rs == 3)
            {
                fV = Mathf.Abs(fV - 1);
                return new int[] { adjRow, adjCol, fH, fV };
            }
            else
            {
                return new int[] { adjRow, adjCol, fH, fV };
            }
        }
        else
        {
            return null;
        }
    }

    private void MoveUp(Transform transform, Vector3 pos, int row, int col)
    {
        transform.eulerAngles = new Vector3(0, 0, 90.0f);
        tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y + 1.0f), 0.7f);
        currentPos[0] = row;
        currentPos[1] = col;
    }

    private void MoveBtm(Transform transform, Vector3 pos, int row, int col)
    {
        transform.eulerAngles = new Vector3(0, 0, 270.0f);
        tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y - 1.0f), 0.7f);
        currentPos[0] = row;
        currentPos[1] = col;
    }

    private void MoveLeft(Transform transform, Vector3 pos, int row, int col)
    {
        transform.eulerAngles = new Vector3(0, 180.0f, 0);
        tweener.AddTween(transform, pos, new Vector2(pos.x - 1.0f, pos.y), 0.7f);
        currentPos[0] = row;
        currentPos[1] = col;
    }

    private void MoveRight(Transform transform, Vector3 pos, int row, int col)
    {
        transform.eulerAngles = new Vector3(0, 0, 0);
        tweener.AddTween(transform, pos, new Vector2(pos.x + 1.0f, pos.y), 0.7f);
        currentPos[0] = row;
        currentPos[1] = col;
    }

    // 0 -  false, 1 - true, 2 -  true + flipH, 3 - true + flipV
    private int CheckWalkable(int row, int col)
    {
        // Check if coordinates are out of bounds first
        if (IsOutOfBounds(col, row))
        {
            return 0;
        }

        // Check if it is spawn entrance (we want to prevent them from re-entering)
        if (IsSpawnEntrance(row, col)) { return 0; }

        // Check if about to enter teleporter entrance
        if (IsEnteringTeleporter(col, row)) { return 0; }

        // Check if entering a mirrored quadrant 
        int rs = IsEnteringMirrored(col, row);

        if (rs == 1) // set FlipH to indicate that we are now in the flipped quadrant
        {
            return 2; // If we are entering a mirrored quadrant, it is guaranteed that it is walkable
        }
        else if (rs == 2) // set FlipV to indicate that we are now in the flipped quadrant
        {
            return 3;
        }
        else
        { // If it is just regular movement within a quadrant, we then check for walls etc... 
            int dest = map[row, col];
            if (dest != 1 && dest != 2 && dest != 3 && dest != 4 && dest != 7) { return 1; }
            else { return 0; }
        }

    }

    // Check if it is a spawn entrance
    private bool IsSpawnEntrance(int di, int dj)
    {
        return dj != spawnEntryPos[0]; // hard-coded entrance y-axis coordinate since spawn area won't be changed
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

    // 0 - False, 1 - TeleporterEntrance
    // If it is going to enter the entrance of the teleporter (used for preventing ghost from using it)
    // ASSUMING entrance is 1 grid wide
    private bool IsEnteringTeleporter(int di, int dj)
    {
        if (di == 0 || dj == 0)
        {
            return false; // Entrance to a teleporter cannot be on the edge of the map
        }

        // Get the adjacent values
        // An entrance will have the 2 outer corners (1) besides it (left + right) or (top + btm)
        int top = map[dj - 1, di];
        int btm = map[dj + 1, di];
        int left = map[dj, di - 1];
        int right = map[dj, di + 1];

        if (top == 1 && btm == 1) // top and btm pattern means it is a left/right entrance to the teleporter
        {
            return true;
        }
        else if (left == 1 && right == 1) // left and right pattern, means top/btm entrance
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}

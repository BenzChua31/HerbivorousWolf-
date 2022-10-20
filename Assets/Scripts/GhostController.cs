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
    private string currentDirection; // I know enums exist but it's just my preference (W, A, S, D)
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
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
            // DeterminePath();
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

    private void DeterminePath() // chooses one of the paths out of the available paths
    {
        Transform transform = gameObject.transform;

        if (!tweener.TweenExists(transform))
        {
            int row = currentPos[0];
            int col = currentPos[1];

            Dictionary<string, int[]> rs = GetAvailablePaths(row, col);

            int[] leftrs = null;
            int[] toprs = null;
            int[] rightrs = null;
            int[] botrs = null;

            // The dist to PacStudent for each adjacent node (1625 is the maxDist w/o sqrt)
            int left = 1625;
            int top = 1625;
            int right = 1625;
            int bot = 1625;

            // CalculateDistance of adjacent cell to wolf
            if (rs.ContainsKey("left"))
            {
                leftrs = rs["left"];
                left = CalculateDist(leftrs[0], leftrs[1], leftrs[2], leftrs[3]);
            }
            if (rs.ContainsKey("top"))
            {
                toprs = rs["top"];
                top = CalculateDist(toprs[0], toprs[1], toprs[2], toprs[3]);
            }
            if (rs.ContainsKey("right"))
            {
                rightrs = rs["right"];
                right = CalculateDist(rightrs[0], rightrs[1], rightrs[2], rightrs[3]);
            }
            if (rs.ContainsKey("bot"))
            {
                botrs = rs["bot"];
                bot = CalculateDist(botrs[0], botrs[1], botrs[2], botrs[3]);
            }

            // Get Best Path (could prob break this part of the code and split it into different sections for bunnyType)
            int bestPath = Mathf.Min(left, top, right, bot); 

            // Prioritize keeping the original path direction
            if (currentDirection.Equals("W"))
            {
                if (bestPath == top && toprs != null) 
                { MoveUp(transform, transform.position, toprs[0], toprs[1]); }

                else if (bestPath == left && leftrs != null) 
                { MoveLeft(transform, transform.position, leftrs[0], leftrs[1]); }

                else if (bestPath == right && rightrs != null) 
                { MoveRight(transform, transform.position, rightrs[0], rightrs[1]); }

                else 
                { MoveBtm(transform, transform.position, botrs[0], botrs[1]); } 
                // If all above option fails, means bot is def the only option (backtracking is the only option)
            }
            else if (currentDirection.Equals("A"))
            {
                if (bestPath == left && leftrs != null)
                { MoveLeft(transform, transform.position, leftrs[0], leftrs[1]); }

                else if (bestPath == bot && botrs != null)
                { MoveBtm(transform, transform.position, botrs[0], botrs[1]); }

                else if (bestPath == top && toprs != null)
                { MoveUp(transform, transform.position, toprs[0], toprs[1]); }

                else
                { MoveRight(transform, transform.position, rightrs[0], rightrs[1]); }
            }
            else if (currentDirection.Equals("S"))
            {
                if (bestPath == bot && botrs != null)
                { MoveBtm(transform, transform.position, botrs[0], botrs[1]); }

                else if (bestPath == right && rightrs != null)
                { MoveRight(transform, transform.position, rightrs[0], rightrs[1]); }

                else if (bestPath == left && leftrs != null)
                { MoveLeft(transform, transform.position, leftrs[0], leftrs[1]); }

                else 
                { MoveUp(transform, transform.position, toprs[0], toprs[1]); }
            }
            else if (currentDirection.Equals("D"))
            {
                if (bestPath == right && rightrs != null)
                { MoveRight(transform, transform.position, rightrs[0], rightrs[1]); }

                else if (bestPath == top && toprs != null)
                { MoveUp(transform, transform.position, toprs[0], toprs[1]); }

                else if (bestPath == bot && botrs != null)
                { MoveBtm(transform, transform.position, botrs[0], botrs[1]); }

                else 
                { MoveLeft(transform, transform.position, leftrs[0], leftrs[1]); }
            }

        }
    }

    private int CalculateDist(int row, int col, int fH, int fV)
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

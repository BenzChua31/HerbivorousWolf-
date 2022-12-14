using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{

    [SerializeField] private GameObject wolf;
    public Sprite[] wolfState;
    private AudioManager audioManager;
    private UIManager uiManager;
    private Tweener tweener;
    private Animator animator;
    public static bool quit;
    private static bool colPlayed = false;
    private bool isListening = true;
    private int lives = 3;
    private string lastInput = "";
    private string currentInput = "";
    private bool flippedH = false; // Initially it isn't flipped, first quad (TL)
    private bool flippedV = false;
    private int[,] map;
    private int[] currentPos = { 1, 1 }; // Fixed Start Position in relation to the 2D map
    private Vector3 startPosition;
    private int berryCount;

    // Start is called before the first frame update
    void Start()
    {
        quit = false;
        tweener = gameObject.GetComponent<Tweener>();
        map = LevelGenerator.GetMap();
        audioManager = GameObject.FindWithTag("Managers").GetComponent<AudioManager>();
        uiManager = GameObject.FindWithTag("Managers").GetComponent<UIManager>();
        animator = wolf.GetComponent<Animator>();
        startPosition = wolf.transform.position;
        BerryCount();
    }

    // Update is called once per frame
    void Update()
    {


        if (isListening)
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
        }

        if (!quit)
        {
            int statusL = Move(lastInput);
            if (statusL == 0) // if CheckWalkable returns a false, then we execute this
            {
                int statusC = Move(currentInput); // shuld naturally stop if no possible motion can be made
                if (statusC == 0)
                {
                    wolf.GetComponent<ParticleSystem>().Stop();
                    if (!currentInput.Equals("") && !colPlayed) { PlayCollisionFX(); colPlayed = true; }
                    StopEffects();
                }
                else
                {
                    colPlayed = false;
                }
            }
            else
            {
                colPlayed = false;
            }
        }
        else
        {
            wolf.GetComponent<ParticleSystem>().Stop();
            StopEffects();
        }

    }

    private int Move(string inputType)
    {
        Transform transform = wolf.GetComponent<Transform>();
        Vector3 pos = wolf.transform.position;

        int row = currentPos[0];
        int col = currentPos[1];
        // Debug.Log(row + " " + col); // Coordinate tested, works 100%

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

            int teleport = IsEnteringTeleporter(col, row);
            if (teleport == 1)
            {
                if (!flippedH)
                {
                    flippedH = true;
                    currentPos[0] = row;
                    currentPos[1] = col;
                    Teleport(1);
                    return 1;
                }
            }

            int rs = CheckWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by CheckWalkable method
            {
                PlayParticles();
                PlayEffects();
                transform.eulerAngles = new Vector3(0, 0, 90.0f);
                currentInput = "W";
                tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y + 1.0f), 0.7f);
                if (rs == 2)
                {
                    currentPos[0] = row - 1;
                    currentPos[1] = col;
                    return 1;
                }
                else
                {
                    currentPos[0] = adjRow;
                    currentPos[1] = adjCol; // Because the bottom row of flippedH quadrants have been removed
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

            int teleport = IsEnteringTeleporter(col, row);
            if (teleport == 1)
            {
                if (flippedH)
                {
                    flippedH = false;
                    currentPos[0] = row;
                    currentPos[1] = col;
                    Teleport(1);
                    return 1;
                }
            }

            int rs = CheckWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by CheckWalkable method
            {
                PlayParticles();
                PlayEffects();
                transform.eulerAngles = new Vector3(0, 0, 270.0f);
                currentInput = "S";
                tweener.AddTween(transform, pos, new Vector2(pos.x, pos.y - 1.0f), 0.7f);
                if (rs == 2)
                {
                    currentPos[0] = row - 1;
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

            int teleport = IsEnteringTeleporter(col, row);
            if (teleport == 2)
            {
                if (!flippedV)
                {
                    flippedV = true;
                    currentPos[0] = row;
                    currentPos[1] = col;
                    Teleport(2);
                    return 1;
                }
            }

            int rs = CheckWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by CheckWalkable method
            {
                PlayParticles();
                PlayEffects();
                transform.eulerAngles = new Vector3(0, 180.0f, 0);
                currentInput = "A";
                tweener.AddTween(transform, pos, new Vector2(pos.x - 1.0f, pos.y), 0.7f);
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

            int teleport = IsEnteringTeleporter(col, row);
            if (teleport == 2)
            {
                if (flippedV)
                {
                    flippedV = false;
                    currentPos[0] = row;
                    currentPos[1] = col;
                    Teleport(2);
                    return 1;
                }
            }

            int rs = CheckWalkable(adjRow, adjCol);
            if (rs != 0) // The flippedH/V is handled by CheckWalkable method
            {
                PlayParticles();
                PlayEffects();
                transform.eulerAngles = new Vector3(0, 0, 0);
                currentInput = "D";
                tweener.AddTween(transform, pos, new Vector2(pos.x + 1.0f, pos.y), 0.7f);
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
    private int CheckWalkable(int row, int col)
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

    // 0 - False, 1 - TopTeleporter, 2 - LeftTeleporter
    private int IsEnteringTeleporter(int di, int dj)
    {
        if (dj == 0 && di == 0) { return 0; } // Top Left corner won't have a teleporter
        else if (dj == 0) { return 1; } // TopTeleporter
        else if (di == 0) { return 2; } // LeftTeleporter
        else { return 0; } // Is neither 
    }

    private void PlayEffects()
    {
        animator.SetBool("Eating", true);
        audioManager.PlayRustlingLeaves();
    }

    private void StopEffects()
    {
        animator.SetBool("Eating", false);
        audioManager.StopRustlingLeaves();
    }

    private void PlayParticles()
    {
        ParticleSystem ps = wolf.GetComponent<ParticleSystem>();

        if (ps.isPlaying == false)
        {
            wolf.GetComponent<ParticleSystem>().Play();
            // Stop child particlesystem from playing along parent's
            wolf.GetComponent<Transform>().Find("CollisionFx").GetComponent<ParticleSystem>().Stop();
            wolf.GetComponent<Transform>().Find("DeathFx").GetComponent<ParticleSystem>().Stop();
        }
    }

    private void StopParticles()
    {
        wolf.GetComponent<ParticleSystem>().Stop();
    }

    private void PlayCollisionFX()
    {
        GameObject colFX = wolf.GetComponent<Transform>().Find("CollisionFx").gameObject;
        GameObject newColFX = Instantiate(colFX, colFX.GetComponent<Transform>().position, Quaternion.identity);
        newColFX.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DeleteCollisionFX(newColFX));
        audioManager.PlayKnockWall();
    }

    IEnumerator DeleteCollisionFX(GameObject colFX)
    {
        while (!colFX.GetComponent<ParticleSystem>().isStopped)
        {
            yield return null;
        }
        Destroy(colFX);
    }

    private void Teleport(int flip)
    {
        Transform transform = wolf.GetComponent<Transform>();
        SpriteRenderer rend = wolf.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = wolf.GetComponent<CircleCollider2D>();
        Animator anim = wolf.GetComponent<Animator>();
        Vector3 start = transform.position;
        Vector3 end = start;

        if (flip == 1) { end = new Vector3(start.x, -start.y, 0); }
        else if (flip == 2) { end = new Vector3(-start.x, start.y, 0); }

        collider.enabled = false;
        anim.enabled = false;
        rend.sprite = wolfState[1];
        StopEffects();
        transform.GetComponent<ParticleSystem>().Stop();
        tweener.AddTween(transform, start, end, 1.0f);
        StartCoroutine(EnableRend(rend, transform, anim, collider));
    }

    IEnumerator EnableRend(SpriteRenderer rend, Transform transform, Animator animator, CircleCollider2D collider)
    {
        while (tweener.TweenExists(transform))
        {
            yield return null;
        }
        collider.enabled = true;
        animator.enabled = true;
        rend.sprite = wolfState[0];
        PlayEffects();
        PlayParticles();
        yield return null;
    }

    public void PlayWolfFuneral()
    {
        isListening = false; // Disable InputListener
        lastInput = ""; currentInput = ""; // Reset Inputs 
        tweener.RemoveTweens(); // RemoveAllTweens 
        PlayDeathFX();
    }

    private void PlayDeathFX() // Play death animation, sfx and particle effect
    {
        GameObject deathFX = wolf.GetComponent<Transform>().Find("DeathFx").gameObject;
        GameObject newDeathFX = Instantiate(deathFX, deathFX.GetComponent<Transform>().position, Quaternion.identity);
        animator.SetBool("Dead", true);
        audioManager.PlayPerish();
        newDeathFX.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DeleteDeathFX(newDeathFX)); // Detector to delete the death FX after it is done playing
        if (lives > 1) { StartCoroutine(ReduceLife(Mathf.Ceil(audioManager.audios[6].clip.length))); }
        else { EndGame(); }
    }

    IEnumerator ReduceLife(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Reduce Life after death animation ends
        lives -= 1;
        uiManager.ReduceLife(); 
        // reset to start Position
        currentPos[0] = 1; currentPos[1] = 1; 
        flippedH = false; flippedV = false;
        wolf.transform.position = startPosition;
        wolf.transform.rotation = Quaternion.identity;
        // reset wall collision detector
        colPlayed = false;
        // reset wolf animation back to normal from dead state
        animator.SetBool("Dead", false);
        StopEffects();
        StopParticles();
        // Enable inputlistener
        isListening = true;
    }

    IEnumerator DeleteDeathFX(GameObject deathFX)
    {
        while (!deathFX.GetComponent<ParticleSystem>().isStopped)
        {
            yield return null;
        }
        Destroy(deathFX);
    }

    private void EndGame()
    {
        uiManager.ShowGameOver(); // show gameover text
        uiManager.DisableGame(); // Disable the game, doesn't stop Time
        uiManager.SaveGameTimerAndScore(); // Save the score and timer if needed
    }

    private void BerryCount()
    {
        int extraBerriesToDelete = 0;
        berryCount = 0;
        for (int j = 0; j < map.GetLength(0); j++) 
        {
            for (int i = 0; i < map.GetLength(1); i++) 
            {
                if (map[j, i] == 5) 
                { 
                    berryCount++; 
                    if (j == map.GetLength(0) - 1)
                    {
                        extraBerriesToDelete++;
                    }
                }
            }
        }
        berryCount = (berryCount * 4) - (extraBerriesToDelete * 2);
    }

    public void ConsumeBerry() 
    { 
        berryCount -= 1; 
        if (berryCount == 0)
        {
            EndGame();
        }
    }

    public void DisableInputListener() { isListening = false; }
    public void EnableInputListener() { isListening = true; }

    // indicates which quadrant and position the wolf is in 
    public int[] GetWolfPosition() 
    {
        int fH = (flippedH) ? 1 : 0;
        int fV = (flippedV) ? 1 : 0;
        return new int[] { currentPos[0], currentPos[1], fH, fV };
    }

}

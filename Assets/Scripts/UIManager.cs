using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Handles all the UI interface & Loading Screens 
public class UIManager : MonoBehaviour
{
    private static UIManager itself;
    public RectTransform loadingTitle;
    public RectTransform loadingCanva;
    private Text countDown;
    private bool gameStarted = false;
    private bool startCountdown = false;
    private float countDownTime;
    private AudioManager audioManager;
    private static Text scorePts;
    private static int score;
    private Text gameTimer;
    private float gameSeconds;
    private Text scaredLabel;
    private Text scaredTimer;
    private bool isBlinking = false;
    private static List<Image> livesImg;


    private void Awake()
    {
        // Make it so that we will only have one instance of UIManager (Singleton approach)
        if (!itself) { itself = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // Start is called before the first frame update
    void Start()
    {
        livesImg = new List<Image>();
        GameObject.FindWithTag("Lvl1Btn").GetComponent<Button>().onClick.AddListener(LoadFirst);
        // Sets the top and bottom of the loadingTitle
        loadingTitle.offsetMin = new Vector2(loadingTitle.offsetMin.x, -loadingCanva.rect.height);
        loadingTitle.offsetMax = new Vector2(loadingTitle.offsetMax.x, -loadingCanva.rect.height);
        audioManager = gameObject.GetComponent<AudioManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        countDownTime = audioManager.audios[0].clip.length;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted && startCountdown)
        {
            countDownTime -= Time.deltaTime;
            SetCountDown((int) Mathf.Ceil(countDownTime));
        }

        if (gameStarted && gameTimer != null)
        {
            gameSeconds += Time.deltaTime;
            SetGameTimer(gameSeconds);
        }
    }

    public void Exit()
    {
        PacStudentController.quit = true;
        audioManager.PlayMainMenu();
        ShowLoading(1);
    }

    public void LoadFirst()
    {
        ShowLoading(0);
    }

    private void ShowLoading(int index)
    {
        StopAllCoroutines();
        StartCoroutine(Loading(index));
    }

    IEnumerator Loading(int index)
    {

        float elapsedTime = 0.0f;
        float duration = 2.0f;

        SetGameState(index);

        while (elapsedTime < duration)
        {
            loadingTitle.anchoredPosition = Vector3.Lerp(loadingTitle.anchoredPosition, new Vector2(0.0f, 0.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);

        yield return new WaitForSeconds(1.0f);

        elapsedTime = 0.0f;
        duration = 2.0f;
        bool played = false;

        while (elapsedTime < duration)
        { // the -600.0f is hard-coded for aspect ratio 4:3 
            loadingTitle.anchoredPosition = Vector3.Lerp(loadingTitle.anchoredPosition, new Vector2(0.0f, -loadingCanva.rect.height - 500.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            // REMINDER: MAKE THIS DYNAMIC IF POSSIBLE
            // Added this because I don't want the music to play onSceneLoad, but when LoadingScreen is sliding off
            // Since the LoadingScreen timing is hard-coded (Could probably check w/ GetCurrentScene for dynamic)
            if (played == false && GameStateManager.currentScene == GameStateManager.SceneType.Level)
            {
                played = true; // To prevent PlayLevel from being called more than once
                audioManager.PlayLevel();
                StartCountDown();
            }

            yield return null;
        }

        ResetLoadingTitle(); // Hopefully by now the Level has been loaded, hard-coded, same for loadingScreen duration anyways

        yield return null;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            GameObject.FindWithTag("Exit").GetComponent<Button>().onClick.AddListener(Exit);
            countDown = GameObject.FindWithTag("Countdown").GetComponent<Text>();
            scorePts = GameObject.FindWithTag("Score").GetComponent<Text>();
            gameTimer = GameObject.FindWithTag("GameTimer").GetComponent<Text>();
            scaredTimer = GameObject.FindWithTag("ScaredTimer").GetComponent<Text>();
            // Slightly hard-coded since GetComponentInChildren<Text>() does not work (likely a bug?)
            scaredLabel = GameObject.Find("ScaredTimer").GetComponent<Text>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Life")) { livesImg.Add(go.GetComponent<Image>()); }
            score = 0;
            gameSeconds = 0;
            OnStartHold();
        }
        else if (scene.buildIndex == 1)
        {
            GameObject.FindWithTag("Lvl1Btn").GetComponent<Button>().onClick.AddListener(LoadFirst);
        }
    }

    // Pause the game on start until the loadingScreen is done loading
    // Reason why it is better to not set timescale = 0 because loadingscreen animation depends on it, as well as the countdown
    private void OnStartHold() 
    {
        // Disable Cherry Spawn 
        GameObject.FindWithTag("CherryController").GetComponent<CherryController>().DisableSpawn(); 
        // Disable gameTimer
        gameStarted = false;
        // Disable wolf collider
        GameObject.FindWithTag("Wolf").GetComponent<CircleCollider2D>().enabled = false;
        // Disable InputListener (don't want to detect any inputs or pre-store inputs)
        GameObject.FindWithTag("WolfController").GetComponent<PacStudentController>().DisableInputListener();
        // Disable BunnyMovement

    }

    private void OnStartUnhold() // After countdown ends 
    {
        GameObject.FindWithTag("CherryController").GetComponent<CherryController>().EnableSpawn();
        gameStarted = true;
        GameObject.FindWithTag("Wolf").GetComponent<CircleCollider2D>().enabled = true;
        GameObject.FindWithTag("WolfController").GetComponent<PacStudentController>().EnableInputListener();

    }

    private void StartCountDown()
    {
        startCountdown = true;
    }

    private void SetCountDown(int second)
    {
        if (!countDown.enabled && second >= 0) { countDown.enabled = true; }
        else if (countDown.enabled && second < 0) 
        { 
            countDown.enabled = false;
            OnStartUnhold();
        }

        if (second >= 0)
        {
            if (second < 1) { countDown.text = "GO!"; }
            else if (second > 3) { countDown.text = ""; }
            else { countDown.text = second.ToString(); }
        }
    }

    public void PauseGame()
    {
        // Wolf movement, bunny, cherry movement use tween which depends on deltaTime
        // cherry timer, game timer, scared timer depends on Time too
        Time.timeScale = 0;
        // Disable Input listener, don't want to detect any inputs while paused
        GameObject.FindWithTag("WolfController").GetComponent<PacStudentController>().DisableInputListener();
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        GameObject.FindWithTag("WolfController").GetComponent<PacStudentController>().EnableInputListener();
    }

    private void SetGameState(int index)
    {
        if (index == 0) { GameStateManager.currentScene = GameStateManager.SceneType.Level; }
        else if (index == 1) { GameStateManager.currentScene = GameStateManager.SceneType.Start; }
    }

    private void ResetLoadingTitle()
    {
        loadingCanva.GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        loadingTitle.offsetMin = new Vector2(loadingTitle.offsetMin.x, -loadingCanva.rect.height);
        loadingTitle.offsetMax = new Vector2(loadingTitle.offsetMax.x, -loadingCanva.rect.height);
    }

    // UIManager is a Singleton anyways, so why not Static for direct access
    public static void AddScore(int pts)
    {
        score += pts;
        scorePts.text = score.ToString();
    }

    private void SetGameTimer(float elapsedSeconds) 
    {
        int min = (int) Mathf.Floor(elapsedSeconds / 60);
        int sec = (int) Mathf.Floor(elapsedSeconds % 60);
        int ms = (int) ((elapsedSeconds % 1) * 100);

        gameTimer.text = min.ToString("00") + ":" + sec.ToString("00") + ":" + ms.ToString("00");
    }

    public void UpdateScaredTimer(float scaredSeconds)
    {
        scaredTimer.text = ((int)scaredSeconds).ToString();
    }

    public void HideScaredTimer()
    {
        scaredLabel.color = new Color(255, 255, 255, 0);
        scaredTimer.color = new Color(255, 255, 255, 0);
    }

    public void ShowScaredTimer()
    {
        scaredLabel.color = new Color(255, 255, 255, 255);
        scaredTimer.color = new Color(255, 255, 255, 255);
    }

    public void ActivateTimerBlink()
    {
        isBlinking = true;
        scaredTimer.color = new Color(255, 0, 0, 255);
        StartCoroutine(Recovering());
    }

    public void StopTimerBlink()
    {
        isBlinking = false;
    }

    IEnumerator Recovering()
    {
        while (isBlinking)
        {
            scaredTimer.color = new Color(255, 255, 255, 255);
            yield return new WaitForSeconds(0.5f);
            scaredTimer.color = new Color(255, 0, 0, 255);
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }

    public void ReduceLife()
    {
        foreach (Image img in livesImg) { if (img.enabled) { img.enabled = false; break; } } // Just remove one
    }

    public void ResetLives()
    {
        foreach (Image img in livesImg) { img.enabled = true; } 
    }

}

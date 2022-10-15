using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager itself;
    public RectTransform loadingTitle;
    public RectTransform loadingCanva;
    private AudioManager audioManager;
    // private bool ongoingCoroutine;

    private void Awake()
    {
        // Make it so that we will only have one instance of UIManager (Singleton approach)
        if (!itself) { itself = this;  DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); } 
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindWithTag("Lvl1Btn").GetComponent<Button>().onClick.AddListener(LoadFirst);
        // Sets the top and bottom of the loadingTitle
        loadingTitle.offsetMin = new Vector2(loadingTitle.offsetMin.x, -loadingCanva.rect.height);
        loadingTitle.offsetMax = new Vector2(loadingTitle.offsetMax.x, -loadingCanva.rect.height);
        audioManager = gameObject.GetComponent<AudioManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        // ongoingCoroutine = false;
    }

    // Update is called once per frame
    void Update()
    {

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
        // ongoingCoroutine = true;

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
        {
            loadingTitle.anchoredPosition = Vector3.Lerp(loadingTitle.anchoredPosition, new Vector2(0.0f, -loadingCanva.rect.height - 30.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            // Added this because I don't want the music to play onSceneLoad, but when LoadingScreen is sliding off
            // since the LoadingScreen timing is hard-coded (Could probably check w/ GetCurrentScene for dynamic)
            if (played == false && GameStateManager.currentScene == GameStateManager.SceneType.Level) 
            {
                played = true; // To prevent PlayLevel from being called more than once
                audioManager.PlayLevel();
            }

            yield return null;
        }

        ResetLoadingTitle(); // Hopefully by now the Level has been loaded, hard-coded, same for loadingScreen duration anyways

        // ongoingCoroutine = false;

        yield return null;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            GameObject.FindWithTag("Exit").GetComponent<Button>().onClick.AddListener(Exit);
        }
        else if (scene.buildIndex == 1)
        {
            GameObject.FindWithTag("Lvl1Btn").GetComponent<Button>().onClick.AddListener(LoadFirst);
        }
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


}

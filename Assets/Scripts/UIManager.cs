using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public RectTransform loadingTitle;
    public RectTransform loadingCanva;
    private AudioManager audioManager;
    private bool ongoingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Sets the top and bottom of the loadingTitle
        loadingTitle.offsetMin = new Vector2(loadingTitle.offsetMin.x, -loadingCanva.rect.height);
        loadingTitle.offsetMax = new Vector2(loadingTitle.offsetMax.x, -loadingCanva.rect.height);
        audioManager = gameObject.GetComponent<AudioManager>();
        DontDestroyOnLoad(gameObject);
        ongoingCoroutine = false;
    }

    // Update is called once per frame
    void Update()
    {

        // Hard-Coded since I want the audio to play when LoadingScreen is done sliding

        float top = loadingTitle.offsetMax.y;
        float btm = loadingTitle.offsetMin.y;
        float canvaHeight = loadingCanva.rect.height;

        if (btm <= -canvaHeight + 5.0f && top <= -canvaHeight + 5.0f && GameStateManager.currentScene == GameStateManager.SceneType.Level) 
            // Get it to play 10.0f before loading reaches the end
        {
            audioManager.playLevel();
        }

        // Need to fix this so that it will only check and play this method once
        // Issue is when we pause the game, it restarts the audio cuz of the isPlayin checks. 
        // And before, it kept calling playLevel and restarting the music


    }

    public void Exit()
    {
        audioManager.playMainMenu();
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
        ongoingCoroutine = true;

        float elapsedTime = 0.0f;
        float duration = 2.0f;


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

        while (elapsedTime < duration)
        {
            loadingTitle.anchoredPosition = Vector3.Lerp(loadingTitle.anchoredPosition, new Vector2(0.0f, -loadingCanva.rect.height - 30.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        setGameState(index); // Needs to be here to ensure Main Menu Scene has been unloaded
        ResetLoadingTitle(); // Hopefully by now the Level has been loaded, hard-coded, same for loadingScreen duration anyways

        ongoingCoroutine = false;

        yield return null;
    }

    private void setGameState(int index)
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

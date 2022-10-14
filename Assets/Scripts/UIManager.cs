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
        bool played = false;

        while (elapsedTime < duration)
        {
            loadingTitle.anchoredPosition = Vector3.Lerp(loadingTitle.anchoredPosition, new Vector2(0.0f, -loadingCanva.rect.height - 30.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            if (played == false) 
            {
                audioManager.playLevel();
                played = true; // To prevent playLevel from being called more than once
            }

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

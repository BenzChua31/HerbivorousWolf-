using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{

    public RectTransform loadingT;
    private Tweener tweener; 

    // Start is called before the first frame update
    void Start()
    {
        tweener = gameObject.GetComponent<Tweener>();
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

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


        while (elapsedTime < duration)
        {
            loadingT.anchoredPosition = Vector3.Lerp(loadingT.anchoredPosition, new Vector2(0.0f, 0.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);

        yield return new WaitForSeconds(1.0f);

        elapsedTime = 0.0f;
        duration = 2.0f;

        while (elapsedTime < duration)
        {
            loadingT.anchoredPosition = Vector3.Lerp(loadingT.anchoredPosition, new Vector2(0.0f, -23.0f), elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        gameObject.GetComponentInChildren<AudioSource>().Stop();

        yield return null;
    }

}

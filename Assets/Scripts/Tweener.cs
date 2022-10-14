using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tweener itself allows us to execute frame-rate independent motion
public class Tweener : MonoBehaviour
{
    private AudioManager audioManager;
    private List<Tween> activeTweens;

    // Start is called before the first frame update
    void Start()
    {
        activeTweens = new List<Tween>();
        audioManager = GameObject.FindWithTag("Managers").GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // ToArray used to clone the array to avoid ConcurrentModification
        foreach (Tween activeTween in activeTweens.ToArray())
        {

            float timer = activeTween.TimeElapsed;
            timer += Time.deltaTime; 

            if (activeTween != null)
            {
                // Make sure the LevelScene has been loaded
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0)) { audioManager.playRustlingLeaves(); }
                Vector3 current = activeTween.Target.position;
                Vector3 start = activeTween.StartPos;
                Vector3 end = activeTween.EndPos;
                float duration = activeTween.Duration;
                float currDist = Vector3.Distance(end, current);
                float tDist = Vector3.Distance(end, start);

                if (currDist > 0.1f)
                {
                    float fraction = timer / duration; // Linear
                    activeTween.Target.position = Vector3.Lerp(start, end, fraction);
                    activeTween.TimeElapsed = timer;
                }

                if (currDist <= 0.1f)
                {
                    activeTween.Target.position = activeTween.EndPos;
                    if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0)) { audioManager.stopRustlingLeaves(); }
                    activeTweens.Remove(activeTween);
                }
            }

        }
    }

    public bool AddTween(Transform targetObject, Vector3 startPos, Vector3 endPos, float duration)
    {
        if (TweenExists(targetObject))
        {
            return false;
            // We have this if-else here to verify that a tween exists for this particular Transform component and therefore, shouldn't interfere with it
            // until it is done with its motion and removed from the list. This will prevent spam WASD for one GameObject. 
        }
        else
        {
            activeTweens.Add(new Tween(targetObject, startPos, endPos, Time.time, duration));
            return true;
        }
    }

    public bool TweenExists(Transform target)
    {
        // loops through whole list to find the matching tween if any (compare by reference)
        foreach (Tween activeTween in activeTweens)
        {
            if (activeTween.Target == target) { return true; }
        }

        return false;
    }

    public void RemoveAllTweens()
    {
        activeTweens.Clear();
    }

}

// Tested DOTween but not allowed to use it, so just going to keep a copy of my DOTween solution here

// DOTween.Init(); // Best to include even if it auto inits
// sequence = DOTween.Sequence();
//sequence.SetUpdate(UpdateType.Late, true);
//sequence.InsertCallback(0.0f, PlayMovementAudio);
// -8 X, -1 Y (BL) | 1 X, -1 Y (BR) | 1 X, 6 Y (TR) | -8 X, 6 Y (TL)
//sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 0), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DOMove(new Vector3(-11.0f, 12.0f, 0), 3.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 90), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DOMove(new Vector3(-11.0f, 16.0f, 0), 2.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DORotate(new Vector3(0, 180, 0), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DOMove(new Vector3(-16.0f, 16.0f, 0), 3.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 270), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.Append(itemTransform.DOMove(new Vector3(-16.0f, 12.0f, 0), 2.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
//sequence.SetLoops(-1, LoopType.Restart);

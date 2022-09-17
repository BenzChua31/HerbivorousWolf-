using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tweener : MonoBehaviour
{

    [SerializeField] private Transform itemTransform;
    [SerializeField] private AudioSource rustlingLeaves;
    private Sequence sequence;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init(); // Best to include even if it auto inits
        sequence = DOTween.Sequence();
        sequence.SetUpdate(UpdateType.Late, true);
        sequence.InsertCallback(0.0f, PlayMovementAudio);
        // -8 X, -1 Y (BL) | 1 X, -1 Y (BR) | 1 X, 6 Y (TR) | -8 X, 6 Y (TL)
        sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 0), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DOMove(new Vector3(-11.0f, 12.0f, 0), 3.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 90), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DOMove(new Vector3(-11.0f, 16.0f, 0), 2.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DORotate(new Vector3(0, 180, 0), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DOMove(new Vector3(-16.0f, 16.0f, 0), 3.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DORotate(new Vector3(0, 0, 270), 0.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.Append(itemTransform.DOMove(new Vector3(-16.0f, 12.0f, 0), 2.0f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late, true));
        sequence.SetLoops(-1, LoopType.Restart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayMovementAudio()
    {
        rustlingLeaves.Stop();
        rustlingLeaves.Play();
        rustlingLeaves.loop = true;
    }

}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingHandler : MonoBehaviour
{
    #region
    public RectTransform targetPosition; // Assign this in the Inspector
    public float moveDuration = 1f; // Duration for the movement
    public float waitDuration = 1f; // Duration to wait at each position
    public Vector3 myVector = new Vector3(0f, 0f, 0f);

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StartMovement();
    }

    void StartMovement()
    {
        Sequence mySequence = DOTween.Sequence();

        // Move to target position
        mySequence.Append(rectTransform.DOLocalMove(targetPosition.localPosition, moveDuration))
                  .AppendInterval(waitDuration)
                  .Append(rectTransform.DOLocalMove(myVector, moveDuration))
                  .AppendInterval(waitDuration)
                  .SetLoops(-1, LoopType.Restart); // Loop indefinitely
    }
    #endregion

}
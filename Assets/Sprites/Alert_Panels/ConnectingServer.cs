using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ConnectingServer : MonoBehaviour
{
    private void OnEnable()
    {
        MoveBackAndForth();
    }

    private void MoveBackAndForth()
    {
        // Move to 100
        transform.DOLocalMoveX(100, 0.1f)
            .SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                // Wait for 1 second
                DOVirtual.DelayedCall(1f, () =>
                {
                    // Move to -100
                    transform.DOLocalMoveX(-100, 0.1f)
                        .SetEase(Ease.InOutBack)
                        .OnComplete(() =>
                        {
                            // Wait for 1 second before looping again
                            DOVirtual.DelayedCall(1f, MoveBackAndForth);
                        });
                });
            });
    }

    private void OnDisable()
    {
        // Kill all tweens on this transform
        DOTween.Kill(transform);
    }
}

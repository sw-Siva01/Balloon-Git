using UnityEngine;
using DG.Tweening;

public class Tweening : MonoBehaviour
{
    public RectTransform fire;
    [SerializeField] private float endValue;
    [SerializeField] private float speed;

    private void Start()
    {
        FireEfx();
    }
    private void OnDisable()
    {
        StopEfx();
    }
    public void FireEfx()
    {
        fire.transform.DOScaleY(endValue, speed)
            .SetLoops(-1, LoopType.Yoyo);
    }
    public void StopEfx()
    {
        fire.transform.DOKill();
    }
}

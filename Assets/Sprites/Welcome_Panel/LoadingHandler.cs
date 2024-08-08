using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingHandler : MonoBehaviour
{
    [SerializeField] private GameObject LoadingImage;
    public float Delay;

    private void OnEnable()
    {
        LoadingImage.transform?.DOKill();
        LoadingImage.transform.DOLocalRotate(new Vector3(0,0,-360) , Delay , RotateMode.FastBeyond360).SetLoops(-1 , LoopType.Incremental).SetEase(Ease.Linear);
    }

    private void OnDisable()
    {
        LoadingImage.transform?.DOKill();
    }
}

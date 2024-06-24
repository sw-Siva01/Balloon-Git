using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BonusBallon : MonoBehaviour
{
    public RectTransform bonusObj;
    private void OnEnable()
    {
        bonusObj.DOMove(new Vector2(0f, -0.23f), 1f).SetEase(Ease.InSine);
    }
}

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
        Invoke("TimeDelay", 3f);
    }
    void TimeDelay()
    {
        bonusObj.DOMove(new Vector2(0f, 8f), 0.8f).SetEase(Ease.InOutBack);
        Invoke("SetOFF", 1f);
    }
    void SetOFF()
    {
        bonusObj.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        bonusObj.DOMove(new Vector2(0f, -8f), 0.1f).SetEase(Ease.InSine);
    }
}

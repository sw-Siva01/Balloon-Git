using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BonusBallon : MonoBehaviour
{
    public RectTransform bonusObj;
    public GameObject Fill_Img;
    private void OnEnable()
    {
        //bonusObj.DOMove(new Vector2(0f, -0.23f), 1f).SetEase(Ease.InSine);
        bonusObj.DOMove(new Vector2(0f, -0.6f), 1f).SetEase(Ease.InSine); // new balloon position
        Invoke("SetON", 2f);
        Invoke("TimeDelay", 5f);
    }
    void SetON()
    {
        Fill_Img.SetActive(true);
    }
    void TimeDelay()
    {
        bonusObj.DOMove(new Vector2(0f, 8f), 0.8f).SetEase(Ease.InOutBack);
        Invoke("SetOFF", 1f);
    }

    void SetOFF()
    {
        bonusObj.gameObject.SetActive(false);
        Fill_Img.SetActive(false);
    }
    private void OnDisable()
    {
        bonusObj.DOMove(new Vector2(0f, -8f), 0.1f).SetEase(Ease.InSine);
    }
}

using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BonusBallon : MonoBehaviour
{
    public RectTransform bonusObj;
    public GameObject Fill_Img;
    private void OnEnable()
    {
        bonusObj.DOMove(new Vector2(0f, -0.6f), 1f).SetEase(Ease.InSine); // new balloon position

        SetON();
        TimeDelay();
    }
    async void SetON()
    {
        await UniTask.Delay(2000);
        Fill_Img.SetActive(true);
    }
    async void TimeDelay()
    {
        await UniTask.Delay(4500);
        bonusObj.DOMove(new Vector2(0f, 8f), 0.8f).SetEase(Ease.InOutBack);

        await UniTask.Delay(5500);
        SetOFF();
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

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomSettings : UIHandler
{
    [Header("UI_Reference")]
    [SerializeField] private RectTransform bgTrans;
    [SerializeField] private Button popupCloseBtn, continueBtn, closeBtn;

    public static WelcomSettings instance;
    private void Awake()
    {
        AudioListener.volume = 0;
        instance = this;
        continueBtn.onClick.AddListener(() => { HideMe(); GameController.instance.Welcom_Button(); });
        popupCloseBtn.onClick.AddListener(() => { ExitGame(); });
        closeBtn.onClick.AddListener(() => { ExitGame(); });
    }
    private void ExitGame()
    {
        Invoke(nameof(SetDelay), 0.5f);
    }
    private void SetDelay()
    {
        APIController.CloseWindow();
    }
    public void Show()
    {
        Invoke(nameof(ShowMe), 0.1f);
    }
    #region UI_Handler
    public override void ShowMe()
    {
        base.ShowMe();
    }
    public override void HideMe()
    {
        GameController.instance.CanPlayAudio = true;
        AudioListener.volume = 1;
        Debug.Log(" ^^^^^^^^^ 3 : " + AudioListener.volume);
        base.HideMe();
    }
    public override void OnBack()
    {
        base.OnBack();
    }
    #endregion
}

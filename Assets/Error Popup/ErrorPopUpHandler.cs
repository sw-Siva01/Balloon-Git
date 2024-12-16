using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopUpHandler : MonoBehaviour
{
    public static ErrorPopUpHandler instance;
    public GameObject ErrorPanel;
    
    public TMP_Text errorText;
    CanvasGroup canvasGroup;
    int _code;

    void Awake()
    {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowError(int code, string error)
    {
        ErrorPanel.gameObject.SetActive(true);
        canvasGroup.DOFade(1, 0.2f).From(0);
        CancelInvoke(nameof(HideError));
        Invoke(nameof(HideError), 1.2f);
        errorText.text = "<sprite=0>" + error;
        _code = code;
    }

    void HideError()
    {
        canvasGroup.DOFade(0, 0.2f).OnComplete(() =>
        {
            ErrorPanel.gameObject.SetActive(false);

            if (_code == 401)
            {
                /*MiniRouletteUIController.instance.SettingsPanel.redirectingToMainMenu.SetActive(true);*/
                SettingsPanelHandler.instance.RedirectingPanel.SetActive(true);
                //GameController.Instance.ExitWebGL();
                SettingsPanelHandler.instance.OnExitBtnClick();
            }
            else
            {

            }
        });
    }

}

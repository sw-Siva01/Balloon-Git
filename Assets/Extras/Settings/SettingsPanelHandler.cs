using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanelHandler : UIHandler
{
    #region { ::::::::::::::::::::::::: Headers ::::::::::::::::::::::::: }
    public RectTransform PanelTransform;
    public float XOffPos = 700;
    public Button ExitBtn;
    public Toggle SoundToggle;
    public UnityAction SwapSpriteSequence;
    public Button emptySpaceBtn;
    public GameObject emptySpaceObj;
    public string playerName;
    public Button _fullScreen;
    public TMP_Text playerNameTxt;
    public Button howtoPlayBtn;
    public Toggle MusicToggle;
    public Toggle soundTogWelcome, musicTogWelcome;
    #endregion  ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    private void Awake()
    {
        _fullScreen.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); FullScreenFunc(); });

        howtoPlayBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); HowToPlay(); });

        ExitBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); OnExitBtnClick(); });
        emptySpaceBtn.onClick.AddListener(() =>
        {
            UI_Controller.instance.settingsHandler.HideMe();
        });
        //HideMe();
        //ToggleSound(true);
        //SetMusicVolume(true);
        SoundToggle.onValueChanged.AddListener((state) => { ToggleSound(state); });
        MusicToggle.onValueChanged.AddListener((state) => { SetMusicVolume(state); });
        soundTogWelcome.onValueChanged.AddListener((state) => { ToggleSound(state); });
        musicTogWelcome.onValueChanged.AddListener((state) => { SetMusicVolume(state); });
    }
    public void FullScreenFunc()
    {
        Debug.Log("FullScreenFunc");
        APIController.FullScreen();
        UI_Controller.instance.settingsHandler.HideMe();
    }
    public void HowToPlay()
    {
        /*UI_Controller.instance.howToPlay_Panel.ShowMe();*/
    }
    public void ToggleSound(bool value)
    {
        SoundToggle.isOn = value;

        soundTogWelcome.isOn = value;
        Debug.Log("Sound volume --> " + value);

        if (SoundToggle.isOn)
        {
           
            MasterAudioController.instance.SetVolumeUnMute();
           
        }
        else
        {
            MasterAudioController.instance.SetVolumeMute();
        }
        if (SoundToggle.isOn)
            MasterAudioController.instance.PlayAudio(AudioEnum.toggle);
    }
    public void SetMusicVolume(bool _state)
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.toggle);

        Debug.Log("Music volume --> " + _state);
        MusicToggle.isOn = _state;
        musicTogWelcome.isOn = _state;

        MasterAudioController.instance.BackgroundAudio.SetBgmSoundStatus(MusicToggle.isOn);
    }
    public override void ShowMe()
    {
        base.ShowMe();
        PanelTransform.DOKill();
        PanelTransform.gameObject.SetActive(true);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        PanelTransform.DOAnchorPosX(19f, 0.1f);
        emptySpaceObj.SetActive(true);
    }
    public override void HideMe()
    {
        Debug.Log("Settings Panel Open ");
        UI_Controller.instance.PlayButtonSound();
        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-1000f, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { gameObject.SetActive(false); PanelTransform.gameObject.SetActive(false); });
        SwapSpriteSequence?.Invoke();
        emptySpaceObj.SetActive(false);
    }
    public void Welcomebtn_OFF()
    {
        Debug.Log("Welcome button OFF ");
        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-1000f, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { gameObject.SetActive(false); PanelTransform.gameObject.SetActive(false); });
        SwapSpriteSequence?.Invoke();
        emptySpaceObj.SetActive(false);
    }

    public void OnExitBtnClick()
    {
        UI_Controller.instance.ExitWebGL();
        UI_Controller.instance.settingsHandler.HideMe();
    }
    public void CallingSettingPanel()
    {
        if (!UI_Controller.instance.settingsHandler.gameObject.activeSelf)
        {
            UI_Controller.instance.settingsHandler.ShowMe();
            UI_Controller.instance.PlayButtonSound();
        }
        else
        {

            UI_Controller.instance.settingsHandler.HideMe();
        }
    }
    public void ShowDeposit()
    {
#if UNITY_WEBGL
        if (!APIController.instance.userDetails.isBlockApiConnection)
            APIController.instance.OnClickDepositBtn();
#endif

    }
}

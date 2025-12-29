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
    public static SettingsPanelHandler instance;
    public RectTransform PanelTransform;
    public float XOffPos = 700;
    public Button ExitBtn;
    public Toggle SoundToggle;
    public UnityAction SwapSpriteSequence;
    public Button emptySpaceBtn;
    public GameObject emptySpaceObj;
    public GameObject HTP;
    [SerializeField] private Button betHistory_Btn;
    [SerializeField] private GameObject betHistory;
    public string playerName;
    /*public Button _fullScreen;*/
    public TMP_Text playerNameTxt;
    public Button howtoPlayBtn;
    public Toggle MusicToggle;
    public Toggle soundTogWelcome, musicTogWelcome;
    public GameObject RedirectingPanel;

    public Button settingsBtn;
    public Button settingsCloseBtn;
    [SerializeField] private Button gameLimits_Btn;
    public GameObject GameLimits;
    public Button addCashBtn;
    #endregion  ::::::::::::::::::::::::: END :::::::::::::::::::::::::
    private void Awake()
    {
        instance = this;
        /* _fullScreen.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); FullScreenFunc(); });*/
        howtoPlayBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); HowToPlay(); });
        ExitBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); OnExitBtnClick(); });

        emptySpaceBtn.onClick.AddListener(() =>
        {
            HideMe();
        });
        //HideMe();
        //ToggleSound(true);
        //SetMusicVolume(true);
        SoundToggle.onValueChanged.AddListener((state) => { ToggleSound(state); });
        MusicToggle.onValueChanged.AddListener((state) => { SetMusicVolume(state); });
        soundTogWelcome.onValueChanged.AddListener((state) => { ToggleSound(state); });
        musicTogWelcome.onValueChanged.AddListener((state) => { SetMusicVolume(state); });

        gameLimits_Btn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowGameLimitScreen(); });
        betHistory_Btn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowBetHistoryScreen(); });

        addCashBtn.onClick.AddListener(() => { UI_Controller.instance.PlayButtonSound(); ShowDeposit(); });
    }
    /*public void FullScreenFunc()
    {
        DebugHelper.Log("FullScreenFunc");
        APIController.FullScreen();
        HideMe();
    }*/
    public void ShowBetHistoryScreen()
    {
        betHistory.SetActive(true);
        HideMe();
        settingsBtn.gameObject.SetActive(true);
        settingsCloseBtn.gameObject.SetActive(false);
    }
    public void HowToPlay()
    {
        /*UI_Controller.instance.howToPlay_Panel.ShowMe();*/
    }
    public void ToggleSound(bool value)
    {
        SoundToggle.isOn = value;

        soundTogWelcome.isOn = value;
        DebugHelper.Log("Sound volume --> " + value);

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

        if (!Updatetoggle) return;

        CancelInvoke(nameof(UpdateMusic));
        Invoke(nameof(UpdateMusic), 0.5f);
    }
    public void SetMusicVolume(bool _state)
    {
        MasterAudioController.instance.PlayAudio(AudioEnum.toggle);

        DebugHelper.Log("Music volume --> " + _state);
        MusicToggle.isOn = _state;
        musicTogWelcome.isOn = _state;

        MasterAudioController.instance.BackgroundAudio.SetBgmSoundStatus(MusicToggle.isOn);

        if (!Updatetoggle) return;

        CancelInvoke(nameof(UpdateMusic));
        Invoke(nameof(UpdateMusic), 0.5f);
    }
    public override void ShowMe()
    {
        base.ShowMe();
        PanelTransform.DOKill();
        PanelTransform.gameObject.SetActive(true);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        PanelTransform.DOAnchorPosX(25f, 0.1f);
        emptySpaceObj.SetActive(true);
    }
    public override void HideMe()
    {
        DebugHelper.Log("Settings Panel Open ");
        UI_Controller.instance.PlayButtonSound();
        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-1000f, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { gameObject.SetActive(false); PanelTransform.gameObject.SetActive(false); });
        SwapSpriteSequence?.Invoke();
        emptySpaceObj.SetActive(false);
        DebugHelper.Log("SettingPanel__Close :");
        settingsCloseBtn.gameObject.SetActive(false);
        settingsBtn.gameObject.SetActive(true);
    }
    public void Welcomebtn_OFF()
    {
        DebugHelper.Log("Welcome button OFF ");
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
            ShowMe();
            UI_Controller.instance.PlayButtonSound();
            DebugHelper.Log("SettingPanel__Open :");
            settingsBtn.gameObject.SetActive(false);
            settingsCloseBtn.gameObject.SetActive(true);
        }
        else
        {
            HideMe();
        }
    }
    public void ShowDeposit()
    {
#if UNITY_WEBGL
        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (!success)
            {
                return;
            }
            else
            {
                if (!GameController.instance.demo)
                {
                    APIController.instance.OnClickDepositBtn();
                }
            }
        });
#endif
    }
    private void UpdateMusic()
    {
        APIController.instance.authentication.sound = SoundToggle.isOn;
        APIController.instance.authentication.music = MusicToggle.isOn;

        LocalStorage.Save("Lootrix_sound", APIController.instance.authentication.sound ? "true" : "false");
        LocalStorage.Save("Lootrix_music", APIController.instance.authentication.music ? "true" : "false");

        APIController.instance.CheckInternetandProcess(async (success) =>
        {
            if (success)
            {
                APIController.instance.UpdateAudioSettings();
            }
            else
            {
                Invoke(nameof(UpdateMusic), 0.5f);
            }
        });
    }

    bool Updatetoggle = false;
    public void SetToggleValueFromAPI(bool sound, bool music)
    {
        SoundToggle.isOn = sound;
        MusicToggle.isOn = music;
        Updatetoggle = true;
    }

    public void ShowGameLimitScreen()
    {
        GameLimits.SetActive(true);
        HideMe();
        settingsBtn.gameObject.SetActive(true);
        settingsCloseBtn.gameObject.SetActive(false);
    }
}

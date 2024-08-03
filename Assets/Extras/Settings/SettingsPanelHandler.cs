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
    /*public RectTransform PanelTransform;
    public float XOffPos = 700;
    public Button ExitBtn;
    public Button FullScreen;

    public Toggle SoundToggle;
    public Toggle MusicToggle;

    private bool SoundActive;
    public UnityAction SwapSpriteSequence;
    public Button settCoverImg;
    public TMP_Text playerName;
    public MasterAudioController audioController;
    public static SettingsPanelHandler Instance;
    public AudioSource BackgroundAudio;
    private void Awake()
    {
        Instance = this;
        // CheckPlayerprefs();
        SoundToggle.onValueChanged.RemoveAllListeners();
        SoundToggle.onValueChanged.AddListener((state) => { ToggleSound(state); });
        MusicToggle.onValueChanged.AddListener((state) => { ToggleMusic(state); });
        ExitBtn.onClick.RemoveAllListeners();
        ExitBtn.onClick.AddListener(OnExitBtnClick);
        FullScreen.onClick.AddListener(ShowFullScreen);
        settCoverImg.onClick.AddListener(HideSettings);
    }
    private void Start()
    {
        //   ToggleSound(true);
        HideSettings();
    }
    public void ToggleMusic(bool value)
    {
        if (value == true)
        {
            if (SoundToggle.isOn)
            {
                //   AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);
                audioController.PlayAudio(AudioEnum.toggle);
            }
            // AudioController.Instance.BGM.Play();
            BackgroundAudio.Play();

        }
        else
        {
            if (!SoundToggle.isOn)
            {
                // AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);
                audioController.PlayAudio(AudioEnum.toggle);
            }
            //    AudioController.Instance.BGM.Stop();
            BackgroundAudio.Stop();
        }

    }
    public void ToggleSound(bool value)
    {
        SoundActive = value;
        SoundToggle.isOn = value;
        if (value == false)
        {
            //for (int i = 0; i < AudioController.Instance.AllUiSounds.Length; i++)
            //{
            //    AudioController.Instance.AllUiSounds[i].mute = true;
            audioController.SetVolumeMute();
            //}
        }
        else
        {
            //for (int i = 0; i < AudioController.Instance.AllUiSounds.Length; i++)
            //{
            //    AudioController.Instance.AllUiSounds[i].mute = false;
            audioController.SetVolumeUnMute();
            //}
        }
        audioController.PlayAudio(AudioEnum.toggle);
        //   AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);
        //  AudioController.Instance.AudioPlay(true, AudioController.Instance.BGM);
    }



    //public void ShowSettings()
    //{
    //    PanelTransform.transform.gameObject.SetActive(true);

    //    PanelTransform.DOKill();
    //    PanelTransform.transform.localScale = Vector3.zero;
    //    //PanelTransform.DOKill();
    //    //PanelTransform.DOAnchorPosX(0, 0.25f).From(new Vector2(XOffPos, PanelTransform.anchoredPosition.y));
    //    PanelTransform.transform.DOScale(1, 0.1f).OnComplete(() => { settCoverImg.gameObject.SetActive(true); });


    //}

    //public void HideSettings()
    //{

    //    PanelTransform.DOKill();

    //    //PanelTransform.DOKill();
    //    //PanelTransform.DOAnchorPosX(XOffPos, 0.25f).OnComplete(() => gameObject.SetActive(false));
    //    PanelTransform.transform.DOScale(0, 0.1f).OnComplete(()=> { PanelTransform.transform.gameObject.SetActive(false); settCoverImg.gameObject.SetActive(false); });
    //    //SwapSpriteSequence?.Invoke();
    //}
    public void ShowSettings()
    {
        //settCoverImg.gameObject.SetActive(true);
        // PlayerName.text = GameController.Instance.PlayerName;
        //  AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);
        audioController.PlayAudio(AudioEnum.UiButtonClick);
        gameObject.SetActive(true);
        PanelTransform?.DOKill();
        PanelTransform.gameObject.SetActive(true);
        PanelTransform.DOAnchorPosX(40, 0.1f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
    }
    int tempcountHidesetting = 0;
    public void HideSettings()
    {
        if(tempcountHidesetting > 0)
        {
            //  AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);
            audioController.PlayAudio(AudioEnum.UiButtonClick);
        }

        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-640, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { PanelTransform.gameObject.SetActive(false); gameObject.SetActive(false); });
        tempcountHidesetting++;
    }
    public void OnExitBtnClick()
    {
        //    AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);
        audioController.PlayAudio(AudioEnum.UiButtonClick);
        Debug.Log("Exit Entered");

        APIController.CloseWindow();
        Debug.Log("Exit Called");
        HideSettings();
    }
    private void ShowFullScreen()
    {
        // AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);
        audioController.PlayAudio(AudioEnum.UiButtonClick);
        APIController.FullScreen();
        HideSettings();
    }*/

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
}

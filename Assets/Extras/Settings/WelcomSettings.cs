using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomSettings : UIHandler
{
    #region
    /*public Toggle SoundToggle;
    public Toggle MusicToggle;

    private bool SoundActive;
    private void Awake()
    {
        SoundToggle.onValueChanged.RemoveAllListeners();
        SoundToggle.onValueChanged.AddListener((state) => { ToggleSound(state); });
        MusicToggle.onValueChanged.AddListener((state) => { ToggleMusic(state); });
       
    }
    public void ToggleMusic(bool value)
    {

        if (value == true)
        {
            if (SoundToggle.isOn)
            {
                //   AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);

            }
            // AudioController.Instance.BGM.Play();

            
        }
        else
        {
            if (SoundToggle.isOn)
            {
                // AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);

            }
            //    AudioController.Instance.BGM.Stop();
            SettingsPanelHandler.Instance.MusicToggle.isOn = value;
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

            //}
            SettingsPanelHandler.Instance.SoundToggle.isOn = value;
        }
        else
        {
            //for (int i = 0; i < AudioController.Instance.AllUiSounds.Length; i++)
            //{
            //    AudioController.Instance.AllUiSounds[i].mute = false;

            //}
        }
        //   AudioController.Instance.AudioPlay(true, AudioController.Instance.ToggleSFX);
        //  AudioController.Instance.AudioPlay(true, AudioController.Instance.BGM);
    }*/
    #endregion
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
        base.HideMe();
    }

    public override void OnBack()
    {
        base.OnBack();
    }
    #endregion
}

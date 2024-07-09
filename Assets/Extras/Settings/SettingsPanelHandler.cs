using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPanelHandler : MonoBehaviour
{
    public RectTransform PanelTransform;
    public float XOffPos = 700;
    public Button ExitBtn;
    public Button FullScreen;

    public Toggle SoundToggle;
    public Toggle MusicToggle;

    private bool SoundActive;
    public UnityAction SwapSpriteSequence;
    public Button settCoverImg;
    public TMP_Text playerName;
    public static SettingsPanelHandler Instance;
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

        }

        PanelTransform?.DOKill();
        PanelTransform.DOAnchorPosX(-640, 0.3f);
        PanelTransform.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => { PanelTransform.gameObject.SetActive(false); gameObject.SetActive(false); });
        tempcountHidesetting++;
    }
    public void OnExitBtnClick()
    {
    //    AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);

        Debug.Log("Exit Entered");

        APIController.CloseWindow();
        Debug.Log("Exit Called");
        HideSettings();
    }
    private void ShowFullScreen()
    {
       // AudioController.Instance.AudioPlay(true, AudioController.Instance.UIbtnsSFX);

        APIController.FullScreen();
        HideSettings();
    }
}

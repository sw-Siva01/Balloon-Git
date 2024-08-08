using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Controller : MonoBehaviour
{
    //public UIHandler howToPlay_Panel;
    public SettingsPanelHandler settingsHandler;
    /*public InsufficientPanelHandler insufficientPanelHandler;*/
    public List<UIHandler> openedPages;
    //public GameObject LoadingPanel;
    public static UI_Controller instance;
    public GameObject Welcomepop;

    private void Awake()
    {
        instance = this;
        //Welcomepop.SetActive(true);
        //LoadingPanel.SetActive(true);
    }

    #region Adding & Removing CurrentPages
    public void AddToOpenPages(UIHandler handler)
    {
        if (openedPages.Contains(handler))
        {
            openedPages.Remove(handler);
        }
        openedPages.Add(handler);
    }

    public void RemoveFromOpenPages(UIHandler handler)
    {
        if (openedPages.Contains(handler))
            openedPages.Remove(handler);

    }
  
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape) && openedPages.Count > 0 && openedPages.Count != 0)
        //    openedPages[openedPages.Count - 1].OnBack();
    }
    #endregion

    #region EXIT GAME
    public void ExitWebGL()
    {
        APIController.CloseWindow();
    }
    #endregion

    #region Sound Properties
    public bool IsSoundOn()
    {
        return settingsHandler.SoundToggle.isOn;
    }
    public void PlayButtonSound()
    {
        MasterAudioController.instance.StopAudio(AudioEnum.UiButtonClick);
        MasterAudioController.instance.PlayAudio(AudioEnum.UiButtonClick);
    }

    #endregion
}

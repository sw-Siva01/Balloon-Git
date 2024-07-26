using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomSettings : MonoBehaviour
{
    public Toggle SoundToggle;
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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InternetChecking : MonoBehaviour
{
    public GameObject connectionPanel;
    public static InternetChecking instance;

    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
    }

    #region Network Status

    /// <summary>
    /// Checks the network status and performs actions based on the status.
    /// </summary>
    /// <param name="data">String representing the network status ("true" or "false").</param>

    public void GetNetworkStatus(string data)
    {
        connectionPanel.SetActive(data.ToLower() == "false");
        if (!APIController.instance.isInFocus)
        {
            AudioListener.volume = 0;
        }
        else if (connectionPanel.activeSelf)
        {
            if (UI_Controller.instance.settingsHandler.SoundToggle.isOn)
            {
                AudioListener.volume = 0;
            }
        }
        else
        {
            if (UI_Controller.instance.settingsHandler.SoundToggle.isOn)
            {
                if (GameController.instance.CanPlayAudio)
                    AudioListener.volume = 1;

                Debug.Log(" ^^^^^^^^^ 1 : " + AudioListener.volume);
            }
        }
    }


    #endregion
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class InternetChecking : MonoBehaviour
{
    public static InternetChecking instance;

    public GameObject connectionPanel;


    private void Start()
    {
#if !UNITY_EDITOR
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
#endif
    }

    #region Network Status

    /// <summary>
    /// Checks the network status and performs actions based on the status.
    /// </summary>
    /// <param name="data">String representing the network status ("true" or "false").</param>
    public void GetNetworkStatus(string data)
    {

        Debug.Log("Internet Status Check ********** " + data);



        //connectionPanel.SetActive(data.ToLower() == "true" ? false : true);
        connectionPanel.SetActive(data.ToLower() == "false");

        if (!connectionPanel.activeSelf)
        {
            Debug.Log($"Connection Panel check network --->   {data} connection panel is == {connectionPanel.activeSelf} ");
            if (!APIController.instance.userDetails.isBlockApiConnection)
            {
                APIController.GetUpdatedBalance();
                Debug.Log($"Updated Blaance ---> 1 ");

            }
            else
            {
                GameController.instance.InitAmountDetails();
            }
        }

        if (connectionPanel.activeSelf)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }
        if (GameController.instance.IsInTab)
        {
            if (AudioListener.volume == 0)
            {

            }
            if (GameController.instance.CanPlayAudio)
                AudioListener.volume = 1;
        }
    }

    #endregion
}


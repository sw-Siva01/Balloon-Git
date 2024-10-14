using System;
using UnityEngine;

public class InternetChecking : MonoBehaviour
{
    public GameObject ConnectionPanel;
    DateTime lastupdate = new DateTime();
    public GameObject ServerMaintancePopup;
    public static InternetChecking instance;

    private void Start()
    {
        instance = this;
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
        lastupdate = DateTime.Now;
    }

    public void GetNetworkStatus(NetworkStatus data)
    {
        Debug.Log($"NetworkStatus ==> {data.ToString()}");
        if (data != NetworkStatus.Active && (DateTime.Now > lastupdate.AddSeconds(4)))
        {
            if (data == NetworkStatus.NetworkIssue)
            {
                ConnectionPanel.SetActive(true);
            }
            else if (!ConnectionPanel.activeSelf)
            {
               // ServerMaintancePopup.SetActive(true);
            }
        }
        else
        {
            if (ConnectionPanel.activeSelf || ServerMaintancePopup.activeSelf)
            {
                lastupdate = DateTime.Now;
                //ServerMaintancePopup.SetActive(false);
                ConnectionPanel.SetActive(false);
            }
        }

        AudioListener.volume = (GameController.instance.CanPlayAudio && data == NetworkStatus.Active && !ConnectionPanel.gameObject.activeSelf && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;
    }
}

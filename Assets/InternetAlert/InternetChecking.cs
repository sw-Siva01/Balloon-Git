using UnityEngine;

public class InternetChecking : MonoBehaviour
{
    public GameObject ConnectionPanel;
    public GameObject ServerMaintancePopup;
    public static InternetChecking instance;

    private void Start()
    {
        instance = this;
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
    }

    public void GetNetworkStatus(NetworkStatus data)
    {
        Debug.Log($"NetworkStatus ==> {data.ToString()}");
        if (data != NetworkStatus.Active)
        {
            if (data == NetworkStatus.NetworkIssue)
            {
                ConnectionPanel.SetActive(true);
            }
            else
            {
                ServerMaintancePopup.SetActive(true);
            }
        }
        else
        {
            ConnectionPanel.SetActive(false);
        }

        AudioListener.volume = (GameController.instance.CanPlayAudio && data == NetworkStatus.Active && !ConnectionPanel.gameObject.activeSelf && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;
    }
}

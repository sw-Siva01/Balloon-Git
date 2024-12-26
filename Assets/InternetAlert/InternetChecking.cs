
using UnityEngine;

public class InternetChecking : MonoBehaviour
{
    public static InternetChecking instance;
    public GameObject InternetDisconnectedPopup;
    public GameObject ServerMaintenancePopup;

    private void Start()
    {
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
    }
    private void Awake()
    {
        instance = this;
    }
    public void GetNetworkStatus(NetworkStatus data)
    {
        if (data != NetworkStatus.Active)
        {
            if (data == NetworkStatus.NetworkIssue)
            {
                InternetDisconnectedPopup.SetActive(true);
                Debug.Log($"Internet Status ===> {data.ToString()}....Enabling Internet Popup");
            }
            else if (data == NetworkStatus.ServerIssue)
            {
                ServerMaintenancePopup.SetActive(true);
                Debug.Log($"Internet Status ===> {data.ToString()}....Enabling Server Maintenance Popup");
            }
        }
        else
        {
            Debug.Log($"NetworkStatus ==> {data.ToString()}");
            if (InternetDisconnectedPopup.activeSelf || ServerMaintenancePopup.activeSelf)
            {
                InternetDisconnectedPopup.SetActive(false);
                ServerMaintenancePopup.SetActive(false);
            }
        }
        AudioListener.volume = (data == NetworkStatus.Active && !InternetDisconnectedPopup.gameObject.activeSelf && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;
    }
}

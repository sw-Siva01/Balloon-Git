
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    public GameObject ConnectionPanel;
    public GameObject ServerPopPanel;
    public static NetworkHandler instance;

    public GameObject waitingForResponse;

    private void Start()
    {
        instance = this;
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
    }

    public void GetNetworkStatus(NetworkStatus data)
    {
        DebugHelper.Log($"NetworkStatus ==> {data.ToString()}");
        if (data != NetworkStatus.Active)
        {
            if (data == NetworkStatus.NetworkIssue)
            {
                ConnectionPanel.SetActive(true);
                waitingForResponse.gameObject.SetActive(false);
            }
            else if (!ConnectionPanel.activeSelf && data == NetworkStatus.ServerIssue)
            {
                CancelInvoke(nameof(CheckToEnable));
                Invoke(nameof(CheckToEnable), 3);
                waitingForResponse.gameObject.SetActive(false);
            }
            else if (data == NetworkStatus.WaitingforResponse)
            {
                if (!waitingForResponse.activeSelf)
                    ShowWaitingForResponse();
            }
        }
        else
        {
           HideWaitingForResponse();
            DebugHelper.Log($"NetworkStatus ==> {data.ToString()}");
            if (ConnectionPanel.activeSelf || ServerPopPanel.activeSelf)
            {
                ConnectionPanel.SetActive(false);
                ServerPopPanel.SetActive(false);
            }
        }
        DebugHelper.Log("AudioController.Instance.IsActive");
        DebugHelper.Log("APIController.instance.isOnline");
        DebugHelper.Log("APIController.instance.isInFocus");
        DebugHelper.Log("!ConnectionPanel.gameObject.activeSelf");
        DebugHelper.Log("data == NetworkStatus.Active");

      
        AudioListener.volume = (data == NetworkStatus.Active && !ConnectionPanel.gameObject.activeSelf && APIController.instance.isOnline && APIController.instance.isInFocus) ? 1 : 0;

    }
    private void CheckToEnable()
    {
        if (!ConnectionPanel.activeSelf)
        {
            ServerPopPanel.SetActive(true);
        }
    }

    public void ShowWaitingForResponse()
    {
        waitingForResponse.gameObject.SetActive(true);
        ConnectionPanel.SetActive(false);
    }
    public void HideWaitingForResponse()
    {
        waitingForResponse.gameObject.SetActive(false);
        ConnectionPanel.SetActive(false);
    }
}


using System;
using System.Collections;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    public GameObject ConnectionPanel;
    public GameObject ServerPopPanel;
    public static NetworkHandler instance;

    public GameObject waitingForResponse;
    public GameObject SessionPopup;

    private DateTime _LastActiveTime;
    [SerializeField] private double _SessionDelay;

    private void Start()
    {
        instance = this;
        APIController.instance.OnInternetStatusChange += GetNetworkStatus;
        SetDelay(60);
        StartIdleSession();
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Mouse0) && !GameController.instance.startGame) || (SettingsPanelHandler.instance.HTP.gameObject.activeSelf && Input.mouseScrollDelta.y != 0))
        {
            StartIdleSession();
        }
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
            CancelInvoke(nameof(CheckToEnable));
            ConnectionPanel.SetActive(false);
            ServerPopPanel.SetActive(false);
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

    private IEnumerator ValidateIdle()
    {
        while ((DateTime.Now - _LastActiveTime).TotalSeconds <= _SessionDelay)
        {
            yield return new WaitForSeconds(1f);
            DebugHelper.Log($"ValidateIdle : {DateTime.Now}");
        }
        if (!GameController.instance.startGame && !GameController.instance.autoPlayPanel.activeSelf)
            SessionPopup.SetActive(true);
        SetDelay();
    }

    public void StartIdleSession(bool _check = true)
    {
        if (SessionPopup.activeSelf) { return; }

        StopCoroutine(nameof(ValidateIdle));
        if (_check)
        {
            _LastActiveTime = DateTime.Now;
            DebugHelper.Log($"Active Time : {_LastActiveTime}, {_SessionDelay}");
            StartCoroutine(nameof(ValidateIdle));
        }
    }

    public DateTime GetLastActiveTime()
    {
        return _LastActiveTime;
    }

    public void SetDelay(double delay = 60)
    {
        _SessionDelay = delay;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aws_Gateway;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Colyseus_SocketController : MonoBehaviour
{

    private NetworkClient _networkClient;

    public string url = "ws://localhost:2567"; // Set your Colyseus server address
    //public string gameName = "your_game_name"; // Set your game name

    public bool UseLocalUrl;

    public List<WSS_Event> wss_Events = new List<WSS_Event>();

    Dictionary<string, bool> ActiveTasks = new Dictionary<string, bool>();

    string gameName = "";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _networkClient = gameObject.AddComponent<NetworkClient>();
        // _networkClient.HostAddress = url;
        // _networkClient.GameName = gameName;
        SubscribeToEvents();
    }

    public bool IsOnline()
    {
        if (_networkClient._client == null)
        {
            // _networkClient.CreateGame(gameName, url, APIController.instance.authentication.environment);
            return false;
        }
        else
        {
            return _networkClient.IsConnected();
        }
    }

    public async UniTask<bool> TryPing()
    {
        bool success = await _networkClient.TryPing();
        if (success)
            _ = _networkClient.JoinOrCreateGame(gameName, url, APIController.instance.authentication.environment);
        return success;
    }

    void SubscribeToEvents()
    {
        _networkClient.OnConnected += HandleConnected;
        _networkClient.OnDisconnected += HandleDisconnected;
        _networkClient.OnRawMessageReceived += HandleMessageReceived;
        _networkClient.OnErrorReceived += HandleErrorReceived;
    }

    private void HandleConnected()
    {
        APIController.instance.isOnline = true;
        APIController.instance.OnInternetStatusChange?.Invoke(NetworkStatus.Active);
    }

    private void HandleDisconnected()
    {
        APIController.instance.isOnline = false;
        APIController.instance.OnInternetStatusChange?.Invoke(NetworkStatus.NetworkIssue);
        DebugHelper.LogError("WebSocket disconnected.");
    }

    private void HandleErrorReceived(string message)
    {
        try
        {
            JObject jsonObject = JObject.Parse(message);
            string requestID = jsonObject["requestID"]?.ToString();

            WSS_Event wSS_Event = wss_Events.FirstOrDefault(x => x.RequestID == requestID);
            DebugHelper.LogError($"Response Time: {WSClient.GetResponseTime(requestID)} ms,RequestType {wSS_Event?.RequestType}, Message {message}");

            if (string.IsNullOrEmpty(wSS_Event.RequestID))
            {
                return;
            }
            string payload = jsonObject["payload"]?.ToString();
            DebugHelper.Log("Payload" + payload);
            if (string.IsNullOrEmpty(payload))
            {
                DebugHelper.Log(jsonObject["message"].ToString());
                if (jsonObject["message"].ToString() != "timeout")
                {
                    // if(ActiveTasks.ContainsKey(wSS_Event.TaskID))
                    //     ActiveTasks[wSS_Event.TaskID] = false;
                    wSS_Event.InitialtedCallBack?.Invoke(message);
                }
                else
                {
                    if (ActiveTasks.ContainsKey(wSS_Event.TaskID))
                        ActiveTasks.Remove(wSS_Event.TaskID);
                }
            }
            else
            {
                wSS_Event.ErrorCallBack?.Invoke(jsonObject["payload"]["Message"]?.ToString());
                if (ActiveTasks.ContainsKey(wSS_Event.TaskID))
                    ActiveTasks.Remove(wSS_Event.TaskID);
                wss_Events.RemoveAll(x => x.RequestID.Equals(requestID));

            }
        }
        catch { }
    }

    private void HandleMessageReceived(string message)
    {
        try
        {
            JObject jsonObject = JObject.Parse(message);
            string requestID = jsonObject["requestID"]?.ToString();
            DebugHelper.Log($"Response Time: {WSClient.GetResponseTime(requestID)} ms, Message {message}");
            WSS_Event wSS_Event = wss_Events.FirstOrDefault(x => x.RequestID == requestID);

            if (string.IsNullOrEmpty(wSS_Event.RequestID))
            {
                return;
            }

            string payload = jsonObject["payload"]?.ToString();
            DebugHelper.Log("Payload" + payload);
            if (string.IsNullOrEmpty(payload))
            {
                wSS_Event.InitialtedCallBack?.Invoke(message);
            }
            else
            {
                JObject payloadmessage = JObject.Parse(jsonObject["payload"].ToString());
                //DebugHelper.Log("Payload Message11" + jsonObject);
                JObject jObject = JObject.Parse(payloadmessage["Message"]?.ToString());
                //DebugHelper.Log("Payload Message" + payloadmessage["Message"]);
                if ((int)jObject["code"] == 200 || (int)jObject["code"] == 224)
                {
                    wSS_Event.SuccessCallBack?.Invoke(payloadmessage["Message"]?.ToString());
                }
                else
                {
                    // DebugHelper.Log("Payload Message22" + wSS_Event.RequestID + "==="+ wSS_Event.RequestType);
                    wSS_Event.ErrorCallBack?.Invoke(jObject?.ToString());
                }
                wss_Events.RemoveAll(x => x.RequestID.Equals(requestID));
            }


        }
        catch { }
    }


    public void ConnectWebSocket(string _url, string _gameName)
    {
        gameName = _gameName;

        if (!UseLocalUrl)
            url = _url;
        _networkClient.Initialize(url, gameName);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public bool GetTaskStatus(string TaskID)
    {
        if (ActiveTasks.ContainsKey(TaskID))
            return ActiveTasks[TaskID];
        else
            return false;
    }


    public void RemoveRequestEvent(string requestID)
    {
        if (ActiveTasks.ContainsKey(requestID))
            ActiveTasks.Remove(requestID);
        wss_Events.RemoveAll(x => x.RequestID.Equals(requestID));
    }
    private readonly Dictionary<string, CancellationTokenSource> _sendTokens = new();
    public string SendRequest(
    string messageType,
    string requestType,
    string body,
    Action<string> initialAction,
    Action<string> successAction,
    Action<string> errorAction)
    {
        WSMessage message = new WSMessage(messageType, body);

        string reqID = message.RequestID;
        Debug.Log("Checking ID Already exists " + reqID);
        while (wss_Events.Exists(x => x.RequestID == reqID))
        {
            Debug.Log("Request ID Already exists " + reqID);
            reqID = (long.Parse(reqID) + 1).ToString() + UnityEngine.Random.Range(0, 100);
        }
        message.RequestID = reqID;

        var wssevent = new WSS_Event()
        {
            RequestType = requestType,
            InitialtedCallBack = initialAction,
            SuccessCallBack = successAction,
            ErrorCallBack = errorAction,
            RequestID = message.RequestID,
            TaskID = message.RequestID
        };

        wss_Events.Add(wssevent);
        ActiveTasks.Add(wssevent.TaskID, true);
        message.Body = body;

        // Cancel old token if any (retry case)
        if (_sendTokens.TryGetValue(requestType, out var existingCts))
        {
            existingCts.Cancel();
            _sendTokens.Remove(requestType);
        }

        // Create new CTS for this message
        var cts = new CancellationTokenSource();
        _sendTokens[requestType] = cts;

        Debug.Log($"[{requestType}] Sending message of type {messageType}");

        _networkClient.SendClientMsg(message, requestType, cts.Token);

        return message.RequestID;
    }

}

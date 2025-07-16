using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;
using NativeWebSocket;
using UnityEditor;
using System.Numerics;


namespace Aws_Gateway
{
    public class AWS_SocketController : MonoBehaviour
    {
        private WSClient _wsClient;
        public string url = "wss://q0j2tch76b.execute-api.ap-south-1.amazonaws.com/live/";

        // public static AWS_SocketController instance;
        public List<WSS_Event> wss_Events = new List<WSS_Event>();


        Dictionary<string, bool> ActiveTasks = new Dictionary<string, bool>();
        //public bool isOnline = false;

        private void Start()
        {
            _wsClient = gameObject.AddComponent<WSClient>();
            SubscribeToEvents();
            // ConnectToWebSocket();
        }

        public bool GetTaskStatus(string TaskID)
        {
            if (ActiveTasks.ContainsKey(TaskID))
                return ActiveTasks[TaskID];
            else
                return false;
        }


        public void ConnectWebSocket(string _url)
        {
            url = _url;
            //url += environment;
            ConnectToWebSocket();
        }

        private void SubscribeToEvents()
        {
            _wsClient.OnConnected += HandleConnected;
            _wsClient.OnDisconnected += HandleDisconnected;
            _wsClient.OnMessageReceived += HandleMessageReceived;
            _wsClient.OnErrorReceived += HandleErrorReceived;
        }
        #region Websocket 
        public bool IsOnline()
        {
            DebugHelper.Log($"Check IsOnline => {_wsClient._webSocket == null}");
            if (_wsClient._webSocket == null)
            {
                _ = _wsClient.Connect(url);
                return false;
            }
            else
            {
                DebugHelper.Log($"Check IsOnline => {_wsClient._webSocket.State}");
                return _wsClient._webSocket?.State == WebSocketState.Open;
            }
        }
        #region Connection and Disconnection
        private void ConnectToWebSocket()
        {
            _wsClient.isPingLogRequired = false;
            _ = _wsClient.Connect(url);
        }

        private void HandleConnected()
        {
            //isOnline = true;
            APIController.instance.isOnline = true;
            APIController.instance.OnInternetStatusChange?.Invoke(NetworkStatus.Active);

        }


        private void HandleDisconnected()
        {
            //isOnline = false;
            APIController.instance.isOnline = false;
            APIController.instance.OnInternetStatusChange?.Invoke(NetworkStatus.NetworkIssue);
            DebugHelper.LogError("WebSocket disconnected.");
        }
        #endregion



        #region Request and Response handlers
        public string SendRequest(string body, Action<bool, string> action)
        {
            // while (!IsOnline())
            // {
            //     await UniTask.Delay(100);
            //     DebugHelper.Log("waiting for server connect  - Create AND Join");
            // }

            DebugHelper.Log("Send Request" + body);
            WSMessage message = new WSMessage("lambda", body);
            WSS_Event wssevent = new WSS_Event();
            wssevent.CallBack = action;
            wssevent.RequestID = message.RequestID;
            wss_Events.Add(wssevent);
            _ = _wsClient.Send(message, "");
            return message.RequestID;
        }

        public string SendRequest(string messaheType, string requestType, string body, Action<string> initialaction, Action<string> successaction, Action<string> errorAction)
        {
            WSMessage message = new WSMessage(messaheType, body);

            string reqID = message.RequestID;
            DebugHelper.Log("Checking ID Already exists" + reqID);
            while (wss_Events.Exists(x => x.RequestID == reqID))
            {
                DebugHelper.Log("Request ID Already exists" + reqID);
                reqID = (long.Parse(reqID) + 1).ToString() + UnityEngine.Random.Range(0, 100);
            }
            message.RequestID = reqID;

            WSS_Event wssevent = new WSS_Event()
            {
                RequestType = requestType,
                InitialtedCallBack = initialaction,
                SuccessCallBack = successaction,
                ErrorCallBack = errorAction,
                RequestID = message.RequestID,
                TaskID = message.RequestID
            };
            wss_Events.Add(wssevent);
            ActiveTasks.Add(wssevent.TaskID, true);
            message.Body = body;
            DebugHelper.Log(message.Body + "??");
            // reqID = message.RequestID;

            _ = _wsClient.Send(message, wssevent.TaskID);
            return message.RequestID;

        }

        public void GetRandomCard(string environment, string betAmount, string gameName)
        {

            // string payload = JsonConvert.SerializeObject(bodyDict);
            // WSS_Event wssevent = new WSS_Event()
            // {
            //     RequestType = requestType,
            //     InitialtedCallBack = initialaction,
            //     SuccessCallBack = successaction,
            //     ErrorCallBack = errorAction,
            //     RequestID = message.RequestID
            // };
            // wss_Events.Add(wssevent);
            // _wsClient.FetchGameResponse(gameName, "gameprediction", payload);
        }

        public void RemoveRequestEvent(string requestID)
        {
            if (ActiveTasks.ContainsKey(requestID))
                ActiveTasks.Remove(requestID);
            wss_Events.RemoveAll(x => x.RequestID.Equals(requestID));
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
        #endregion
        #endregion
    }


}
[System.Serializable]
public class WSS_Event
{
    public string RequestType;
    public string RequestID;
    public Action<bool, string> CallBack;
    public Action<string> InitialtedCallBack;
    public Action<string> ErrorCallBack;
    public Action<string> SuccessCallBack;

    public string TaskID;
}
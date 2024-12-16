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

public class AWS_SocketController : MonoBehaviour
{
    private WSClient _wsClient;
    public string url = "wss://q0j2tch76b.execute-api.ap-south-1.amazonaws.com/live/";
    public static AWS_SocketController instance;
    public List<WSS_Event> wss_Events = new List<WSS_Event>();
    //public bool isOnline = false;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        _wsClient = gameObject.AddComponent<WSClient>();
        SubscribeToEvents();
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
        Debug.Log($"Check IsOnline => {_wsClient._webSocket == null}");
        if (_wsClient._webSocket == null)
        {
            _ = _wsClient.Connect(url);
            return false;
        }
        else
        {
            Debug.Log($"Check IsOnline => {_wsClient._webSocket.State}");
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
        Debug.LogError("WebSocket disconnected.");
    }
    #endregion



    #region Request and Response handlers
    public async void SendRequest(string body, Action<bool, string> action)
    {
        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - Create AND Join");
        }

        Debug.Log("Send Request" + body);
        WSMessage message = new WSMessage("lambda", body);
        WSS_Event wssevent = new WSS_Event();
        wssevent.CallBack = action;
        wssevent.RequestID = message.RequestID;
        wss_Events.Add(wssevent);
        await _wsClient.Send(message);
    }


    public async void SendRequest(string requestType, string body, Action<string> initialaction, Action<string> successaction, Action<string> errorAction)
    {
        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - Create AND Join");
        }

        WSMessage message = new WSMessage("lambda", body);

        WSS_Event wssevent = new WSS_Event()
        {
            RequestType = requestType,
            InitialtedCallBack = initialaction,
            SuccessCallBack = successaction,
            ErrorCallBack = errorAction,
            RequestID = message.RequestID
        };

        wss_Events.Add(wssevent);
        Debug.Log("------" + message.Body);
        await _wsClient.Send(message);
    }


    private void HandleErrorReceived(string message)
    {
        try
        {
            JObject jsonObject = JObject.Parse(message);
            string requestID = jsonObject["requestID"]?.ToString();

            WSS_Event wSS_Event = wss_Events.FirstOrDefault(x => x.RequestID == requestID);
            Debug.LogError($"Response Time: {WSClient.GetResponseTime(requestID)} ms,RequestType {wSS_Event?.RequestType}, Message {message}");

            if (string.IsNullOrEmpty(wSS_Event.RequestID))
            {
                return;
            }
            string payload = jsonObject["payload"]?.ToString();
            Debug.Log("Payload" + payload);
            if (string.IsNullOrEmpty(payload))
            {
                wSS_Event.InitialtedCallBack?.Invoke(message);
            }
            else
            {
                wSS_Event.ErrorCallBack?.Invoke(jsonObject["payload"]["Message"]?.ToString());
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
            Debug.Log($"Response Time: {WSClient.GetResponseTime(requestID)} ms, Message {message}");
            WSS_Event wSS_Event = wss_Events.FirstOrDefault(x => x.RequestID == requestID);

            if (string.IsNullOrEmpty(wSS_Event.RequestID))
            {
                return;
            }

            string payload = jsonObject["payload"]?.ToString();
            Debug.Log("Payload" + payload);
            if (string.IsNullOrEmpty(payload))
            {
                wSS_Event.InitialtedCallBack?.Invoke(message);
            }
            else
            {
                JObject payloadmessage = JObject.Parse(jsonObject["payload"].ToString());
                Debug.Log("Payload Message11" + jsonObject);
                JObject jObject = JObject.Parse(payloadmessage["Message"]?.ToString());
                //Debug.Log("Payload Message" + payloadmessage["Message"]);
                if ((int)jObject["code"] == 200)
                {
                    wSS_Event.SuccessCallBack?.Invoke(payloadmessage["Message"]?.ToString());
                }
                else
                {
                    wSS_Event.ErrorCallBack?.Invoke(jObject?.ToString());
                }
                wss_Events.RemoveAll(x => x.RequestID.Equals(requestID));
            }


        }
        catch { }
    }
    #endregion
    #endregion

    #region  WSS_Events

    public async void WSS_Authentication(Action<string> initiatedAction, Action<string> successAction, Action<string> errorAction)
    {
        /*  Sample Request Data ********************************
        {
          "request_type": "auth",
          "user_token": "fbe8cea2-f1f3-4245-b5ef-371732801757",
          "platform": "mobile",
          "currency": "INR",
          "game_name": "hilo",
          "operator": "rumblebets"
          }
        */

        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - Authentication");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>() {
            {"request_type","auth"},
            {"user_token",APIController.instance.authentication.token},
            {"platform",APIController.instance.authentication.platform},
            {"currency",APIController.instance.authentication.currency_type},
            {"game_name",APIController.instance.authentication.gamename},
            {"operator",APIController.instance.authentication.operatorname}
        };
        SendRequest("Authentication", JsonConvert.SerializeObject(payload), initiatedAction, successAction, errorAction);
    }

    public async void WSS_CreateAndJoin(string lobbyName, int bet_index, double amount, bool isAbleToCancel, string metaData, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {

        /*  Sample Request Data ********************************
        {
            "user_id": "cbb0c034-3491-4b14-8165-e3610384187d",
            "user_token": "21510b9d-2a72-4a31-94a1-601733142068",
            "game_name": "poker",
            "operator": "rumblebets",
            "session_token": "f68f67e8-4343-db45-28f2-febd07b0e9d1",

            "created_by": "40c5a9d0-b3a3-40d7-99e9-7c44b9654237",
            "game_id": "0d1b08db-5a4d-4a73-b050-ee616402912a",
            "request_type": "CreateAndJoinMatch",
            "lobby_name": "Room : 11/20/2024 8:29:46 AM481_11/20/2024 1:59:49 PM",
            "players": ["40c5a9d0-b3a3-40d7-99e9-7c44b9654237"],
            "bet_amount": 10.00,
            "cash_type": 1,
            "index": 0,
            "is_able_to_cancel": false,
            "is_bot": 0,
            "meta_data": {
                "amount": 10,
                "info": "Bet Placed"
            },
            "currency": "INR",
            "provider": "poker_lootrix",
            "action": "initbet",
            "action_id": "poker_initbet",
            "platform": "tablet"
        }
        */

        Debug.Log("Trying to Authenticate");

        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - Create AND Join");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>() {
            { "created_by", APIController.instance.authentication.Id },
            { "user_id", APIController.instance.authentication.Id },
            { "game_name", APIController.instance.authentication.gamename },
            { "game_id", APIController.instance.userDetails.gameId },
            { "operator", APIController.instance.authentication.operatorname },
            { "request_type", "CreateAndJoinMatch" },
            { "lobby_name", lobbyName },
            { "players", new List<string> {  APIController.instance.authentication.Id } },
            { "bet_amount", amount },
            { "cash_type", 1 },
            { "index", bet_index },
            { "is_able_to_cancel", isAbleToCancel },
            { "is_bot", 0 },
            { "meta_data", metaData },
            { "session_token", APIController.instance.authentication.session_token },
            { "user_token", APIController.instance.authentication.token },
            { "platform", APIController.instance.authentication.platform },
            { "currency", APIController.instance.authentication.currency_type },
            { "provider", APIController.instance.authentication.gamename+"_lootrix" },
            { "action", "initbet" },
            { "action_id", APIController.instance.authentication.gamename+"_initbet" },
        };
        SendRequest("CreateAndJoinMatch", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public async void WSS_WinningBet(string betID, int isWin, double amount, double spendAmount, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {
        /*  Sample Request Data ********************************
        {
            "user_id": "cbb0c034-3491-4b14-8165-e3610384187d",
            "user_token": "21510b9d-2a72-4a31-94a1-601733142068",
            "game_name": "poker",
            "operator": "rumblebets",
            "session_token": "f68f67e8-4343-db45-28f2-febd07b0e9d1",
            "id": "6ef10210-60b6-4d08-8715-0cbb0df19e44",
            "game_id": "625b85f7-4c90-4313-afb6-0c093d78d32f",
            "is_bot": 0,
            "win_amount": 10.60,
            "amount_spend": 10.00,
            "comission": 0.00,
            "is_win": 0,
            "request_type": "winningBet",
            "currency": "",
            "provider": "poker_lootrix",
            "action": "winningbet",
            "action_id": "poker_winningbet",
            "platform": "tablet"
        }
        */

        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - WinningBet");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "user_id",  APIController.instance.authentication.Id},
            { "user_token",  APIController.instance.authentication.token},
            { "game_name",  APIController.instance.authentication.gamename},
            { "operator", APIController.instance.authentication.operatorname },
            { "session_token", APIController.instance.authentication.session_token },
            { "id", betID },
            { "game_id", APIController.instance.userDetails.gameId },
            { "is_bot", 0 },
            { "win_amount", amount },
            { "amount_spend", spendAmount },
            { "comission", 0.00 },
            { "is_win", isWin },
            { "request_type", "winningBet" },
            { "currency", APIController.instance.authentication.currency_type },
            { "provider", APIController.instance.authentication.gamename+"_lootrix" },
            { "action", "winningbet" },
            { "action_id", APIController.instance.authentication.gamename+"_winningbet" },
            { "platform", APIController.instance.authentication.platform }
        };
        Debug.Log("Winning Bet Request " + JsonConvert.SerializeObject(payload));
        SendRequest("WinningBet", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public async void WSS_PlayerInfo(Action<string> initalizedAction = null, Action<string> successAction = null, Action<string> errorAction = null)
    {
        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - PlayerInfo");
        }
        Dictionary<string, string> payload = new Dictionary<string, string>(){
            {"request_type", "info"},
            {"user_id", APIController.instance.authentication.Id},
            {"user_token",APIController.instance.authentication.token},
            {"session_token", APIController.instance.authentication.session_token},
            {"currency", APIController.instance.authentication.currency_type},
            {"operator",APIController.instance.authentication.operatorname},
            {"game_name", APIController.instance.authentication.gamename}
        };
        SendRequest("PlayerInfo", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public async void WSS_AddBet(string betID, double amount, string metadata, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {
        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - AddBet");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "Game_Id", APIController.instance.userDetails.gameId },
            { "GameName", APIController.instance.authentication.gamename },
            { "Operator", APIController.instance.authentication.operatorname },
            { "isBot", 0 },
            { "Id", betID },
            { "Bet_amount", amount },
            { "requestType", "addBet" },
            { "MetaData", metadata },
            { "currency", APIController.instance.authentication.currency_type },
            { "provider", APIController.instance.authentication.gamename+"_lootrix" },
            { "action", "addbet" },
            { "action_id", APIController.instance.authentication.gamename+"_addbet" },
            { "session_token", APIController.instance.authentication.session_token},
            { "platform", APIController.instance.authentication.platform },
            { "url", APIController.instance.authentication.operatorDomainUrl+"api/deposit" }
        };
        SendRequest("AddBet", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public async void WSS_GetRandomPredictions(int round_count, int column_count, int prediction_count, string gameName, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {
        /*  Sample Request Data ********************************
            {
                "row_count": 5,
                "column_count": 5,
                "prediction_count": 11,
                "game_name": "tower",
                "request_type": "RandomPrediction"
            }
        */

        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - GetRandomPrediction");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "row_count", round_count },
            { "column_count", column_count },
            { "prediction_count", prediction_count },
            { "game_name", gameName },
            { "request_type", "RandomPrediction" }
        };
        SendRequest("GetRandomprediction", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);

    }


    public async void WSS_AddBet(string betID, double amount, string metadata, Action<bool, string> action)
    {
        while (!IsOnline())
        {
            await UniTask.Delay(100);
            Debug.Log("waiting for server connect  - AddBet");
        }
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "Game_Id", APIController.instance.userDetails.gameId },
            { "GameName", APIController.instance.authentication.gamename },
            { "Operator", APIController.instance.authentication.operatorname },
            { "isBot", 0 },
            { "Id", betID },
            { "Bet_amount", amount },
            { "requestType", "addBet" },
            { "MetaData", metadata },
            { "currency", APIController.instance.authentication.currency_type },
            { "provider", APIController.instance.authentication.gamename+"_lootrix" },
            { "action", "addbet" },
            { "action_id", APIController.instance.authentication.gamename+"_addbet" },
            { "session_token", APIController.instance.authentication.session_token},
            { "platform", APIController.instance.authentication.platform },
            { "url", APIController.instance.authentication.operatorDomainUrl+"api/deposit" }
        };
        SendRequest(JsonConvert.SerializeObject(payload), action);
    }
    #endregion
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
}

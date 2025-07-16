using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Aws_Gateway;

public class BaseSocketController : MonoBehaviour
{

    public static BaseSocketController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public WebSocketProvider websocketProvider = WebSocketProvider.AWS;

    public AWS_SocketController _awsSocketController;

    public Colyseus_SocketController _colyseusSocketController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string WSS_PlayerInfo(Action<string> initalizedAction = null, Action<string> successAction = null, Action<string> errorAction = null)
    {
        Dictionary<string, string> payload = new Dictionary<string, string>(){
            {"request_type", "info"},
            {"user_id", APIController.instance.authentication.Id},
            {"user_token",APIController.instance.authentication.token},
            {"session_token", APIController.instance.authentication.session_token},
            {"currency", APIController.instance.authentication.currency_type},
            {"operator",APIController.instance.authentication.operatorname},
            {"game_name", APIController.instance.authentication.gamename},
            { "environment", APIController.instance.authentication.environment }
        };
        return SendRequest("lambda", "WSS_PlayerInfo", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public string WSS_WinningBet(string betID, int isWin, double amount, double spendAmount, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {
        DebugHelper.Log("WSS_WinningBet " + amount + " sa " + spendAmount);

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
            { "platform", APIController.instance.authentication.platform },
            {"balance",APIController.instance.userDetails.balance},
            { "environment", APIController.instance.authentication.environment }
        };
        DebugHelper.Log("Winning Bet Request " + JsonConvert.SerializeObject(payload));
        return SendRequest("lambda", "WinningBet", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }



    public string WSS_GetRandomPredictions(int round_count, int column_count, int prediction_count, string gameName, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
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

        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { "row_count", round_count },
            { "column_count", column_count },
            { "prediction_count", prediction_count },
            { "game_name", gameName },
            { "request_type", "RandomPrediction" }
        };
        return SendRequest("lambda", "GetRandomprediction", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public string WSS_AddBet(string betID, double amount, string metadata, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
    {
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
            { "url", APIController.instance.authentication.operatorDomainUrl+"api/deposit" },
            { "balance",APIController.instance.userDetails.balance},
            { "environment", APIController.instance.authentication.environment }
        };
        return SendRequest("lambda", "AddBet", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }

    public string WSS_CreateAndJoin(string lobbyName, int bet_index, double amount, bool isAbleToCancel, string metaData, Action<string> initalizedAction, Action<string> successAction, Action<string> errorAction)
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
        //await UniTask.Delay(1);
        DebugHelper.Log("Trying to Authenticate" + " a " + amount + " m " + metaData);


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
             {"balance",APIController.instance.userDetails.balance},
             { "environment", APIController.instance.authentication.environment }
        };
        return SendRequest("lambda", "CreateAndJoinMatch", JsonConvert.SerializeObject(payload), initalizedAction, successAction, errorAction);
    }


    public string WSS_Authentication(Action<string> initiatedAction, Action<string> successAction, Action<string> errorAction)
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
        //await UniTask.Delay(50);


        Dictionary<string, object> payload = new Dictionary<string, object>() {
            {"request_type","auth"},
            {"user_token",APIController.instance.authentication.token},
            {"platform",APIController.instance.authentication.platform},
            {"currency",APIController.instance.authentication.currency_type},
            {"game_name",APIController.instance.authentication.gamename},
            {"operator",APIController.instance.authentication.operatorname},
            { "environment", APIController.instance.authentication.environment }
        };
        return SendRequest("lambda", "Authentication", JsonConvert.SerializeObject(payload), initiatedAction, successAction, errorAction);
    }


    public string FetchGamePrediction(string requestType, string gamename, string body, Action<string> initialaction, Action<string> successaction, Action<string> errorAction)
    {
        Dictionary<string, string> bodyDict = new Dictionary<string, string>
        {
            { "gameId", gamename },
            { "gameAction", "gameprediction" },
            { "body", body },
        };
        return SendRequest("gameservice", requestType, JsonConvert.SerializeObject(bodyDict), initialaction, successaction, errorAction);
    }

    public bool IsOnline()
    {

        if (websocketProvider == WebSocketProvider.AWS)
        {
            return _awsSocketController?.IsOnline() ?? false;
        }
        else
        {
            return _colyseusSocketController?.IsOnline() ?? false;
        }
    }

    public void RemoveRequestEvent(string requestID)
    {
        if (websocketProvider == WebSocketProvider.AWS)
        {
            _awsSocketController?.RemoveRequestEvent(requestID);
        }
        else
        {
            _colyseusSocketController?.RemoveRequestEvent(requestID);
            //Debug.LogError("WebSocketProvider not implemented: " + websocketProvider);
        }
    }

    public void ConnectWebSocket(string url, string gameName)
    {
        if (websocketProvider == WebSocketProvider.AWS)
        {
            _awsSocketController?.ConnectWebSocket(url);
        }
        else
        {
            _colyseusSocketController?.ConnectWebSocket(url, gameName);
            // Debug.LogError("WebSocketProvider not implemented: " + websocketProvider);
        }
    }


    public string SendRequest(string requestName, string requestType, string payload, Action<string> initalizedAction = null, Action<string> successAction = null, Action<string> errorAction = null)
    {

        DebugHelper.Log($"Request Name : {requestName} Request Type : {requestType} Payload : {payload}");

        if (websocketProvider == WebSocketProvider.AWS)
        {
            return _awsSocketController?.SendRequest(requestName, requestType, payload, initalizedAction, successAction, errorAction);
        }
        else
        {

            return _colyseusSocketController?.SendRequest(requestName, requestType, payload, initalizedAction, successAction, errorAction);
        }
    }

    public void SetServerType(WebSocketProvider provider)
    {
        websocketProvider = provider;
        DebugHelper.Log("WebSocket Provider set to: " + websocketProvider);
    }
}


public enum WebSocketProvider
{
    Colyseus,
    AWS
}
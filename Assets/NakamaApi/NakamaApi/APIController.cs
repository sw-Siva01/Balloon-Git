using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Jobs;
using UnityEngine;

public class APIController : MonoBehaviour
{

    public static APIController instance;

    [Header("Response from webGL json")]
    public string DummyData;
    [Header("Need Dummy Data to test live games in editor")]

    private int defaultDelay = 2;

    [Header("==============================================")]

    public Action OnUserDetailsUpdate;
    public Action OnUserBalanceUpdate;
    public Action OnUserDeposit;

    public Action<NetworkStatus> OnInternetStatusChange;

    public Action<bool> OnSwitchingTab;
    public bool isWin = false;
    public bool IsBotInGame = true;
    public GameWinningStatus winningStatus;
    public UserGameData userDetails;
    public List<BetRequest> betRequest = new List<BetRequest>();
    public bool isPlayByDummyData;
    public double maxWinAmount;
    public bool isClickDeopsit = false;
    public string defaultGameName;
    public int defaultBootAmount = 25;
    public List<APIRequestList> apiRequestList;
    public bool isInFocus = true;
    public bool isOnline = true;
    public bool MobileShow;
    public BackendAPI BackendAPIURL = new BackendAPI();
#if UNITY_WEBGL
    #region WebGl Events
    [DllImport("__Internal")]
    public static extern void GetLoginData();
    [DllImport("__Internal")]
    public static extern void DisconnectGame(string message);
    [DllImport("__Internal")]
    public static extern void SetAudio(int sound, int music); // 1 or 0
    [DllImport("__Internal")]
    public static extern void FullScreen();
    [DllImport("__Internal")]
    private static extern void ShowDeposit();
    [DllImport("__Internal")]
    private static extern void UpdateBalance();

    [DllImport("__Internal")]
    public static extern void CloseWindow();

    private Action<BotDetails> GetABotAction;

    #endregion

    private void Awake()
    {
        AudioListener.volume = 0;
        instance = this;
    }

    void Start()
    {
        GetLambdaURL(true);
#if UNITY_WEBGL && !UNITY_EDITOR
        GetLoginData();
#elif UNITY_EDITOR
        // SetUserData("");
        StartAuthentication(DummyData);
#endif
    }
    #region WebGl Response


    public void UpdateBalanceTrigger()
    {
        Debug.Log("UpdateBalanceTrigger ==>  ");
        OnUserDeposit?.Invoke();
        isClickDeopsit = false;
        if (!userDetails.isBlockApiConnection)
            GetUpdatedBalance();
    }

    public void UpdateBalanceResponse(double data)
    {
        Debug.Log("Balance Updated response  :::::::----::: " + data);
        userDetails.balance = (float)data;
        OnUserBalanceUpdate?.Invoke();
#if !UNITY_EDITOR && UNITY_WEBGL
        UpdateBalance();
#endif
        if (isClickDeopsit)
        {
            OnUserDeposit?.Invoke();
        }
    }
    public void ApiCallBackDebugger(string data)
    {
        byte[] bytesToEncode = Convert.FromBase64String(data);
        string base64EncodedString = Encoding.UTF8.GetString(bytesToEncode);
        Debug.Log(base64EncodedString + "inpuT");
        JObject OBJ = JObject.Parse(base64EncodedString);
        string url = OBJ["url"].ToString();
        int code = int.Parse(OBJ["status"].ToString());
        string body = OBJ["body"].ToString();
        string error = OBJ["error"].ToString();
        APICallBack(url, code, body, error);
    }
    public async void CheckAPICallBack(string url, int timeout)
    {
        Debug.Log($"API_ Response :-  URL{url} -- timeout check");
        await UniTask.Delay(timeout * 1000);
        Debug.Log($"API_ Response :-  URL{url} -- timeout..");

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                item.callback(false, "timeout", "timeout");
            }
        }

        apiRequestList.RemoveAll(x => x.url == url);
    }

    public void OnSwitchingTabs(string data)
    {
        Time.timeScale = 1;
        isInFocus = data == "true" ? true : false;
        Debug.Log($"Calleeedddd switching tab {data}   -   {isOnline}   -   {isInFocus}");
        OnSwitchingTab?.Invoke(data.ToLower() == "true");
    }
    #endregion

    public void OnClickDepositBtn()
    {
        isClickDeopsit = true;
        ShowDeposit();
    }
#endif


    public async void GetUpdatedBalance()
    {
        bool playerinfoReceived = false;
        AWS_SocketController.instance.WSS_PlayerInfo(
        (initateRes) =>
        {

        },
        (successAction) =>
        {
            playerinfoReceived = true;
            Debug.Log("authentication response is : " + successAction);
            JObject apiResponse = JObject.Parse(successAction);
            Debug.Log("authentication response is : " + (int)apiResponse["code"]);
            Debug.Log(apiResponse.ToString());
            if ((int)apiResponse["code"] == 200)
            {
                JObject json = JObject.Parse(apiResponse["data"].ToString());
                Debug.Log($"Player Info Json Response 1 => {json.ToString()}");
                authentication.balance = (float)json["balance"];
                userDetails.balance = authentication.balance;
                Debug.Log("/8::----::: " + authentication.balance);
                OnUserBalanceUpdate?.Invoke();
#if !UNITY_EDITOR && UNITY_WEBGL
                UpdateBalance();
#endif
            }
            else
            {
                DisconnectGame((string)apiResponse["message"]);
            }
        }, (falseAction) =>
        {
            Debug.Log("Player Info Failed " + falseAction);
        });

        float time = Time.time;

        while (!playerinfoReceived)
        {
            if (Time.time - time > 7)
            {
                if (AWS_SocketController.instance.IsOnline())
                {
                    GetUpdatedBalance();
                    return;
                }
            }
            await UniTask.Delay(100);
        }
    }


    public AuthenticationData authentication = new AuthenticationData();
    public void StartAuthentication(string data)
    {
        Debug.Log("Response from wegbl for authentication : " + data);
        authentication = JsonUtility.FromJson<AuthenticationData>(data);
        if (data.Length < 30 || (authentication != null && authentication.operatorname == "demo"))
        {
            SetUserData("");
            return;
        }
        bool ignoreAuthdata = true;

        try
        {
            JObject apiResponse = JObject.Parse(data);
            string json = apiResponse["game_data"].ToString();
            Debug.Log(json);
            if (string.IsNullOrEmpty(json))
            {
                ignoreAuthdata = false;
            }
            else
            {
                authentication.entryAmountDetails.SetEntryAmount(json, authentication.currency_type);
            }
        }
        catch (Exception ex)
        {
            ignoreAuthdata = false;
        }



        AWS_SocketController.instance.WSS_Authentication(

           (initaitedres) =>
           {
               Debug.Log("authentication response is : " + initaitedres + "ignore" + ignoreAuthdata);
           },

           (successRes) =>
           {
               Debug.Log("authentication response is : " + successRes);
               JObject apiResponse = JObject.Parse(successRes);
               Debug.Log("authentication response is : " + (int)apiResponse["code"]);
               /*
               {
                "code": 200,
                "message": "Success",
                "data": "{\"user_id\":\"f1e0a9e7-dd42-415a-a572-80c3efb99714\",\"username\":\"THALA\",\"balance\":\"5343.68\",\"currency\":\"INR\"}",
                "output": "{\"session_token\":\"67bd004a-a124-0e3f-0cca-a9cf0e6f2eba\",\"gameid\":\"0d1b08db-5a4d-4a73-b050-ee616402912a\"}"
                }
               */
               Debug.Log("Auth response  => " + apiResponse.ToString());
               if ((int)apiResponse["code"] == 200)
               {
                   JObject json = JObject.Parse(apiResponse["data"].ToString());
                   JObject json1 = JObject.Parse(apiResponse["output"].ToString());
                   authentication.session_token = (string)json1["session_token"];
                   authentication.name = (string)json["username"];
                   authentication.balance = (float)json["balance"];
                   Debug.Log("Auth response 1 => " + authentication.balance);
                   if (!ignoreAuthdata)
                   {
                       authentication.entryAmountDetails = new();
                       authentication.entryAmountDetails.SetEntryAmount(json1["game_data"].ToString(), authentication.currency_type);
                   }
                   //authentication.music = (string)json1["music"].ToString();

                   Debug.Log("authentication  is : " + JsonUtility.ToJson(authentication));
                   userDetails.Id = authentication.Id;
                   userDetails.game_Id = authentication.operatorname + "_" + authentication.gamename;
                   userDetails.isBlockApiConnection = authentication.operatorname == "demo";
                   userDetails.name = authentication.name;
                   userDetails.session_token = authentication.session_token;
                   userDetails.token = authentication.token;
                   userDetails.platform = authentication.platform;
                   userDetails.operatorDomainUrl = authentication.operatorDomainUrl;
                   userDetails.currency_type = authentication.currency_type;
                   userDetails.gameId = (string)json1["gameid"];
                   userDetails.hasBot = true;
                   userDetails.balance = authentication.balance;
                   userDetails.maxWin = 0;
                   userDetails.isWin = true;
                   userDetails.commission = 0;
                   userDetails.bootAmount = 25;
                   IsBotInGame = userDetails.hasBot;
                   userDetails.bootAmount = defaultBootAmount;
                   if (string.IsNullOrWhiteSpace(userDetails.gameId))
                       userDetails.gameId = "ecd5c5ce-e0a1-4732-82a0-099ec7d180be";
                   Debug.Log("Check this once !!!!!!!!!!!!!" + JsonUtility.ToJson(userDetails));

                   // MiniRouletteUIController.instance.SettingsPanel.UpdateToggle(authentication.sound, authentication.music);
                   AudioListener.volume = 1;
#if UNITY_EDITOR
                   if (MobileShow)
                   {
                       userDetails.platform = "mobile";
                   }
                   else
                   {
                       userDetails.platform = "desktop";
                   }
#endif
               }
               else
               {
#if !UNITY_EDITOR
                    DisconnectGame("Illigal Access");
#else
                   Debug.Log("Illigal Access");
#endif
               }
               Debug.Log("On User Balance Updated");

               OnUserBalanceUpdate?.Invoke();
               OnUserDetailsUpdate?.Invoke();
           },


           (errorRes) =>
           {
               Debug.Log("Authentication Failed " + errorRes);
           }
       );
    }

    public void SetUserData(string data)
    {
        Debug.Log("Response from webgl ::::: " + data);
        if (data.Length < 30)
        {
            userDetails = new UserGameData();
            userDetails.balance = 5000;
            userDetails.currency_type = "USD";
            authentication = new();
            authentication.entryAmountDetails.SetDefaultAmount(userDetails.currency_type);
            userDetails.Id = UnityEngine.Random.Range(5000, 500000) + SystemInfo.deviceUniqueIdentifier.ToGuid().ToString();
            userDetails.token = UnityEngine.Random.Range(5000, 500000) + SystemInfo.deviceUniqueIdentifier.ToGuid().ToString();
            userDetails.name = "User_" + UnityEngine.Random.Range(100, 999);
            isPlayByDummyData = true;
            userDetails.hasBot = true;
            userDetails.game_Id = "demo_" + defaultGameName;
            userDetails.isBlockApiConnection = true;
        }
        else
        {
            userDetails = JsonUtility.FromJson<UserGameData>(data);
            isPlayByDummyData = userDetails.isBlockApiConnection;
            isWin = userDetails.isWin;
            maxWinAmount = userDetails.maxWin;
        }
        authentication.entryAmountDetails.SetDefaultAmount(userDetails.currency_type);
        IsBotInGame = userDetails.hasBot;
        userDetails.bootAmount = defaultBootAmount;
        //MiniRouletteUIController.instance.SettingsPanel.UpdateToggle(authentication.sound, authentication.music);
        AudioListener.volume = 1;
        if (string.IsNullOrWhiteSpace(userDetails.gameId))
            userDetails.gameId = "ecd5c5ce-e0a1-4732-82a0-099ec7d180be";
#if UNITY_EDITOR
        if (MobileShow)
        {
            userDetails.platform = "mobile";
        }
        else
        {
            userDetails.platform = "desktop";
        }

#endif
        OnUserBalanceUpdate?.Invoke();
        OnUserDetailsUpdate?.Invoke();
    }

    public async void CheckInternetForButtonClick(Action<bool> action)
    {
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.POST_METHOD_USING_FORMDATA
                 ,
                 "https://6rugffwb323fkm7j7umild4vjm0hfcfm.lambda-url.ap-south-1.on.aws/",
                 new List<KeyValuePojo>(),
                 (bool isSuccess, string error, string body) =>
                 {
                     action.Invoke(isSuccess);
                 }, 2);
    }

    public async void CheckInternetandProcess(Action<bool> action, bool isLooping = false)
    {
        action.Invoke(isOnline);
        return;
    }

    #region API
    public int InitlizeBet(float amount, TransactionMetaData metadata, bool isAbleToCancel = false, Action<bool> action = null, string playerId = "", bool isBot = false, Action<string> betIdAction = null)
    {
        Debug.Log("" + amount);
        if (string.IsNullOrWhiteSpace(playerId) || playerId == userDetails.Id)
        {
            Debug.Log("Dummy Data" + amount);
            userDetails.balance -= amount;
            OnUserBalanceUpdate.Invoke();
        }
        else
        {
            Debug.Log(playerId + " __ " + userDetails.Id + "__ Dummy Data ---1" + amount);
        }
        action?.Invoke(true);
        return 0;

    }

    public void AddBet(int index, string BetId, TransactionMetaData metadata, float amount, Action<bool> action = null, string playerId = "", bool isBot = false)
    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance -= amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }

    public void CancelBet(int index, string metadata, float amount, Action<bool> action = null, string playerId = "", bool isBot = false)

    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance += amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }

    public void WinningsBet(int index, float amount, double spend_amount, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false)
    {


        if (isPlayByDummyData)
        {
            if (playerId == "" || playerId == userDetails.Id)
            {
                Debug.Log("Winning Bet Data **********");
                userDetails.balance += amount;
                OnUserBalanceUpdate.Invoke();
            }
            action?.Invoke(true);
            return;
        }
    }

    public void APICallBack(string url, int code, string body, string error)
    {
        Debug.Log($"API_ Response :-  URL{url} -- Code {code} -- Body {body} -- Error {error}");

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                if (code == 200)
                {
                    item.callback(true, error, body);
                }
                else
                {
                    item.callback(false, error, body);
                }
            }
        }
        apiRequestList.RemoveAll(x => x.url == url);
    }

    public async void ExecuteAPI(ApiRequest api, int timeout = 0)
    {
        WebApiManager.Instance.GetNetWorkCall(api.callType, api.url, api.param, (success, error, body) =>
        {
            Debug.Log($"<color=orange>Success is set to {success}, error is set to {error} and body is set to {body}\nURL is : {api.url}</color>");
            if (success)
            {
                Debug.Log($"<color=orange>API sent to success</color>");
                api.action?.Invoke(success, error, body);
            }
            else
            {
                if (timeout >= 3)
                {
                    api.action?.Invoke(success, error, body);
                    Debug.Log($"<color=orange>API run failed with timeout {timeout}</color>");
                }
                else
                {
                    Debug.Log($"<color=orange>API recalled with timeout set to {timeout}</color>");
                    ExecuteAPI(api, timeout++);
                }
            }
        }, defaultDelay);
    }
    public bool winningBetCalled;

    public async void WinningsBetMultiplayerAPI(int betIndex, string betId, float win_amount_with_comission, float spend_amount, double pot_amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, float commission, string matchToken)
    {
        winningBetCalled = true;
        // Debug.Log($"BetIndex: {betIndex}, playerId: {playerId}, matchToken: {matchToken} , BetId : {betId}");
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        // Debug.Log($"Request data is {JsonUtility.ToJson(request)}");
        while (request.BetId != betId)
        {
            await UniTask.Delay(200);
        }

        float currentBalance = userDetails.balance + win_amount_with_comission;
        Debug.Log("Winning Bet amount" + win_amount_with_comission + "==" + currentBalance + "==" + APIController.instance.userDetails.balance + "==" + spend_amount);
#if CasinoGames
        bool winningBetResponce = false;
        AWS_SocketController.instance.WSS_WinningBet(betId, isWin ? 1 : 0, win_amount_with_comission, spend_amount,
        (initatedres) =>
        {
            winningBetResponce = true;

            Debug.Log(initatedres);
            double userbalance = currentBalance;
            UpdateBalanceResponse(userbalance);
            action?.Invoke(true);
            Debug.Log("WinningsBetMultiplayerAPI Initialized" + initatedres);
        },
        (successRes) =>
        {

            Debug.Log(successRes);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(successRes);
            //  action?.Invoke(response != null && (response.code == 200 || response.code == 224));
            if (response.code == 224)
            {
                GetUpdatedBalance();
                return;
            }
            JObject json = JObject.Parse(response.message);
            double userbalance = (double)json["balance"];
            UpdateBalanceResponse(userbalance);
        },
        (failRes) =>
        {
            Debug.Log("WinningsBetMultiplayerAPI Failed" + failRes);
        });


        float time = Time.time;

        while (!winningBetResponce)
        {
            if (Time.time - time > 7)
            {
                if (AWS_SocketController.instance.IsOnline())
                {
                    Debug.Log("WinningBetResponce  Retry Called");
                    // CreateAndJoinMatch(index, amount, metadata, isAbleToCancel, lobbyName, playerId, isBot, gameName, operatorName, game_ID, isBlockAPI, players, initalizedAction, successAction, errorAction);
                    WinningsBetMultiplayerAPI(betIndex, betId, win_amount_with_comission, spend_amount, pot_amount, metadata, action, playerId, isBot, isWinner, gameName, operatorName, gameId, commission, matchToken);
                    return;
                }
            }
            await UniTask.Delay(100);
        }
#endif
    }

    public async void GetRandomPredictionIndexApi(int rowCount, int columnCount, int predectedCount, Action<string, bool> OnScucces = null, string gamename = "")
    {
#if CasinoGames

        bool randomPredictionResponce = false;

        AWS_SocketController.instance.WSS_GetRandomPredictions(rowCount, columnCount, predectedCount, gamename,
        (init) =>
        {
            randomPredictionResponce = true;

        },
        (success) =>
        {

            ApiResponse response = JsonUtility.FromJson<ApiResponse>(success);
            if (response.code == 200)
            {
                OnScucces?.Invoke(response.message, true);
            }
            else
            {
                OnScucces?.Invoke(success, false);
            }
        },
        (error) =>
        {

        }
        );
        float time = Time.time;
        while (!randomPredictionResponce)
        {
            if (Time.time - time > 7)
            {
                if (AWS_SocketController.instance.IsOnline())
                {
                    Debug.Log("WinningBetResponce  Retry Called");
                    GetRandomPredictionIndexApi(rowCount, columnCount, predectedCount, OnScucces, gamename);

                    return;
                }
            }
            await UniTask.Delay(100);
        }
#endif
    }

    public async void AddBetMultiplayerAPI(int index, string BetId, TransactionMetaData metadata, double amount, Action<bool> action, string playerId, bool isBot, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == index && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != BetId)
        {
            await UniTask.Delay(200);
        }

        AWS_SocketController.instance.WSS_AddBet(BetId, amount, matchToken, (success, res) =>
        {
            if (success)
            {
                Debug.Log("Add Bet Res :: " + res);
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
                action?.Invoke(response != null && response.code == 200);
                JObject json = JObject.Parse(response.message);
                double userbalance = (double)json["balance"];
                UpdateBalanceResponse(userbalance);
            }
            else
            {
                Debug.Log("Add Bet Failed" + res);
            }

        });
        return;
    }
    public CreateMatchResponse matchResponse;
    public async void CreateAndJoinMatch(int index, float amount, TransactionMetaData metadata, bool isAbleToCancel, string lobbyName, string playerId, bool isBot, string gameName, string operatorName, string game_ID, bool isBlockAPI, List<string> players, Action<CreateMatchResponse> initalizedAction, Action<int, CreateMatchResponse> successAction, Action<CreateMatchResponse> errorAction)
    {
        Debug.Log("CreateAndJoinMatch_" + index);
        matchResponse = new CreateMatchResponse();
        if (isBlockAPI)
        {
            matchResponse.status = true;
            matchResponse.MatchToken = DateTime.UtcNow.ToString().ToGuid().ToString();
            successAction.Invoke(index, matchResponse);
            return;
        }

        if (betRequest.Exists(x => x.betId == index))
        {
            Debug.Log("Checking BetRequest already Exists ===> " + index);
            betRequest.RemoveAll(x => x.betId == index);
        }

        BetRequest bet = new BetRequest();
        bet.PlayerId = playerId;
        bet.betId = index;
        betRequest.Add(bet);
        Debug.Log($"BetRequest JSON Temp before Response.....BetIndex_{index}");
        bool createandjoingameResponseReceived = false;
        AWS_SocketController.instance.WSS_CreateAndJoin(lobbyName, index, amount, isAbleToCancel, JsonUtility.ToJson(metadata),

        (initiatedres) =>
        {
            createandjoingameResponseReceived = true;
            //Debug.Log("CreateAndJoinMatch 1 => " + initiatedres);
            JObject obj = JObject.Parse(initiatedres);
            string message = obj["message"]?.ToString();
            Debug.Log(message);
            if (message.Contains("timeout"))
            {
                // ErrorPopUpHandler.instance.ShowError("Request TimeOut");
                matchResponse.status = false;
                matchResponse.Message = initiatedres;
                errorAction?.Invoke(matchResponse);
                Debug.Log("CreateAndJoinMatch Failed " + initiatedres);
            }
            else
            {
                Debug.Log("CreateAndJoinMatch 1 => " + initiatedres);
            }
        },
        (successres) =>
        {
            Debug.Log("CreateAndJoinMatch 1 => " + successres);
            JObject jsonObject = JObject.Parse(successres);
            if ((int)jsonObject["code"] == 200)
            {
                winningBetCalled = false;
                JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                Debug.Log("jsonObject1  => " + double.Parse(jsonObject1["balance"].ToString()));
                //Response for Create and join match
                /*
                "{\"MatchToken\":\"7473f476-9495-446e-ad1c-df923b19bf7b\",
                \"TransactionID\":\"100f10c8-ffe2-4be8-8e44-ec9364f443ca\",
                \"MatchCount\":1,
                \"WinChance\":0,
                \"Message\":\"8a4fcdac-95ea-47c0-b229-1a772cf7a619\",
                \"IsRandom\":0,
                \"balance\":\"5363.68\"}"*/
                matchResponse.status = true;
                matchResponse.MatchToken = jsonObject1["MatchToken"].ToString();
                matchResponse.MatchCount = int.Parse(jsonObject1["MatchCount"].ToString());
                matchResponse.WinChance = double.Parse(jsonObject1["WinChance"].ToString());
                matchResponse.balance = double.Parse(jsonObject1["balance"].ToString());
                matchResponse.Message = jsonObject1["Message"].ToString();
                matchResponse.IsRandom = int.Parse(jsonObject1["IsRandom"].ToString());
                bet.BetId = jsonObject1["Message"].ToString();
                bet.MatchToken = jsonObject1["MatchToken"].ToString();
                UpdateBalanceResponse(matchResponse.balance);
                successAction?.Invoke(index, matchResponse);
                Debug.Log(JsonConvert.SerializeObject(matchResponse) + "CreateAndJoinMatch 2 => " + bet.BetId + " ... " + bet.MatchToken + "....Balance" + matchResponse.balance);
            }
            else
            {
                /*
                             401 - User token is invalid
                             402 - Insufficient fund
                             403 - User token is expired
                             405 - Internal error with no retry
                             409 - Duplicate transaction
                             413 - Invalid Client-Signature
                             500 - Internal error
                             502 - Error in lootrix side
                             408 - timeout
                             */

                matchResponse.Message = "";

                Debug.Log(jsonObject.ToString());

                ErrorPopUpHandler.instance.ShowError((int)jsonObject["code"], jsonObject["message"]?.ToString());
                matchResponse.status = false;
                errorAction?.Invoke(matchResponse);

            }
        },
        (errorres) =>
        {
            Debug.Log("CreateAndJoinMatch Failed " + errorres);
            JObject jobject = JObject.Parse(errorres);
            matchResponse.status = false;
            matchResponse.Message = jobject["message"]?.ToString();
            errorAction?.Invoke(matchResponse);
            ErrorPopUpHandler.instance.ShowError((int)jobject["code"], jobject["message"].ToString());

        });

        // Wait for 5 seconds for response
        float time = Time.time;

        while (!createandjoingameResponseReceived)
        {
            if (Time.time - time > 7)
            {
                if (AWS_SocketController.instance.IsOnline())
                {
                    Debug.Log("NewCreateAndJoinMatch_1  Retry Called");
                    CreateAndJoinMatch(index, amount, metadata, isAbleToCancel, lobbyName, playerId, isBot, gameName, operatorName, game_ID, isBlockAPI, players, initalizedAction, successAction, errorAction);
                    return;
                }
            }
            await UniTask.Delay(100);
        }
        // return index;
    }

    #endregion
    public void UpdateAudioSettings()
    {
#if !UNITY_EDITOR
        SetAudio(APIController.instance.authentication.sound ? 1 : 0, APIController.instance.authentication.music ? 1 : 0);
#endif
        return;
        if (APIController.instance.userDetails.isBlockApiConnection)
            return;
        var param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "requestType", value = "audio" });
        param.Add(new KeyValuePojo { keyId = "session_token", value = APIController.instance.authentication.session_token });
        param.Add(new KeyValuePojo { keyId = "user_id", value = APIController.instance.authentication.Id });
        param.Add(new KeyValuePojo { keyId = "sound", value = (APIController.instance.authentication.sound ? 1 : 0).ToString() });
        param.Add(new KeyValuePojo { keyId = "music", value = (APIController.instance.authentication.music ? 1 : 0).ToString() });
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, BackendAPIURL.LootrixAudioUpdate, param, (success, error, body) =>
        {
            if (success)
            {
                Debug.Log("Audio Settings Has been Updated");
            }
        });
    }
    public void ServerInactiveAPI()
    {
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, BackendAPIURL.LootrixServerInactiveAPI, new List<KeyValuePojo>()
      {
          new KeyValuePojo { keyId = "requestType", value = "ServerInactive" }, new KeyValuePojo { keyId = "Id", value = userDetails.gameId }, new KeyValuePojo { keyId = "Host", value = "Nakama.Helpers.NakamaManager.Instance.connectedHost" }
      }, (bool isSuccess, string error, string body) => { }, 2);
    }

    #region  TODO
    public async void GetLambdaURL(bool isLive)
    {
        return;
        bool success = false;
        while (!success)
        {
            ApiRequest apiRequest = new ApiRequest();///?requestType=GetGameServer&game_name=carrom
            apiRequest.url = "https://qllb52jc5pxturffykekbtewn40osanl.lambda-url.ap-south-1.on.aws/";
            List<KeyValuePojo> param = new List<KeyValuePojo>();
            param.Add(new KeyValuePojo { keyId = "LoginType", value = isLive ? "1" : "0" });
            param.Add(new KeyValuePojo { keyId = "GameName", value = defaultGameName });
            apiRequest.param = param;
            apiRequest.callType = NetworkCallType.GET_METHOD;
            apiRequest.action = (success1, error, body) =>
            {
                success = success1;

                if (success1)
                {
                    ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                    if (response.code == 200)
                    {
                        BackendAPIURL = JsonUtility.FromJson<BackendAPI>(response.message);
                        BackendAPIURL.isGetData = true;
                        // NakamaManager.Instance.connectedHost = BackendAPIURL.LootrixHost;
                        // Debug.Log(NakamaManager.Instance.connectedHost + "DSFSDFSDF");
                    }
                }
            };
            ExecuteAPI(apiRequest, 3);
            await UniTask.Delay(3000);
        }
    }
    public async void CancelBetMultiplayerAPI(int betIndex, string betId, double amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != betId)
        {
            await UniTask.Delay(200);
        }
        CancelBetReq cancelBetreq = new CancelBetReq();
        cancelBetreq.Amount = amount;
        cancelBetreq.GameID = gameId == "" ? userDetails.gameId : gameId;
        cancelBetreq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        cancelBetreq.Index = betIndex;
        cancelBetreq.IsBot = isBot;
        cancelBetreq.Metadata = metadata;
        cancelBetreq.Betid = betId;
        cancelBetreq.MatchToken = matchToken;
        cancelBetreq.OperatorName = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        cancelBetreq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        // Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_CancelBet", cancelBetreq.ToJson(), (res) =>
        // {
        //     Debug.Log(res);
        //     ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
        //     action?.Invoke(response != null && response.code == 200);
        //     JObject json = JObject.Parse(response.message);
        //     double userbalance = (double)json["balance"];
        //     UpdateBalanceResponse(userbalance);

        // });
        return;
    }
    public void GetRNG_API(float amount, string operatorname, string gameid, Action<bool, float, int> canWin, string gamename, float playersetmultiplier)
    {
        WinLoseRNG winlogic = new()
        {
            amount = amount,
            operatorName = operatorname,
            gameID = gameid,
            gameName = gamename,
            playerSetMultiplier = playersetmultiplier
        };

        // Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_GetIsWinOrLose", winlogic.ToJson(), (res) =>
        // {
        //     Debug.Log("Rng Calculation inside GetRNG 2");
        //     JObject jsonObject = JObject.Parse(res);
        //     userDetails.isWin = ((int.Parse(jsonObject["iswin"].ToString()) > 0));
        //     userDetails.maxWin = float.Parse(jsonObject["Multiplier"].ToString());
        //     int gameCount = int.Parse(jsonObject["GameCount"].ToString());
        //     canWin.Invoke(userDetails.isWin, userDetails.maxWin, gameCount);
        // });
    }
    #endregion
}
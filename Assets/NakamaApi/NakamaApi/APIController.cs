using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using static WebApiManager;
using System.Collections;


[Serializable]
public class BetRequest
{
    public string BetId;
    public string PlayerId;
    public string MatchToken;
    public int betId;
}


[Serializable]
public class APIRequestList
{
    public string url;
    public ReqCallback callback;
}

public class APIController : MonoBehaviour
{

    #region REST_API_VARIABLES
    public static APIController instance;
    [Header("==============================================")]

    #endregion
    public Action OnUserDetailsUpdate;
    public Action OnUserBalanceUpdate;
    public Action OnUserDeposit;
    public Action<bool> OnDepositCancelAction;

    public Action<string> OnInternetStatusChange;
    public Action<bool> OnSwitchingTab;
    public bool isWin = false;
    public bool IsBotInGame = true;
    public GameWinningStatus winningStatus;
    public UserGameData userDetails;
    public List<BetDetails> betDetails = new List<BetDetails>();
    public List<BetRequest> betRequest = new List<BetRequest>();
    public bool isPlayByDummyData;
    public double maxWinAmount;
    public bool isClickDeopsit = false;
    public string testJson;
    public string defaultGameName;
    public string defaultGameId;
    public int defaultBootAmount = 25;
    public List<APIRequestList> apiRequestList;
    public Action<double> OnUserDepositTrigger = null;

    public bool isInFocus = true;

    public bool isOnline = true;
#if UNITY_WEBGL
    #region WebGl Events

    [DllImport("__Internal")]
    public static extern void ExternalApiResponse(string data);

    [DllImport("__Internal")]
    public static extern void GetLoginData();
    [DllImport("__Internal")]
    public static extern void DisconnectGame(string message);
    [DllImport("__Internal")]
    public static extern void GetUpdatedBalance();
    [DllImport("__Internal")]
    public static extern void FullScreen();
    [DllImport("__Internal")]
    private static extern void ShowDeposit();

    [DllImport("__Internal")]
    public static extern void CloseWindow();
    private Action<BotDetails> GetABotAction;

    private Action<string, bool> GetPredictionAction;

    [DllImport("__Internal")]
    public static extern void ExecuteExternalUrl(string url, int timout);

    #endregion

    #region WebGl Response


    public bool IsInitBetSucceeded = false;
    public void GetNetworkStatus(string data)
    {
        isOnline = data.ToLower() == "true" ? true : false;

        OnInternetStatusChange?.Invoke(data);

    }

    public async void GetBalanceWithAction(Action<double> action)
    {
        if (APIController.instance.userDetails.isBlockApiConnection)
        {
            action.Invoke(userDetails.balance);
        }
        else
        {
            action.Invoke(userDetails.balance);
            OnUserDepositTrigger = action;
            GetUpdatedBalance();
        }
    }

 
    public void UpdateBalanceResponse(double data)
    {
        if (OnUserDepositTrigger != null)
        {
            OnUserDepositTrigger.Invoke(data);
            OnUserDepositTrigger = null;
        }

        Debug.Log("Balance Updated response  :::::::----::: " + data);
        userDetails.balance = data;
        OnUserBalanceUpdate?.Invoke();
        if (isClickDeopsit)
        {
            OnUserDeposit?.Invoke();
        }
    }
    public void ExecuteExternalAPI(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        string encryptedData = Convert.ToBase64String(bytes);
        Debug.Log("unity :: base64 is :: " + encryptedData);
        encryptedData = Nakama.Helpers.NakamaManager.Encryptbase64String(encryptedData);
        Debug.Log("unity :: encoded base64 is :: " + encryptedData);
        ExternalApiResponse(encryptedData);
    }

    public bool isNeedToPauseWhileSwitchingTab = false;
   

    public void OnSwitchingTabs(string data)
    {
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

 
    public void SendApiRequest(string url, ReqCallback callback)
    {

        byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);

        string base64EncodedString = Convert.ToBase64String(bytesToEncode);
        apiRequestList.Add(new APIRequestList() { url = base64EncodedString, callback = callback });
        //Debug.Log($"Sending Api Request URl :-" + base64EncodedString);
        ExecuteExternalUrl(base64EncodedString, 10);
        CheckAPICallBack(base64EncodedString);

    }

    public async void SetUserData(string data)
    {
        Debug.Log("Response from webgl ::::: " + data);
        if (data.Length < 30)
        {
            userDetails = new UserGameData();
            userDetails.balance = 5000;
            userDetails.currency_type = "USD";
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
        if (userDetails.game_Id == "lootrix_default")
            userDetails.balance = 5000;
        IsBotInGame = userDetails.hasBot;
        userDetails.bootAmount = defaultBootAmount;
        if (string.IsNullOrWhiteSpace(userDetails.gameId))
            userDetails.gameId = defaultGameId;
        Debug.Log(JsonUtility.ToJson(userDetails));
        InitNakamaClient();
    }
    public void StartSession()
    {
        StopCoroutine(nameof(CheckSession));
        StartCoroutine(nameof(CheckSession));
    }

    public void InitNakamaClient()
    {
        Nakama.Helpers.NakamaManager.Instance.AutoLogin(success =>
        {
            if (success)
            {
                OnUserDetailsUpdate?.Invoke();
                OnUserBalanceUpdate?.Invoke();
                StartSession();
            }
            else
            {
                Debug.LogError("Check nakama server");
            }
        });
    }

    public void OnDepositCancel(string data)
    {
        OnDepositCancelAction?.Invoke((data.ToLower() == "true"));
    }

    async void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetLoginData();
#elif UNITY_EDITOR
        SetUserData("");
#endif
    }
    private void Awake()
    {
        instance = this;
    }


    #region API
    int id = 0;


    public int InitlizeBet(double amount, TransactionMetaData metadata, bool isAbleToCancel = false, Action<bool> action = null, string playerId = "", bool isBot = false, Action<string> betIdAction = null)
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

    public void AddBet(int index, string BetId, TransactionMetaData metadata, double amount, Action<bool> action = null, string playerId = "", bool isBot = false)
    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance -= amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }


    public void CancelBet(int index, string metadata, double amount, Action<bool> action = null, string playerId = "", bool isBot = false)

    {
        if (playerId == "" || playerId == userDetails.Id)
        {
            userDetails.balance += amount;
            OnUserBalanceUpdate.Invoke();
        }
        action?.Invoke(true);
        return;
    }
    public void FinilizeBet(int index, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false)
    {
        action?.Invoke(true);
        return;
    }


    [ContextMenu("RandomPrediction")]
    public void GetRandomPrediction()
    {
        GetRandomPredictionIndexApi(9, 5, 1, (data, status) => { Debug.Log(data); }, "tower");
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

    public void APICallBack(string url, int code, string body, string error)
    {
        // Debug.Log($"API_ Response :-  URL{url} -- Code {code} -- Body {body} -- Error {error}");

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                if (code == 200)
                {
                    item.callback(true, error, body);
#if UNITY_WEBGL
                    if (!userDetails.isBlockApiConnection)
                        GetUpdatedBalance();
#endif
                }
                else
                {
                    item.callback(false, error, body);
                }
            }
        }

        apiRequestList.RemoveAll(x => x.url == url);
    }

    public async void CheckAPICallBack(string url)
    {
        // Debug.Log($"API_ Response :-  URL{url} -- Code {code} -- Body {body} -- Error {error}");
        await UniTask.Delay(10000);

        foreach (var item in apiRequestList)
        {
            if (item.url == url)
            {
                item.callback(false, "timeout", "timeout");
            }
        }

        apiRequestList.RemoveAll(x => x.url == url);
    }


    public void GetRandomPredictionIndexApi(int rowCount, int columnCount, int predectedCount, Action<string, bool> OnScucces = null, string gamename = "")
    {
        GetPredictionReq predictionReq = new GetPredictionReq();
        predictionReq.RowCount = rowCount.ToString();
        predictionReq.ColumnCount = columnCount.ToString();
        predictionReq.PredictionCount = predectedCount.ToString();
        predictionReq.GameName = gamename == "" ? userDetails.game_Id.Split("_")[1] : gamename;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_GetRandomPresiction", predictionReq.ToJson(), (res) =>
        {
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
            if (response.code == 200)
            {
                OnScucces?.Invoke(response.message, true);
            }
            else
            {
                OnScucces?.Invoke(res, false);
            }
        });    
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
        }, 15);
    }
    public string LootrixMatchAPIPath = "https://vmiama243zvt2v2icbu4twsknm0opjax.lambda-url.ap-south-1.on.aws/";
    public string RumbleBetsAPIPath = "https://rumblebets.utwebapps.com:7350/v2/rpc/";
    public string LootrixAPIPath = "https://xpxpmhpjldqvjulexwx34z7jca0kdxzf.lambda-url.ap-south-1.on.aws/";
    bool isRunApi = false;



    /// <summary>
    /// /
    /// </summary>
    /// <param name="betId"></param>
    /// <param name="win_amount_with_comission"></param>
    /// <param name="spend_amount"></param>
    /// <param name="pot_amount"></param>
    /// <param name="metadata"></param>
    /// <param name="action"></param>
    /// <param name="playerId"></param>
    /// <param name="isBot"></param>
    /// <param name="isWinner"></param>
    /// <param name="gameName"></param> <param name="Operator"></param> game name must be APIController.instance.userDetails.game_Id. Get that from client side and stored that into server side. (or) manualy give that in serverside. ex : APIController.instance.userDetails.game_Id = rumbblebets_aviator
    /// <param name="gameId"></param> Get that from client side and stored that into server side. APIController.instance.userDetails.gameId


    public async void WinningsBetMultiplayerAPI(int betIndex, string betId, double win_amount_with_comission, double spend_amount, double pot_amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, float commission, string matchToken)
    {
        Debug.Log($"BetIndex: {betIndex}, playerId: {playerId}, matchToken: {matchToken} , BetId : {betId}");
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        Debug.Log($"Request data is {JsonUtility.ToJson(request)}");
        while (request.BetId != betId)
        {
            await UniTask.Delay(200);
        }

        WinningBetReq winningBetreq = new WinningBetReq();
        winningBetreq.Amount = win_amount_with_comission;
        winningBetreq.AmountSpend = spend_amount;
        winningBetreq.Commission = commission;
        winningBetreq.Potamount = pot_amount;
        winningBetreq.IsWin = isWin;
        winningBetreq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        winningBetreq.GameID = gameId == "" ? userDetails.gameId : gameId;
        winningBetreq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        winningBetreq.Index = betIndex;
        winningBetreq.IsBot = isBot;
        winningBetreq.Metadata = metadata;
        winningBetreq.BetId = betId;
        winningBetreq.MatchToken = matchToken;
        winningBetreq.OperatorName = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        winningBetreq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_WinningBet", winningBetreq.ToJson(), (res) =>
        {
            Debug.Log(res);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
            action?.Invoke(response != null && response.code == 200);

        });
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
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_CancelBet", cancelBetreq.ToJson(), (res) =>
        {
            Debug.Log(res);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
            action?.Invoke(response != null && response.code == 200);

        });

    }

    public async void AddBetMultiplayerAPI(int index, string BetId, TransactionMetaData metadata, double amount, Action<bool> action, string playerId, bool isBot, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == index && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != BetId)
        {
            await UniTask.Delay(200);
        }

        AddBetReq addBetreq = new AddBetReq();
        addBetreq.Amount = amount;
        addBetreq.GameID = gameId == "" ? userDetails.gameId : gameId;
        addBetreq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        addBetreq.Index = index;
        addBetreq.IsBot = isBot;
        addBetreq.Metadata = metadata;
        addBetreq.Betid = BetId;
        addBetreq.OperatorName = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        addBetreq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_AddBet", addBetreq.ToJson(), (res) =>
        {
            Debug.Log(res);
            ApiResponse response = JsonUtility.FromJson<ApiResponse>(res);
            action?.Invoke(response != null && response.code == 200);

        });
     

    }

    // <bool,string,CreateMatchResponse>  = <status,matchid,CreateMatchResponse>
    //https://vmiama243zvt2v2icbu4twsknm0opjax.lambda-url.ap-south-1.on.aws/?Created_By=fed1a31f-d93b-4ffc-ab1b-8652e7d0f29c&Game_Name=mines&Operator=rumblebetsdev&requestType=CreateAndJoinMatch&Lobby_Name=rumblebetsdev_mines_3_5&GameName=mines&Players=%5b%22fed1a31f-d93b-4ffc-ab1b-8652e7d0f29c%22%5d&Game_user_Id=fed1a31f-d93b-4ffc-ab1b-8652e7d0f29c&Game_Id=2540ae31-19f4-4133-9ca8-fa5115c50e2b&IsAbleToCancel=false&playerID=fed1a31f-d93b-4ffc-ab1b-8652e7d0f29c&Index=1&Bet_amount=5&cashType=1&MetaData=%7b%22Amount%22%3a5.0%2c%22Info%22%3a%22Bet+placed%22%7d&IsBot=0&DateTime=Date___22%2f03%2f2024+06%3a50%3a35
    public int CreateAndJoinMatch(int index, double amount, TransactionMetaData metadata, bool isAbleToCancel, string lobbyName, string playerId, bool isBot, string gameName, string operatorName, string game_ID, bool isBlockAPI, List<string> players, Action<bool, string, CreateMatchResponse> action)
    {
        CreateMatchResponse matchResponse = new CreateMatchResponse();
        if (isBlockAPI)
        {
            matchResponse.status = true;
            matchResponse.MatchToken = DateTime.UtcNow.ToString().ToGuid().ToString();
            action.Invoke(true, matchResponse.Message, matchResponse);
            return index;
        }
        BetRequest bet = new BetRequest();
        bet.PlayerId = playerId;
        bet.betId = index;
        betRequest.Add(bet);

        CreateAndJoinGameReq createAndJoinGameReq = new CreateAndJoinGameReq();
        createAndJoinGameReq.CreateBy = playerId == "" ? userDetails.Id : playerId;
        createAndJoinGameReq.Amount = amount;
        createAndJoinGameReq.GameId = game_ID == "" ? userDetails.gameId : game_ID;
        createAndJoinGameReq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        createAndJoinGameReq.Index = index;
        createAndJoinGameReq.IsAbleToCancel = isAbleToCancel;
        createAndJoinGameReq.IsBot = isBot;
        createAndJoinGameReq.LobbyName = lobbyName;
        createAndJoinGameReq.Metadata = metadata;
        createAndJoinGameReq.Operator = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        createAndJoinGameReq.Players = new List<string> { playerId == "" ? userDetails.Id : playerId };
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_CreateAndJoin", createAndJoinGameReq.ToJson(), (res) =>
        {
            Debug.Log(res);
            JObject jsonObject = JObject.Parse(res);
            if ((int)(jsonObject["code"]) == 200)
            {
                JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                matchResponse = JsonConvert.DeserializeObject<CreateMatchResponse>(jsonObject["data"].ToString());
                matchResponse.status = true;
                bet.BetId = matchResponse.Message;
                bet.MatchToken = matchResponse.MatchToken;
                action.Invoke(true, matchResponse.Message, matchResponse);
                IsInitBetSucceeded = true;
            }
            else
            {
                matchResponse.status = false;
                matchResponse.Message = res;
                action.Invoke(false, "", matchResponse);

            }

        });
        return index;

    }

    public bool isCheckInternet;

    public async void StopCheckInternetLoop()
    {
        isCheckInternet = false;
    }
    public async void StartCheckInternetLoop()
    {
        isCheckInternet = true;
        while (isCheckInternet)
        {
            bool isrun = false;
            if (Nakama.Helpers.NakamaManager.Instance.socket != null && Nakama.Helpers.NakamaManager.Instance.socket.IsConnected)
            {
                isrun = true;
                ValidateSessionReq validateSession = new ValidateSessionReq();
                validateSession.PlayerId = userDetails.Id;
                validateSession.GameName = userDetails.game_Id.Split("_")[1];
                validateSession.Operator = userDetails.game_Id.Split("_")[0];
                validateSession.Session_token = userDetails.session_token;
                validateSession.Control = "0";
                validateSession.Token = userDetails.token;
                Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_ValidateSession", validateSession.ToJson(), (res) =>
                {
                    try
                    {
                        JObject jsonObject = JObject.Parse(res);
                        if ((int)(jsonObject["code"]) == 200)
                        {
                            Debug.Log("Network Check :::: online true 1 a");
                            isOnline = true;
                        }
                        else
                        {
                            Debug.Log("Network Check :::: online false 1" + res);
                            isOnline = false;
                        }
                    }
                    catch
                    {
                        Debug.Log("Network Check :::: online false 2");
                        isOnline = false;
                    }
                    Debug.Log("Network Check :::: socket connected 2" + isOnline);
                    isrun = false;
                });
            }
            else
            {

                isOnline = false;
                Debug.Log("Network Check :::: socket not connected" + isOnline);
            }
            for (int i = 0; i < 20; i++)
            {
                if (!isrun)
                    break;
                await UniTask.Delay(100);
            }
            await UniTask.Delay(500);

            if (isrun)
            {
                Debug.Log("Network Check :::: online false 3");
                isOnline = false;
            }
            if(!isOnline)
                GetNetworkStatus(isOnline.ToString());
            Debug.Log("Network Check :::: Internet check " + isOnline);
        }
    }
    public async void CheckInternetandProcess(Action<bool> action)
    {
       /* action.Invoke(true);
        return;*/
        bool isrun = false;
        if (Nakama.Helpers.NakamaManager.Instance.socket != null && Nakama.Helpers.NakamaManager.Instance.socket.IsConnected)
        {
            isrun = true;
            ValidateSessionReq validateSession = new ValidateSessionReq();
            validateSession.PlayerId = userDetails.Id;
            validateSession.GameName = userDetails.game_Id.Split("_")[1];
            validateSession.Operator = userDetails.game_Id.Split("_")[0];
            validateSession.Session_token = userDetails.session_token;
            validateSession.Control = "0";
            validateSession.Token = userDetails.token;
            Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_ValidateSession", validateSession.ToJson(), (res) =>
            {
                try
                {
                    JObject jsonObject = JObject.Parse(res);


                    if ((int)(jsonObject["code"]) == 200)
                    {
                        Debug.Log("Network Check :::: online true 1 b");
                        isOnline = true;
                    }
                    else
                    {
                        Debug.Log("Network Check :::: online false 1" + res);
                        isOnline = false;
                    }
                }
                catch
                {
                    Debug.Log("Network Check :::: online false 2" + res);

                    isOnline = false;

                }
                Debug.Log("Network Check ::::  socket connected 3" + isOnline);
                isrun = false;
            });
        }
        else
        {
            isOnline = false;
            Debug.Log("Network Check :::: socket not connected" + isOnline);
        }
        for (int i = 0; i < 20; i++)
        {
            if (!isrun)
                break;
            await UniTask.Delay(100);
        }
        if (isrun)
        {
            Debug.Log("Network Check :::: online false 3");
            isOnline = false;
        }
        action.Invoke(isOnline);
        if (!isOnline)
            GetNetworkStatus(isOnline.ToString());
        Debug.Log("Network Check :::: Internet check " + isOnline);
    }

    public double lastRuntime = 0;
    public DateTime appstarttime;
    public DateTime lastUpdatetime;

    public IEnumerator CheckSession()
    {
        int Runcount = 0;
        while (true)
        {
            lastUpdatetime = DateTime.Now;
            Runcount += 1;
            bool isrun = false;
            if (Nakama.Helpers.NakamaManager.Instance.socket != null && Nakama.Helpers.NakamaManager.Instance.socket.IsConnected)
            {
                isrun = true;
                ValidateSessionReq validateSession = new ValidateSessionReq();
                validateSession.PlayerId = userDetails.Id;
                validateSession.GameName = userDetails.game_Id.Split("_")[1];
                validateSession.Operator = userDetails.game_Id.Split("_")[0];
                validateSession.Session_token = userDetails.session_token;
                validateSession.Control = "0";
                if (Runcount == 4)
                {
                    if (APIController.instance.userDetails.isBlockApiConnection)
                    {
                        Runcount = 1;
                    }
                    else
                    {
                        Runcount = 0;
                        validateSession.Control = "1";
                    }
                }
                validateSession.Token = userDetails.token;
                Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_ValidateSession", validateSession.ToJson(), (res) =>
                {
                    try
                    {
                        JObject jsonObject = JObject.Parse(res);
                        if ((int)(jsonObject["code"]) == 200)
                        {
                            Debug.Log("Network Check :::: online true c");
                            isOnline = true;
                        }
                        else if ((int)(jsonObject["code"]) != 200)
                        {
                            Debug.Log("Network Check :::: online true 2");
                            isOnline = true;
                            DisconnectGame("Session expired. Account active in another device.");
                            Debug.Log("invalide session");
                        }
                        else
                        {
                            Debug.Log("Network Check :::: online false 1" + res);
                            isOnline = false;
                        }
                    }
                    catch
                    {
                        Debug.Log("Network Check :::: online false 2");
                        isOnline = false;
                    }
                    /*if (isOnline)
                        GetNetworkStatus(isOnline.ToString());*/
                    Debug.Log("Network Check :::: socket connected 1 " + isOnline);
                    isrun = false;
                });
            }
            else
            {
                Debug.Log("Network Check :::: socket not connected" + isOnline);
                if (Nakama.Helpers.NakamaManager.Instance.Socket != null && !Nakama.Helpers.NakamaManager.Instance.Socket.IsConnected && !Nakama.Helpers.NakamaManager.Instance.Socket.IsConnecting)
                {
                    yield return new WaitForSeconds(2f);
                    if (!Nakama.Helpers.NakamaManager.Instance.Socket.IsConnected && !Nakama.Helpers.NakamaManager.Instance.Socket.IsConnecting && Nakama.Helpers.NakamaManager.Instance.client != null)
                    {
                        yield return Nakama.Helpers.NakamaManager.Instance.OpenSocket();
                    }
                }
                Debug.Log("Network Check :::: socket not connected" + isOnline);
            }
            yield return new WaitForSeconds(2);
            Debug.Log("Network Check :::: Internet check " + isOnline);
        }
    }


   


    public void WinningsBet(int index, double amount, double spend_amount, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false)
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

    #endregion

    public class WinLoseRNG
    {
        public double amount;
        public string operatorName;
        public string gameID;
        public string gameName;
        public double playerSetMultiplier;

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

        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_GetIsWinOrLose", winlogic.ToJson(), (res) =>
        {
            Debug.Log("Rng Calculation inside GetRNG 2");
            JObject jsonObject = JObject.Parse(res);
            userDetails.isWin = ((int.Parse(jsonObject["iswin"].ToString()) > 0));
            userDetails.maxWin = float.Parse(jsonObject["Multiplier"].ToString());
            int gameCount = int.Parse(jsonObject["GameCount"].ToString());
            canWin.Invoke(userDetails.isWin, userDetails.maxWin, gameCount);
        });
    }
}

[System.Serializable]
public class ApiRequest
{
    public NetworkCallType callType;
    public string url;
    public List<KeyValuePojo> param;
    public Action<bool, string, string> action;
}

public class InitBetDetails
{
    public bool status;
    public GameWinningStatus message;
    public int index;
}

[System.Serializable]
public class MinMaxOffest
{
    public float min;
    public float max;
}

[System.Serializable]
public class GameWinningStatus
{
    public string Id;
    public double Amount;
    public MinMaxOffest WinCutOff;
    public float WinProbablity;
    public string Game_Id;
    public string Operator;
    public DateTime create_at;
}

[System.Serializable]
public class UserGameData
{
    public string Id;
    public string name;
    public string token;
    public string session_token;
    public double balance;
    public string currency_type;
    public string game_Id;
    public string gameId;
    public bool isBlockApiConnection;
    public double bootAmount;
    public bool isWin;
    public bool hasBot;
    public float commission;
    public float maxWin;
    public bool hasSound;
    public bool hasMusic;
    public string operatorDomainUrl;
    public string UserDevice = "mobile";
}

[System.Serializable]
public class TransactionMetaData
{
    public double Amount;
    public string Info;
}
public class CreateAndJoinGameReq
{
    public string CreateBy;
    public string GameName;
    public string LobbyName;
    public string Operator;
    public string GameId;
    public int Index;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsAbleToCancel;
    public bool IsBot;
    public List<string> Players;
}
public class InitBetReq
{
    public int Index;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsAbleToCancel;
    public bool IsBot;
    public string GameName;
    public string OperatorName;
    public string GameID;
    public string MatchToken;
    public string PlayerId;
}
public class AddPlayersReq
{
    public string MatchToken { get; set; }
    public List<string> Players { get; set; }
}
public class AddBetReq
{
    public int Index { get; set; }
    public string Betid { get; set; }
    public double Amount { get; set; }
    public TransactionMetaData Metadata { get; set; }
    public bool IsBot { get; set; }
    public string GameName { get; set; }
    public string OperatorName { get; set; }
    public string GameID { get; set; }
    public string PlayerId { get; set; }
}
public class GetPredictionReq
{
    public string RowCount;
    public string ColumnCount;
    public string PredictionCount;
    public string GameName;

}
public class GetRandomCardReq
{
    public string RiskCount;
    public string CardData;
    public string Currentval;
    public string TotalMultiplier;
    public string Status;
    public string Amount;
    public string Operator;
}
public class CancelBetReq
{
    public int Index;
    public string Betid;
    public double Amount;
    public TransactionMetaData Metadata;
    public bool IsBot;
    public string GameName;
    public string OperatorName;
    public string GameID;
    public string PlayerId;
    public string MatchToken;
}
public class WinningBetReq
{
    public int Index;
    public string BetId;
    public double Amount;
    public double AmountSpend;
    public float Commission;
    public double Potamount;
    public TransactionMetaData Metadata;
    public bool IsBot;
    public bool IsWin;
    public string GameName;
    public string OperatorName;
    public string GameID;
    public string MatchToken;
    public string PlayerId;
}
public class CreateGameReq
{
    public string CreateBy;
    public string GameName;
    public string LobbyName;
    public string Operator;
    public string GameId;
}
public class ValidateSessionReq
{
    public string PlayerId;
    public string Token;
    public string GameName;
    public string Operator;
    public string Session_token;
    public string Control;
}

[System.Serializable]
public class BetDetails
{
    public string betID;
    public int index;
    public BetProcess Status;
    public string IsAbleToCancel;
    public Action<bool> action;
    public Action<string> betIdAction;
}

public enum BetProcess
{
    Processing = 0,
    Success = 1,
    Failed = 2,
    None = 3
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class ApiResponse
{
    public int code;
    public string message;
    public string data;
    public object output;
}

public class NakamaApiResponse
{
    public int Code;
    public string Message;
}

public class BetResponse
{
    public bool status;
    public string message;
    public int index;
}

[System.Serializable]
public class BotDetails
{
    public string userId;
    public string name;
    public double balance;
}

public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}
public class CreateMatchResponse
{
    public bool status;
    public string MatchToken;
    public int MatchCount;
    public double WinChance;
    public string Message;
    public int IsRandom;
}

public class ExternalAPIRequest
{
    public string data;
}

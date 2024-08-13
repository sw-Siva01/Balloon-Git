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
    public int defaultBootAmount = 25;
    public List<APIRequestList> apiRequestList;
    //public List<ApiResponse> apiResponse;
    //    public List<ApiRequest> apis = new List<ApiRequest>();

    public List<string> PlayerIDs = new() { "f0255647-61d5-4807-b700-352ce052c791", "f24c7429-3946-4567-87be-e258571f704f", "72b183bc-fcbb-4ed4-9061-775d3e908731", "72d43cf3-7bbb-4b88-b443-0669b1390d5e", "cb282b1f-d52f-4958-a28c-9c4a31610877", "bd004aed-2912-46ed-b30b-216e861376e4", "4b2921b6-275b-4454-b61a-d120b57933f3" };
    public bool isInFocus = true;

    public bool isOnline = true;
#if UNITY_WEBGL
    #region WebGl Events

    [DllImport("__Internal")]
    public static extern void GetLoginData();
    [DllImport("__Internal")]
    public static extern void DisconnectGame(string message);
    [DllImport("__Internal")]
    public static extern void ExternalApiResponse(string data);
    [DllImport("__Internal")]
    public static extern void GetUpdatedBalance();
    [DllImport("__Internal")]
    public static extern void FullScreen();
    [DllImport("__Internal")]
    private static extern void ShowDeposit();

    [DllImport("__Internal")]
    public static extern void CloseWindow();

    [DllImport("__Internal")]
    public static extern void CheckOnlineStatus();

    private Action<BotDetails> GetABotAction;
    [DllImport("__Internal")]
    private static extern void GetABot();

    [DllImport("__Internal")]
    private static extern void InitPlayerBet(string type, int index, string game_user_Id, string game_Id, string metaData, string isAbleToCancel, double bet_amount, int isBot);

    [DllImport("__Internal")]
    private static extern void AddPlayerBet(string type, int index, string id, string metaData, string game_user_Id, string game_Id, double bet_amount, int isBot);
    [DllImport("__Internal")]
    private static extern void CancelPlayerBet(string type, string id, string metaData, string game_user_Id, string game_Id, double amount, int isBot);
    [DllImport("__Internal")]
    private static extern void FinilizePlayerBet(string type, string id, string metadata, string game_user_Id, string game_Id, int isBot);
    [DllImport("__Internal")]
    private static extern void WinningsPlayerBet(string type, string id, string metadata, string game_user_Id, string game_Id, double win_amount, double spend_amount, int isBot);
    [DllImport("__Internal")]
    private static extern void MultiplayerWinningsPlayerBet(string type, string id, string metadata, string game_user_Id, string game_Id, double win_amount, double spend_amount, double pot_amount, int isBot, int isWinner);
    [DllImport("__Internal")]
    private static extern void GetRandomPrediction(string type, int rowCount, int columnCount, int predectedCount);
    private Action<string, bool> GetPredictionAction;

    [DllImport("__Internal")]
    public static extern void ExecuteExternalUrl(string url, int timout);

    #endregion

    #region WebGl Response
    [ContextMenu("check json")]
    public void CheckJson()
    {
        InitPlayerBetResponse(testJson);
    }
    public void GetABotResponse(string data)
    {
        Debug.Log("get bot response :::::::----::: " + data);

        BotDetails bot = new BotDetails();
        bot = JsonUtility.FromJson<BotDetails>(data);
        GetABotAction?.Invoke(bot);
        GetABotAction = null;
        Debug.Log("get bot response :::::::----::: after response " + data);
    }
    public void UpdateBalanceResponse(double data)
    {
        Debug.Log("Balance Updated response  :::::::----::: " + data);
        userDetails.balance = (float)data;
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
        return;
    }

    public bool isNeedToPauseWhileSwitchingTab = false;
    public void GetNetworkStatus(string data)
    {
        Time.timeScale = 1;
        isOnline = data.ToLower() == "true" ? true : false;
        Debug.Log($"Calleeedddd check internet {data}   -   {isOnline}   -   {isInFocus}");
        if (isNeedToPauseWhileSwitchingTab)
        {
            if (isInFocus && isOnline)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;

            }
        }
        OnInternetStatusChange?.Invoke(data);

    }

    public void OnSwitchingTabs(string data)
    {
        Time.timeScale = 1;
        isInFocus = data == "true" ? true : false;
        Debug.Log($"Calleeedddd switching tab {data}   -   {isOnline}   -   {isInFocus}");

        OnSwitchingTab?.Invoke(data.ToLower() == "true");
    }

    public void InitPlayerBetResponse(string data)
    {
        Debug.Log("init bet response :::::::----::: " + data);
        InitBetDetails response = JsonUtility.FromJson<InitBetDetails>(data);
        BetDetails bet = betDetails.Find(x => x.index == response.index);
        if (response.status)
        {
            winningStatus = response.message;
            Debug.Log("init bet response :::::::----::: " + response.message);
            Debug.Log("init bet response :::::::----::: " + winningStatus.Id);
            bet.betID = winningStatus.Id;
            bet.Status = BetProcess.Success;
            bet.betIdAction?.Invoke(winningStatus.Id);
            bet.action?.Invoke(true);
        }
        else
        {
            bet.action?.Invoke(false);
            betDetails.RemoveAll(x => x.index == response.index);
        }
        bet.action = null;
    }

    public void CancelPlayerBetResponse(string data)
    {
        Debug.Log("cancel bet response :::::::----::: " + data);
        BetResponse response = JsonUtility.FromJson<BetResponse>(data);
        if (betDetails.Exists(x => x.index == response.index))
        {
            BetDetails bet = betDetails.Find(x => x.index == response.index);
            if (response.status)
            {
                bet.Status = response.status ? BetProcess.Success : BetProcess.Failed;
                bet.action?.Invoke(true);
            }
            else
            {
                bet.action?.Invoke(false);
            }
            bet.action = null;
        }
    }

    public void CheckInternet()
    {
#if !UNITY_EDITOR
        CheckOnlineStatus();
#endif
    }

    public void AddPlayerBetResponse(string data)
    {
        Debug.Log("add bet response :::::::----::: " + data);
        BetResponse response = JsonUtility.FromJson<BetResponse>(data);
        if (betDetails.Exists(x => x.index == response.index))
        {
            BetDetails bet = betDetails.Find(x => x.index == response.index);
            if (response.status)
            {
                bet.Status = response.status ? BetProcess.Success : BetProcess.Failed;
                bet.action?.Invoke(true);
            }
            else
            {
                bet.action?.Invoke(false);
            }
            bet.action = null;
        }
    }

    public void FinilizePlayerBetResponse(string data)
    {
        BetResponse response = JsonUtility.FromJson<BetResponse>(data);
        if (betDetails.Exists(x => x.index == response.index))
        {
            BetDetails bet = betDetails.Find(x => x.index == response.index);
            if (response.status)
            {
                bet.Status = response.status ? BetProcess.Success : BetProcess.Failed;
                bet.action?.Invoke(true);
            }
            else
            {
                bet.action?.Invoke(false);
            }
            bet.action = null;
        }
    }

    public void WinningsPlayerBetResponse(string data)
    {
        Debug.Log("winning bet response :::::::----::: " + data);
        BetResponse response = JsonUtility.FromJson<BetResponse>(data);
        if (betDetails.Exists(x => x.index == response.index))
        {
            BetDetails bet = betDetails.Find(x => x.index == response.index);
            if (response.status)
            {
                bet.Status = response.status ? BetProcess.Success : BetProcess.Failed;
                bet.action?.Invoke(true);
            }
            else
            {
                bet.action?.Invoke(false);
            }
            bet.action = null;
        }
    }
    #endregion


    public void OnClickDepositBtn()
    {
        isClickDeopsit = true;
        ShowDeposit();
    }
#endif

    private async void ClearBetResponse(string betID)
    {
        await UniTask.Delay(5000);
        betRequest.RemoveAll(x => x.BetId.Equals(betID));
    }

    public void SendApiRequest(string url, ReqCallback callback)
    {

        byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);

        string base64EncodedString = Convert.ToBase64String(bytesToEncode);
        apiRequestList.Add(new APIRequestList() { url = base64EncodedString, callback = callback });
        //Debug.Log($"Sending Api Request URl :-" + base64EncodedString);
#if UNITY_WEBGL
        ExecuteExternalUrl(base64EncodedString, 10);
#endif

        CheckAPICallBack(base64EncodedString);

    }
    public bool MobileShow;
    public void SetUserData(string data)
    {
        Debug.Log("Response from webgl ::::: " + data);
        if (data.Length < 30)
        {
            userDetails = new UserGameData();
            userDetails.balance = 5000;
            userDetails.currency_type = "USD";
            userDetails.Id = UnityEngine.Random.Range(5000, 500000) + SystemInfo.deviceUniqueIdentifier.ToGuid().ToString();
            userDetails.token = UnityEngine.Random.Range(5000, 500000) + SystemInfo.deviceUniqueIdentifier.ToGuid().ToString();
            //userDetails.name = SystemInfo.deviceName + SystemInfo.deviceModel;
            userDetails.name = "User_" + UnityEngine.Random.Range(100, 999);
            isPlayByDummyData = true;
            userDetails.hasBot = true;
            userDetails.game_Id = "demo_" + defaultGameName;
            userDetails.isBlockApiConnection = true;
            //userDetails.commission = 0.2f;
        }
        else
        {
            userDetails = JsonUtility.FromJson<UserGameData>(data);
            isPlayByDummyData = userDetails.isBlockApiConnection;
            isWin = userDetails.isWin;
            maxWinAmount = userDetails.maxWin;
        }
        IsBotInGame = userDetails.hasBot;
        //if (userDetails.bootAmount == 0)
        userDetails.bootAmount = defaultBootAmount;
        if (string.IsNullOrWhiteSpace(userDetails.gameId))
            userDetails.gameId = "ecd5c5ce-e0a1-4732-82a0-099ec7d180be";
        Debug.Log(JsonUtility.ToJson(userDetails));
#if UNITY_EDITOR

        //userDetails.UserDevice = "mobile";
        //if (MobileShow)
        //{
        userDetails.UserDevice = "mobile";
        //}
        //else
        //{
        //    userDetails.UserDevice = "desktop";
        //}

#endif
#if CasinoGames

        InitNakamaClient();
#else
        OnUserDetailsUpdate?.Invoke();
        OnUserBalanceUpdate?.Invoke();
#endif
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
                            Debug.Log("online true 1");
                            isOnline = true;
                        }
                        else
                        {
                            Debug.Log("online false 1" + res);
                            isOnline = false;
                        }
                    }
                    catch
                    {
                        Debug.Log("online false 2");
                        isOnline = false;
                    }
                    Debug.Log("socket connected" + isOnline);
                    isrun = false;
                });
            }
            else
            {
                isOnline = false;
                Debug.Log("socket not connected" + isOnline);
            }
            for (int i = 0; i < 20; i++)
            {
                if (!isrun)
                    break;
                await UniTask.Delay(100);
            }
            if (isrun)
            {
                Debug.Log("online false 3");
                isOnline = false;
            }
            GetNetworkStatus(isOnline.ToString());
            Debug.Log("Internet check " + isOnline);
        }
    }
    public async void CheckInternetandProcess(Action<bool> action)
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
                        Debug.Log("online true 1");
                        isOnline = true;
                    }
                    else
                    {
                        Debug.Log("online false 1" + res);
                        isOnline = false;
                    }
                }
                catch
                {
                    Debug.Log("online false 2");
                    isOnline = false;
                }
                Debug.Log("socket connected" + isOnline);
                isrun = false;
            });
        }
        else
        {
            isOnline = false;
            Debug.Log("socket not connected" + isOnline);
        }
        for (int i = 0; i < 20; i++)
        {
            if (!isrun)
                break;
            await UniTask.Delay(100);
        }
        if (isrun)
        {
            Debug.Log("online false 3");
            isOnline = false;
        }

        action.Invoke(isOnline);
        GetNetworkStatus(isOnline.ToString());
        Debug.Log("Internet check " + isOnline);
    }
    public async void CheckSession()
    {
        int Runcount = 0;
        while (true)
        {
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
                            Debug.Log("online true 1");
                            isOnline = true;
                        }
                        else if ((int)(jsonObject["code"]) != 200)
                        {
                            Debug.Log("online true 2");
                            isOnline = true;
                            DisconnectGame("Session expired. Account active in another device.");
                            Debug.Log("invalide session");
                        }
                        else
                        {
                            Debug.Log("online false 1" + res);
                            isOnline = false;
                        }
                    }
                    catch
                    {
                        Debug.Log("online false 2");
                        isOnline = false;
                    }
                    Debug.Log("socket connected" + isOnline);
                    isrun = false;
                });
            }
            else
            {
                if (!Nakama.Helpers.NakamaManager.Instance.Socket.IsConnected && !Nakama.Helpers.NakamaManager.Instance.Socket.IsConnecting)
                {
                    await UniTask.Delay(2000);
                    if (!Nakama.Helpers.NakamaManager.Instance.Socket.IsConnected && !Nakama.Helpers.NakamaManager.Instance.Socket.IsConnecting && Nakama.Helpers.NakamaManager.Instance.client != null)
                    {
                        await Nakama.Helpers.NakamaManager.Instance.OpenSocket();
                    }
                }
                Debug.Log("socket not connected" + isOnline);
            }
            int count = 0;
            await UniTask.Delay(2000);
            if (isrun && Runcount != 0)
            {
                Debug.Log("online false 3");
                isOnline = false;
            }
            if (isOnline)
                GetNetworkStatus(isOnline.ToString());
            Debug.Log(/*GameHudManager.Instance.BetButton.interactable + */"Internet check " + isOnline);
        }
    }

#if CasinoGames

    public void InitNakamaClient()
    {
        Nakama.Helpers.NakamaManager.Instance.AutoLogin(success =>
        {
            if (success)
            {
                OnUserDetailsUpdate?.Invoke();
                OnUserBalanceUpdate?.Invoke();
                CheckSession();
            }
            else
            {
                Debug.LogError("Check nakama server");
            }
        });
    }
#endif
    public void GetLoginDataResponseFromWebGL(string data)
    {
    }
    public void OnDepositCancel(string data)
    {
        OnDepositCancelAction?.Invoke((data.ToLower() == "true"));
    }

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetLoginData();
        CheckInternetStatus();
#elif UNITY_EDITOR
        SetUserData("");
        CheckInternetStatus();
#endif
    }
    private void Awake()
    {
        instance = this;
    }
    public PlayerData GeneratePlayerDataForBot(string botAccountData)
    {
        BotData account = JsonUtility.FromJson<BotData>(botAccountData);
        PlayerWalletData walletData = JsonUtility.FromJson<PlayerWalletData>(account.wallet);
        PlayerData player = new();
        player.playerID = account.user.id;
        player.playerName = account.user.display_name;
        player.isBot = true;
        ProfilePicture profile = JsonUtility.FromJson<ProfilePicture>(account.user.avatar_url);
        player.profilePicURL = profile.ProfileUrl;
        player.avatarIndex = (int)profile.ProfileType;
        player.gold = (walletData.CashDepositVal / 100) + (walletData.CashDepositVal / 1000);
        player.silver = (walletData.SilverVal / 100);
        player.money = player.gold;
        player.totalWinnings = walletData.NetWinning;
        return player;
    }
    public void RandomPrediction_Response(string data)
    {
        Debug.Log("get Prediction response :::::::----::: " + data);
#if UNITY_WEBGL
        GetPredictionAction?.Invoke(data, true);
        GetPredictionAction = null;
#endif
    }
    public void GetRandomPredictionIndex(int rowCount, int columnCount, int predectedCount, Action<string, bool> OnScucces = null)
    {
#if UNITY_WEBGL
        GetPredictionAction = OnScucces;
        GetRandomPrediction("RandomPrediction", rowCount, columnCount, predectedCount);
#endif
    }


    #region API
    int id = 0;

    public void GetBot(Action<BotDetails> action)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GetABotAction = action;
        GetABot();
#endif
    }


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
    public void FinilizeBet(int index, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false)
    {
        action?.Invoke(true);
        return;
    }


    public void CreateMatch(string lobbyName, Action<CreateMatchResponse> action, string gamename, string operatorname, string playerId, bool isBlockAPI, string game_ID = "")
    {

        CreateMatchResponse matchResponse = new CreateMatchResponse();



#if CasinoGames
        CreateGameReq createGameReq = new CreateGameReq();
        createGameReq.CreateBy = playerId == "" ? userDetails.Id : playerId;
        createGameReq.GameId = game_ID == "" ? userDetails.gameId : game_ID;
        createGameReq.GameName = gamename == "" ? userDetails.game_Id.Split("_")[1] : gamename;
        createGameReq.LobbyName = lobbyName;
        createGameReq.Operator = operatorname == "" ? userDetails.game_Id.Split("_")[0] : operatorname;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_Create", createGameReq.ToJson(), (res) =>
        {
            Debug.Log(res);
            JObject jsonObject = JObject.Parse(res);
            if ((int)(jsonObject["code"]) == 200)
            {
                JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                matchResponse = JsonUtility.FromJson<CreateMatchResponse>(jsonObject["data"].ToString());
                matchResponse.status = true;
                action.Invoke(matchResponse);
            }
            else
            {
                matchResponse.status = false;
                action.Invoke(matchResponse);

            }

        });
        return;
#endif





        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "Created_By", value = playerId == "" ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "Game_Name", value = gamename == "" ? userDetails.game_Id.Split("_")[1] : gamename });
        param.Add(new KeyValuePojo { keyId = "Operator", value = operatorname == "" ? userDetails.game_Id.Split("_")[0] : operatorname });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "CreateMatch" });
        param.Add(new KeyValuePojo { keyId = "Lobby_Name", value = lobbyName });
        param.Add(new KeyValuePojo { keyId = "GameName", value = gamename == "" ? userDetails.game_Id.Split("_")[1] : gamename });


        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD,
            LootrixMatchAPIPath, param, (success, error, body) =>
            {
                if (success)
                {
                    JObject jsonObject = JObject.Parse(body);
                    if ((int)(jsonObject["code"]) == 200)
                    {
                        JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                        matchResponse = JsonUtility.FromJson<CreateMatchResponse>(jsonObject["data"].ToString());
                        matchResponse.status = true;
                        action.Invoke(matchResponse);

                    }
                    else
                    {
                        matchResponse.status = false;
                        action.Invoke(matchResponse);

                    }

                }
                else
                {
                    matchResponse.status = false;
                    action.Invoke(matchResponse);
                }
            });
    }
    [ContextMenu("RandomPrediction")]
    public void GetRandomPrediction()
    {
        GetRandomPredictionIndexApi(9, 5, 1, (data, status) => { Debug.Log(data); }, "tower");
    }



    public void ApiCallBackDebugger(string data)
    {
        //Debug.Log("API Call Back Debug :- " + data);

        //byte[] bytesToEncode = Encoding.UTF8.GetBytes(url);
        byte[] bytesToEncode = Convert.FromBase64String(data);

        string base64EncodedString = Encoding.UTF8.GetString(bytesToEncode);

        Debug.Log(base64EncodedString + "inpuT");

        JObject OBJ = JObject.Parse(base64EncodedString);
        string url = OBJ["url"].ToString();
        int code = int.Parse(OBJ["status"].ToString());
        string body = OBJ["body"].ToString();
        string error = OBJ["error"].ToString();
        //Debug.Log("===========================");
        //Debug.Log(url);
        //Debug.Log(body);
        //Debug.Log(code);
        //Debug.Log(error);
        //Debug.Log("===========================");
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


#if CasinoGames
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
        return;
#endif


        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "rowCount", value = rowCount.ToString() });
        param.Add(new KeyValuePojo { keyId = "columnCount", value = columnCount.ToString() });
        param.Add(new KeyValuePojo { keyId = "predictionCount", value = predectedCount.ToString() });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "RandomPrediction" });
        param.Add(new KeyValuePojo { keyId = "gameName", value = gamename == "" ? userDetails.game_Id.Split("_")[1] : gamename });

        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, LootrixAPIPath, param, (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);

                ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                if (response.code == 200)
                {
                    OnScucces?.Invoke(response.message, true);

                }
                else
                {
                    OnScucces?.Invoke(body, false);

                }

            }
            else
            {
                OnScucces?.Invoke("error", false);
            }
        });
    }

    public void CheckInternetStatus(int tryCount = 0)
    {
        return;
#if !UNITY_SERVER
        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, "https://google.com/", new List<KeyValuePojo>(), async (success, error, body) =>
        {
            if (success)
            {
                OnInternetStatusChange?.Invoke(success.ToString());
                await UniTask.Delay(2500);
                CheckInternetStatus();
            }
            else
            {
                tryCount += 1;
                if (tryCount > 4)
                {
                    OnInternetStatusChange?.Invoke(success.ToString());
                    await UniTask.Delay(500);
                    CheckInternetStatus(tryCount);
                }
                else
                {
                    CheckInternetStatus(tryCount);
                }

            }
        }, 2);
#endif
    }

    public void AddMatchLog(string matchToken, string action, string metadata, string PlayerId = "")
    {
        if (userDetails.isBlockApiConnection)
        {
            return;
        }
        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "Token", value = matchToken });
        param.Add(new KeyValuePojo { keyId = "Action", value = action });
        param.Add(new KeyValuePojo { keyId = "Metadata", value = metadata });
        param.Add(new KeyValuePojo { keyId = "PlayerId", value = PlayerId == "" ? userDetails.Id : PlayerId });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "AddMatchLog" });

        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, LootrixMatchAPIPath, param, (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);
                if ((int)(jsonObject["code"]) == 200)
                {

                }
                else
                {

                }

            }
        });
    }

    public void AddUnclaimAmount(string matchToken, double amount)
    {
        if (APIController.instance.userDetails.isBlockApiConnection)
        {
            return;
        }
        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "Id", value = matchToken });
        param.Add(new KeyValuePojo { keyId = "unclaim_amount", value = amount.ToString() });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "AddUnclaimAmount" });

        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, LootrixMatchAPIPath, param, (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);
                if ((int)(jsonObject["code"]) == 200)
                {

                }
                else
                {
                }
            }
        });
    }

    public void AddPlayers(string matchToken, List<string> players)
    {



#if CasinoGames
        AddPlayersReq addPlayersReq = new AddPlayersReq();
        addPlayersReq.MatchToken = matchToken;
        addPlayersReq.Players = players;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_AddBet", addPlayersReq.ToJson(), (res) =>
        {
            Debug.Log(res);
            JObject jsonObject = JObject.Parse(res);
            if ((int)(jsonObject["code"]) == 200)
            {
            }
            else
            {
            }

        });
        return;
#endif




        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "Id", value = matchToken });
        param.Add(new KeyValuePojo { keyId = "Players", value = JsonConvert.SerializeObject(players) });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "AddPlayers" });

        WebApiManager.Instance.GetNetWorkCall(NetworkCallType.GET_METHOD, LootrixMatchAPIPath, param, (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);
                if ((int)(jsonObject["code"]) == 200)
                {

                }
                else
                {
                }
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


    public void GetABotAPI(List<string> botId, Action<BotDetails> action)
    {
        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "player", value = JsonConvert.SerializeObject(botId) });
        string url = RumbleBetsAPIPath + "rpc_GetABot?http_key=defaulthttpkey&unwrap=";
        ApiRequest apiRequest = new ApiRequest();
        apiRequest.action = (success, error, body) =>
        {
            if (success)
            {
                NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                BotDetails bot = new BotDetails();
                bot = JsonUtility.FromJson<BotDetails>(body);
                action?.Invoke(bot);
            }

        };
        apiRequest.url = url;
        apiRequest.param = param;
        apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
        ExecuteAPI(apiRequest);
    }


    public void GetrngwinLogic(float amount, string operatorname, string gameid, Action<string,string> canWin)
    {
        WinLoseRNG winlogic = new()
        {
            amount = amount,
            operatorName = operatorname,
            gameID = gameid
        };

        Debug.Log("GetRNGWinLogic ============> " + winlogic.ToJson());
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_GetIsWinOrLose", winlogic.ToJson(), (res) =>
        {
            Debug.Log(" Res : " + res);
            JObject jsonObject = JObject.Parse(res);
            canWin.Invoke(jsonObject["data"].ToString(),jsonObject["message"].ToString());
        });
    }

    public class WinLoseRNG
    {
        public float amount;
        public string operatorName;
        public string gameID;
    }


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

#if CasinoGames
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
            GetUpdatedBalance();

        });
        return;
#endif


        Debug.Log($"<color=orange>WinningsBetMultiplayerAPI called with commission set to {commission}</color>");
        if (commission == 0 || !isWinner)
        {
            List<KeyValuePojo> param = new List<KeyValuePojo>();

            param.Add(new KeyValuePojo { keyId = "playerID", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
            param.Add(new KeyValuePojo { keyId = "amount", value = win_amount_with_comission.ToString() });
            param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
            param.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
            param.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
            param.Add(new KeyValuePojo { keyId = "matchToken", value = matchToken });

            string url = RumbleBetsAPIPath + "RPC_AddAmounttoUser?http_key=defaulthttpkey&unwrap=";
            ApiRequest apiRequest = new ApiRequest();
            apiRequest.action = (success, error, body) =>
            {
                GetUpdatedBalance();
                if (success)
                {

                    NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                    if (nakamaApi.Code == 200)
                    {
                        List<KeyValuePojo> param1 = new List<KeyValuePojo>
                    {
                        new KeyValuePojo { keyId = "Id", value = betId },
                        new KeyValuePojo { keyId = "Game_user_Id", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId },
                        new KeyValuePojo { keyId = "GameName", value = gameName },
                        new KeyValuePojo { keyId = "Operator", value = operatorName },
                        new KeyValuePojo { keyId = "Game_Id", value = gameId },
                        new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" },
                        new KeyValuePojo { keyId = "Win_amount", value = win_amount_with_comission.ToString() },
                        new KeyValuePojo { keyId = "amountSpend", value = spend_amount.ToString() },
                        new KeyValuePojo { keyId = "potamount", value = pot_amount.ToString() },
                        new KeyValuePojo { keyId = "Comission", value = commission.ToString() },
                        new KeyValuePojo { keyId = "isWin", value = isWinner ? "1" : "0" },
                        new KeyValuePojo { keyId = "requestType", value = "winningBet" }
                    };

                        string url1 = LootrixAPIPath;
                        ApiRequest apiRequest1 = new ApiRequest();
                        apiRequest1.action = (success, error, body) =>
                        {
                            if (success)
                            {

                                ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                                action?.Invoke(response != null && response.code == 200);
                                ClearBetResponse(request.BetId);
                            }
                            else
                            {
                                action?.Invoke(false);
                            }
                        };
                        apiRequest1.url = url1;
                        apiRequest1.param = param1;
                        apiRequest1.callType = NetworkCallType.GET_METHOD;
                        ExecuteAPI(apiRequest1);
                    }
                    else
                    {
                        action?.Invoke(false);
                    }
                }
                else
                {
                    action?.Invoke(false);
                }
            };
            apiRequest.url = url;
            apiRequest.param = param;
            apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
            ExecuteAPI(apiRequest);
        }
        else
        {
            List<KeyValuePojo> param = new List<KeyValuePojo>();

            param.Add(new KeyValuePojo { keyId = "playerID", value = playerId });
            param.Add(new KeyValuePojo { keyId = "amount", value = win_amount_with_comission.ToString() });
            param.Add(new KeyValuePojo { keyId = "amountSpend", value = spend_amount.ToString() });
            param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
            param.Add(new KeyValuePojo { keyId = "game_name", value = gameName });
            param.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
            param.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
            param.Add(new KeyValuePojo { keyId = "matchToken", value = playerId });

            string url = RumbleBetsAPIPath + "RPC_AddWinningAmount?http_key=defaulthttpkey&unwrap=";
            int timeout = 0;
            ApiRequest apiRequest = new ApiRequest();
            apiRequest.action = (success, error, body) =>
            {
                if (success)
                {
                    NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                    if (nakamaApi.Code == 200)
                    {

                        List<KeyValuePojo> param1 = new List<KeyValuePojo>
                    {
                        new KeyValuePojo { keyId = "Id", value = betId },
                        new KeyValuePojo { keyId = "GameName", value = gameName },
                        new KeyValuePojo { keyId = "Operator", value = operatorName },
                        new KeyValuePojo { keyId = "Game_Id", value = gameId },
                        new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" },
                        new KeyValuePojo { keyId = "Win_amount", value = win_amount_with_comission.ToString() },
                        new KeyValuePojo { keyId = "amountSpend", value = spend_amount.ToString() },
                        new KeyValuePojo { keyId = "potamount", value = pot_amount.ToString() },
                        new KeyValuePojo { keyId = "Comission", value = commission.ToString() },
                        new KeyValuePojo { keyId = "isWin", value = isWinner ? "1" : "0" },
                        new KeyValuePojo { keyId = "requestType", value = "winningBet" }
                    };

                        string url1 = LootrixAPIPath;
                        ApiRequest apiRequest1 = new ApiRequest();
                        apiRequest1.action = (success, error, body) =>
                        {
                            if (success)
                            {
                                ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                                action?.Invoke(response != null && response.code == 200);
                                ClearBetResponse(request.BetId);
                            }
                            else
                            {
                                action?.Invoke(false);
                            }
                        };
                        apiRequest1.url = url1;
                        apiRequest1.param = param1;
                        apiRequest1.callType = NetworkCallType.GET_METHOD;
                        ExecuteAPI(apiRequest1);
                    }
                    else
                    {
                        action?.Invoke(false);
                    }
                }
                else
                {
                    action?.Invoke(false);
                }
            };
            apiRequest.url = url;
            apiRequest.param = param;
            apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
            ExecuteAPI(apiRequest);
        }

    }
    public async void CancelBetMultiplayerAPI(int betIndex, string betId, double amount, TransactionMetaData metadata, Action<bool> action, string playerId, bool isBot, bool isWinner, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == betIndex && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != betId)
        {
            await UniTask.Delay(200);
        }

#if CasinoGames
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
        return;
#endif

        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "playerID", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "amount", value = amount.ToString() });
        param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
        param.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
        param.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
        param.Add(new KeyValuePojo { keyId = "matchToken", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
        string url = RumbleBetsAPIPath + "RPC_AddAmounttoUser?http_key=defaulthttpkey&unwrap=";
        ApiRequest apiRequest = new ApiRequest();
        apiRequest.action = (success, error, body) =>
        {
            if (success)
            {
                NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                if (nakamaApi.Code == 200)
                {
                    List<KeyValuePojo> param1 = new List<KeyValuePojo>
                {
                    new KeyValuePojo { keyId = "Id", value = betId },
                    new KeyValuePojo { keyId = "GameName", value = gameName },
                    new KeyValuePojo { keyId = "Operator", value = operatorName },
                    new KeyValuePojo { keyId = "Game_Id", value = gameId },
                    new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" },
                    new KeyValuePojo { keyId = "requestType", value = "cancelBet" }
                };
                    string url1 = LootrixAPIPath;
                    ApiRequest apiRequest1 = new ApiRequest();
                    apiRequest1.action = (success, error, body) =>
                    {
                        if (success)
                        {
                            ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                            action?.Invoke(response != null && response.code == 200);
                        }
                        else
                        {
                            action?.Invoke(false);
                        }
                    };
                    apiRequest1.url = url1;
                    apiRequest1.param = param1;
                    apiRequest1.callType = NetworkCallType.GET_METHOD;
                    ExecuteAPI(apiRequest1);

                }
                else
                {
                    action?.Invoke(false);
                }
            }
            else
            {
                action?.Invoke(false);
            }
        };
        apiRequest.url = url;
        apiRequest.param = param;
        apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
        ExecuteAPI(apiRequest);
    }

    public async void AddBetMultiplayerAPI(int index, string BetId, TransactionMetaData metadata, double amount, Action<bool> action, string playerId, bool isBot, string gameName, string operatorName, string gameId, string matchToken)
    {
        BetRequest request = betRequest.Find(x => x.betId == index && x.PlayerId == playerId && x.MatchToken.Equals(matchToken));
        while (request.BetId != BetId)
        {
            await UniTask.Delay(200);
        }

#if CasinoGames
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
        return;
#endif


        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "playerID", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "amount", value = amount.ToString() });
        param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
        param.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
        param.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
        param.Add(new KeyValuePojo { keyId = "matchToken", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });

        string url = RumbleBetsAPIPath + "RPC_SubractAmountFromUser?http_key=defaulthttpkey&unwrap=";
        ApiRequest apiRequest = new ApiRequest();
        apiRequest.action = (success, error, body) =>
        {
            if (success)
            {
                NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                if (nakamaApi.Code == 200)
                {
                    id += 1;
                    List<KeyValuePojo> param1 = new List<KeyValuePojo>();

                    param1.Add(new KeyValuePojo { keyId = "Game_Id", value = gameId });
                    param1.Add(new KeyValuePojo { keyId = "GameName", value = gameName });
                    param1.Add(new KeyValuePojo { keyId = "Operator", value = operatorName });
                    param1.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
                    param1.Add(new KeyValuePojo { keyId = "Id", value = BetId });
                    param1.Add(new KeyValuePojo { keyId = "Bet_amount", value = amount.ToString() });
                    param1.Add(new KeyValuePojo { keyId = "requestType", value = "addBet" });
                    param1.Add(new KeyValuePojo { keyId = "MetaData", value = JsonUtility.ToJson(metadata) });

                    string url1 = LootrixAPIPath;
                    ApiRequest apiRequest1 = new ApiRequest();
                    apiRequest1.action = (success, error, body) =>
                    {
                        if (success)
                        {
                            ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                            action?.Invoke(response != null && response.code == 200);
                        }
                        else
                        {
                            action?.Invoke(false);
                        }
                    };
                    apiRequest1.url = url1;
                    apiRequest1.param = param1;
                    apiRequest1.callType = NetworkCallType.GET_METHOD;
                    ExecuteAPI(apiRequest1);
                }
                else
                {
                    action?.Invoke(false);
                }
            }
            else
            {
                Debug.Log("AddBetCAlled   Failure" + amount + " *********************************** " + isBot + " *********************************************################################################################################### " + playerId);
                action?.Invoke(false);
            }
        };
        apiRequest.url = url;
        apiRequest.param = param;
        apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
        ExecuteAPI(apiRequest);

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

#if CasinoGames
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

        Debug.Log(createAndJoinGameReq.ToJson() + "TEST__!!!");
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
            }
            else
            {
                matchResponse.status = false;
                matchResponse.Message = res;
                action.Invoke(false, "", matchResponse);

            }

        });
        return index;
#endif


        List<KeyValuePojo> param = new List<KeyValuePojo>();
        param.Add(new KeyValuePojo { keyId = "Created_By", value = playerId == "" ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "Game_Name", value = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName });
        param.Add(new KeyValuePojo { keyId = "Game_Id", value = game_ID == "" ? userDetails.gameId : game_ID });
        param.Add(new KeyValuePojo { keyId = "Operator", value = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName });
        param.Add(new KeyValuePojo { keyId = "requestType", value = "CreateAndJoinMatch" });
        param.Add(new KeyValuePojo { keyId = "Lobby_Name", value = lobbyName });
        param.Add(new KeyValuePojo { keyId = "Players", value = JsonConvert.SerializeObject(players) });
        param.Add(new KeyValuePojo { keyId = "Game_user_Id", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "Bet_amount", value = amount.ToString() });
        param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
        param.Add(new KeyValuePojo { keyId = "Index", value = index.ToString() });
        param.Add(new KeyValuePojo { keyId = "IsAbleToCancel", value = isAbleToCancel.ToString() });
        param.Add(new KeyValuePojo { keyId = "MetaData", value = JsonUtility.ToJson(metadata) });
        param.Add(new KeyValuePojo { keyId = "IsBot", value = isBot ? "1" : "0" });

        ApiRequest api = new ApiRequest();
        api.url = LootrixMatchAPIPath;
        api.param = param;
        api.callType = NetworkCallType.GET_METHOD;
        api.action = (success, error, body) =>
        {
            if (success)
            {
                JObject jsonObject = JObject.Parse(body);

                Debug.Log("Success Case 1 : " + (JObject.Parse(body)));
                if ((int)(jsonObject["code"]) == 200)
                {
                    JObject jsonObject1 = JObject.Parse(jsonObject["data"].ToString());
                    matchResponse = JsonUtility.FromJson<CreateMatchResponse>(jsonObject["data"].ToString());
                    matchResponse.status = true;
                    bet.BetId = matchResponse.Message;
                    bet.MatchToken = matchResponse.MatchToken;

                    List<KeyValuePojo> param1 = new List<KeyValuePojo>();

                    param1.Add(new KeyValuePojo { keyId = "playerID", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
                    param1.Add(new KeyValuePojo { keyId = "amount", value = amount.ToString() });
                    param1.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
                    param1.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
                    param1.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
                    param1.Add(new KeyValuePojo { keyId = "matchToken", value = matchResponse.MatchToken });

                    string url = RumbleBetsAPIPath + "rpc_subractamountfromuser?http_key=defaulthttpkey&unwrap=";
                    ApiRequest apiRequest = new ApiRequest();
                    apiRequest.action = (success, error, body) =>
                    {
                        if (success)
                        {
                            action.Invoke(true, matchResponse.Message, matchResponse);
                            Debug.Log("Success Case 2 : " + body + " ... " + (matchResponse == null));
                        }
                        else
                        {
                            matchResponse.status = false;
                            matchResponse.Message = body;
                            action.Invoke(false, error, matchResponse);

                            Debug.Log("Failure Case 1 : " + matchResponse.Message + " Error : " + error);
                        }
                    };
                    apiRequest.url = url;
                    apiRequest.param = param1;
                    apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
                    ExecuteAPI(apiRequest);




                }
                else
                {
                    matchResponse.status = false;
                    action.Invoke(false, matchResponse.Message, matchResponse);
                    Debug.Log("Failure Case 2 : " + matchResponse.Message + " MatchResponse : " + (matchResponse == null));
                }
            }
            else
            {
                matchResponse.status = false;
                action.Invoke(false, matchResponse.Message, matchResponse);

                Debug.Log("Failure Case 3 : " + matchResponse.Message + " MatchResponse : " + (matchResponse == null) + " Error : " + error + " Body : " + body);
            }

        };
        ExecuteAPI(api);

        return bet.betId;
    }


    public int InitBetMultiplayerAPI(int index, double amount, TransactionMetaData metadata, bool isAbleToCancel, Action<bool> action, string playerId, bool isBot, Action<string> betIdAction, string gameName, string operatorName, string gameID, string matchToken)
    {
        Debug.Log($"<color=orange>Initializing bet for player {playerId}, index is {index}</color>");
        List<KeyValuePojo> param = new List<KeyValuePojo>();
        BetRequest bet = new BetRequest();
        bet.MatchToken = matchToken;
        bet.PlayerId = playerId;
        bet.betId = index;
        betRequest.Add(bet);

        GameWinningStatus _winningStatus;

#if CasinoGames
        InitBetReq initBetReq = new InitBetReq();
        initBetReq.Amount = amount;
        initBetReq.GameID = gameID == "" ? userDetails.gameId : gameID;
        initBetReq.GameName = gameName == "" ? userDetails.game_Id.Split("_")[1] : gameName;
        initBetReq.Index = index;
        initBetReq.IsAbleToCancel = isAbleToCancel;
        initBetReq.IsBot = isBot;
        initBetReq.Metadata = metadata;
        initBetReq.OperatorName = operatorName == "" ? userDetails.game_Id.Split("_")[0] : operatorName;
        initBetReq.MatchToken = matchToken;
        initBetReq.PlayerId = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId;
        Nakama.Helpers.NakamaManager.Instance.SendRPC("rpc_InitBet", initBetReq.ToJson(), (res) =>
        {
            Debug.Log(res);
            JObject jsonObject = JObject.Parse(res);
            if ((int)(jsonObject["code"]) == 200)
            {
                _winningStatus = JsonUtility.FromJson<GameWinningStatus>(jsonObject["data"].ToString());
                bet.BetId = _winningStatus.Id;
                betIdAction.Invoke(_winningStatus.Id);
                GetUpdatedBalance();

            }
            else
            {
                action.Invoke(false);

            }

        });
        return index;
#endif



        param.Add(new KeyValuePojo { keyId = "playerID", value = string.IsNullOrEmpty(playerId) ? userDetails.Id : playerId });
        param.Add(new KeyValuePojo { keyId = "amount", value = amount.ToString() });
        param.Add(new KeyValuePojo { keyId = "cashType", value = 1.ToString() });
        param.Add(new KeyValuePojo { keyId = "metaData", value = JsonUtility.ToJson(metadata) });
        param.Add(new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" });
        param.Add(new KeyValuePojo { keyId = "matchToken", value = matchToken });

        string url = RumbleBetsAPIPath + "rpc_subractamountfromuser?http_key=defaulthttpkey&unwrap=";
        ApiRequest apiRequest = new ApiRequest();
        apiRequest.action = (success, error, body) =>
        {
            if (success)
            {

                NakamaApiResponse nakamaApi = JsonUtility.FromJson<NakamaApiResponse>(body);
                if (nakamaApi.Code == 200)
                {
                    List<KeyValuePojo> param1 = new List<KeyValuePojo>
                {
                    new KeyValuePojo { keyId = "Game_Id", value = gameID},
                    new KeyValuePojo { keyId = "GameName", value = gameName },
                    new KeyValuePojo { keyId = "Operator", value = operatorName },
                    new KeyValuePojo { keyId = "Game_user_Id", value = playerId },
                    new KeyValuePojo { keyId = "Status", value = isAbleToCancel.ToString() },
                    new KeyValuePojo { keyId = "IsAbleToCancel", value = isAbleToCancel.ToString() },
                    new KeyValuePojo { keyId = "isBot", value = isBot ? "1" : "0" },
                    new KeyValuePojo { keyId = "Index", value = index.ToString() },
                    new KeyValuePojo { keyId = "Bet_amount", value = amount.ToString() },
                    new KeyValuePojo { keyId = "requestType", value = "initBet" },
                    new KeyValuePojo { keyId = "MatchToken", value = matchToken },
                    new KeyValuePojo { keyId = "MetaData", value = JsonUtility.ToJson(metadata) }
                };
                    string url1 = LootrixAPIPath;
                    ApiRequest apiRequest1 = new ApiRequest();
                    apiRequest1.action = (success, error, body) =>
                    {
                        if (success)
                        {
                            ApiResponse response = JsonUtility.FromJson<ApiResponse>(body);
                            action?.Invoke(response != null && response.code == 200);
                            if (response.code == 200)
                            {
                                Debug.Log($"<color=aqua>Response message is : {response.data}</color>");
                                _winningStatus = JsonUtility.FromJson<GameWinningStatus>(response.data);
                                bet.BetId = _winningStatus.Id;
                                betIdAction.Invoke(_winningStatus.Id);
                            }
                        }
                        else
                        {
                            action?.Invoke(false);
                        }
                    };
                    apiRequest1.url = url1;
                    apiRequest1.param = param1;
                    apiRequest1.callType = NetworkCallType.GET_METHOD;
                    ExecuteAPI(apiRequest1);


                }
                else
                {
                    action?.Invoke(false);
                }
            }
            else
            {
                action?.Invoke(false);
            }
        };
        apiRequest.url = url;
        apiRequest.param = param;
        apiRequest.callType = NetworkCallType.POST_METHOD_USING_JSONDATA;
        ExecuteAPI(apiRequest);
        return index;
    }

    public void WinningsBetMultiplayer(int betIndex, string betId, float win_amount_with_comission, double spend_amount, double pot_amount, TransactionMetaData metadata, Action<bool> action = null, string playerId = "", bool isBot = false, bool isWinner = true)
    {

        if (isPlayByDummyData)
        {
            if (playerId == "" || playerId == userDetails.Id)
            {
                Debug.Log("Winning Bet Data **********");
                userDetails.balance += win_amount_with_comission;
                OnUserBalanceUpdate.Invoke();
            }
            action?.Invoke(true);
            return;
        }

        if (betDetails.Exists(x => x.betID == betId))
        {
            BetDetails bet = betDetails.Find(x => x.betID == betId);
            bet.Status = BetProcess.Processing;
            bet.action = action;
            //var data = new
            //{
            //    type = "WinningsBet",
            //    Id = bet.betID,
            //    MetaData = metadata,
            //    Game_user_Id = userDetails.Id,
            //    Game_Id = userDetails.game_Id,
            //    Win_amount = amount,
            //    Spend_amount = amount,
            //};

#if UNITY_WEBGL
            Debug.Log("Winning Bet Data");
            MultiplayerWinningsPlayerBet("WinningsBet", betId, JsonUtility.ToJson(metadata), playerId == "" ? userDetails.Id : playerId, userDetails.game_Id, win_amount_with_comission, spend_amount, pot_amount, isBot ? 1 : 0, isWinner ? 1 : 0);
#endif
        }
        else
        {
#if UNITY_WEBGL
            BetDetails bet = new BetDetails();
            id += 1;
            bet.index = id;
            bet.betID = betId;
            bet.Status = BetProcess.Processing;
            betDetails.Add(bet);
            Debug.Log("Winning Bet Data");
            MultiplayerWinningsPlayerBet("WinningsBet", betId, JsonUtility.ToJson(metadata), playerId == "" ? userDetails.Id : playerId, userDetails.game_Id, win_amount_with_comission, spend_amount, pot_amount, isBot ? 1 : 0, isWinner ? 1 : 0);
#endif

        }
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

        if (betDetails.Exists(x => x.index == index))
        {
            BetDetails bet = betDetails.Find(x => x.index == index);
            bet.Status = BetProcess.Processing;
            bet.action = action;
            //var data = new
            //{
            //    type = "WinningsBet",
            //    Id = bet.betID,
            //    MetaData = metadata,
            //    Game_user_Id = userDetails.Id,
            //    Game_Id = userDetails.game_Id,
            //    Win_amount = amount,
            //    Spend_amount = amount,
            //};
#if UNITY_WEBGL
            Debug.Log("Winning Bet Data");
            WinningsPlayerBet("WinningsBet", bet.betID, JsonUtility.ToJson(metadata), playerId == "" ? userDetails.Id : playerId, userDetails.game_Id, amount, spend_amount, isBot ? 1 : 0);
#endif
        }
    }

    public BetProcess CheckBetStatus(int index)
    {
        if (isPlayByDummyData)
            return BetProcess.Success;
        if (betDetails.Exists(x => x.index == index))
        {
            BetDetails bet = betDetails.Find(x => x.index == index);
            return bet.Status;
        }
        return BetProcess.Failed;
    }
    #endregion
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
    public float balance;
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
    public string UserDevice;
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

public class ExternalAPIRequest
{
    public string data;
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
}